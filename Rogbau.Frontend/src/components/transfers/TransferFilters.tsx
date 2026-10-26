import React from 'react';
import type { TransferFiltersType as TransferFiltersType } from '../../types/TransferFilters';
import {
  MagnifyingGlassIcon,
  CalendarDaysIcon,
} from '@heroicons/react/24/outline';

interface TransferFiltersProps {
  filters: TransferFiltersType;
  onFilterChange: (newFilters: TransferFiltersType) => void;
}

export const TransferFilters: React.FC<TransferFiltersProps> = ({
  filters,
  onFilterChange,
}) => {
  return (
    <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col sm:flex-row gap-4 justify-between items-center">

      {/* Numer dokumentu */}
      <div className="relative w-full sm:w-80">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400">
          <MagnifyingGlassIcon className="w-5 h-5" />
        </div>

        <input
          type="text"
          placeholder="Szukaj po numerze dokumentu..."
          value={filters.search}
          onChange={(e) =>
            onFilterChange({
              ...filters,
              search: e.target.value,
            })
          }
          className="w-full pl-10 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm text-slate-800 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-all"
        />
      </div>

      {/* Daty */}
      <div className="flex flex-col sm:flex-row items-center gap-3 w-full sm:w-auto">

        <CalendarDaysIcon className="w-5 h-5 text-slate-400 hidden sm:block" />

        <div className="flex items-center gap-2 w-full sm:w-auto">
          <span className="text-sm text-slate-500">
            Od
          </span>

          <input
            type="date"
            value={filters.dateFrom}
            onChange={(e) =>
              onFilterChange({
                ...filters,
                dateFrom: e.target.value,
              })
            }
            className="bg-slate-50 border border-slate-200 text-slate-700 text-sm rounded-lg py-2 px-3 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
          />
        </div>

        <div className="flex items-center gap-2 w-full sm:w-auto">
          <span className="text-sm text-slate-500">
            Do
          </span>

          <input
            type="date"
            value={filters.dateTo}
            onChange={(e) =>
              onFilterChange({
                ...filters,
                dateTo: e.target.value,
              })
            }
            className="bg-slate-50 border border-slate-200 text-slate-700 text-sm rounded-lg py-2 px-3 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
          />
        </div>

      </div>
    </div>
  );
};