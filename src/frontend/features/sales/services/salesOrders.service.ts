import { appApi } from '@/lib/api/appRouteClient';
import type {
  AddSalesOrderLineRequest,
  CancelSalesOrderRequest,
  CreateSalesOrderRequest,
  SalesOrderDto,
  SalesOrderListResponse,
} from '../types/salesOrders.types';
import type {
  AllocateSalesOrderRequest,
  SalesAllocationDto,
  UnallocateSalesOrderRequest,
} from '../types/allocations.types';

export async function listSalesOrders(
  siteId: string,
  options?: {
    page?: number;
    pageSize?: number;
    status?: string;
    search?: string;
  }
): Promise<SalesOrderListResponse> {
  const params = new URLSearchParams();
  if (options?.page) params.set('page', String(options.page));
  if (options?.pageSize) params.set('pageSize', String(options.pageSize));
  if (options?.status) params.set('status', options.status);
  if (options?.search) params.set('search', options.search);

  const qs = params.toString();
  return appApi.get<SalesOrderListResponse>(
    `/api/v1/sites/${siteId}/sales/orders${qs ? `?${qs}` : ''}`
  );
}

export function getSalesOrder(siteId: string, salesOrderId: string): Promise<SalesOrderDto> {
  return appApi.get<SalesOrderDto>(`/api/v1/sites/${siteId}/sales/orders/${salesOrderId}`);
}

export function createSalesOrder(siteId: string, request: CreateSalesOrderRequest): Promise<SalesOrderDto> {
  return appApi.post<SalesOrderDto>(`/api/v1/sites/${siteId}/sales/orders`, request);
}

export function addSalesOrderLine(
  siteId: string,
  salesOrderId: string,
  request: AddSalesOrderLineRequest
): Promise<SalesOrderDto> {
  return appApi.post<SalesOrderDto>(
    `/api/v1/sites/${siteId}/sales/orders/${salesOrderId}/lines`,
    request
  );
}

export function submitSalesOrder(siteId: string, salesOrderId: string): Promise<SalesOrderDto> {
  return appApi.post<SalesOrderDto>(
    `/api/v1/sites/${siteId}/sales/orders/${salesOrderId}/submit`
  );
}

export function cancelSalesOrder(
  siteId: string,
  salesOrderId: string,
  request: CancelSalesOrderRequest
): Promise<SalesOrderDto> {
  return appApi.post<SalesOrderDto>(
    `/api/v1/sites/${siteId}/sales/orders/${salesOrderId}/cancel`,
    request
  );
}

export function getAllocations(siteId: string, salesOrderId: string): Promise<SalesAllocationDto[]> {
  return appApi.get<SalesAllocationDto[]>(
    `/api/v1/sites/${siteId}/sales/orders/${salesOrderId}/allocations`
  );
}

export function allocate(
  siteId: string,
  salesOrderId: string,
  request: AllocateSalesOrderRequest
): Promise<SalesAllocationDto[]> {
  return appApi.post<SalesAllocationDto[]>(
    `/api/v1/sites/${siteId}/sales/orders/${salesOrderId}/allocations/allocate`,
    request
  );
}

export function unallocate(
  siteId: string,
  salesOrderId: string,
  request: UnallocateSalesOrderRequest
): Promise<SalesAllocationDto[]> {
  return appApi.post<SalesAllocationDto[]>(
    `/api/v1/sites/${siteId}/sales/orders/${salesOrderId}/allocations/unallocate`,
    request
  );
}

