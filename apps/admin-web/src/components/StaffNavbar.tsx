import React, { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Calendar, Camera, ChevronDown, ChevronRight, LogOut, Package, User, Users } from 'lucide-react';

interface StaffNavbarProps { onLogout: () => void; userName?: string; }

export default function StaffNavbar({ onLogout, userName }: StaffNavbarProps) {
  const [open, setOpen] = useState(false);
  const location = useLocation();
  const items = [
    { path: '/staff/profile', label: 'Hồ sơ cá nhân', icon: User },
    { path: '/staff/attendance', label: 'Chấm công', icon: Camera },
    { path: '/staff/schedule', label: 'Lịch làm việc', icon: Calendar },
    { path: '/staff/inventory', label: 'Tổng quan kho', icon: Package }
  ];
  return <nav className="relative z-[200] flex h-12 items-center justify-between border-b border-blue-800 bg-[#0f3d75] px-4 text-white shadow-md">
    <Link to="/staff/profile" className="flex items-center text-lg font-bold italic tracking-tight"><span className="mr-2 flex h-6 w-6 items-center justify-center rounded-full bg-blue-500 text-sm not-italic">S</span>DOAN STAFF</Link>
    <div className="relative"><button type="button" onClick={() => setOpen(!open)} className="flex items-center gap-2 rounded-lg px-3 py-1.5 hover:bg-blue-800"><span className="flex h-7 w-7 items-center justify-center rounded-full bg-blue-500 text-xs font-bold">{(userName?.charAt(0) || 'S').toUpperCase()}</span><span className="hidden text-left sm:block"><span className="block text-[10px] uppercase text-blue-200">Nhân viên</span><span className="block text-xs font-bold">{userName}</span></span><ChevronDown size={14} /></button>
      {open && <><button type="button" aria-label="Đóng menu" className="fixed inset-0 z-40 cursor-default" onClick={() => setOpen(false)} /><div className="absolute right-0 z-50 mt-2 w-64 overflow-hidden rounded-2xl border border-slate-100 bg-white text-slate-800 shadow-2xl"><div className="p-2">{items.map(({ path, label, icon: Icon }) => <Link key={path} to={path} onClick={() => setOpen(false)} className={`group flex items-center justify-between rounded-xl px-4 py-3 text-sm font-bold ${location.pathname === path ? 'bg-blue-50 text-blue-700' : 'text-slate-600 hover:bg-slate-50'}`}><span className="flex items-center gap-3"><Icon size={16} />{label}</span><ChevronRight size={14} className="opacity-0 transition-opacity group-hover:opacity-100" /></Link>)}</div><div className="border-t bg-slate-50 p-2"><button type="button" onClick={onLogout} className="flex w-full items-center justify-center gap-2 rounded-xl p-3 text-sm font-bold text-red-600 hover:bg-red-50"><LogOut size={16} />Đăng xuất</button></div></div></>}
    </div>
  </nav>;
}
