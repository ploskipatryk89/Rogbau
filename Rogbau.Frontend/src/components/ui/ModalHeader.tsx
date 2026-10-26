import { XMarkIcon } from '@heroicons/react/24/outline';

interface Props {
  title: string;
  description: string;
  onClose: () => void;
  disabled?: boolean;
}

export function ModalHeader({ title, description, onClose, disabled }: Props) {
  return (
    <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
      <div>
        <h3 className="text-lg font-bold text-slate-900">{title}</h3>
        <p className="text-sm text-slate-500">{description}</p>
      </div>
      <button type="button" onClick={onClose} disabled={disabled} aria-label="Zamknij"
        className="p-1 text-slate-400 hover:text-slate-600 rounded-lg transition-colors cursor-pointer">
        <XMarkIcon className="w-5 h-5" />
      </button>
    </div>
  );
}
