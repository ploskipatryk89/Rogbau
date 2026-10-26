export interface PurchaseFiltersType {
  supplierName: string;
  dateFrom: string;
  dateTo: string;
}

interface Props {
  filters: PurchaseFiltersType;
  onChange: (filters: PurchaseFiltersType) => void;
}

export function PurchaseFilters({ filters, onChange }: Props) {
  const fieldClass = 'w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500';
  return <div className="bg-white border border-slate-200 rounded-xl p-4 space-y-3">
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 items-end">
      <div><label htmlFor="purchase-supplier-filter" className="block text-xs font-semibold text-slate-600 mb-1">Dostawca</label><input id="purchase-supplier-filter" type="search" placeholder="Wyszukaj po nazwie dostawcy..." value={filters.supplierName} onChange={e => onChange({ ...filters, supplierName: e.target.value })} className={fieldClass} /></div>
      <div><label htmlFor="purchase-date-from" className="block text-xs font-semibold text-slate-600 mb-1">Data od</label><input id="purchase-date-from" type="date" value={filters.dateFrom} onChange={e => onChange({ ...filters, dateFrom: e.target.value })} className={fieldClass} /></div>
      <div><label htmlFor="purchase-date-to" className="block text-xs font-semibold text-slate-600 mb-1">Data do</label><input id="purchase-date-to" type="date" value={filters.dateTo} onChange={e => onChange({ ...filters, dateTo: e.target.value })} className={fieldClass} /></div>
      <button type="button" onClick={() => onChange({ supplierName: '', dateFrom: '', dateTo: '' })} className="px-4 py-2 border border-slate-200 rounded-xl text-sm font-semibold text-slate-600 hover:bg-slate-50">Wyczyść filtry</button>
    </div>
    {filters.dateFrom && filters.dateTo && filters.dateFrom > filters.dateTo && <p className="text-sm text-red-600">Data od nie może być późniejsza niż data do.</p>}
  </div>;
}
