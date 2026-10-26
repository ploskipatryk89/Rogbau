import React, { useState } from 'react';
import type { PurchaseListDto } from '../../types/response/PurchaseResponse';
import {
  ShoppingBagIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  PencilSquareIcon,
  TrashIcon,
} from '@heroicons/react/24/outline';
import { IsEmpty } from '../ui/IsEmpty';
import { useAuth } from '../../context/AuthContext';

interface PurchaseTableProps {
  items: PurchaseListDto[];
  isLoading: boolean;
  onEdit?: (purchase: PurchaseListDto) => void;
  onDelete?: (purchase: PurchaseListDto) => void;
}

export const PurchaseTable: React.FC<PurchaseTableProps> = ({
  items,
  isLoading,
  onEdit,
  onDelete,
}) => {
  const { user } = useAuth();
  const [expandedIds, setExpandedIds] = useState<Record<string, boolean>>({});

  const toggleRow = (id: string) => {
    setExpandedIds((prev) => ({
      ...prev,
      [id]: !prev[id],
    }));
  };

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-12 text-center text-slate-500">
        Ładowanie historii zakupów...
      </div>
    );
  }

  if (items.length === 0) {
    return (
      <IsEmpty
      title='Brak zarejestrowanych zakupów'
      subTitle={user?.role === 'Admin' ? 'Kliknij "Dodaj zakup", aby zarejestrować nową dostawę.' : undefined}
      icon = {ShoppingBagIcon}
      />
    );
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">

          <thead>
            <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase tracking-wider">
              <th className="py-3.5 px-4 w-10"></th>
              <th className="py-3.5 px-4">Dostawca</th>
              <th className="py-3.5 px-4 text-center">Data Zakupu</th>
              <th className="py-3.5 px-4 text-center">Liczba Pozycji</th>
              <th className="py-3.5 px-4 text-center">Całkowity koszt</th>
              <th className="py-3.5 px-4 text-right">Łączna Ilość</th>
              {(onEdit || onDelete) && <th className="py-3.5 px-4 text-right">Akcje</th>}
            </tr>
          </thead>

          <tbody className="divide-y divide-slate-200 text-sm text-slate-700">

            {items.map((item) => {
              const isExpanded = !!expandedIds[item.id];

              return (
                <React.Fragment key={item.id}>

                  {/* GŁÓWNY WIERSZ ZAKUPU */}
                  <tr
                    className="hover:bg-slate-50/80 transition-colors cursor-pointer"
                    onClick={() => toggleRow(item.id)}
                  >
                    <td className="py-3.5 px-4 text-slate-400">
                      {isExpanded ? (
                        <ChevronUpIcon className="w-4 h-4 text-amber-600 font-bold" />
                      ) : (
                        <ChevronDownIcon className="w-4 h-4" />
                      )}
                    </td>

                    <td className="py-3.5 px-4 font-semibold text-slate-900">
                      {item.supplierName}
                    </td>

                    <td className="py-3.5 px-4 text-center text-slate-500">
                      {item.date}
                    </td>

                    <td className="py-3.5 px-4 text-center font-medium text-slate-700">
                      {item.totalItemsCount}
                    </td>

                    <td className="py-3.5 px-4 text-center font-bold text-emerald-600">
                      {item.totalPrice.toLocaleString("pl-Pl" , {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2,
                      })} zł
                    </td>

                    <td className="py-3.5 px-4 text-right font-bold text-amber-600">
                      {item.totalQuantity}
                    </td>
                    {(onEdit || onDelete) && <td className="py-3.5 px-4 text-right whitespace-nowrap">
                      {onEdit && <button type="button" aria-label={`Edytuj zakup od ${item.supplierName}`} title="Edytuj zakup"
                        onClick={event => { event.stopPropagation(); onEdit(item); }}
                        className="p-2 rounded-lg text-slate-500 hover:text-amber-600 hover:bg-amber-50">
                        <PencilSquareIcon className="w-5 h-5" />
                      </button>}
                      {onDelete && <button type="button" aria-label={`Usuń fakturę zakupu od ${item.supplierName}`} title="Usuń fakturę zakupu"
                        onClick={event => { event.stopPropagation(); onDelete(item); }}
                        className="p-2 rounded-lg text-slate-500 hover:text-red-600 hover:bg-red-50">
                        <TrashIcon className="w-5 h-5" />
                      </button>}
                    </td>}
                  </tr>

                  {/* ROZWINIĘTE POZYCJE ZAKUPU */}
                  {isExpanded && (
                    <tr className="bg-slate-50/60 border-b border-slate-200">
                      <td colSpan={onEdit || onDelete ? 7 : 6} className="py-3 px-6">

                        <div className="pl-6 border-l-2 border-amber-400 space-y-2">

                          <p className="text-xs font-bold text-slate-500 uppercase tracking-wider">
                            Zakupione pozycje sprzętowe ({item.items.length}):
                          </p>

                          {item.items.length === 0 ? (
                            <p className="text-xs text-slate-400 italic">
                              Brak pozycji na liście.
                            </p>
                          ) : (

                            <div className="bg-white rounded-lg border border-slate-200 overflow-hidden max-w-3xl">

                              {/* NAGŁÓWKI */}
                              <div className="grid grid-cols-[1.6fr_1fr_1fr_1fr] gap-4 px-4 py-2 bg-slate-50 border-b border-slate-200 text-[10px] font-bold text-slate-400 uppercase tracking-wider">
                                <span>Sprzęt</span>
                                <span>Ilość</span>
                                <span>Cena / szt.</span>
                                <span>Razem</span>
                              </div>

                              {/* POZYCJE */}
                              <div className="divide-y divide-slate-100">

                                {item.items.map((subItem, index) => (
                                  <div
                                    key={index}
                                    className="grid grid-cols-[1.6fr_1fr_1fr_1fr] items-center gap-4 px-4 py-2.5 text-xs"
                                  >
                                    <span className="font-semibold text-slate-800">
                                      {subItem.equipmentName}
                                    </span>

                                    <span className="font-semibold text-slate-700">
                                      {subItem.quantity} {subItem.unit}
                                    </span>

                                    <span className="font-semibold text-slate-700">
                                      {subItem.purchaseItemPrice.toLocaleString("pl-PL", {
                                    minimumFractionDigits: 2,
                                    maximumFractionDigits: 2,
                                  })} zł
                                      


                                    </span>

                                    <span className="font-bold text-emerald-600">
                                      {subItem.purchaseItemTotalPrice.toLocaleString("pl-PL", {
                                        minimumFractionDigits: 2,
                                        maximumFractionDigits: 2,
                                      })} zł
                                    </span>
                                  </div>
                                ))}

                              </div>
                            </div>
                          )}
                        </div>
                      </td>
                    </tr>
                  )}

                </React.Fragment>
              );
            })}

          </tbody>
        </table>
      </div>
    </div>
  );
};
