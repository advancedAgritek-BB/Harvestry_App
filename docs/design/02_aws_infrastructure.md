# AWS Infrastructure Design

**Document Version:** 1.0  
**Date:** December 4, 2025  
**Status:** Ready for Review

---

## 1. Architecture Diagram

```
┌────────────────────────────────────────────────────────────────────────────────────┐
│                                    AWS Region                                       │
│                                   (us-east-1)                                       │
│                                                                                     │
│  ┌──────────────────────────────────────────────────────────────────────────────┐  │
│  │                              VPC: harvestry-vpc                               │  │
│  │                             CIDR: 10.0.0.0/16                                 │  │
│  │                                                                               │  │
│  │  ┌─────────────────────────────┐  ┌─────────────────────────────┐           │  │
│  │  │    Public Subnet A          │  │    Public Subnet B          │           │  │
│  │  │    10.0.1.0/24              │  │    10.0.2.0/24              │           │  │
│  │  │                             │  │                             │           │  │
│  │  │  ┌─────────────────────┐   │  │  ┌─────────────────────┐   │           │  │
│  │  │  │   NAT Gateway       │   │  │  │   NAT Gateway       │   │           │  │
│  │  │  └─────────────────────┘   │  │  └─────────────────────┘   │           │  │
│  │  └─────────────────────────────┘  └─────────────────────────────┘           │  │
│  │                                                                               │  │
│  │  ┌─────────────────────────────┐  ┌─────────────────────────────┐           │  │
│  │  │   Private Subnet A          │  │   Private Subnet B          │           │  │
│  │  │   10.0.10.0/24              │  │   10.0.20.0/24              │           │  │
│  │  │                             │  │                             │           │  │
│  │  │  ┌─────────────────────┐   │  │  ┌─────────────────────┐   │           │  │
│  │  │  │  ECS Task (API)     │   │  │  │  ECS Task (API)     │   │           │  │
│  │  │  │  - Identity Svc     │   │  │  │  - Identity Svc     │   │           │  │
│  │  │  │  - Genetics Svc     │   │  │  │  - Genetics Svc     │   │           │  │
│  │  │  │  - Tasks Svc        │   │  │  │  - Tasks Svc        │   │           │  │
│  │  │  │  - Spatial Svc      │   │  │  │  - Spatial Svc      │   │           │  │
│  │  │  └─────────────────────┘   │  │  └─────────────────────┘   │           │  │
│  │  └─────────────────────────────┘  └─────────────────────────────┘           │  │
│  │                                                                               │  │
│  │  ┌─────────────────────────────┐  ┌─────────────────────────────┐           │  │
│  │  │   Database Subnet A         │  │   Database Subnet B         │           │  │
│  │  │   10.0.100.0/24             │  │   10.0.200.0/24             │           │  │
│  │  │                             │  │                             │           │  │
│  │  │  ┌─────────────────────┐   │  │  ┌─────────────────────┐   │           │  │
│  │  │  │  RDS Primary        │   │  │  │  RDS Standby        │   │           │  │
│  │  │  │  PostgreSQL 15      │   │  │  │  (Multi-AZ)         │   │           │  │
│  │  │  │  + TimescaleDB      │   │  │  │                     │   │           │  │
│  │  │  └─────────────────────┘   │  │  └─────────────────────┘   │           │  │
│  │  │                             │  │                             │           │  │
│  │  │  ┌─────────────────────┐   │  │                             │           │  │
│  │  │  │  ElastiCache Redis  │   │  │                             │           │  │
│  │  │  │  (Sessions/Cache)   │   │  │                             │           │  │
│  │  │  └─────────────────────┘   │  │                             │           │  │
│  │  └─────────────────────────────┘  └─────────────────────────────┘           │  │
│  │                                                                               │  │
│  └──────────────────────────────────────────────────────────────────────────────┘  │
│                                                                                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐                    │
│  │   ALB           │  │   CloudFront    │  │   S3 Bucket     │                    │
│  │   (API)         │  │   (Frontend)    │  │   (Static)      │                    │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘                    │
│                                                                                     │
└────────────────────────────────────────────────────────────────────────────────────┘
                                        │
                                        │ HTTPS
                                        ▼
                               ┌─────────────────┐
                               │    Internet     │
                               └─────────────────┘
                                        │
                            ┌───────────┴───────────┐
                            │                       │
                    ┌───────▼───────┐       ┌──────▼──────┐
                    │    Users      │       │  Supabase   │
                    │               │       │    Auth     │
                    └───────────────┘       └─────────────┘
```

