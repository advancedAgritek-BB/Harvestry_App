#!/bin/bash
# FRP-05 Day Zero: Environment Variable Setup Script
# Purpose: Create and validate environment variables for FRP-05
# Usage: ./setup-frp05-environment.sh [environment]

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

ENVIRONMENT="${1:-dev}"
ENV_FILE=".env.frp05.${ENVIRONMENT}"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}FRP-05 Environment Setup${NC}"
echo -e "${BLUE}========================================${NC}"
echo ""
echo -e "Environment: ${YELLOW}${ENVIRONMENT}${NC}"
echo -e "Target file: ${YELLOW}${ENV_FILE}${NC}"
echo ""

# Create environment file with template
cat > "$ENV_FILE" << 'EOF'
# FRP-05 Telemetry Ingest & Rollups - Environment Variables
# Generated: $(date)
# Environment: ${ENVIRONMENT}

#
# Database Configuration
#
# TimescaleDB-enabled PostgreSQL connection string
# Must support: extensions, compression, continuous aggregates
TELEMETRY_DB_CONNECTION="postgresql://postgres:postgres@localhost:5432/harvestry_${ENVIRONMENT}"

#
# MQTT Broker Configuration
#
# MQTT broker endpoint for device telemetry ingest
TELEMETRY_MQTT_BROKER_URL="mqtt://localhost:1883"

# MQTT authentication (optional - use for authenticated brokers)
# Store these in secrets manager for production
TELEMETRY_MQTT_USERNAME=""
TELEMETRY_MQTT_PASSWORD=""

# MQTT topic prefix (default: site/{siteId}/equipment/{equipmentId}/telemetry)
TELEMETRY_MQTT_TOPIC_PREFIX="site"

#
# HTTP Ingest Configuration
#
# API key for HTTP ingest endpoints (optional until external partners onboard)
TELEMETRY_HTTP_API_KEY=""

#
# Performance Tuning
#
# Maximum readings per ingest batch (protects memory)
TELEMETRY_MAX_BATCH_SIZE=5000

# Target batch size for TimescaleDB COPY operations (bytes)
TELEMETRY_COPY_BATCH_BYTES=1048576

#
# Alert Engine Configuration
#
# Alert evaluation interval (seconds)
TELEMETRY_ALERT_EVALUATION_INTERVAL=30

# Default cooldown between repeated alert firings (minutes)
TELEMETRY_ALERT_COOLDOWN_MINUTES=15

#
# Real-Time Push Configuration (SignalR/WebSocket)
#
# Comma-separated list of allowed origins for WebSocket connections
TELEMETRY_SIGNALR_ALLOWED_ORIGINS="http://localhost:3000,http://localhost:3001"

# PostgreSQL logical replication slot name for WAL fan-out
TELEMETRY_WAL_SLOT_NAME="telemetry_wal_slot"

#
# Feature Flags
#
# Enable/disable specific features
TELEMETRY_MQTT_ENABLED=true
TELEMETRY_HTTP_INGEST_ENABLED=true
TELEMETRY_REALTIME_PUSH_ENABLED=true
TELEMETRY_ALERT_ENGINE_ENABLED=true

#
# Logging & Monitoring
#
# Log level (Debug, Information, Warning, Error)
TELEMETRY_LOG_LEVEL=Information

# Enable detailed performance metrics
TELEMETRY_ENABLE_DETAILED_METRICS=true
EOF

echo -e "${GREEN}✓ Environment file created: ${ENV_FILE}${NC}"
echo ""
echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}Required Actions${NC}"
echo -e "${YELLOW}========================================${NC}"
echo ""
echo "1. Edit ${ENV_FILE} and update the following:"
echo ""
echo "   ${BLUE}REQUIRED:${NC}"
echo "   - TELEMETRY_DB_CONNECTION (database connection string)"
echo "   - TELEMETRY_MQTT_BROKER_URL (MQTT broker endpoint)"
echo ""
echo "   ${YELLOW}OPTIONAL:${NC}"
echo "   - TELEMETRY_MQTT_USERNAME (if broker requires auth)"
echo "   - TELEMETRY_MQTT_PASSWORD (if broker requires auth)"
echo "   - TELEMETRY_SIGNALR_ALLOWED_ORIGINS (frontend URLs)"
echo ""
echo "2. For production/staging:"
echo "   - Store sensitive values in secrets manager"
echo "   - Reference secrets via environment variable injection"
echo "   - Never commit .env files to version control"
echo ""
echo "3. Validate configuration:"
echo "   ./scripts/setup/validate-frp05-environment.sh ${ENVIRONMENT}"
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Next Steps${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo "After configuring ${ENV_FILE}, run validation:"
echo ""
echo "  source ${ENV_FILE}"
echo "  ./scripts/db/validate-timescaledb.sh"
echo "  ./scripts/db/validate-logical-replication.sh"
echo ""

