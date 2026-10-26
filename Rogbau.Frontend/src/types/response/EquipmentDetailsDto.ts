export interface EquipmentDetailsDto {
  id: string;
  name: string;
  unit: string;
  weightKg: number;
  categoryId: string;
  categoryName: string;
  subCategoryId: string | null;
  subCategoryName: string | null;
  initialQuantity: number;
  purchasedQuantity: number;
  quantityAvailable: number;
  quantityScrapped: number;
  quantityOnAllSites: number;
  quantityInTransit: number;
  purchasePrice: number;
  dayPrice: number;
  isActive: boolean;
  siteAllocations: EquipmentSiteAllocationDto[];
}

export interface EquipmentSiteAllocationDto {
  siteId: string;
  siteName: string;
  address: string | null;
  quantityOnSite: number;
}