import React from 'react';
import { Link } from 'react-router-dom';
import { MapPin, User } from 'lucide-react';

interface CustomerHeaderProps {
  homeHref: string;
  isGuest?: boolean;
}

const CustomerHeader: React.FC<CustomerHeaderProps> = ({ homeHref, isGuest = false }) => (
  <header className="sticky top-0 z-[120] flex min-h-16 items-center justify-between border-b border-[#ebe5dc] bg-[#fffdf9]/95 px-4 shadow-sm backdrop-blur-xl sm:px-6">
    <Link to={homeHref} aria-label="DOAN Restaurant - Trang chủ" className="flex min-h-11 min-w-0 items-center rounded-xl focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-300">
      <span className="text-lg font-black tracking-tight text-[#22252b]">DOAN <span className="text-[#ee6c36]">Restaurant</span></span>
    </Link>
    <div className="flex items-center gap-2 rounded-full border border-[#eadfd3] bg-white px-3 py-1.5 text-[10px] font-bold text-[#6e6258] shadow-sm">
      <MapPin size={13} className="text-[#3e8069]" aria-hidden="true" />
      <span className="hidden sm:inline">DOAN Restaurant</span>
      <span className="flex items-center gap-1 border-l border-[#eadfd3] pl-2">
        <User size={13} className="text-[#ee6c36]" aria-hidden="true" />
        {isGuest ? 'Khách vãng lai' : 'Khách hàng'}
      </span>
    </div>
  </header>
);

export default CustomerHeader;
