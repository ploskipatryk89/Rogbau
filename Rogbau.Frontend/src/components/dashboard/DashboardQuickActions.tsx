import { useState } from 'react';
import { Link } from 'react-router-dom';
import { CreateWzModal } from '../transfers/CreatWzModal';
import { CreatePzModal } from '../transfers/CreatePzModal';
import { CreateTransferModal } from '../transfers/CreateTransferModal';
import { CreateOrderModal } from '../transfers/CreateOrderModal';
import { CreateSiteModal } from '../sites/CreateSiteModal';
import type { DashboardResponseDto } from '../../types/response/DashboardResponseDto';

interface Props {
  dashboardType: DashboardResponseDto['dashboardType'];
  selectedSiteId?: string | null;
  onSuccess: () => void;
}

export function DashboardQuickActions({ dashboardType, selectedSiteId, onSuccess }: Props) {
  const [action, setAction] = useState<string | null>(null);
  const canCreateWarehouseDocuments = dashboardType === 'admin' || dashboardType === 'warehouseman';
  const buttonClass = 'px-4 py-2.5 bg-amber-500 hover:bg-amber-600 rounded-xl text-sm font-semibold text-slate-950 disabled:opacity-50 disabled:cursor-not-allowed';
  const linkClass = 'px-4 py-2.5 border border-slate-200 rounded-xl text-sm font-semibold text-slate-600 hover:bg-slate-50';

  return (
    <>
      <section className="bg-white rounded-xl border border-slate-200 p-5">
        <h2 className="text-base font-bold text-slate-900 mb-4">Szybkie akcje</h2>
        <div className="flex flex-wrap gap-3">
          {canCreateWarehouseDocuments && <>
            <button type="button" className={buttonClass} onClick={() => setAction('wz')}>+ Wystaw WZ</button>
            <button type="button" className={buttonClass} onClick={() => setAction('transfer')}>+ Przesunięcie</button>
          </>}
          {dashboardType === 'admin' && <button type="button" className={linkClass} onClick={() => setAction('site')}>+ Dodaj budowę</button>}
          {dashboardType === 'siteManager' && <>
            <button type="button" className={buttonClass} disabled={!selectedSiteId} onClick={() => setAction('pz')}>+ Zwróć sprzęt (PZ)</button>
            <button type="button" className={buttonClass} disabled={!selectedSiteId} onClick={() => setAction('order')}>+ Zamów sprzęt</button>
            <button type="button" className={linkClass} disabled={!selectedSiteId} onClick={() => setAction('transfer')}>+ Przesunięcie</button>
            <Link to="/sites" className={linkClass}>Moje budowy i sprzęt</Link>
          </>}
          <Link to="/equipment" className={linkClass}>Stan magazynu</Link>
        </div>
      </section>

      {canCreateWarehouseDocuments && action === 'wz' && <CreateWzModal isOpen onClose={() => setAction(null)} onSuccess={onSuccess} />}
      {dashboardType === 'siteManager' && action === 'pz' && <CreatePzModal isOpen initialSiteId={selectedSiteId ?? undefined} onClose={() => setAction(null)} onSuccess={onSuccess} />}
      {action === 'transfer' && <CreateTransferModal isOpen onClose={() => setAction(null)} onSuccess={onSuccess} />}
      {dashboardType === 'admin' && action === 'site' && <CreateSiteModal isOpen onClose={() => setAction(null)} onSuccess={onSuccess} />}
      {dashboardType === 'siteManager' && action === 'order' && <CreateOrderModal isOpen initialSiteId={selectedSiteId ?? undefined} onClose={() => setAction(null)} onSuccess={onSuccess} />}
    </>
  );
}
