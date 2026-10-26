import { OrderItemsTable } from "../components/transfers/OrderItemsTable";
import { OrderTransfersList } from "../components/transfers/OrderTransfersList";
import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { getOrderById, orderStatusLabels } from '../api/orderService';
import type { OrderDetails } from '../api/orderService';
import { useAuth } from '../context/AuthContext';
import { CreateWzModal } from '../components/transfers/CreatWzModal';

export const OrderDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const [order, setOrder] = useState<OrderDetails | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [isWzOpen, setIsWzOpen] = useState(false);
  const canIssue = user?.role === 'Admin' || user?.role === 'Magazynier';

  const loadOrder = async () => {
    if (!id) return;
    setError(null);
    try { setOrder(await getOrderById(id)); }
    catch (err) { setError(err instanceof Error ? err.message : 'Nie udało się pobrać zamówienia.'); }
    finally { setLoading(false); }
  };
  useEffect(() => {
    let active = true;
    if (id) getOrderById(id).then(data => { if (active) { setOrder(data); setError(null); } })
      .catch(err => { if (active) { setOrder(null); setError(err instanceof Error ? err.message : 'Nie udało się pobrać zamówienia.'); } })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [id]);

  return <div className="space-y-6">
    <Link to="/transfers" className="text-sm font-medium text-slate-600 hover:text-slate-900">← Powrót do transferów</Link>
    {error && <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700">{error}</div>}
    {loading || (order && order.id !== id) ? <p className="text-slate-500">Ładowanie zamówienia...</p> : order && <>
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 pb-5">
        <div><h1 className="text-2xl font-bold text-slate-900">{order.orderNumber}</h1><p className="text-slate-500">{order.siteName} · {new Date(order.date).toLocaleDateString('pl-PL')}</p></div>
        <div className="flex items-center gap-3"><span className="px-3 py-2 bg-amber-50 text-amber-700 border border-amber-200 rounded-xl text-sm">{orderStatusLabels[order.status]}</span>
          {canIssue && order.status !== 3 && order.items.some(x => x.quantity > x.sentQuantity) && <button type="button" onClick={() => setIsWzOpen(true)} className="px-4 py-2 bg-amber-500 hover:bg-amber-600 rounded-xl font-bold text-sm">Utwórz WZ</button>}
        </div>
      </div>
      {order.note && <div className="p-4 bg-white border border-slate-200 rounded-xl whitespace-pre-wrap"><p className="text-xs font-semibold text-slate-500 mb-2">UWAGA</p>{order.note}</div>}
      <OrderItemsTable items={order.items} showAvailable={canIssue} />
      <OrderTransfersList transfers={order.transfers} />
      {canIssue && <CreateWzModal isOpen={isWzOpen} onClose={() => setIsWzOpen(false)} onSuccess={() => { void loadOrder(); }} order={order} />}
    </>}
  </div>;
};
