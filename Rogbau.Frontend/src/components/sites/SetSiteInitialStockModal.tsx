import React, { useState, useEffect } from 'react';
import type { SiteDto } from '../../types/response/SiteResponse';
import type { EquipmentResponse } from '../../types/response/EquipmentResponse';
import { getAllEquipments } from '../../api/equipmentService';
import { setSiteInitialStock } from '../../api/siteService';
import { XMarkIcon, ScaleIcon } from '@heroicons/react/24/outline';

interface SetSiteInitialStockModalProps {
  isOpen: boolean;
  site: SiteDto | null;
  onClose: () => void;
  onSuccess: () => void;
}

export const SetSiteInitialStockModal: React.FC<SetSiteInitialStockModalProps> = ({
  isOpen,
  site,
  onClose,
  onSuccess,
}) => {
  const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [loadingData, setLoadingData] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && site) {
      const fetchEquipments = async () => {
        try {
          setLoadingData(true);
          const data = await getAllEquipments();
          setEquipments(data.filter((e) => e.isActive));
          
          // Inicjalizacja ilości zerami
          const initialMap: Record<string, number> = {};
          data.forEach((e) => {
            initialMap[e.id] = 0;
          });
          setQuantities(initialMap);
        } catch {
          setError('Nie udało się pobrać listy sprzętu.');
        } finally {
          setLoadingData(false);
        }
      };

      fetchEquipments();
      setError(null);
    }
  }, [isOpen, site]);

  const handleQuantityChange = (equipmentId: string, value: string) => {
    const parsed = parseInt(value, 10);
    setQuantities((prev) => ({
      ...prev,
      [equipmentId]: Number.isNaN(parsed) || parsed < 0 ? 0 : parsed,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!site) return;

    try {
      setIsSubmitting(true);
      setError(null);

      const items = Object.entries(quantities)
        .filter(([_, qty]) => qty > 0)
        .map(([equipmentId, quantity]) => ({
          equipmentId,
          quantity,
        }));

      // Przekazujemy ID budowy oraz samą tablicę pozycji (items)
      await setSiteInitialStock(site.id, items);

      onSuccess();
      onClose();
    } catch (err: any) {
      setError(err instanceof Error ? err.message : 'Wystąpił błąd podczas zapisywania bilansu.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen || !site) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden animate-in fade-in zoom-in-95 duration-150">
        
        {/* Nagłówek */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50/50">
          <div className="flex items-center gap-2">
            <ScaleIcon className="w-5 h-5 text-amber-500" />
            <div>
              <h3 className="text-lg font-bold text-slate-900">Bilans otwarcia sprzętu</h3>
              <p className="text-xs text-slate-500">
                Budowa: <span className="font-semibold text-slate-700">{site.name}</span>
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="p-1 text-slate-400 hover:text-slate-600 rounded-lg hover:bg-slate-100 transition-colors cursor-pointer"
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        </div>

        {/* Formularz */}
        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="p-3.5 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
              {error}
            </div>
          )}

          {loadingData ? (
            <div className="py-8 text-center text-slate-400 text-sm">
              Ładowanie listy sprzętu...
            </div>
          ) : equipments.length === 0 ? (
            <div className="py-8 text-center text-slate-400 text-sm">
              Brak dostępnego sprzętu w systemie.
            </div>
          ) : (
            <div className="space-y-3 max-h-80 overflow-y-auto pr-1">
              {equipments.map((eq) => (
                <div
                  key={eq.id}
                  className="flex items-center justify-between p-3 bg-slate-50 rounded-xl border border-slate-200/80"
                >
                  <div>
                    <p className="text-sm font-bold text-slate-800">{eq.name}</p>
                    <p className="text-xs text-slate-400">Jednostka: {eq.unit}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <input
                      type="number"
                      min="0"
                      value={quantities[eq.id] ?? 0}
                      onChange={(e) => handleQuantityChange(eq.id, e.target.value)}
                      className="w-20 px-3 py-1.5 text-sm bg-white border border-slate-200 rounded-lg text-center font-bold text-slate-900 focus:outline-none focus:border-amber-500"
                    />
                    <span className="text-xs text-slate-500 font-medium w-8">{eq.unit}</span>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Stopka z przyciskami */}
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-4 py-2.5 rounded-xl border border-slate-200 hover:bg-slate-50 text-slate-700 text-sm font-semibold transition-colors cursor-pointer"
            >
              Anuluj
            </button>
            <button
              type="submit"
              disabled={isSubmitting || loadingData}
              className="px-5 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-600 font-bold text-slate-950 text-sm shadow-sm transition-all active:scale-95 disabled:opacity-50 cursor-pointer"
            >
              {isSubmitting ? 'Zapisywanie...' : 'Zapisz Bilans'}
            </button>
          </div>
        </form>

      </div>
    </div>
  );
};