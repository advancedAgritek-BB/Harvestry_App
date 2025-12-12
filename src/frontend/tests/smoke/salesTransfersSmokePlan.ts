/**
 * Sales CRM + Transfers smoke test plan.
 * 
 * These are manual/E2E test scenarios to verify the full workflow.
 * Run through each scenario to ensure the demo workflow works end-to-end.
 */
export const salesTransfersSmokePlan = [
  // CRM Navigation
  'CRM Shell: Navigate to Sales → verify Dashboard tab is active and shows KPIs.',
  'CRM Shell: Navigate through all tabs (Dashboard, Customers, Orders, Shipments, Transfers, Reports) → verify each loads without error.',
  'CRM Shell: Verify sidebar Sales link navigates to /sales/dashboard.',
  
  // Customer Management
  'Customers: Navigate to Customers tab → verify customer list displays with license badges.',
  'Customers: Click "New Customer" → verify form loads with required fields (name, license).',
  'Customers: Submit new customer form → verify customer appears in list.',
  'Customers: Click customer row → verify detail page shows compliance panel and contact info.',
  
  // Order Workflow
  'Orders: Navigate to Orders tab → verify order list displays with status badges and "Next Action" column.',
  'Orders: Click "New Order" → verify form loads.',
  'Orders: Create draft order → add lines → verify lines appear in table.',
  'Orders: Set destination license → verify compliance panel shows "pass" for destination license.',
  'Orders: Submit order → verify status changes to "Submitted" and "Next Action" shows "Allocate".',
  'Orders: Allocate inventory → verify status changes to "Allocated".',
  
  // Shipment Workflow
  'Shipments: Navigate to Shipments tab → verify shipment list displays.',
  'Shipments: Create shipment from allocated order → verify shipment appears in list.',
  'Shipments: Start picking → scan packages → pack → verify status progression.',
  'Shipments: Ship → verify "Ready for Transfer" action becomes available.',
  
  // Transfer Workflow
  'Transfers: Navigate to Transfers tab → verify transfer list displays with METRC sync status.',
  'Transfers: Create transfer from packed shipment → verify transfer appears.',
  'Transfers: Submit to METRC → verify sync status changes.',
  'Transfers: Verify compliance summary strip shows correct counts.',
  
  // Inbound (existing)
  'Inbound: Create receipt draft → accept/reject.',
  
  // Scanner (existing)
  'Scanner: Scan wrong label → confirm error shown.',
  'Scanner: Scan same label twice → verify idempotent behavior.',
  'Scanner: Pack with override reason.',
  
  // Compliance Features
  'Compliance: Order with missing destination license → verify blocking message in compliance panel.',
  'Compliance: Customer with "Failed" license status → verify warning badge displays.',
  'Compliance: Transfer with "Failed" METRC sync → verify alert in compliance summary.',
  
  // Demo Mode
  'Demo Mode: With backend unavailable → verify demo mode banner appears.',
  'Demo Mode: Verify demo data displays consistently across all tabs.',
  'Demo Mode: Verify "Create" actions are disabled/marked as demo.',
  
  // Reports
  'Reports: Navigate to Reports tab → verify report cards display.',
  'Reports: Click "Generate Report" → verify placeholder behavior.',
  
  // Permissions
  'Permissions: As Viewer role → verify "New Order" / "New Customer" buttons are hidden.',
  'Permissions: As Admin role → verify all actions are available.',
];

/**
 * Component test scenarios for compliance indicators.
 */
export const complianceComponentTests = [
  // StatusBadge
  'StatusBadge: Renders correct color for each status (Draft, Submitted, Allocated, Shipped, Cancelled).',
  'StatusBadge: Shows icon for each status type.',
  
  // ComplianceBadge
  'ComplianceBadge: Renders "Verified" state with shield-check icon in green.',
  'ComplianceBadge: Renders "Pending" state with shield-alert icon in amber.',
  'ComplianceBadge: Renders "Failed" state with shield-x icon in red.',
  'ComplianceBadge: Renders "Unknown" state with shield icon in gray.',
  
  // MetrcBadge
  'MetrcBadge: Renders "Synced" state in green.',
  'MetrcBadge: Renders "Pending" state in amber.',
  'MetrcBadge: Renders "Failed" state in red.',
  'MetrcBadge: Renders null/NotRequired state as dash.',
  
  // CompliancePanel
  'CompliancePanel: Shows destination license check as pass when license is set.',
  'CompliancePanel: Shows order lines check as pending when no lines.',
  'CompliancePanel: Shows allocation check after order is submitted.',
  'CompliancePanel: Shows blocking message when required fields are missing.',
  'CompliancePanel: Overall status reflects worst check status.',
];
