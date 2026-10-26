export interface TransferResponseDto{
    type: number;
    id: string;
    number: string;
    fromName: string;
    toName: string;
    date: Date;
    status: number;

}

export interface TransferDetailsItemDto {
    equipmentId: string;
    equipmentName: string;
    unit: string;
    quantity: number;
    scrappedQuantity: number;
    receivedQuantity: number | null;
    receivedScrappedQuantity: number | null;
}

export interface ConfirmReturnItem {
    equipmentId: string;
    quantity: number;
    scrappedQuantity: number;
}

export interface TransferDetailsDto {
    orderId: string | null;
    note: string | null;
    id: string;
    number: string;

    fromConstructionSiteId: string | null;
    fromName: string | null;

    toConstructionSiteId: string | null;
    toName: string | null;

    date: string;

    status: number;
    type: number;

    canConfirm: boolean;
    totalWeight: number;
    items: TransferDetailsItemDto[];

}

