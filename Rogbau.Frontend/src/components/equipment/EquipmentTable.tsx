import { useAuth } from "../../context/AuthContext";
import React from 'react';
import type { EquipmentResponse } from '../../types/response/EquipmentResponse';
import { WrenchScrewdriverIcon, EyeIcon, TrashIcon, PencilSquareIcon} from '@heroicons/react/24/outline';
import { IsEmpty } from '../ui/IsEmpty';

interface EquipmentTableProps {
  items: EquipmentResponse[];
  isLoading: boolean;
  onViewDetails: (id: string) => void;
  onDelete: (id: string, name: string) => void;
  onEdit: (equipment: EquipmentResponse) => void;
  
  
}

export const EquipmentTable: React.FC<EquipmentTableProps> = ({
  items,
  isLoading,
  onViewDetails,
  onDelete,
  onEdit,

}) => {
  const { user } = useAuth();
  const isAdmin = user?.role === "Admin";
  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-12 text-center text-slate-500">
        Ładowanie listy sprzętu...
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <IsEmpty
      title='Brak sprzętu do wyświetlenia'
      subTitle={isAdmin ? 'Kliknij "Dodaj Sprzęt", aby zarejestrować nowe pozycje.' : undefined}
      icon ={WrenchScrewdriverIcon}
      />
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              <th className="py-3.5 px-4">Nazwa sprzętu</th>
              <th className="py-3.5 px-4 text-center">J.m.</th>
              <th className="py-3.5 px-4 text-right">Waga (kg)</th>
              <th className="py-3.5 px-4 text-right">Dostępne na magazynie</th>
              <th className="py-3.5 px-4 text-right">Stawka/Dzień</th>
              <th className="py-3.5 px-4 text-center">Status</th>
              <th className="py-3.5 px-4 text-right">Akcje</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
            {items.map((item) => (
              <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                <td className="py-3.5 px-4 font-semibold text-slate-900">{item.name}</td>
                <td className="py-3.5 px-4 text-center text-slate-500">{item.unit}</td>
                <td className="py-3.5 px-4 text-right">{item.weightKg} kg</td>
                <td className="py-3.5 px-4 text-right font-bold text-emerald-600">{item.quantityAvailable}</td>
                <td className="py-3.5 px-4 text-right font-medium">{item.dayPrice.toLocaleString("pl-Pl" , {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 3,
                })} zł</td>
                <td className="py-3.5 px-4 text-center">
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                    item.isActive ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' : 'bg-slate-100 text-slate-500'
                  }`}>
                    {item.isActive ? 'Aktywny' : 'Inaktywny'}
                  </span>
                </td>
                <td className="py-3.5 px-4 text-right">
                  <div className="flex items-center justify-end gap-2">
                    <button
                      onClick={() => onViewDetails(item.id)}
                      className="p-1.5 text-slate-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors cursor-pointer"
                      title="Szczegóły"
                    >
                      <EyeIcon className="w-4 h-4" />
                    </button>

                    <button
  hidden={!isAdmin}
  onClick={() => onEdit(item)}
  className="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors cursor-pointer"
  title="Edytuj"
>
  <PencilSquareIcon className="w-4 h-4" />
</button>
                    {isAdmin && (
  <button
    onClick={() => onDelete(item.id, item.name)}
    className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
    title="Usuń trwale"
    aria-label={`Usuń trwale: ${item.name}`}
  >
    <TrashIcon className="w-4 h-4" />
  </button>

  
)}

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
