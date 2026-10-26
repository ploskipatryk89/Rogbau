import { SiteManagersSection } from './SiteManagersSection';
import { SiteEquipmentSection } from './SiteEquipmentSection';
import { useAuth } from "../../context/AuthContext";
import React, { useState } from 'react';
import type {
  SiteDto,
  SiteDetailsDto,
} from '../../types/response/SiteResponse';

import {
  SiteStatus,
  SiteStatusLabels,
  SiteStatusColors,
} from '../../types/enums/SiteStatus';

import {
  deleteUserFromSite,
  getSiteById,
} from '../../api/siteService';

import {
  BuildingOfficeIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  MapPinIcon,
  CalendarIcon,
} from '@heroicons/react/24/outline';

import { IsEmpty } from '../ui/IsEmpty';
import { AddUserToSiteModal } from './AddUserToSiteModal';
import { SetSiteInitialStockModal } from './SetSiteInitialStockModal';
import { CreatePzModal } from '../transfers/CreatePzModal';

interface SiteTableProps {
  items: SiteDto[];
  isLoading: boolean;
  onStockChanged?: () => Promise<void>;

  onOpenStatusModal?: (
    site: SiteDto,
    mode: 'start' | 'finish'
  ) => void;
}

export const SiteTable: React.FC<SiteTableProps> = ({
  items = [],
  isLoading,
  onStockChanged,
  onOpenStatusModal,
}) => {
  const { user } = useAuth();
  const isAdmin = user?.role === "Admin";
  const [returnEquipment, setReturnEquipment] = useState<{ siteId: string; equipmentId: string } | null>(null);
  const [expandedIds, setExpandedIds] = useState<
    Record<string, boolean>
  >({});

  const [siteDetails, setSiteDetails] = useState<
    Record<string, SiteDetailsDto>
  >({});

  const [loadingDetails, setLoadingDetails] = useState<
    Record<string, boolean>
  >({});

  const [deletingUserId, setDeletingUserId] =
    useState<string | null>(null);

  const [deleteError, setDeleteError] =
    useState<string | null>(null);

  const [addUserSite, setAddUserSite] =
    useState<SiteDto | null>(null);

  const [initialStockSite, setInitialStockSite] =
    useState<SiteDto | null>(null);

  const loadSiteDetails = async (
    siteId: string
  ): Promise<SiteDetailsDto | null> => {
    try {
      setLoadingDetails((prev) => ({
        ...prev,
        [siteId]: true,
      }));

      const details =
        await getSiteById(siteId);

      setSiteDetails((prev) => ({
        ...prev,
        [siteId]: details,
      }));

      return details;
    } catch (err) {
      console.error(
        'Błąd pobierania szczegółów budowy:',
        err
      );

      return null;
    } finally {
      setLoadingDetails((prev) => ({
        ...prev,
        [siteId]: false,
      }));
    }
  };

  const toggleRow = async (
    siteId: string
  ) => {
    if (!siteId) {
      return;
    }

    const isCurrentlyExpanded =
      !!expandedIds[siteId];

    setExpandedIds((prev) => ({
      ...prev,
      [siteId]: !isCurrentlyExpanded,
    }));

    if (isCurrentlyExpanded) {
      return;
    }

    if (siteDetails[siteId]) {
      return;
    }

    await loadSiteDetails(siteId);
  };

  const handleDeleteUser = async (
    siteId: string,
    userId: string
  ) => {
    if (deletingUserId !== null) {
      return;
    }

    try {
      setDeletingUserId(userId);
      setDeleteError(null);

      await deleteUserFromSite(
        siteId,
        userId
      );

      setSiteDetails((prev) => {
        const details = prev[siteId];

        if (!details) {
          return prev;
        }

        return {
          ...prev,
          [siteId]: {
            ...details,
            managers:
              details.managers.filter(
                (manager) =>
                  manager.id !== userId
              ),
          },
        };
      });
    } catch (err) {
      setDeleteError(
        err instanceof Error
          ? err.message
          : 'Nie udało się usunąć kierownika z budowy.'
      );
    } finally {
      setDeletingUserId(null);
    }
  };

  const handleAddUserSuccess = async () => {
    if (!addUserSite) {
      return;
    }

    const siteId = addUserSite.id;

    await loadSiteDetails(siteId);

    setAddUserSite(null);
  };

  const handleInitialStockSuccess =
    async () => {
      if (!initialStockSite) {
        return;
      }

      const siteId =
        initialStockSite.id;

      await loadSiteDetails(siteId);

      setInitialStockSite(null);
      await onStockChanged?.();
    };

  if (isLoading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 p-12 text-center text-slate-500">
        Ładowanie listy budów...
      </div>
    );
  }

  if (
    !items ||
    !Array.isArray(items) ||
    items.length === 0
  ) {
    return (
      <IsEmpty
        title="Brak zarejestrowanych budów"
        subTitle={isAdmin ? 'Kliknij "Dodaj budowę", aby zarejestrować nowy projekt.' : undefined}
        icon={BuildingOfficeIcon}
      />
    );
  }

  return (
    <>
      <div className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">
        {deleteError && (
          <div className="m-4 p-3 bg-red-50 border border-red-200 rounded-xl text-sm text-red-700">
            {deleteError}
          </div>
        )}

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                <th className="py-3.5 px-4 w-10"></th>

                <th className="py-3.5 px-4">
                  Nazwa Budowy
                </th>

                <th className="py-3.5 px-4">
                  Adres
                </th>

                <th className="py-3.5 px-4 text-center">
                  Okres Realizacji
                </th>

                <th className="py-3.5 px-4 text-center">
                  Status
                </th>
              </tr>
            </thead>

            <tbody className="divide-y divide-slate-200 text-sm text-slate-700">
              {items.map((site) => {
                if (!site || !site.id) {
                  return null;
                }

                const isExpanded =
                  !!expandedIds[site.id];

                const details =
                  siteDetails[site.id];
                const canChangeStatus = isAdmin || (user?.role === 'Kierownik Budowy' &&
                  details?.managers.some(manager => manager.id === user.id));

                const isLoadingThisDetails =
                  !!loadingDetails[site.id];

                const currentStatus =
                  site.status ??
                  SiteStatus.Planned;

                return (
                  <React.Fragment
                    key={site.id}
                  >
                    <tr className="hover:bg-slate-50/80 transition-colors">
                      <td
                        className="py-3.5 px-4 text-slate-400 cursor-pointer"
                        onClick={() =>
                          toggleRow(site.id)
                        }
                      >
                        {isExpanded ? (
                          <ChevronUpIcon className="w-4 h-4 text-amber-600 font-bold" />
                        ) : (
                          <ChevronDownIcon className="w-4 h-4" />
                        )}
                      </td>

                      <td className="py-3.5 px-4 font-bold text-slate-900">
                        {site.name ||
                          '---'}
                      </td>

                      <td className="py-3.5 px-4 text-slate-600">
                        {site.address ? (
                          <span className="flex items-center gap-1.5">
                            <MapPinIcon className="w-4 h-4 text-slate-400 shrink-0" />

                            {site.address}
                          </span>
                        ) : (
                          <span className="text-slate-400 italic">
                            Brak adresu
                          </span>
                        )}
                      </td>

                      <td className="py-3.5 px-4 text-center text-xs text-slate-500">
                        <span className="inline-flex items-center gap-1">
                          <CalendarIcon className="w-3.5 h-3.5 text-slate-400" />

                          {site.startDate ||
                            '---'}{' '}
                          do{' '}
                          {site.endDate ||
                            '---'}
                        </span>
                      </td>

                      <td className="py-3.5 px-4 text-center">
                        <span
                          className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border ${
                            SiteStatusColors[
                              currentStatus
                            ] ||
                            'bg-slate-100 text-slate-700'
                          }`}
                        >
                          {SiteStatusLabels[
                            currentStatus
                          ] || 'Nieznany'}
                        </span>
                      </td>
                    </tr>

                    {isExpanded && (
                      <tr className="bg-slate-50/60 border-b border-slate-200">
                        <td
                          colSpan={5}
                          className="py-3 px-6"
                        >
                          <div className="pl-6 border-l-2 border-amber-400 space-y-5">

                            <SiteManagersSection
                              site={site} details={details} isAdmin={isAdmin}
                              isLoadingThisDetails={isLoadingThisDetails} deletingUserId={deletingUserId}
                              setAddUserSite={setAddUserSite} handleDeleteUser={handleDeleteUser}
                            />
                            <SiteEquipmentSection
                              site={site} details={details} canChangeStatus={!!canChangeStatus} canFinishSite={isAdmin}
                              currentStatus={currentStatus} isLoadingThisDetails={isLoadingThisDetails}
                              setInitialStockSite={setInitialStockSite} onOpenStatusModal={onOpenStatusModal}
                              onReturnEquipment={currentStatus !== SiteStatus.Completed &&
                                (user?.role === 'Kierownik Budowy' && details?.managers.some(manager => manager.id === user.id))
                                ? equipmentId => setReturnEquipment({ siteId: site.id, equipmentId }) : undefined}
                            />
                          </div>
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>

      {/* MODAL DODAWANIA KIEROWNIKA */}
      {returnEquipment && <CreatePzModal isOpen={true}
        initialSiteId={returnEquipment.siteId} initialEquipmentId={returnEquipment.equipmentId}
        onClose={() => setReturnEquipment(null)}
        onSuccess={async () => {
          await loadSiteDetails(returnEquipment.siteId);
          setReturnEquipment(null);
          await onStockChanged?.();
        }} />}

      {addUserSite &&
        siteDetails[addUserSite.id] && (
          <AddUserToSiteModal
            isOpen={true}
            siteId={addUserSite.id}
            siteName={addUserSite.name}
            assignedUserIds={
              siteDetails[
                addUserSite.id
              ].managers.map(
                (manager) =>
                  manager.id
              )
            }
            onClose={() =>
              setAddUserSite(null)
            }
            onSuccess={
              handleAddUserSuccess
            }
          />
        )}

      {/* MODAL BILANSU OTWARCIA */}

      {initialStockSite && (
        <SetSiteInitialStockModal
          isOpen={true}
          site={initialStockSite}
          onClose={() =>
            setInitialStockSite(null)
          }
          onSuccess={
            handleInitialStockSuccess
          }
        />
      )}
    </>
  );
};
