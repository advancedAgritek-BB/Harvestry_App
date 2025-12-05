using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Harvestry.Shared.Authentication;

/// <summary>
/// Extension methods for configuring Supabase JWT authentication.
/// </summary>
public static class SupabaseJwtAuthenticationExtensions
{
    /// <summary>
    /// Adds Supabase JWT authentication to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The authentication builder for further configuration.</returns>
    public static AuthenticationBuilder AddSupabaseJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = new SupabaseSettings();
        configuration.GetSection(SupabaseSettings.SectionName).Bind(settings);
        
        // Validate settings
        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            throw new InvalidOperationException(
                $"Supabase URL is not configured. Set {SupabaseSettings.SectionName}:Url in configuration.");
        }
        
        if (string.IsNullOrWhiteSpace(settings.JwtSecret))
        {
            throw new InvalidOperationException(
                $"Supabase JWT secret is not configured. Set {SupabaseSettings.SectionName}:JwtSecret in configuration.");
        }
        
        // Register settings for dependency injection
        services.Configure<SupabaseSettings>(configuration.GetSection(SupabaseSettings.SectionName));
        
        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.GetIssuer(),
                    
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(settings.JwtSecret)),
                    
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(settings.ClockSkewSeconds),
                    
                    // Additional security settings
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerHandler>>();
                        
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers.Append("Token-Expired", "true");
                            logger.LogDebug("Token expired for request to {Path}", 
                                context.Request.Path);
                        }
                        else
                        {
                            logger.LogWarning(context.Exception,
                                "Authentication failed for request to {Path}",
                                context.Request.Path);
                        }
                        
                        return Task.CompletedTask;
                    },
                    
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerHandler>>();
                        
                        var userId = context.Principal?.FindFirst("sub")?.Value;
                        logger.LogDebug("Token validated for user {UserId}", userId);
                        
                        return Task.CompletedTask;
                    },
                    
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerHandler>>();
                        
                        logger.LogDebug("Authentication challenge for request to {Path}",
                            context.Request.Path);
                        
                        return Task.CompletedTask;
                    }
                };
            });
    }
    
    /// <summary>
    /// Adds Supabase JWT authentication with optional fallback to header-based auth for development.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="allowHeaderAuth">Whether to allow header-based authentication as fallback.</param>
    /// <returns>The authentication builder for further configuration.</returns>
    public static AuthenticationBuilder AddSupabaseJwtAuthenticationWithFallback(
        this IServiceCollection services,
        IConfiguration configuration,
        bool allowHeaderAuth = false)
    {
        var supabaseSection = configuration.GetSection(SupabaseSettings.SectionName);
        
        // If Supabase is not configured and header auth is allowed, use header auth
        if (!supabaseSection.Exists() || 
            string.IsNullOrWhiteSpace(supabaseSection["JwtSecret"]))
        {
            if (allowHeaderAuth)
            {
                return services.AddAuthentication("Header")
                    .AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>("Header", null);
            }
            
            throw new InvalidOperationException(
                "Supabase authentication is not configured and header auth is disabled.");
        }
        
        return services.AddSupabaseJwtAuthentication(configuration);
    }
}



