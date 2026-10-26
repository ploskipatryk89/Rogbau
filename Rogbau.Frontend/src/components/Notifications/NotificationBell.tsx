import React from 'react';
import { BellIcon } from '@heroicons/react/24/outline';

interface NotificationBellProps {
  hasUnread: boolean;
  onClick: () => void;
}

export const NotificationBell: React.FC<NotificationBellProps> = ({
  hasUnread,
  onClick
}) => {
  return (
    <button
      onClick={onClick}
      className="p-2 text-slate-500 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors relative shrink-0 cursor-pointer"
    >
      <BellIcon className="w-5 h-5" />

      {hasUnread && (
        <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-amber-500 rounded-full" />
      )}
    </button>
  );
};