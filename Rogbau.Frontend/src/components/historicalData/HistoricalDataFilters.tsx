import { useEffect, useState } from "react";
import type { HistoricalDataFilters as Filters } from "../../types/response/HistoricalData";
import type { SiteDto } from "../../types/response/SiteResponse";
import type { EquipmentResponse } from "../../types/response/EquipmentResponse";
import { getSites } from "../../api/siteService";
import { getAllEquipments } from "../../api/equipmentService";

interface Props {
    filters: Filters;
    onChange: (filters: Filters) => void;
}

export function HistoricalDataFilters({
    filters,
    onChange
}: Props) {
    const [sites, setSites] = useState<SiteDto[]>([]);
    const [equipments, setEquipments] = useState<EquipmentResponse[]>([]);

    useEffect(() => {
        async function loadFiltersData() {
            const [sitesData, equipmentsData] = await Promise.all([
                getSites(),
                getAllEquipments()
            ]);

            setSites(sitesData);
            setEquipments(equipmentsData);
        }

        loadFiltersData();
    }, []);

    return (
        <div className="rounded-xl border border-slate-200 bg-white p-4">
            <div className="grid grid-cols-1 gap-4 md:grid-cols-4">

                {/* SPRZĘT */}
                <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">
                        Sprzęt
                    </label>

                    <select
                        value={filters.equipmentId ?? ""}
                        onChange={(e) =>
                            onChange({
                                ...filters,
                                equipmentId: e.target.value || undefined
                            })
                        }
                        className="w-full rounded-lg border border-slate-300 px-3 py-2"
                    >
                        <option value="">Wszystkie</option>

                        {equipments.map((equipment) => (
                            <option
                                key={equipment.id}
                                value={equipment.id}
                            >
                                {equipment.name}
                            </option>
                        ))}
                    </select>
                </div>

                {/* BUDOWA */}
                <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">
                        Budowa
                    </label>

                    <select
                        value={filters.siteId ?? ""}
                        onChange={(e) =>
                            onChange({
                                ...filters,
                                siteId: e.target.value || undefined
                            })
                        }
                        className="w-full rounded-lg border border-slate-300 px-3 py-2"
                    >
                        <option value="">Wszystkie</option>

                        {sites.map((site) => (
                            <option
                                key={site.id}
                                value={site.id}
                            >
                                {site.name}
                            </option>
                        ))}
                    </select>
                </div>

                {/* DATA OD */}
                <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">
                        Data od
                    </label>

                    <input
                        type="date"
                        value={filters.fromDate ?? ""}
                        onChange={(e) =>
                            onChange({
                                ...filters,
                                fromDate: e.target.value || undefined
                            })
                        }
                        className="w-full rounded-lg border border-slate-300 px-3 py-2"
                    />
                </div>

                {/* DATA DO */}
                <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">
                        Data do
                    </label>

                    <input
                        type="date"
                        value={filters.toDate ?? ""}
                        onChange={(e) =>
                            onChange({
                                ...filters,
                                toDate: e.target.value || undefined
                            })
                        }
                        className="w-full rounded-lg border border-slate-300 px-3 py-2"
                    />
                </div>

            </div>
        </div>
    );
}