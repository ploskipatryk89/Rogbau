export interface SetSiteInitialStockItemDto {
  equipmentId: string;
  quantity: number;
}

export interface SetSiteInitialStockCommand {
  siteId: string;
  items: SetSiteInitialStockItemDto[];
}

export interface StartSiteRequest{
    id: string;
    startDate: string;
}

export interface FinishSiteRequest{
  id: string;
  endDate: string;
}