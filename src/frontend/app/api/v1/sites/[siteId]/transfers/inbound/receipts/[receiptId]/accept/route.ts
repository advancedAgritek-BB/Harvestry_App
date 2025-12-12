import { BackendProxy } from '@/lib/api/backendProxy';

type RouteParams = {
  params: Promise<{ siteId: string; receiptId: string }>;
};

export async function POST(request: Request, { params }: RouteParams) {
  const { siteId, receiptId } = await params;
  return new BackendProxy().proxyJson({
    request,
    backendPath: `/api/v1/sites/${siteId}/transfers/inbound/receipts/${receiptId}/accept`,
  });
}

