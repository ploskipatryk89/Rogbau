interface Props {
  onCancel: () => void;
  submitLabel: string;
  submitDisabled: boolean;
  cancelDisabled?: boolean;
}

export function FormActions({ onCancel, submitLabel, submitDisabled, cancelDisabled }: Props) {
  return (
    <div className="flex items-center justify-end gap-3 pt-4 border-t border-slate-200">
      <button type="button" onClick={onCancel} disabled={cancelDisabled}
        className="px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100 rounded-xl transition-colors cursor-pointer">
        Anuluj
      </button>
      <button type="submit" disabled={submitDisabled}
        className="px-4 py-2 text-sm font-bold bg-amber-500 hover:bg-amber-600 text-slate-950 rounded-xl shadow-sm transition-all cursor-pointer disabled:opacity-50">
        {submitLabel}
      </button>
    </div>
  );
}
