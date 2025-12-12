export type LicenseVerificationStatus = 'Unknown' | 'Pending' | 'Verified' | 'Failed';

export interface CustomerSummaryDto {
  id: string;
  siteId: string;
  name: string;
  licenseNumber: string;
  facilityName?: string | null;
  facilityType?: string | null;
  primaryContactName?: string | null;
  email?: string | null;
  phone?: string | null;
  licenseVerifiedStatus: LicenseVerificationStatus;
  orderCount: number;
  isActive: boolean;
}

export interface CustomerDetailDto {
  id: string;
  siteId: string;
  name: string;
  licenseNumber: string;
  facilityName?: string | null;
  facilityType?: string | null;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  zip?: string | null;
  primaryContactName?: string | null;
  email?: string | null;
  phone?: string | null;
  licenseVerifiedStatus: LicenseVerificationStatus;
  licenseVerifiedAt?: string | null;
  licenseVerificationSource?: string | null;
  licenseVerificationNotes?: string | null;
  metrcRecipientId?: string | null;
  isActive: boolean;
  notes?: string | null;
  tags?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CustomerListResponse {
  customers: CustomerSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateCustomerRequest {
  name: string;
  licenseNumber: string;
  facilityName?: string | null;
  facilityType?: string | null;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  zip?: string | null;
  primaryContactName?: string | null;
  email?: string | null;
  phone?: string | null;
  metrcRecipientId?: string | null;
  notes?: string | null;
  tags?: string | null;
}

export interface UpdateCustomerRequest {
  name: string;
  licenseNumber: string;
  facilityName?: string | null;
  facilityType?: string | null;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  zip?: string | null;
  primaryContactName?: string | null;
  email?: string | null;
  phone?: string | null;
  metrcRecipientId?: string | null;
  notes?: string | null;
  tags?: string | null;
  isActive?: boolean | null;
}

export interface UpdateLicenseVerificationRequest {
  status: LicenseVerificationStatus;
  source?: string | null;
  notes?: string | null;
}
