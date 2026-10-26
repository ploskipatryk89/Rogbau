import { DashboardPage } from './pages/Dashboard';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './components/layout/MainLayout';
import { LoginPage } from './pages/LoginPage';
import { EquipmentPage } from './pages/EquipmentPage';
import { EquipmentDetailsPage } from './pages/EquipmentDetailsPage';
import { PurchasesPage } from './pages/PurchasePage';
import { SitesPage } from './pages/SitesPage';
import { SupplierPage } from './pages/SupplierPage';
import { UserPage } from './pages/UserPage';
import TransferPage from './pages/TransferPage';
import { TransferDetailsPage } from './pages/TransferDetailsPage';
import { HistoricalDataPage } from './pages/HistoricalDataPage';
import { OrderDetailsPage } from './pages/OrderDetailsPage';
import { CategoriesPage } from './pages/CategoriesPage';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Główny adres startuje od logowania */}
        <Route path="/" element={<Navigate to="/login" replace />} />
        <Route path="/login" element={<LoginPage />} />

        {/* Widoki aplikacji owinięte w MainLayout */}
        <Route element={<MainLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/sites" element={<SitesPage />} />
          <Route path="/purchases" element={<PurchasesPage />} />
          <Route path="/equipment" element={<EquipmentPage />} />
          <Route path="/categories" element={<CategoriesPage />} />
          <Route path="/suppliers" element={<SupplierPage />} />
          {/* Rejestracja ścieżki dla szczegółów sprzętu */}
          <Route path="/equipment/:id" element={<EquipmentDetailsPage />} />
          <Route path="/equipments/:id" element={<EquipmentDetailsPage />} />
          <Route path="/transfers" element={<TransferPage/>} />
          <Route path="/transfers/:id" element={<TransferDetailsPage />} />
          <Route path="/orders/:id" element={<OrderDetailsPage />} />
          <Route path="/historical-data" element={<HistoricalDataPage />} />
          <Route path="/users" element={<UserPage/>}/>
        </Route>

        {/* Każdy nieznany adres wraca do logowania */}
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