---

## 2. Compute: ECS Fargate

### 2.1 Cluster Configuration

```yaml
# infrastructure/ecs/cluster.tf

resource "aws_ecs_cluster" "harvestry" {
  name = "harvestry-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  configuration {
    execute_command_configuration {
      logging = "OVERRIDE"
      log_configuration {
        cloud_watch_log_group_name = "/ecs/harvestry/exec"
      }
    }
  }
}

resource "aws_ecs_cluster_capacity_providers" "harvestry" {
  cluster_name = aws_ecs_cluster.harvestry.name

  capacity_providers = ["FARGATE", "FARGATE_SPOT"]

  default_capacity_provider_strategy {
    capacity_provider = "FARGATE"
    weight            = 1
    base              = 1
  }
}
```

### 2.2 Service Task Definition

```yaml
# infrastructure/ecs/task-definitions/identity-service.tf

resource "aws_ecs_task_definition" "identity" {
  family                   = "harvestry-identity"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = 256
  memory                   = 512
  execution_role_arn       = aws_iam_role.ecs_execution.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  container_definitions = jsonencode([
    {
      name  = "identity-api"
      image = "${aws_ecr_repository.harvestry.repository_url}:identity-latest"
      
      portMappings = [
        {
          containerPort = 8080
          protocol      = "tcp"
        }
      ]

      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = "Production"
        }
      ]

      secrets = [
        {
          name      = "ConnectionStrings__PostgreSQL"
          valueFrom = "${aws_secretsmanager_secret.db_connection.arn}:connection_string::"
        },
        {
          name      = "Supabase__JwtSecret"
          valueFrom = "${aws_secretsmanager_secret.supabase.arn}:jwt_secret::"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = "/ecs/harvestry/identity"
          awslogs-region        = var.region
          awslogs-stream-prefix = "ecs"
        }
      }

      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"]
        interval    = 30
        timeout     = 5
        retries     = 3
        startPeriod = 60
      }
    }
  ])
}
```

### 2.3 Service Configuration

```yaml
resource "aws_ecs_service" "identity" {
  name            = "identity-service"
  cluster         = aws_ecs_cluster.harvestry.id
  task_definition = aws_ecs_task_definition.identity.arn
  desired_count   = 2
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = var.private_subnet_ids
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.identity.arn
    container_name   = "identity-api"
    container_port   = 8080
  }

  deployment_configuration {
    maximum_percent         = 200
    minimum_healthy_percent = 100
  }

  deployment_circuit_breaker {
    enable   = true
    rollback = true
  }
}
```

---

## 3. Database: RDS PostgreSQL

### 3.1 RDS Instance

```yaml
# infrastructure/rds/main.tf

resource "aws_db_instance" "harvestry" {
  identifier = "harvestry-db"
  
  # Engine
  engine               = "postgres"
  engine_version       = "15.4"
  instance_class       = "db.t3.medium"  # Adjust for production
  
  # Storage
  allocated_storage     = 100
  max_allocated_storage = 500
  storage_type          = "gp3"
  storage_encrypted     = true
  kms_key_id           = aws_kms_key.rds.arn

  # Database
  db_name  = "harvestry"
  username = "harvestry_admin"
  password = random_password.db_password.result

  # Network
  db_subnet_group_name   = aws_db_subnet_group.harvestry.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  publicly_accessible    = false
  port                   = 5432

  # High Availability
  multi_az = true

  # Backup
  backup_retention_period = 30
  backup_window           = "03:00-04:00"
  maintenance_window      = "Mon:04:00-Mon:05:00"
  
  # Deletion protection
  deletion_protection = true
  skip_final_snapshot = false
  final_snapshot_identifier = "harvestry-final-${formatdate("YYYY-MM-DD", timestamp())}"

  # Performance Insights
  performance_insights_enabled = true
  performance_insights_retention_period = 7

  # Logging
  enabled_cloudwatch_logs_exports = ["postgresql", "upgrade"]

  # Parameter group for TimescaleDB
  parameter_group_name = aws_db_parameter_group.harvestry.name

  tags = {
    Name        = "harvestry-db"
    Environment = var.environment
  }
}

# Parameter group for PostgreSQL + TimescaleDB
resource "aws_db_parameter_group" "harvestry" {
  family = "postgres15"
  name   = "harvestry-params"

  parameter {
    name  = "shared_preload_libraries"
    value = "timescaledb"
  }

  parameter {
    name  = "max_connections"
    value = "200"
  }

  parameter {
    name  = "log_statement"
    value = "ddl"
  }
}
```

