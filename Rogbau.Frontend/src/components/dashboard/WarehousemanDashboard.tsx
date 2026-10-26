import { CubeIcon, ArrowUpTrayIcon, ArrowDownTrayIcon } from '@heroicons/react/24/outline';
import type { WarehousemanDashboardDto } from '../../types/response/DashboardResponseDto';
import { DashboardStatCard } from './DashboardStatCard';
import { DashboardQuickActions } from './DashboardQuickActions';
import { RecentDocumentsList } from './RecentDocumentsList';
import { RecentOrdersList } from './RecentOrdersList';
import { LowEquipmentStockList } from './LowEquipmentStockList';

export function WarehousemanDashboard({ data, onRefresh }: { data: WarehousemanDashboardDto; onRefresh: () => void }) {
  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <DashboardStatCard title="Sprzęt w magazynie" value={data.equipmentAvailable} unit="szt." icon={<CubeIcon className="w-6 h-6" />} />
        <DashboardStatCard title="Wydania WZ w tym miesiącu" value={data.wzQuantityThisMonth} icon={<ArrowUpTrayIcon className="w-6 h-6" />} />
        <DashboardStatCard title="Przyjęcia PZ w tym miesiącu" value={data.pzQuantityThisMonth} icon={<ArrowDownTrayIcon className="w-6 h-6" />} />
      </div>
      <DashboardQuickActions dashboardType={data.dashboardType} onSuccess={onRefresh} />
      <RecentDocumentsList items={data.recentlyDocuments} />
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
        <RecentOrdersList items={data.recentlyOrders} />
        <LowEquipmentStockList items={data.lowEquipmentStocks} />
      </div>
    </div>
  );
}
