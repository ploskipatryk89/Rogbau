import { TrashIcon } from '@heroicons/react/24/outline';
import type { EquipmentResponse } from '../../types/response/EquipmentResponse';
import type { OrderDetails } from '../../api/orderService';

interface WzItem {
  equipmentId: string;
  quantity: number;
}

interface Props {
  items: WzItem[];
  getEquipment: (equipmentId: string) => EquipmentResponse | undefined;
  handleQuantityChange: (equipmentId: string, quantity: number) => void;
  handleRemoveItem: (equipmentId: string) => void;
  order?: OrderDetails;
}

export function WzItemsTable({ items, getEquipment, handleQuantityChange, handleRemoveItem, order }: Props) {
  return (
    <table className="w-full text-left">
      <thead>
        <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase">
          <th className="px-4 py-3">Sprzęt</th>
          {order && <><th className="px-4 py-3">Zamówiono</th><th className="px-4 py-3">Wysłano</th><th className="px-4 py-3">Pozostało</th></>}
          <th className="px-4 py-3 text-center">
            Dostępne
          </th>
          <th className="px-4 py-3 text-center">
            {order ? 'Wyślij teraz' : 'Ilość'}
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
          const ordered = order?.items.find(x => x.equipmentId === item.equipmentId);

          return (
            <tr key={item.equipmentId}>
              <td className="px-4 py-3 font-semibold text-slate-900">
                {equipment.name}
              </td>
              {ordered && <><td className="px-4 py-3">{ordered.quantity}</td><td className="px-4 py-3">{ordered.sentQuantity}</td><td className="px-4 py-3">{ordered.quantity - ordered.sentQuantity}</td></>}

              <td className="px-4 py-3 text-center text-emerald-600 font-semibold">
                {equipment.quantityAvailable}{' '}
                {equipment.unit}
              </td>

              <td className="px-4 py-3">
                <input
                  type="number"
                  min={order ? 0 : 1}
                  step="1"
                  max={Math.min(equipment.quantityAvailable, ordered ? ordered.quantity - ordered.sentQuantity : equipment.quantityAvailable)}
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
                    handleRemoveItem(item.equipmentId)
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
