'use client';

import { Shield, CheckCircle, Clock, AlertTriangle, FileText } from 'lucide-react';
import { ComplianceBadge } from '@/features/sales/components/shared';
import type { ComplianceStatus } from '@/features/sales/components/shared';
import type { SalesOrderDto } from '@/features/sales/types/salesOrders.types';

interface CompliancePanelProps {
  order: SalesOrderDto;
}

interface ComplianceCheck {
  id: string;
  label: string;
  status: 'pass' | 'warn' | 'fail' | 'pending';
  message?: string;
}

function getComplianceChecks(order: SalesOrderDto): ComplianceCheck[] {
  const checks: ComplianceCheck[] = [];

  // Destination license check
  if (order.destinationLicenseNumber) {
    checks.push({
      id: 'destination-license',
      label: 'Destination License',
      status: 'pass', // In real implementation, would check verification status
      message: order.destinationLicenseNumber,
    });
  } else {
    checks.push({
      id: 'destination-license',
      label: 'Destination License',
      status: order.status === 'Draft' ? 'pending' : 'fail',
      message: 'Not specified',
    });
  }

  // Lines check
  const hasLines = (order.lines?.length ?? 0) > 0;
  checks.push({
    id: 'order-lines',
    label: 'Order Lines',
    status: hasLines ? 'pass' : order.status === 'Draft' ? 'pending' : 'fail',
    message: hasLines ? `${order.lines?.length} line(s)` : 'No items added',
  });

  // Allocation check (for submitted orders)
  if (order.status !== 'Draft') {
    const totalRequested = order.lines?.reduce((sum, l) => sum + l.requestedQuantity, 0) ?? 0;
    const totalAllocated = order.lines?.reduce((sum, l) => sum + l.allocatedQuantity, 0) ?? 0;
    const fullyAllocated = totalAllocated >= totalRequested;

    checks.push({
      id: 'allocation',
      label: 'Inventory Allocated',
      status: fullyAllocated ? 'pass' : totalAllocated > 0 ? 'warn' : 'pending',
      message: `${totalAllocated} of ${totalRequested} allocated`,
    });
  }

  return checks;
}

function CheckIcon({ status }: { status: ComplianceCheck['status'] }) {
  switch (status) {
    case 'pass':
      return <CheckCircle className="w-4 h-4 text-emerald-400" />;
    case 'warn':
      return <AlertTriangle className="w-4 h-4 text-amber-400" />;
    case 'fail':
      return <AlertTriangle className="w-4 h-4 text-rose-400" />;
    case 'pending':
      return <Clock className="w-4 h-4 text-slate-400" />;
  }
}

export function CompliancePanel({ order }: CompliancePanelProps) {
  const checks = getComplianceChecks(order);
  const failedChecks = checks.filter((c) => c.status === 'fail');
  const warnChecks = checks.filter((c) => c.status === 'warn');
  const passChecks = checks.filter((c) => c.status === 'pass');

  // Determine overall status
  let overallStatus: ComplianceStatus = 'Unknown';
  if (failedChecks.length > 0) {
    overallStatus = 'Failed';
  } else if (warnChecks.length > 0) {
    overallStatus = 'Pending';
  } else if (passChecks.length === checks.length) {
    overallStatus = 'Verified';
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Shield className="w-4 h-4 text-muted-foreground" />
          <span className="text-sm font-medium text-foreground">Compliance Status</span>
        </div>
        <ComplianceBadge status={overallStatus} />
      </div>

      {/* Blocking Warning */}
      {failedChecks.length > 0 && order.status === 'Draft' && (
        <div className="p-3 rounded-lg bg-rose-500/10 border border-rose-500/30">
          <div className="flex items-start gap-2">
            <AlertTriangle className="w-4 h-4 text-rose-400 flex-shrink-0 mt-0.5" />
            <div>
              <div className="text-sm font-medium text-rose-300">Cannot Submit</div>
              <div className="text-xs text-rose-300/70">
                Resolve the following before submitting:
              </div>
              <ul className="mt-1 text-xs text-rose-300/70 list-disc list-inside">
                {failedChecks.map((c) => (
                  <li key={c.id}>{c.label}: {c.message}</li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      )}

      {/* Checks List */}
      <div className="space-y-2">
        {checks.map((check) => (
          <div
            key={check.id}
            className="flex items-center justify-between p-2 rounded-lg bg-muted/30"
          >
            <div className="flex items-center gap-2">
              <CheckIcon status={check.status} />
              <span className="text-sm text-foreground">{check.label}</span>
            </div>
            <span className="text-xs text-muted-foreground">{check.message}</span>
          </div>
        ))}
      </div>

      {/* METRC Info (if applicable) */}
      {order.status === 'Shipped' && (
        <div className="p-3 rounded-lg bg-emerald-500/10 border border-emerald-500/30">
          <div className="flex items-center gap-2">
            <FileText className="w-4 h-4 text-emerald-400" />
            <span className="text-sm text-emerald-300">Ready for METRC Transfer</span>
          </div>
          <p className="text-xs text-emerald-300/70 mt-1">
            Create a transfer to generate a METRC manifest.
          </p>
        </div>
      )}
    </div>
  );
}
