import { ErrorMessage } from "../components/ui/ErrorMessage";
import { useAuth } from "../context/AuthContext";
import React, { useEffect, useState } from 'react';
import type { PurchaseListDto } from '../types/response/PurchaseResponse';
import { deletePurchase, getPurchases } from '../api/purchaseSerwise';
import { PurchaseTable } from '../components/purchases/PurchaseTable';
import { CreatePurchaseModal } from '../components/purchases/CreatePurchaseModal';
import { EditPurchaseModal } from '../components/purchases/EditPurchaseModal';
import { DeleteConfirmModal } from '../components/ui/DeleteConfirmModal';
import { PageHeader } from '../components/ui/PageHeader';
import { PurchaseFilters } from '../components/purchases/PurchaseFilters';
import type { PurchaseFiltersType } from '../components/purchases/PurchaseFilters';
import { PaginationControls } from "../components/Pagination/PaginationControls";

export const PurchasesPage: React.FC = () => {
  const { user } = useAuth();
  const [purchases, setPurchases] = useState<PurchaseListDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState<boolean>(false);
  const [editingPurchase, setEditingPurchase] = useState<PurchaseListDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<PurchaseListDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [filters, setFilters] = useState<PurchaseFiltersType>({ supplierName: '', dateFrom: '', dateTo: '' });

const [pageNumber, setPageNumber] = useState(1);
const [pageSize, setPageSize] = useState(10);

const[pagination, setPagination] = useState({
  totalCount: 0,
  totalPages: 0,
  hasPreviousPage: false,
  hasNextPage: false
})

  const filteredPurchases = purchases.filter(purchase => {
    const date = purchase.date.slice(0, 10);
    return purchase.supplierName.toLocaleLowerCase('pl').includes(filters.supplierName.trim().toLocaleLowerCase('pl')) &&
      (!filters.dateFrom || date >= filters.dateFrom) &&
      (!filters.dateTo || date <= filters.dateTo);
  });

  const fetchPurchases = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getPurchases({
        pageNumber,
        pageSize
      });

      const lastPage = Math.max(1, data.totalPages);

        if (pageNumber > lastPage) {
        setPageNumber(lastPage);
        return;
      }

      setPurchases(data.items);

        setPagination({
        totalCount: data.totalCount,
        totalPages: data.totalPages,
        hasPreviousPage: data.hasPreviousPage,
        hasNextPage: data.hasNextPage,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Wystąpił błąd podczas pobierania zakupów.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
  fetchPurchases();
}, [pageNumber, pageSize]);

    const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setPageNumber(1);
  };
  const handleConfirmDelete = async () => {
    if (!deleteTarget || user?.role !== 'Admin' || isDeleting) return;

    try {
      setIsDeleting(true);
      setError(null);
      await deletePurchase(deleteTarget.id);
      setDeleteTarget(null);
      await fetchPurchases();
    } catch (err) {
      setDeleteTarget(null);
      setError(err instanceof Error ? err.message : 'Nie udało się usunąć faktury zakupu.');
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      
      <PageHeader
      title='Zakupy i Dostawy'
      subTile='Rejestruj dostawy sprzętu i materiałów od dostawców'
      onClickFetchButton={fetchPurchases}
      onclickActionButton={() => setIsCreateModalOpen(true)}
      actionButtonText='Dodaj zakup'
      loadingItems = {loading}
      actionButtonVisible = {user?.role === 'Admin'}
      />


      {error && (
        <ErrorMessage message={error} className="p-4" />
      )}

      <PurchaseFilters filters={filters} onChange={setFilters} />

 <PaginationControls
        pageNumber={pageNumber}
        pageSize={pageSize}
        totalCount={pagination.totalCount}
        totalPages={pagination.totalPages}
        hasPreviousPage={pagination.hasPreviousPage}
        hasNextPage={pagination.hasNextPage}
        onPageChange={setPageNumber}
        onPageSizeChange={handlePageSizeChange}
        isLoading={loading}
      />
      <PurchaseTable 
        items={filteredPurchases} 
        isLoading={loading} 
        onEdit={user?.role === 'Admin' ? setEditingPurchase : undefined}
        onDelete={user?.role === 'Admin' ? setDeleteTarget : undefined}
      />

       <PaginationControls
        pageNumber={pageNumber}
        pageSize={pageSize}
        totalCount={pagination.totalCount}
        totalPages={pagination.totalPages}
        hasPreviousPage={pagination.hasPreviousPage}
        hasNextPage={pagination.hasNextPage}
        onPageChange={setPageNumber}
        onPageSizeChange={handlePageSizeChange}
        isLoading={loading}
      />

      <CreatePurchaseModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSuccess={fetchPurchases}
      />
      <EditPurchaseModal purchase={editingPurchase} onClose={() => setEditingPurchase(null)} onSuccess={fetchPurchases} />
      <DeleteConfirmModal
        isOpen={!!deleteTarget}
        title="Usuń fakturę zakupu"
        message={`Czy na pewno chcesz usunąć fakturę zakupu od ${deleteTarget?.supplierName ?? ''} z dnia ${deleteTarget?.date ?? ''}? Spowoduje to cofnięcie zakupionych ilości sprzętu. Tej operacji nie można cofnąć.`}
        isDeleting={isDeleting}
        onClose={() => { if (!isDeleting) setDeleteTarget(null); }}
        onConfirm={handleConfirmDelete}
      />
    </div>
  );
};
