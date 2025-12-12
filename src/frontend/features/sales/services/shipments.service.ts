import { appApi } from '@/lib/api/appRouteClient';
import type {
  CancelShipmentRequest,
  CreateShipmentRequest,
  MarkShipmentShippedRequest,
  ShipmentDto,
  ShipmentListResponse,
} from '../types/shipments.types';

export async function listShipments(
  siteId: string,
  options?: {
    page?: number;
    pageSize?: number;
    status?: string;
    salesOrderId?: string;
  }
): Promise<ShipmentListResponse> {
  const params = new URLSearchParams();
  if (options?.page) params.set('page', String(options.page));
  if (options?.pageSize) params.set('pageSize', String(options.pageSize));
  if (options?.status) params.set('status', options.status);
  if (options?.salesOrderId) params.set('salesOrderId', options.salesOrderId);

  const qs = params.toString();
  return appApi.get<ShipmentListResponse>(
    `/api/v1/sites/${siteId}/sales/shipments${qs ? `?${qs}` : ''}`
  );
}

export function getShipment(siteId: string, shipmentId: string): Promise<ShipmentDto> {
  return appApi.get<ShipmentDto>(`/api/v1/sites/${siteId}/sales/shipments/${shipmentId}`);
}

export function createShipmentFromAllocations(
  siteId: string,
  salesOrderId: string,
  request: CreateShipmentRequest
): Promise<ShipmentDto> {
  const qs = new URLSearchParams({ salesOrderId }).toString();
  return appApi.post<ShipmentDto>(`/api/v1/sites/${siteId}/sales/shipments?${qs}`, request);
}

export function startPicking(siteId: string, shipmentId: string): Promise<ShipmentDto> {
  return appApi.post<ShipmentDto>(
    `/api/v1/sites/${siteId}/sales/shipments/${shipmentId}/start-picking`
  );
}

export function markPacked(siteId: string, shipmentId: string): Promise<ShipmentDto> {
  return appApi.post<ShipmentDto>(`/api/v1/sites/${siteId}/sales/shipments/${shipmentId}/pack`);
}

export function markShipped(
  siteId: string,
  shipmentId: string,
  request: MarkShipmentShippedRequest
): Promise<ShipmentDto> {
  return appApi.post<ShipmentDto>(
    `/api/v1/sites/${siteId}/sales/shipments/${shipmentId}/ship`,
    request
  );
}

export function cancelShipment(
  siteId: string,
  shipmentId: string,
  request: CancelShipmentRequest
): Promise<ShipmentDto> {
  return appApi.post<ShipmentDto>(
    `/api/v1/sites/${siteId}/sales/shipments/${shipmentId}/cancel`,
    request
  );
}

