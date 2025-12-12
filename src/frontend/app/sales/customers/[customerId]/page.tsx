'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import {
  ArrowLeft,
  Building2,
  Mail,
  Phone,
  MapPin,
  FileText,
  Package,
  Truck,
  Edit,
  Shield,
  Clock,
  CheckCircle,
} from 'lucide-react';
import {
  Card,
  CardHeader,
  DemoModeBanner,
  ComplianceBadge,
  StatusBadge,
} from '@/features/sales/components/shared';
import { usePermissions } from '@/providers/PermissionsProvider';
import type { ComplianceStatus } from '@/features/sales/components/shared';

// Demo customer data
const DEMO_CUSTOMER = {
  id: 'cust-001',
  name: 'Green Valley Dispensary',
  licenseNumber: 'LIC-2024-001',
  facilityName: 'Green Valley Retail',
  facilityType: 'Dispensary',
  address: '123 Cannabis Way',
  city: 'Denver',
  state: 'CO',
  zip: '80202',
  contactName: 'Sarah Johnson',
  email: 'sarah@greenvalley.com',
  phone: '(555) 123-4567',
  licenseVerifiedStatus: 'Verified' as ComplianceStatus,
  licenseVerifiedAt: new Date(Date.now() - 86400000 * 7).toISOString(),
  metrcRecipientId: 'METRC-REC-001',
  notes: 'Preferred customer. Weekly delivery schedule on Tuesdays.',
  isActive: true,
  createdAt: new Date(Date.now() - 86400000 * 90).toISOString(),
};

const DEMO_RECENT_ORDERS = [
  { id: 'order-001', orderNumber: 'SO-2024-042', status: 'Shipped', date: '2024-01-15', total: 12500 },
  { id: 'order-002', orderNumber: 'SO-2024-038', status: 'Shipped', date: '2024-01-08', total: 8750 },
  { id: 'order-003', orderNumber: 'SO-2024-031', status: 'Allocated', date: '2024-01-02', total: 15200 },
];

const DEMO_ACTIVITY = [
  { id: '1', type: 'order', text: 'Order SO-2024-042 shipped', timestamp: new Date(Date.now() - 3600000).toISOString() },
  { id: '2', type: 'license', text: 'License verification completed', timestamp: new Date(Date.now() - 86400000 * 7).toISOString() },
  { id: '3', type: 'order', text: 'Order SO-2024-038 created', timestamp: new Date(Date.now() - 86400000 * 14).toISOString() },
];

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

function formatRelativeTime(timestamp: string): string {
  const diff = Date.now() - new Date(timestamp).getTime();
  const days = Math.floor(diff / 86400000);
  if (days === 0) return 'Today';
  if (days === 1) return 'Yesterday';
  if (days < 7) return `${days} days ago`;
  if (days < 30) return `${Math.floor(days / 7)} weeks ago`;
  return formatDate(timestamp);
}

