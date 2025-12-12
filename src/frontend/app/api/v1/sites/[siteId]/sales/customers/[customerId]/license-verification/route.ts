import { BackendProxy } from '@/lib/api/backendProxy';

type RouteParams = {
  params: Promise<{ siteId: string; customerId: string }>;
};

export async function POST(request: Request, { params }: RouteParams) {
  const { siteId, customerId } = await params;
  return new BackendProxy().proxyJson({
    request,
    backendPath: `/api/v1/sites/${siteId}/sales/customers/${customerId}/license-verification`,
  });
}
