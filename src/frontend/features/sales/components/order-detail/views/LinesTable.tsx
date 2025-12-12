'use client';

import type { SalesOrderLineDto } from '@/features/sales/types/salesOrders.types';

export function LinesTable({ lines }: { lines: SalesOrderLineDto[] }) {
  if (!lines || lines.length === 0) {
    return <div className="text-sm text-muted-foreground">No lines yet.</div>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead className="text-xs text-muted-foreground">
          <tr className="border-b border-border">
            <th className="text-left py-2 pr-3">#</th>
            <th className="text-left py-2 pr-3">Item</th>
            <th className="text-left py-2 pr-3">Requested</th>
            <th className="text-left py-2 pr-3">Allocated</th>
            <th className="text-left py-2 pr-3">Shipped</th>
          </tr>
        </thead>
        <tbody>
          {lines.map((l) => (
            <tr key={l.id} className="border-b border-border/60">
              <td className="py-2 pr-3">{l.lineNumber}</td>
              <td className="py-2 pr-3">
                <div className="font-medium">{l.itemName}</div>
                <div className="text-xs text-muted-foreground">{l.itemId}</div>
              </td>
              <td className="py-2 pr-3 text-muted-foreground">
                {l.requestedQuantity} {l.unitOfMeasure}
              </td>
              <td className="py-2 pr-3 text-muted-foreground">
                {l.allocatedQuantity} {l.unitOfMeasure}
              </td>
              <td className="py-2 pr-3 text-muted-foreground">
                {l.shippedQuantity} {l.unitOfMeasure}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

