import type { CreateWzCommand } from "../types/request/CreateTransferRequest";
import type { CreatePzCommand } from "../types/request/CreateTransferRequest";
import type { TransferResponseDto } from "../types/response/TransferResponseDto";
import type { TransferFiltersType } from "../types/TransferFilters";
import type { PaginationList, PaginatedRequest } from "../types/pagination/pagination";
import { get, post, patch, put } from "./apiService";
import type { EditPzRequest } from '../types/request/EditPzRequest';

export async function editPz(id: string, data: EditPzRequest): Promise<void> {
    return put<EditPzRequest, void>(`/api/transfers/${id}/pz`, data);
}
import type { CreateTransferCommand } from "../types/request/CreateTransferRequest";
import type { TransferDetailsDto } from "../types/response/TransferResponseDto";
import type { ConfirmReturnItem } from "../types/response/TransferResponseDto";

export async function getTransfers(
    filters?: TransferFiltersType,
    type?: number | null,
    pagination?: PaginatedRequest
): Promise<PaginationList<TransferResponseDto>> {

    const params = new URLSearchParams();

    if (pagination) {
        params.set("pagination.pageNumber", String(pagination.pageNumber));
        params.set("pagination.pageSize", String(pagination.pageSize));
    }

    if (type) {
        params.append("type", type.toString());
    }

    if (filters?.search) {
        params.append("search", filters.search);
    }

    if (filters?.dateFrom) {
        params.append("dateFrom", filters.dateFrom);
    }

    if (filters?.dateTo) {
        params.append("dateTo", filters.dateTo);
    }

    const query = params.toString();

    return get<PaginationList<TransferResponseDto>>(
        `/api/transfers${query ? `?${query}` : ""}`
    );
}

export async function createWz(data: CreateWzCommand): Promise<string> {
    return post<CreateWzCommand, string>('/api/transfers/createWz', data);
}

export async function createPz(data: CreatePzCommand): Promise<string> {
    return post<CreatePzCommand, string>('/api/transfers/createPz', data);
}

export async function createTransfer(data: CreateTransferCommand): Promise<string> {
    return post<CreateTransferCommand, string>('/api/transfers/createTransfer', data);
}

export async function getTransferById(
    id: string
): Promise<TransferDetailsDto> {
    return get<TransferDetailsDto>(`/api/transfers/${id}`);
}

export async function confirmTransfer(id: string, items?: ConfirmReturnItem[]): Promise<void> {
    return patch<{ items: ConfirmReturnItem[] } | undefined, void>(
        `/api/transfers/${id}/confirm`,
        items ? { items } : undefined
    );
}
import type { EditWzRequest, EditTransferRequest } from '../types/request/EditMovementRequest';

export async function editWz(id: string, data: EditWzRequest): Promise<void> {
    return put<EditWzRequest, void>(`/api/transfers/${id}/wz`, data);
}

export async function editTransfer(id: string, data: EditTransferRequest): Promise<void> {
    return put<EditTransferRequest, void>(`/api/transfers/${id}/transfer`, data);
}
