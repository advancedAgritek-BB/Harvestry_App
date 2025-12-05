using System.Security.Claims;

namespace Harvestry.Shared.Authentication;

/// <summary>
/// Extension methods for working with claims from Supabase JWTs.
/// </summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// Gets the user ID from the claims principal.
    /// Looks for the 'sub' claim (Supabase) or NameIdentifier claim.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirst("sub")?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
    
    /// <summary>
    /// Gets the user's email from the claims principal.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("email")?.Value
            ?? principal.FindFirst(ClaimTypes.Email)?.Value;
    }
    
    /// <summary>
    /// Gets the user's role from the claims principal.
    /// Looks for the 'harvestry_role' custom claim or standard role claim.
    /// </summary>
    public static string GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("harvestry_role")?.Value
            ?? principal.FindFirst(ClaimTypes.Role)?.Value
            ?? principal.FindFirst("role")?.Value
            ?? "operator";
    }
    
    /// <summary>
    /// Gets the Supabase session ID from the claims principal.
    /// </summary>
    public static string? GetSessionId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("session_id")?.Value;
    }
    
    /// <summary>
    /// Gets the authentication level from the claims principal.
    /// </summary>
    public static string? GetAuthenticationLevel(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("aal")?.Value;
    }
    
    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
    {
        var userRole = principal.GetRole();
        return string.Equals(userRole, role, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Checks if the user has any of the specified roles.
    /// </summary>
    public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
    {
        var userRole = principal.GetRole();
        return roles.Any(r => string.Equals(r, userRole, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Checks if the user is an admin (admin or service_account role).
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.HasAnyRole("admin", "service_account");
    }
    
    /// <summary>
    /// Checks if the user is a manager or higher.
    /// </summary>
    public static bool IsManager(this ClaimsPrincipal principal)
    {
        return principal.HasAnyRole("admin", "service_account", "manager");
    }
}


