import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import type { NotificationResponse } from '../../types/response/NotificationResponse';
import { NotificationItem } from './NotificationItem';
import { ErrorMessage } from '../ui/ErrorMessage';

interface NotificationListProps {
  notifications: NotificationResponse[];
  onClose: () => void;
  onRead: (notification: NotificationResponse) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
}

export const NotificationList: React.FC<NotificationListProps> = ({
  notifications,
  onClose,
  onRead,
  onDelete
}) => {
  const navigate = useNavigate();
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleNotificationClick = async (notification: NotificationResponse) => {
    if (busy) return;
    setBusy(true);
    setError(null);
    try {
      await onRead(notification);
      onClose();
      if (notification.actionUrl) navigate(notification.actionUrl);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Nie udało się otworzyć powiadomienia.');
    } finally {
      setBusy(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (busy) return;
    setBusy(true);
    setError(null);
    try { await onDelete(id); }
    catch (err) { setError(err instanceof Error ? err.message : 'Nie udało się usunąć powiadomienia.'); }
    finally { setBusy(false); }
  };

  const unreadCount = notifications.filter(
    notification => !notification.isRead
  ).length;

  return (
    <div className="absolute right-0 top-full mt-3 w-80 sm:w-96 bg-white border border-slate-200 rounded-2xl shadow-xl z-50 overflow-hidden">

      {/* Nagłówek */}
      <div className="px-4 py-3 border-b border-slate-100 flex items-center justify-between bg-slate-50">
        <span className="font-semibold text-slate-800 text-sm">
          Powiadomienia
        </span>

        <span className="text-xs text-slate-500">
          {unreadCount} nieprzeczytanych
        </span>
      </div>

      {/* Lista */}
      <ErrorMessage message={error} className="p-3" />
      <div className="max-h-80 overflow-y-auto divide-y divide-slate-100">

        {notifications.length === 0 ? (
          <div className="p-6 text-center text-sm text-slate-500">
            Brak powiadomień
          </div>
        ) : (
          notifications.map(notification => (
            <NotificationItem
              key={notification.id}
              notification={notification}
              disabled={busy}
              onDelete={() => { void handleDelete(notification.id); }}
              onClick={() =>
                void handleNotificationClick(notification)
              }
            />
          ))
        )}

      </div>
    </div>
  );
};
