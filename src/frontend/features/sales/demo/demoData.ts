/**
 * Centralized demo data for the Sales CRM.
 * Used when backend is unavailable or in demo mode.
 */

import type { CustomerSummaryDto, CustomerDetailDto } from '../types/customers.types';
import type { SalesOrderDto } from '../types/salesOrders.types';
import type { ShipmentDto } from '../types/shipments.types';
import type { OutboundTransferDto } from '@/features/transfers/types/outboundTransfers.types';

// Consistent site ID for demo mode
export const DEMO_SITE_ID = 'demo-site-001';

// ===== CUSTOMERS =====
export const DEMO_CUSTOMERS: CustomerSummaryDto[] = [
  {
    id: 'cust-001',
    siteId: DEMO_SITE_ID,
    name: 'Green Valley Dispensary',
    licenseNumber: 'LIC-2024-001',
    facilityName: 'Green Valley Retail',
    facilityType: 'Dispensary',
    primaryContactName: 'Sarah Johnson',
    email: 'sarah@greenvalley.com',
    phone: '(555) 123-4567',
    licenseVerifiedStatus: 'Verified',
    orderCount: 24,
    isActive: true,
  },
  {
    id: 'cust-002',
    siteId: DEMO_SITE_ID,
    name: 'Mountain Top Cannabis',
    licenseNumber: 'LIC-2024-002',
    facilityName: 'Mountain Top Retail',
    facilityType: 'Dispensary',
    primaryContactName: 'Mike Chen',
    email: 'mike@mountaintop.com',
    phone: '(555) 234-5678',
    licenseVerifiedStatus: 'Verified',
    orderCount: 18,
    isActive: true,
  },
  {
    id: 'cust-003',
    siteId: DEMO_SITE_ID,
    name: 'Coastal Wellness',
    licenseNumber: 'LIC-2024-003',
    facilityName: 'Coastal Wellness Center',
    facilityType: 'Dispensary',
    primaryContactName: 'Emily Davis',
    email: 'emily@coastalwellness.com',
    phone: '(555) 345-6789',
    licenseVerifiedStatus: 'Pending',
    orderCount: 12,
    isActive: true,
  },
  {
    id: 'cust-004',
    siteId: DEMO_SITE_ID,
    name: 'Valley Green Farms',
    licenseNumber: 'LIC-2024-004',
    facilityName: 'Valley Green Processing',
    facilityType: 'Manufacturer',
    primaryContactName: 'Tom Wilson',
    email: 'tom@valleygreen.com',
    phone: '(555) 456-7890',
    licenseVerifiedStatus: 'Failed',
    orderCount: 6,
    isActive: true,
  },
];

export function getDemoCustomerDetail(customerId: string): CustomerDetailDto | null {
  const summary = DEMO_CUSTOMERS.find((c) => c.id === customerId);
  if (!summary) return null;

  return {
    id: summary.id,
    siteId: summary.siteId,
    name: summary.name,
    licenseNumber: summary.licenseNumber,
    facilityName: summary.facilityName,
    facilityType: summary.facilityType,
    address: '123 Cannabis Way',
    city: 'Denver',
    state: 'CO',
    zip: '80202',
    primaryContactName: summary.primaryContactName,
    email: summary.email,
    phone: summary.phone,
    licenseVerifiedStatus: summary.licenseVerifiedStatus,
    licenseVerifiedAt: new Date(Date.now() - 86400000 * 7).toISOString(),
    licenseVerificationSource: 'Manual verification',
    licenseVerificationNotes: null,
    metrcRecipientId: `METRC-REC-${summary.id.slice(-3)}`,
    isActive: summary.isActive,
    notes: 'Preferred customer. Weekly delivery schedule.',
    tags: 'preferred,weekly',
    createdAt: new Date(Date.now() - 86400000 * 90).toISOString(),
    updatedAt: new Date(Date.now() - 86400000 * 2).toISOString(),
  };
}

