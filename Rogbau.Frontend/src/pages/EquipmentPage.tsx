import { ErrorMessage } from "../components/ui/ErrorMessage";
import React, { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import type { EquipmentResponse } from "../types/response/EquipmentResponse";
import type { EquipmentFilters as EquipmentFiltersType } from "../types/equipmentFilter";
import type { CategoryDto } from "../types/response/CategoryDto";
import { PaginationControls } from "../components/Pagination/PaginationControls";
import {
  getEquipments,
  getCategories,
  deleteEquipment,
} from "../api/equipmentService";
import { EquipmentTable } from "../components/equipment/EquipmentTable";
import { EquipmentFilters } from "../components/equipment/EquipmentFilters";
import { EquipmentCategoryTabs } from "../components/equipment/EquipmentCategoryTabs";
import { AddEquipmentModal } from "../components/equipment/AddEquipmentModal";
import { DeleteEquipmentModal } from "../components/equipment/DeleteEquipmentModal";
import { EditEquipmentModal } from "../components/equipment/EditEquipmentModal";
import { useAuth } from "../context/AuthContext";
import { PageHeader } from "../components/ui/PageHeader";

export const EquipmentPage: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useAuth();

  const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [pagination, setPagination] = useState({
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  });

  const [isAddModalOpen, setIsAddModalOpen] = useState<boolean>(false);

  const [deleteTarget, setDeleteTarget] = useState<{
    id: string;
    name: string;
  } | null>(null);

  const [editTarget, setEditTarget] =
    useState<EquipmentResponse | null>(null);

  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  const [filters, setFilters] = useState<EquipmentFiltersType>({
    searchQuery: "",
    isActive: undefined,
  });

  const [selectedCategoryId, setSelectedCategoryId] =
    useState<string>("ALL");

  const [selectedSubCategoryId, setSelectedSubCategoryId] =
    useState<string>("ALL");

  // Pobranie kategorii przy starcie
  useEffect(() => {
    getCategories()
      .then(setCategories)
      .catch((err) =>
        console.warn("Błąd pobierania kategorii:", err)
      );
  }, []);

  // Pobieranie sprzętu
  const fetchEquipments = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getEquipments({
        searchQuery: filters.searchQuery,
        isActive: filters.isActive,
        categoryId: selectedCategoryId,
        subCategoryId: selectedSubCategoryId,
        pageNumber,
        pageSize,
      });

      // Powrót do istniejącej strony, np. po usunięciu ostatniej pozycji.
      const lastPage = Math.max(1, data.totalPages);

      if (pageNumber > lastPage) {
        setPageNumber(lastPage);
        return;
      }

      setEquipments(data.items);

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
          : "Wystąpił błąd podczas pobierania danych."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchEquipments();
  }, [
    filters,
    selectedCategoryId,
    selectedSubCategoryId,
    pageNumber,
    pageSize,
  ]);

  const handleCategoryChange = (categoryId: string) => {
    setSelectedCategoryId(categoryId);
    setSelectedSubCategoryId("ALL");
    setPageNumber(1);
  };

  const handleSubCategoryChange = (subCategoryId: string) => {
    setSelectedSubCategoryId(subCategoryId);
    setPageNumber(1);
  };

  const handleFilterChange = (newFilters: EquipmentFiltersType) => {
    setFilters(newFilters);
    setPageNumber(1);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setPageNumber(1);
  };

  const handleOpenDeleteModal = (id: string, name: string) => {
    setDeleteTarget({ id, name });
  };

  const handleOpenEditModal = (equipment: EquipmentResponse) => {
    setEditTarget(equipment);
  };

  const handleConfirmDelete = async () => {
    if (!deleteTarget || user?.role !== "Admin" || isDeleting) return;

    try {
      setIsDeleting(true);

      await deleteEquipment(deleteTarget.id);

      setDeleteTarget(null);
      await fetchEquipments();
    } catch (err) {
      setDeleteTarget(null);
      setError(
        err instanceof Error
          ? err.message
          : "Nie udało się trwale usunąć sprzętu."
      );
    } finally {
      setIsDeleting(false);
    }
  };

  const handleViewDetails = (id: string) => {
    navigate(`/equipment/${id}`);
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Magazyn Sprzętu"
        subTile="Zarządzaj parkiem maszynowym i sprzętem budowlanym"
        onClickFetchButton={fetchEquipments}
        onclickActionButton={() => setIsAddModalOpen(true)}
        actionButtonText="Dodaj Sprzęt"
        loadingItems={loading}
        actionButtonVisible={user?.role === "Admin"}
      />

      {user?.role === "Admin" && (
        <Link
          to="/categories"
          className="inline-block px-4 py-2 border border-slate-200 bg-white rounded-xl text-sm font-semibold"
        >
          Zarządzaj kategoriami
        </Link>
      )}

      <EquipmentCategoryTabs
        categories={categories}
        selectedCategoryId={selectedCategoryId}
        selectedSubCategoryId={selectedSubCategoryId}
        onSelectCategory={handleCategoryChange}
        onSelectSubCategory={handleSubCategoryChange}
      />

      <EquipmentFilters
        filters={filters}
        onFilterChange={handleFilterChange}
      />

      {error && (
        <ErrorMessage message={error} className="p-4" />
      )}

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
      <EquipmentTable
        items={equipments}
        isLoading={loading}
        onDelete={handleOpenDeleteModal}
        onViewDetails={handleViewDetails}
        onEdit={handleOpenEditModal}
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

      <EditEquipmentModal
        isOpen={!!editTarget}
        equipment={editTarget}
        onClose={() => setEditTarget(null)}
        onSuccess={fetchEquipments}
      />

      <AddEquipmentModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        onSuccess={fetchEquipments}
      />

      <DeleteEquipmentModal
        isOpen={!!deleteTarget}
        equipmentName={deleteTarget?.name ?? ""}
        isDeleting={isDeleting}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleConfirmDelete}
      />
    </div>
  );
};

export default EquipmentPage;