import { ErrorMessage } from "../components/ui/ErrorMessage";
import { useAuth } from "../context/AuthContext";
import React, {
  useEffect,
  useState,
} from 'react';

import type {
  SiteDto,
} from '../types/response/SiteResponse';

import {
  getSites,
} from '../api/siteService';

import { SiteTable } from '../components/sites/SiteTable';
import { CreateSiteModal } from '../components/sites/CreateSiteModal';
import { StartFinishSiteModal } from '../components/sites/StartFinishSiteModal';
import { PageHeader } from '../components/ui/PageHeader';
import { SiteFilters } from '../components/sites/SiteFilters';

export const SitesPage: React.FC = () => {
  const { user } = useAuth();
  const [statusFilter, setStatusFilter] = useState('');
  const [sites, setSites] =
    useState<SiteDto[]>([]);

  const [loading, setLoading] =
    useState<boolean>(true);

  const [error, setError] =
    useState<string | null>(null);

  const [
    isCreateModalOpen,
    setIsCreateModalOpen,
  ] = useState<boolean>(false);

  const [
    selectedSiteForStatus,
    setSelectedSiteForStatus,
  ] = useState<SiteDto | null>(
    null
  );

  const [
    statusModalMode,
    setStatusModalMode,
  ] = useState<
    'start' | 'finish'
  >('start');

  const fetchSites = async () => {
    try {
      setLoading(true);
      setError(null);

      const data =
        await getSites();

      setSites(
        Array.isArray(data)
          ? data
          : []
      );
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Wystąpił błąd podczas pobierania budów.'
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSites();
  }, []);

  const handleOpenStatusModal = (
    site: SiteDto,
    mode: 'start' | 'finish'
  ) => {
    setSelectedSiteForStatus(
      site
    );

    setStatusModalMode(mode);
  };

  const handleCloseStatusModal =
    () => {
      setSelectedSiteForStatus(
        null
      );
    };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Rejestr budów"
        subTile="Zarządzaj budowami i realizowanymi projektami budowlanymi"
        onClickFetchButton={
          fetchSites
        }
        loadingItems={loading}
        onclickActionButton={() =>
          setIsCreateModalOpen(
            true
          )
        }
        actionButtonText="Dodaj budowę"
        actionButtonVisible={user?.role === 'Admin'}
      />

      {error && (
        <ErrorMessage message={error} className="p-4" />
      )}

      <SiteFilters status={statusFilter} onStatusChange={setStatusFilter} />

      <SiteTable
        items={sites.filter(site => !statusFilter || String(site.status) === statusFilter)}
        isLoading={loading}
        onStockChanged={fetchSites}
        onOpenStatusModal={
          handleOpenStatusModal
        }
      />

      <CreateSiteModal
        isOpen={
          isCreateModalOpen
        }
        onClose={() =>
          setIsCreateModalOpen(
            false
          )
        }
        onSuccess={fetchSites}
      />

      {selectedSiteForStatus && (
        <StartFinishSiteModal
          siteId={
            selectedSiteForStatus.id
          }
          mode={
            statusModalMode
          }
          onClose={
            handleCloseStatusModal
          }
          onSuccess={
            fetchSites
          }
        />
      )}
    </div>
  );
};

export default SitesPage;
