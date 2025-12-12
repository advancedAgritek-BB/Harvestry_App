export type InboundReceiptStatus = 'Draft' | 'Accepted' | 'Rejected' | string;

export interface InboundReceiptLineDto {
  id: string;
  packageLabel: string;
  receivedQuantity: number;
  unitOfMeasure: string;
  accepted: boolean;
  rejectionReason?: string | null;
}

export interface InboundReceiptDto {
  id: string;
  siteId: string;
  outboundTransferId?: string | null;
  metrcTransferId?: number | null;
  metrcTransferNumber?: string | null;
  status: InboundReceiptStatus;
  receivedAt?: string | null;
  notes?: string | null;
  lines: InboundReceiptLineDto[];
}

export interface InboundReceiptListResponse {
  receipts: InboundReceiptDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateInboundReceiptLineRequest {
  packageLabel: string;
  receivedQuantity: number;
  unitOfMeasure: string;
  accepted?: boolean;
  rejectionReason?: string | null;
}

export interface CreateInboundReceiptRequest {
  outboundTransferId?: string | null;
  metrcTransferId?: number | null;
  metrcTransferNumber?: string | null;
  notes?: string | null;
  lines: CreateInboundReceiptLineRequest[];
}

export interface AcceptInboundReceiptRequest {
  notes?: string | null;
}

export interface RejectInboundReceiptRequest {
  reason: string;
}

