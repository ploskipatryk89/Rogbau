export type PageNavigationProps = {
  pageNumber: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
  isLoading?: boolean;
};

export function PageNavigation({
  pageNumber,
  totalPages,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
  isLoading = false,
}: PageNavigationProps) {
  return (
    <nav
      aria-label="Nawigacja między stronami wyników"
      className="flex items-center gap-3"
    >
      <button
        type="button"
        onClick={() => onPageChange(pageNumber - 1)}
        disabled={!hasPreviousPage || isLoading}
        className="rounded border px-3 py-1 disabled:opacity-50"
      >
        Poprzednia
      </button>

      <span>
        {totalPages === 0
          ? "Brak wyników"
          : `Strona ${pageNumber} z ${totalPages}`}
      </span>

      <button
        type="button"
        onClick={() => onPageChange(pageNumber + 1)}
        disabled={!hasNextPage || isLoading}
        className="rounded border px-3 py-1 disabled:opacity-50"
      >
        Następna
      </button>
    </nav>
  );
}