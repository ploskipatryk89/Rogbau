export interface HistoricalDataDto {
    id: string;

    siteId: string;
    siteName: string;

    equipmentId: string;
    equipmentName: string;

    fromDate: string;
    toDate: string | null;

    quantity: number;
    dayPrice: number;

    days: number;
    totalEarned: number;
}

export interface HistoricalDataFilters {
    siteId?: string;
    equipmentId?: string;
    fromDate?: string;
    toDate?: string;
}