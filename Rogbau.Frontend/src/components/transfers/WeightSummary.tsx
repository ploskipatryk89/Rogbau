interface Props {
  totalWeightKg: number;
}

export function WeightSummary({ totalWeightKg }: Props) {
  return (
    <div className="flex justify-end px-4 py-3 bg-slate-50 border-t border-slate-200">
      <div className="text-sm">
        <span className="text-slate-500">Łączna waga: </span>
        <span className="font-bold text-slate-900">
          {totalWeightKg >= 1000
            ? `${(totalWeightKg / 1000).toFixed(2)} t`
            : `${totalWeightKg.toFixed(2)} kg`}
        </span>
      </div>
    </div>
  );
}
