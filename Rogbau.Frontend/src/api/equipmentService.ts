import { get, post, del, put} from './apiService';
import type { EquipmentResponse } from '../types/response/EquipmentResponse';
import type { EquipmentDetailsDto } from '../types/response/EquipmentDetailsDto';
import type { CreateEquipmentRequest } from '../types/request/CreateEquipmentRequest';
import type { CategoryDto } from '../types/response/CategoryDto';
import type { EditEquipmentRequest } from '../types/request/EditEquipmentRequest';
import type { PaginationList } from '../types/pagination/pagination';


export async function getEquipments(params?: {
  searchQuery?: string;
  isActive?: boolean;
  categoryId?: string;
  subCategoryId?: string;
  pageNumber?: number; 
  pageSize?: number;   
}): Promise<PaginationList<EquipmentResponse>> {
  const queryParams = new URLSearchParams();

  if (params?.searchQuery && params.searchQuery.trim() !== '') {
    queryParams.append('searchQuery', params.searchQuery.trim());
  }
  if (params?.isActive !== undefined) {
    queryParams.append('isActive', String(params.isActive));
  }
  if (params?.categoryId && params.categoryId !== 'ALL') {
    queryParams.append('categoryId', params.categoryId);
  }
  if (params?.subCategoryId && params.subCategoryId !== 'ALL') {
    queryParams.append('subCategoryId', params.subCategoryId);
  }

  
  if (params?.pageNumber !== undefined) {
    queryParams.append('pageNumber', String(params.pageNumber));
  }
  if (params?.pageSize !== undefined) {
    queryParams.append('pageSize', String(params.pageSize));
  }

  const queryString = queryParams.toString();
  const url = queryString ? `/api/equipments?${queryString}` : '/api/equipments';

  return get<PaginationList<EquipmentResponse>>(url);
}

// Formularze wyboru sprzętu potrzebują wszystkich wyników, nie jednej strony.
export async function getAllEquipments(
  params?: Omit<NonNullable<Parameters<typeof getEquipments>[0]>, 'pageNumber' | 'pageSize'>
): Promise<EquipmentResponse[]> {
  const items: EquipmentResponse[] = [];
  let pageNumber = 1;

  while (true) {
    const page = await getEquipments({ ...params, pageNumber, pageSize: 100 });
    items.push(...page.items);

    if (!page.hasNextPage) return items;
    pageNumber += 1;
  }
}

export async function getEquipmentById(id: string): Promise<EquipmentDetailsDto> {
    return get<EquipmentDetailsDto>(`/api/equipments/${id}`);
}

export async function createEquipment(data: CreateEquipmentRequest): Promise<EquipmentResponse> {
    return post<CreateEquipmentRequest, EquipmentResponse>('/api/equipments/createEquipment', data);
}


export async function deleteEquipment(id: string): Promise<void> {
    return del<void>(`/api/equipments/${id}`);
}

export async function getCategories(): Promise<CategoryDto[]> {
    return get<CategoryDto[]>('/api/Categories');
}

export async function editEquipment(
  id: string,
  data: EditEquipmentRequest
): Promise<void> {
  return put<EditEquipmentRequest, void>(
    `/api/equipments/${id}`,
    data
  );
}

