export interface SalesAllocationDto {
  id: string;
  salesOrderId: string;
  salesOrderLineId: string;
  packageId: string;
  packageLabel?: string | null;
  allocatedQuantity: number;
  unitOfMeasure: string;
  isCancelled: boolean;
  createdAt: string;
}

export interface AllocatePackageRequest {
  packageId: string;
  quantity: number;
}

export interface AllocateSalesOrderLineRequest {
  salesOrderLineId: string;
  packages: AllocatePackageRequest[];
}

export interface AllocateSalesOrderRequest {
  lines: AllocateSalesOrderLineRequest[];
}

export interface UnallocateSalesOrderRequest {
  allocationIds: string[];
  reason: string;
}

