using System.Collections.Generic;
using System.Linq;
using Harvestry.Identity.Domain.ValueObjects;

namespace Harvestry.Identity.Domain.Constants;

/// <summary>
/// Defines all available permission sections and their granular permissions.
/// Organized by functional areas of the application.
/// </summary>
public static class PermissionSections
{
    // =========================================================================
    // SECTION DEFINITIONS
    // =========================================================================

    public static readonly PermissionSection Dashboard = new(
        id: "dashboard",
        label: "Dashboard & Overview",
        description: "Access to dashboards, widgets, and data visualization",
        displayOrder: 1,
        permissions: new[]
        {
            new PermissionDefinition("dashboard:view", "View Dashboard", "Access main dashboard and overview screens"),
            new PermissionDefinition("dashboard:customize", "Customize Widgets", "Add, remove, and rearrange dashboard widgets"),
            new PermissionDefinition("dashboard:export", "Export Dashboard Data", "Export dashboard data to CSV/PDF")
        });

    public static readonly PermissionSection Cultivation = new(
        id: "cultivation",
        label: "Cultivation & Environment",
        description: "Environmental monitoring and control for grow rooms",
        displayOrder: 2,
        permissions: new[]
        {
            new PermissionDefinition("cultivation:view", "View Cultivation Data", "View environmental readings and status"),
            new PermissionDefinition("cultivation:control", "Control Environment", "Manually override environmental controls"),
            new PermissionDefinition("cultivation:configure", "Configure Setpoints", "Set target values for temperature, humidity, CO2, etc."),
            new PermissionDefinition("cultivation:alerts", "Manage Alerts", "Configure alert thresholds and notifications"),
            new PermissionDefinition("cultivation:recipes", "Manage Environment Recipes", "Create and edit environment recipe profiles")
        });

    public static readonly PermissionSection Irrigation = new(
        id: "irrigation",
        label: "Irrigation & Fertigation",
        description: "Watering schedules, nutrient delivery, and fertigation management",
        displayOrder: 3,
        permissions: new[]
        {
            new PermissionDefinition("irrigation:view", "View Irrigation Data", "View irrigation schedules and history"),
            new PermissionDefinition("irrigation:manual-trigger", "Manual Irrigation", "Manually trigger irrigation events"),
            new PermissionDefinition("irrigation:programs", "Manage Programs", "Create and edit irrigation programs"),
            new PermissionDefinition("irrigation:schedules", "Manage Schedules", "Configure irrigation schedules and timing"),
            new PermissionDefinition("irrigation:recipes", "Manage Feed Recipes", "Create and edit nutrient recipes"),
            new PermissionDefinition("irrigation:tanks", "Manage Mix Tanks", "Configure mix tanks and stock solutions")
        });

    public static readonly PermissionSection Inventory = new(
        id: "inventory",
        label: "Inventory & Packages",
        description: "Package tracking, inventory management, and movements",
        displayOrder: 4,
        permissions: new[]
        {
            new PermissionDefinition("inventory:view", "View Inventory", "View packages, lots, and inventory levels"),
            new PermissionDefinition("inventory:create", "Create Packages", "Create new packages and lots"),
            new PermissionDefinition("inventory:adjust", "Adjust Inventory", "Make inventory adjustments and corrections"),
            new PermissionDefinition("inventory:transfer", "Transfer Inventory", "Move inventory between locations"),
            new PermissionDefinition("inventory:destroy", "Destroy Inventory", "Destroy packages (compliance tracked)", 
                requiresTwoPersonApproval: true, requiresReason: true),
            new PermissionDefinition("inventory:holds", "Manage Holds", "Place and release inventory holds"),
            new PermissionDefinition("inventory:labels", "Print Labels", "Generate and print package labels"),
            new PermissionDefinition("inventory:ship", "Ship Inventory", "Confirm shipments and decrement inventory"),
            new PermissionDefinition("inventory:receive", "Receive Inventory", "Receive inbound transfers and inventory")
        });

    public static readonly PermissionSection Sales = new(
        id: "sales",
        label: "Sales & Fulfillment",
        description: "Sales orders, allocation, picking, packing, and shipping",
        displayOrder: 5,
        permissions: new[]
        {
            new PermissionDefinition("sales:orders:view", "View Sales Orders", "View sales orders and related documents"),
            new PermissionDefinition("sales:orders:create", "Create Sales Orders", "Create and edit sales orders"),
            new PermissionDefinition("sales:orders:submit", "Submit Sales Orders", "Submit sales orders for fulfillment"),
            new PermissionDefinition("sales:orders:cancel", "Cancel Sales Orders", "Cancel sales orders",
                requiresReason: true),
            new PermissionDefinition("sales:allocate", "Allocate Inventory", "Allocate/reserve packages to sales orders"),
            new PermissionDefinition("sales:shipments:create", "Create Shipments", "Create pick/pack shipments from allocations"),
            new PermissionDefinition("sales:shipments:pack", "Pack Shipments", "Confirm packing of shipments"),
            new PermissionDefinition("sales:shipments:ship", "Ship Shipments", "Confirm shipment shipment and generate inventory movements")
        });

