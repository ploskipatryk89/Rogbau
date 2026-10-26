import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import type { TransferDetailsDto } from '../../types/response/TransferResponseDto';
import type { EditMovementItem } from '../../types/request/EditMovementRequest';
import type { EquipmentResponse } from '../../types/response/EquipmentResponse';
import type { SiteDto } from '../../types/response/SiteResponse';
import { getAllEquipments } from '../../api/equipmentService';
import { getSites } from '../../api/siteService';
import { editWz, editTransfer } from '../../api/transferService';
import { SearchableSelect } from '../ui/SearchableSelect';
import { ModalHeader } from '../ui/ModalHeader';
import { FormActions } from '../ui/FormActions';
import { ErrorMessage } from '../ui/ErrorMessage';

interface EditMovementModalProps {
  transfer: TransferDetailsDto;
  onClose: () => void;
  onSuccess: () => void;
}

// WZ i przesunięcie mają takie same pozycje. Przesunięcie ma dodatkowo budowę źródłową.
export function EditMovementModal({ transfer, onClose, onSuccess }: EditMovementModalProps) {
  const isTransfer = transfer.type === 3;
  const [sourceId, setSourceId] = useState(transfer.fromConstructionSiteId || '');
  const [targetId, setTargetId] = useState(transfer.toConstructionSiteId || '');
  const [keepOrder, setKeepOrder] = useState(Boolean(transfer.orderId));
  const [note, setNote] = useState(transfer.note || '');
  const [items, setItems] = useState<EditMovementItem[]>(transfer.items.map(item => ({
    equipmentId: item.equipmentId, quantity: item.quantity,
  })));
  const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);
  const [sites, setSites] = useState<SiteDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [loaded, setLoaded] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const inputClass = 'w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white';

  useEffect(() => {
    let active = true;
    async function load() {
      try {
        const data = await Promise.all([getAllEquipments(), getSites()]);
        if (active) {
          setEquipments(data[0]);
          setSites(data[1]);
          setLoaded(true);
        }
      } catch (err) {
        if (active) setError(err instanceof Error ? err.message : 'Nie udało się pobrać danych.');
      } finally {
        if (active) setLoading(false);
      }
    }
    load();
    return () => { active = false; };
  }, []);

  function changeEquipment(index: number, equipmentId: string) {
    const updated = [...items];
    updated[index] = { ...updated[index], equipmentId };
    setItems(updated);
  }

  function changeQuantity(index: number, quantity: number) {
    const updated = [...items];
    updated[index] = { ...updated[index], quantity };
    setItems(updated);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (saving || !loaded) return;
    setError('');
    if (!targetId || (isTransfer && !sourceId) || items.length === 0) {
      setError('Wybierz budowy i dodaj przynajmniej jedną pozycję.');
      return;
    }
    if (isTransfer && sourceId === targetId) {
      setError('Budowa źródłowa i docelowa muszą być różne.');
      return;
    }
    const ids: string[] = [];
    for (const item of items) {
      if (!item.equipmentId || ids.includes(item.equipmentId)) {
        setError('Wybierz sprzęt bez powtarzających się pozycji.');
        return;
      }
      ids.push(item.equipmentId);
      if (!Number.isInteger(item.quantity) || item.quantity <= 0 || item.quantity > 2147483647) {
        setError('Podaj poprawną dodatnią ilość sprzętu.');
        return;
      }
    }
    try {
      setSaving(true);
      if (isTransfer) {
        await editTransfer(transfer.id, { fromConstructionSiteId: sourceId, toConstructionSiteId: targetId, note, items });
      } else {
        let orderId: string | null = null;
        if (keepOrder) orderId = transfer.orderId;
        await editWz(transfer.id, { toConstructionSiteId: targetId, orderId, note, items });
      }
      onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Nie udało się zapisać dokumentu.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4 print:hidden">
      <div role="dialog" aria-modal="true" aria-label={`Edytuj ${transfer.number}`} className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-5xl max-h-[90vh] overflow-y-auto">
        <ModalHeader title={`Edytuj ${transfer.number}`} description={isTransfer ? "Korekta przesunięcia sprzętu między budowami" : "Korekta wydania sprzętu z magazynu na budowę"} onClose={onClose} disabled={saving} />
        <form onSubmit={submit} className="p-6 space-y-5">
          <p className="rounded-xl border border-amber-200 bg-amber-50/40 p-3 text-sm text-slate-600">Zmiany skorygują aktualne stany. Korekta przydziałów obowiązuje od dnia edycji; wcześniejsze okresy naliczania kosztów pozostają bez zmian.</p>
          {error && <div role="alert"><ErrorMessage message={error} className="p-3" /></div>}
          {loading && <div className="py-8 text-center text-sm text-slate-500">Ładowanie danych...</div>}
          <fieldset disabled={!loaded || saving} className="space-y-4 disabled:opacity-60">
            <div className="grid gap-4 md:grid-cols-2">
              {isTransfer && <div>
                <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">Budowa źródłowa</label>
                <SearchableSelect value={sourceId} onChange={setSourceId} options={sites.map(site => ({ value: site.id, label: site.name }))} placeholder="Wybierz budowę..." />
              </div>}
              <div>
                <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">Budowa docelowa</label>
                <SearchableSelect value={targetId} onChange={setTargetId} options={sites.map(site => ({ value: site.id, label: site.name }))} placeholder="Wybierz budowę..." />
              </div>
            </div>
            {!isTransfer && transfer.orderId && <div className="rounded-lg bg-amber-50 p-3 text-sm">
              <label className="flex gap-2 font-semibold"><input type="checkbox" checked={keepOrder} onChange={e => setKeepOrder(e.target.checked)} />Zachowaj powiązanie z zamówieniem</label>
              <p className="mt-1 text-slate-600">Jeśli poprawiony sprzęt, ilość lub budowa nie pasuje do zamówienia, odznacz tę opcję. Zamówienie zostanie ponownie przeliczone.</p>
            </div>}
            {items.map((item, index) => (
              <div key={index} className="space-y-3 rounded-xl border border-slate-200 p-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-semibold">Pozycja {index + 1}</span>
                  <button type="button" onClick={() => setItems(items.filter((_, i) => i !== index))} className="px-2 py-1 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer">Usuń</button>
                </div>
                <SearchableSelect value={item.equipmentId} onChange={value => changeEquipment(index, value)}
                  options={equipments.filter(equipment => equipment.id === item.equipmentId || !items.some(other => other.equipmentId === equipment.id)).map(equipment => ({ value: equipment.id, label: equipment.name + ' (' + equipment.unit + ')' }))}
                  placeholder="Wybierz sprzęt..." />
                <label className="block text-sm">Ilość<input required type="number" min="1" max="2147483647" step="1" value={item.quantity} onChange={e => changeQuantity(index, Number(e.target.value))} className={inputClass} /></label>
              </div>
            ))}
            <button type="button" onClick={() => setItems([...items, { equipmentId: '', quantity: 1 }])} className="px-4 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-xl text-sm font-semibold transition-colors cursor-pointer">Dodaj sprzęt</button>
            <label className="block text-xs font-semibold text-slate-600 mb-1">Uwagi (opcjonalnie)<textarea value={note} onChange={e => setNote(e.target.value)} rows={3} className={inputClass} /></label>
          </fieldset>
          <FormActions onCancel={onClose} submitLabel={saving ? "Zapisywanie..." : "Zapisz zmiany"} submitDisabled={!loaded || saving} cancelDisabled={saving} />
        </form>
      </div>
    </div>
  );
}

