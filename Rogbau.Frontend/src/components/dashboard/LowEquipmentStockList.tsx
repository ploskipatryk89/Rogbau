import { Link } from 'react-router-dom';
import type { LowEquipmentStockDto } from '../../types/response/DashboardResponseDto';

export function LowEquipmentStockList({ items }: { items: LowEquipmentStockDto[] }) {
  return (
    <section className="bg-white rounded-xl border border-slate-200 overflow-hidden">
      <div className="p-5"><h2 className="text-base font-bold text-slate-900">Niskie stany magazynowe</h2><p className="mt-1 text-xs text-slate-500">Sprzęt z dostępnością do 5 sztuk.</p></div>
      {items.length === 0 ? <p className="px-5 pb-5 text-sm text-slate-500">Brak sprzętu z niskim stanem magazynowym.</p> : (
        <ul className="divide-y divide-slate-100">
          {items.map(item => <li key={item.id} className="px-5 py-3 flex items-center justify-between gap-4">
            <Link to={`/equipment/${item.id}`} className="text-sm text-slate-700 hover:text-amber-600">{item.name}</Link>
            <span className="shrink-0 rounded-lg bg-amber-50 px-2 py-1 text-sm font-semibold text-amber-700">{item.quantity} szt.</span>
          </li>)}
        </ul>
      )}
      <div className="px-5 py-4 border-t border-slate-100"><Link to="/equipment" className="text-sm font-semibold text-amber-600 hover:text-amber-700">Zobacz magazyn →</Link></div>
    </section>
  );
}
