'use client';

import { useEffect, useMemo, useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { useAuthStore } from '@/stores/auth/authStore';
import { getManifest, upsertManifest } from '@/features/transfers/services/manifests.service';
import type { UpsertTransportManifestRequest } from '@/features/transfers/types/transportManifest.types';

export default function ManifestEditorPage() {
  const router = useRouter();
  const params = useParams<{ transferId: string }>();
  const transferId = params.transferId;
  const siteId = useAuthStore((s) => s.currentSiteId);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<UpsertTransportManifestRequest>({
    transporterName: '',
    transporterLicenseNumber: '',
    driverName: '',
    driverLicenseNumber: '',
    driverPhone: '',
    vehicleMake: '',
    vehicleModel: '',
    vehiclePlate: '',
    departureAt: '',
    arrivalAt: '',
  });

  const refreshKey = useMemo(() => `${siteId ?? 'none'}|${transferId}`, [siteId, transferId]);

  useEffect(() => {
    if (!siteId) return;
    let cancelled = false;
    setIsLoading(true);
    setError(null);
    getManifest(siteId, transferId)
      .then((m) => {
        if (cancelled) return;
        setForm({
          transporterName: m.transporterName ?? '',
          transporterLicenseNumber: m.transporterLicenseNumber ?? '',
          driverName: m.driverName ?? '',
          driverLicenseNumber: m.driverLicenseNumber ?? '',
          driverPhone: m.driverPhone ?? '',
          vehicleMake: m.vehicleMake ?? '',
          vehicleModel: m.vehicleModel ?? '',
          vehiclePlate: m.vehiclePlate ?? '',
          departureAt: m.departureAt ?? '',
          arrivalAt: m.arrivalAt ?? '',
        });
      })
      .catch((e) => {
        if (cancelled) return;
        setError(e instanceof Error ? e.message : 'Failed to load manifest');
      })
      .finally(() => {
        if (cancelled) return;
        setIsLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [refreshKey, siteId, transferId]);

  async function onSave() {
    if (!siteId) return;
    setIsLoading(true);
    setError(null);
    try {
      await upsertManifest(siteId, transferId, {
        transporterName: form.transporterName?.trim() || undefined,
        transporterLicenseNumber: form.transporterLicenseNumber?.trim() || undefined,
        driverName: form.driverName?.trim() || undefined,
        driverLicenseNumber: form.driverLicenseNumber?.trim() || undefined,
        driverPhone: form.driverPhone?.trim() || undefined,
        vehicleMake: form.vehicleMake?.trim() || undefined,
        vehicleModel: form.vehicleModel?.trim() || undefined,
        vehiclePlate: form.vehiclePlate?.trim() || undefined,
        departureAt: form.departureAt?.trim() || undefined,
        arrivalAt: form.arrivalAt?.trim() || undefined,
      });
      router.push(`/transfers/outbound/${transferId}`);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to save manifest');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="p-6 space-y-4 max-w-4xl">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-foreground">Manifest Editor</h1>
          <p className="text-sm text-muted-foreground">Transfer: {transferId}</p>
        </div>
        <button
          type="button"
          onClick={() => router.push(`/transfers/outbound/${transferId}`)}
          className="inline-flex items-center h-10 px-4 rounded-lg bg-muted hover:bg-muted/80 text-foreground text-sm font-medium transition-colors"
        >
          Back
        </button>
      </div>

      {error && (
        <div className="bg-rose-500/10 border border-rose-500/30 rounded-xl p-3 text-sm text-rose-200">
          {error}
        </div>
      )}

      <div className="bg-surface border border-border rounded-xl p-4 space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <TextField label="Transporter Name" value={form.transporterName ?? ''} onChange={(v) => setForm((p) => ({ ...p, transporterName: v }))} />
          <TextField label="Transporter License #" value={form.transporterLicenseNumber ?? ''} onChange={(v) => setForm((p) => ({ ...p, transporterLicenseNumber: v }))} />
          <TextField label="Driver Name" value={form.driverName ?? ''} onChange={(v) => setForm((p) => ({ ...p, driverName: v }))} />
          <TextField label="Driver License #" value={form.driverLicenseNumber ?? ''} onChange={(v) => setForm((p) => ({ ...p, driverLicenseNumber: v }))} />
          <TextField label="Driver Phone" value={form.driverPhone ?? ''} onChange={(v) => setForm((p) => ({ ...p, driverPhone: v }))} />
          <TextField label="Vehicle Make" value={form.vehicleMake ?? ''} onChange={(v) => setForm((p) => ({ ...p, vehicleMake: v }))} />
          <TextField label="Vehicle Model" value={form.vehicleModel ?? ''} onChange={(v) => setForm((p) => ({ ...p, vehicleModel: v }))} />
          <TextField label="Vehicle Plate" value={form.vehiclePlate ?? ''} onChange={(v) => setForm((p) => ({ ...p, vehiclePlate: v }))} />
          <TextField label="DepartureAt (ISO, optional)" value={form.departureAt ?? ''} onChange={(v) => setForm((p) => ({ ...p, departureAt: v }))} />
          <TextField label="ArrivalAt (ISO, optional)" value={form.arrivalAt ?? ''} onChange={(v) => setForm((p) => ({ ...p, arrivalAt: v }))} />
        </div>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={onSave}
            disabled={isLoading}
            className="inline-flex items-center h-10 px-4 rounded-lg bg-violet-600 hover:bg-violet-500 disabled:opacity-60 text-white text-sm font-medium transition-colors"
          >
            {isLoading ? 'Saving…' : 'Save Manifest'}
          </button>
        </div>
      </div>
    </div>
  );
}

function TextField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="space-y-1.5">
      <div className="text-xs font-medium text-muted-foreground">{label}</div>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30"
      />
    </div>
  );
}

