'use client';

import { useParams } from 'next/navigation';
import { SalesOrderDetailView } from '@/features/sales/components/order-detail/SalesOrderDetailView';

export default function SalesOrderDetailPage() {
  const params = useParams<{ salesOrderId: string }>();
  return <SalesOrderDetailView salesOrderId={params.salesOrderId} />;
}