    public static readonly PermissionSection Transfers = new(
        id: "transfers",
        label: "Transfers & Manifests",
        description: "Outbound transfers, transport manifests, and inbound receipts",
        displayOrder: 6,
        permissions: new[]
        {
            new PermissionDefinition("transfers:view", "View Transfers", "View outbound/inbound transfer records"),
            new PermissionDefinition("transfers:create", "Create Transfers", "Create outbound transfers and manifests"),
            new PermissionDefinition("transfers:submit", "Submit Transfers", "Submit transfer templates to METRC",
                requiresReason: true),
            new PermissionDefinition("transfers:void", "Void Transfers", "Void/cancel transfers",
                requiresReason: true),
            new PermissionDefinition("transfers:receive", "Receive Transfers", "Accept/reject inbound transfers and record receipts",
                requiresReason: true)
        });

    public static readonly PermissionSection Plants = new(
        id: "plants",
        label: "Plants & Batches",
        description: "Plant lifecycle management from clone to harvest",
        displayOrder: 7,
        permissions: new[]
        {
            new PermissionDefinition("plants:view", "View Plants", "View plant records and batch information"),
            new PermissionDefinition("plants:create", "Create Plants", "Create new plant records and batches"),
            new PermissionDefinition("plants:move", "Move Plants", "Move plants between rooms and locations"),
            new PermissionDefinition("plants:phase", "Change Growth Phase", "Transition plants between growth phases"),
            new PermissionDefinition("plants:destroy", "Destroy Plants", "Record plant destruction (compliance tracked)",
                requiresTwoPersonApproval: true, requiresReason: true)
        });

    public static readonly PermissionSection Harvests = new(
        id: "harvests",
        label: "Harvests & Processing",
        description: "Harvest workflows, weighing, and processing",
        displayOrder: 8,
        permissions: new[]
        {
            new PermissionDefinition("harvests:view", "View Harvests", "View harvest records and history"),
            new PermissionDefinition("harvests:create", "Create Harvests", "Initiate new harvest batches"),
            new PermissionDefinition("harvests:weigh", "Record Weights", "Record wet and dry weights"),
            new PermissionDefinition("harvests:waste", "Record Waste", "Record harvest waste and byproducts"),
            new PermissionDefinition("harvests:finalize", "Finalize Harvests", "Complete and lock harvest records"),
            new PermissionDefinition("harvests:processing", "Manage Processing", "Create and manage processing jobs")
        });

    public static readonly PermissionSection LabTesting = new(
        id: "labtests",
        label: "Lab Testing & QA",
        description: "Laboratory testing, quality assurance, and certifications",
        displayOrder: 9,
        permissions: new[]
        {
            new PermissionDefinition("labtests:view", "View Lab Results", "View test results and certificates"),
            new PermissionDefinition("labtests:submit", "Submit for Testing", "Submit samples to labs for testing"),
            new PermissionDefinition("labtests:review", "Review Results", "Review and approve test results"),
            new PermissionDefinition("labtests:certify", "Certify Batches", "Mark batches as lab-certified for sale")
        });

    public static readonly PermissionSection Compliance = new(
        id: "compliance",
        label: "Compliance & Reporting",
        description: "Regulatory compliance, METRC integration, and audit trails",
        displayOrder: 10,
        permissions: new[]
        {
            new PermissionDefinition("compliance:view", "View Compliance Data", "View compliance status and reports"),
            new PermissionDefinition("compliance:reports", "Generate Reports", "Create compliance and regulatory reports"),
            new PermissionDefinition("compliance:metrc-sync", "Sync with METRC", "Manually trigger METRC synchronization"),
            new PermissionDefinition("compliance:metrc-submit", "Submit to METRC", "Submit data to METRC/state system",
                requiresReason: true),
            new PermissionDefinition("compliance:audit", "Access Audit Logs", "View detailed audit trail and history")
        });

    public static readonly PermissionSection Tasks = new(
        id: "tasks",
        label: "Tasks & SOPs",
        description: "Task management, workflows, and standard operating procedures",
        displayOrder: 11,
        permissions: new[]
        {
            new PermissionDefinition("tasks:view", "View Tasks", "View assigned and available tasks"),
            new PermissionDefinition("tasks:create", "Create Tasks", "Create new tasks and work orders"),
            new PermissionDefinition("tasks:assign", "Assign Tasks", "Assign tasks to team members"),
            new PermissionDefinition("tasks:complete", "Complete Tasks", "Mark tasks as completed"),
            new PermissionDefinition("tasks:templates", "Manage Templates", "Create and edit task templates"),
            new PermissionDefinition("tasks:sops", "Manage SOPs", "Create and edit standard operating procedures"),
            new PermissionDefinition("tasks:blueprints", "Manage Blueprints", "Create and edit task blueprints")
        });

