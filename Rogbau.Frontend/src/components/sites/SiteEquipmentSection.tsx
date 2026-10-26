import type { SiteDto, SiteDetailsDto } from '../../types/response/SiteResponse';
import { PlusCircleIcon, PlayIcon, CheckCircleIcon } from '@heroicons/react/24/outline';
import { SiteStatus } from '../../types/enums/SiteStatus';

interface Props {
  site: SiteDto;
  details?: SiteDetailsDto;
  canChangeStatus: boolean;
  canFinishSite: boolean;
  currentStatus: SiteStatus;
  isLoadingThisDetails: boolean;
  setInitialStockSite: (site: SiteDto) => void;
  onOpenStatusModal?: (site: SiteDto, mode: 'start' | 'finish') => void;
  onReturnEquipment?: (equipmentId: string) => void;
}

export function SiteEquipmentSection({ site, details, canChangeStatus, canFinishSite, currentStatus,
  isLoadingThisDetails, setInitialStockSite, onOpenStatusModal, onReturnEquipment }: Props) {
  return (
    <div>
      <div className="flex items-center justify-between max-w-3xl">
        <p className="text-xs font-bold text-slate-500 uppercase tracking-wider">
          Sprzęt
          alokowany na
          budowie:
        </p>

        <div className="flex items-center gap-2">
          {canChangeStatus && site.status !==
            SiteStatus.Completed && (
            <button
              type="button"
              onClick={(
                e
              ) => {
                e.stopPropagation();

                setInitialStockSite(
                  site
                );
              }}
              className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-semibold text-amber-700 bg-amber-50 hover:bg-amber-100 border border-amber-200/80 rounded-md transition-colors cursor-pointer"
            >
              <PlusCircleIcon className="w-3.5 h-3.5 text-amber-600" />

              Ustalić
              bilans
              otwarcia
            </button>
          )}

          {canChangeStatus && currentStatus ===
            SiteStatus.Planned && (
            <button
              type="button"
              onClick={(
                e
              ) => {
                e.stopPropagation();

                onOpenStatusModal?.(
                  site,
                  'start'
                );
              }}
              className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-semibold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200/80 rounded-md transition-colors cursor-pointer"
            >
              <PlayIcon className="w-3.5 h-3.5" />

              Rozpocznij
              budowę
            </button>
          )}

          {canFinishSite && currentStatus ===
            SiteStatus.InProgress && (
            <button
              type="button"
              onClick={(
                e
              ) => {
                e.stopPropagation();

                onOpenStatusModal?.(
                  site,
                  'finish'
                );
              }}
              className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-semibold text-slate-700 bg-white hover:bg-slate-100 border border-slate-300 rounded-md transition-colors cursor-pointer"
            >
              <CheckCircleIcon className="w-3.5 h-3.5" />

              Zakończ
              budowę
            </button>
          )}
        </div>
      </div>

      {isLoadingThisDetails ? (
        <p className="text-xs text-slate-400 italic mt-2">
          Ładowanie
          szczegółów
          sprzętu...
        </p>
      ) : details
          ?.equipment &&
        details.equipment
          .length >
          0 ? (
        <div className="bg-white rounded-lg border border-slate-200 overflow-hidden max-w-3xl mt-2">
          <div className="grid grid-cols-[1.6fr_1fr_1fr] gap-4 px-4 py-2 bg-slate-50 border-b border-slate-200 text-[10px] font-bold text-slate-400 uppercase tracking-wider">
            <span>
              Sprzęt
            </span>

            <span>
              Ilość
            </span>

            <span>
              Wartość
            </span>
          </div>

          <div className="divide-y divide-slate-100">
            {details.equipment.map(
              (eq) => (
                <div
                  key={
                    eq.equipmentId
                  }
                  className="grid grid-cols-[1.6fr_1fr_1fr] items-center gap-4 px-4 py-2.5 text-xs"
                >
                  <span className="font-semibold text-slate-800">
                    {
                      eq.equipmentName
                    }
                  </span>

                  <span className="font-semibold text-slate-700">
                    {
                      eq.allocatedQuantity
                    }{' '}
                    {
                      eq.unit
                    }
                  </span>

                  <span className="font-semibold text-slate-700">
                    {
                      eq.purchaseTotalPrice
                    }{' '}
                    zł
                    {onReturnEquipment && eq.allocatedQuantity > 0 && <button type="button"
                      onClick={() => onReturnEquipment(eq.equipmentId)}
                      className="block mt-2 px-2 py-1 rounded border border-amber-200 bg-amber-50 text-amber-700 hover:bg-amber-100 cursor-pointer">
                      Zwróć sprzęt
                    </button>}
                  </span>
                </div>
              )
            )}
          </div>
        </div>
      ) : (
        <p className="text-xs text-slate-400 italic mt-2">
          Na tej budowie
          nie ma obecnie
          przypisanego
          sprzętu.
        </p>
      )}
    </div>
  );
}
