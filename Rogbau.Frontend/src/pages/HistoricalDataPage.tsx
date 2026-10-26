import { ErrorMessage } from "../components/ui/ErrorMessage";
import { useEffect, useState } from "react";
import { HistoricalDataPageHeader } from "../components/ui/HistoricalDataPageHeader";
import { HistoricalDataFilters } from "../components/historicalData/HistoricalDataFilters";
import { HistoricalDataTable } from "../components/historicalData/HistoricalDataTable";
import { PaginationControls } from "../components/Pagination/PaginationControls";
import { getHistoricalData } from "../api/historicalDataService";
import type {
    HistoricalDataDto,
    HistoricalDataFilters as Filters
} from "../types/response/HistoricalData";

export function HistoricalDataPage() {
    const [data, setData] = useState<HistoricalDataDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [filters, setFilters] = useState<Filters>({});
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize, setPageSize] = useState(10);
    const [pagination, setPagination] = useState({
        totalCount: 0,
        totalPages: 0,
        hasPreviousPage: false,
        hasNextPage: false,
    });

    useEffect(() => {
        let active = true;
        async function loadData() {
            setLoading(true);
            setError(null);
            try {
                const result = await getHistoricalData({ ...filters, pageNumber, pageSize });
                if (active) {
                    const lastPage = Math.max(1, result.totalPages);
                    if (pageNumber > lastPage) {
                        setPageNumber(lastPage);
                        return;
                    }

                    setData(result.items);
                    setPagination({
                        totalCount: result.totalCount,
                        totalPages: result.totalPages,
                        hasPreviousPage: result.hasPreviousPage,
                        hasNextPage: result.hasNextPage,
                    });
                }
            } catch (err) {
                if (active) {
                    setData([]);
                    setError(err instanceof Error ? err.message : 'Nie udało się pobrać danych historycznych.');
                }
            } finally {
                if (active) setLoading(false);
            }
        }

        loadData();
        return () => { active = false; };
    }, [filters, pageNumber, pageSize]);

    const handleFilterChange = (newFilters: Filters) => {
        setFilters(newFilters);
        setPageNumber(1);
    };

    const handlePageSizeChange = (size: number) => {
        setPageSize(size);
        setPageNumber(1);
    };

    const totalEarned = data.reduce(
    (sum, item) => sum + item.totalEarned,
    0
);
    return (
        <div className="space-y-6">
            <HistoricalDataPageHeader />

            <HistoricalDataFilters
                filters={filters}
                onChange={handleFilterChange}
            />

            {error && <ErrorMessage message={error} className="p-4" />}

            <div className="rounded-xl border border-slate-200 bg-white p-5">
    <p className="text-sm font-medium text-slate-500">
        Łącznie naliczono na tej stronie
    </p>

    <p className="mt-1 text-2xl font-bold text-slate-900">
        {loading ? 'Ładowanie...' : `${totalEarned.toFixed(2)} zł`}
    </p>
</div>

            <p className="text-sm text-slate-500">Naliczamy zakończone dni — w dniu dostawy kwota wynosi 0 zł. Dla stanu początkowego naliczanie rozpoczyna się od daty jego wprowadzenia. Wcześniejsze rozliczenia z Excela nie są uwzględnione.</p>

            <PaginationControls
                pageNumber={pageNumber}
                pageSize={pageSize}
                totalCount={pagination.totalCount}
                totalPages={pagination.totalPages}
                hasPreviousPage={pagination.hasPreviousPage}
                hasNextPage={pagination.hasNextPage}
                onPageChange={setPageNumber}
                onPageSizeChange={handlePageSizeChange}
                isLoading={loading}
            />
            <HistoricalDataTable data={data} />
            <PaginationControls
                pageNumber={pageNumber}
                pageSize={pageSize}
                totalCount={pagination.totalCount}
                totalPages={pagination.totalPages}
                hasPreviousPage={pagination.hasPreviousPage}
                hasNextPage={pagination.hasNextPage}
                onPageChange={setPageNumber}
                onPageSizeChange={handlePageSizeChange}
                isLoading={loading}
            />
        </div>
    );
}
