export interface SubCategoryDto {
  id: string;
  name: string;
  categoryId: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  subCategories: SubCategoryDto[];
}