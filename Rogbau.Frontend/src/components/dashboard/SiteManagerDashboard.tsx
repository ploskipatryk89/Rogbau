import { CubeIcon, ArrowDownTrayIcon, ArrowUpTrayIcon } from '@heroicons/react/24/outline';
import type { SiteManagerDashboardDto } from '../../types/response/DashboardResponseDto';
import { SiteStatusLabels } from '../../types/enums/SiteStatus';
import { DashboardStatCard } from './DashboardStatCard';
import { DashboardQuickActions } from './DashboardQuickActions';
import { RecentDocumentsList } from './RecentDocumentsList';

interface Props {
  data: SiteManagerDashboardDto;
  onSiteChange: (siteId: string) => void;
  onRefresh: () => void;
}

export function SiteManagerDashboard({ data, onSiteChange, onRefresh }: Props) {
  const selectedSite = data.managerSites.find(site => site.id === data.selectedSiteId);

  if (data.managerSites.length === 0) {
    return <div className="bg-white rounded-xl border border-slate-200 p-8 text-slate-500">Nie masz jeszcze przypisanej budowy. Administrator może przypisać Cię do budowy.</div>;
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end gap-4">
        <div className="w-full sm:w-72">
          <label htmlFor="dashboard-site" className="block mb-2 text-sm font-semibold text-slate-600">Twoja budowa</label>
          <select id="dashboard-site" value={data.selectedSiteId ?? ''} onChange={event => onSiteChange(event.target.value)} className="w-full bg-white rounded-xl border border-slate-200 px-3 py-2.5 text-sm focus:outline-none focus:border-amber-500">
            {data.managerSites.map(site => <option key={site.id} value={site.id}>{site.siteName}</option>)}
          </select>
        </div>
        {data.status !== null && <span className="mb-1 rounded-lg bg-slate-100 px-3 py-2 text-xs font-semibold text-slate-600">{SiteStatusLabels[data.status]}</span>}
      </div>
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <DashboardStatCard title="Sprzęt na budowie" value={data.siteEquipmentQuantity} unit="szt." icon={<CubeIcon className="w-6 h-6" />} />
        <DashboardStatCard title="Dostawy WZ w tym miesiącu" value={data.wzQuantityThisMonth} icon={<ArrowDownTrayIcon className="w-6 h-6" />} />
        <DashboardStatCard title="Zwroty PZ w tym miesiącu" value={data.pzQuantityThisMonth} icon={<ArrowUpTrayIcon className="w-6 h-6" />} />
      </div>
      <DashboardQuickActions dashboardType={data.dashboardType} selectedSiteId={data.selectedSiteId} onSuccess={onRefresh} />
      <RecentDocumentsList items={data.recentlyDocuments} title={`Ostatnie dokumenty — ${selectedSite?.siteName ?? 'Twoja budowa'}`} />
    </div>
  );
}
