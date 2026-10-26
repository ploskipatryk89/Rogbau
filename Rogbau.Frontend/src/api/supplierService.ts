import { get, post, put } from './apiService.ts';
import { del } from './apiService.ts';
import type { SupplierResponseDto } from '../types/response/SupplierResponseDto.ts';
import type { CreateSupplierRequest } from '../types/request/CreateSupplierRequest.ts';

export async function getSuppliers(): Promise<SupplierResponseDto[]> {
  return get<SupplierResponseDto[]>('/api/suppliers');
}

export async function editSupplier(id: string, data: CreateSupplierRequest): Promise<void> {
  return put<CreateSupplierRequest, void>(`/api/suppliers/${id}`, data);
}





export async function deleteSupplier(id: string): Promise<void> {
    return del<void>(`/api/suppliers/${id}`);
}

export async function createSupplier(data: CreateSupplierRequest): Promise<SupplierResponseDto> {
    return post<CreateSupplierRequest, SupplierResponseDto>('/api/Suppliers/createSupplier', data);
}
