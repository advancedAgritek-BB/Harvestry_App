export type ShipmentStatus =
  | 'Draft'
  | 'Picking'
  | 'Packed'
  | 'Shipped'
  | 'Cancelled'
  | string;

export interface ShipmentPackageDto {
  id: string;
  packageId: string;
  packageLabel?: string | null;
  quantity: number;
  unitOfMeasure: string;
  packedAt?: string | null;
}

export interface ShipmentDto {
  id: string;
  siteId: string;
  shipmentNumber: string;
  salesOrderId: string;
  status: ShipmentStatus;
  pickingStartedAt?: string | null;
  packedAt?: string | null;
  shippedAt?: string | null;
  cancelledAt?: string | null;
  carrierName?: string | null;
  trackingNumber?: string | null;
  packages: ShipmentPackageDto[];
}

export interface ShipmentListResponse {
  shipments: ShipmentDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateShipmentRequest {
  shipmentNumber: string;
  notes?: string | null;
}

export interface MarkShipmentShippedRequest {
  carrierName?: string | null;
  trackingNumber?: string | null;
  outboundTransferId?: string | null;
}

export interface CancelShipmentRequest {
  reason: string;
}

