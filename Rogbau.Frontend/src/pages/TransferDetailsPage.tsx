import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import type { TransferDetailsDto } from '../types/response/TransferResponseDto';
import { CreatePzModal } from '../components/transfers/CreatePzModal';
import { EditPzModal } from '../components/transfers/EditPzModal';
import { EditMovementModal } from '../components/transfers/EditMovementModal';
import { useAuth } from '../context/AuthContext';
import { TransferPrintDocument } from '../components/transfers/TransferPrintDocument';
import { getTransferById, confirmTransfer } from '../api/transferService';
import {
  ArrowLeftIcon,
  ArrowRightIcon,
  BuildingOffice2Icon,
  CalendarDaysIcon,
  CircleStackIcon,
  DocumentTextIcon,
} from '@heroicons/react/24/outline';

export const TransferDetailsPage: React.FC = () => {
  const { user } = useAuth();
  const [isEditingDocument, setIsEditingDocument] = useState(false);
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [transfer, setTransfer] = useState<TransferDetailsDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isConfirming, setIsConfirming] = useState<boolean>(false);
  const [isReceiptOpen, setIsReceiptOpen] = useState(false);

  useEffect(() => {
    if (!id) return;

    const fetchDetails = async () => {
      try {
        setLoading(true);
        setError(null);

        const data = await getTransferById(id);
        setTransfer(data);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : 'Nie udało się pobrać szczegółów dokumentu.'
        );
      } finally {
        setLoading(false);
      }
    };

    fetchDetails();
  }, [id]);

  const handleConfirm = async () => {
    if (!id) return;

    try {
      setIsConfirming(true);
      setError(null);

      await confirmTransfer(id);

      const updatedTransfer = await getTransferById(id);
      setTransfer(updatedTransfer);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Nie udało się potwierdzić odbioru.'
      );
    } finally {
      setIsConfirming(false);
    }
  };

  const getTypeName = (type: number) => {
    switch (type) {
      case 1:
        return 'WZ';
      case 2:
        return 'PZ';
      case 3:
        return 'Przesunięcie';
      default:
        return 'Nieznany';
    }
  };

  const getStatusName = (status: number) => {
    switch (status) {
      case 1:
        return 'Szkic';
      case 2:
        return 'W transporcie';
      case 3:
        return 'Zakończony';
      case 4:
        return 'Anulowany';
      default:
        return 'Nieznany';
    }
  };

  const getStatusClass = (status: number) => {
    switch (status) {
      case 2:
        return 'bg-amber-50 text-amber-700 border-amber-200';
      case 3:
        return 'bg-emerald-50 text-emerald-700 border-emerald-200';
      case 4:
        return 'bg-red-50 text-red-700 border-red-200';
      default:
        return 'bg-slate-100 text-slate-600 border-slate-200';
    }
  };

  if (loading) {
    return (
      <div className="p-8 text-center text-slate-500">
        Ładowanie szczegółów dokumentu...
      </div>
    );
  }

  if (!transfer) {
    return (
      <div className="p-6 max-w-6xl mx-auto">
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm mb-4">
          {error || 'Nie znaleziono dokumentu.'}
        </div>

        <button
          type="button"
          onClick={() => navigate('/transfers')}
          className="inline-flex items-center gap-2 px-4 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl font-medium transition-colors cursor-pointer"
        >
          <ArrowLeftIcon className="w-4 h-4" />
          Powrót do listy
        </button>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-6xl mx-auto space-y-6">
      <TransferPrintDocument transfer={transfer} />
      {isEditingDocument && transfer.type === 2 && <EditPzModal transfer={transfer} onClose={() => setIsEditingDocument(false)} onSuccess={() => {
        setIsEditingDocument(false);
        getTransferById(transfer.id).then(setTransfer).catch(err => setError(err instanceof Error ? err.message : 'Odśwież szczegóły dokumentu.'));
      }} />}
      {isEditingDocument && transfer.type !== 2 && <EditMovementModal transfer={transfer} onClose={() => setIsEditingDocument(false)} onSuccess={() => {
        setIsEditingDocument(false);
        getTransferById(transfer.id).then(setTransfer).catch(err => setError(err instanceof Error ? err.message : 'Odśwież szczegóły dokumentu.'));
      }} />}
      {error && <p className="p-3 rounded-xl bg-red-50 text-red-700">{error}</p>}
      {transfer.type === 2 && transfer.canConfirm && <CreatePzModal
        isOpen={isReceiptOpen} receipt={transfer} onClose={() => setIsReceiptOpen(false)}
        onSuccess={() => {
          setIsReceiptOpen(false);
          setTransfer({ ...transfer, canConfirm: false });
          getTransferById(transfer.id).then(setTransfer).catch(err =>
            setError(err instanceof Error ? err.message : 'Przyjęto PZ, ale nie udało się odświeżyć dokumentu. Odśwież stronę.'));
        }} />}

      {/* Powrót + status + potwierdzenie */}
      <div className="flex items-center justify-between">
        <button
          type="button"
          onClick={() => navigate(-1)}
          className="inline-flex items-center gap-2 text-sm font-medium text-slate-600 hover:text-slate-900 transition-colors cursor-pointer"
        >
          <ArrowLeftIcon className="w-4 h-4" />
          Powrót
        </button>

        <div className="flex items-center gap-3">
          {user?.role === 'Admin' && (transfer.type === 1 || transfer.type === 2 || transfer.type === 3) && (transfer.status === 2 || transfer.status === 3) &&
            <button type="button" onClick={() => setIsEditingDocument(true)} className="rounded-xl border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 print:hidden">Edytuj dokument</button>}
          <span
            className={`inline-flex items-center px-3 py-1.5 rounded-lg text-sm font-semibold border ${getStatusClass(
              transfer.status
            )}`}
          >
            {getStatusName(transfer.status)}
          </span>

          {transfer.canConfirm && (
  <button
    type="button"
    onClick={() => transfer.type === 2 ? setIsReceiptOpen(true) : void handleConfirm()}
    disabled={isConfirming}
    className="px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 disabled:cursor-not-allowed text-white text-sm font-semibold transition-colors cursor-pointer"
  >
    {isConfirming ? 'Potwierdzanie...' : transfer.type === 2 ? 'Przyjmij PZ' : 'Potwierdź odbiór'}
  </button>

  
)}

<button
  type="button"
  onClick={() => window.print()}
  className="px-4 py-2 rounded-xl bg-amber-500 hover:bg-amber-600 text-white text-sm font-semibold transition-colors cursor-pointer print:hidden"
>
  Drukuj
</button>
        </div>
      </div>

      {/* Nagłówek */}
      <div className="flex items-start justify-between border-b border-slate-200 pb-5">
        <div className="flex items-center gap-3">
          <div className="p-2.5 bg-amber-50 rounded-xl border border-amber-200 text-amber-600">
            <DocumentTextIcon className="w-6 h-6" />
          </div>

          <div>
            <h1 className="text-2xl font-bold text-slate-900">
              {transfer.number}
            </h1>

            <p className="text-sm text-slate-500 mt-0.5">
              Typ dokumentu:{' '}
              <span className="font-medium text-slate-700">
                {getTypeName(transfer.type)}
              </span>
            </p>
          </div>
        </div>
      </div>

      {/* Informacje */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">

        <div className="p-4 bg-white rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center gap-2 text-slate-500 text-xs font-medium mb-1">
            <BuildingOffice2Icon className="w-4 h-4 text-slate-400" />
            Z
          </div>

          <p className="text-lg font-bold text-slate-900">
            {transfer.fromName || 'Magazyn'}
          </p>
        </div>

        <div className="p-4 bg-white rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center gap-2 text-slate-500 text-xs font-medium mb-1">
            <BuildingOffice2Icon className="w-4 h-4 text-slate-400" />
            Do
          </div>

          <p className="text-lg font-bold text-slate-900">
            {transfer.toName || 'Magazyn'}
          </p>
        </div>

        <div className="p-4 bg-white rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center gap-2 text-slate-500 text-xs font-medium mb-1">
            <CalendarDaysIcon className="w-4 h-4 text-slate-400" />
            Data utworzenia
          </div>

          <p className="text-lg font-bold text-slate-900">
            {new Date(transfer.date).toLocaleDateString('pl-PL')}
          </p>
        </div>

        <div className="p-4 bg-white rounded-2xl border border-slate-200 shadow-xs">
          <div className="flex items-center gap-2 text-slate-500 text-xs font-medium mb-1">
            <BuildingOffice2Icon className="w-4 h-4 text-slate-400" />
            Waga transportu
          </div>

          <p className="text-lg font-bold text-slate-900">
            {transfer.totalWeight || 0} kg
          </p>
        </div>
      </div>

      {/* Kierunek */}
      <div className="p-5 bg-amber-50/40 rounded-2xl border border-amber-200/80">
        <div className="flex items-center justify-center gap-4">
          <span className="font-bold text-slate-900">
            {transfer.fromName || 'Magazyn'}
          </span>

          <ArrowRightIcon className="w-6 h-6 text-amber-600" />

          <span className="font-bold text-slate-900">
            {transfer.toName || 'Magazyn'}
          </span>
        </div>
      </div>

      {transfer.note && <div className="p-5 bg-white rounded-xl border border-slate-200"><h2 className="font-semibold mb-2">Uwagi</h2><p className="text-sm text-slate-600 whitespace-pre-wrap break-words">{transfer.note}</p></div>}
      {/* Pozycje dokumentu */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-xs overflow-hidden">

        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <CircleStackIcon className="w-5 h-5 text-slate-500" />

            <h2 className="text-base font-bold text-slate-900">
              Sprzęt
            </h2>
          </div>

          <span className="text-xs font-semibold px-2.5 py-1 bg-slate-100 text-slate-700 rounded-full">
            Pozycje: {transfer.items.length}
          </span>
        </div>

        {transfer.items.length === 0 ? (
          <div className="p-8 text-center text-slate-400 text-sm">
            Dokument nie zawiera żadnych pozycji.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600">

              <thead className="bg-slate-50 text-slate-400 text-xs font-semibold uppercase border-b border-slate-100">
                <tr>
                  <th className="px-6 py-3">
                    Sprzęt
                  </th>

                  <th className="px-6 py-3">
                    Jednostka
                  </th>

                  <th className="px-6 py-3 text-right">
                    {transfer.type === 2 ? 'Zgłoszona ilość' : 'Ilość'}
                  </th>

                  {transfer.type === 2 && (
                    <th className="px-6 py-3 text-right">
                      Złom / uszkodzone
                    </th>
                  )}
                </tr>
              </thead>

              <tbody className="divide-y divide-slate-100">
                {transfer.items.map((item) => (
                  <tr
                    key={item.equipmentId}
                    className="hover:bg-slate-50/80 transition-colors"
                  >
                    <td className="px-6 py-4 font-semibold text-slate-900">
                      {item.equipmentName}
                    </td>

                    <td className="px-6 py-4 text-slate-500">
                      {item.unit}
                    </td>

                    <td className="px-6 py-4 text-right font-bold text-slate-900">
                      {item.quantity} {item.unit}
                    </td>

                    {transfer.type === 2 && (
                      <td className="px-6 py-4 text-right font-bold text-rose-600">
                        {item.scrappedQuantity} {item.unit}
                        {item.receivedQuantity != null && <p className="mt-2 text-emerald-700 text-xs">
                          Przyjęto: {item.receivedQuantity} {item.unit}, w tym złom: {item.receivedScrappedQuantity ?? 0}
                        </p>}
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>

            </table>
          </div>
        )}
      </div>
    </div>
  );
};

