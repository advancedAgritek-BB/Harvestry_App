#!/bin/bash
# Harvestry ERP - Blue/Green Deployment
# Track A: Zero-downtime deployment with automatic rollback

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
ENVIRONMENT="staging"
SERVICE=""
VERSION=""
TRAFFIC_SHIFT="5,25,50,100"
SHIFT_INTERVAL=300 # 5 minutes between shifts
HEALTH_CHECK_RETRIES=10
ROLLBACK_ON_ERROR=true
PROMETHEUS_URL="http://prometheus:9090"

# Print usage
usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Perform blue/green deployment with gradual traffic shifting.

OPTIONS:
    -e, --env ENV           Environment (staging, production) [default: staging]
    -s, --service SERVICE   Service name (required)
    -v, --version VERSION   Version tag to deploy (required)
    -t, --traffic SHIFTS    Traffic shift percentages (comma-separated) [default: 5,25,50,100]
    -i, --interval SECS     Seconds between traffic shifts [default: 300]
    -p, --prometheus URL    Prometheus URL [default: http://prometheus:9090]
    -r, --no-rollback       Disable automatic rollback on error
    -h, --help              Show this help message

EXAMPLES:
    # Standard blue/green deployment
    $0 --env staging --service api-gateway --version v1.2.3

    # Fast rollout for low-risk change
    $0 --env staging --service ui --version v1.2.3 --traffic 25,100 --interval 60

    # Production deployment (requires confirmation)
    $0 --env production --service api-gateway --version v1.2.3

EOF
    exit 1
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -e|--env)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -s|--service)
            SERVICE="$2"
            shift 2
            ;;
        -v|--version)
            VERSION="$2"
            shift 2
            ;;
        -t|--traffic)
            TRAFFIC_SHIFT="$2"
            shift 2
            ;;
        -i|--interval)
            SHIFT_INTERVAL="$2"
            shift 2
            ;;
        -p|--prometheus)
            PROMETHEUS_URL="$2"
            shift 2
            ;;
        -r|--no-rollback)
            ROLLBACK_ON_ERROR=false
            shift
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo -e "${RED}Error: Unknown option $1${NC}"
            usage
            ;;
    esac
done

# Validate required parameters
if [ -z "$SERVICE" ] || [ -z "$VERSION" ]; then
    echo -e "${RED}Error: Service and version are required${NC}"
    usage
fi

# Production safety check
if [ "$ENVIRONMENT" = "production" ]; then
    echo -e "${RED}WARNING: You are about to deploy to PRODUCTION!${NC}"
    echo "Service: $SERVICE"
    echo "Version: $VERSION"
    echo "Traffic Shifts: $TRAFFIC_SHIFT"
    echo ""
    read -p "Type 'DEPLOY TO PRODUCTION' to continue: " confirmation
    if [ "$confirmation" != "DEPLOY TO PRODUCTION" ]; then
        echo -e "${GREEN}Deployment cancelled.${NC}"
        exit 1
    fi
fi

# Kubernetes namespace based on environment
K8S_NAMESPACE="harvestry-${ENVIRONMENT}"
DEPLOYMENT_NAME="${SERVICE}-deployment"
GREEN_DEPLOYMENT="${SERVICE}-green"

echo -e "${BLUE}======================================${NC}"
echo -e "${BLUE}Harvestry ERP - Blue/Green Deployment${NC}"
echo -e "${BLUE}======================================${NC}"
echo ""
echo "Environment:    $ENVIRONMENT"
echo "Service:        $SERVICE"
echo "Version:        $VERSION"
echo "Traffic Shifts: $TRAFFIC_SHIFT"
echo ""

# Step 1: Deploy green version
echo -e "${YELLOW}Step 1: Deploying green version...${NC}"

cat <<EOF | kubectl --namespace="$K8S_NAMESPACE" apply -f -
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ${GREEN_DEPLOYMENT}
  namespace: ${K8S_NAMESPACE}
spec:
  replicas: 1
  selector:
    matchLabels:
      app: ${SERVICE}
      version: green
  template:
    metadata:
      labels:
        app: ${SERVICE}
        version: green
    spec:
      containers:
      - name: ${SERVICE}
        image: harvestry/${SERVICE}:${VERSION}
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: ${ENVIRONMENT}
EOF

# Wait for green deployment to be ready
echo -e "${YELLOW}Waiting for green deployment to be ready...${NC}"
kubectl --namespace="$K8S_NAMESPACE" wait --for=condition=available --timeout=300s \
    deployment/"$GREEN_DEPLOYMENT"

# Step 2: Health check green deployment
echo -e "${YELLOW}Step 2: Running health checks on green deployment...${NC}"

GREEN_POD=$(kubectl --namespace="$K8S_NAMESPACE" get pods -l app="$SERVICE",version=green \
    -o jsonpath="{.items[0].metadata.name}")

HEALTH_CHECK_SUCCESS=false
for i in $(seq 1 $HEALTH_CHECK_RETRIES); do
    echo -e "${YELLOW}Health check attempt $i/$HEALTH_CHECK_RETRIES...${NC}"
    
    if kubectl --namespace="$K8S_NAMESPACE" exec "$GREEN_POD" -- \
        wget -q -O- http://localhost:8080/health/ready > /dev/null 2>&1; then
        echo -e "${GREEN}✓ Health check passed${NC}"
        HEALTH_CHECK_SUCCESS=true
        break
    fi
    
    sleep 10
done

