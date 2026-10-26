import { OrderTable } from "../components/transfers/OrderTable";
import { ErrorMessage } from "../components/ui/ErrorMessage";
import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';

import type { TransferResponseDto, TransferDetailsDto } from '../types/response/TransferResponseDto';
import type { TransferFiltersType } from '../types/TransferFilters';
import { getTransfers, getTransferById } from '../api/transferService';
import { EditPzModal } from '../components/transfers/EditPzModal';
import { EditMovementModal } from '../components/transfers/EditMovementModal';

import { TransferTable } from '../components/transfers/TransferTable';
import { PaginationControls } from '../components/Pagination/PaginationControls';
import { TransferFilters } from '../components/transfers/TransferFilters';
import { TransferPageHeader } from '../components/ui/TransferPageHeader';
import { CreateWzModal } from '../components/transfers/CreatWzModal';
import { CreatePzModal } from '../components/transfers/CreatePzModal';
import { TransferTypeTabs } from '../components/transfers/TransferTypeTabs';
import { CreateTransferModal } from '../components/transfers/CreateTransferModal';
import { useAuth } from '../context/AuthContext';
import { CreateOrderModal } from '../components/transfers/CreateOrderModal';
import { getOrders } from '../api/orderService';
import type { OrderDetails } from '../api/orderService';
export const TransferPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { user } = useAuth();
  const canCreateWarehouseDocuments = user?.role === 'Admin' || user?.role === 'Magazynier';
  const canCreateTransfer = canCreateWarehouseDocuments || user?.role === 'Kierownik Budowy';
  const canOrder = user?.role === 'Kierownik Budowy';
  const [isOrderModalOpen, setIsOrderModalOpen] = useState(false);
  const [orders, setOrders] = useState<OrderDetails[]>([]);
  const [view, setView] = useState<'transfers' | 'orders'>(searchParams.get('view') === 'orders' ? 'orders' : 'transfers');
  const [ordersLoading, setOrdersLoading] = useState(false);
  const [ordersError, setOrdersError] = useState<string | null>(null);

  const fetchOrders = async () => {
    setOrdersLoading(true); setOrdersError(null);
    try { setOrders(await getOrders()); }
    catch (err) { setOrdersError(err instanceof Error ? err.message : 'Nie udało się pobrać zamówień.'); }
    finally { setOrdersLoading(false); }
  };

  useEffect(() => { if (view === 'orders') void fetchOrders(); }, [view]);

  const [transfers, setTransfers] = useState<TransferResponseDto[]>([]);
  const [editingDocument, setEditingDocument] = useState<TransferDetailsDto | null>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedType, setSelectedType] = useState<number | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [pagination, setPagination] = useState({
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  });

  const [filters, setFilters] = useState<TransferFiltersType>({
    search: '',
    dateFrom: '',
    dateTo: '',
  });

  const [isWzModalOpen, setIsWzModalOpen] = useState<boolean>(false);
  const [isPzModalOpen, setIsPzModalOpen] = useState<boolean>(false);
  const [isTransferModalOpen, setIsTransferModalOpen] = useState<boolean>(false);

  const fetchTransfers = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getTransfers(filters, selectedType, { pageNumber, pageSize });
      const lastPage = Math.max(1, data.totalPages);

      if (pageNumber > lastPage) {
        setPageNumber(lastPage);
        return;
      }

      setTransfers(data.items);
      setPagination({
        totalCount: data.totalCount,
        totalPages: data.totalPages,
        hasPreviousPage: data.hasPreviousPage,
        hasNextPage: data.hasNextPage,
      });
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Wystąpił błąd podczas pobierania transferów.'
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTransfers();
  }, [filters, selectedType, pageNumber, pageSize]);

  const handleFilterChange = (newFilters: TransferFiltersType) => {
    setFilters(newFilters);
    setPageNumber(1);
  };

  const handleTypeChange = (type: number | null) => {
    setSelectedType(type);
    setPageNumber(1);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setPageNumber(1);
  };

  const handleViewDetails = (id: string) => {
    navigate(`/transfers/${id}`);
  };

  const handleEditDocument = async (id: string) => {
    try {
      setEditingId(id);
      setError(null);
      const document = await getTransferById(id);
      if ((document.type !== 1 && document.type !== 2 && document.type !== 3) || (document.status !== 2 && document.status !== 3)) {
        setError('Ten dokument nie może być edytowany.');
        return;
      }
      setEditingDocument(document);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Nie udało się pobrać dokumentu do edycji.');
    } finally {
      setEditingId(null);
    }
  };

  return (
    <div className="space-y-6">
      <TransferPageHeader
        title="Transfery sprzętu"
        subTitle="Wydania, przyjęcia i przesunięcia sprzętu"
        onClickFetchButton={view === 'orders' ? fetchOrders : fetchTransfers}
        onClickWzButton={() => setIsWzModalOpen(true)}
        onClickPzButton={() => setIsPzModalOpen(true)}
        onClickTransferButton={() => setIsTransferModalOpen(true)}
        loadingItems={view === 'orders' ? ordersLoading : loading}
        actionButtonsVisible={canCreateTransfer}
        warehouseButtonsVisible={canCreateWarehouseDocuments}
        orderButtonVisible={canOrder}
        onClickOrderButton={() => setIsOrderModalOpen(true)}
      />

      <div className="flex gap-2 border-b border-slate-200 pb-3">
        <button type="button" onClick={() => setView('transfers')} className={`px-4 py-2 rounded-xl text-sm font-semibold ${view === 'transfers' ? 'bg-slate-900 text-white' : 'text-slate-600 hover:bg-slate-100'}`}>Ruch sprzętu</button>
        <button type="button" onClick={() => setView('orders')} className={`px-4 py-2 rounded-xl text-sm font-semibold ${view === 'orders' ? 'bg-slate-900 text-white' : 'text-slate-600 hover:bg-slate-100'}`}>Zamówienia</button>
      </div>

      {view === 'orders' ? <>
        {ordersError && <ErrorMessage message={ordersError} className="p-4" />}
        <OrderTable items={orders} isLoading={ordersLoading}
          onViewDetails={id => navigate(`/orders/${id}`)} />
      </> : <>

      {error && (
        <ErrorMessage message={error} className="p-4" />
      )}

      <TransferTypeTabs
  selectedType={selectedType}
  onSelectType={handleTypeChange}
/>

      <TransferFilters
        filters={filters}
        onFilterChange={handleFilterChange}
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
      <TransferTable
        items={transfers}
        isLoading={loading}
        onViewDetails={handleViewDetails}
        onEdit={user?.role === 'Admin' ? handleEditDocument : undefined}
        editingId={editingId}
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
      </>}

      {editingDocument && editingDocument.type === 2 && <EditPzModal transfer={editingDocument} onClose={() => setEditingDocument(null)}
        onSuccess={() => { setEditingDocument(null); void fetchTransfers(); }} />}
      {editingDocument && editingDocument.type !== 2 && <EditMovementModal transfer={editingDocument} onClose={() => setEditingDocument(null)}
        onSuccess={() => { setEditingDocument(null); void fetchTransfers(); }} />}

      {canOrder && isOrderModalOpen && <CreateOrderModal isOpen={isOrderModalOpen} onClose={() => setIsOrderModalOpen(false)}
        onSuccess={() => { setView('orders'); void fetchOrders(); }} />}

      {canCreateWarehouseDocuments && <CreateWzModal
        isOpen={isWzModalOpen}
        onClose={() => setIsWzModalOpen(false)}
        onSuccess={fetchTransfers}
      />}

      {canOrder && <CreatePzModal
        isOpen={isPzModalOpen}
        onClose={() => setIsPzModalOpen(false)}
        onSuccess={fetchTransfers}
      />}

      <CreateTransferModal
  isOpen={isTransferModalOpen}
  onClose={() => setIsTransferModalOpen(false)}
  onSuccess={fetchTransfers}
/>
    </div>
  );
};

export default TransferPage;

