import { BackendProxy } from '@/lib/api/backendProxy';

type RouteParams = {
  params: Promise<{ siteId: string }>;
};

export async function POST(request: Request, { params }: RouteParams) {
  const { siteId } = await params;
  return new BackendProxy().proxyJson({
    request,
    backendPath: `/api/v1/sites/${siteId}/transfers/outbound/create-from-shipment`,
  });
}

