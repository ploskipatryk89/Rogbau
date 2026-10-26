export interface EditPzItem {
  equipmentId: string;
  quantity: number;
  scrappedQuantity: number;
  receivedQuantity: number | null;
  receivedScrappedQuantity: number | null;
}

export interface EditPzRequest {
  fromConstructionSiteId: string;
  note: string;
  items: EditPzItem[];
}
