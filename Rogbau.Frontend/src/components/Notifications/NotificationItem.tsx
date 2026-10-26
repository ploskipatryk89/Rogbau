import { TrashIcon } from '@heroicons/react/24/outline';
import type { NotificationResponse } from '../../types/response/NotificationResponse';

interface Props {
  notification: NotificationResponse;
  onClick: () => void;
  onDelete: () => void;
  disabled: boolean;
}

export function NotificationItem({ notification, onClick, onDelete, disabled }: Props) {
  return (
    <div className={`flex items-start gap-2 p-4 text-sm transition-colors ${notification.isRead ? 'bg-white' : 'bg-amber-50/60'}`}>
      <button type="button" onClick={onClick} disabled={disabled}
        className="flex-1 min-w-0 text-left cursor-pointer disabled:opacity-60">
        <p className={notification.isRead ? 'text-slate-500 font-normal' : 'text-slate-950 font-bold'}>
          {notification.message}
        </p>
        <span className="text-xs text-slate-500">
          {new Date(notification.dateOfNotification).toLocaleString('pl-PL', {
            day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit'
          })}
        </span>
      </button>
      <button type="button" onClick={onDelete} disabled={disabled} title="Usuń powiadomienie"
        aria-label="Usuń powiadomienie"
        className="shrink-0 p-1.5 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg cursor-pointer disabled:opacity-50">
        <TrashIcon className="w-4 h-4" />
      </button>
    </div>
  );
}
