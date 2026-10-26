import { get } from './apiService';
import type { DashboardResponseDto } from '../types/response/DashboardResponseDto';

export function getDashboard(siteId?: string): Promise<DashboardResponseDto> {
  let endpoint = '/api/dashboard';
  if (siteId) endpoint += `?siteId=${encodeURIComponent(siteId)}`;
  return get<DashboardResponseDto>(endpoint);
}
