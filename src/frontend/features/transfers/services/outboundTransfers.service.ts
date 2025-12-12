import { appApi } from '@/lib/api/appRouteClient';
import type {
  CreateOutboundTransferFromShipmentRequest,
  OutboundTransferDto,
  OutboundTransferListResponse,
  SubmitOutboundTransferToMetrcRequest,
  VoidOutboundTransferRequest,
} from '../types/outboundTransfers.types';

export async function listOutboundTransfers(
  siteId: string,
  options?: { page?: number; pageSize?: number; status?: string }
): Promise<OutboundTransferListResponse> {
  const params = new URLSearchParams();
  if (options?.page) params.set('page', String(options.page));
  if (options?.pageSize) params.set('pageSize', String(options.pageSize));
  if (options?.status) params.set('status', options.status);

  const qs = params.toString();
  return appApi.get<OutboundTransferListResponse>(
    `/api/v1/sites/${siteId}/transfers/outbound${qs ? `?${qs}` : ''}`
  );
}

export function getOutboundTransfer(siteId: string, transferId: string): Promise<OutboundTransferDto> {
  return appApi.get<OutboundTransferDto>(`/api/v1/sites/${siteId}/transfers/outbound/${transferId}`);
}

export function createOutboundTransferFromShipment(
  siteId: string,
  request: CreateOutboundTransferFromShipmentRequest
): Promise<OutboundTransferDto> {
  return appApi.post<OutboundTransferDto>(
    `/api/v1/sites/${siteId}/transfers/outbound/create-from-shipment`,
    request
  );
}

export function markReady(siteId: string, transferId: string): Promise<OutboundTransferDto> {
  return appApi.post<OutboundTransferDto>(
    `/api/v1/sites/${siteId}/transfers/outbound/${transferId}/ready`
  );
}

export function submitToMetrc(
  siteId: string,
  transferId: string,
  request: SubmitOutboundTransferToMetrcRequest
): Promise<OutboundTransferDto> {
  return appApi.post<OutboundTransferDto>(
    `/api/v1/sites/${siteId}/transfers/outbound/${transferId}/submit-to-metrc`,
    request
  );
}

export function voidTransfer(
  siteId: string,
  transferId: string,
  request: VoidOutboundTransferRequest
): Promise<OutboundTransferDto> {
  return appApi.post<OutboundTransferDto>(
    `/api/v1/sites/${siteId}/transfers/outbound/${transferId}/void`,
    request
  );
}

