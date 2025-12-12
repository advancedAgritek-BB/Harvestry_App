import { BackendProxy } from '@/lib/api/backendProxy';

type RouteParams = {
  params: Promise<{ siteId: string; customerId: string }>;
};

export async function GET(request: Request, { params }: RouteParams) {
  const { siteId, customerId } = await params;
  return new BackendProxy().proxyJson({
    request,
    backendPath: `/api/v1/sites/${siteId}/sales/customers/${customerId}`,
  });
}

export async function PUT(request: Request, { params }: RouteParams) {
  const { siteId, customerId } = await params;
  return new BackendProxy().proxyJson({
    request,
    backendPath: `/api/v1/sites/${siteId}/sales/customers/${customerId}`,
  });
}