// ===== ORDERS =====
export const DEMO_ORDERS: SalesOrderDto[] = [
  {
    id: 'order-001',
    siteId: DEMO_SITE_ID,
    orderNumber: 'SO-2024-042',
    customerName: 'Green Valley Dispensary',
    destinationLicenseNumber: 'LIC-2024-001',
    destinationFacilityName: 'Green Valley Retail',
    status: 'Shipped',
    requestedShipDate: new Date(Date.now() - 86400000).toISOString().split('T')[0],
    createdAt: new Date(Date.now() - 86400000 * 3).toISOString(),
    updatedAt: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: 'order-002',
    siteId: DEMO_SITE_ID,
    orderNumber: 'SO-2024-043',
    customerName: 'Mountain Top Cannabis',
    destinationLicenseNumber: 'LIC-2024-002',
    destinationFacilityName: 'Mountain Top Retail',
    status: 'Allocated',
    requestedShipDate: new Date(Date.now() + 86400000).toISOString().split('T')[0],
    createdAt: new Date(Date.now() - 86400000 * 2).toISOString(),
    updatedAt: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: 'order-003',
    siteId: DEMO_SITE_ID,
    orderNumber: 'SO-2024-044',
    customerName: 'Coastal Wellness',
    destinationLicenseNumber: 'LIC-2024-003',
    destinationFacilityName: 'Coastal Wellness Center',
    status: 'Submitted',
    requestedShipDate: new Date(Date.now() + 86400000 * 3).toISOString().split('T')[0],
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    updatedAt: new Date(Date.now() - 3600000).toISOString(),
  },
  {
    id: 'order-004',
    siteId: DEMO_SITE_ID,
    orderNumber: 'SO-2024-045',
    customerName: 'Valley Green Farms',
    destinationLicenseNumber: 'LIC-2024-004',
    destinationFacilityName: 'Valley Green Processing',
    status: 'Draft',
    requestedShipDate: null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

// ===== SHIPMENTS =====
export const DEMO_SHIPMENTS: ShipmentDto[] = [
  {
    id: 'ship-001',
    siteId: DEMO_SITE_ID,
    salesOrderId: 'order-001',
    shipmentNumber: 'SH-2024-018',
    status: 'Shipped',
    pickingStartedAt: new Date(Date.now() - 86400000 * 2).toISOString(),
    packedAt: new Date(Date.now() - 86400000 * 2 + 3600000).toISOString(),
    shippedAt: new Date(Date.now() - 86400000).toISOString(),
    packages: [],
    createdAt: new Date(Date.now() - 86400000 * 2).toISOString(),
    updatedAt: new Date(Date.now() - 86400000).toISOString(),
  },
  {
    id: 'ship-002',
    siteId: DEMO_SITE_ID,
    salesOrderId: 'order-002',
    shipmentNumber: 'SH-2024-019',
    status: 'Packed',
    pickingStartedAt: new Date(Date.now() - 3600000).toISOString(),
    packedAt: new Date().toISOString(),
    shippedAt: null,
    packages: [],
    createdAt: new Date(Date.now() - 3600000).toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'ship-003',
    siteId: DEMO_SITE_ID,
    salesOrderId: 'order-003',
    shipmentNumber: 'SH-2024-020',
    status: 'Picking',
    pickingStartedAt: new Date().toISOString(),
    packedAt: null,
    shippedAt: null,
    packages: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

// ===== TRANSFERS =====
export const DEMO_TRANSFERS: OutboundTransferDto[] = [
  {
    id: 'transfer-001',
    siteId: DEMO_SITE_ID,
    shipmentId: 'ship-001',
    salesOrderId: 'order-001',
    destinationLicenseNumber: 'LIC-2024-001',
    destinationFacilityName: 'Green Valley Dispensary',
    status: 'Submitted',
    metrcTransferNumber: 'MT-2024-00142',
    metrcSyncStatus: 'Synced',
    plannedDepartureAt: new Date(Date.now() - 86400000).toISOString(),
    packages: [],
  },
  {
    id: 'transfer-002',
    siteId: DEMO_SITE_ID,
    shipmentId: 'ship-002',
    salesOrderId: 'order-002',
    destinationLicenseNumber: 'LIC-2024-002',
    destinationFacilityName: 'Mountain Top Cannabis',
    status: 'Ready',
    metrcSyncStatus: 'Pending',
    packages: [],
  },
];

// ===== DASHBOARD KPIs =====
export const DEMO_DASHBOARD_KPIS = {
  openOrders: 12,
  customersActive: 28,
  shipmentsThisWeek: 8,
  pendingTransfers: 3,
  metrcPending: 2,
  metrcFailed: 0,
};

export const DEMO_PIPELINE = [
  { stage: 'Draft', count: 3, color: 'bg-slate-500' },
  { stage: 'Submitted', count: 4, color: 'bg-blue-500' },
  { stage: 'Allocated', count: 3, color: 'bg-violet-500' },
  { stage: 'Shipped', count: 2, color: 'bg-emerald-500' },
];

export const DEMO_RECENT_ACTIVITY = [
  {
    id: '1',
    type: 'order' as const,
    title: 'Order SO-2024-042 submitted',
    customer: 'Green Valley Dispensary',
    timestamp: new Date(Date.now() - 1800000).toISOString(),
  },
  {
    id: '2',
    type: 'shipment' as const,
    title: 'Shipment SH-2024-018 packed',
    customer: 'Mountain Top Cannabis',
    timestamp: new Date(Date.now() - 3600000).toISOString(),
  },
  {
    id: '3',
    type: 'transfer' as const,
    title: 'Transfer submitted to METRC',
    customer: 'Coastal Wellness',
    timestamp: new Date(Date.now() - 7200000).toISOString(),
  },
  {
    id: '4',
    type: 'order' as const,
    title: 'Order SO-2024-041 created',
    customer: 'Valley Green Farms',
    timestamp: new Date(Date.now() - 14400000).toISOString(),
  },
];

// ===== COMPLIANCE SUMMARY =====
export const DEMO_COMPLIANCE_SUMMARY = {
  synced: 24,
  pending: 2,
  failed: 0,
};
