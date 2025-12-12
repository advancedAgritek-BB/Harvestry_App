import { BackendProxy } from '@/lib/api/backendProxy';

type RouteParams = {
  params: Promise<{ siteId: string; shipmentId: string }>;
};

export async function POST(request: Request, { params }: RouteParams) {
  const { siteId, shipmentId } = await params;
  return new BackendProxy().proxyJson({
    request,
    backendPath: `/api/v1/sites/${siteId}/sales/shipments/${shipmentId}/ship`,
  });
}

