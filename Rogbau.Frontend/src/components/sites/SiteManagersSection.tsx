import type { SiteDto, SiteDetailsDto } from '../../types/response/SiteResponse';
import { PlusCircleIcon, UserIcon, TrashIcon } from '@heroicons/react/24/outline';

interface Props {
  site: SiteDto;
  details?: SiteDetailsDto;
  isAdmin: boolean;
  isLoadingThisDetails: boolean;
  deletingUserId: string | null;
  setAddUserSite: (site: SiteDto) => void;
  handleDeleteUser: (siteId: string, userId: string) => Promise<void>;
}

export function SiteManagersSection({ site, details, isAdmin, isLoadingThisDetails,
  deletingUserId, setAddUserSite, handleDeleteUser }: Props) {
  return (
    <div className="max-w-3xl">
      <div className="flex items-center justify-between mb-2">
        <p className="text-xs font-bold text-slate-500 uppercase tracking-wider">
          Kierownicy
          budowy:
        </p>

        {isAdmin && details && (
          <button
            type="button"
            onClick={(
              e
            ) => {
              e.stopPropagation();

              setAddUserSite(
                site
              );
            }}
            className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-semibold text-amber-700 bg-amber-50 hover:bg-amber-100 border border-amber-200/80 rounded-md transition-colors cursor-pointer"
          >
            <PlusCircleIcon className="w-3.5 h-3.5 text-amber-600" />

            Przypisz
            kierownika
          </button>
        )}
      </div>

      {isLoadingThisDetails ? (
        <p className="text-xs text-slate-400 italic">
          Ładowanie
          kierowników...
        </p>
      ) : details
          ?.managers &&
        details.managers
          .length >
          0 ? (
        <div className="bg-white rounded-lg border border-slate-200 overflow-hidden">
          <div className="grid grid-cols-[1.5fr_2fr_1.5fr_auto] gap-4 px-4 py-2 bg-slate-50 border-b border-slate-200 text-[10px] font-bold text-slate-400 uppercase tracking-wider">
            <span>
              Kierownik
            </span>

            <span>
              Email
            </span>

            <span>
              Telefon
            </span>

            <span>
              Akcje
            </span>
          </div>

          <div className="divide-y divide-slate-100">
            {details.managers.map(
              (
                manager
              ) => (
                <div
                  key={
                    manager.id
                  }
                  className="grid grid-cols-[1.5fr_2fr_1.5fr_auto] items-center gap-4 px-4 py-2.5 text-xs"
                >
                  <span className="flex items-center gap-2 font-semibold text-slate-800">
                    <UserIcon className="w-4 h-4 text-slate-400 shrink-0" />

                    {
                      manager.fullName
                    }
                  </span>

                  <span className="text-slate-600">
                    {
                      manager.email
                    }
                  </span>

                  <span className="text-slate-600">
                    {manager.phoneNumber ||
                      '---'}
                  </span>

                  <button
                    type="button"
                    disabled={
                      deletingUserId !==
                      null
                    }
                    onClick={(
                      e
                    ) => {
                      e.stopPropagation();

                      handleDeleteUser(
                        site.id,
                        manager.id
                      );
                    }}
                    className="inline-flex items-center gap-1 px-2 py-1 text-xs font-semibold text-red-600 hover:text-red-700 hover:bg-red-50 rounded-md transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                    style={isAdmin ? undefined : { display: 'none' }}
                    title="Usuń kierownika z budowy"
                  >
                    <TrashIcon className="w-3.5 h-3.5" />

                    {deletingUserId ===
                    manager.id
                      ? 'Usuwanie...'
                      : 'Usuń'}
                  </button>
                </div>
              )
            )}
          </div>
        </div>
      ) : (
        <div className="flex items-center justify-between bg-white border border-dashed border-slate-300 rounded-lg px-4 py-3">
          <p className="text-xs text-slate-400 italic">
            Brak
            przypisanego
            kierownika
            budowy.
          </p>

          {isAdmin && details && (
            <button
              type="button"
              onClick={(
                e
              ) => {
                e.stopPropagation();

                setAddUserSite(
                  site
                );
              }}
              className="text-xs font-semibold text-amber-600 hover:text-amber-700 cursor-pointer"
            >
              + Przypisz
              kierownika
            </button>
          )}
        </div>
      )}
    </div>
  );
}
