using System.Collections.Generic;
using System.Linq;
using Harvestry.Identity.Domain.ValueObjects;

namespace Harvestry.Identity.Domain.Constants;

/// <summary>
/// Defines preset permission bundles for common role types.
/// Bundles can be used as starting points when creating roles.
/// </summary>
public static class PermissionBundles
{
    // =========================================================================
    // VIEWER BUNDLES (Read-Only Access)
    // =========================================================================

    public static readonly PermissionBundle Viewer = new(
        id: "viewer",
        name: "Viewer",
        description: "Read-only access to dashboards and basic data",
        category: "Basic",
        displayOrder: 1,
        permissions: new[]
        {
            "dashboard:view",
            "cultivation:view",
            "irrigation:view",
            "inventory:view",
            "plants:view",
            "tasks:view"
        });

    public static readonly PermissionBundle ComplianceViewer = new(
        id: "compliance_viewer",
        name: "Compliance Viewer",
        description: "Read-only access to compliance and audit data",
        category: "Compliance",
        displayOrder: 2,
        permissions: new[]
        {
            "dashboard:view",
            "inventory:view",
            "plants:view",
            "harvests:view",
            "labtests:view",
            "compliance:view",
            "compliance:audit"
        });

    // =========================================================================
    // OPERATOR BUNDLES (Day-to-Day Operations)
    // =========================================================================

