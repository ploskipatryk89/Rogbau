import React, { useEffect, useState } from 'react';
import { useParams, useNavigate} from 'react-router-dom';
import type { EquipmentDetailsDto } from '../types/response/EquipmentDetailsDto';
import { deleteEquipment, getEquipmentById } from '../api/equipmentService';
import { DeleteEquipmentModal } from '../components/equipment/DeleteEquipmentModal';
import { useAuth } from '../context/AuthContext';
import { 
  WrenchScrewdriverIcon, 
  ScaleIcon, 
  BanknotesIcon, 
  CircleStackIcon, 
  BuildingOffice2Icon,
  ArrowLeftIcon,
  MapPinIcon
} from '@heroicons/react/24/outline';

export const EquipmentDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';

  const [equipment, setEquipment] = useState<EquipmentDetailsDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
    const [deleteTarget, setDeleteTarget] = useState<{ id: string, name: string} | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  

  useEffect(() => {
    if (!id) return;

    const fetchDetails = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await getEquipmentById(id);
        setEquipment(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Nie udało się pobrać szczegółów sprzętu.');
      } finally {
        setLoading(false);
      }
    };

    fetchDetails();
  }, [id]);

   const handleOpenDeleteModal = (id: string, name: string) => {
  if (!isAdmin) return;
  setDeleteTarget({ id, name});
};