if [ "$HEALTH_CHECK_SUCCESS" = false ]; then
    echo -e "${RED}✗ Health checks failed after $HEALTH_CHECK_RETRIES attempts${NC}"
    
    if [ "$ROLLBACK_ON_ERROR" = true ]; then
        echo -e "${YELLOW}Rolling back...${NC}"
        kubectl --namespace="$K8S_NAMESPACE" delete deployment "$GREEN_DEPLOYMENT"
    fi
    
    exit 1
fi

# Step 3: Gradual traffic shift
IFS=',' read -ra SHIFTS <<< "$TRAFFIC_SHIFT"

for shift in "${SHIFTS[@]}"; do
    echo -e "${YELLOW}Step 3.$shift: Shifting ${shift}% traffic to green...${NC}"
    
    # Create/update VirtualService for traffic splitting (Istio)
    cat <<EOF | kubectl apply -f -
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: ${SERVICE}-vs
  namespace: ${K8S_NAMESPACE}
spec:
  hosts:
  - ${SERVICE}
  http:
  - match:
    - uri:
        prefix: /
    route:
    - destination:
        host: ${SERVICE}
        subset: blue
      weight: $((100 - shift))
    - destination:
        host: ${SERVICE}
        subset: green
      weight: ${shift}
EOF

    echo -e "${GREEN}Traffic shifted: ${shift}% green, $((100 - shift))% blue${NC}"
    
    # Monitor for errors
    echo -e "${YELLOW}Monitoring for ${SHIFT_INTERVAL}s...${NC}"
    
    MONITORING_SUCCESS=true
    for i in $(seq 1 $((SHIFT_INTERVAL / 30))); do
        # Check error rate via HTTP query
        QUERY="rate(http_request_errors_total{service=\"${SERVICE}\",environment=\"${ENVIRONMENT}\"}[1m]) / rate(http_requests_total{service=\"${SERVICE}\",environment=\"${ENVIRONMENT}\"}[1m])"

        RESPONSE=$(curl -s "${PROMETHEUS_URL}/api/v1/query" --data-urlencode "query=${QUERY}")
        ERROR_RATE=$(echo "$RESPONSE" | jq -r '.data.result[0].value[1]' 2>/dev/null || echo "0")

        if (( $(echo "$ERROR_RATE > 0.01" | bc -l) )); then
            echo -e "${RED}✗ Error rate exceeded threshold: $ERROR_RATE${NC}"
            MONITORING_SUCCESS=false
            break
        fi

        echo -e "${GREEN}✓ Metrics healthy (error rate: $ERROR_RATE)${NC}"
        sleep 30
    done
    
    if [ "$MONITORING_SUCCESS" = false ]; then
        echo -e "${RED}Rollback triggered by error rate violation${NC}"
        
        if [ "$ROLLBACK_ON_ERROR" = true ]; then
            echo -e "${YELLOW}Rolling back to blue...${NC}"
            
            # Shift all traffic back to blue
            cat <<EOF | kubectl apply -f -
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: ${SERVICE}-vs
  namespace: ${K8S_NAMESPACE}
spec:
  hosts:
  - ${SERVICE}
  http:
  - match:
    - uri:
        prefix: /
    route:
    - destination:
        host: ${SERVICE}
        subset: blue
      weight: 100
EOF
            
            # Delete green deployment
            kubectl --namespace="$K8S_NAMESPACE" delete deployment "$GREEN_DEPLOYMENT"
            
            echo -e "${GREEN}Rollback complete${NC}"
        fi
        
        exit 1
    fi
    
    # Wait before next shift (unless at 100%)
    if [ "$shift" -ne 100 ]; then
        echo -e "${YELLOW}Waiting ${SHIFT_INTERVAL}s before next shift...${NC}"
        sleep "$SHIFT_INTERVAL"
    fi
done

# Step 4: Finalize deployment
echo -e "${YELLOW}Step 4: Finalizing deployment...${NC}"

# Scale down blue deployment
kubectl --namespace="$K8S_NAMESPACE" scale deployment "$DEPLOYMENT_NAME" --replicas=0

# Rename green to blue
kubectl --namespace="$K8S_NAMESPACE" delete deployment "$DEPLOYMENT_NAME" || true
kubectl --namespace="$K8S_NAMESPACE" get deployment "$GREEN_DEPLOYMENT" -o yaml | \
    sed "s/${GREEN_DEPLOYMENT}/${DEPLOYMENT_NAME}/g" | \
    kubectl apply -f -
kubectl --namespace="$K8S_NAMESPACE" delete deployment "$GREEN_DEPLOYMENT"

# Remove traffic split (100% to new blue)
cat <<EOF | kubectl apply -f -
apiVersion: networking.istio.io/v1beta1
kind: VirtualService
metadata:
  name: ${SERVICE}-vs
  namespace: ${K8S_NAMESPACE}
spec:
  hosts:
  - ${SERVICE}
  http:
  - match:
    - uri:
        prefix: /
    route:
    - destination:
        host: ${SERVICE}
        subset: blue
      weight: 100
EOF

echo ""
echo -e "${GREEN}======================================${NC}"
echo -e "${GREEN}Deployment Complete!${NC}"
echo -e "${GREEN}======================================${NC}"
echo ""
echo "Service:  $SERVICE"
echo "Version:  $VERSION"
echo "Environment: $ENVIRONMENT"
echo ""
echo "Monitor: kubectl --namespace=$K8S_NAMESPACE get pods -l app=$SERVICE -w"
echo "Logs:    kubectl --namespace=$K8S_NAMESPACE logs -f deployment/$DEPLOYMENT_NAME"
echo ""

exit 0