export default function CustomerDetailPage() {
  const params = useParams();
  const permissions = usePermissions();
  const canEdit = permissions.has('sales:customers:edit');

  const [isDemoMode] = useState(true);
  const customer = DEMO_CUSTOMER;

  return (
    <div className="p-6 space-y-6">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* Back + Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link
            href="/sales/customers"
            className="p-2 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
          </Link>
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center">
              <Building2 className="w-6 h-6 text-violet-400" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-xl font-semibold text-foreground">{customer.name}</h1>
                <ComplianceBadge status={customer.licenseVerifiedStatus} />
              </div>
              <p className="text-sm text-muted-foreground">{customer.licenseNumber}</p>
            </div>
          </div>
        </div>

        {canEdit && (
          <Link
            href={`/sales/customers/${params.customerId}/edit`}
            className="flex items-center gap-2 px-4 py-2 rounded-lg bg-muted/50 text-foreground hover:bg-muted transition-colors"
          >
            <Edit className="w-4 h-4" />
            <span className="text-sm font-medium">Edit</span>
          </Link>
        )}
      </div>

      {/* Main Grid */}
      <div className="grid grid-cols-12 gap-6">
        {/* Left Column - Details */}
        <div className="col-span-12 lg:col-span-8 space-y-6">
          {/* Contact Info */}
          <Card>
            <CardHeader title="Contact Information" />
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-muted/50 flex items-center justify-center">
                  <Building2 className="w-5 h-5 text-muted-foreground" />
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">Facility</div>
                  <div className="text-sm text-foreground">{customer.facilityName}</div>
                  <div className="text-xs text-muted-foreground">{customer.facilityType}</div>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-muted/50 flex items-center justify-center">
                  <MapPin className="w-5 h-5 text-muted-foreground" />
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">Address</div>
                  <div className="text-sm text-foreground">{customer.address}</div>
                  <div className="text-xs text-muted-foreground">
                    {customer.city}, {customer.state} {customer.zip}
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-muted/50 flex items-center justify-center">
                  <Mail className="w-5 h-5 text-muted-foreground" />
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">Email</div>
                  <div className="text-sm text-foreground">{customer.email}</div>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-muted/50 flex items-center justify-center">
                  <Phone className="w-5 h-5 text-muted-foreground" />
                </div>
                <div>
                  <div className="text-xs text-muted-foreground">Phone</div>
                  <div className="text-sm text-foreground">{customer.phone}</div>
                  <div className="text-xs text-muted-foreground">{customer.contactName}</div>
                </div>
              </div>
            </div>
            {customer.notes && (
              <div className="mt-4 p-3 rounded-lg bg-muted/30 border border-border">
                <div className="text-xs text-muted-foreground mb-1">Notes</div>
                <div className="text-sm text-foreground">{customer.notes}</div>
              </div>
            )}
          </Card>

          {/* Recent Orders */}
          <Card>
            <CardHeader
              title="Recent Orders"
              action={
                <Link
                  href={`/sales/orders?customerId=${params.customerId}`}
                  className="text-xs text-amber-400 hover:text-amber-300"
                >
                  View All →
                </Link>
              }
            />
            <div className="space-y-2">
              {DEMO_RECENT_ORDERS.map((order) => (
                <Link
                  key={order.id}
                  href={`/sales/orders/${order.id}`}
                  className="flex items-center justify-between p-3 rounded-lg hover:bg-muted/30 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <FileText className="w-4 h-4 text-blue-400" />
                    <div>
                      <div className="text-sm font-medium text-foreground">{order.orderNumber}</div>
                      <div className="text-xs text-muted-foreground">{formatDate(order.date)}</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <span className="text-sm text-muted-foreground">${order.total.toLocaleString()}</span>
                    <StatusBadge status={order.status} />
                  </div>
                </Link>
              ))}
            </div>
          </Card>
        </div>

        {/* Right Column - Compliance + Activity */}
        <div className="col-span-12 lg:col-span-4 space-y-6">
          {/* Compliance Panel */}
          <Card>
            <CardHeader title="Compliance Status" />
            <div className="space-y-4">
              <div className="flex items-center justify-between p-3 rounded-lg bg-muted/30">
                <div className="flex items-center gap-2">
                  <Shield className="w-4 h-4 text-muted-foreground" />
                  <span className="text-sm text-foreground">License Status</span>
                </div>
                <ComplianceBadge status={customer.licenseVerifiedStatus} />
              </div>
              {customer.licenseVerifiedAt && (
                <div className="flex items-center justify-between text-xs text-muted-foreground">
                  <span>Last Verified</span>
                  <span>{formatRelativeTime(customer.licenseVerifiedAt)}</span>
                </div>
              )}
              {customer.metrcRecipientId && (
                <div className="flex items-center justify-between text-xs text-muted-foreground">
                  <span>METRC Recipient ID</span>
                  <span className="font-mono">{customer.metrcRecipientId}</span>
                </div>
              )}
            </div>
          </Card>

          {/* Activity Timeline */}
          <Card>
            <CardHeader title="Recent Activity" />
            <div className="space-y-3">
              {DEMO_ACTIVITY.map((activity) => (
                <div key={activity.id} className="flex items-start gap-3">
                  <div className="w-6 h-6 rounded-full bg-muted/50 flex items-center justify-center flex-shrink-0 mt-0.5">
                    {activity.type === 'order' && <FileText className="w-3 h-3 text-blue-400" />}
                    {activity.type === 'license' && <CheckCircle className="w-3 h-3 text-emerald-400" />}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="text-sm text-foreground">{activity.text}</div>
                    <div className="text-xs text-muted-foreground">
                      {formatRelativeTime(activity.timestamp)}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
