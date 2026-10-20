import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Utensils, User, QrCode, ClipboardList } from 'lucide-react';

const BottomNav = () => {
  const location = useLocation();
  const qrToken = new URLSearchParams(window.location.search).get('qr') || sessionStorage.getItem('guestQrToken');
  const qrGuestSession = sessionStorage.getItem('guestQrSession') === 'true';

  const navItems = [
    { path: '/', label: 'Thực đơn', icon: <Utensils size={20} /> },
    { path: '/reservation', label: 'Đặt bàn', icon: <ClipboardList size={20} /> },
    { path: '/scan', label: 'Quét mã', icon: <QrCode size={20} /> },
    { path: '/profile', label: 'Cá nhân', icon: <User size={20} /> },
  ];

  return (
    <nav aria-label="Điều hướng Customer Web" className="customer-bottom-nav fixed bottom-0 left-0 right-0 bg-[#fffdf9]/95 backdrop-blur-xl border-t border-[#ebe5dc] px-4 py-2.5 flex justify-around items-center z-[100] shadow-[0_-12px_30px_-24px_rgba(68,46,31,.45)]">
       {navItems.map((item) => (
         <Link
            key={item.path}
            to={qrGuestSession && qrToken && item.path !== '/scan'
              ? `${item.path}?qr=${encodeURIComponent(qrToken)}`
              : item.path}
            className={`flex min-w-[4.5rem] min-h-12 touch-manipulation flex-col items-center justify-center space-y-0.5 rounded-2xl transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-300 ${
               location.pathname === item.path ? 'text-orange-600 scale-105' : 'text-gray-400 hover:text-gray-600'
            }`}
         >
            <div className={`p-2 rounded-2xl ${location.pathname === item.path ? 'bg-orange-50' : ''}`}>
               {item.icon}
            </div>
            <span className="text-[10px] font-black uppercase tracking-tighter">{item.label}</span>
         </Link>
       ))}
    </nav>
  );
};

export default BottomNav;
