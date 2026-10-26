import { SearchableSelect } from '../ui/SearchableSelect';
import { WeightSummary } from "./WeightSummary";
import { WzItemsTable } from "./WzItemsTable";
import { FormActions } from "../ui/FormActions";
import { ModalHeader } from "../ui/ModalHeader";
import { ErrorMessage } from "../ui/ErrorMessage";
import { SiteStatus } from '../../types/enums/SiteStatus';
import React, { useEffect, useState } from 'react';

import { createWz } from '../../api/transferService';
import { getAllEquipments } from '../../api/equipmentService';
import { getSites } from '../../api/siteService';

import type { EquipmentResponse } from '../../types/response/EquipmentResponse';
import type { OrderDetails } from '../../api/orderService';

interface CreateWzModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  order?: OrderDetails;
}

interface SiteOption {
  id: string;
  name: string;
}

interface WzItem {
  equipmentId: string;
  quantity: number;
}

export const CreateWzModal: React.FC<CreateWzModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  order,
}) => {
  const [sites, setSites] = useState<SiteOption[]>([]);
  const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);

  const [toConstructionSiteId, setToConstructionSiteId] = useState('');
  const [items, setItems] = useState<WzItem[]>([]);

  const [selectedEquipmentId, setSelectedEquipmentId] = useState('');

  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(false);
  const [note, setNote] = useState('');
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;

    const fetchData = async () => {
      try {
        setLoadingData(true);
        setError(null);

        const [sitesData, equipmentData] = await Promise.all([
          getSites(),
          getAllEquipments({ isActive: true }),
        ]);

        setSites(sitesData.filter(site => site.status !== SiteStatus.Completed));
        setEquipments(equipmentData);
        setToConstructionSiteId(order?.siteId ?? '');
        setSelectedEquipmentId('');
        setNote('');
        setItems(order ? order.items.filter(x => x.quantity > x.sentQuantity &&
          equipmentData.some(equipment => equipment.id === x.equipmentId)).map(x => ({
          equipmentId: x.equipmentId,
          quantity: Math.min(x.quantity - x.sentQuantity, equipmentData.find(e => e.id === x.equipmentId)?.quantityAvailable ?? 0),
        })) : []);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : 'Nie udało się pobrać danych.'
        );
      } finally {
        setLoadingData(false);
      }
    };

    fetchData();
  }, [isOpen, order]);

  const handleAddEquipment = () => {
    if (!selectedEquipmentId) return;

    const alreadyAdded = items.some(
      (x) => x.equipmentId === selectedEquipmentId
    );

    if (alreadyAdded) {
      setError('Ten sprzęt został już dodany do WZ.');
      return;
    }

    setItems([
      ...items,
      {
        equipmentId: selectedEquipmentId,
        quantity: 1,
      },
    ]);

    setSelectedEquipmentId('');
    setError(null);
  };

  const handleQuantityChange = (
    equipmentId: string,
    quantity: number
  ) => {
    setItems(
      items.map((item) =>
        item.equipmentId === equipmentId
          ? { ...item, quantity }
          : item
      )
    );
  };

  const handleRemoveItem = (equipmentId: string) => {
    setItems(items.filter((x) => x.equipmentId !== equipmentId));
  };

  const getEquipment = (equipmentId: string) => {
    return equipments.find((x) => x.id === equipmentId);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!toConstructionSiteId) {
      setError('Wybierz budowę docelową.');
      return;
    }

    const sendingItems = order ? items.filter(x => x.quantity > 0) : items;
    if (sendingItems.length === 0) {
      setError('Dodaj przynajmniej jeden sprzęt.');
      return;
    }

    const invalidItem = items.find((item) => {
      const equipment = getEquipment(item.equipmentId);
      const ordered = order?.items.find(x => x.equipmentId === item.equipmentId);

      return (
        !Number.isInteger(item.quantity) || item.quantity < (order ? 0 : 1) ||
        !equipment ||
        item.quantity > equipment.quantityAvailable ||
        (order && (!ordered || item.quantity > ordered.quantity - ordered.sentQuantity))
      );
    });

    if (invalidItem) {
      setError('Sprawdź ilości wydawanego sprzętu.');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      await createWz({
        note,
        orderId: order?.id,
        toConstructionSiteId,
        items: sendingItems.map((item) => ({
          equipmentId: item.equipmentId,
          quantity: Number(item.quantity),
        })),
      });

      setToConstructionSiteId('');
      setItems([]);
      setSelectedEquipmentId('');

      onSuccess();
      onClose();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Wystąpił błąd podczas tworzenia WZ.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

 const totalWeightKg = items.reduce((total, item) => {
    const equipment = equipments.find(
        x => x.id === item.equipmentId
    );

    return total + (equipment?.weightKg ?? 0) * item.quantity;
}, 0);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-5xl max-h-[90vh] overflow-y-auto">

        <ModalHeader title="Utwórz WZ" description={"Wydanie sprzętu z magazynu na budowę" + (order ? ` — zamówienie ${order.orderNumber}` : "")} onClose={onClose} disabled={loading} />

        <form onSubmit={handleSubmit} className="p-6 space-y-5">

          {error && (
            <ErrorMessage message={error} className="p-3" />
          )}

          {loadingData ? (
            <div className="py-8 text-center text-sm text-slate-500">
              Ładowanie danych...
            </div>
          ) : (
            <>
              {/* Budowa docelowa */}

              <div>
                <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                  Budowa docelowa
                </label>

                <select
                  required
                  disabled={!!order}
                  value={toConstructionSiteId}
                  onChange={(e) =>
                    setToConstructionSiteId(e.target.value)
                  }
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white"
                >
                  <option value="">Wybierz budowę...</option>

                  {sites.map((site) => (
                    <option key={site.id} value={site.id}>
                      {site.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Dodawanie sprzętu */}

              {!order && <div>
                <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                  Dodaj sprzęt
                </label>

                <div className="flex gap-3">
                  <SearchableSelect value={selectedEquipmentId} onChange={setSelectedEquipmentId}  placeholder="Wyszukaj sprzęt..." options={equipments.filter(equipment => equipment.quantityAvailable > 0 && !items.some(item => item.equipmentId === equipment.id)).map(equipment => ({ value: equipment.id, label: `${equipment.name} — dostępne: ${equipment.quantityAvailable} ${equipment.unit}` }))} />

                  <button
                    type="button"
                    onClick={handleAddEquipment}
                    disabled={!selectedEquipmentId}
                    className="px-4 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-xl text-sm font-semibold transition-colors cursor-pointer disabled:opacity-50"
                  >
                    Dodaj
                  </button>
                </div>
              </div>}

              {/* Pozycje WZ */}

              {items.length > 0 && (
                <div className="border border-slate-200 rounded-xl overflow-x-auto">
                  <WzItemsTable items={items} getEquipment={getEquipment}
                    handleQuantityChange={handleQuantityChange} handleRemoveItem={handleRemoveItem} order={order} />
                </div>
              )}
            </>
          )}

          {/* Przyciski */}
          <div><label className="block text-xs font-semibold text-slate-600 mb-1">Uwagi (opcjonalnie)</label><textarea maxLength={1000} value={note} onChange={e => setNote(e.target.value)} className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm" /></div>

          <FormActions onCancel={onClose} submitLabel={loading ? 'Tworzenie...' : 'Utwórz WZ'}
            submitDisabled={loading || loadingData} cancelDisabled={loading} />

          <WeightSummary totalWeightKg={totalWeightKg} />
        </form>
      </div>

      
    </div>
  );
};
