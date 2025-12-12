import { BackendProxy } from '@/lib/api/backendProxy';

type RouteParams = {
  params: Promise<{ siteId: string; salesOrderId: string }>;
};

export async function POST(request: Request, { params }: RouteParams) {
  const { siteId, salesOrderId } = await params;
  return new BackendProxy().proxyJson({
    request,
    backendPath: `/api/v1/sites/${siteId}/sales/orders/${salesOrderId}/lines`,
  });
}

