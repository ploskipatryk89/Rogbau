import { BuildingOfficeIcon } from '@heroicons/react/24/outline';
import type { AdminDashboardDto } from '../../types/response/DashboardResponseDto';
import { DashboardStatCard } from './DashboardStatCard';
import { DashboardQuickActions } from './DashboardQuickActions';
import { RecentDocumentsList } from './RecentDocumentsList';
import { RecentOrdersList } from './RecentOrdersList';
import { LowEquipmentStockList } from './LowEquipmentStockList';

export function AdminDashboard({ data, onRefresh }: { data: AdminDashboardDto; onRefresh: () => void }) {
  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <DashboardStatCard title="Aktywne budowy" value={data.activeSiteQuantity} icon={<BuildingOfficeIcon className="w-6 h-6" />} />
        
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
