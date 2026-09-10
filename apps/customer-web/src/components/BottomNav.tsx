import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Utensils, User, QrCode, ClipboardList } from 'lucide-react';

const BottomNav = () => {
  const location = useLocation();

  const navItems = [
    { path: '/', label: 'Thực đơn', icon: <Utensils size={20} /> },
    { path: '/reservation', label: 'Đặt bàn', icon: <ClipboardList size={20} /> },
    { path: '/scan', label: 'Quét mã', icon: <QrCode size={20} /> },
    { path: '/profile', label: 'Cá nhân', icon: <User size={20} /> },
  ];

  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-white/80 backdrop-blur-xl border-t border-gray-100 px-6 py-3 flex justify-between items-center z-[100] safe-bottom">
       {navItems.map((item) => (
         <Link
            key={item.path}
            to={item.path}
            className={`flex flex-col items-center space-y-1 transition-all ${
               location.pathname === item.path ? 'text-blue-600 scale-110' : 'text-gray-400'
            }`}
         >
            <div className={`p-2 rounded-2xl ${location.pathname === item.path ? 'bg-blue-50' : ''}`}>
               {item.icon}
            </div>
            <span className="text-[10px] font-black uppercase tracking-tighter">{item.label}</span>
         </Link>
       ))}
    </nav>
  );
};

export default BottomNav;