const handleConfirmDelete = async () => {
  if (!isAdmin || !deleteTarget) return;

  try {
    setIsDeleting(true);

    await deleteEquipment(deleteTarget.id);

    setDeleteTarget(null);
    navigate('/equipment');
  } catch (err) {
    setError(
      err instanceof Error
        ? err.message
        : 'Nie udało się usunąć sprzętu.'
    );
  } finally {
    setIsDeleting(false);
  }
};


  if (loading) {
    return (
      <div className="p-8 text-center text-slate-500">
        Ładowanie szczegółów sprzętu...
      </div>
    );
  }

  if (error || !equipment) {
    return (
      <div className="p-6 max-w-4xl mx-auto">
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm mb-4">
          {error || 'Nie znaleziono sprzętu.'}
        </div>
        <button
          type="button"
          onClick={() => navigate('/equipments')}
          className="inline-flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl font-medium transition-colors cursor-pointer"
        >
          <ArrowLeftIcon className="w-4 h-4" /> Powrót do listy
        </button>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-6xl mx-auto space-y-6">
      {/* Nagłówek i przycisk powrotu */}
      <div className="flex items-center justify-between">
        <button
          type="button"
          onClick={() => navigate(-1)}
          className="inline-flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-slate-900 transition-colors cursor-pointer"
        >
          <ArrowLeftIcon className="w-4 h-4" /> Powrót
        </button>
<div className="flex items-center gap-3">
  <span
    className={`inline-flex items-center px-3 py-1.5 rounded-lg text-sm font-semibold border ${
      equipment.isActive
        ? 'bg-emerald-50 text-emerald-700 border-emerald-200'
        : 'bg-slate-100 text-slate-500 border-slate-200'
    }`}
  >
    {equipment.isActive ? 'Aktywny w katalogu' : 'Nieaktywny'}
  </span>

  {isAdmin && <button
  type="button"
  className="px-3 py-1.5 rounded-lg border border-red-300 text-red-600
             hover:bg-red-50 text-sm font-semibold transition-colors cursor-pointer"
  onClick={() => handleOpenDeleteModal(equipment.id, equipment.name)}
>
  Usuń trwale
</button>}
</div>
  
      </div>

      <div className="flex items-start justify-between border-b border-slate-200 pb-5">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-amber-50 rounded-xl border border-amber-200 text-amber-600">
            <WrenchScrewdriverIcon className="w-6 h-6" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{equipment.name}</h1>
            <p className="text-sm text-slate-500 mt-0.5">
              Kategoria: <span className="font-medium text-slate-700">{equipment.categoryName}</span>
              {equipment.subCategoryName && (
                <> &rsaquo; <span className="font-medium text-slate-700">{equipment.subCategoryName}</span></>
              )}
            </p>
          </div>
        </div>
      </div>

      {/* Karty KPI */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="p-4 bg-white rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center gap-2 text-slate-500 text-xs font-medium mb-1">
            <ScaleIcon className="w-4 h-4 text-slate-400" />
            Waga jednostkowa
          </div>
          <p className="text-xl font-bold text-slate-900">{equipment.weightKg} kg</p>
        </div>

        <div className="p-4 bg-white rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center gap-2 text-slate-500 text-xs font-medium mb-1">
            <BanknotesIcon className="w-4 h-4 text-slate-400" />
            Stawka dzienna
          </div>
          <p className="text-xl font-bold text-slate-900">{equipment.dayPrice.toLocaleString("pl-Pl" , {
            minimumFractionDigits: 2,
            maximumFractionDigits: 3
          })} PLN / dzień</p>
        </div>

        

        <div className="p-4 bg-white rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center gap-2 text-slate-500 text-xs font-medium mb-1">
            <CircleStackIcon className="w-4 h-4 text-slate-400" />
            Jednostka miary
          </div>
          <p className="text-xl font-bold text-slate-900">{equipment.unit}</p>
        </div>
      </div>

      {/* Stany magazynowe */}
      <div className="p-5 bg-amber-50/40 rounded-2xl border border-amber-200/80 space-y-4">
        <div className="flex items-center gap-2 text-amber-950 font-bold text-base">
          <CircleStackIcon className="w-5 h-5 text-amber-600" />
          Stany ilościowe ({equipment.unit})
        </div>
        
        <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
          <div className="bg-white p-3 rounded-xl border border-amber-200/70 shadow-2xs">
            <span className="text-xs text-slate-500 block">Dokupiona</span>
            <span className="text-lg font-bold text-slate-800">{equipment.purchasedQuantity}</span>
          </div>
          <div className="bg-white p-3 rounded-xl border border-amber-200/70 shadow-2xs">
            <span className="text-xs text-slate-500 block">Dostępne w bazie</span>
            <span className="text-lg font-bold text-emerald-600">{equipment.quantityAvailable}</span>
          </div>
          <div className="bg-white p-3 rounded-xl border border-amber-200/70 shadow-2xs">
            <span className="text-xs text-slate-500 block">Na budowach</span>
            <span className="text-lg font-bold text-blue-600">{equipment.quantityOnAllSites}</span>
          </div>
          <div className="bg-white p-3 rounded-xl border border-amber-200/70 shadow-2xs">
            <span className="text-xs text-slate-500 block">Złom/Uszkodzone</span>
            <span className="text-lg font-bold text-rose-600">{equipment.quantityScrapped}</span>
          </div>
           <div className="bg-white p-3 rounded-xl border border-amber-200/70 shadow-2xs">
            <span className="text-xs text-slate-500 block">W transporcie</span>
            <span className="text-lg font-bold text-cyan-400">{equipment.quantityInTransit}</span>
          </div>
        </div>
      </div>

      <div>

      </div>

      {/* Tabela Alokacji na Budowach */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-xs overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <BuildingOffice2Icon className="w-5 h-5 text-slate-500" />
            <h2 className="text-base font-bold text-slate-900">Alokacja na budowach</h2>
          </div>
          <span className="text-xs font-semibold px-2.5 py-1 bg-slate-100 text-slate-700 rounded-full">
            Suma: {equipment.quantityOnAllSites} {equipment.unit}
          </span>
        </div>

        {(!equipment.siteAllocations || equipment.siteAllocations.length === 0) ? (
          <div className="p-8 text-center text-slate-400 text-sm">
            Ten sprzęt nie znajduje się obecnie na żadnej budowie.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">
              <thead className="bg-slate-50 text-slate-400 text-xs font-semibold uppercase border-b border-slate-100">
                <tr>
                  <th className="px-6 py-3">Nazwa budowy</th>
                  <th className="px-6 py-3">Adres</th>
                  <th className="px-6 py-3 text-right">Przypisana ilość</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {equipment.siteAllocations.map((allocation) => (
                  <tr key={allocation.siteId} className="hover:bg-slate-50/80 transition-colors">
                    <td className="px-6 py-4 font-semibold text-slate-900">
                      
                        <span>{allocation.siteName}</span>
                      
                    </td>
                    <td className="px-6 py-4 text-slate-500">
                      {allocation.address ? (
                        <div className="flex items-center gap-1.5">
                          <MapPinIcon className="w-4 h-4 text-slate-400 shrink-0" />
                          <span>{allocation.address}</span>
                        </div>
                      ) : (
                        <span className="text-slate-400 italic">Brak adresu</span>
                      )}
                    </td>
                    <td className="px-6 py-4 text-right font-bold text-slate-900">
                      {allocation.quantityOnSite} {equipment.unit}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <DeleteEquipmentModal
        isOpen={isAdmin && !!deleteTarget}
        equipmentName={deleteTarget?.name ?? ''}
        isDeleting={isDeleting}
        onClose={() => setDeleteTarget(null)}
        onConfirm={handleConfirmDelete}
      />
    </div>

    
  );
};

