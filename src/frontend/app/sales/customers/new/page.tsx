'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Building2, Save } from 'lucide-react';
import { Card, CardHeader, DemoModeBanner } from '@/features/sales/components/shared';

interface CustomerFormData {
  name: string;
  licenseNumber: string;
  facilityName: string;
  facilityType: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  contactName: string;
  email: string;
  phone: string;
  notes: string;
}

const FACILITY_TYPES = [
  'Dispensary',
  'Cultivator',
  'Manufacturer',
  'Distributor',
  'Testing Lab',
  'Other',
];

export default function NewCustomerPage() {
  const router = useRouter();
  const [isDemoMode] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [form, setForm] = useState<CustomerFormData>({
    name: '',
    licenseNumber: '',
    facilityName: '',
    facilityType: 'Dispensary',
    address: '',
    city: '',
    state: '',
    zip: '',
    contactName: '',
    email: '',
    phone: '',
    notes: '',
  });

  function updateField(field: keyof CustomerFormData, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    // Validation
    if (!form.name.trim()) {
      setError('Customer name is required');
      return;
    }
    if (!form.licenseNumber.trim()) {
      setError('License number is required');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      // Demo mode - just simulate success
      if (isDemoMode) {
        await new Promise((resolve) => setTimeout(resolve, 1000));
        router.push('/sales/customers');
        return;
      }

      // TODO: Call API to create customer
      router.push('/sales/customers');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create customer');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="p-6 space-y-6 max-w-4xl mx-auto">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* Header */}
      <div className="flex items-center gap-4">
        <Link
          href="/sales/customers"
          className="p-2 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
        </Link>
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-violet-500/10 flex items-center justify-center">
            <Building2 className="w-5 h-5 text-violet-400" />
          </div>
          <div>
            <h1 className="text-xl font-semibold text-foreground">New Customer</h1>
            <p className="text-sm text-muted-foreground">Add a new customer account</p>
          </div>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}

      {/* Form */}
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Business Information */}
        <Card>
          <CardHeader title="Business Information" subtitle="Primary customer details" />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                Customer Name *
              </label>
              <input
                type="text"
                value={form.name}
                onChange={(e) => updateField('name', e.target.value)}
                placeholder="Green Valley Dispensary"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
                required
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                License Number *
              </label>
              <input
                type="text"
                value={form.licenseNumber}
                onChange={(e) => updateField('licenseNumber', e.target.value)}
                placeholder="LIC-2024-001"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
                required
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                Facility Type
              </label>
              <select
                value={form.facilityType}
                onChange={(e) => updateField('facilityType', e.target.value)}
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
              >
                {FACILITY_TYPES.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>
            <div className="md:col-span-2">
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                Facility Name
              </label>
              <input
                type="text"
                value={form.facilityName}
                onChange={(e) => updateField('facilityName', e.target.value)}
                placeholder="Green Valley Retail Location"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
              />
            </div>
          </div>
        </Card>

        {/* Address */}
        <Card>
          <CardHeader title="Address" />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="md:col-span-2">
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                Street Address
              </label>
              <input
                type="text"
                value={form.address}
                onChange={(e) => updateField('address', e.target.value)}
                placeholder="123 Cannabis Way"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                City
              </label>
              <input
                type="text"
                value={form.city}
                onChange={(e) => updateField('city', e.target.value)}
                placeholder="Denver"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                  State
                </label>
                <input
                  type="text"
                  value={form.state}
                  onChange={(e) => updateField('state', e.target.value)}
                  placeholder="CO"
                  className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                  ZIP
                </label>
                <input
                  type="text"
                  value={form.zip}
                  onChange={(e) => updateField('zip', e.target.value)}
                  placeholder="80202"
                  className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
                />
              </div>
            </div>
          </div>
        </Card>

        {/* Contact */}
        <Card>
          <CardHeader title="Primary Contact" />
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                Contact Name
              </label>
              <input
                type="text"
                value={form.contactName}
                onChange={(e) => updateField('contactName', e.target.value)}
                placeholder="Sarah Johnson"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                Phone
              </label>
              <input
                type="tel"
                value={form.phone}
                onChange={(e) => updateField('phone', e.target.value)}
                placeholder="(555) 123-4567"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
              />
            </div>
            <div className="md:col-span-2">
              <label className="block text-xs font-medium text-muted-foreground mb-1.5">
                Email
              </label>
              <input
                type="email"
                value={form.email}
                onChange={(e) => updateField('email', e.target.value)}
                placeholder="sarah@greenvalley.com"
                className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30"
              />
            </div>
          </div>
        </Card>

        {/* Notes */}
        <Card>
          <CardHeader title="Notes" subtitle="Optional internal notes" />
          <textarea
            value={form.notes}
            onChange={(e) => updateField('notes', e.target.value)}
            placeholder="Add any notes about this customer..."
            rows={3}
            className="w-full px-3 py-2 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-amber-500/30 resize-none"
          />
        </Card>

        {/* Actions */}
        <div className="flex items-center justify-end gap-3">
          <Link
            href="/sales/customers"
            className="px-4 py-2 rounded-lg text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
          >
            Cancel
          </Link>
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex items-center gap-2 px-4 py-2 rounded-lg bg-amber-600 hover:bg-amber-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
          >
            <Save className="w-4 h-4" />
            {isSubmitting ? 'Saving...' : 'Save Customer'}
          </button>
        </div>
      </form>
    </div>
  );
}
