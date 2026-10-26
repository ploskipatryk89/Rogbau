import { TrashIcon } from '@heroicons/react/24/outline';
import type { SiteEquipmentDto } from '../../types/response/SiteResponse';

interface TransferItem {
  equipmentId: string;
  quantity: number;
}

interface Props {
  items: TransferItem[];
  getEquipment: (equipmentId: string) => SiteEquipmentDto | undefined;
  handleQuantityChange: (equipmentId: string, quantity: number) => void;
  handleRemoveItem: (equipmentId: string) => void;
}

export function TransferItemsTable({ items, getEquipment, handleQuantityChange, handleRemoveItem }: Props) {
  return (
    <table className="w-full text-left">
      <thead>
        <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase">
          <th className="px-4 py-3">
            Sprzęt
          </th>

          <th className="px-4 py-3 text-center">
            Na budowie
          </th>

          <th className="px-4 py-3 text-center">
            Ilość
          </th>

          <th className="px-4 py-3 text-right">
            Akcje
          </th>
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

              <td className="px-4 py-3 text-center font-semibold text-emerald-600">
                {equipment.allocatedQuantity}
              </td>

              <td className="px-4 py-3">
                <input
                  type="number"
                  min="1"
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

              <td className="px-4 py-3 text-right">
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
                
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
