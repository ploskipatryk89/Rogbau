import { SearchableSelect } from '../ui/SearchableSelect';
import { ErrorMessage } from "../ui/ErrorMessage";
import { useEffect, useState } from 'react';
import { XMarkIcon, TrashIcon } from '@heroicons/react/24/outline';
import { getMySites } from '../../api/siteService';
import { SiteStatus } from '../../types/enums/SiteStatus';
import { getAllEquipments } from '../../api/equipmentService';
import { createOrder } from '../../api/orderService';
import type { EquipmentResponse } from '../../types/response/EquipmentResponse';

interface Props { isOpen: boolean; onClose: () => void; onSuccess: () => void; initialSiteId?: string }

export const CreateOrderModal = ({ isOpen, onClose, onSuccess, initialSiteId }: Props) => {
  const [sites, setSites] = useState<{ id: string; name: string }[]>([]);
  const [equipment, setEquipment] = useState<EquipmentResponse[]>([]);
  const [siteId, setSiteId] = useState('');
  const [note, setNote] = useState('');
  const [selectedId, setSelectedId] = useState('');
  const [items, setItems] = useState<{ equipmentId: string; quantity: number }[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;
    setLoading(true);
    setError(null);
    const load = async () => {
      try {
        const [mySites, equipments] = await Promise.all([getMySites(), getAllEquipments({})]);
        const availableSites = mySites.filter(site => site.status !== SiteStatus.Completed);
        setSites(availableSites); setEquipment(equipments.filter(x => x.isActive));
        if (initialSiteId && availableSites.some(site => site.id === initialSiteId)) setSiteId(initialSiteId);
        else if (availableSites.length === 1) setSiteId(availableSites[0].id);
        else setSiteId('');
      } catch (err) { setError(err instanceof Error ? err.message : 'Nie udało się pobrać danych.'); }
      finally { setLoading(false); }
    };
    void load();
  }, [isOpen, initialSiteId]);

  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!siteId || items.length === 0 || items.some(x => !Number.isInteger(x.quantity) || x.quantity <= 0)) {
      setError('Wybierz budowę i dodaj sprzęt z dodatnią, całkowitą ilością.'); return;
    }
    setSaving(true); setError(null);
    try { await createOrder({ siteId, note, items }); onSuccess(); onClose(); }
    catch (err) { setError(err instanceof Error ? err.message : 'Nie udało się utworzyć zamówienia.'); }
    finally { setSaving(false); }
  };

  if (!isOpen) return null;
  const field = 'w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500';
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-3xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
          <div><h3 className="text-lg font-bold text-slate-900">Zamów sprzęt</h3><p className="text-sm text-slate-500">Zamówienie z magazynu dla budowy</p></div>
          <button type="button" onClick={onClose} disabled={saving} className="p-1 text-slate-400 hover:text-slate-600" aria-label="Zamknij"><XMarkIcon className="w-5 h-5" /></button>
        </div>
        <form onSubmit={submit} className="p-6 space-y-5">
          {error && <ErrorMessage message={error} className="p-3" />}
          {loading ? <p className="text-slate-500">Ładowanie danych...</p> : <>
            <div><label htmlFor="order-site" className="block text-xs font-semibold text-slate-600 mb-1">BUDOWA</label>
              <select id="order-site" className={field} required value={siteId} onChange={e => setSiteId(e.target.value)}>
                <option value="">Wybierz budowę...</option>{sites.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>{sites.length === 0 && <p className="text-sm text-slate-500 mt-2">Nie masz przypisanej budowy, dla której można zamówić sprzęt.</p>}
            </div>
            <div><label htmlFor="order-equipment" className="block text-xs font-semibold text-slate-600 mb-1">DODAJ SPRZĘT</label>
              <div className="flex gap-3"><SearchableSelect inputId="order-equipment" value={selectedId} onChange={setSelectedId} placeholder="Wyszukaj sprzęt..." options={equipment.filter(e => !items.some(i => i.equipmentId === e.id)).map(e => ({ value: e.id, label: `${e.name} (${e.unit})` }))} /><button type="button" disabled={!selectedId} className="px-4 py-2 bg-slate-900 text-white rounded-xl disabled:opacity-50" onClick={() => {
                setItems([...items, { equipmentId: selectedId, quantity: 1 }]); setSelectedId('');
              }}>Dodaj</button></div>
              <p className="text-xs text-slate-500 mt-2">Możesz zamówić więcej sprzętu niż jest obecnie dostępne w magazynie.</p>
            </div>
            {items.length > 0 && <div className="border border-slate-200 rounded-xl overflow-x-auto"><table className="w-full text-left text-sm">
              <thead className="bg-slate-50 text-slate-500"><tr><th className="p-3">Sprzęt</th><th className="p-3">Ilość zamawiana</th><th className="p-3">Akcje</th></tr></thead>
              <tbody>{items.map(item => <tr key={item.equipmentId} className="border-t border-slate-200">
                <td className="p-3 font-semibold">{equipment.find(x => x.id === item.equipmentId)?.name}</td>
                <td className="p-3"><input type="number" required min="1" step="1" aria-label="Ilość zamawiana" className={`${field} max-w-28`} value={item.quantity} onChange={e => setItems(items.map(x => x.equipmentId === item.equipmentId ? { ...x, quantity: Number(e.target.value) } : x))} /></td>
                <td className="p-3"><button type="button" aria-label="Usuń sprzęt" onClick={() => setItems(items.filter(x => x.equipmentId !== item.equipmentId))}><TrashIcon className="w-4 h-4 text-slate-400 hover:text-red-600" /></button></td>
              </tr>)}</tbody></table></div>}
            <div><label htmlFor="order-note" className="block text-xs font-semibold text-slate-600 mb-1">UWAGA (OPCJONALNIE)</label><textarea id="order-note" className={field} maxLength={1000} value={note} onChange={e => setNote(e.target.value)} /></div>
          </>}
          <div className="flex justify-end gap-3 pt-4 border-t border-slate-200">
            <button type="button" disabled={saving} onClick={onClose} className="px-4 py-2 text-sm text-slate-600 rounded-xl hover:bg-slate-100">Anuluj</button>
            <button type="submit" disabled={saving || loading || sites.length === 0} className="px-4 py-2 bg-amber-500 hover:bg-amber-600 text-slate-950 rounded-xl font-bold text-sm disabled:opacity-50">{saving ? 'Zapisywanie...' : 'Zamów sprzęt'}</button>
          </div>
        </form>
      </div>
    </div>
  );
};
