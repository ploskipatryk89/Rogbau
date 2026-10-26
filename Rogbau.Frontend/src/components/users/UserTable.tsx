import React from "react";
import type { UserResponseDto } from "../../types/response/UserResponseDto";
import {
    UserIcon,
    KeyIcon,
    PencilSquareIcon,
    TrashIcon,
} from "@heroicons/react/24/outline";
import { IsEmpty } from "../ui/IsEmpty";
import { useAuth } from "../../context/AuthContext";

interface UserTableProps {
    items: UserResponseDto[];
    isLoading: boolean;
    onDelete: (id: string) => void;
    onChangePassword: (user: UserResponseDto) => void;
        onEdit: (user: UserResponseDto) => void;

}

export const UserTable: React.FC<UserTableProps> = ({
    items,
    isLoading,
    onDelete,
    onChangePassword,
    onEdit
}) => {
    const { user } = useAuth();

    if (isLoading) {
        return (
            <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-12 text-center text-slate-500">
                Ładowanie listy użytkowników...
            </div>
        );
    }

    if (items.length === 0) {
        return (
            <IsEmpty
                title="Brak użytkowników w bazie"
                subTitle={user?.role === 'Admin' ? 'Kliknij "Dodaj użytkownika", aby dodać nowego pracownika.' : undefined}
                icon={UserIcon}
            />
        );
    }

    return (
        <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
            <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                    <thead>
                        <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                            <th className="py-3.5 px-4">Imię</th>
                            <th className="py-3.5 px-4">Nazwisko</th>
                            <th className="py-3.5 px-4">Email</th>
                            <th className="py-3.5 px-4">Telefon</th>
                            <th className="py-3.5 px-4">Rola</th>

                            {user?.role === "Admin" && (
                                <th className="py-3.5 px-4 text-right w-36">
                                    Akcje
                                </th>
                            )}
                        </tr>
                    </thead>

                    <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
                        {items.map((item) => (
                            <tr
                                key={item.id}
                                className="hover:bg-slate-50/80 transition-colors"
                            >
                                <td className="py-3.5 px-4 font-semibold text-slate-900">
                                    {item.firstName}
                                </td>

                                <td className="py-3.5 px-4 font-semibold text-slate-900">
                                    {item.lastName}
                                </td>

                                <td className="py-3.5 px-4 text-slate-600">
                                    {item.email}
                                </td>

                                <td className="py-3.5 px-4 text-slate-500">
                                    {item.phoneNumber}
                                </td>

                                <td className="py-3.5 px-4">
                                    {item.role}
                                </td>

                                {user?.role === "Admin" && (
                                    <td className="py-3.5 px-4 text-right">
                                        <div className="flex items-center justify-end gap-2">
                                            <button
                                                type="button"
                                                onClick={() => onEdit(item)}
                                                className="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors cursor-pointer"
                                                title="Edytuj"
                                            >
                                                <PencilSquareIcon className="w-4 h-4" />
                                            </button>

                                            {user.id !== item.id && (
                                                <>
                                                    <button
                                                        type="button"
                                                        onClick={() =>
                                                            onChangePassword(item)
                                                        }
                                                        className="p-1.5 text-slate-400 hover:text-amber-600 hover:bg-amber-50 rounded-lg transition-colors cursor-pointer"
                                                        title="Zmień hasło"
                                                    >
                                                        <KeyIcon className="w-4 h-4" />
                                                    </button>

                                                    <button
                                                        type="button"
                                                        onClick={() =>
                                                            onDelete(item.id)
                                                        }
                                                        className="p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors cursor-pointer"
                                                        title="Usuń"
                                                    >
                                                        <TrashIcon className="w-4 h-4" />
                                                    </button>
                                                </>
                                            )}
                                        </div>
                                    </td>
                                )}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};
