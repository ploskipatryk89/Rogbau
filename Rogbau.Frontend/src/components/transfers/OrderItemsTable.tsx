import type { OrderItem } from '../../api/orderService';

interface Props {
  items: OrderItem[];
  showAvailable: boolean;
}

export function OrderItemsTable({ items, showAvailable }: Props) {
  return (
    <div className="bg-white rounded-xl border border-slate-200 overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead className="bg-slate-50 text-slate-500">
          <tr>
            <th className="p-4">Sprzęt</th>
            <th className="p-4">Zamówiono</th>
            <th className="p-4">Wysłano</th>
            <th className="p-4">Pozostało</th>
            {showAvailable && <th className="p-4">Dostępne w magazynie</th>}
          </tr>
        </thead>
        <tbody>
          {items.map(item => (
            <tr key={item.equipmentId} className="border-t border-slate-200">
              <td className="p-4 font-semibold">{item.equipmentName}</td>
              <td className="p-4">{item.quantity} {item.unit}</td>
              <td className="p-4">{item.sentQuantity} {item.unit}</td>
              <td className="p-4">{Math.max(0, item.quantity - item.sentQuantity)} {item.unit}</td>
              {showAvailable && <td className="p-4">{item.quantityAvailable} {item.unit}</td>}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
