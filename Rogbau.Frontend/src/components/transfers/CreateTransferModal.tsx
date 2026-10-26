import { SearchableSelect } from '../ui/SearchableSelect';
import { WeightSummary } from "./WeightSummary";
import { TransferItemsTable } from "./TransferItemsTable";
import { FormActions } from "../ui/FormActions";
import { ModalHeader } from "../ui/ModalHeader";
import { ErrorMessage } from "../ui/ErrorMessage";
import { SiteStatus } from '../../types/enums/SiteStatus';
import React, { useEffect, useState } from 'react';

import { createTransfer } from '../../api/transferService';
import { getSites, getSiteById, getMySites } from '../../api/siteService';

import type {
  SiteDto,
  SiteEquipmentDto,
} from '../../types/response/SiteResponse';

interface CreateTransferModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

interface TransferItem {
  equipmentId: string;
  quantity: number;
}

export const CreateTransferModal: React.FC<CreateTransferModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [sites, setSites] = useState<SiteDto[]>([]);
  const [sourceSiteIds, setSourceSiteIds] = useState<string[]>([]);
  const [equipments, setEquipments] = useState<SiteEquipmentDto[]>([]);

  const [fromConstructionSiteId, setFromConstructionSiteId] = useState('');
  const [toConstructionSiteId, setToConstructionSiteId] = useState('');

  const [selectedEquipmentId, setSelectedEquipmentId] = useState('');
  const [items, setItems] = useState<TransferItem[]>([]);

  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(false);
  const [loadingEquipments, setLoadingEquipments] = useState(false);

  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;

    setFromConstructionSiteId('');
    setToConstructionSiteId('');
    setSelectedEquipmentId('');
    setEquipments([]);
    setItems([]);
    setError(null);

    const fetchSites = async () => {
      try {
        setLoadingData(true);

        const [sitesData, mySites] = await Promise.all([getSites(), getMySites()]);

        setSites(sitesData.filter(site => site.status !== SiteStatus.Completed));
        setSourceSiteIds(mySites.map(x => x.id));
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : 'Nie udało się pobrać budów.'
        );
      } finally {
        setLoadingData(false);
      }
    };

    fetchSites();
  }, [isOpen]);

  const handleFromSiteChange = async (
    e: React.ChangeEvent<HTMLSelectElement>
  ) => {
    const siteId = e.target.value;

    setFromConstructionSiteId(siteId);
    setSelectedEquipmentId('');
    setEquipments([]);
    setItems([]);
    setError(null);

    if (siteId === toConstructionSiteId) {
      setToConstructionSiteId('');
    }

    if (!siteId) return;

    try {
      setLoadingEquipments(true);

      const site = await getSiteById(siteId);

      setEquipments(site.equipment);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Nie udało się pobrać sprzętu budowy.'
      );
    } finally {
      setLoadingEquipments(false);
    }
  };

  const handleAddEquipment = () => {
    if (!selectedEquipmentId) return;

    const alreadyAdded = items.some(
      (x) => x.equipmentId === selectedEquipmentId
    );

    if (alreadyAdded) return;

    setItems([
      ...items,
      {
        equipmentId: selectedEquipmentId,
        quantity: 1,
      },
    ]);

    setSelectedEquipmentId('');
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
    setItems(
      items.filter((x) => x.equipmentId !== equipmentId)
    );
  };

  const getEquipment = (equipmentId: string) => {
    return equipments.find(
      (x) => x.equipmentId === equipmentId
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!fromConstructionSiteId) {
      setError('Wybierz budowę źródłową.');
      return;
    }

    if (!toConstructionSiteId) {
      setError('Wybierz budowę docelową.');
      return;
    }

    if (fromConstructionSiteId === toConstructionSiteId) {
      setError('Budowa źródłowa i docelowa nie mogą być takie same.');
      return;
    }

    if (items.length === 0) {
      setError('Dodaj przynajmniej jeden sprzęt.');
      return;
    }

    const invalidItem = items.find((item) => {
      const equipment = getEquipment(item.equipmentId);

      return (
        !equipment ||
        item.quantity <= 0 ||
        item.quantity > equipment.allocatedQuantity
      );
    });

    if (invalidItem) {
      setError('Sprawdź ilość przesuwanego sprzętu.');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      await createTransfer({
        fromConstructionSiteId,
        toConstructionSiteId,
        items: items.map((item) => ({
          equipmentId: item.equipmentId,
          quantity: Number(item.quantity),
        })),
      });

      onSuccess();
      onClose();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Wystąpił błąd podczas tworzenia przesunięcia.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  const totalWeightKg = items.reduce((total, item) => {
    const equipment = equipments.find(
        x => x.equipmentId === item.equipmentId
    );

    return total + (equipment?.weightKg ?? 0) * item.quantity;
}, 0);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-4xl overflow-hidden">

        <ModalHeader title="Przesunięcie między budowami" description="Przekazanie sprzętu z jednej budowy na drugą" onClose={onClose} />

        <form onSubmit={handleSubmit} className="p-6 space-y-5">

          {error && (
            <ErrorMessage message={error} className="p-3" />
          )}

          {loadingData ? (
            <div className="py-8 text-center text-sm text-slate-500">
              Ładowanie budów...
            </div>
          ) : (
            <>
              {/* Budowy */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">

                <div>
                  <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                    Z budowy
                  </label>

                  <select
                    required
                    value={fromConstructionSiteId}
                    onChange={handleFromSiteChange}
                    className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white"
                  >
                    <option value="">Wybierz budowę...</option>

                    {sites.filter(site => sourceSiteIds.includes(site.id)).map((site) => (
                      <option key={site.id} value={site.id}>
                        {site.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                    Do budowy
                  </label>

                  <select
                    required
                    value={toConstructionSiteId}
                    onChange={(e) =>
                      setToConstructionSiteId(e.target.value)
                    }
                    className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white"
                  >
                    <option value="">Wybierz budowę...</option>

                    {sites
                      .filter(
                        (site) =>
                          site.id !== fromConstructionSiteId
                      )
                      .map((site) => (
                        <option key={site.id} value={site.id}>
                          {site.name}
                        </option>
                      ))}
                  </select>
                </div>

              </div>

              {/* Wybór sprzętu */}
              {fromConstructionSiteId && (
                <div>
                  <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                    Dodaj sprzęt
                  </label>

                  <div className="flex gap-3">
                    <SearchableSelect value={selectedEquipmentId} onChange={setSelectedEquipmentId} disabled={loadingEquipments} placeholder="Wyszukaj sprzęt..." options={equipments.filter(equipment => equipment.allocatedQuantity > 0 && !items.some(item => item.equipmentId === equipment.equipmentId)).map(equipment => ({ value: equipment.equipmentId, label: `${equipment.equipmentName} — dostępne: ${equipment.allocatedQuantity} ${equipment.unit}` }))} />

                    <button
                      type="button"
                      onClick={handleAddEquipment}
                      disabled={
                        !selectedEquipmentId ||
                        loadingEquipments
                      }
                      className="px-4 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-xl text-sm font-semibold transition-colors cursor-pointer disabled:opacity-50"
                    >
                      Dodaj
                    </button>
                  </div>
                </div>
              )}

              {/* Pozycje */}
              {items.length > 0 && (
                <div className="border border-slate-200 rounded-xl overflow-hidden">
                  <TransferItemsTable items={items} getEquipment={getEquipment}
                    handleQuantityChange={handleQuantityChange} handleRemoveItem={handleRemoveItem} />
                </div>
              )}
            </>
          )}

          <FormActions onCancel={onClose} submitLabel={loading ? 'Tworzenie...' : 'Utwórz przesunięcie'}
            submitDisabled={loading || loadingData || loadingEquipments || !fromConstructionSiteId || !toConstructionSiteId || items.length === 0} />

          <WeightSummary totalWeightKg={totalWeightKg} />
        </form>
      </div>
    </div>
  );
};
