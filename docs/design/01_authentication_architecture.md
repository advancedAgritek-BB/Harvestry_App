# Authentication Architecture Design

**Document Version:** 1.0  
**Date:** December 4, 2025  
**Status:** Ready for Review

---

## 1. Architecture Overview

### 1.1 Selected Architecture: AWS RDS + Supabase Auth

```
┌───────────────────────────────────────────────────────────────────┐
│                           AWS VPC                                  │
│                                                                    │
│  ┌────────────────┐    ┌────────────────┐    ┌────────────────┐   │
│  │    Next.js     │◄──►│  .NET Backend  │◄──►│   AWS RDS      │   │
│  │   (Amplify)    │    │    (ECS)       │    │  PostgreSQL    │   │
│  │                │    │                │    │  +TimescaleDB  │   │
│  └───────┬────────┘    └───────┬────────┘    └────────────────┘   │
│          │                     │                                   │
│          │                     │                                   │
└──────────│─────────────────────│───────────────────────────────────┘
           │                     │
           │    Auth Only        │
           ▼                     │
    ┌──────────────┐             │
    │   Supabase   │◄────────────┘ (JWT validation)
    │     Auth     │
    └──────────────┘
```

### 1.2 Key Design Decisions

| Decision | Rationale |
|----------|-----------|
| Supabase Auth Only | Simple SDK, reliable, battle-tested |
| AWS RDS for Data | No egress costs on high-volume data |
| JWT-based Auth | Stateless, scalable, industry standard |
| Keep existing RLS pattern | `current_setting()` works with JWTs |

---

## 2. Authentication Flow

### 2.1 Login Flow

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│   Browser   │         │   Supabase  │         │  Harvestry  │
│             │         │    Auth     │         │   Backend   │
└──────┬──────┘         └──────┬──────┘         └──────┬──────┘
       │                       │                       │
       │ 1. signInWithPassword │                       │
       │      (email, pass)    │                       │
       │──────────────────────►│                       │
       │                       │                       │
       │                       │ 2. Validate           │
       │                       │    credentials        │
       │                       │                       │
       │◄──────────────────────│                       │
       │ 3. JWT (access_token, │                       │
       │    refresh_token)     │                       │
       │                       │                       │
       │ 4. Store in memory    │                       │
       │    + localStorage     │                       │
       │                       │                       │
       │ 5. Fetch user profile │                       │
       │ Authorization: Bearer │                       │
       │──────────────────────────────────────────────►│
       │                       │                       │
       │                       │                       │ 6. Validate JWT
       │                       │                       │    (Supabase secret)
       │                       │                       │
       │◄──────────────────────────────────────────────│
       │ 7. User profile       │                       │
       │    + site permissions │                       │
```

### 2.2 Token Refresh Flow

```
┌─────────────┐         ┌─────────────┐
│   Browser   │         │   Supabase  │
│             │         │    Auth     │
└──────┬──────┘         └──────┬──────┘
       │                       │
       │ 1. Token expires      │
       │    (or nearing expiry)│
       │                       │
       │ 2. refreshSession()   │
       │    (refresh_token)    │
       │──────────────────────►│
       │                       │
       │◄──────────────────────│
       │ 3. New access_token   │
       │    + new refresh_token│
```

### 2.3 Logout Flow

```
┌─────────────┐         ┌─────────────┐
│   Browser   │         │   Supabase  │
│             │         │    Auth     │
└──────┬──────┘         └──────┬──────┘
       │                       │
       │ 1. signOut()          │
       │──────────────────────►│
       │                       │
       │◄──────────────────────│
       │ 2. Session invalidated│
       │                       │
       │ 3. Clear local state  │
       │    (tokens, user)     │
       │                       │
       │ 4. Redirect to /login │
