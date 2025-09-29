# Sentry Error Tracking Setup

**Track A Requirement** - Error monitoring and alerting

## 🎯 Purpose

Sentry provides real-time error tracking, performance monitoring, and release health tracking for Harvestry ERP services.

---

## 🚀 Getting Started

### 1. Create Sentry Project

#### Option A: Self-Hosted (Recommended for Data Control)
```bash
# Clone Sentry
git clone https://github.com/getsentry/self-hosted.git sentry-self-hosted
cd sentry-self-hosted

# Install
./install.sh

# Start services
docker-compose up -d

# Access Sentry at http://localhost:9000
# Default credentials: admin@sentry.local / password (change immediately)
```

#### Option B: Sentry.io (SaaS)
1. Visit https://sentry.io/signup/
2. Create organization: "Harvestry"
3. Create project: "Harvestry-API" (Platform: ASP.NET Core)
4. Note your DSN: `https://<key>@<org>.ingest.sentry.io/<project>`

---

## 🔧 Backend Integration (.NET)

### Install SDK
```bash
dotnet add package Sentry.AspNetCore
```

### Configure in `Program.cs`
```csharp
using Sentry;

var builder = WebApplication.CreateBuilder(args);

// Add Sentry
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Environment = builder.Environment.EnvironmentName;
    options.TracesSampleRate = builder.Environment.IsProduction() ? 0.1 : 1.0;
    options.ProfilesSampleRate = builder.Environment.IsProduction() ? 0.1 : 1.0;
    
    // Release tracking
    options.Release = builder.Configuration["Sentry:Release"] ?? "unknown";
    
    // Performance monitoring
    options.EnableTracing = true;
    options.TracesSampler = context =>
    {
        // Sample 100% of errors, 10% of successful requests
        if (context.CustomSamplingContext.TryGetValue("is_error", out var isError) && (bool)isError)
        {
            return 1.0;
        }
        return 0.1;
    };
    
    // Tag requests with site_id for filtering
    options.BeforeSend = (sentryEvent) =>
    {
        // Add custom tags
        if (sentryEvent.Request?.Headers?.ContainsKey("X-Site-Id") == true)
        {
            sentryEvent.SetTag("site_id", sentryEvent.Request.Headers["X-Site-Id"].ToString());
        }
        
        return sentryEvent;
    };
    
    // Filter sensitive data
    options.BeforeBreadcrumb = (breadcrumb) =>
    {
        // Remove PII from breadcrumbs
        if (breadcrumb.Data?.ContainsKey("password") == true)
        {
            breadcrumb.Data["password"] = "[Filtered]";
        }
        return breadcrumb;
    };
});

var app = builder.Build();

// Use Sentry middleware
app.UseSentryTracing();

// Health check endpoint (no side effects)
app.MapGet("/health/sentry", () => "Sentry integration is configured");

// Admin-only endpoint for manual Sentry testing (requires authentication)
app.MapPost("/admin/test-sentry", () =>
{
    SentrySdk.CaptureMessage("Manual Sentry test triggered by admin", SentryLevel.Info);
    return Results.Ok(new { message = "Sentry test event sent" });
})
.RequireAuthorization("AdminOnly"); // Assumes you have admin policy configured

app.Run();
```

### Configure in `appsettings.json`
```json
{
  "Sentry": {
    "Dsn": "${SENTRY_DSN}",
    "Environment": "production",
    "Release": "${RELEASE_VERSION}",
    "TracesSampleRate": 0.1,
    "ProfilesSampleRate": 0.1,
    "Debug": false,
    "AttachStacktrace": true,
    "SendDefaultPii": false
  }
}
```

### Manual Error Capture
```csharp
// Capture exception
try
{
    // Risky operation
}
catch (Exception ex)
{
    SentrySdk.CaptureException(ex, scope =>
    {
        scope.SetTag("operation", "inventory_destruction");
        scope.SetExtra("lot_id", lotId);
        scope.Level = SentryLevel.Error;
    });
}

// Capture message
SentrySdk.CaptureMessage("High-risk operation completed", SentryLevel.Warning);

// Add breadcrumb
SentrySdk.AddBreadcrumb("User initiated closed-loop control", "user.action", level: BreadcrumbLevel.Info);
```

---

## 🎨 Frontend Integration (Next.js)

### Install SDK
```bash
npm install @sentry/nextjs
```

