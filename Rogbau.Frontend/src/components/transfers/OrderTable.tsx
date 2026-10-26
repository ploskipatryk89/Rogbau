import { orderStatusLabels } from '../../api/orderService';
import type { OrderDetails } from '../../api/orderService';

interface Props {
  items: OrderDetails[];
  isLoading: boolean;
  onViewDetails: (id: string) => void;
}

export function OrderTable({ items, isLoading, onViewDetails }: Props) {
  return (
    <div className="bg-white rounded-xl border border-slate-200 overflow-x-auto">
      {isLoading ? (
        <p className="p-8 text-center text-slate-500">Ładowanie zamówień...</p>
      ) : items.length === 0 ? (
        <p className="p-8 text-center text-slate-500">Brak zamówień.</p>
      ) : (
        <table className="w-full text-left text-sm">
          <thead className="bg-slate-50 text-slate-500">
            <tr>
              <th className="p-4">Numer</th>
              <th className="p-4">Budowa</th>
              <th className="p-4">Data</th>
              <th className="p-4">Status</th>
              <th className="p-4">Akcje</th>
            </tr>
          </thead>
          <tbody>
            {items.map(order => (
              <tr key={order.id} className={`border-t border-slate-200 ${order.status === 1 || order.status === 4 ? 'bg-amber-50/60 hover:bg-amber-50' : 'hover:bg-slate-50'} transition-colors`}>
                <td className={`p-4 ${order.status === 1 || order.status === 4 ? 'font-bold text-amber-800' : 'font-semibold'}`}>{order.orderNumber}</td>
                <td className="p-4">{order.siteName}</td>
                <td className="p-4">{new Date(order.date).toLocaleDateString('pl-PL')}</td>
                <td className={`p-4 ${order.status === 1 || order.status === 4 ? 'font-semibold text-amber-700' : ''}`}>{orderStatusLabels[order.status]}</td>
                <td className="p-4">
                  <button type="button" className="text-amber-700 hover:underline" onClick={() => onViewDetails(order.id)}>
                    Szczegóły
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
