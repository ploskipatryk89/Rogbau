import React, { useState, useEffect } from 'react';
import type { CreatePurchaseCommand, CreatePurchaseItemDto } from '../../types/request/CreatePurchaseRequest';
import type { EquipmentResponse } from '../../types/response/EquipmentResponse';
import type { SupplierResponseDto } from '../../types/response/SupplierResponseDto';
import { getAllEquipments } from '../../api/equipmentService';
import { getSuppliers } from '../../api/supplierService';
import { createPurchase, editPurchase } from '../../api/purchaseSerwise';
import type { PurchaseListDto } from '../../types/response/PurchaseResponse';
import { XMarkIcon, PlusIcon, TrashIcon, ShoppingCartIcon } from '@heroicons/react/24/outline';
import { SearchableSelect } from '../ui/SearchableSelect';

interface CreatePurchaseModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  purchase?: PurchaseListDto | null;
}

export const CreatePurchaseModal: React.FC<CreatePurchaseModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  purchase,
}) => {
  const [supplierId, setSupplierId] = useState<string>('');
  const [purchaseDate, setPurchaseDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [items, setItems] = useState<CreatePurchaseItemDto[]>([]);
  
  const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);
  const [suppliers, setSuppliers] = useState<SupplierResponseDto[]>([]);
  
  const [loadingData, setLoadingData] = useState<boolean>(false);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      const fetchData = async () => {
        try {
          setLoadingData(true);
          const [equipmentsData, suppliersData] = await Promise.all([
            getAllEquipments(),
            getSuppliers()
          ]);
          
          setEquipments(equipmentsData.filter(e => e.isActive || purchase?.items.some(item => item.equipmentId === e.id)));
          setSuppliers(suppliersData || []);
        } catch {
          setError('Nie udało się pobrać danych (sprzętu lub dostawców).');
        } finally {
          setLoadingData(false);
        }
      };

      fetchData();
      
      // Reset formularza
      setSupplierId(purchase?.supplierId ?? '');
      setPurchaseDate(purchase?.date.slice(0, 10) ?? new Date().toISOString().split('T')[0]);
      setItems(purchase?.items.map(item => ({ equipmentId: item.equipmentId, quantity: item.quantity, purchasePrice: item.purchaseItemPrice })) ?? []);
      setError(null);
    }
  }, [isOpen, purchase]);

  const handleAddItem = () => {
    setItems((prev) => [...prev, { equipmentId: '', quantity: 1, purchasePrice: 0 }]);
  };

  const handleRemoveItem = (index: number) => {
    setItems((prev) => prev.filter((_, i) => i !== index));
  };

  const handleItemChange = (index: number, field: keyof CreatePurchaseItemDto, value: string | number) => {
    setItems((prev) => {
      const updated = [...prev];
      updated[index] = { ...updated[index], [field]: value };
      return updated;
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!supplierId.trim()) {
      setError('Proszę wybrać dostawcę z listy.');
      return;
    }

    if (items.length === 0) {
      setError('Dodaj przynajmniej jedną pozycję sprzętu do zakupu.');
      return;
    }

    const hasInvalidItems = items.some(i => !i.equipmentId || !Number.isInteger(i.quantity) || i.quantity <= 0 || !Number.isFinite(i.purchasePrice) || i.purchasePrice <= 0);
    if (new Set(items.map(i => i.equipmentId)).size !== items.length) {
      setError('Sprzęt nie może się powtarzać w zakupie.');
      return;
    }
    if (hasInvalidItems) {
      setError('Upewnij się, że każdy wybrany sprzęt ma poprawnie zaznaczony model oraz ilość większą od 0.');
      return;
    }

    try {
      setIsSubmitting(true);

      // Czysty ciąg "YYYY-MM-DD" dopasowany pod C# DateOnly
      const payload: CreatePurchaseCommand = {
        supplierId,
        purchaseDate, 
        items,
      };

      if (purchase) await editPurchase(purchase.id, payload);
      else await createPurchase(payload);
      onSuccess();
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Wystąpił błąd podczas dodawania zakupu.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
<div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-3xl max-h-[90vh] overflow-y-auto animate-in fade-in zoom-in-95 duration-150">        
        {/* Nagłówek */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50/50">
          <div className="flex items-center gap-2">
            <ShoppingCartIcon className="w-5 h-5 text-amber-500" />
            <h3 className="text-lg font-bold text-slate-900">{purchase ? 'Edytuj zakup / dostawę' : 'Nowy Zakup / Dostawa'}</h3>
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
        <form onSubmit={handleSubmit} className="p-6 space-y-6">
          {purchase && <p className="text-sm text-slate-600">Zmiany pozycji skorygują stan magazynu. Zmniejszenie ilości jest możliwe tylko, gdy odpowiednia ilość jest dostępna w magazynie.</p>}
          {error && (
            <div className="p-3.5 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
              {error}
            </div>
          )}

          {/* Nagłówek zakupu */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">
                Dostawca *
              </label>
              <SearchableSelect value={supplierId} onChange={setSupplierId} disabled={loadingData} placeholder="Wyszukaj dostawcę..." options={suppliers.map(supplier => ({ value: supplier.id, label: supplier.name }))} />
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">
                Data Zakupu *
              </label>
              <input
                type="date"
                required
                value={purchaseDate}
                onChange={(e) => setPurchaseDate(e.target.value)}
                className="w-full px-3.5 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
              />
            </div>
          </div>

          {/* Sekcja Pozycji */}
          <div className="space-y-3 pt-2 border-t border-slate-100">
            <div className="flex items-center justify-between">
              <label className="text-xs font-bold text-slate-700 uppercase tracking-wider">
                Pozycje Zakupowe ({items.length})
              </label>
              <button
                type="button"
                onClick={handleAddItem}
                disabled={loadingData}
                className="flex items-center gap-1.5 text-xs font-bold text-amber-600 hover:text-amber-700 bg-amber-50 hover:bg-amber-100 px-3 py-1.5 rounded-lg transition-colors cursor-pointer"
              >
                <PlusIcon className="w-4 h-4 stroke-[2.5]" />
                Dodaj Sprzęt
              </button>
            </div>

            {items.length === 0 ? (
              <div className="p-6 text-center border-2 border-dashed border-slate-200 rounded-xl text-slate-400 text-sm">
                Brak dodanych pozycji. Kliknij "Dodaj Sprzęt", aby skompletować fakturę.
              </div>
            ) : (
              <div className="space-y-2 pr-1">
                {items.map((item, index) => (
                  <div key={index} className="flex flex-wrap items-center gap-3 p-2.5 bg-slate-50 rounded-xl border border-slate-200">
                    <div className="flex-1 min-w-48">
<SearchableSelect options={equipments.filter(e => e.id === item.equipmentId || !items.some(i => i.equipmentId === e.id)).map(e => ({ value: e.id, label: `${e.name} (${e.unit})` }))} value={item.equipmentId} onChange={value => handleItemChange(index, 'equipmentId', value)} placeholder="Wyszukaj sprzęt..." />
                    </div>

                    <div className="w-32">
                      <label className="block text-xs text-slate-500 mb-1">Ilość</label>
                      <input
                        type="number"
                        min="1"
                        value={item.quantity}
                        onChange={(e) => handleItemChange(index, 'quantity', parseInt(e.target.value) || 0)}
                        placeholder="Ilość"
                        className="w-full px-3 py-1.5 text-xs bg-white border border-slate-200 rounded-lg focus:outline-none focus:border-amber-500 text-center font-semibold"
                      />
                    </div>

                    <div className="w-32">
                      <label className="block text-xs text-slate-500 mb-1">Cena / szt. (zł)</label>
                      <input type="number" required min="0.01" step="0.01" value={item.purchasePrice}
                        onChange={e => handleItemChange(index, 'purchasePrice', Number(e.target.value))}
                        className="w-full px-3 py-1.5 text-xs bg-white border border-slate-200 rounded-lg" />
                      <p className="text-xs mt-1">Razem: {(item.quantity * item.purchasePrice).toFixed(2)} zł</p>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleRemoveItem(index)}
                      className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                    >
                      <TrashIcon className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>

          <p className="text-right font-semibold">Łącznie: {items.reduce((sum, item) => sum + item.quantity * item.purchasePrice, 0).toFixed(2)} zł</p>
          {/* Przyciski Akcji */}
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
              {isSubmitting ? 'Zapisywanie...' : purchase ? 'Zapisz zmiany' : 'Zapisz Zakup'}
            </button>
          </div>
        </form>

      </div>
    </div>
  );
};
