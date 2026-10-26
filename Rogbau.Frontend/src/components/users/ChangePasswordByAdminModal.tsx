import React, { useState } from "react";
import {
    KeyIcon,
    XMarkIcon,
    InformationCircleIcon,
} from "@heroicons/react/24/outline";
import { resetPassword } from "../../api/userService";

interface ChangePasswordByAdminModalProps {
    isOpen: boolean;
    onClose: () => void;
    userId: string;
    userName?: string;
}

export const ChangePasswordByAdminModal: React.FC<
    ChangePasswordByAdminModalProps
> = ({
    isOpen,
    onClose,
    userId,
    userName,
}) => {
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!newPassword || !confirmPassword) {
            setError("Uzupełnij wszystkie pola.");
            return;
        }

        if (newPassword !== confirmPassword) {
            setError("Hasła nie są takie same.");
            return;
        }

        try {
            setIsSubmitting(true);
            setError(null);

            await resetPassword(userId, {
                newPassword,
            });

            setNewPassword("");
            setConfirmPassword("");
            onClose();
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
                <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
                    <div className="flex items-center gap-3">
                        <div className="flex items-center justify-center w-10 h-10 rounded-lg bg-amber-100">
                            <KeyIcon className="w-5 h-5 text-amber-600" />
                        </div>

                        <div>
                            <h2 className="text-lg font-bold text-slate-900">
                                Zmień hasło
                            </h2>

                            {userName && (
                                <p className="text-sm text-slate-500">
                                    Użytkownik: {userName}
                                </p>
                            )}
                        </div>
                    </div>

                    <button
                        type="button"
                        onClick={onClose}
                        className="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors cursor-pointer"
                        title="Zamknij"
                    >
                        <XMarkIcon className="w-5 h-5" />
                    </button>
                </div>

                <form onSubmit={handleSubmit}>
                    <div className="p-6 space-y-5">
                        {error && (
                            <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
                                {error}
                            </div>
                        )}

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
                                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-lg text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-colors"
                            />
                        </div>

                        <div>
                            <label
                                htmlFor="confirmPassword"
                                className="block text-sm font-semibold text-slate-700 mb-1.5"
                            >
                                Powtórz nowe hasło
                            </label>

                            <input
                                id="confirmPassword"
                                type="password"
                                value={confirmPassword}
                                onChange={(e) =>
                                    setConfirmPassword(e.target.value)
                                }
                                placeholder="Powtórz nowe hasło"
                                className="w-full px-3.5 py-2.5 border border-slate-300 rounded-lg text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500 transition-colors"
                            />
                        </div>

                        <div className="flex gap-3 p-3.5 bg-amber-50 border border-amber-200 rounded-lg">
                            <InformationCircleIcon className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />

                            <p className="text-sm text-amber-800">
                                Użytkownik otrzyma hasło tymczasowe i będzie
                                musiał je zmienić przy następnym logowaniu.
                            </p>
                        </div>
                    </div>

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