export interface TransportManifestDto {
  id: string;
  siteId: string;
  outboundTransferId: string;
  status: string;
  transporterName?: string | null;
  transporterLicenseNumber?: string | null;
  driverName?: string | null;
  driverLicenseNumber?: string | null;
  driverPhone?: string | null;
  vehicleMake?: string | null;
  vehicleModel?: string | null;
  vehiclePlate?: string | null;
  departureAt?: string | null;
  arrivalAt?: string | null;
  metrcManifestNumber?: string | null;
}

export interface UpsertTransportManifestRequest {
  transporterName?: string | null;
  transporterLicenseNumber?: string | null;
  driverName?: string | null;
  driverLicenseNumber?: string | null;
  driverPhone?: string | null;
  vehicleMake?: string | null;
  vehicleModel?: string | null;
  vehiclePlate?: string | null;
  departureAt?: string | null;
  arrivalAt?: string | null;
}

