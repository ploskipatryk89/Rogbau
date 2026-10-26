import React from 'react';

interface TransferTypeTabsProps {
  selectedType: number | null;
  onSelectType: (type: number | null) => void;
}

export const TransferTypeTabs: React.FC<TransferTypeTabsProps> = ({
  selectedType,
  onSelectType,
}) => {
  const tabs = [
    { label: 'Wszystkie', value: null },
    { label: 'WZ', value: 1 },
    { label: 'PZ', value: 2 },
    { label: 'Przesunięcia między budowami', value: 3 },
  ];

  return (
    <div className="border-b border-slate-200">
      <nav className="-mb-px flex space-x-6 overflow-x-auto">
        {tabs.map((tab) => (
          <button
            key={tab.label}
            type="button"
            onClick={() => onSelectType(tab.value)}
            className={`py-3 px-1 border-b-2 font-semibold text-sm whitespace-nowrap transition-colors cursor-pointer ${
              selectedType === tab.value
                ? 'border-amber-500 text-amber-600'
                : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </nav>
    </div>
  );
};