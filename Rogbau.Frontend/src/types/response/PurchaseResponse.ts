export interface PurchaseItemDto {
  equipmentId: string;
  equipmentName: string;
  unit: string;
  quantity: number;
  purchaseItemPrice: number;
  purchaseItemTotalPrice: number;
}

export interface PurchaseListDto {
  supplierId: string;
  id: string;
  date: string; // DateOnly z .NET przychodzi w formacie JSON jako YYYY-MM-DD
  supplierName: string;
  totalItemsCount: number;
  totalQuantity: number;
  totalPrice: number;
  items: PurchaseItemDto[];
}
