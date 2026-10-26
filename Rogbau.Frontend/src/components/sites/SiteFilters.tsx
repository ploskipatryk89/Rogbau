import { SiteStatus, SiteStatusLabels } from '../../types/enums/SiteStatus';

interface Props {
  status: string;
  onStatusChange: (status: string) => void;
}

export function SiteFilters({ status, onStatusChange }: Props) {
  return <div className="bg-white border border-slate-200 rounded-xl p-4">
    <label htmlFor="site-status-filter" className="block text-xs font-semibold text-slate-600 mb-1">Status budowy</label>
    <select id="site-status-filter" value={status} onChange={e => onStatusChange(e.target.value)} className="w-full sm:w-64 px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500">
      <option value="">Wszystkie statusy</option>
      {Object.values(SiteStatus).map(value => <option key={value} value={value}>{SiteStatusLabels[value]}</option>)}
    </select>
  </div>;
}
