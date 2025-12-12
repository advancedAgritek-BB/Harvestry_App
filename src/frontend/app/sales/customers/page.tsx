'use client';

import { useState } from 'react';
import { Users, Plus, Search, Filter, Building2, Phone, Mail } from 'lucide-react';
import Link from 'next/link';
import {
  Card,
  CardHeader,
  EmptyState,
  DemoModeBanner,
  ComplianceBadge,
} from '@/features/sales/components/shared';
import { usePermissions } from '@/providers/PermissionsProvider';
import type { ComplianceStatus } from '@/features/sales/components/shared';

// Demo customer data
interface DemoCustomer {
  id: string;
  name: string;
  licenseNumber: string;
  facilityName: string;
  facilityType: string;
  contactName: string;
  email: string;
  phone: string;
  licenseVerifiedStatus: ComplianceStatus;
  orderCount: number;
  isActive: boolean;
}

const DEMO_CUSTOMERS: DemoCustomer[] = [
  {
    id: 'cust-001',
    name: 'Green Valley Dispensary',
    licenseNumber: 'LIC-2024-001',
    facilityName: 'Green Valley Retail',
    facilityType: 'Dispensary',
    contactName: 'Sarah Johnson',
    email: 'sarah@greenvalley.com',
    phone: '(555) 123-4567',
    licenseVerifiedStatus: 'Verified',
    orderCount: 24,
    isActive: true,
  },
  {
    id: 'cust-002',
    name: 'Mountain Top Cannabis',
    licenseNumber: 'LIC-2024-002',
    facilityName: 'Mountain Top Retail',
    facilityType: 'Dispensary',
    contactName: 'Mike Chen',
    email: 'mike@mountaintop.com',
    phone: '(555) 234-5678',
    licenseVerifiedStatus: 'Verified',
    orderCount: 18,
    isActive: true,
  },
  {
    id: 'cust-003',
    name: 'Coastal Wellness',
    licenseNumber: 'LIC-2024-003',
    facilityName: 'Coastal Wellness Center',
    facilityType: 'Dispensary',
    contactName: 'Emily Davis',
    email: 'emily@coastalwellness.com',
    phone: '(555) 345-6789',
    licenseVerifiedStatus: 'Pending',
    orderCount: 12,
    isActive: true,
  },
  {
    id: 'cust-004',
    name: 'Valley Green Farms',
    licenseNumber: 'LIC-2024-004',
    facilityName: 'Valley Green Processing',
    facilityType: 'Manufacturer',
    contactName: 'Tom Wilson',
    email: 'tom@valleygreen.com',
    phone: '(555) 456-7890',
    licenseVerifiedStatus: 'Failed',
    orderCount: 6,
    isActive: true,
  },
];

export default function CustomersPage() {
  const permissions = usePermissions();
  const canCreateCustomer = permissions.has('sales:customers:edit');

  const [isDemoMode] = useState(true);
  const [search, setSearch] = useState('');
  const [customers] = useState<DemoCustomer[]>(DEMO_CUSTOMERS);

  const filteredCustomers = customers.filter(
    (c) =>
      c.name.toLowerCase().includes(search.toLowerCase()) ||
      c.licenseNumber.toLowerCase().includes(search.toLowerCase()) ||
      c.contactName.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-6 space-y-6">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* Header Actions */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div className="flex items-center gap-3 flex-1">
          {/* Search */}
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search customers by name, license, contact..."
              className="w-full pl-10 pr-4 py-2.5 bg-muted/30 border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-amber-500/30"
            />
          </div>
          {/* Filter */}
          <button className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-muted/30 border border-border text-muted-foreground hover:text-foreground transition-colors">
            <Filter className="w-4 h-4" />
            <span className="text-sm hidden sm:inline">Filters</span>
          </button>
        </div>

        {canCreateCustomer && (
          <Link
            href="/sales/customers/new"
            className="flex items-center gap-2 px-4 py-2.5 rounded-lg bg-amber-500/10 text-amber-400 hover:bg-amber-500/20 transition-colors"
          >
            <Plus className="w-4 h-4" />
            <span className="text-sm font-medium">New Customer</span>
          </Link>
        )}
      </div>

      {/* Customer List */}
      <Card padding="none">
        {filteredCustomers.length === 0 ? (
          <EmptyState
            icon={Users}
            title="No customers found"
            description={
              search
                ? 'Try adjusting your search criteria'
                : 'Add your first customer to get started'
            }
            action={
              canCreateCustomer
                ? {
                    label: 'Add Customer',
                    onClick: () => (window.location.href = '/sales/customers/new'),
                  }
                : undefined
            }
          />
        ) : (
          <div className="divide-y divide-border">
            {filteredCustomers.map((customer) => (
              <Link
                key={customer.id}
                href={`/sales/customers/${customer.id}`}
                className="flex items-center gap-4 p-4 hover:bg-muted/30 transition-colors"
              >
                {/* Avatar / Icon */}
                <div className="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center flex-shrink-0">
                  <Building2 className="w-6 h-6 text-violet-400" />
                </div>

                {/* Customer Info */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    <span className="text-sm font-medium text-foreground truncate">
                      {customer.name}
                    </span>
                    <ComplianceBadge
                      status={customer.licenseVerifiedStatus}
                      showLabel={false}
                    />
                  </div>
                  <div className="flex items-center gap-4 text-xs text-muted-foreground">
                    <span>{customer.licenseNumber}</span>
                    <span>•</span>
                    <span>{customer.facilityType}</span>
                    <span>•</span>
                    <span>{customer.orderCount} orders</span>
                  </div>
                </div>

                {/* Contact Info */}
                <div className="hidden lg:flex flex-col gap-1 text-xs text-muted-foreground">
                  <div className="flex items-center gap-1.5">
                    <Users className="w-3 h-3" />
                    {customer.contactName}
                  </div>
                  <div className="flex items-center gap-1.5">
                    <Mail className="w-3 h-3" />
                    {customer.email}
                  </div>
                </div>

                {/* Status */}
                <div className="hidden sm:flex items-center">
                  <span
                    className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                      customer.isActive
                        ? 'bg-emerald-500/10 text-emerald-400'
                        : 'bg-slate-500/10 text-slate-400'
                    }`}
                  >
                    {customer.isActive ? 'Active' : 'Inactive'}
                  </span>
                </div>
              </Link>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}
