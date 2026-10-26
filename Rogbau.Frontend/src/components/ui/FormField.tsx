// src/components/ui/FormField.tsx

interface FormFieldProps {
  label: string;
  type?: string;
  value: string;
  onChange: (e: any) => void;
  placeholder?: string;
  required?: boolean;
  rightElement?: React.ReactNode; // np. link "Zapomniałeś hasła?"
}

export const FormField: React.FC<FormFieldProps> = ({
  label,
  type = "text",
  value,
  onChange,
  placeholder,
  required = false,
  rightElement,
}) => {
  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <label className="text-xs font-bold uppercase tracking-wider text-slate-700">
          {label}
        </label>
        {rightElement}
      </div>
      <input
        type={type}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        required={required}
        className="w-full rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-3 text-sm text-slate-900 transition-all placeholder:text-slate-400 focus:border-amber-500 focus:bg-white focus:outline-none focus:ring-2 focus:ring-amber-500/20"
      />
    </div>
  );
}