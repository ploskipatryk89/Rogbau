// src/api/purchaseSerwise.ts

import { get, post, del, put } from './apiService';
import type { PurchaseListDto } from '../types/response/PurchaseResponse';
import type { CreatePurchaseCommand } from '../types/request/CreatePurchaseRequest';
import type { PaginationList } from '../types/pagination/pagination';

export async function getPurchases(params?: {
  pageNumber?: number;
  pageSize?: number;
}): Promise<PaginationList<PurchaseListDto>> {
  const queryParams = new URLSearchParams();

  if (params?.pageNumber !== undefined) {
    queryParams.set('pageNumber', String(params.pageNumber));
  }

  if (params?.pageSize !== undefined) {
    queryParams.set('pageSize', String(params.pageSize));
  }

  const query = queryParams.toString();

  return get<PaginationList<PurchaseListDto>>(
    `/api/purchases${query ? `?${query}` : ''}`
  );
}

export async function getPurchaseById(id: string): Promise<PurchaseListDto> {
  return get<PurchaseListDto>(`/api/purchases/${id}`);
}

export async function createPurchase(data: CreatePurchaseCommand): Promise<string> {
  //  Przekazujemy oba typy: <TRequest, TResponse>
  return post<CreatePurchaseCommand, string>('/api/purchases/createPurchase', data);
}

export async function editPurchase(id: string, data: CreatePurchaseCommand): Promise<void> {
  return put<CreatePurchaseCommand, void>(`/api/purchases/${id}`, data);
}

export async function deletePurchase(id: string): Promise<void> {
  return del<void>(`/api/purchases/${id}`);
}


