import React from "react";
import type { TransferResponseDto } from "../../types/response/TransferResponseDto";
import {
  ArrowRightIcon,
  EyeIcon,
  ArrowsRightLeftIcon,
  PencilSquareIcon,
} from "@heroicons/react/24/outline";
import { IsEmpty } from "../ui/IsEmpty";

interface TransferTableProps {
  items: TransferResponseDto[];
  isLoading: boolean;
  onViewDetails: (id: string) => void;
  onEdit?: (id: string) => void;
  editingId?: string | null;
}

export const TransferTable: React.FC<TransferTableProps> = ({
  items,
  isLoading,
  onViewDetails,
  onEdit,
  editingId,
}) => {
  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-12 text-center text-slate-500">
        Ładowanie listy transferów...
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <IsEmpty
        title="Brak transferów"
        subTitle="Nie znaleziono żadnych dokumentów WZ, PZ ani przesunięć."
        icon={ArrowsRightLeftIcon}
      />
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              <th className="py-3.5 px-4">
                Numer
              </th>

              <th className="py-3.5 px-4">
                Z
              </th>

              <th className="py-3.5 px-4 text-center">
                
              </th>

              <th className="py-3.5 px-4">
                Do
              </th>

              <th className="py-3.5 px-4">
                Data
              </th>

              <th className="py-3.5 px-4 text-right">
                Akcje
              </th>
            </tr>
          </thead>

          <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
            {items.map((item) => (
              <tr
                key={item.id}
                className={item.status === 2 ? 'bg-amber-50/60 hover:bg-amber-50 transition-colors' : 'hover:bg-slate-50/80 transition-colors'}
              >
                <td className={`py-3.5 px-4 ${item.status === 2 ? 'font-bold text-amber-800' : 'font-medium text-slate-900'}`}>
                  {item.number}
                  {item.status === 2 && <span className="block mt-1 text-xs font-semibold text-amber-700">Oczekuje na odbiór</span>}
                </td>

                <td className="py-3.5 px-4">
                  {item.fromName}
                </td>

                <td className="py-3.5 px-4 text-center">
                  <ArrowRightIcon className="w-4 h-4 text-slate-400 inline-block" />
                </td>

                <td className="py-3.5 px-4">
                  {item.toName}
                </td>

                <td className="py-3.5 px-4 text-slate-500">
                  {new Date(item.date).toLocaleDateString("pl-PL")}
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
                    {onEdit && (item.type === 1 || item.type === 2 || item.type === 3) && (item.status === 2 || item.status === 3) && <button
                      type="button"
                      onClick={() => onEdit(item.id)}
                      disabled={editingId != null}
                      className="p-1.5 text-slate-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors cursor-pointer"
                      title="Edytuj dokument"
                      aria-label={`Edytuj ${item.number}`}
                    >
                     <PencilSquareIcon className="w-5 h-5" />
                    </button>}
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

