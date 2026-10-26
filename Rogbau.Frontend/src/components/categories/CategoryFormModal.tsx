import { useState } from 'react';
import type { CategoryDto } from '../../types/response/CategoryDto';
import { createCategory, updateCategory } from '../../api/categoryService';
import { ModalHeader } from '../ui/ModalHeader';
import { FormActions } from '../ui/FormActions';
import { SearchableSelect } from '../ui/SearchableSelect';

export interface CategoryEditTarget { id: string; name: string; categoryId?: string; }
interface Props {
  isSubCategory: boolean;
  categories: CategoryDto[];
  target: CategoryEditTarget | null;
  onClose: () => void;
  onSuccess: () => void;
}

export function CategoryFormModal({ isSubCategory, categories, target, onClose, onSuccess }: Props) {
  const [name, setName] = useState(target?.name ?? '');
  const [categoryId, setCategoryId] = useState(target?.categoryId ?? '');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  async function save(e: React.FormEvent) {
    e.preventDefault();
    if (isSubCategory && !categoryId) { setError('Wybierz kategorię nadrzędną.'); return; }
    setBusy(true); setError('');
    try {
      const data = { name, isSubCategory, categoryId: categoryId || undefined };
      if (target) await updateCategory(target.id, data); else await createCategory(data);
      onSuccess(); onClose();
    } catch (err) { setError(err instanceof Error ? err.message : 'Nie udało się zapisać.'); }
    finally { setBusy(false); }
  }

  return <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50"><div className="bg-white rounded-2xl w-full max-w-lg">
    <ModalHeader title={`${target ? 'Edytuj' : 'Dodaj'} ${isSubCategory ? 'podkategorię' : 'kategorię'}`} description="Wprowadź nazwę grupy sprzętu." onClose={onClose} />
    <form onSubmit={save} className="p-6 space-y-4">
      {error && <p className="p-3 bg-red-50 text-red-700 rounded-xl text-sm">{error}</p>}
      {isSubCategory && <div><label className="block text-xs font-semibold text-slate-600 mb-1">Kategoria nadrzędna</label><SearchableSelect options={categories.map(c => ({ value: c.id, label: c.name }))} value={categoryId} onChange={setCategoryId} disabled={!!target || busy} placeholder="Wybierz kategorię..." /></div>}
      <div><label htmlFor="category-name" className="block text-xs font-semibold text-slate-600 mb-1">Nazwa</label><input id="category-name" required maxLength={100} value={name} onChange={e => setName(e.target.value)} className="w-full border border-slate-200 rounded-xl px-3 py-2" disabled={busy} /></div>
      <FormActions onCancel={onClose} submitLabel={busy ? 'Zapisywanie...' : 'Zapisz'} submitDisabled={busy} cancelDisabled={busy} />
    </form>
  </div></div>;
}
