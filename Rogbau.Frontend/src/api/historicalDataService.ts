import { get } from "./apiService";
import type { PaginationList } from "../types/pagination/pagination";
import type {
    HistoricalDataDto,
    HistoricalDataFilters
} from "../types/response/HistoricalData";

export async function getHistoricalData(
    filters?: HistoricalDataFilters & {
        pageNumber?: number;
        pageSize?: number;
    }
): Promise<PaginationList<HistoricalDataDto>> {
    const params = new URLSearchParams();

    if (filters?.pageNumber !== undefined) {
        params.append("pageNumber", String(filters.pageNumber));
    }

    if (filters?.pageSize !== undefined) {
        params.append("pageSize", String(filters.pageSize));
    }

    if (filters?.siteId) {
        params.append("siteId", filters.siteId);
    }

    if (filters?.equipmentId) {
        params.append("equipmentId", filters.equipmentId);
    }

    if (filters?.fromDate) {
        params.append("fromDate", filters.fromDate);
    }

    if (filters?.toDate) {
        params.append("toDate", filters.toDate);
    }

    const query = params.toString();

    return get<PaginationList<HistoricalDataDto>>(
        `/api/historical-data${query ? `?${query}` : ""}`
    );
}
