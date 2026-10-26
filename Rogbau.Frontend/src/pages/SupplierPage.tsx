import { ErrorMessage } from "../components/ui/ErrorMessage";
import { useAuth } from "../context/AuthContext";
import React, { useEffect, useState } from 'react';
import type { SupplierResponseDto } from '../types/response/SupplierResponseDto';
import { getSuppliers } from '../api/supplierService';
import { deleteSupplier } from '../api/supplierService';
import { AddSupplierModal } from '../components/suppliers/AddSupplierModal';
import { EditSupplierModal } from '../components/suppliers/EditSupplierModal';
import { SupplierTable } from '../components/suppliers/SupplierTable';

import { DeleteConfirmModal } from '../components/ui/DeleteConfirmModal';
import { PageHeader } from '../components/ui/PageHeader';

export const SupplierPage: React.FC = () => {
  const { user } = useAuth();
  const [suppliers, setSuppliers] = useState<SupplierResponseDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [isAddModalOpen, setIsAddModalOpen] = useState<boolean>(false);
  const [editingSupplier, setEditingSupplier] = useState<SupplierResponseDto | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  const fetchSuppliers = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getSuppliers();
      setSuppliers(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Wystąpił błąd podczas pobierania dostawców.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSuppliers();
  }, []);

  const handleOpenDeleteModal = (id: string, name: string) => {
    setDeleteTarget({ id, name });
  };

  const handleConfirmDelete = async () => {
    if (!deleteTarget) return;

    try {
      setIsDeleting(true);
      await deleteSupplier(deleteTarget.id);
      setDeleteTarget(null);
      fetchSuppliers();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Nie udało się usunąć dostawcy.');
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Nagłówek */}
      <PageHeader
      title='Baza dostawców'
      subTile='Zarządzaj partnerami handlowymi i firmami dostarczającymi sprzęt'
      onClickFetchButton={fetchSuppliers}
      actionButtonText='Dodaj Dostawcę'
      actionButtonVisible = {user?.role === 'Admin'}
      loadingItems = {loading}
      onclickActionButton={() => setIsAddModalOpen(true)}
      />

      {error && (
        <ErrorMessage message={error} className="p-4" />
      )}

      {/* Tabela */}
      <SupplierTable 
        items={suppliers} 
        isLoading={loading} 
        onDelete={handleOpenDeleteModal}
        onEdit={user?.role === 'Admin' ? setEditingSupplier : undefined}
      />

      {/* Modal dodawania dostawcy (utwórz odpowiednio według potrzeb) */}
      <AddSupplierModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        onSuccess={fetchSuppliers}
      />

      <EditSupplierModal supplier={editingSupplier} onClose={() => setEditingSupplier(null)} onSuccess={fetchSuppliers} />

      <DeleteConfirmModal
        isOpen={!!deleteTarget}
        title="Usuń dostawcę"
        message={`Czy na pewno chcesz usunąć dostawcę "${deleteTarget?.name}"? Operacji nie można cofnąć.`}
        isDeleting={isDeleting}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleConfirmDelete}
      />
    </div>
  );
};
