import React, { useState, useEffect } from 'react';
import { NotificationBell } from '../Notifications/NotificationBell';
import { NotificationList } from '../Notifications/NotificationsList';
import type { NotificationResponse } from '../../types/response/NotificationResponse';
import { getMyNotifications, markAsRead, deleteNotification } from '../../api/notificationService';
import { notificationHub } from '../../api/notificationHubService';

interface NavbarProps {
  title: string;
  onToggleSidebar: () => void;
}

export const Navbar: React.FC<NavbarProps> = ({ title, onToggleSidebar }) => {
  const [notifications, setNotifications] = useState<NotificationResponse[]>([]);
  const [isOpen, setIsOpen] = useState(false);

  // Pobieranie powiadomień przy starcie komponentu
 useEffect(() => {
  let active = true;

  const fetchNotifications = async () => {
    try {
      const data = await getMyNotifications();
      if (!active) return;
      setNotifications(prev => [
        ...prev.filter(n => !data.some(saved => saved.id === n.id)),
        ...data,
      ]);
    } catch (error) {
      console.error(
        "Błąd pobierania powiadomień:",
        error
      );
    }
  };

  fetchNotifications();

  const receiveNotification = (newNotification: NotificationResponse) => {
    if (!active) return;

    setNotifications(prev => [
      newNotification,
      ...prev.filter(n => n.id !== newNotification.id)
    ]);

  };

  const refreshNotifications = async () => {
    await notificationHub.start(receiveNotification, () => { void fetchNotifications(); });
    if (active) await fetchNotifications();
  };
  void refreshNotifications();
  window.addEventListener('focus', refreshNotifications);

  return () => {
    active = false;
    window.removeEventListener('focus', refreshNotifications);
    void notificationHub.stop();
  };

}, []);

  // Dzwonek świeci się TYLKO wtedy, gdy jest jakiekolwiek nieprzeczytane powiadomienie
  const hasUnread = notifications.some(n => !n.isRead);

  const handleToggleNotifications = async () => {
    setIsOpen(!isOpen);
    if (!isOpen) {
      try {
        const data = await getMyNotifications();
        setNotifications(prev => [...prev.filter(n => !data.some(saved => saved.id === n.id)), ...data]);
      } catch (error) {
        console.error('Nie udało się odświeżyć powiadomień:', error);
      }
    }
  };

  const handleReadNotification = async (notification: NotificationResponse) => {
    if (notification.isRead) return;
    await markAsRead(notification.id);
    setNotifications(prev => prev.map(item => item.id === notification.id ? { ...item, isRead: true } : item));
  };

  const handleDeleteNotification = async (id: string) => {
    await deleteNotification(id);
    setNotifications(prev => prev.filter(item => item.id !== id));
  };

  return (
    <header className="h-16 bg-white border-b border-slate-200 sticky top-0 z-10 flex items-center justify-between px-4 md:px-8 gap-4">
      
      {/* LEWA STRONA: Hamburger + Tytuł obok siebie */}
      <div className="flex items-center gap-2 sm:gap-3">
        <button 
          onClick={onToggleSidebar}
          className="p-2 rounded-lg text-slate-600 hover:bg-slate-100 md:hidden shrink-0"
        >
          <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        
        <h1 className="text-base sm:text-lg font-bold text-slate-900 truncate">{title}</h1>
      </div>

      {/* PRAWA STRONA: Powiadomienia */}
      <div className="flex items-center gap-6 relative">
       <NotificationBell 
  hasUnread={hasUnread}
  onClick={handleToggleNotifications}
/>

        {/* Rozwijana lista powiadomień */}
        {isOpen && (
          <NotificationList 
            notifications={notifications}
            onRead={handleReadNotification}
            onDelete={handleDeleteNotification} 
            onClose={() => setIsOpen(false)} 
          />
        )}
      </div>
    </header>
  );
};