### 3.2 Database Subnet Group

```yaml
resource "aws_db_subnet_group" "harvestry" {
  name       = "harvestry-db-subnet-group"
  subnet_ids = var.database_subnet_ids

  tags = {
    Name = "Harvestry DB Subnet Group"
  }
}
```

### 3.3 Security Group

```yaml
resource "aws_security_group" "rds" {
  name        = "harvestry-rds-sg"
  description = "Security group for Harvestry RDS"
  vpc_id      = var.vpc_id

  ingress {
    description     = "PostgreSQL from ECS"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_tasks.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}
```

---

## 4. Caching: ElastiCache Redis

```yaml
# infrastructure/elasticache/main.tf

resource "aws_elasticache_cluster" "harvestry" {
  cluster_id           = "harvestry-cache"
  engine               = "redis"
  engine_version       = "7.0"
  node_type            = "cache.t3.micro"  # Adjust for production
  num_cache_nodes      = 1
  parameter_group_name = "default.redis7"
  port                 = 6379
  
  subnet_group_name  = aws_elasticache_subnet_group.harvestry.name
  security_group_ids = [aws_security_group.elasticache.id]

  snapshot_retention_limit = 7
  snapshot_window          = "05:00-06:00"

  tags = {
    Name        = "harvestry-cache"
    Environment = var.environment
  }
}

resource "aws_elasticache_subnet_group" "harvestry" {
  name       = "harvestry-cache-subnet"
  subnet_ids = var.database_subnet_ids
}

resource "aws_security_group" "elasticache" {
  name        = "harvestry-elasticache-sg"
  description = "Security group for Harvestry ElastiCache"
  vpc_id      = var.vpc_id

  ingress {
    description     = "Redis from ECS"
    from_port       = 6379
    to_port         = 6379
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_tasks.id]
  }
}
```

---

## 5. Load Balancer

```yaml
# infrastructure/alb/main.tf

resource "aws_lb" "harvestry" {
  name               = "harvestry-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = var.public_subnet_ids

  enable_deletion_protection = true
  enable_http2               = true

  tags = {
    Name        = "harvestry-alb"
    Environment = var.environment
  }
}

resource "aws_lb_listener" "https" {
  load_balancer_arn = aws_lb.harvestry.arn
  port              = "443"
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = aws_acm_certificate.harvestry.arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }
}

resource "aws_lb_listener" "http_redirect" {
  load_balancer_arn = aws_lb.harvestry.arn
  port              = "80"
  protocol          = "HTTP"

  default_action {
    type = "redirect"
    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }
}

# Target group for API services
resource "aws_lb_target_group" "api" {
  name        = "harvestry-api-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    interval            = 30
    matcher             = "200"
    path                = "/health"
    port                = "traffic-port"
    protocol            = "HTTP"
    timeout             = 5
    unhealthy_threshold = 3
  }
}
```

---

## 6. Frontend: AWS Amplify

```yaml
# infrastructure/amplify/main.tf

resource "aws_amplify_app" "harvestry" {
  name       = "harvestry-frontend"
  repository = "https://github.com/your-org/harvestry-app"

  build_spec = <<-EOT
version: 1
frontend:
  phases:
    preBuild:
      commands:
        - npm ci
    build:
      commands:
        - npm run build
  artifacts:
    baseDirectory: .next
    files:
      - '**/*'
  cache:
    paths:
      - node_modules/**/*
EOT

  environment_variables = {
    NEXT_PUBLIC_SUPABASE_URL      = var.supabase_url
    NEXT_PUBLIC_SUPABASE_ANON_KEY = var.supabase_anon_key
    NEXT_PUBLIC_API_URL           = "https://api.harvestry.io"
  }

  custom_rule {
    source = "/<*>"
    status = "404-200"
    target = "/index.html"
  }
}

resource "aws_amplify_branch" "main" {
  app_id      = aws_amplify_app.harvestry.id
  branch_name = "main"
  stage       = "PRODUCTION"

  environment_variables = {
    NEXT_PUBLIC_ENV = "production"
  }
}

resource "aws_amplify_domain_association" "harvestry" {
  app_id      = aws_amplify_app.harvestry.id
  domain_name = "harvestry.io"

  sub_domain {
    branch_name = aws_amplify_branch.main.branch_name
    prefix      = "app"
  }
}
```

