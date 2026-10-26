import React from 'react';
import type { EquipmentFilters as EquipmentFiltersType } from '../../types/equipmentFilter';
import { MagnifyingGlassIcon, FunnelIcon } from '@heroicons/react/24/outline';

interface EquipmentFiltersProps {
  filters: EquipmentFiltersType;
  onFilterChange: (newFilters: EquipmentFiltersType) => void;
}

export const EquipmentFilters: React.FC<EquipmentFiltersProps> = ({ filters, onFilterChange }) => {
  return (
    <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col sm:flex-row gap-4 justify-between items-center">
      {/* Wyszukiwarka tekstowa */}
      <div className="relative w-full sm:w-80">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400">
          <MagnifyingGlassIcon className="w-5 h-5" />
        </div>
        <input
          type="text"
          placeholder="Szukaj po nazwie sprzętu..."
          value={filters.searchQuery}
          onChange={(e) => onFilterChange({ ...filters, searchQuery: e.target.value })}
          className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm text-slate-800 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-all"
        />
      </div>

      {/* Filtr statusu (Domyślnie aktywne, bez opcji "Wszystkie") */}
      <div className="flex items-center gap-3 w-full sm:w-auto justify-end">
        <FunnelIcon className="w-5 h-5 text-slate-400 hidden sm:block" />
        <select
          value={filters.isActive === false ? 'inactive' : 'active'}
          onChange={(e) => {
            const val = e.target.value;
            onFilterChange({
              ...filters,
              isActive: val === 'active',
            });
          }}
          className="w-full sm:w-auto bg-slate-50 border border-slate-200 text-slate-700 text-sm rounded-lg py-2 px-3 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 cursor-pointer"
        >
          <option value="active">Tylko aktywne</option>
          <option value="inactive">Tylko nieaktywne</option>
        </select>
      </div>
    </div>
  );
};