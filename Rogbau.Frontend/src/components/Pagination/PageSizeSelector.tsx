export type PageSizeSelectorProps = {
  pageSize: number;
  totalCount: number;
  onPageSizeChange: (size: number) => void;
  isLoading?: boolean;
};

export function PageSizeSelector({
  pageSize,
  totalCount,
  onPageSizeChange,
  isLoading = false,
}: PageSizeSelectorProps) {
  return (
    <div className="flex flex-wrap items-center gap-4">
      <label className="flex items-center gap-2">
        Pozycji na stronie:
        <select
          value={pageSize}
          onChange={(event) =>
            onPageSizeChange(Number(event.target.value))
          }
          disabled={isLoading}
          className="rounded border px-2 py-1"
        >
          <option value={10}>10</option>
          <option value={20}>20</option>
          <option value={50}>50</option>
        </select>
      </label>

      <span>Łącznie: {totalCount}</span>
    </div>
  );
}