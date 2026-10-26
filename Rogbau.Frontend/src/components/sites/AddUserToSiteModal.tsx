import React, { useEffect, useState } from 'react';
import {
  UserPlusIcon,
  XMarkIcon,
} from '@heroicons/react/24/outline';

import { assignUserToSite } from '../../api/siteService';
import { getAll } from '../../api/userService';
import type { UserResponseDto } from '../../types/response/UserResponseDto';

interface AddUserToSiteModalProps {
  isOpen: boolean;
  siteId: string;
  siteName: string;
  assignedUserIds?: string[];
  onClose: () => void;
  onSuccess: () => Promise<void>;
}

export const AddUserToSiteModal: React.FC<AddUserToSiteModalProps> = ({
  isOpen,
  siteId,
  siteName,
  assignedUserIds = [],
  onClose,
  onSuccess,
}) => {
  const [users, setUsers] = useState<UserResponseDto[]>([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [loadingUsers, setLoadingUsers] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const loadUsers = async () => {
      try {
        setLoadingUsers(true);
        setError(null);
        setSelectedUserId('');

        const result = await getAll();

        setUsers(result);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : 'Nie udało się pobrać listy użytkowników.'
        );
      } finally {
        setLoadingUsers(false);
      }
    };

    loadUsers();
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  const availableManagers = users.filter(
    (user) =>
      user.role === 'Kierownik Budowy' &&
      !assignedUserIds.includes(user.id)
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedUserId) {
      setError('Wybierz kierownika budowy.');
      return;
    }

    try {
      setSaving(true);
      setError(null);

      await assignUserToSite(siteId, selectedUserId);

      await onSuccess();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Nie udało się przypisać kierownika do budowy.'
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden">
        {/* HEADER */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
          <div>
            <h3 className="text-lg font-bold text-slate-900">
              Przypisz kierownika
            </h3>

            <p className="text-xs text-slate-500 mt-0.5">
              Budowa: {siteName}
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={saving}
            className="p-1 text-slate-400 hover:text-slate-600 rounded-lg transition-colors cursor-pointer disabled:opacity-50"
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="p-3 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
              {error}
            </div>
          )}

          <div>
            <label className="block text-xs font-semibold text-slate-600 uppercase tracking-wider mb-1">
              Kierownik budowy
            </label>

            {loadingUsers ? (
              <div className="px-3 py-3 border border-slate-200 rounded-xl text-sm text-slate-400">
                Ładowanie kierowników...
              </div>
            ) : availableManagers.length === 0 ? (
              <div className="p-4 bg-slate-50 border border-slate-200 rounded-xl">
                <p className="text-sm text-slate-500">
                  Brak dostępnych kierowników budowy.
                </p>
              </div>
            ) : (
              <select
                required
                value={selectedUserId}
                onChange={(e) => setSelectedUserId(e.target.value)}
                className="w-full px-3 py-2 border border-slate-200 rounded-xl text-sm focus:outline-none focus:border-amber-500 bg-white"
              >
                <option value="">
                  Wybierz kierownika...
                </option>

                {availableManagers.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.firstName} {user.lastName}
                    {user.email ? ` - ${user.email}` : ''}
                  </option>
                ))}
              </select>
            )}
          </div>

          {selectedUserId && (
            <div className="flex items-start gap-3 p-3 bg-amber-50 border border-amber-200/80 rounded-xl">
              <UserPlusIcon className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />

              <div>
                <p className="text-sm font-semibold text-slate-800">
                  Przypisanie kierownika
                </p>

                <p className="text-xs text-slate-600 mt-0.5">
                  Wybrany użytkownik zostanie przypisany do budowy{' '}
                  <span className="font-semibold">
                    {siteName}
                  </span>.
                </p>
              </div>
            </div>
          )}

          {/* BUTTONS */}
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-200">
            <button
              type="button"
              onClick={onClose}
              disabled={saving}
              className="px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100 rounded-xl transition-colors cursor-pointer disabled:opacity-50"
            >
              Anuluj
            </button>

            <button
              type="submit"
              disabled={
                saving ||
                loadingUsers ||
                !selectedUserId ||
                availableManagers.length === 0
              }
              className="inline-flex items-center gap-2 px-4 py-2 text-sm font-bold bg-amber-500 hover:bg-amber-600 text-slate-950 rounded-xl shadow-sm transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <UserPlusIcon className="w-4 h-4" />

              {saving
                ? 'Przypisywanie...'
                : 'Przypisz kierownika'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};