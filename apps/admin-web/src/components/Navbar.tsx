import React, { useState } from 'react';
import { Bell, Settings, User, Search, HelpCircle, ChevronRight, Globe, LogOut, Printer, Users, Store, ShieldCheck, CreditCard, BellRing } from 'lucide-react';

interface NavbarProps {
  activeTab: string;
  setActiveTab: (tab: string) => void;
  onLogout: () => void;
  userName?: string;
}

const Navbar: React.FC<NavbarProps> = ({ activeTab, setActiveTab, onLogout, userName }) => {
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isSettingsOpen, setIsSettingsOpen] = useState(false);

  const navItems = [
    { id: 'dashboard', label: 'Tổng quan' },
    { id: 'products', label: 'Thực đơn' },
    { id: 'tables', label: 'Phòng/Bàn' },
    { id: 'invoices', label: 'Giao dịch' },
    { id: 'expenses', label: 'Chi tiêu' },
    { id: 'employees', label: 'Nhân viên' },
  ];

  const navigateTo = (tabId: string) => {
    setActiveTab(tabId);
    setIsUserMenuOpen(false);
    setIsSettingsOpen(false);
  };

  return (
    <nav className="bg-[#0070f4] text-white h-12 flex items-center justify-between px-4 shadow-md sticky top-0 z-50">
      <div className="flex items-center h-full">
        {/* Logo/Brand */}
        <div className="flex items-center mr-6 font-bold text-lg italic tracking-tighter cursor-pointer" onClick={() => navigateTo('dashboard')}>
          <div className="bg-white text-[#0070f4] rounded-full w-6 h-6 flex items-center justify-center mr-1 not-italic text-sm">K</div>
          KiotViet
        </div>

        {/* Nav Links */}
        <div className="flex h-full items-center space-x-1">
          {navItems.map((item) => (
            <button
              key={item.id}
              onClick={() => navigateTo(item.id)}
              className={`px-3 h-full flex items-center text-[13px] font-medium transition-colors hover:bg-blue-600 ${
                activeTab === item.id ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              {item.label}
            </button>
          ))}
        </div>
      </div>

      <div className="flex items-center space-x-4">
        {/* Search Box */}
        <div className="relative hidden lg:block">
          <span className="absolute inset-y-0 left-0 pl-2 flex items-center">
            <Search className="h-3.5 w-3.5 text-blue-200" />
          </span>
          <input
            type="text"
            className="bg-blue-600/50 border-none rounded text-xs py-1.5 pl-8 pr-2 w-48 placeholder-blue-200 text-white focus:ring-1 focus:ring-white outline-none"
            placeholder="Tìm món, hóa đơn..."
          />
        </div>

        <div className="flex items-center space-x-3 relative">
          <button className="p-1 hover:bg-blue-600 rounded transition-colors" title="Thông báo">
            <Bell size={18} />
          </button>

          {/* Settings Icon & Dropdown */}
          <div className="relative">
            <button
              onClick={() => { setIsSettingsOpen(!isSettingsOpen); setIsUserMenuOpen(false); }}
              className={`p-1 hover:bg-blue-600 rounded transition-colors ${isSettingsOpen || activeTab === 'settings' ? 'bg-blue-700' : ''}`}
              title="Thiết lập"
            >
              <Settings size={18} />
            </button>

            {isSettingsOpen && (
              <div className="absolute right-0 mt-2 w-72 bg-white rounded-lg shadow-2xl border border-gray-200 z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-2">
                <div className="p-3 border-b bg-gray-50 flex items-center text-sm font-bold text-gray-700">
                  <Settings className="h-4 w-4 mr-2 text-blue-600" /> Thiết lập hệ thống
                </div>
                <div className="p-1">
                   {[
                     { icon: <Settings className="h-4 w-4" />, label: "Thiết lập tính năng", tab: 'settings' },
                     { icon: <Printer className="h-4 w-4" />, label: "Quản lý mẫu in", tab: 'print-templates' },
                     { icon: <Store className="h-4 w-4" />, label: "Quản lý chi nhánh", tab: 'branches' },
                     { icon: <Users className="h-4 w-4" />, label: "Quản lý người dùng", tab: 'users' },
                   ].map((item, i) => (
                     <button
                       key={i}
                       onClick={() => navigateTo(item.tab)}
                       className="w-full flex items-center justify-between px-3 py-2 hover:bg-blue-50 rounded group transition-colors"
                     >
                       <div className="flex items-center text-sm text-gray-600 group-hover:text-blue-700 font-medium">
                         <span className="mr-3 text-gray-400 group-hover:text-blue-500">{item.icon}</span> {item.label}
                       </div>
                       <ChevronRight className="h-3 w-3 text-gray-300 group-hover:text-blue-400" />
                     </button>
                   ))}
                </div>

                <div className="p-1 border-t">
                  <p className="px-3 py-1 text-[10px] font-bold text-gray-400 uppercase tracking-widest">Tiện ích</p>
                  {[
                    { icon: <BellRing size={14}/>, label: "Thông báo hệ thống" },
                    { icon: <ShieldCheck size={14}/>, label: "Bảo mật & Phân quyền" },
                    { icon: <CreditCard size={14}/>, label: "Lịch sử thanh toán gói" },
                  ].map((item, i) => (
                    <button
                      key={i}
                      onClick={() => navigateTo('settings')}
                      className="w-full flex items-center px-3 py-2 text-sm text-gray-600 hover:bg-gray-50 rounded transition-colors"
                    >
                      <span className="mr-3 text-gray-400">{item.icon}</span> {item.label}
                    </button>
                  ))}
                </div>

                <div className="p-1 border-t bg-gray-50 text-center">
                   <button
                     onClick={() => navigateTo('settings')}
                     className="text-[11px] font-bold text-blue-600 py-1.5 uppercase w-full hover:bg-blue-100 transition-colors"
                   >
                     Xem tất cả thiết lập
                   </button>
                </div>
              </div>
            )}
          </div>

          <button className="p-1 hover:bg-blue-600 rounded transition-colors" title="Hướng dẫn">
            <HelpCircle size={18} />
          </button>

          <div className="h-6 w-[1px] bg-blue-400 mx-1"></div>

          {/* User Profile & Dropdown */}
          <div className="relative">
            <button
              onClick={() => { setIsUserMenuOpen(!isUserMenuOpen); setIsSettingsOpen(false); }}
              className={`flex items-center space-x-2 hover:bg-blue-600 px-2 py-1 rounded transition-colors ${isUserMenuOpen || activeTab === 'profile' ? 'bg-blue-700' : ''}`}
            >
              <div className="w-7 h-7 bg-blue-300 rounded-full flex items-center justify-center text-blue-700">
                <User size={16} />
              </div>
              <span className="text-[13px] font-medium hidden sm:block">{userName || 'Admin'}</span>
            </button>

            {isUserMenuOpen && (
              <div className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-2xl border border-gray-200 z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-2">
                <div className="p-4 border-b flex items-center bg-gray-50">
                  <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center mr-3">
                    <User className="h-6 w-6 text-blue-500" />
                  </div>
                  <div>
                    <p className="text-sm font-bold">{userName || 'Quản trị viên'}</p>
                    <p className="text-[10px] text-orange-500 font-medium">Chưa bật xác thực 2 lớp</p>
                  </div>
                </div>

                <div className="p-1">
                  <button onClick={() => navigateTo('profile')} className="w-full flex items-center justify-between px-3 py-2 text-sm text-gray-600 hover:bg-blue-50 rounded group">
                    <span className="flex items-center"><Store className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" /> Hồ sơ cửa hàng</span>
                    <ChevronRight className="h-4 w-4 text-gray-300" />
                  </button>
                  <button onClick={() => navigateTo('profile')} className="w-full flex items-center justify-between px-3 py-2 text-sm text-gray-600 hover:bg-blue-50 rounded group">
                    <span className="flex items-center"><Users className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" /> Chi nhánh</span>
                    <ChevronRight className="h-4 w-4 text-gray-300" />
                  </button>
                </div>

                <div className="p-1 border-t">
                  <div className="flex items-center justify-between px-3 py-2">
                    <span className="text-sm text-gray-600 flex items-center"><Globe className="h-4 w-4 mr-3 text-gray-400" /> Ngôn ngữ</span>
                    <span className="text-[10px] font-bold text-gray-400 flex items-center uppercase">Tiếng Việt <ChevronRight className="h-3 w-3 ml-1" /></span>
                  </div>
                  <button
                    onClick={onLogout}
                    className="w-full flex items-center px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded font-medium mt-1 transition-colors"
                  >
                    <LogOut className="h-4 w-4 mr-3" /> Đăng xuất
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
