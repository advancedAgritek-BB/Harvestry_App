import { appApi } from '@/lib/api/appRouteClient';
import type {
  AcceptInboundReceiptRequest,
  CreateInboundReceiptRequest,
  InboundReceiptDto,
  InboundReceiptListResponse,
  RejectInboundReceiptRequest,
} from '../types/inboundReceipts.types';

export async function listInboundReceipts(
  siteId: string,
  options?: { page?: number; pageSize?: number; status?: string }
): Promise<InboundReceiptListResponse> {
  const params = new URLSearchParams();
  if (options?.page) params.set('page', String(options.page));
  if (options?.pageSize) params.set('pageSize', String(options.pageSize));
  if (options?.status) params.set('status', options.status);
  const qs = params.toString();

  return appApi.get<InboundReceiptListResponse>(
    `/api/v1/sites/${siteId}/transfers/inbound/receipts${qs ? `?${qs}` : ''}`
  );
}

export function getInboundReceipt(siteId: string, receiptId: string): Promise<InboundReceiptDto> {
  return appApi.get<InboundReceiptDto>(
    `/api/v1/sites/${siteId}/transfers/inbound/receipts/${receiptId}`
  );
}

export function createInboundReceiptDraft(
  siteId: string,
  request: CreateInboundReceiptRequest
): Promise<InboundReceiptDto> {
  return appApi.post<InboundReceiptDto>(
    `/api/v1/sites/${siteId}/transfers/inbound/receipts`,
    request
  );
}

export function acceptInboundReceipt(
  siteId: string,
  receiptId: string,
  request: AcceptInboundReceiptRequest
): Promise<InboundReceiptDto> {
  return appApi.post<InboundReceiptDto>(
    `/api/v1/sites/${siteId}/transfers/inbound/receipts/${receiptId}/accept`,
    request
  );
}

export function rejectInboundReceipt(
  siteId: string,
  receiptId: string,
  request: RejectInboundReceiptRequest
): Promise<InboundReceiptDto> {
  return appApi.post<InboundReceiptDto>(
    `/api/v1/sites/${siteId}/transfers/inbound/receipts/${receiptId}/reject`,
    request
  );
}

