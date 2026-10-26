import { useAuth } from "../../context/AuthContext";
import React from 'react';
import type { SupplierResponseDto } from '../../types/response/SupplierResponseDto';
import { BuildingStorefrontIcon, TrashIcon, PencilSquareIcon } from '@heroicons/react/24/outline';
import { IsEmpty } from '../ui/IsEmpty';

interface SupplierTableProps {
  items: SupplierResponseDto[];
  isLoading: boolean;
  onDelete: (id: string, name: string) => void;
  onEdit?: (supplier: SupplierResponseDto) => void;
}

export const SupplierTable: React.FC<SupplierTableProps> = ({
  items,
  isLoading,
  onDelete,
  onEdit,
}) => {
  const { user } = useAuth();
  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-12 text-center text-slate-500">
        Ładowanie listy dostawców...
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <IsEmpty
      title='Brak zarejestrowanych dostawców'
      subTitle={user?.role === 'Admin' ? 'Kliknij "Dodaj dostawcę", aby zarejestrować nowego dostawcę.' : undefined}
      icon={BuildingStorefrontIcon}
      />
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden max-w-3xl">
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              <th className="py-3.5 px-6">Nazwa Dostawcy / Firmy</th>
              <th className="py-3.5 px-6 text-right">Akcje</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
            {items.map((item) => (
              <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                <td className="py-4 px-6">
                  <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-lg bg-amber-50 border border-amber-200/60 flex items-center justify-center text-amber-600 shrink-0 font-bold shadow-sm">
                      {item.name.charAt(0).toUpperCase()}
                    </div>
                    <div>
                      <span className="font-bold text-slate-900 block">{item.name}</span>
                      <span className="text-xs text-slate-400 font-mono">ID: {item.id}</span>
                    </div>
                  </div>
                </td>
                <td className="py-4 px-6 text-right">
                  <div className="flex items-center justify-end gap-2">
                    {user?.role === 'Admin' && onEdit && <button
                      type="button" onClick={() => onEdit(item)}
                      className="p-1.5 text-slate-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors cursor-pointer"
                      title="Edytuj" aria-label={`Edytuj dostawcę ${item.name}`}>
                      <PencilSquareIcon className="w-4 h-4" />
                    </button>}
                    <button
                      hidden={user?.role !== "Admin"}
                      onClick={() => onDelete(item.id, item.name)}
                      className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                      title="Usuń"
                    >
                      <TrashIcon className="w-4 h-4" />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
