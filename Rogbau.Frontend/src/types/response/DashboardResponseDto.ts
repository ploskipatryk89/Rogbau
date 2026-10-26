import type { EquipmentMovementStatus } from '../enums/EquipmentMovementStatus';
import type { SiteStatus } from '../enums/SiteStatus';

export interface RecentlyDocumentDto {
  id: string;
  number: string;
  from: string;
  to: string;
  status: EquipmentMovementStatus;
  date: string;
}

export interface RecentlyOrderDto {
  id: string;
  number: string;
  siteName: string;
  status: number;
}

export interface LowEquipmentStockDto {
  id: string;
  name: string;
  quantity: number;
}

export interface ManagerSiteDto {
  id: string;
  siteName: string;
}

export interface AdminDashboardDto {
  dashboardType: 'admin';
  activeSiteQuantity: number;
  equipmentAvailable: number;
  equipmentsOnAllSites: number;
  recentlyDocuments: RecentlyDocumentDto[];
  recentlyOrders: RecentlyOrderDto[];
  lowEquipmentStocks: LowEquipmentStockDto[];
}

export interface WarehousemanDashboardDto {
  dashboardType: 'warehouseman';
  equipmentAvailable: number;
  wzQuantityThisMonth: number;
  pzQuantityThisMonth: number;
  recentlyDocuments: RecentlyDocumentDto[];
  recentlyOrders: RecentlyOrderDto[];
  lowEquipmentStocks: LowEquipmentStockDto[];
}

export interface SiteManagerDashboardDto {
  dashboardType: 'siteManager';
  managerSites: ManagerSiteDto[];
  selectedSiteId: string | null;
  status: SiteStatus | null;
  siteEquipmentQuantity: number;
  wzQuantityThisMonth: number;
  pzQuantityThisMonth: number;
  recentlyDocuments: RecentlyDocumentDto[];
}

export type DashboardResponseDto = AdminDashboardDto | WarehousemanDashboardDto | SiteManagerDashboardDto;
