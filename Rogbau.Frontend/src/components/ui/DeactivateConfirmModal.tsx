import React from 'react';
import { ExclamationTriangleIcon, XMarkIcon } from '@heroicons/react/24/outline';

interface DeactivateConfirmModalProps {
  isOpen: boolean;
  title: string;
  message: string;
  isDeactivating?: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export const DeactivateConfirmModal: React.FC<DeactivateConfirmModalProps> = ({
  isOpen,
  title,
  message,
  isDeactivating,
  onClose,
  onConfirm,
}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-xl border border-slate-200 w-full max-w-md overflow-hidden">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <div className="flex items-center gap-2 text-red-600 ">
            <ExclamationTriangleIcon className="w-5 h-5" />
            <h3 className="text-lg font-bold text-slate-900">{title}</h3>
          </div>
          <button
            onClick={onClose}
            className="p-1 text-slate-400 hover:text-slate-600 rounded-lg hover:bg-slate-100 transition-colors cursor-pointer"
          >
            <XMarkIcon className="w-5 h-5" />
          </button>
        </div>

        <div className="p-6">
          <p className="text-sm text-slate-600">{message}</p>
        </div>

        <div className="flex items-center justify-end gap-3 px-6 py-4 bg-slate-50 border-t border-slate-100">
          <button
            type="button"
            onClick={onClose}
            disabled={isDeactivating}
            className="px-4 py-2 rounded-xl border border-slate-200 hover:bg-white text-slate-700 text-sm font-semibold transition-colors cursor-pointer"
          >
            Anuluj
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isDeactivating}
            className="px-4 py-2 rounded-xl bg-red-600 hover:bg-red-700 text-white font-bold text-sm shadow-sm transition-all active:scale-95 disabled:opacity-50 cursor-pointer"
          >
            {isDeactivating ? 'Wycofywanie...' : 'Wycofaj'}
          </button>
        </div>
      </div>
    </div>
  );
};