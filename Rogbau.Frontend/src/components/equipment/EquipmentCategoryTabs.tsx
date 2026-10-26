import React from 'react';
import type { CategoryDto } from '../../types/response/CategoryDto';

interface EquipmentCategoryTabsProps {
  categories: CategoryDto[];
  selectedCategoryId: string;
  selectedSubCategoryId: string;
  onSelectCategory: (id: string) => void;
  onSelectSubCategory: (id: string) => void;
}

export const EquipmentCategoryTabs: React.FC<EquipmentCategoryTabsProps> = ({
  categories,
  selectedCategoryId,
  selectedSubCategoryId,
  onSelectCategory,
  onSelectSubCategory,
}) => {
  const currentCategory = categories.find((c) => c.id === selectedCategoryId);

  return (
    <div className="space-y-2">
      {/* KATEGORIE GŁÓWNE */}
      <div className="border-b border-slate-200">
        <nav className="-mb-px flex space-x-6 overflow-x-auto">
          <button
            type="button"
            onClick={() => onSelectCategory('ALL')}
            className={`py-3 px-1 border-b-2 font-semibold text-sm whitespace-nowrap transition-colors cursor-pointer ${
              selectedCategoryId === 'ALL'
                ? 'border-amber-500 text-amber-600'
                : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
            }`}
          >
            Wszystkie
          </button>

          {categories.map((cat) => (
            <button
              key={cat.id}
              type="button"
              onClick={() => onSelectCategory(cat.id)}
              className={`py-3 px-1 border-b-2 font-semibold text-sm whitespace-nowrap transition-colors cursor-pointer ${
                selectedCategoryId === cat.id
                  ? 'border-amber-500 text-amber-600'
                  : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
              }`}
            >
              {cat.name}
            </button>
          ))}
        </nav>
      </div>

      {/* PODKATEGORIE */}
      {currentCategory && currentCategory.subCategories && currentCategory.subCategories.length > 0 && (
        <div className="flex items-center gap-2 overflow-x-auto pt-1">
          <span className="text-xs font-semibold text-slate-400 mr-2 uppercase tracking-wider">
            Podkategorie:
          </span>
          <button
            type="button"
            onClick={() => onSelectSubCategory('ALL')}
            className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors cursor-pointer ${
              selectedSubCategoryId === 'ALL' ? 'bg-slate-900 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            }`}
          >
            Wszystkie
          </button>

          {currentCategory.subCategories.map((subCat) => (
            <button
              key={subCat.id}
              type="button"
              onClick={() => onSelectSubCategory(subCat.id)}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium transition-colors cursor-pointer ${
                selectedSubCategoryId === subCat.id ? 'bg-slate-900 text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
              }`}
            >
              {subCat.name}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};