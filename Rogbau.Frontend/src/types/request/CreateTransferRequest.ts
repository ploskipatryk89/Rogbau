export interface CreatePzItemDto {
    equipmentId: string;
    quantity: number;
    quantityScrapped: number;
}

export interface CreateWzItemDto {
    equipmentId: string;
    quantity: number;
}

export interface CreateTransferItemDto {
    equipmentId: string;
    quantity: number;
}

export interface CreatePzCommand {
    note?: string;
    fromConstructionSiteId: string;
    items: CreatePzItemDto[];
}

export interface CreateWzCommand {
    note?: string;
    orderId?: string;
    toConstructionSiteId: string;
    items: CreateWzItemDto[];
}

export interface CreateTransferCommand {
    fromConstructionSiteId: string;
    toConstructionSiteId: string;
    items: CreateTransferItemDto[];
}
