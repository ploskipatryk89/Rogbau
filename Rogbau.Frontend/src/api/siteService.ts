import { del, get, patch, post } from './apiService';
import type { SiteDto, SiteDetailsDto } from '../types/response/SiteResponse.ts';
import type { CreateSiteCommand } from '../types/request/CreateSiteRequest.ts';
import type { SiteStatus } from '../types/enums/SiteStatus';
import type {
  SetSiteInitialStockCommand,
  SetSiteInitialStockItemDto,
  StartSiteRequest,
  FinishSiteRequest
} from '../types/request/SiteRequest.ts';

export async function getSites(): Promise<SiteDto[]> {
  return get<SiteDto[]>('/api/sites');
}

export const getMySites = () => get<{ id: string; name: string; status: SiteStatus }[]>('/api/sites/my');

export async function getSiteById(id: string): Promise<SiteDetailsDto> {
  return get<SiteDetailsDto>(`/api/sites/${id}`);
}

export async function createSite(data: CreateSiteCommand): Promise<string> {
  return post<CreateSiteCommand, string>('/api/Sites/createSite', data);
}

export async function assignUserToSite(
  siteId: string,
  userId: string
): Promise<void> {
  return post<void, void>(
    `/api/sites/${siteId}/users/${userId}`,
    undefined
  );
}

export async function deleteUserFromSite(
  siteId: string,
  userId: string
): Promise<void> {
  return del<void>(
    `/api/sites/${siteId}/users/${userId}`
  );
}

export async function setSiteInitialStock(
  siteId: string,
  items: SetSiteInitialStockItemDto[]
): Promise<void> {
  const payload: SetSiteInitialStockCommand = {
    siteId,
    items,
  };

  return post<SetSiteInitialStockCommand, void>(
    `/api/sites/${siteId}/initial-stock`,
    payload
  );
}

export async function startSite(data: StartSiteRequest): Promise<void> {
  return patch<StartSiteRequest, void>(
    `/api/Sites/${data.id}/start`,
    data
  );
}

export async function finishSite(data: FinishSiteRequest): Promise<void> {
  return patch<FinishSiteRequest, void>(
    `/api/Sites/${data.id}/finish`,
    data
  );
}
