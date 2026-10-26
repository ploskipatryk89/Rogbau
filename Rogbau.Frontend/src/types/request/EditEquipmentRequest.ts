export interface EditEquipmentRequest {
  id: string;
  name: string;
  unit: string;
  weightKg: number;
  categoryId: string;
  subCategoryId: string | null;
  purchasePrice: number;
  dayPrice: number;
}