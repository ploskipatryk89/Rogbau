export interface EquipmentResponse {
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
  
  purchasePrice: number;
  dayPrice: number;
  isActive: boolean;

}