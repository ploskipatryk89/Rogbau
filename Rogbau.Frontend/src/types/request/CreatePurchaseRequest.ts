export interface CreatePurchaseItemDto {
  equipmentId: string;
  quantity: number;
  purchasePrice: number;
}

export interface CreatePurchaseCommand {
  supplierId: string;
  purchaseDate: string; // YYYY-MM-DD
  items: CreatePurchaseItemDto[];
}