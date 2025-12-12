export type SalesOrderStatus =
  | 'Draft'
  | 'Submitted'
  | 'Allocated'
  | 'Shipped'
  | 'Cancelled'
  | string;

export interface SalesOrderLineDto {
  id: string;
  lineNumber: number;
  itemId: string;
  itemName: string;
  unitOfMeasure: string;
  requestedQuantity: number;
  allocatedQuantity: number;
  shippedQuantity: number;
  unitPrice?: number | null;
  currencyCode: string;
}

export interface SalesOrderDto {
  id: string;
  siteId: string;
  orderNumber: string;
  customerName: string;
  destinationLicenseNumber?: string | null;
  destinationFacilityName?: string | null;
  status: SalesOrderStatus;
  requestedShipDate?: string | null; // DateOnly serialized
  submittedAt?: string | null;
  cancelledAt?: string | null;
  notes?: string | null;
  createdAt: string;
  updatedAt: string;
  lines: SalesOrderLineDto[];
}

export interface SalesOrderListResponse {
  orders: SalesOrderDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateSalesOrderRequest {
  orderNumber: string;
  customerName: string;
  destinationLicenseNumber: string;
  destinationFacilityName?: string | null;
  requestedShipDate?: string | null; // YYYY-MM-DD
  notes?: string | null;
}

export interface AddSalesOrderLineRequest {
  lineNumber: number;
  itemId: string;
  itemName: string;
  requestedQuantity: number;
  unitOfMeasure: string;
  unitPrice?: number | null;
  currencyCode?: string;
}

export interface CancelSalesOrderRequest {
  reason: string;
}

