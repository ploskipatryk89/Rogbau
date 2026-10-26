import React from 'react';
import { Outlet, useLocation, Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { Sidebar } from './Sidebar';
import { Navbar } from './Navbar';
import { useState } from 'react';

const pageTitles: Record<string, string> = {
  '/': 'Dashboard',
  '/dashboard': 'Pulpit',
  '/sites': 'Budowy i Inwestycje',
  '/purchases': 'Rejestr Zakupów i Faktur',
  '/equipment': 'Magazyn Sprzętu',
  '/categories': 'Kategorie sprzętu',
  '/transfers': 'Przesunięcia Międzybudowlane',
  '/suppliers': 'Dostawcy sprzętów',
  '/users': 'Użytkownicy',
};

export const MainLayout: React.FC = () => {

  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const location = useLocation();
  const { user, loading } = useAuth();
  const currentTitle = pageTitles[location.pathname] || 'Panel Zarządzania';

  if (loading) return <div className="p-8 text-slate-500">Ładowanie sesji...</div>;
  if (!user) return <Navigate to="/login" replace />;
  if (user.mustChangePassword) return <Navigate to="/login" replace />;
  if (location.pathname.startsWith('/users') && user.role !== 'Admin')
    return <Navigate to="/dashboard" replace />;
  if (location.pathname.startsWith('/categories') && user.role !== 'Admin')
    return <Navigate to="/dashboard" replace />;
  if (location.pathname.startsWith('/historical-data') && user.role !== 'Admin')
    return <Navigate to="/dashboard" replace />;
  if ((location.pathname.startsWith('/purchases') || location.pathname.startsWith('/suppliers')) &&
      user.role !== 'Admin' && user.role !== 'Magazynier')
    return <Navigate to="/dashboard" replace />;

  return (
    <div className="min-h-screen bg-slate-50 flex">
      {/* Fixed Sidebar */}
      <Sidebar 
      isOpen={isSidebarOpen}
      onClose={() => setIsSidebarOpen(false)}/>
      

      {/* Main Content Wrapper */}
      <div className="flex-1 min-w-0 ml-0 md:ml-64 flex flex-col min-h-screen w-full">
        <Navbar title={currentTitle} onToggleSidebar={() => setIsSidebarOpen(!isSidebarOpen)}/>

        {/* Dynamic Outlet Area */}
        <main className="flex-1 p-8 max-w-7xl w-full mx-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
