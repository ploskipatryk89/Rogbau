import type { ReactNode } from 'react';

interface Props {
  title: string;
  value: number;
  unit?: string;
  icon: ReactNode;
}

export function DashboardStatCard({ title, value, unit, icon }: Props) {
  return (
    <div className="bg-white rounded-xl border border-slate-200 p-5 flex items-center justify-between gap-4">
      <div>
        <p className="text-sm text-slate-500">{title}</p>
        <p className="mt-2 text-3xl font-bold text-slate-900">
          {value.toLocaleString('pl-PL')} <span className="text-xl">{unit}</span>
        </p>
      </div>
      <div className="bg-amber-50 text-amber-600 rounded-xl p-3 shrink-0">{icon}</div>
    </div>
  );
}
