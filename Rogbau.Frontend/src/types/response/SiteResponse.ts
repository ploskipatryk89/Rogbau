import { SiteStatus } from '../enums/SiteStatus';

export interface SiteEquipmentDto {
  equipmentId: string;
  equipmentName: string;
  unit: string;
  allocatedQuantity: number;
  purchaseTotalPrice: number;
  daysOnSite: number;
  totalEarned: number;
  weightKg: number;
}

export interface SiteDto {
  id: string;
  name: string;
  address?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  status: SiteStatus;
  totalEquipmentCount: number;
}

export interface SiteDetailsDto {
  id: string;
  name: string;
  address?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  status: SiteStatus;
  equipment: SiteEquipmentDto[];
  managers: SiteManagerDto[];

}

export interface SiteManagerDto{
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
}