---

## 7. Secrets Management

```yaml
# infrastructure/secrets/main.tf

resource "aws_secretsmanager_secret" "db_connection" {
  name        = "harvestry/database/connection"
  description = "Database connection string"
}

resource "aws_secretsmanager_secret_version" "db_connection" {
  secret_id = aws_secretsmanager_secret.db_connection.id
  secret_string = jsonencode({
    connection_string = "Host=${aws_db_instance.harvestry.endpoint};Database=harvestry;Username=harvestry_admin;Password=${random_password.db_password.result}"
  })
}

resource "aws_secretsmanager_secret" "supabase" {
  name        = "harvestry/supabase/credentials"
  description = "Supabase credentials"
}

resource "aws_secretsmanager_secret_version" "supabase" {
  secret_id = aws_secretsmanager_secret.supabase.id
  secret_string = jsonencode({
    jwt_secret     = var.supabase_jwt_secret
    webhook_secret = var.supabase_webhook_secret
  })
}
```

---

## 8. Monitoring: CloudWatch

```yaml
# infrastructure/monitoring/main.tf

# Log groups
resource "aws_cloudwatch_log_group" "ecs" {
  for_each = toset(["identity", "genetics", "spatial", "tasks", "telemetry"])
  
  name              = "/ecs/harvestry/${each.key}"
  retention_in_days = 30
}

# Dashboard
resource "aws_cloudwatch_dashboard" "harvestry" {
  dashboard_name = "Harvestry-Operations"

  dashboard_body = jsonencode({
    widgets = [
      {
        type   = "metric"
        x      = 0
        y      = 0
        width  = 12
        height = 6
        properties = {
          title  = "ECS Service CPU Utilization"
          region = var.region
          metrics = [
            ["AWS/ECS", "CPUUtilization", "ServiceName", "identity-service", "ClusterName", "harvestry-cluster"],
            [".", ".", ".", "genetics-service", ".", "."],
            [".", ".", ".", "tasks-service", ".", "."]
          ]
        }
      },
      {
        type   = "metric"
        x      = 12
        y      = 0
        width  = 12
        height = 6
        properties = {
          title  = "RDS Connections"
          region = var.region
          metrics = [
            ["AWS/RDS", "DatabaseConnections", "DBInstanceIdentifier", "harvestry-db"]
          ]
        }
      }
    ]
  })
}

# Alarms
resource "aws_cloudwatch_metric_alarm" "high_cpu" {
  alarm_name          = "harvestry-high-cpu"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 2
  metric_name         = "CPUUtilization"
  namespace           = "AWS/ECS"
  period              = 300
  statistic           = "Average"
  threshold           = 80
  alarm_description   = "ECS CPU utilization is high"
  
  dimensions = {
    ClusterName = aws_ecs_cluster.harvestry.name
  }

  alarm_actions = [aws_sns_topic.alerts.arn]
}
```

---

## 9. Cost Estimate

| Service | Configuration | Est. Monthly Cost |
|---------|--------------|-------------------|
| RDS PostgreSQL | db.t3.medium, Multi-AZ, 100GB | $120-180 |
| ECS Fargate | 5 services × 0.25 vCPU × 0.5 GB | $40-80 |
| ALB | 1 ALB + data processing | $20-40 |
| ElastiCache Redis | cache.t3.micro | $15-25 |
| Amplify Hosting | Build minutes + hosting | $5-20 |
| CloudWatch | Logs + metrics | $10-30 |
| Secrets Manager | 5 secrets | $2-3 |
| S3 | Static assets | $1-5 |
| **Total** | | **$213-383/mo** |

*Costs scale with usage. Production workloads may require larger instances.*

---

## 10. Deployment Pipeline

```yaml
# .github/workflows/deploy.yml

name: Deploy to AWS

on:
  push:
    branches: [main]

jobs:
  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Login to ECR
        uses: aws-actions/amazon-ecr-login@v2

      - name: Build and push images
        run: |
          docker build -t harvestry/identity ./src/backend/services/core-platform/identity
          docker tag harvestry/identity:latest $ECR_REGISTRY/harvestry:identity-latest
          docker push $ECR_REGISTRY/harvestry:identity-latest

      - name: Update ECS service
        run: |
          aws ecs update-service --cluster harvestry-cluster --service identity-service --force-new-deployment
```

---

*End of AWS Infrastructure Design*