```

---

## 3. User Registration

### 3.1 Self-Service Signup Flow

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│   Browser   │         │   Supabase  │         │  Harvestry  │         │    RDS      │
│             │         │    Auth     │         │   Backend   │         │             │
└──────┬──────┘         └──────┬──────┘         └──────┬──────┘         └──────┬──────┘
       │                       │                       │                       │
       │ 1. signUp(email,pass) │                       │                       │
       │──────────────────────►│                       │                       │
       │                       │                       │                       │
       │                       │ 2. Create user        │                       │
       │                       │    in auth.users      │                       │
       │                       │                       │                       │
       │                       │ 3. Send verification  │                       │
       │◄──────────────────────│    email              │                       │
       │                       │                       │                       │
       │ ────────────────────  │                       │                       │
       │ User clicks email link│                       │                       │
       │ ────────────────────  │                       │                       │
       │                       │                       │                       │
       │ 4. Webhook: user.created                      │                       │
       │                       │──────────────────────►│                       │
       │                       │                       │                       │
       │                       │                       │ 5. Create user record │
       │                       │                       │──────────────────────►│
       │                       │                       │                       │
       │                       │                       │ 6. Create organization│
       │                       │                       │──────────────────────►│
       │                       │                       │                       │
       │                       │                       │ 7. Create default site│
       │                       │                       │──────────────────────►│
       │                       │                       │                       │
       │                       │                       │ 8. Assign user to site│
       │                       │                       │──────────────────────►│
       │                       │                       │                       │
       │ 9. Redirect to app    │                       │                       │
       │    (confirmed)        │                       │                       │
```

### 3.2 Admin-Invited User Flow

```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│   Admin     │         │   Backend   │         │   Supabase  │
│             │         │             │         │    Auth     │
└──────┬──────┘         └──────┬──────┘         └──────┬──────┘
       │                       │                       │
       │ 1. POST /users/invite │                       │
       │    {email, siteId,    │                       │
       │     role}             │                       │
       │──────────────────────►│                       │
       │                       │                       │
       │                       │ 2. inviteUserByEmail()│
       │                       │──────────────────────►│
       │                       │                       │
       │                       │◄──────────────────────│
       │                       │ 3. Invitation sent    │
       │                       │                       │
       │                       │ 4. Pre-create user    │
       │                       │    record (pending)   │
       │                       │                       │
       │◄──────────────────────│                       │
       │ 5. Invitation sent    │                       │
       │                       │                       │
       │ ────────────────────  │                       │
       │ User clicks invite    │                       │
       │ Sets password         │                       │
       │ ────────────────────  │                       │
       │                       │                       │
       │                       │ 6. Webhook: user.created
       │                       │◄──────────────────────│
       │                       │                       │
       │                       │ 7. Activate user      │
       │                       │    record             │
```

---

## 4. JWT Structure

### 4.1 Supabase JWT Claims

```json
{
  "aud": "authenticated",
  "exp": 1701748800,
  "iat": 1701745200,
  "iss": "https://your-project.supabase.co/auth/v1",
  "sub": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "email": "user@example.com",
  "phone": "",
  "app_metadata": {
    "provider": "email",
    "providers": ["email"]
  },
  "user_metadata": {
    "full_name": "John Doe"
  },
  "role": "authenticated",
  "aal": "aal1",
  "amr": [{"method": "password", "timestamp": 1701745200}],
  "session_id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
}
```

### 4.2 Custom Claims (Optional)

If needed, add custom claims via Supabase hooks:

```sql
-- Supabase Auth Hook (runs on token generation)
CREATE OR REPLACE FUNCTION auth.custom_access_token_hook(event jsonb)
RETURNS jsonb AS $$
DECLARE
  claims jsonb;
  user_role text;
BEGIN
  -- Get user's primary role from our users table
  SELECT r.name INTO user_role
  FROM public.users u
  JOIN public.user_sites us ON us.user_id = u.id
  JOIN public.roles r ON r.id = us.role_id
  WHERE u.id = (event->>'user_id')::uuid
  AND us.is_primary_site = true
  LIMIT 1;

  claims := event->'claims';
  claims := jsonb_set(claims, '{harvestry_role}', to_jsonb(COALESCE(user_role, 'viewer')));
  
  RETURN jsonb_set(event, '{claims}', claims);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Enable the hook
ALTER FUNCTION auth.custom_access_token_hook SECURITY DEFINER;
```

---

## 5. Backend JWT Validation

### 5.1 JWT Validation Middleware

