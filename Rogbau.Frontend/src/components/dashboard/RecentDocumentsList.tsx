import { Link } from 'react-router-dom';
import type { RecentlyDocumentDto } from '../../types/response/DashboardResponseDto';

interface Props {
  items: RecentlyDocumentDto[];
  title?: string;
}

const statusLabels: Record<number, string> = {
  1: 'Szkic', 2: 'Oczekuje na odbiór', 3: 'Zakończony', 4: 'Anulowany',
};

export function RecentDocumentsList({ items, title = 'Ostatnie dokumenty' }: Props) {
  return (
    <section className="bg-white rounded-xl border border-slate-200 overflow-hidden">
      <h2 className="p-5 text-base font-bold text-slate-900">{title}</h2>
      {items.length === 0 ? <p className="px-5 pb-5 text-sm text-slate-500">Brak dokumentów do wyświetlenia.</p> : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs text-slate-500">
              <tr><th className="px-5 py-3">Dokument</th><th className="px-5 py-3">Trasa</th><th className="px-5 py-3">Data</th><th className="px-5 py-3">Status</th></tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {items.map(item => (
                <tr key={item.id} className={item.status === 2 ? 'bg-amber-50/60 hover:bg-amber-50' : 'hover:bg-slate-50'}>
                  <td className="px-5 py-3 font-semibold whitespace-nowrap"><Link className="text-slate-900 hover:text-amber-600" to={`/transfers/${item.id}`}>{item.number}</Link></td>
                  <td className="px-5 py-3 text-slate-600">{item.from} <span className="text-slate-400">→</span> {item.to}</td>
                  <td className="px-5 py-3 text-slate-500 whitespace-nowrap">{new Date(item.date).toLocaleDateString('pl-PL')}</td>
                  <td className={`px-5 py-3 whitespace-nowrap ${item.status === 2 ? 'font-bold text-amber-700' : 'text-slate-500'}`}>{statusLabels[item.status]}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <div className="px-5 py-4 border-t border-slate-100"><Link to="/transfers" className="text-sm font-semibold text-amber-600 hover:text-amber-700">Wszystkie dokumenty →</Link></div>
    </section>
  );
}
