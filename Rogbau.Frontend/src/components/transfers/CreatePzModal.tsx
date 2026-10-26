import { SearchableSelect } from '../ui/SearchableSelect';
import { PzItemsTable } from "./PzItemsTable";
import { FormActions } from "../ui/FormActions";
import { ModalHeader } from "../ui/ModalHeader";
import { ErrorMessage } from "../ui/ErrorMessage";
import React, { useEffect, useState } from 'react';

import { createPz, confirmTransfer } from '../../api/transferService';
import type { TransferDetailsDto } from '../../types/response/TransferResponseDto';
import { getSites, getSiteById, getMySites } from '../../api/siteService';
import { useAuth } from '../../context/AuthContext';

import type {
  SiteDto,
  SiteEquipmentDto,
} from '../../types/response/SiteResponse';

interface CreatePzModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  initialSiteId?: string;
  initialEquipmentId?: string;
  receipt?: TransferDetailsDto;
}

interface PzItem {
  equipmentId: string;
  quantity: number;
  quantityScrapped: number;
}

export const CreatePzModal: React.FC<CreatePzModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  initialSiteId,
  initialEquipmentId,
  receipt,
}) => {
  const { user } = useAuth();
  const [sites, setSites] = useState<SiteDto[]>([]);
  const [equipments, setEquipments] = useState<SiteEquipmentDto[]>([]);

  const [fromConstructionSiteId, setFromConstructionSiteId] = useState('');
  const [selectedEquipmentId, setSelectedEquipmentId] = useState('');
  const [items, setItems] = useState<PzItem[]>([]);

  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(false);
  const [loadingEquipments, setLoadingEquipments] = useState(false);

  const [note, setNote] = useState('');
  const [error, setError] = useState<string | null>(null);

  // Pobieramy listę budów po otwarciu modala
  useEffect(() => {
    if (!isOpen) return;

    setFromConstructionSiteId('');
    setSelectedEquipmentId('');
    setItems([]);
    setEquipments([]);
    setError(null);

    const fetchSites = async () => {
      try {
        setLoadingData(true);

        if (receipt && receipt.fromConstructionSiteId) {
          const site = await getSiteById(receipt.fromConstructionSiteId);
          setSites([{ ...site, totalEquipmentCount: site.equipment.reduce((total, item) => total + item.allocatedQuantity, 0) }]);
          setFromConstructionSiteId(site.id);
          setEquipments(receipt.items.map(item => ({
            equipmentId: item.equipmentId,
            equipmentName: item.equipmentName,
            unit: item.unit,
            allocatedQuantity: item.quantity + (site.equipment.find(eq => eq.equipmentId === item.equipmentId)?.allocatedQuantity ?? 0),
            purchaseTotalPrice: 0, daysOnSite: 0, totalEarned: 0, weightKg: 0,
          })));
          setItems(receipt.items.map(item => ({
            equipmentId: item.equipmentId, quantity: item.quantity, quantityScrapped: item.scrappedQuantity,
          })));
          return;
        }

        const sitesData = await getSites();

        const mySites = user?.role === 'Kierownik Budowy' ? await getMySites() : null;
        const availableSites = mySites ? sitesData.filter(site => mySites.some(mySite => mySite.id === site.id)) : sitesData;
        setSites(availableSites);
        if (initialSiteId && availableSites.some(site => site.id === initialSiteId)) {
          const site = await getSiteById(initialSiteId);
          setFromConstructionSiteId(initialSiteId);
          setEquipments(site.equipment);
          if (initialEquipmentId && site.equipment.some(eq => eq.equipmentId === initialEquipmentId)) {
            setItems([{ equipmentId: initialEquipmentId, quantity: 1, quantityScrapped: 0 }]);
          }
        }
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
  }, [isOpen, initialSiteId, initialEquipmentId, user?.role, receipt]);

  // Po wybraniu budowy pobieramy jej sprzęty
  const handleSiteChange = async (
    e: React.ChangeEvent<HTMLSelectElement>
  ) => {
    const siteId = e.target.value;

    setFromConstructionSiteId(siteId);
    setSelectedEquipmentId('');
    setItems([]);
    setEquipments([]);
    setError(null);

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

    if (alreadyAdded) {
      setError('Ten sprzęt został już dodany do PZ.');
      return;
    }

    setItems([
      ...items,
      {
        equipmentId: selectedEquipmentId,
        quantity: 1,
        quantityScrapped: 0,
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
          ? {
              ...item,
              quantity,
              quantityScrapped: Math.min(
                item.quantityScrapped,
                quantity
              ),
            }
          : item
      )
    );
  };

  const handleScrappedQuantityChange = (
    equipmentId: string,
    quantityScrapped: number
  ) => {
    setItems(
      items.map((item) =>
        item.equipmentId === equipmentId
          ? {
              ...item,
              quantityScrapped,
            }
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

    if (items.length === 0) {
      setError('Dodaj przynajmniej jeden sprzęt.');
      return;
    }

    const invalidItem = items.find((item) => {
      const equipment = getEquipment(item.equipmentId);

      return (
        !equipment ||
        !Number.isInteger(item.quantity) ||
        !Number.isInteger(item.quantityScrapped) ||
        item.quantity < (receipt ? 0 : 1) ||
        item.quantity > equipment.allocatedQuantity ||
        item.quantityScrapped < 0 ||
        item.quantityScrapped > item.quantity
      );
    });

    if (invalidItem) {
      setError(
        'Sprawdź ilość zwracanego sprzętu oraz ilość złomu.'
      );
      return;
    }

    try {
      setLoading(true);
      setError(null);

      if (receipt) {
        await confirmTransfer(receipt.id, items.map(item => ({
          equipmentId: item.equipmentId, quantity: item.quantity, scrappedQuantity: item.quantityScrapped,
        })));
      } else {
        await createPz({
        note,
        fromConstructionSiteId,
        items: items.map((item) => ({
          equipmentId: item.equipmentId,
          quantity: Number(item.quantity),
          quantityScrapped: Number(item.quantityScrapped),
        })),
      });
      }

      setFromConstructionSiteId('');
      setSelectedEquipmentId('');
      setEquipments([]);
      setItems([]);

      onSuccess();
      onClose();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Wystąpił błąd podczas tworzenia PZ.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-4xl max-h-[90vh] overflow-y-auto">

        {/* Header */}
        <ModalHeader title={receipt ? `Przyjmij ${receipt.number}` : 'Zgłoś zwrot (PZ)'} description={receipt ? 'Ilości uzupełniono ze zgłoszenia budowy. Wpisz rzeczywiście przyjęty sprzęt i złom.' : 'Zgłoszenie zwrotu. Magazyn przyjmie sprzęt po potwierdzeniu odbioru.'} onClose={onClose} />

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
              {/* Budowa źródłowa */}
              <div>
                <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                  Budowa źródłowa
                </label>

                <select
                  required
                  value={fromConstructionSiteId}
                  disabled={!!receipt}
                  onChange={handleSiteChange}
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

              {/* Sprzęt */}
              {fromConstructionSiteId && !receipt && (
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

                  {!loadingEquipments &&
                    equipments.length === 0 && (
                      <p className="mt-2 text-sm text-slate-500">
                        Brak sprzętu przypisanego do tej budowy.
                      </p>
                    )}
                </div>
              )}

              {/* Pozycje PZ */}
              {items.length > 0 && (
                <div className="border border-slate-200 rounded-xl overflow-x-auto">
                  <PzItemsTable items={items} getEquipment={getEquipment}
                    declaredItems={receipt?.items}
                    handleQuantityChange={handleQuantityChange} handleRemoveItem={handleRemoveItem} handleScrappedQuantityChange={handleScrappedQuantityChange} />
                </div>
              )}
            </>
          )}

          {!receipt && <div><label className="block text-xs font-semibold text-slate-600 mb-1">Uwagi (opcjonalnie)</label><textarea maxLength={1000} value={note} onChange={e => setNote(e.target.value)} className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm" /></div>}
          {/* Footer */}
          <FormActions onCancel={onClose} submitLabel={loading ? 'Zapisywanie...' : receipt ? 'Potwierdź przyjęcie' : 'Zgłoś zwrot'}
            submitDisabled={loading || loadingData || loadingEquipments || items.length === 0} />

        </form>
      </div>
    </div>
  );
};
