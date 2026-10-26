import { Link } from 'react-router-dom';
import { orderStatusLabels } from '../../api/orderService';
import type { RecentlyOrderDto } from '../../types/response/DashboardResponseDto';

export function RecentOrdersList({ items }: { items: RecentlyOrderDto[] }) {
  return (
    <section className="bg-white rounded-xl border border-slate-200 overflow-hidden">
      <h2 className="p-5 text-base font-bold text-slate-900">Ostatnie zamówienia</h2>
      {items.length === 0 ? <p className="px-5 pb-5 text-sm text-slate-500">Brak zamówień do wyświetlenia.</p> : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs text-slate-500"><tr><th className="px-5 py-3">Numer</th><th className="px-5 py-3">Budowa</th><th className="px-5 py-3">Status</th></tr></thead>
            <tbody className="divide-y divide-slate-100">
              {items.map(item => <tr key={item.id} className={item.status === 1 || item.status === 4 ? 'bg-amber-50/60 hover:bg-amber-50 transition-colors' : 'hover:bg-slate-50 transition-colors'}>
                <td className={`px-5 py-3 ${item.status === 1 || item.status === 4 ? 'font-bold' : 'font-semibold'}`}><Link to={`/orders/${item.id}`} className={item.status === 1 || item.status === 4 ? 'text-amber-800 hover:text-amber-600' : 'text-slate-900 hover:text-amber-600'}>{item.number}</Link></td>
                <td className="px-5 py-3 text-slate-600">{item.siteName}</td>
                <td className={`px-5 py-3 ${item.status === 1 || item.status === 4 ? 'font-semibold text-amber-700' : 'text-slate-500'}`}>{orderStatusLabels[item.status]}</td>
              </tr>)}
            </tbody>
          </table>
        </div>
      )}
      <div className="px-5 py-4 border-t border-slate-100"><Link to="/transfers?view=orders" className="text-sm font-semibold text-amber-600 hover:text-amber-700">Wszystkie zamówienia →</Link></div>
    </section>
  );
}
