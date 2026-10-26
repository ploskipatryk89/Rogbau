import { useEffect, useState } from "react";
import { XMarkIcon, InformationCircleIcon } from "@heroicons/react/24/outline";

import { getAllRoles } from "../../api/roleService";
import { create } from "../../api/userService";
import type { RoleResponseDto } from "../../types/response/RoleDto";
import type { CreateUserRequest } from "../../types/request/CreateUserRequest";

interface AddUserModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
}

export const AddUserModal: React.FC<AddUserModalProps> = ({
    isOpen,
    onClose,
    onSuccess,
}) => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [email, setEmail] = useState("");
    const [phoneNumber, setPhoneNumber] = useState("");
    const [password, setPassword] = useState("");
    const [role, setRole] = useState("");

    const [roles, setRoles] = useState<RoleResponseDto[]>([]);

    useEffect(() => {
        if (!isOpen) return;

        setFirstName("");
        setLastName("");
        setEmail("");
        setPhoneNumber("");
        setPassword("");
        setRole("");
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
    }, [isOpen]);

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        try {
            setLoading(true);
            setError(null);

            const data: CreateUserRequest = {
                firstName: firstName.trim(),
                lastName: lastName.trim(),
                email: email.trim(),
                telephoneNumber: phoneNumber.trim(),
                password,
                role,
            };

            await create(data);

            onSuccess();
            onClose();
        } catch (error) {
            console.error(error);
            setError("Nie udało się dodać użytkownika.");
        } finally {
            setLoading(false);
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
            <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden">

                {/* Nagłówek */}
                <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
                    <h3 className="text-lg font-bold text-slate-900">
                        Dodaj Nowego Użytkownika
                    </h3>

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

                    {/* Informacja o haśle tymczasowym */}
                    <div className="flex items-start gap-3 p-3.5 bg-amber-50 border border-amber-200 rounded-xl">
                        <InformationCircleIcon className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />

                        <div>
                            <p className="text-sm font-semibold text-amber-900">
                                Hasło tymczasowe
                            </p>

                            <p className="text-xs text-amber-700 mt-0.5">
                                Ustaw hasło tymczasowe dla nowego użytkownika.
                                Po pierwszym zalogowaniu użytkownik będzie zobowiązany
                                do ustawienia własnego hasła.
                            </p>
                        </div>
                    </div>

                    {error && (
                        <div className="p-3 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
                            {error}
                        </div>
                    )}

                    {/* Imię + nazwisko */}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <div>
                            <label
                                htmlFor="firstName"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Imię
                            </label>

                            <input
                                id="firstName"
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
                                htmlFor="lastName"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Nazwisko
                            </label>

                            <input
                                id="lastName"
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
                            htmlFor="email"
                            className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                        >
                            Email
                        </label>

                        <input
                            id="email"
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
                                htmlFor="phoneNumber"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Telefon
                            </label>

                            <input
                                id="phoneNumber"
                                type="tel"
                                value={phoneNumber}
                                onChange={(e) => setPhoneNumber(e.target.value)}
                                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
                                placeholder="+48 123 456 789"
                            />
                        </div>

                        <div>
                            <label
                                htmlFor="role"
                                className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                            >
                                Rola
                            </label>

                            <select
                                id="role"
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

                    {/* Hasło tymczasowe */}
                    <div>
                        <label
                            htmlFor="password"
                            className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1"
                        >
                            Hasło tymczasowe
                        </label>

                        <input
                            id="password"
                            type="password"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            autoComplete="new-password"
                            className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500"
                            placeholder="Wprowadź hasło tymczasowe"
                        />

                        <p className="text-xs text-slate-400 mt-1.5">
                            Użytkownik będzie musiał zmienić to hasło po pierwszym
                            zalogowaniu.
                        </p>
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
                            {loading ? "Dodawanie..." : "Dodaj użytkownika"}
                        </button>
                    </div>

                </form>
            </div>
        </div>
    );
};