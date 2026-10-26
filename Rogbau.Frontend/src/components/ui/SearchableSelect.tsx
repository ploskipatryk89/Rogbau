import Select from 'react-select';

interface Props {
  options: { value: string; label: string }[];
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  placeholder?: string;
  inputId?: string;
}

export function SearchableSelect({ options, value, onChange, disabled, placeholder = 'Wyszukaj...', inputId }: Props) {
  return <Select inputId={inputId} options={options} value={options.find(option => option.value === value) ?? null}
    onChange={option => onChange(option?.value ?? '')} isDisabled={disabled} isSearchable isClearable
    placeholder={placeholder} noOptionsMessage={() => 'Brak wyników'}
    menuPortalTarget={document.body} menuPosition="fixed"
    styles={{ menuPortal: base => ({ ...base, zIndex: 100 }), control: base => ({ ...base, borderRadius: 12, borderColor: '#e2e8f0' }) }}
    className="flex-1 min-w-0 text-sm" />;
}
