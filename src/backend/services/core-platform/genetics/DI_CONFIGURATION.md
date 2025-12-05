# Genetics Service - Dependency Injection Configuration

## Program.cs Registration Checklist

### 1. Database Connection (Reuse Existing)
```csharp
// Already registered in core-platform - reuse the existing NpgsqlDataSource
// Connection string: "GENETICS_DB_CONNECTION" (or reuse spatial/identity connection)
```

### 2. Repositories (Scoped)
```csharp
// ✅ SLICE 1 COMPLETE
builder.Services.AddScoped<IGeneticsRepository, GeneticsRepository>();
builder.Services.AddScoped<IPhenotypeRepository, PhenotypeRepository>();
builder.Services.AddScoped<IStrainRepository, StrainRepository>();

// ✅ SLICE 2 COMPLETE
builder.Services.AddScoped<IBatchRepository, BatchRepository>();
builder.Services.AddScoped<IBatchEventRepository, BatchEventRepository>();
builder.Services.AddScoped<IBatchRelationshipRepository, BatchRelationshipRepository>();
builder.Services.AddScoped<IBatchStageDefinitionRepository, BatchStageDefinitionRepository>();
builder.Services.AddScoped<IBatchStageTransitionRepository, BatchStageTransitionRepository>();
builder.Services.AddScoped<IBatchStageHistoryRepository, BatchStageHistoryRepository>();
builder.Services.AddScoped<IBatchCodeRuleRepository, BatchCodeRuleRepository>();
builder.Services.AddScoped<IMotherPlantRepository, MotherPlantRepository>();
builder.Services.AddScoped<IMotherHealthLogRepository, MotherHealthLogRepository>();
builder.Services.AddScoped<IPropagationSettingsRepository, PropagationSettingsRepository>();
builder.Services.AddScoped<IPropagationOverrideRequestRepository, PropagationOverrideRequestRepository>();
builder.Services.AddScoped<IMotherPlantRepository, MotherPlantRepository>();
builder.Services.AddScoped<IMotherHealthLogRepository, MotherHealthLogRepository>();
builder.Services.AddScoped<IPropagationSettingsRepository, PropagationSettingsRepository>();
builder.Services.AddScoped<IPropagationOverrideRequestRepository, PropagationOverrideRequestRepository>();
```

### 3. Application Services (Scoped)
```csharp
// ✅ SLICE 1 COMPLETE
builder.Services.AddScoped<IGeneticsManagementService, GeneticsManagementService>();

// ✅ SLICE 2 COMPLETE
builder.Services.AddScoped<IBatchLifecycleService, BatchLifecycleService>();
builder.Services.AddScoped<IBatchStageConfigurationService, BatchStageConfigurationService>();
builder.Services.AddScoped<IMotherHealthService, MotherHealthService>();
```

### 4. Validators (Transient)
```csharp
// ✅ SLICE 1 COMPLETE - All 6 validators created
builder.Services.AddValidatorsFromAssemblyContaining<CreateGeneticsRequestValidator>();
```

### 5. FluentValidation Integration
```csharp
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
```

### 6. RLS Context Accessor (if needed)
```csharp
// Reuse from identity service
builder.Services.AddScoped<IRlsContextAccessor, AsyncLocalRlsContextAccessor>();
```

### 7. Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("GeneticsDb"),
        name: "genetics-database",
        tags: new[] { "db", "genetics" });
```

### 8. CORS Policy (if standalone API)
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("GeneticsPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

### 9. Rate Limiting (if needed)
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("genetics", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.SegmentsPerWindow = 2;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
});
```

### 10. Swagger/OpenAPI
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Harvestry Genetics API",
        Version = "v1",
        Description = "Genetics, Strains, and Batch Management API"
    });
});
```

### 11. Serilog Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Genetics")
    .WriteTo.Console(new JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();
```

### 12. Controllers
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
```

## Middleware Pipeline

```csharp
var app = builder.Build();

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection
app.UseHttpsRedirection();

// CORS (if configured)
app.UseCors("GeneticsPolicy");

// Rate limiting (if configured)
app.UseRateLimiter();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// RLS Context Middleware (set from JWT claims)
app.UseMiddleware<RlsContextMiddleware>();

// Error handling
app.UseMiddleware<ErrorHandlingMiddleware>();

// Map controllers
app.MapControllers();

// Health checks
app.MapHealthChecks("/health");

app.Run();
```

## Environment Variables

### Required
- `GENETICS_DB_CONNECTION` - PostgreSQL connection string (or reuse existing)
- `JWT_SECRET` - JWT signing key (for authentication)
- `JWT_ISSUER` - JWT issuer
- `JWT_AUDIENCE` - JWT audience

### Optional
- `ALLOWED_ORIGINS` - CORS allowed origins (comma-separated)
- `RATE_LIMIT_REQUESTS` - Rate limit per minute (default: 100)
- `LOG_LEVEL` - Logging level (default: Information)

## Notes

1. **Connection Pooling**: Reuse the existing `NpgsqlDataSource` from identity/spatial services
2. **RLS Context**: Leverage existing `IRlsContextAccessor` from identity service
3. **Validation**: FluentValidation auto-validation is enabled for all controllers
4. **Error Handling**: Centralized error handling via middleware (returns ProblemDetails)
5. **Logging**: Structured JSON logging via Serilog
6. **Health Checks**: Database connectivity check at `/health` endpoint

