import React, { useState } from "react";
import {
    KeyIcon,
    XMarkIcon,
    InformationCircleIcon,
    EnvelopeIcon,
} from "@heroicons/react/24/outline";
import { changePassword } from "../../api/userService";

interface ChangePasswordByUserModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSuccess: () => void;
    title: string;
    subTitle: string;
    email: string;
}

export const ChangePasswordByUserModal: React.FC<
    ChangePasswordByUserModalProps
> = ({
    isOpen,
    onClose,
    onSuccess,
    title,
    subTitle,
    email,
}) => {
    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmNewPassword, setConfirmNewPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!currentPassword || !newPassword || !confirmNewPassword) {
            setError("Uzupełnij wszystkie pola");
            return;
        }

        if (newPassword !== confirmNewPassword) {
            setError("Hasła nie są takie same");
            return;
        }

        try {
            setIsSubmitting(true);
            setError(null);

            await changePassword({
                currentPassword,
                newPassword,
            });

            setCurrentPassword("");
            setNewPassword("");
            setConfirmNewPassword("");

            onSuccess();
            
        } catch (err) {
            setError(
                err instanceof Error
                    ? err.message
                    : "Wystąpił błąd podczas zmiany hasła."
            );
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/40 p-4">
            <div className="w-full max-w-md bg-white rounded-xl shadow-xl border border-slate-200 overflow-hidden">

                {/* Header */}
                <div className="flex items-start justify-between px-6 py-4 border-b border-slate-200">
                    <div className="flex items-start gap-3">
                        <div className="flex items-center justify-center w-10 h-10 rounded-lg bg-amber-100 shrink-0">
                            <KeyIcon className="w-5 h-5 text-amber-600" />
                        </div>

                        <div>
                            <h2 className="text-lg font-bold text-slate-900">
                                {title}
                            </h2>

                            <p className="text-sm text-slate-500 mt-0.5">
                                {subTitle}
                            </p>
                        </div>
                    </div>

                    <button
                        type="button"
                        onClick={onClose}
                        disabled={isSubmitting}
                        className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors cursor-pointer disabled:opacity-50"
                        title="Zamknij"
                    >
                        <XMarkIcon className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit}>
                    <div className="p-6 space-y-5">

                        {/* Email */}
                        <div className="flex items-center gap-3 p-3.5 bg-slate-50 border border-slate-200 rounded-lg">
                            <EnvelopeIcon className="w-5 h-5 text-slate-400 shrink-0" />

                            <div className="min-w-0">
                                <p className="text-xs font-medium text-slate-500">
                                    Konto użytkownika
                                </p>

                                <p className="text-sm font-semibold text-slate-800 truncate">
                                    {email}
                                </p>
                            </div>
                        </div>

                        {/* Error */}
                        {error && (
                            <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
                                {error}
                            </div>
                        )}

                        {/* Current password */}
                        <div>
                            <label
                                htmlFor="currentPassword"
                                className="block text-sm font-semibold text-slate-700 mb-1.5"
                            >
                                Aktualne hasło
                            </label>

                            <input
                                id="currentPassword"
                                type="password"
                                value={currentPassword}
                                onChange={(e) =>
                                    setCurrentPassword(e.target.value)
                                }
                                placeholder="Wprowadź aktualne hasło"
                                autoComplete="current-password"
                                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-lg text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-colors"
                            />
                        </div>

                        {/* New password */}
                        <div>
                            <label
                                htmlFor="newPassword"
                                className="block text-sm font-semibold text-slate-700 mb-1.5"
                            >
                                Nowe hasło
                            </label>

                            <input
                                id="newPassword"
                                type="password"
                                value={newPassword}
                                onChange={(e) =>
                                    setNewPassword(e.target.value)
                                }
                                placeholder="Wprowadź nowe hasło"
                                autoComplete="new-password"
                                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-lg text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-colors"
                            />
                        </div>

                        {/* Confirm password */}
                        <div>
                            <label
                                htmlFor="confirmNewPassword"
                                className="block text-sm font-semibold text-slate-700 mb-1.5"
                            >
                                Powtórz nowe hasło
                            </label>

                            <input
                                id="confirmNewPassword"
                                type="password"
                                value={confirmNewPassword}
                                onChange={(e) =>
                                    setConfirmNewPassword(e.target.value)
                                }
                                placeholder="Powtórz nowe hasło"
                                autoComplete="new-password"
                                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-lg text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-colors"
                            />
                        </div>

                        {/* Info */}
                        <div className="flex gap-3 p-3.5 bg-amber-50 border border-amber-200 rounded-lg">
                            <InformationCircleIcon className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />

                            <p className="text-sm text-amber-800">
                                Po zmianie hasła używaj nowego hasła podczas
                                kolejnego logowania do systemu.
                            </p>
                        </div>
                    </div>

                    {/* Footer */}
                    <div className="flex items-center justify-end gap-3 px-6 py-4 bg-slate-50 border-t border-slate-200">
                        <button
                            type="button"
                            onClick={onClose}
                            disabled={isSubmitting}
                            className="px-4 py-2.5 text-sm font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer disabled:opacity-50"
                        >
                            Anuluj
                        </button>

                        <button
                            type="submit"
                            disabled={isSubmitting}
                            className="flex items-center gap-2 px-4 py-2.5 bg-amber-500 hover:bg-amber-600 text-slate-950 rounded-lg text-sm font-bold transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <KeyIcon className="w-4 h-4" />

                            {isSubmitting
                                ? "Zmienianie..."
                                : "Zmień hasło"}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};