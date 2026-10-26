import { useEffect, useState } from "react";

import { UserTable } from "../components/users/UserTable";
import { AddUserModal } from "../components/users/AddUserModal";
import { deleteUser, getAll } from "../api/userService";
import type { UserResponseDto } from "../types/response/UserResponseDto";
import { DeleteConfirmModal } from "../components/ui/DeleteConfirmModal";
import { ChangePasswordByAdminModal } from "../components/users/ChangePasswordByAdminModal";
import { PageHeader } from "../components/ui/PageHeader";
import { EditUserModal } from "../components/users/EditUserModal";

export const UserPage: React.FC = () => {
    const [users, setUsers] = useState<UserResponseDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [isAddModalOpen, setIsAddModalOpen] = useState(false);

     const [deleteTarget, setDeleteTarget] = useState<{ id: string} | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  const [selectedUser, setSelectedUser] = useState<UserResponseDto | null>(null);
const [isChangePasswordModalOpen, setIsChangePasswordModalOpen] = useState(false);

const [isEditModalOpen, setIsEditModalOpen] = useState(false);


    const fetchUsers = async () => {
        try {
            setIsLoading(true);
            setError(null);

            const data = await getAll();
            setUsers(data);
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Wystąpił błąd podczas pobierania użytkowników."
            );
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    const handleOpenDeleteModal = (id: string) => {
    setDeleteTarget({ id});
  };

  const handleConfirmDelete = async () => {
    if (!deleteTarget) return;

    try {
      setIsDeleting(true);
      await deleteUser(deleteTarget.id);
      setDeleteTarget(null);
      fetchUsers();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Nie udało się usunąć dostawcy.');
    } finally {
      setIsDeleting(false);
    }
  };

  const handleChangePassword = (user: UserResponseDto) => {
    setSelectedUser(user);
    setIsChangePasswordModalOpen(true);
};

const handleEditUser = (user: UserResponseDto) => {
    setSelectedUser(user);
    setIsEditModalOpen(true);
};

    return (
        <div className="space-y-6">

            <PageHeader
            title="Baza użytkowników"
            subTile="Zarządzaj użytkownikami w systemie"
            onclickActionButton={() => setIsAddModalOpen(true)}
            onClickFetchButton={fetchUsers}
            actionButtonText="Dodaj użytkownika"
            actionButtonVisible = {true}
            loadingItems = {isLoading}
            />

            {error && (
                <div
                    role="alert"
                    className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl text-sm"
                >
                    {error}
                </div>
            )}

            <UserTable
                items={users}
                isLoading={isLoading}
                onDelete={handleOpenDeleteModal}
                onChangePassword={handleChangePassword}
                onEdit={handleEditUser}
            />

            <AddUserModal
                isOpen={isAddModalOpen}
                onClose={() => setIsAddModalOpen(false)}
                onSuccess={fetchUsers}
            />

             <DeleteConfirmModal
        isOpen={!!deleteTarget}
        title="Usuń użytkownika"
        message={`Czy na pewno chcesz usunąć tego użytkownika  "? Operacji nie można cofnąć.`}
        isDeleting={isDeleting}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleConfirmDelete}
      />

     {selectedUser && (
    <ChangePasswordByAdminModal
        isOpen={isChangePasswordModalOpen}
        userId={selectedUser.id}
        userName={`${selectedUser.firstName} ${selectedUser.lastName}`}
        onClose={() => {
            setIsChangePasswordModalOpen(false);
            setSelectedUser(null);
        }}
    />
)}

{selectedUser && (
    <EditUserModal
        isOpen={isEditModalOpen}
        user={selectedUser}
        onClose={() => {
            setIsEditModalOpen(false);
            setSelectedUser(null);
        }}
        onSuccess={fetchUsers}
    />
)}


        </div>
    );
};