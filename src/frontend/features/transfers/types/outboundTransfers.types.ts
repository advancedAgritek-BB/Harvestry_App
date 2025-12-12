export type OutboundTransferStatus =
  | 'Draft'
  | 'Ready'
  | 'Submitted'
  | 'Voided'
  | 'Cancelled'
  | string;

export interface OutboundTransferPackageDto {
  id: string;
  packageId: string;
  packageLabel?: string | null;
  quantity: number;
  unitOfMeasure: string;
}

export interface OutboundTransferDto {
  id: string;
  siteId: string;
  shipmentId?: string | null;
  salesOrderId?: string | null;
  destinationLicenseNumber: string;
  destinationFacilityName?: string | null;
  status: OutboundTransferStatus;
  statusReason?: string | null;
  plannedDepartureAt?: string | null;
  plannedArrivalAt?: string | null;
  metrcTransferTemplateId?: number | null;
  metrcTransferNumber?: string | null;
  metrcSyncStatus?: string | null;
  metrcSyncError?: string | null;
  packages: OutboundTransferPackageDto[];
}

export interface OutboundTransferListResponse {
  transfers: OutboundTransferDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateOutboundTransferFromShipmentRequest {
  shipmentId: string;
  plannedDepartureAt?: string | null;
  plannedArrivalAt?: string | null;
}

export interface SubmitOutboundTransferToMetrcRequest {
  metrcSyncJobId: string;
  licenseNumber: string;
  priority?: number;
}

export interface VoidOutboundTransferRequest {
  metrcSyncJobId: string;
  licenseNumber: string;
  reason: string;
}

