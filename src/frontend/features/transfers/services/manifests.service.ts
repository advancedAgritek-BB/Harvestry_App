import { appApi } from '@/lib/api/appRouteClient';
import type { TransportManifestDto, UpsertTransportManifestRequest } from '../types/transportManifest.types';

export function getManifest(siteId: string, transferId: string): Promise<TransportManifestDto> {
  return appApi.get<TransportManifestDto>(
    `/api/v1/sites/${siteId}/transfers/outbound/${transferId}/manifest`
  );
}

export function upsertManifest(
  siteId: string,
  transferId: string,
  request: UpsertTransportManifestRequest
): Promise<TransportManifestDto> {
  return appApi.put<TransportManifestDto>(
    `/api/v1/sites/${siteId}/transfers/outbound/${transferId}/manifest`,
    request
  );
}

