import { appApi } from '@/lib/api/appRouteClient';
import type {
  CustomerListResponse,
  CustomerDetailDto,
  CreateCustomerRequest,
  UpdateCustomerRequest,
  UpdateLicenseVerificationRequest,
} from '../types/customers.types';

export interface ListCustomersParams {
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export async function listCustomers(
  siteId: string,
  params: ListCustomersParams = {}
): Promise<CustomerListResponse> {
  const searchParams = new URLSearchParams();
  if (params.search) searchParams.set('search', params.search);
  if (params.isActive !== undefined) searchParams.set('isActive', String(params.isActive));
  if (params.page) searchParams.set('page', String(params.page));
  if (params.pageSize) searchParams.set('pageSize', String(params.pageSize));

  const query = searchParams.toString();
  const endpoint = `/api/v1/sites/${siteId}/sales/customers${query ? `?${query}` : ''}`;
  return appApi.get<CustomerListResponse>(endpoint, { siteId });
}

export async function getCustomer(
  siteId: string,
  customerId: string
): Promise<CustomerDetailDto> {
  return appApi.get<CustomerDetailDto>(
    `/api/v1/sites/${siteId}/sales/customers/${customerId}`,
    { siteId }
  );
}

export async function createCustomer(
  siteId: string,
  request: CreateCustomerRequest
): Promise<CustomerDetailDto> {
  return appApi.post<CustomerDetailDto>(
    `/api/v1/sites/${siteId}/sales/customers`,
    request,
    { siteId }
  );
}

export async function updateCustomer(
  siteId: string,
  customerId: string,
  request: UpdateCustomerRequest
): Promise<CustomerDetailDto> {
  return appApi.put<CustomerDetailDto>(
    `/api/v1/sites/${siteId}/sales/customers/${customerId}`,
    request,
    { siteId }
  );
}

export async function updateLicenseVerification(
  siteId: string,
  customerId: string,
  request: UpdateLicenseVerificationRequest
): Promise<CustomerDetailDto> {
  return appApi.post<CustomerDetailDto>(
    `/api/v1/sites/${siteId}/sales/customers/${customerId}/license-verification`,
    request,
    { siteId }
  );
}