**File:** `src/backend/shared/Authentication/SupabaseJwtHandler.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSupabaseJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var supabaseUrl = configuration["Supabase:Url"];
        var jwtSecret = configuration["Supabase:JwtSecret"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"{supabaseUrl}/auth/v1",
                    ValidateAudience = true,
                    ValidAudience = "authenticated",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
```

### 5.2 RLS Context Middleware

**File:** `src/backend/shared/Middleware/RlsContextMiddleware.cs`

```csharp
using System.Security.Claims;
using Npgsql;

public class RlsContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RlsContextMiddleware> _logger;

    public RlsContextMiddleware(RequestDelegate next, ILogger<RlsContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, NpgsqlConnection connection)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            // Extract user ID from JWT 'sub' claim
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? context.User.FindFirst("sub")?.Value;

            // Get role from custom claim or default
            var role = context.User.FindFirst("harvestry_role")?.Value ?? "operator";

            // Get site ID from header (user-selected site context)
            var siteId = context.Request.Headers["X-Site-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(userId))
            {
                await SetRlsContext(connection, userId, role, siteId);
            }
        }

        await _next(context);
    }

    private async Task SetRlsContext(
        NpgsqlConnection connection,
        string userId,
        string role,
        string? siteId)
    {
        await connection.OpenAsync();

        var commands = new List<string>
        {
            $"SELECT set_config('app.current_user_id', '{userId}', true)",
            $"SELECT set_config('app.user_role', '{role}', true)"
        };

        if (!string.IsNullOrEmpty(siteId))
        {
            commands.Add($"SELECT set_config('app.site_id', '{siteId}', true)");
        }

        foreach (var sql in commands)
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        _logger.LogDebug(
            "RLS context set: user={UserId}, role={Role}, site={SiteId}",
            userId, role, siteId);
    }
}
```

---

## 6. Frontend Implementation

### 6.1 Supabase Client

**File:** `src/frontend/lib/supabase/client.ts`

```typescript
import { createBrowserClient } from '@supabase/ssr';

export function createClient() {
  return createBrowserClient(
    process.env.NEXT_PUBLIC_SUPABASE_URL!,
    process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY!
  );
}
```

### 6.2 Auth Provider

**File:** `src/frontend/providers/AuthProvider.tsx`

```typescript
'use client';

import { createContext, useContext, useEffect, useState } from 'react';
import { Session, User } from '@supabase/supabase-js';
import { createClient } from '@/lib/supabase/client';
import { useAuthStore } from '@/stores/auth/authStore';

interface AuthContextType {
  session: Session | null;
  user: User | null;
  isLoading: boolean;
  signIn: (email: string, password: string) => Promise<void>;
  signUp: (email: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<Session | null>(null);
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  
  const setStoreUser = useAuthStore((state) => state.setUser);
  const supabase = createClient();

  useEffect(() => {
    // Get initial session
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session);
      setUser(session?.user ?? null);
      if (session?.user) {
        fetchUserProfile(session.user.id);
      }
      setIsLoading(false);
    });

    // Listen for auth changes
    const { data: { subscription } } = supabase.auth.onAuthStateChange(
      async (_event, session) => {
        setSession(session);
        setUser(session?.user ?? null);
        if (session?.user) {
          await fetchUserProfile(session.user.id);
        } else {
          setStoreUser(null);
        }
      }
    );

    return () => subscription.unsubscribe();
  }, []);

  const fetchUserProfile = async (userId: string) => {
    const response = await fetch(`/api/v1/users/${userId}`, {
      headers: {
        'Authorization': `Bearer ${session?.access_token}`,
      },
    });
    if (response.ok) {
      const profile = await response.json();
      setStoreUser(profile);
    }
  };

  const signIn = async (email: string, password: string) => {
    const { error } = await supabase.auth.signInWithPassword({
      email,
      password,
    });
    if (error) throw error;
  };

  const signUp = async (email: string, password: string) => {
    const { error } = await supabase.auth.signUp({
      email,
      password,
    });
    if (error) throw error;
  };

  const signOut = async () => {
    const { error } = await supabase.auth.signOut();
    if (error) throw error;
    setStoreUser(null);
  };

  return (
    <AuthContext.Provider value={{
      session,
      user,
      isLoading,
      signIn,
      signUp,
      signOut,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
```

