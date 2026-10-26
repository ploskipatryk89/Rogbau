import React from 'react';
import { Link } from 'react-router-dom';
import { Logo } from '../ui/Logo';
import { SidebarNav } from './SidebarNav';
import { UserFooter } from './UserFooter';

// Definiujemy, co Sidebar przyjmuje od rodzica
interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  return (
    <>
      {/* 1. Tło przyciemniające na telefonach, gdy menu jest otwarte */}
      {isOpen && (
        <div 
          onClick={onClose}
          className="fixed inset-0 bg-black/50 z-40 md:hidden"
        />
      )}

      {/* 2. Właściwy Sidebar */}
      <aside 
        className={`
          w-64 bg-[#0F172A] text-slate-300 flex flex-col h-screen fixed top-0 left-0 z-50
          transition-transform duration-300 ease-in-out
          ${isOpen ? 'translate-x-0' : '-translate-x-full'} 
          md:translate-x-0
        `}
      >
        <Link to="/dashboard" onClick={onClose}>
          <Logo />
        </Link>
        
        <hr className="text-slate-400 mx-3" />
        
        {/* Przekazujemy onClose też do nawigacji, żeby po kliknięciu w link na telefonie menu się schowało */}
        <div onClick={onClose} className="flex-1 flex flex-col overflow-y-auto">
          <SidebarNav />
        </div>
        
        <UserFooter />
      </aside>
    </>
  );
};