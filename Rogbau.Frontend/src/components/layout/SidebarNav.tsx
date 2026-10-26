import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { 
  TruckIcon,
  TagIcon,
  HomeIcon,
  WrenchScrewdriverIcon, 
  ShoppingCartIcon, 
  BuildingOffice2Icon, 
  ArrowsRightLeftIcon, 
  UsersIcon,
  ChartBarIcon
} from '@heroicons/react/24/outline';
import { useAuth } from '../../context/AuthContext';

interface NavItem {
  name: string;
  path: string;
  icon: React.ComponentType<{ className?: string }>;
  roles?: string[];
}

const navItems: NavItem[] = [
  { name: 'Pulpit', path: '/dashboard', icon: HomeIcon },
  { name: 'Sprzęt', path: '/equipment', icon: WrenchScrewdriverIcon },
  { name: 'Kategorie', path: '/categories', icon: TagIcon, roles: ['Admin'] },
  { name: 'Zakupy', path: '/purchases', icon: ShoppingCartIcon, roles: ['Admin', 'Magazynier'] },
  { name: 'Budowy', path: '/sites', icon: BuildingOffice2Icon },
  { name: 'Transfery', path: '/transfers', icon: ArrowsRightLeftIcon },
  { name: 'Dane historyczne', path: '/historical-data', icon: ChartBarIcon, roles: ['Admin'] },
  { name: 'Dostawcy', path: '/suppliers', icon: TruckIcon, roles: ['Admin', 'Magazynier'] },
  { name: 'Użytkownicy', path: '/users', icon: UsersIcon, roles: ['Admin'] },
];





export const SidebarNav: React.FC = () => {
  const location = useLocation();
  const {user} = useAuth();


  const visibleNavItems = navItems.filter(item => {
  if (item.roles && !item.roles.includes(user?.role ?? '')) {
    return false;
  }

  return true;
});

  return (
    <nav className="flex-1 px-3 py-4 space-y-1">
      {visibleNavItems.map((item) => {
        const isActive = location.pathname === item.path || location.pathname.startsWith(item.path + '/');
        
   

        const baseStyles = "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-all duration-150";
        const activeStyles = isActive
          ? "bg-slate-800/90 text-amber-500 border-l-4 border-amber-500 rounded-l-none pl-2 font-semibold"
          : "text-slate-400 hover:bg-slate-800/50 hover:text-slate-200";

        return (
          <NavLink
            key={item.path}
            to={item.path}
            className={`${baseStyles} ${activeStyles}`}
          >
            <item.icon className="w-5 h-5 shrink-0" />
            <span>{item.name}</span>
          </NavLink>
        );
      })}
    </nav>
  );
};
