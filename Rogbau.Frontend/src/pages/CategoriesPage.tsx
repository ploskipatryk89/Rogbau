import { useEffect, useState } from 'react';
import { getCategories } from '../api/equipmentService';
import { deleteCategory } from '../api/categoryService';
import type { CategoryDto } from '../types/response/CategoryDto';
import { PageHeader } from '../components/ui/PageHeader';
import { DeleteConfirmModal } from '../components/ui/DeleteConfirmModal';
import { CategoryFormModal } from '../components/categories/CategoryFormModal';
import type { CategoryEditTarget } from '../components/categories/CategoryFormModal';

export function CategoriesPage() {
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [tab, setTab] = useState<'categories' | 'subcategories'>('categories');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [target, setTarget] = useState<CategoryEditTarget | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<CategoryEditTarget | null>(null);
  const [deleting, setDeleting] = useState(false);
  const isSubCategory = tab === 'subcategories';

  async function load() {
    setLoading(true); setError('');
    try { setCategories(await getCategories()); }
    catch (err) { setError(err instanceof Error ? err.message : 'Nie udało się pobrać kategorii.'); }
    finally { setLoading(false); }
  }
  useEffect(() => { void load(); }, []);

  async function remove() {
    if (!deleteTarget) return;
    setDeleting(true); setError('');
    try { await deleteCategory(deleteTarget.id, isSubCategory); setDeleteTarget(null); await load(); }
    catch (err) {
      setDeleteTarget(null);
      setError(err instanceof Error ? err.message : 'Nie udało się usunąć.');
    }
    finally { setDeleting(false); }
  }

  const rows = isSubCategory
    ? categories.flatMap(category => category.subCategories.map(sub => ({ id: sub.id, name: sub.name, categoryId: category.id, parentName: category.name })))
    : categories.map(category => ({ id: category.id, name: category.name, categoryId: undefined, parentName: '' }));

  return <div className="space-y-6">
    <PageHeader title="Kategorie sprzętu" subTile="Zarządzaj kategoriami i podkategoriami" onClickFetchButton={load} loadingItems={loading} actionButtonVisible
      actionButtonText={isSubCategory ? 'Dodaj podkategorię' : 'Dodaj kategorię'} onclickActionButton={() => { setTarget(null); setFormOpen(true); }} />
    <div className="flex gap-2 border-b border-slate-200 pb-3">
      {(['categories', 'subcategories'] as const).map(value => <button key={value} type="button" disabled={deleting} onClick={() => { setTab(value); setDeleteTarget(null); setError(''); }} className={`px-4 py-2 rounded-xl text-sm font-semibold ${tab === value ? 'bg-slate-900 text-white' : 'text-slate-600 hover:bg-slate-100'}`}>{value === 'categories' ? 'Kategorie' : 'Podkategorie'}</button>)}
    </div>
    {error && <p className="p-4 bg-red-50 text-red-700 rounded-xl text-sm">{error}</p>}
    <DeleteConfirmModal
      isOpen={!!deleteTarget}
      title={isSubCategory ? 'Usuń podkategorię' : 'Usuń kategorię'}
      message={`Czy na pewno chcesz usunąć ${isSubCategory ? 'podkategorię' : 'kategorię'} "${deleteTarget?.name}"? Operacji nie można cofnąć.`}
      isDeleting={deleting}
      onClose={() => { if (!deleting) setDeleteTarget(null); }}
      onConfirm={remove}
    />
    <div className="bg-white rounded-xl border border-slate-200 overflow-x-auto">
      {loading ? <p className="p-8 text-center text-slate-500">Ładowanie...</p> : rows.length === 0 ? <p className="p-8 text-center text-slate-500">Brak {isSubCategory ? 'podkategorii' : 'kategorii'}.</p> : <table className="w-full text-left text-sm"><thead className="bg-slate-50 text-slate-500"><tr><th className="p-4">Nazwa</th>{isSubCategory && <th className="p-4">Kategoria nadrzędna</th>}<th className="p-4 text-right">Akcje</th></tr></thead><tbody className="divide-y divide-slate-200">
        {rows.map(row => <tr key={row.id} className="hover:bg-slate-50"><td className="p-4 font-semibold">{row.name}</td>{isSubCategory && <td className="p-4 text-slate-600">{row.parentName}</td>}<td className="p-4"><div className="flex justify-end gap-4"><button type="button" disabled={deleting} className="text-amber-700" onClick={() => { setTarget(row); setFormOpen(true); }}>Edytuj</button><button type="button" disabled={deleting} className="text-red-600" onClick={() => setDeleteTarget(row)}>Usuń</button></div></td></tr>)}
      </tbody></table>}
    </div>
    {formOpen && <CategoryFormModal isSubCategory={isSubCategory} categories={categories} target={target} onClose={() => setFormOpen(false)} onSuccess={() => void load()} />}
  </div>;
}