    public static readonly PermissionSection Labor = new(
        id: "labor",
        label: "Labor & Scheduling",
        description: "Employee scheduling, time tracking, and team management",
        displayOrder: 12,
        permissions: new[]
        {
            new PermissionDefinition("labor:view", "View Schedules", "View shift schedules and assignments"),
            new PermissionDefinition("labor:schedule", "Manage Schedules", "Create and edit shift schedules"),
            new PermissionDefinition("labor:time-entry", "Enter Time", "Submit time entries and clock in/out"),
            new PermissionDefinition("labor:approve-time", "Approve Time", "Approve or reject time entries"),
            new PermissionDefinition("labor:teams", "Manage Teams", "Create and manage team assignments"),
            new PermissionDefinition("labor:productivity", "View Productivity", "Access productivity metrics and reports")
        });

    public static readonly PermissionSection Library = new(
        id: "library",
        label: "Library & Genetics",
        description: "Strain library, genetics, and recipe management",
        displayOrder: 13,
        permissions: new[]
        {
            new PermissionDefinition("library:view", "View Library", "View strains, genetics, and recipes"),
            new PermissionDefinition("library:genetics", "Manage Genetics", "Create and edit genetic profiles"),
            new PermissionDefinition("library:environment-recipes", "Manage Environment Recipes", "Create environment recipes"),
            new PermissionDefinition("library:fertigation-recipes", "Manage Fertigation Recipes", "Create fertigation recipes"),
            new PermissionDefinition("library:lighting-recipes", "Manage Lighting Recipes", "Create lighting schedules")
        });

    public static readonly PermissionSection Analytics = new(
        id: "analytics",
        label: "Analytics & Reports",
        description: "Business intelligence, custom reports, and data analysis",
        displayOrder: 14,
        permissions: new[]
        {
            new PermissionDefinition("analytics:view", "View Analytics", "Access analytics dashboards"),
            new PermissionDefinition("analytics:reports", "Create Reports", "Build custom reports"),
            new PermissionDefinition("analytics:share", "Share Reports", "Share reports with other users"),
            new PermissionDefinition("analytics:export", "Export Data", "Export analytics data")
        });

    public static readonly PermissionSection Admin = new(
        id: "admin",
        label: "Administration",
        description: "System administration, user management, and configuration",
        displayOrder: 15,
        permissions: new[]
        {
            new PermissionDefinition("admin:users", "Manage Users", "Create, edit, and deactivate users"),
            new PermissionDefinition("admin:roles", "Manage Roles", "Create and edit role definitions"),
            new PermissionDefinition("admin:sites", "Manage Sites", "Configure site settings and access"),
            new PermissionDefinition("admin:equipment", "Manage Equipment", "Configure sensors and equipment"),
            new PermissionDefinition("admin:spatial", "Manage Spatial", "Configure rooms, zones, and locations"),
            new PermissionDefinition("admin:settings", "System Settings", "Modify system-wide settings"),
            new PermissionDefinition("admin:integrations", "Manage Integrations", "Configure third-party integrations"),
            new PermissionDefinition("admin:feature-flags", "Manage Feature Flags", "Enable/disable feature flags")
        });

    public static readonly PermissionSection Simulator = new(
        id: "simulator",
        label: "Simulator & Dev Tools",
        description: "Development tools and simulation environment",
        displayOrder: 16,
        permissions: new[]
        {
            new PermissionDefinition("simulator:access", "Access Simulator", "Access the simulation environment"),
            new PermissionDefinition("simulator:configure", "Configure Simulator", "Modify simulator settings and streams"),
            new PermissionDefinition("simulator:provision", "Provision Sites", "Create simulated sites and data")
        });

    // =========================================================================
    // REGISTRY ACCESS
    // =========================================================================

    /// <summary>
    /// All permission sections in display order
    /// </summary>
    public static IReadOnlyList<PermissionSection> All { get; } = new List<PermissionSection>
    {
        Dashboard,
        Cultivation,
        Irrigation,
        Inventory,
        Sales,
        Plants,
        Harvests,
        LabTesting,
        Compliance,
        Transfers,
        Tasks,
        Labor,
        Library,
        Analytics,
        Admin,
        Simulator
    }.OrderBy(s => s.DisplayOrder).ToList().AsReadOnly();

    /// <summary>
    /// Get a section by its ID
    /// </summary>
    public static PermissionSection? GetById(string sectionId) =>
        All.FirstOrDefault(s => s.Id.Equals(sectionId, System.StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Get all permission keys across all sections
    /// </summary>
    public static IEnumerable<string> GetAllPermissionKeys() =>
        All.SelectMany(s => s.GetPermissionKeys());

    /// <summary>
    /// Find the section containing a specific permission key
    /// </summary>
    public static PermissionSection? FindSectionForPermission(string permissionKey) =>
        All.FirstOrDefault(s => s.ContainsPermission(permissionKey));

    /// <summary>
    /// Check if a permission key is valid (exists in any section)
    /// </summary>
    public static bool IsValidPermission(string permissionKey) =>
        All.Any(s => s.ContainsPermission(permissionKey));
}
