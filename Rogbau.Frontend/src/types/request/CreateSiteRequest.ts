import { SiteStatus } from '../enums/SiteStatus';

export interface CreateSiteCommand {
  name: string;
  address?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  status: SiteStatus;
  userIds: string[];
}