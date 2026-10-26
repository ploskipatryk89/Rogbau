import React, { useState, useEffect } from 'react';
import { createEquipment, getCategories } from '../../api/equipmentService';
import type { CategoryDto } from '../../types/response/CategoryDto';
import { XMarkIcon } from '@heroicons/react/24/outline';

interface AddEquipmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const AddEquipmentModal: React.FC<AddEquipmentModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [subCategories, setSubCategories] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    name: '',
    unit: 'szt',
    weightKg: 0,
    categoryId: '',
    subCategoryId: '',
    purchasePrice: 0,
    dayPrice: 0,
  });

  useEffect(() => {
    if (isOpen) {
      getCategories()
        .then(setCategories)
        .catch((err) => console.warn('Błąd pobierania kategorii:', err));
    }
  }, [isOpen]);

  const handleCategoryChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const catId = e.target.value;
    setFormData({ ...formData, categoryId: catId, subCategoryId: '' });

    // Znajdujemy wybraną kategorię i sprawdzamy czy ma podkategorie (np. szalunki)
    const selectedCat = categories.find((c) => c.id === catId);
    setSubCategories(selectedCat?.subCategories || []);
  };

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLoading(true);
      setError(null);

      await createEquipment({
        ...formData,
        weightKg: Number(formData.weightKg),
        purchasePrice: Number(formData.purchasePrice),
        dayPrice: Number(formData.dayPrice),
        subCategoryId: formData.subCategoryId || null,
      });

      onSuccess();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Wystąpił błąd podczas dodawania sprzętu.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
          <h3 className="text-lg font-bold text-slate-900">Dodaj Nowy Sprzęt</h3>
          <button
            type="button"
            onClick={onClose}
            className="p-1 text-slate-400 hover:text-slate-600 rounded-lg transition-colors cursor-pointer"
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
              {error}
            </div>
          )}

          <div>
            <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
              Nazwa sprzętu
            </label>
            <input
              type="text"
              required
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
              placeholder="np. Wiertarka udarowa"
            />
          </div>

          {/* Dynamiczny układ: jeśli kategoria ma podkategorie, pokazujemy oba pola obok siebie, a jak nie, to kategoria główna zajmuje całą szerokość */}
          <div className={`grid ${subCategories.length > 0 ? 'grid-cols-2' : 'grid-cols-1'} gap-4`}>
            <div>
              <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                Kategoria główna
              </label>
              <select
                required
                value={formData.categoryId}
                onChange={handleCategoryChange}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white"
              >
                <option value="">Wybierz...</option>
                {categories.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name}
                  </option>
                ))}
              </select>
            </div>

            {subCategories.length > 0 && (
              <div>
                <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                  Podkategoria
                </label>
                <select
                  value={formData.subCategoryId}
                  onChange={(e) => setFormData({ ...formData, subCategoryId: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white"
                >
                  <option value="">Wybierz...</option>
                  {subCategories.map((sub) => (
                    <option key={sub.id} value={sub.id}>
                      {sub.name}
                    </option>
                  ))}
                </select>
              </div>
            )}
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                J.m.
              </label>
              <input
                type="text"
                value={formData.unit}
                onChange={(e) => setFormData({ ...formData, unit: e.target.value })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                Waga (kg)
              </label>
              <input
                type="number"
                min="0"
                step="0.1"
                value={formData.weightKg}
                onChange={(e) => setFormData({ ...formData, weightKg: Number(e.target.value) })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">

            <div>
              <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
                Stawka za dzień (zł)
              </label>
              <input
                type="number"
                min="0"
                step="any"
                value={formData.dayPrice}
                onChange={(e) => setFormData({ ...formData, dayPrice: Number(e.target.value) })}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
              />
            </div>
          </div>

          <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-200">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100 rounded-xl transition-colors cursor-pointer"
            >
              Anuluj
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 text-sm font-bold bg-amber-500 hover:bg-amber-600 text-slate-950 rounded-xl shadow-sm transition-all cursor-pointer disabled:opacity-50"
            >
              {loading ? 'Zapisywanie...' : 'Zapisz sprzęt'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
