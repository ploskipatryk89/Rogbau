import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import type { TransferDetailsDto } from '../../types/response/TransferResponseDto';
import type { EditPzItem } from '../../types/request/EditPzRequest';
import type { EquipmentResponse } from '../../types/response/EquipmentResponse';
import type { SiteDto } from '../../types/response/SiteResponse';
import { getAllEquipments } from '../../api/equipmentService';
import { getSites } from '../../api/siteService';
import { editPz } from '../../api/transferService';
import { SearchableSelect } from '../ui/SearchableSelect';
import { ModalHeader } from '../ui/ModalHeader';
import { FormActions } from '../ui/FormActions';
import { ErrorMessage } from '../ui/ErrorMessage';

interface EditPzModalProps {
  transfer: TransferDetailsDto;
  onClose: () => void;
  onSuccess: () => void;
}

export function EditPzModal({ transfer, onClose, onSuccess }: EditPzModalProps) {
  const [siteId, setSiteId] = useState(transfer.fromConstructionSiteId || '');
  const [note, setNote] = useState(transfer.note || '');
  const [items, setItems] = useState<EditPzItem[]>(transfer.items.map(item => ({
    equipmentId: item.equipmentId,
    quantity: item.quantity,
    scrappedQuantity: item.scrappedQuantity,
    receivedQuantity: item.receivedQuantity,
    receivedScrappedQuantity: item.receivedScrappedQuantity,
  })));
  const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);
  const [sites, setSites] = useState<SiteDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const completed = transfer.status === 3;
  const inputClass = 'w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white';

  useEffect(() => {
    let active = true;
    async function load() {
      try {
        const data = await Promise.all([getAllEquipments(), getSites()]);
        if (active) {
          setEquipments(data[0]);
          setSites(data[1]);
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

  function changeItem(index: number, field: keyof EditPzItem, value: string | number) {
    const updated = [...items];
    updated[index] = { ...updated[index], [field]: value };
    setItems(updated);
  }

  function addItem() {
    setItems([...items, {
      equipmentId: '', quantity: 1, scrappedQuantity: 0,
      receivedQuantity: completed ? 1 : null,
      receivedScrappedQuantity: completed ? 0 : null,
    }]);
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setError('');
    if (!siteId || items.length === 0) {
      setError('Wybierz budowę i dodaj przynajmniej jedną pozycję.');
      return;
    }
    const ids: string[] = [];
    for (const item of items) {
      if (!item.equipmentId || ids.includes(item.equipmentId)) {
        setError('Wybierz sprzęt bez powtarzających się pozycji.');
        return;
      }
      ids.push(item.equipmentId);
      if (!Number.isInteger(item.quantity) || item.quantity <= 0 ||
          !Number.isInteger(item.scrappedQuantity) || item.scrappedQuantity < 0 || item.scrappedQuantity > item.quantity) {
        setError('Sprawdź ilości zgłoszone i zgłoszony złom.');
        return;
      }
      if (completed && (item.receivedQuantity === null || item.receivedScrappedQuantity === null ||
          !Number.isInteger(item.receivedQuantity) || !Number.isInteger(item.receivedScrappedQuantity) ||
          item.receivedQuantity < 0 || item.receivedScrappedQuantity < 0 || item.receivedScrappedQuantity > item.receivedQuantity)) {
        setError('Sprawdź ilości przyjęte i przyjęty złom.');
        return;
      }
    }
    try {
      setSaving(true);
      await editPz(transfer.id, { fromConstructionSiteId: siteId, note, items });
      onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Nie udało się zapisać PZ.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4 print:hidden">
      <div role="dialog" aria-modal="true" aria-label={`Edytuj ${transfer.number}`} className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-5xl max-h-[90vh] overflow-y-auto">
        <ModalHeader title={`Edytuj ${transfer.number}`} description="Korekta przyjęcia sprzętu do magazynu" onClose={onClose} disabled={saving} />
        <form onSubmit={submit} className="p-6 space-y-5">
          <p className="rounded-xl border border-amber-200 bg-amber-50/40 p-3 text-sm text-slate-600">Zmiany skorygują aktualne stany. Korekta przydziałów obowiązuje od dnia edycji; wcześniejsze okresy naliczania kosztów pozostają bez zmian.</p>
          {error && <div role="alert"><ErrorMessage message={error} className="p-3" /></div>}
          {loading && <div className="py-8 text-center text-sm text-slate-500">Ładowanie danych...</div>}
          <fieldset disabled={loading || saving} className="space-y-4 disabled:opacity-60">
            <div>
              <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">Budowa źródłowa</label>
              <SearchableSelect value={siteId} onChange={setSiteId} options={sites.map(site => ({ value: site.id, label: site.name }))} placeholder="Wybierz budowę..." />
            </div>
            {items.map((item, index) => (
              <div key={index} className="rounded-xl border border-slate-200 p-4 space-y-3">
                <div className="flex items-center justify-between">
                  <span className="text-sm font-semibold">Pozycja {index + 1}</span>
                  <button type="button" onClick={() => setItems(items.filter((_, i) => i !== index))} className="px-2 py-1 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer">Usuń</button>
                </div>
                <SearchableSelect value={item.equipmentId} onChange={value => changeItem(index, 'equipmentId', value)}
                  options={equipments.filter(equipment => equipment.id === item.equipmentId || !items.some(other => other.equipmentId === equipment.id)).map(equipment => ({ value: equipment.id, label: equipment.name + ' (' + equipment.unit + ')' }))}
                  placeholder="Wybierz sprzęt..." />
                <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
                  <label className="text-xs">Zgłoszona ilość<input required type="number" min="1" step="1" value={item.quantity} onChange={e => changeItem(index, 'quantity', Number(e.target.value))} className={inputClass} /></label>
                  <label className="text-xs">Zgłoszony złom<input required type="number" min="0" max={item.quantity} step="1" value={item.scrappedQuantity} onChange={e => changeItem(index, 'scrappedQuantity', Number(e.target.value))} className={inputClass} /></label>
                  {completed && <>
                    <label className="text-xs">Przyjęta ilość<input required type="number" min="0" step="1" value={item.receivedQuantity ?? ''} onChange={e => changeItem(index, 'receivedQuantity', Number(e.target.value))} className={inputClass} /></label>
                    <label className="text-xs">W tym przyjęty złom<input required type="number" min="0" max={item.receivedQuantity ?? 0} step="1" value={item.receivedScrappedQuantity ?? ''} onChange={e => changeItem(index, 'receivedScrappedQuantity', Number(e.target.value))} className={inputClass} /></label>
                  </>}
                </div>
              </div>
            ))}
            <button type="button" onClick={addItem} className="px-4 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-xl text-sm font-semibold transition-colors cursor-pointer">Dodaj sprzęt</button>
            <label className="block text-xs font-semibold text-slate-600 mb-1">Uwagi (opcjonalnie)<textarea value={note} onChange={e => setNote(e.target.value)} rows={3} className={inputClass} /></label>
          </fieldset>
          <FormActions onCancel={onClose} submitLabel={saving ? "Zapisywanie..." : "Zapisz zmiany"} submitDisabled={loading || saving} cancelDisabled={saving} />
        </form>
      </div>
    </div>
  );
}

