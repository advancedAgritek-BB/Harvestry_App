# User Secrets Configuration

## Overview
This document explains how to configure database credentials using .NET User Secrets to avoid storing sensitive information in source control.

## Setup Instructions

### 1. Initialize User Secrets
Run the following command from the project directory (`genetics/API`):
```bash
dotnet user-secrets init
```

### 2. Set Database Credentials
Add the database username and password to User Secrets:
```bash
dotnet user-secrets set "ConnectionStrings:GeneticsDb:Username" "harvestry_app"
dotnet user-secrets set "ConnectionStrings:GeneticsDb:Password" "your_dev_password"
```

### 3. Connection String Assembly
The application will automatically merge these secrets with the connection string defined in `appsettings.Development.json`. The final connection string will be:
```
Host=localhost;Port=5432;Database=harvestry_genetics;Username={from_secrets};Password={from_secrets}
```

### 4. Verify Configuration
You can list all configured secrets with:
```bash
dotnet user-secrets list
```

## Alternative: Full Connection String in Secrets
If you prefer to store the entire connection string in User Secrets:
```bash
dotnet user-secrets set "ConnectionStrings:GeneticsDb" "Host=localhost;Port=5432;Database=harvestry_genetics;Username=harvestry_app;Password=your_password"
```

## Production Configuration
For production environments, use:
- Environment variables
- Azure Key Vault
- AWS Secrets Manager
- Or other secure secret management solutions

**Never commit credentials to source control.**

