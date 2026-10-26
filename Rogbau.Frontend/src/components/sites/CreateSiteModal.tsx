import React, { useState, useEffect } from 'react';
import { SiteStatus, SiteStatusLabels } from '../../types/enums/SiteStatus';
import type { CreateSiteCommand } from '../../types/request/CreateSiteRequest';
import type { UserResponseDto } from '../../types/response/UserResponseDto';
import { createSite } from '../../api/siteService';
import { getAll } from '../../api/userService';
import {
  XMarkIcon,
  BuildingOfficeIcon,
  UserIcon
} from '@heroicons/react/24/outline';

interface CreateSiteModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CreateSiteModal: React.FC<CreateSiteModalProps> = ({
  isOpen,
  onClose,
  onSuccess
}) => {
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [status, setStatus] = useState<SiteStatus>(SiteStatus.Planned);

  const [users, setUsers] = useState<UserResponseDto[]>([]);
  const [selectedUserIds, setSelectedUserIds] = useState<string[]>([]);

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoadingUsers, setIsLoadingUsers] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;

    setName('');
    setAddress('');
    setStartDate('');
    setEndDate('');
    setStatus(SiteStatus.Planned);
    setSelectedUserIds([]);
    setError(null);

    const loadUsers = async () => {
      try {
        setIsLoadingUsers(true);

        const data = await getAll();

        const managers = data.filter(
          user => user.role === 'Kierownik Budowy'
        );

        setUsers(managers);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : 'Nie udało się pobrać kierowników budowy.'
        );
      } finally {
        setIsLoadingUsers(false);
      }
    };

    loadUsers();
  }, [isOpen]);

  const toggleUser = (userId: string) => {
    setSelectedUserIds(prev =>
      prev.includes(userId)
        ? prev.filter(id => id !== userId)
        : [...prev, userId]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!name.trim()) {
      setError('Nazwa budowy jest wymagana.');
      return;
    }

    try {
      setIsSubmitting(true);

      const command: CreateSiteCommand = {
        name: name.trim(),
        address: address.trim() ? address.trim() : null,
        startDate: startDate ? startDate : null,
        endDate: endDate ? endDate : null,
        status: Number(status) as SiteStatus,
        userIds: selectedUserIds
      };

      await createSite(command);

      onSuccess();
      onClose();
    } catch (err: any) {
      setError(
        err instanceof Error
          ? err.message
          : 'Nie udało się dodać budowy.'
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-lg overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100 bg-slate-50/50">
          <div className="flex items-center gap-2">
            <BuildingOfficeIcon className="w-5 h-5 text-amber-500" />
            <h3 className="text-lg font-bold text-slate-900">
              Dodaj Nową Budowę
            </h3>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="p-1 text-slate-400 hover:text-slate-600 rounded-lg hover:bg-slate-100 transition-colors cursor-pointer"
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
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Nazwa Budowy *
            </label>

            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="np. Osiedle Słoneczne - Etap I"
              className="w-full px-3.5 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Adres
            </label>

            <input
              type="text"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              placeholder="np. ul. Prosta 12, Warszawa"
              className="w-full px-3.5 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">
                Data Rozpoczęcia
              </label>

              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-3.5 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-700 mb-1">
                Data Zakończenia
              </label>

              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full px-3.5 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-1">
              Status Budowy *
            </label>

            <select
              value={status}
              onChange={(e) =>
                setStatus(Number(e.target.value) as SiteStatus)
              }
              className="w-full px-3.5 py-2 text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-amber-500/20 focus:border-amber-500"
            >
              {Object.entries(SiteStatusLabels).map(([key, label]) => (
                <option key={key} value={key}>
                  {label}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-semibold text-slate-700 mb-2">
              Kierownicy Budowy
            </label>

            {isLoadingUsers ? (
              <div className="text-sm text-slate-500">
                Pobieranie kierowników...
              </div>
            ) : users.length === 0 ? (
              <div className="text-sm text-slate-500">
                Brak dostępnych kierowników budowy.
              </div>
            ) : (
              <div className="border border-slate-200 rounded-xl overflow-hidden max-h-44 overflow-y-auto">
                {users.map(user => (
                  <label
                    key={user.id}
                    className="flex items-center gap-3 px-3.5 py-3 border-b last:border-b-0 border-slate-100 hover:bg-slate-50 cursor-pointer"
                  >
                    <input
                      type="checkbox"
                      checked={selectedUserIds.includes(user.id)}
                      onChange={() => toggleUser(user.id)}
                      className="w-4 h-4 accent-amber-500"
                    />

                    <UserIcon className="w-5 h-5 text-slate-400" />

                    <div className="min-w-0">
                      <div className="text-sm font-semibold text-slate-800">
                        {user.firstName} {user.lastName}
                      </div>

                      <div className="text-xs text-slate-500 truncate">
                        {user.email}
                      </div>
                    </div>
                  </label>
                ))}
              </div>
            )}

            {selectedUserIds.length > 0 && (
              <div className="mt-1.5 text-xs text-slate-500">
                Wybrano: {selectedUserIds.length}
              </div>
            )}
          </div>

          <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-4 py-2.5 rounded-xl border border-slate-200 hover:bg-slate-50 text-slate-700 text-sm font-semibold transition-colors cursor-pointer"
            >
              Anuluj
            </button>

            <button
              type="submit"
              disabled={isSubmitting}
              className="px-5 py-2.5 rounded-xl bg-amber-500 hover:bg-amber-600 font-bold text-slate-950 text-sm shadow-sm transition-all active:scale-95 disabled:opacity-50 cursor-pointer"
            >
              {isSubmitting ? 'Zapisywanie...' : 'Utwórz Budowę'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};