### 6.3 API Client with Auth

**File:** `src/frontend/lib/api/client.ts`

```typescript
import { createClient } from '@/lib/supabase/client';

const API_BASE = process.env.NEXT_PUBLIC_API_URL || '';

export async function apiClient(
  endpoint: string,
  options: RequestInit = {}
): Promise<Response> {
  const supabase = createClient();
  const { data: { session } } = await supabase.auth.getSession();

  const headers = new Headers(options.headers);
  headers.set('Content-Type', 'application/json');

  if (session?.access_token) {
    headers.set('Authorization', `Bearer ${session.access_token}`);
  }

  // Get current site from store
  const { currentSiteId } = useAuthStore.getState();
  if (currentSiteId) {
    headers.set('X-Site-Id', currentSiteId);
  }

  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });

  // Handle token expiry
  if (response.status === 401) {
    const tokenExpired = response.headers.get('Token-Expired');
    if (tokenExpired === 'true') {
      // Try to refresh
      const { error } = await supabase.auth.refreshSession();
      if (!error) {
        // Retry request with new token
        return apiClient(endpoint, options);
      }
    }
    // Redirect to login
    window.location.href = '/login';
  }

  return response;
}
```

---

## 7. User Sync Mechanism

### 7.1 Webhook Handler

**File:** `src/backend/services/core-platform/identity/API/Controllers/WebhookController.cs`

```csharp
[ApiController]
[Route("api/v1/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    [HttpPost("supabase/user")]
    public async Task<IActionResult> HandleSupabaseUserWebhook(
        [FromBody] SupabaseWebhookPayload payload,
        [FromHeader(Name = "X-Supabase-Signature")] string signature)
    {
        // Verify webhook signature
        if (!VerifySignature(payload, signature))
        {
            return Unauthorized();
        }

        switch (payload.Type)
        {
            case "user.created":
                await HandleUserCreated(payload.User);
                break;
            case "user.updated":
                await HandleUserUpdated(payload.User);
                break;
            case "user.deleted":
                await HandleUserDeleted(payload.User.Id);
                break;
        }

        return Ok();
    }

    private async Task HandleUserCreated(SupabaseUser supabaseUser)
    {
        // Create user in our database
        var user = new User
        {
            Id = Guid.Parse(supabaseUser.Id),
            Email = supabaseUser.Email,
            FirstName = supabaseUser.UserMetadata?.FirstName ?? "New",
            LastName = supabaseUser.UserMetadata?.LastName ?? "User",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
        };

        await _userService.CreateUserAsync(user);
    }
}
```

---

## 8. Configuration

### 8.1 Environment Variables

**Frontend (`.env.local`):**
```env
NEXT_PUBLIC_SUPABASE_URL=https://your-project.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=your-anon-key
NEXT_PUBLIC_API_URL=https://api.harvestry.io
```

**Backend (`appsettings.json`):**
```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "JwtSecret": "your-jwt-secret",
    "WebhookSecret": "your-webhook-secret"
  }
}
```

### 8.2 Secrets Management (Production)

Store in AWS Secrets Manager:
- `harvestry/supabase/jwt-secret`
- `harvestry/supabase/webhook-secret`
- `harvestry/supabase/anon-key`

---

## 9. Security Considerations

### 9.1 Token Security

- Store access token in memory (not localStorage for sensitive apps)
- Store refresh token in HttpOnly cookie (optional)
- Never expose JWT secret client-side

### 9.2 CORS Configuration

```csharp
services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://app.harvestry.io")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### 9.3 Rate Limiting

```csharp
services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

---

## 10. Testing Strategy

### 10.1 Unit Tests

- JWT validation logic
- RLS context middleware
- User service methods

### 10.2 Integration Tests

- Full login flow
- Token refresh
- User registration webhook

### 10.3 Security Tests

- Token expiry handling
- Invalid token rejection
- Cross-tenant access attempts

---

*End of Authentication Architecture Design*


