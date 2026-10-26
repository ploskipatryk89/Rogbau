import type { HistoricalDataDto } from "../../types/response/HistoricalData";

const dailyRateFormat = new Intl.NumberFormat("pl-PL", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 4,
});

const amountFormat = new Intl.NumberFormat("pl-PL", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});

interface Props {
    data: HistoricalDataDto[];
}

export function HistoricalDataTable({ data }: Props) {
    return (
        <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
            <table className="w-full min-w-[900px]">
                <thead className="bg-slate-50">
                    <tr>
                        <th className="px-4 py-3 text-left text-sm font-semibold text-slate-600">
                            Sprzęt
                        </th>

                        <th className="px-4 py-3 text-left text-sm font-semibold text-slate-600">
                            Budowa
                        </th>

                        <th className="px-4 py-3 text-center text-sm font-semibold text-slate-600">
                            Okres
                        </th>

                        <th className="px-4 py-3 text-center text-sm font-semibold text-slate-600">
                            Ilość
                        </th>

                        <th className="px-4 py-3 text-center text-sm font-semibold text-slate-600">
                            Dni
                        </th>

                        <th className="px-4 py-3 text-right text-sm font-semibold text-slate-600">
                            Stawka / dzień
                        </th>

                        <th className="px-4 py-3 text-right text-sm font-semibold text-slate-600">
                            Naliczona kwota
                        </th>
                    </tr>
                </thead>

                <tbody className="divide-y divide-slate-100">
                    {data.map((item) => (
                        <tr key={item.id}>
                            <td className="px-4 py-3 font-medium text-slate-900">
                                {item.equipmentName}
                            </td>

                            <td className="px-4 py-3 text-slate-700">
                                {item.siteName}
                            </td>

                            <td className="px-4 py-3 text-center text-slate-600">
                                {new Date(item.fromDate).toLocaleDateString("pl-PL")}
                                {" - "}
                                {item.toDate
                                    ? new Date(item.toDate).toLocaleDateString("pl-PL")
                                    : "obecnie"}
                            </td>

                            <td className="px-4 py-3 text-center">
                                {item.quantity}
                            </td>

                            <td className="px-4 py-3 text-center">
                                {item.days}
                            </td>

                            <td className="px-4 py-3 text-right">
                                {dailyRateFormat.format(item.dayPrice)} zł
                            </td>

                            <td className="px-4 py-3 text-right font-semibold">
                                {amountFormat.format(item.totalEarned)} zł
                            </td>
                        </tr>
                    ))}

                    {data.length === 0 && (
                        <tr>
                            <td
                                colSpan={7}
                                className="px-4 py-10 text-center text-slate-500"
                            >
                                Brak danych historycznych.
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    );
}
