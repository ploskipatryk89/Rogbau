import { useEffect, useState } from 'react';
import { ArrowPathIcon } from '@heroicons/react/24/outline';
import { useAuth } from '../context/AuthContext';
import { getDashboard } from '../api/dashboardService';
import type { DashboardResponseDto } from '../types/response/DashboardResponseDto';
import { AdminDashboard } from '../components/dashboard/AdminDashboard';
import { WarehousemanDashboard } from '../components/dashboard/WarehousemanDashboard';
import { SiteManagerDashboard } from '../components/dashboard/SiteManagerDashboard';
import { ErrorMessage } from '../components/ui/ErrorMessage';

export function DashboardPage() {
  const { user } = useAuth();
  const [data, setData] = useState<DashboardResponseDto | null>(null);
  const [selectedSiteId, setSelectedSiteId] = useState<string>();
  const [refresh, setRefresh] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function loadDashboard() {
      setLoading(true);
      setError(null);
      try {
        const result = await getDashboard(selectedSiteId);
        if (!cancelled) setData(result);
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Nie udało się pobrać pulpitu.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    void loadDashboard();
    return () => { cancelled = true; };
  }, [selectedSiteId, refresh, user?.id]);

  function refreshDashboard() {
    setRefresh(value => value + 1);
  }

  let subtitle = 'Podsumowanie Twojej budowy.';
  if (user?.role === 'Admin') subtitle = 'Oto krótkie podsumowanie firmy.';
  if (user?.role === 'Magazynier') subtitle = 'Najważniejsze informacje z magazynu głównego.';

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Dzień dobry, {user?.firstName || user?.fullName || 'Użytkowniku'}!</h1>
          <p className="mt-1 text-sm text-slate-500">{subtitle}</p>
        </div>
        <div className="flex items-center gap-3">
          <span className="text-sm text-slate-500">{new Date().toLocaleDateString('pl-PL', { day: 'numeric', month: 'long', year: 'numeric' })}</span>
          <button type="button" onClick={refreshDashboard} disabled={loading} aria-label="Odśwież pulpit" className="p-2.5 bg-white rounded-xl border border-slate-200 text-slate-500 hover:text-amber-600 disabled:opacity-50">
            <ArrowPathIcon className={`w-5 h-5 ${loading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>
      {error && <ErrorMessage message={error} className="p-4" />}
      {loading && <div className="bg-white rounded-xl border border-slate-200 p-8 text-center text-slate-500">Ładowanie pulpitu...</div>}
      {!loading && !error && data?.dashboardType === 'admin' && <AdminDashboard data={data} onRefresh={refreshDashboard} />}
      {!loading && !error && data?.dashboardType === 'warehouseman' && <WarehousemanDashboard data={data} onRefresh={refreshDashboard} />}
      {!loading && !error && data?.dashboardType === 'siteManager' && <SiteManagerDashboard data={data} onSiteChange={setSelectedSiteId} onRefresh={refreshDashboard} />}
    </div>
  );
}
