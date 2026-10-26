import { post, put, del } from './apiService';
export interface CategoryRequest { name: string; isSubCategory: boolean; categoryId?: string; }
export const createCategory = (data: CategoryRequest) => post<CategoryRequest, string>(data.isSubCategory ? '/api/categories/subcategories' : '/api/categories', data);
export const updateCategory = (id: string, data: CategoryRequest) => put<CategoryRequest, void>(`${data.isSubCategory ? '/api/categories/subcategories' : '/api/categories'}/${id}`, data);
export const deleteCategory = (id: string, isSubCategory: boolean) => del(`${isSubCategory ? '/api/categories/subcategories' : '/api/categories'}/${id}`);
