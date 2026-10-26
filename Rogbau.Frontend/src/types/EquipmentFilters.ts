import type { PaginatedRequest } from "./pagination/pagination";

export interface EquipmentFilters {
  searchQuery: string;
  isActive?: boolean;
  categoryId?: string;
  subCategoryId?: string;
}

export type EquipmentQueryParams =
  EquipmentFilters & PaginatedRequest;