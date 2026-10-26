import { TrashIcon } from '@heroicons/react/24/outline';
import type { SiteEquipmentDto } from '../../types/response/SiteResponse';
import type { TransferDetailsItemDto } from '../../types/response/TransferResponseDto';

interface PzItem {
  equipmentId: string;
  quantity: number;
  quantityScrapped: number;
}

interface Props {
  declaredItems?: TransferDetailsItemDto[];
  items: PzItem[];
  getEquipment: (equipmentId: string) => SiteEquipmentDto | undefined;
  handleQuantityChange: (equipmentId: string, quantity: number) => void;
  handleRemoveItem: (equipmentId: string) => void;
  handleScrappedQuantityChange: (equipmentId: string, quantity: number) => void;
}

export function PzItemsTable({ items, getEquipment, handleQuantityChange, handleRemoveItem, handleScrappedQuantityChange, declaredItems }: Props) {
  return (
    <table className="w-full min-w-[600px] text-left">
      <thead>
        <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase">
          <th className="px-4 py-3">
            Sprzęt
          </th>

          <th className="px-4 py-3 text-center">
            {declaredItems ? 'Zgłoszono' : 'Na budowie'}
          </th>

          <th className="px-4 py-3 text-center">
            {declaredItems ? 'Ilość przyjęta' : 'Ilość zwracana'}
          </th>

          <th className="px-4 py-3 text-center">
            Złom
          </th>

          {!declaredItems && <th className="px-4 py-3 text-right">
            Akcje
          </th>}
        </tr>
      </thead>

      <tbody className="divide-y divide-slate-200 text-sm">
        {items.map((item) => {
          const equipment = getEquipment(
            item.equipmentId
          );

          if (!equipment) return null;

          return (
            <tr key={item.equipmentId}>
              <td className="px-4 py-3">
                <div className="font-semibold text-slate-900">
                  {equipment.equipmentName}
                </div>

                <div className="text-xs text-slate-500">
                  {equipment.unit}
                </div>
              </td>

              {/* Aktualna ilość na budowie */}
              <td className="px-4 py-3 text-center font-semibold text-emerald-600">
                {declaredItems ? declaredItems.find(eq => eq.equipmentId === item.equipmentId)?.quantity : equipment.allocatedQuantity}
                {declaredItems && <p className="text-xs font-normal text-slate-500">Złom: {declaredItems.find(eq => eq.equipmentId === item.equipmentId)?.scrappedQuantity ?? 0}</p>}
              </td>

              {/* Ilość zwracana */}
              <td className="px-4 py-3">
                <input
                  type="number"
                  min={declaredItems ? 0 : 1}
                  max={equipment.allocatedQuantity}
                  value={item.quantity}
                  onChange={(e) =>
                    handleQuantityChange(
                      item.equipmentId,
                      Number(e.target.value)
                    )
                  }
                  className="w-24 mx-auto block px-3 py-2 border border-slate-200 rounded-xl text-sm text-center focus:outline-none focus:border-amber-500"
                />
              </td>

              {/* Złom */}
              <td className="px-4 py-3">
                <input
                  type="number"
                  min="0"
                  max={item.quantity}
                  value={item.quantityScrapped}
                  onChange={(e) =>
                    handleScrappedQuantityChange(
                      item.equipmentId,
                      Number(e.target.value)
                    )
                  }
                  className="w-24 mx-auto block px-3 py-2 border border-slate-200 rounded-xl text-sm text-center focus:outline-none focus:border-red-400"
                />
              </td>

              {!declaredItems && <td className="px-4 py-3 text-right">
                <button
                  type="button"
                  onClick={() =>
                    handleRemoveItem(
                      item.equipmentId
                    )
                  }
                  className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                  title="Usuń"
                >
                  <TrashIcon className="w-4 h-4" />
                </button>
              </td>}
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
