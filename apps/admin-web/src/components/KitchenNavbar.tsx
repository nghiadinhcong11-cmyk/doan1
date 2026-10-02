import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Utensils, User, LogOut, ChevronDown, Store, Clock, ChevronRight, Camera, Calendar, LayoutGrid } from 'lucide-react';

interface KitchenNavbarProps {
  onLogout: () => void;
  userName?: string;
}

const KitchenNavbar: React.FC<KitchenNavbarProps> = ({ onLogout, userName }) => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const location = useLocation();
  const branchName = localStorage.getItem('selectedBranchName') || 'Toàn hệ thống';

  const navItems = [
    { path: '/kitchen', label: 'Màn hình bếp', icon: <Utensils size={16} className="text-orange-500" /> },
    { path: '/kitchen/history', label: 'Lịch sử đơn bếp', icon: <Clock size={16} className="text-blue-500" /> },
    { path: '/kitchen/tables', label: 'Sơ đồ bàn', icon: <LayoutGrid size={16} className="text-blue-500" /> },
    { path: '/kitchen/attendance', label: 'Chấm công nhận diện', icon: <Camera size={16} className="text-orange-500" /> },
    { path: '/kitchen/reservations', label: 'Lịch đặt bàn trước', icon: <Calendar size={16} className="text-blue-500" /> },
    { path: '/kitchen/schedule', label: 'Lịch làm việc', icon: <Calendar size={16} className="text-green-500" /> },
    { path: '/kitchen/profile', label: 'Hồ sơ cá nhân', icon: <User size={16} className="text-purple-500" /> },
  ];

  return (
    <nav className="bg-[#1e293b] text-white h-12 flex items-center justify-between px-4 shadow-md z-[200] relative border-b border-slate-700">
       {/* Brand Logo */}
       <Link to="/kitchen" className="flex items-center font-bold text-lg italic tracking-tighter cursor-pointer">
          <div className="bg-orange-500 text-white rounded-full w-6 h-6 flex items-center justify-center mr-2 not-italic text-sm font-black">K</div>
          DOAN KITCHEN
       </Link>

       <div className="flex-1"></div>

       {/* Quick Status */}
       <div className="hidden md:flex items-center bg-white/5 px-3 py-1 rounded-full text-[10px] font-bold space-x-4 border border-white/5 mr-4">
          <div className="flex items-center"><Clock size={12} className="mr-1 opacity-60"/> {new Date().toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})}</div>
          <div className="flex items-center"><Store size={12} className="mr-1 opacity-60"/> {branchName}</div>
       </div>

       {/* User Dropdown Section */}
       <div className="relative">
          <button
            onClick={() => setIsMenuOpen(!isMenuOpen)}
            className={`flex items-center space-x-2 px-3 py-1.5 rounded-lg transition-all ${isMenuOpen ? 'bg-slate-700' : 'hover:bg-slate-700/50'}`}
          >
             <div className="w-7 h-7 bg-orange-500 rounded-full flex items-center justify-center text-white font-black text-xs border border-white/20 shadow-inner">
                {(userName?.charAt(0) || 'K').toUpperCase()}
             </div>
             <div className="text-left hidden sm:block">
                <p className="text-[10px] font-bold text-slate-400 uppercase leading-none mb-0.5">Bếp trưởng</p>
                <p className="text-xs font-black tracking-tight">{userName}</p>
             </div>
             <ChevronDown size={14} className={`opacity-60 transition-transform duration-200 ${isMenuOpen ? 'rotate-180' : ''}`} />
          </button>

          {/* Dropdown Menu */}
          {isMenuOpen && (
            <>
              <div className="fixed inset-0 z-40" onClick={() => setIsMenuOpen(false)}></div>
              <div className="absolute right-0 mt-2 w-64 bg-white rounded-2xl shadow-2xl border border-gray-100 z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-2 duration-200">
                 <div className="p-4 bg-gray-50 border-b flex items-center">
                    <div className="w-10 h-10 bg-orange-100 rounded-full flex items-center justify-center text-orange-600 mr-3">
                       <Utensils size={20} />
                    </div>
                    <div>
                       <p className="font-black text-sm text-gray-800">{userName}</p>
                       <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest">Khu vực chế biến</p>
                    </div>
                 </div>

                 <div className="p-2">
                    {navItems.map((item) => (
                      <Link
                        key={item.path}
                        to={item.path}
                        onClick={() => setIsMenuOpen(false)}
                        className={`flex items-center justify-between px-4 py-3 rounded-xl transition-all group ${
                          location.pathname === item.path ? 'bg-orange-50 text-orange-600' : 'hover:bg-gray-50 text-gray-600'
                        }`}
                      >
                         <div className="flex items-center">
                            <span className="mr-3">{item.icon}</span>
                            <span className="text-sm font-bold">{item.label}</span>
                         </div>
                         <ChevronRight size={14} className="opacity-0 group-hover:opacity-100 -translate-x-2 group-hover:translate-x-0 transition-all text-orange-400" />
                      </Link>
                    ))}
                 </div>

                 <div className="p-2 border-t bg-gray-50/50">
                    <button
                      onClick={() => { setIsMenuOpen(false); onLogout(); }}
                      className="w-full flex items-center justify-center p-3 text-red-600 hover:bg-red-50 rounded-xl transition-colors text-sm font-black uppercase tracking-widest"
                    >
                       <LogOut size={16} className="mr-2" /> Đăng xuất
                    </button>
                 </div>
              </div>
            </>
          )}
       </div>
    </nav>
  );
};

export default KitchenNavbar;