    public static readonly PermissionBundle CultivationOperator = new(
        id: "cultivation_operator",
        name: "Cultivation Operator",
        description: "Day-to-day cultivation and grow room operations",
        category: "Cultivation",
        displayOrder: 10,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            // Cultivation
            "cultivation:view",
            "cultivation:control",
            // Irrigation
            "irrigation:view",
            "irrigation:manual-trigger",
            // Plants
            "plants:view",
            "plants:move",
            "plants:phase",
            // Tasks
            "tasks:view",
            "tasks:complete",
            // Labor
            "labor:view",
            "labor:time-entry"
        });

    public static readonly PermissionBundle HarvestOperator = new(
        id: "harvest_operator",
        name: "Harvest Operator",
        description: "Harvest workflow and processing operations",
        category: "Cultivation",
        displayOrder: 11,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            // Plants
            "plants:view",
            // Harvests
            "harvests:view",
            "harvests:create",
            "harvests:weigh",
            "harvests:waste",
            // Inventory
            "inventory:view",
            "inventory:create",
            // Tasks
            "tasks:view",
            "tasks:complete",
            // Labor
            "labor:view",
            "labor:time-entry"
        });

    public static readonly PermissionBundle InventoryOperator = new(
        id: "inventory_operator",
        name: "Inventory Operator",
        description: "Inventory management and package handling",
        category: "Inventory",
        displayOrder: 12,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            // Inventory
            "inventory:view",
            "inventory:create",
            "inventory:adjust",
            "inventory:transfer",
            "inventory:ship",
            "inventory:receive",
            "inventory:labels",
            // Tasks
            "tasks:view",
            "tasks:complete",
            // Labor
            "labor:view",
            "labor:time-entry"
        });

    public static readonly PermissionBundle FulfillmentOperator = new(
        id: "fulfillment_operator",
        name: "Fulfillment Operator",
        description: "Sales order fulfillment: allocate, pack, and ship",
        category: "Sales",
        displayOrder: 13,
        permissions: new[]
        {
            "dashboard:view",
            "inventory:view",
            "inventory:ship",
            "sales:orders:view",
            "sales:allocate",
            "sales:shipments:create",
            "sales:shipments:pack",
            "sales:shipments:ship",
            "transfers:view",
            "transfers:create",
            "compliance:metrc-submit",
            "tasks:view",
            "tasks:complete"
        });

    // =========================================================================
    // MANAGER BUNDLES (Supervisory Access)
    // =========================================================================

    public static readonly PermissionBundle CultivationManager = new(
        id: "cultivation_manager",
        name: "Cultivation Manager",
        description: "Full cultivation management including recipes and configuration",
        category: "Management",
        displayOrder: 20,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            "dashboard:customize",
            "dashboard:export",
            // Cultivation
            "cultivation:view",
            "cultivation:control",
            "cultivation:configure",
            "cultivation:alerts",
            "cultivation:recipes",
            // Irrigation
            "irrigation:view",
            "irrigation:manual-trigger",
            "irrigation:programs",
            "irrigation:schedules",
            "irrigation:recipes",
            "irrigation:tanks",
            // Plants
            "plants:view",
            "plants:create",
            "plants:move",
            "plants:phase",
            // Harvests
            "harvests:view",
            "harvests:create",
            "harvests:weigh",
            "harvests:waste",
            "harvests:finalize",
            "harvests:processing",
            // Lab Tests
            "labtests:view",
            "labtests:submit",
            // Tasks
            "tasks:view",
            "tasks:create",
            "tasks:assign",
            "tasks:complete",
            "tasks:templates",
            // Labor
            "labor:view",
            "labor:schedule",
            "labor:time-entry",
            "labor:approve-time",
            "labor:teams",
            "labor:productivity",
            // Library
            "library:view",
            "library:environment-recipes",
            "library:fertigation-recipes",
            "library:lighting-recipes",
            // Analytics
            "analytics:view",
            "analytics:reports"
        });

    public static readonly PermissionBundle InventoryManager = new(
        id: "inventory_manager",
        name: "Inventory Manager",
        description: "Full inventory management including holds and compliance",
        category: "Management",
        displayOrder: 21,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            "dashboard:customize",
            "dashboard:export",
            // Inventory
            "inventory:view",
            "inventory:create",
            "inventory:adjust",
            "inventory:transfer",
            "inventory:destroy",
            "inventory:holds",
            "inventory:labels",
            // Plants
            "plants:view",
            // Harvests
            "harvests:view",
            // Lab Tests
            "labtests:view",
            "labtests:submit",
            "labtests:review",
            // Compliance
            "compliance:view",
            "compliance:reports",
            // Tasks
            "tasks:view",
            "tasks:create",
            "tasks:assign",
            "tasks:complete",
            // Labor
            "labor:view",
            "labor:schedule",
            "labor:approve-time",
            // Analytics
            "analytics:view",
            "analytics:reports"
        });

    // =========================================================================
    // SPECIALIST BUNDLES
    // =========================================================================

    public static readonly PermissionBundle ComplianceOfficer = new(
        id: "compliance_officer",
        name: "Compliance Officer",
        description: "Full compliance management and regulatory reporting",
        category: "Compliance",
        displayOrder: 30,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            "dashboard:export",
            // Inventory
            "inventory:view",
            "inventory:holds",
            // Plants
            "plants:view",
            // Harvests
            "harvests:view",
            // Lab Tests
            "labtests:view",
            "labtests:review",
            "labtests:certify",
            // Compliance
            "compliance:view",
            "compliance:reports",
            "compliance:metrc-sync",
            "compliance:metrc-submit",
            "compliance:audit",
            // Analytics
            "analytics:view",
            "analytics:reports",
            "analytics:export"
        });

    public static readonly PermissionBundle Technician = new(
        id: "technician",
        name: "Technician",
        description: "Equipment maintenance and sensor configuration",
        category: "Technical",
        displayOrder: 31,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            // Cultivation
            "cultivation:view",
            "cultivation:control",
            "cultivation:alerts",
            // Irrigation
            "irrigation:view",
            "irrigation:manual-trigger",
            // Tasks
            "tasks:view",
            "tasks:complete",
            // Admin (equipment only)
            "admin:equipment",
            "admin:spatial",
            // Labor
            "labor:view",
            "labor:time-entry"
        });

    public static readonly PermissionBundle LabTechnician = new(
        id: "lab_technician",
        name: "Lab Technician",
        description: "Laboratory testing and quality assurance",
        category: "Quality",
        displayOrder: 32,
        permissions: new[]
        {
            // Dashboard
            "dashboard:view",
            // Inventory
            "inventory:view",
            // Harvests
            "harvests:view",
            // Lab Tests
            "labtests:view",
            "labtests:submit",
            "labtests:review",
            "labtests:certify",
            // Compliance
            "compliance:view",
            // Tasks
            "tasks:view",
            "tasks:complete"
        });

    // =========================================================================
    // ADMINISTRATIVE BUNDLES
    // =========================================================================

    public static readonly PermissionBundle SiteAdmin = new(
        id: "site_admin",
        name: "Site Administrator",
        description: "Full site administration without system-wide settings",
        category: "Admin",
        displayOrder: 40,
        permissions: PermissionSections.GetAllPermissionKeys()
            .Where(p => !p.StartsWith("simulator:") && p != "admin:feature-flags")
            .ToArray());

    public static readonly PermissionBundle SuperAdmin = new(
        id: "super_admin",
        name: "Super Administrator",
        description: "Complete system access including all administrative functions",
        category: "Admin",
        displayOrder: 41,
        permissions: PermissionSections.GetAllPermissionKeys().ToArray());

    // =========================================================================
    // REGISTRY ACCESS
    // =========================================================================

    /// <summary>
    /// All permission bundles in display order
    /// </summary>
    public static IReadOnlyList<PermissionBundle> All { get; } = new List<PermissionBundle>
    {
        // Viewers
        Viewer,
        ComplianceViewer,
        // Operators
        CultivationOperator,
        HarvestOperator,
        InventoryOperator,
        FulfillmentOperator,
        // Managers
        CultivationManager,
        InventoryManager,
        // Specialists
        ComplianceOfficer,
        Technician,
        LabTechnician,
        // Admins
        SiteAdmin,
        SuperAdmin
    }.OrderBy(b => b.DisplayOrder).ToList().AsReadOnly();

    /// <summary>
    /// Get a bundle by its ID
    /// </summary>
    public static PermissionBundle? GetById(string bundleId) =>
        All.FirstOrDefault(b => b.Id.Equals(bundleId, System.StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Get all bundles in a specific category
    /// </summary>
    public static IEnumerable<PermissionBundle> GetByCategory(string category) =>
        All.Where(b => b.Category?.Equals(category, System.StringComparison.OrdinalIgnoreCase) == true);

    /// <summary>
    /// Get all unique bundle categories
    /// </summary>
    public static IEnumerable<string> GetCategories() =>
        All.Where(b => b.Category != null).Select(b => b.Category!).Distinct();

    /// <summary>
    /// Find bundles that match a given set of permissions
    /// </summary>
    public static IEnumerable<PermissionBundle> FindMatchingBundles(IEnumerable<string> permissions)
    {
        var permSet = new HashSet<string>(permissions, System.StringComparer.OrdinalIgnoreCase);
        return All.Where(b => b.IsFullyGrantedBy(permSet));
    }
}