### Configure `sentry.client.config.ts`
```typescript
import * as Sentry from "@sentry/nextjs";

Sentry.init({
  dsn: process.env.NEXT_PUBLIC_SENTRY_DSN,
  environment: process.env.NEXT_PUBLIC_ENVIRONMENT || "development",
  release: process.env.NEXT_PUBLIC_RELEASE_VERSION,
  
  // Performance monitoring
  tracesSampleRate: process.env.NODE_ENV === "production" ? 0.1 : 1.0,
  
  // Session replay
  replaysSessionSampleRate: 0.1,
  replaysOnErrorSampleRate: 1.0,
  
  // Filter sensitive data
  beforeSend(event, hint) {
    // Remove PII
    if (event.user) {
      delete event.user.email;
      delete event.user.ip_address;
    }
    return event;
  },
  
  integrations: [
    new Sentry.BrowserTracing({
      tracePropagationTargets: [
        "localhost",
        /^https:\/\/api\.harvestry\.com/,
      ],
    }),
    new Sentry.Replay({
      maskAllText: true,
      blockAllMedia: true,
    }),
  ],
});
```

### Usage in Components
```typescript
import * as Sentry from "@sentry/nextjs";

// Capture error
try {
  await fetchData();
} catch (error) {
  Sentry.captureException(error, {
    tags: { component: "TaskList" },
    extra: { siteId: currentSiteId },
  });
}

// Set user context
Sentry.setUser({
  id: user.id,
  username: user.username,
});

// Add breadcrumb
Sentry.addBreadcrumb({
  category: "ui.click",
  message: "User clicked submit button",
  level: "info",
});
```

---

## 📊 Alert Configuration

### Create Alert Rules

1. **High Error Rate**
   - Condition: Error count > 100 in 5 minutes
   - Action: Slack #alerts, Email on-call
   
2. **P0 Critical Errors**
   - Condition: Error with tag `severity:critical`
   - Action: PagerDuty, Slack #incidents
   
3. **Performance Degradation**
   - Condition: p95 latency > 2s for 10 minutes
   - Action: Slack #performance

### Slack Integration
```bash
# In Sentry UI:
# Settings → Integrations → Slack
# Connect workspace → Select channel → Save
```

### PagerDuty Integration
```bash
# In Sentry UI:
# Settings → Integrations → PagerDuty
# Enter Integration Key → Save
```

---

## 🏷️ Release Tracking

### Create Release
```bash
# In CI/CD pipeline
sentry-cli releases new "${RELEASE_VERSION}"
sentry-cli releases set-commits "${RELEASE_VERSION}" --auto
sentry-cli releases finalize "${RELEASE_VERSION}"
```

### Deploy Notification
```bash
sentry-cli releases deploys "${RELEASE_VERSION}" new -e production
```

### GitHub Actions Integration
```yaml
- name: Create Sentry Release
  uses: getsentry/action-release@v1
  env:
    SENTRY_AUTH_TOKEN: ${{ secrets.SENTRY_AUTH_TOKEN }}
    SENTRY_ORG: harvestry
    SENTRY_PROJECT: harvestry-api
  with:
    environment: production
    version: ${{ github.sha }}
```

---

## 📈 Performance Monitoring

### Transaction Naming
```csharp
// In ASP.NET Core
app.Use(async (context, next) =>
{
    var transaction = SentrySdk.GetSpan()?.StartChild("http.request");
    transaction?.SetTag("http.method", context.Request.Method);
    transaction?.SetTag("http.route", context.Request.Path);
    
    await next();
    
    transaction?.Finish();
});
```

### Custom Spans
```csharp
var transaction = SentrySdk.StartTransaction("process_irrigation", "task");

var span = transaction.StartChild("database.query");
try
{
    await _dbContext.SaveChangesAsync();
}
finally
{
    span.Finish();
}

transaction.Finish();
```

---

## 🔍 Querying & Analysis

### Discover Queries

#### Top Errors by Site
```sql
SELECT
  tags[site_id] AS site,
  COUNT() AS error_count
FROM
  events
WHERE
  timestamp > now() - 24h
GROUP BY
  site
ORDER BY
  error_count DESC
LIMIT 10
```

#### P95 Latency by Endpoint
```sql
SELECT
  transaction,
  quantile(0.95)(duration) AS p95
FROM
  transactions
WHERE
  timestamp > now() - 1h
GROUP BY
  transaction
ORDER BY
  p95 DESC
```

---

## ✅ Track A Checklist

- [ ] Sentry project created (self-hosted or SaaS)
- [ ] Backend SDK integrated (.NET services)
- [ ] Frontend SDK integrated (Next.js)
- [ ] Environment variables configured
- [ ] Release tracking enabled
- [ ] Alert rules configured (Slack, PagerDuty)
- [ ] Performance monitoring enabled
- [ ] Sensitive data filtering implemented
- [ ] Team members invited
- [ ] On-call runbook updated with Sentry links

---

**✅ Track A Objective:** Real-time error monitoring with automated alerting and performance insights.
