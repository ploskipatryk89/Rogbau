export interface EditMovementItem {
  equipmentId: string;
  quantity: number;
}

export interface EditWzRequest {
  toConstructionSiteId: string;
  orderId: string | null;
  note: string;
  items: EditMovementItem[];
}

export interface EditTransferRequest {
  fromConstructionSiteId: string;
  toConstructionSiteId: string;
  note: string;
  items: EditMovementItem[];
}
