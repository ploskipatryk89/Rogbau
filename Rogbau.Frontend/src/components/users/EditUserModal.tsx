import { useEffect, useState } from "react";
import { XMarkIcon, PencilSquareIcon } from "@heroicons/react/24/outline";

import { getAllRoles } from "../../api/roleService";
import { editUser } from "../../api/userService";

import type { RoleResponseDto } from "../../types/response/RoleDto";
import type { UserResponseDto } from "../../types/response/UserResponseDto";
import type { EditUserRequest } from "../../types/request/EditUserRequest";

interface EditUserModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
    user: UserResponseDto | null;
}

export const EditUserModal: React.FC<EditUserModalProps> = ({
    isOpen,
    onClose,
    onSuccess,
    user,
}) => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [email, setEmail] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("");
    const [role, setRole] = useState("");

    const [roles, setRoles] = useState<RoleResponseDto[]>([]);

    useEffect(() => {
        if (!isOpen || !user) return;

        // Wypełnienie formularza aktualnymi danymi użytkownika
        setFirstName(user.firstName ?? "");
        setLastName(user.lastName ?? "");
        setEmail(user.email ?? "");
        setPhoneNumber(user.phoneNumber ?? "");
        setRole(user.role ?? "");

        setError(null);
        setLoading(false);

        const loadRoles = async () => {
            try {
                const data = await getAllRoles();
                setRoles(data);
            } catch (error) {
                console.error(error);
                setError("Nie udało się pobrać ról.");
            }
        };

        loadRoles();
    }, [isOpen, user]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (!user) return;

        try {
            setLoading(true);
            setError(null);

            const data: EditUserRequest = {
                id: user.id,
                firstName: firstName.trim(),
                lastName: lastName.trim(),
                email: email.trim(),
                phoneNumber: phoneNumber.trim(),
                role,
            };

            await editUser(user.id, data);

            onSuccess();
            onClose();
        } catch (error) {
            console.error(error);
            setError(error instanceof Error ? error.message : "Nie udało się zaktualizować użytkownika.");
        } finally {
            setLoading(false);
        }
    };

    if (!isOpen || !user) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
            <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden">

                {/* Nagłówek */}
                <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
                    <div>
                        <h3 className="text-lg font-bold text-slate-900">
                            Edytuj użytkownika
                        </h3>

                        <p className="text-xs text-slate-500 mt-0.5">
                            Zmień dane wybranego użytkownika
                        </p>
                    </div>

                    <button
                        type="button"
                        onClick={onClose}
                        disabled={loading}
                        className="p-1 text-slate-400 hover:text-slate-600 rounded-lg transition-colors cursor-pointer disabled:opacity-50"
                    >
                        <XMarkIcon className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit} className="p-6 space-y-4">

                    {/* Informacja */}
                    <div className="flex items-start gap-3 p-3.5 bg-blue-50 border border-blue-200 rounded-xl">
                        <PencilSquareIcon className="w-5 h-5 text-blue-600 shrink-0 mt-0.5" />

                        <div>
                            <p className="text-sm font-semibold text-blue-900">
                                Edycja użytkownika
                            </p>

                            <p className="text-xs text-blue-700 mt-0.5">
                                Zmień wybrane dane użytkownika i zapisz zmiany.
                                Hasło użytkownika nie zostanie zmienione.
                            </p>
                        </div>
                    </div>

                    {/* Błąd */}
                    {error && (
                        <div className="p-3 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
                            {error}
                        </div>
                    )}

                    {/* Imię + nazwisko */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div>
                            <label
                                htmlFor="edit-firstName"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Imię
                            </label>

                            <input
                                id="edit-firstName"
                                type="text"
                                required
                                value={firstName}
                                onChange={(e) => setFirstName(e.target.value)}
                                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
                                placeholder="Jan"
                            />
                        </div>

                        <div>
                            <label
                                htmlFor="edit-lastName"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Nazwisko
                            </label>

                            <input
                                id="edit-lastName"
                                type="text"
                                required
                                value={lastName}
                                onChange={(e) => setLastName(e.target.value)}
                                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
                                placeholder="Kowalski"
                            />
                        </div>
                    </div>

                    {/* Email */}
                    <div>
                        <label
                            htmlFor="edit-email"
                            className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                        >
                            Email
                        </label>

                        <input
                            id="edit-email"
                            type="email"
                            required
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
                            placeholder="jan.kowalski@example.com"
                        />
                    </div>

                    {/* Telefon + rola */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div>
                            <label
                                htmlFor="edit-phoneNumber"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Telefon
                            </label>

                            <input
                                id="edit-phoneNumber"
                                type="tel"
                                value={phoneNumber}
                                onChange={(e) => setPhoneNumber(e.target.value)}
                                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
                                placeholder="+48 123 456 789"
                            />
                        </div>

                        <div>
                            <label
                                htmlFor="edit-role"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Rola
                            </label>

                            <select
                                id="edit-role"
                                required
                                value={role}
                                onChange={(e) => setRole(e.target.value)}
                                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white"
                            >
                                <option value="">Wybierz rolę...</option>

                                {roles.map((item) => (
                                    <option
                                        key={item.id}
                                        value={item.name}
                                    >
                                        {item.name}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    {/* Przyciski */}
                    <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-200">
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={loading}
                            className="px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100 rounded-xl transition-colors cursor-pointer disabled:opacity-50"
                        >
                            Anuluj
                        </button>

                        <button
                            type="submit"
                            disabled={loading}
                            className="px-4 py-2 text-sm font-bold bg-amber-500 hover:bg-amber-600 text-slate-950 rounded-xl shadow-sm transition-all cursor-pointer disabled:opacity-50"
                        >
                            {loading ? "Zapisywanie..." : "Zapisz zmiany"}
                        </button>
                    </div>

                </form>
            </div>
        </div>
    );
};
