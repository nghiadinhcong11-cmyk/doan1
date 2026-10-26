import React, { useState, useEffect, useRef } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Settings, User, HelpCircle, ChevronRight, Globe, LogOut, Printer, Users, Store, ShieldCheck, CreditCard, Users2, ChevronDown, Clock, Calendar, Info, Book, Banknote, Utensils, Settings2, QrCode, X, Bell, BellRing, Loader2, PanelLeftClose, PanelLeftOpen, PanelTopClose, PanelTopOpen, LayoutDashboard, Package } from 'lucide-react';
import { API_URL } from '../config';
import { fetchBranchList } from '../api/branchQuery';
import { useNotificationCenter } from '../features/notifications/useNotificationCenter';
import NotificationBell from '../features/notifications/NotificationBell';
import BrandLogo from '../../../shared/BrandLogo';

const ADMIN_LOGIN_URL = typeof window !== 'undefined' ? window.location.origin : '';

interface Branch {
  id: string;
  name: string;
  isMain?: boolean;
}

interface NavbarProps {
  onLogout: () => void;
  userName?: string;
  userRole?: 'admin' | 'manager' | 'cashier' | null;
  sidebarCollapsed: boolean;
  onSidebarToggle: () => void;
}

type NavItem = {
  id: string;
  label: string;
  path: string;
  icon: React.ReactNode;
};

const Navbar: React.FC<NavbarProps> = ({ onLogout, userName, userRole, sidebarCollapsed, onSidebarToggle }) => {
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isSettingsOpen, setIsSettingsOpen] = useState(false);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [isHelpOpen, setIsHelpOpen] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isCatalogMenuOpen, setIsCatalogMenuOpen] = useState(false);
  const [isCustomerMenuOpen, setIsCustomerMenuOpen] = useState(false);
  const [isEmployeeMenuOpen, setIsEmployeeMenuOpen] = useState(false);
  const [isQRModalOpen, setIsQRModalOpen] = useState(false);
  const [isLanguageMenuOpen, setIsLanguageMenuOpen] = useState(false);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [branchLoadError, setBranchLoadError] = useState(false);
  const [selectedBranch, setSelectedBranch] = useState<Branch | null>(null);
  const navRef = useRef<HTMLElement>(null);
  const location = useLocation();
  const notificationCenter = useNotificationCenter(userRole === 'manager' ? 'manager' : 'admin');
  const pendingOrderCount = notificationCenter.pendingCount;
  const displayedNotifications: any[] = [];
  const loadingNoti = notificationCenter.loadingNotifications;
  const notificationError = notificationCenter.error;

  useEffect(() => {
    setIsCatalogMenuOpen(false);
    setIsCustomerMenuOpen(false);
    setIsEmployeeMenuOpen(false);
  }, [location.pathname]);

  useEffect(() => {
    const closeMenusOutside = (event: PointerEvent) => {
      if (navRef.current?.contains(event.target as Node)) return;
      setIsCatalogMenuOpen(false);
      setIsCustomerMenuOpen(false);
      setIsEmployeeMenuOpen(false);
      setIsSettingsOpen(false);
      setIsHelpOpen(false);
      setIsQRModalOpen(false);
      setIsUserMenuOpen(false);
    };
    document.addEventListener('pointerdown', closeMenusOutside);
    return () => document.removeEventListener('pointerdown', closeMenusOutside);
  }, []);

  const fetchBranches = async () => {
    try {
      const data = await fetchBranchList() as Branch[];
      setBranches(data);

      const savedBranchId = localStorage.getItem('selectedBranchId');
      const branch = data.find(b => b.id === savedBranchId) || data.find(b => b.isMain) || data[0];
      if (branch) {
        setSelectedBranch(branch);
        localStorage.setItem('selectedBranchId', branch.id);
        window.dispatchEvent(new Event('branch:changed'));
      }
    } catch (err) {
      console.error(err);
      setBranchLoadError(true);
    }
  };

  useEffect(() => {
    fetchBranches();
    const handleBranchesChanged = () => { void fetchBranches(); };
    window.addEventListener('branches:changed', handleBranchesChanged);
    return () => window.removeEventListener('branches:changed', handleBranchesChanged);
  }, []);

  const handleBranchChange = (branch: Branch) => {
    if (userRole === 'manager') return;
    setSelectedBranch(branch);
    localStorage.setItem('selectedBranchId', branch.id);
    window.dispatchEvent(new Event('branch:changed'));
  };

  const isActive = (path: string) => location.pathname === path || location.pathname.startsWith(`${path}/`);

  // This is presentation metadata only. Route layouts may still use their area
  // for sizing, but area must never remove an allowed admin/manager nav item.
  const managementNavItems: NavItem[] = [
    { id: 'dashboard', label: 'TỔNG QUAN', path: '/dashboard', icon: <LayoutDashboard size={18} /> },
    { id: 'expenses', label: 'CHI PHÍ', path: '/expenses', icon: <Banknote size={18} /> },
    ...(userRole === 'admin' || userRole === 'manager' ? [{ id: 'inventory', label: 'KHO HÀNG', path: '/inventory', icon: <Package size={18} /> }] : []),
  ];
  const operationsNavItems: NavItem[] = [
    { id: 'pos', label: 'BÁN HÀNG', path: '/pos', icon: <CreditCard size={18} /> },
    { id: 'kitchen', label: 'NHÀ BẾP', path: '/kitchen', icon: <Utensils size={18} /> },
    { id: 'tables', label: 'PHÒNG BÀN', path: '/tables', icon: <Store size={18} /> },
    { id: 'invoices', label: 'HÓA ĐƠN', path: '/invoices', icon: <Printer size={18} /> },
    { id: 'reservations', label: 'ĐẶT BÀN', path: '/reservations', icon: <Calendar size={18} /> },
  ];

  return (
    <nav ref={navRef} className={`sticky top-0 z-50 flex h-12 items-center justify-between bg-[#0070f4] px-4 text-white shadow-md lg:fixed lg:inset-y-0 lg:left-0 lg:h-screen lg:flex-col lg:items-stretch lg:justify-start lg:overflow-visible lg:bg-transparent lg:px-0 lg:pt-0 lg:pb-0 lg:shadow-none lg:z-[60] ${sidebarCollapsed ? 'lg:w-20' : 'lg:w-72'}`}>
      <div className={`flex h-full items-center bg-[#0070f4] lg:h-auto lg:flex-none lg:flex-col lg:items-stretch lg:pt-4 ${sidebarCollapsed ? 'lg:w-20' : 'lg:w-72'}`}>
        {/* Mobile Menu Toggle */}
        <button
          onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
          className="lg:hidden p-2 mr-2 hover:bg-blue-600 rounded-lg transition-colors"
        >
          <div className="space-y-1.5">
            <div className={`w-5 h-0.5 bg-white transition-all ${isMobileMenuOpen ? 'rotate-45 translate-y-2' : ''}`}></div>
            <div className={`w-5 h-0.5 bg-white transition-all ${isMobileMenuOpen ? 'opacity-0' : ''}`}></div>
            <div className={`w-5 h-0.5 bg-white transition-all ${isMobileMenuOpen ? '-rotate-45 -translate-y-2' : ''}`}></div>
          </div>
        </button>

        {/* Logo/Brand */}
        {branchLoadError && <span role="status" className="mr-3 hidden text-[10px] font-bold text-amber-100 xl:inline">Không tải được danh sách cơ sở</span>}
        <Link to="/dashboard" className={`mr-4 flex h-10 shrink-0 cursor-pointer items-center lg:mb-3 lg:mr-0 lg:h-12 lg:justify-center ${sidebarCollapsed ? 'w-12 lg:w-20' : 'w-28 lg:w-72'}`}>
          <BrandLogo markOnly={sidebarCollapsed} className={`shrink-0 ${sidebarCollapsed ? 'h-10 w-10 lg:h-12 lg:w-12' : 'h-10 w-28 lg:h-12 lg:w-44'}`} imageClassName="h-full w-full" />
        </Link>

      </div>
      <div className={`relative hidden flex-1 flex-col items-stretch bg-[#0070f4] lg:flex ${sidebarCollapsed ? 'lg:w-20' : 'lg:w-72'}`}>

        <div className="hidden lg:mb-1 lg:block lg:h-3 lg:w-full lg:overflow-hidden lg:text-blue-200/70" aria-hidden="true">
          <svg viewBox="0 0 240 18" preserveAspectRatio="none" className="h-full w-full">
            <path d="M0 9 C30 1 50 1 80 9 S130 17 160 9 S210 1 240 9" fill="none" stroke="currentColor" strokeWidth="2" />
          </svg>
        </div>

        <button type="button" onClick={onSidebarToggle} className={`hidden lg:mb-2 lg:flex lg:h-10 lg:w-full lg:items-center lg:gap-2 lg:rounded-xl lg:px-3 lg:text-[12px] lg:font-bold lg:uppercase lg:tracking-widest lg:text-blue-100 lg:transition-colors lg:hover:bg-blue-600 ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''}`} aria-label={sidebarCollapsed ? 'Mở rộng thanh điều hướng' : 'Thu gọn thanh điều hướng'} title={sidebarCollapsed ? 'Mở rộng thanh điều hướng' : 'Thu gọn thanh điều hướng'}>
          {sidebarCollapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
          <span className={sidebarCollapsed ? 'lg:hidden' : ''}>{sidebarCollapsed ? 'Mở rộng' : 'Thu gọn'}</span>
        </button>

        {/* Desktop Nav Links */}
        <div className="hidden lg:mt-2 lg:flex lg:h-auto lg:w-full lg:flex-col lg:items-stretch lg:space-y-1">
          {managementNavItems.map((item) => (
            <Link
              key={item.id}
              to={item.path}
              className={`flex h-10 items-center gap-2 rounded-xl px-3 text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${
                isActive(item.path) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              {item.icon}<span className={sidebarCollapsed ? 'lg:hidden' : ''}>{item.label}</span>
            </Link>
          ))}

          {/* THỰC ĐƠN DROPDOWN */}
          <div className="group relative h-auto w-full">
            <button
              onClick={() => setIsCatalogMenuOpen(open => !open)}
              className={`flex h-10 w-full items-center justify-start gap-2 rounded-xl px-3 text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${
                ['/products', '/toppings'].includes(location.pathname) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              <Utensils size={18} /><span className={sidebarCollapsed ? 'lg:hidden' : ''}>Thực đơn</span><ChevronDown size={14} className={`ml-auto opacity-60 group-hover:rotate-180 transition-transform ${sidebarCollapsed ? 'lg:hidden' : ''}`}/>
            </button>

            {/* Dropdown Menu */}
            <div className={`absolute bottom-0 left-full z-[100] ml-0 max-h-[calc(100vh-2rem)] w-56 overflow-y-auto rounded-lg border border-gray-200 bg-white text-gray-800 shadow-2xl ${isCatalogMenuOpen ? 'block' : 'hidden'} group-hover:block animate-in fade-in slide-in-from-left-2`}>
               <Link to="/products" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors border-b border-gray-50">
                  <Utensils size={16} className="mr-3 text-blue-500"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Danh mục món</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Quản lý món ăn, giá bán</p>
                  </div>
               </Link>
               <Link to="/toppings" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors">
                  <Settings2 size={16} className="mr-3 text-orange-500"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Quản lý Topping</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Tùy chọn thêm cho món ăn</p>
                  </div>
               </Link>
            </div>
          </div>

          {/* KHÁCH HÀNG DROPDOWN */}
          <div className="group relative h-auto w-full">
            <button
              onClick={() => setIsCustomerMenuOpen(open => !open)}
              className={`flex h-10 w-full items-center justify-start gap-2 rounded-xl px-3 text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${
                ['/customers', '/promotions'].includes(location.pathname) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              <Users size={18} /><span className={sidebarCollapsed ? 'lg:hidden' : ''}>Khách hàng</span><ChevronDown size={14} className={`ml-auto opacity-60 group-hover:rotate-180 transition-transform ${sidebarCollapsed ? 'lg:hidden' : ''}`}/>
            </button>

            {/* Dropdown Menu */}
            <div className={`absolute bottom-0 left-full z-[100] ml-0 max-h-[calc(100vh-2rem)] w-56 overflow-y-auto rounded-lg border border-gray-200 bg-white text-gray-800 shadow-2xl ${isCustomerMenuOpen ? 'block' : 'hidden'} group-hover:block animate-in fade-in slide-in-from-left-2`}>
               <Link to="/customers" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors border-b border-gray-50">
                  <Users size={16} className="mr-3 text-blue-500"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Danh sách khách</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Quản lý thông tin, tích điểm</p>
                  </div>
               </Link>
               <Link to="/promotions" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors">
                  <CreditCard size={16} className="mr-3 text-orange-500"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Khuyến mãi</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Đổi điểm, voucher quà tặng</p>
                  </div>
               </Link>
            </div>
          </div>

          {/* NHÂN VIÊN DROPDOWN */}
          <div className="group relative h-auto w-full">
            <button
              onClick={() => setIsEmployeeMenuOpen(open => !open)}
              className={`flex h-10 w-full items-center justify-start gap-2 rounded-xl px-3 text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${
                ['/employees', '/attendance', '/schedule', '/shifts'].some(path => isActive(path)) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              <Users size={18} /><span className={sidebarCollapsed ? 'lg:hidden' : ''}>Nhân viên</span><ChevronDown size={14} className={`ml-auto opacity-60 group-hover:rotate-180 transition-transform ${sidebarCollapsed ? 'lg:hidden' : ''}`}/>
            </button>

            {/* Dropdown Menu */}
            <div className={`absolute bottom-0 left-full z-[100] ml-0 max-h-[calc(100vh-2rem)] w-56 overflow-y-auto rounded-lg border border-gray-200 bg-white text-gray-800 shadow-2xl ${isEmployeeMenuOpen ? 'block' : 'hidden'} group-hover:block animate-in fade-in slide-in-from-left-2`}>
               <Link to="/employees" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors border-b border-gray-50">
                  <Users size={16} className="mr-3 text-blue-500"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Hồ sơ nhân viên</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Quản lý hồ sơ và lịch làm</p>
                  </div>
               </Link>
               <Link to="/attendance" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors border-b border-gray-50">
                  <Clock size={16} className="mr-3 text-orange-500"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Chấm công</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Check-in, Check-out ca làm</p>
                  </div>
               </Link>
               <Link to="/schedule" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors">
                  <Calendar size={16} className="mr-3 text-green-500"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Lịch làm việc</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Xếp ca, chia lịch hàng tuần</p>
                  </div>
               </Link>
               <Link to="/shifts" className={`flex items-center px-4 py-3 hover:bg-blue-50 transition-colors border-t border-gray-50 ${isActive('/shifts') ? 'bg-blue-50' : ''}`}>
                  <Banknote size={16} className="mr-3 text-blue-600"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Ca làm</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Đối soát tiền mặt, bàn giao</p>
                  </div>
               </Link>
            </div>
          </div>
          {operationsNavItems.map((item) => (
            <Link
              key={item.id}
              to={item.path}
              className={`flex h-10 items-center gap-2 rounded-xl px-3 text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${
                isActive(item.path) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              {item.icon}<span className={sidebarCollapsed ? 'lg:hidden' : ''}>{item.label}</span>
            </Link>
          ))}
        </div>

      {/* Mobile Menu Overlay */}
      {isMobileMenuOpen && (
        <div className="lg:hidden fixed inset-0 top-12 bg-blue-700 z-[100] animate-in slide-in-from-left duration-300">
           <div className="p-6 space-y-6">
              {managementNavItems.map((item) => (
                <Link
                  key={item.id}
                  to={item.path}
                  onClick={() => setIsMobileMenuOpen(false)}
                  className={`block text-lg font-black uppercase italic tracking-tighter ${isActive(item.path) ? 'text-white' : 'text-blue-200'}`}
                >
                  {item.label}
                </Link>
              ))}
              <div className="pt-6 border-t border-blue-600 space-y-4">
                 <Link to="/products" onClick={() => setIsMobileMenuOpen(false)} className={`block font-black uppercase italic tracking-tighter ${isActive('/products') || isActive('/toppings') ? 'text-white' : 'text-blue-100'}`}>Thực đơn</Link>
                 <Link to="/toppings" onClick={() => setIsMobileMenuOpen(false)} className="block pl-4 text-sm font-bold text-blue-200">Quản lý Topping</Link>
                 <Link to="/customers" onClick={() => setIsMobileMenuOpen(false)} className={`block font-black uppercase italic tracking-tighter ${isActive('/customers') || isActive('/promotions') ? 'text-white' : 'text-blue-100'}`}>Khách hàng</Link>
                 <Link to="/promotions" onClick={() => setIsMobileMenuOpen(false)} className="block pl-4 text-sm font-bold text-blue-200">Khuyến mãi</Link>
                 <Link to="/employees" onClick={() => setIsMobileMenuOpen(false)} className={`block font-black uppercase italic tracking-tighter ${isActive('/employees') || isActive('/attendance') || isActive('/schedule') || isActive('/shifts') ? 'text-white' : 'text-blue-100'}`}>Nhân viên</Link>
                 <Link to="/attendance" onClick={() => setIsMobileMenuOpen(false)} className="block pl-4 text-sm font-bold text-blue-200">Chấm công</Link>
                 <Link to="/schedule" onClick={() => setIsMobileMenuOpen(false)} className="block pl-4 text-sm font-bold text-blue-200">Lịch làm việc</Link>
                 <Link to="/shifts" onClick={() => setIsMobileMenuOpen(false)} className={`block pl-4 text-sm font-bold ${isActive('/shifts') ? 'text-white' : 'text-blue-200'}`}>Ca làm</Link>
                 {operationsNavItems.map((item) => (
                   <Link key={item.id} to={item.path} onClick={() => setIsMobileMenuOpen(false)} className={`block text-lg font-black uppercase italic tracking-tighter ${isActive(item.path) ? 'text-white' : 'text-blue-200'}`}>
                     {item.label}
                   </Link>
                 ))}
                 <button onClick={onLogout} className="text-red-300 font-black uppercase italic tracking-tighter">Đăng xuất</button>
              </div>
           </div>
        </div>
      )}

      <div className="hidden flex-1 flex-col items-stretch gap-1 text-white lg:flex" data-sidebar-tools>
        <div className="w-full">
          <NotificationBell {...notificationCenter} historyPath="/notifications" showLabel={!sidebarCollapsed} menuPlacement="right" />
        </div>
        <div className="flex flex-col items-stretch gap-1">
          {/* Notifications */}
          <div className="relative hidden">
            <button
              onClick={() => {
                setIsNotificationsOpen(!isNotificationsOpen);
                setIsSettingsOpen(false);
                setIsHelpOpen(false);
                setIsUserMenuOpen(false);
              }}
              className={`p-1 hover:bg-blue-600 rounded transition-colors relative ${isNotificationsOpen ? 'bg-blue-700' : ''}`}
              title="Thông báo"
              aria-label={pendingOrderCount > 0 ? `${pendingOrderCount} đơn đang chờ xử lý` : 'Thông báo và đơn đang chờ xử lý'}
            >
              <Bell size={18} />
              {pendingOrderCount > 0 && (
                <span className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full border-2 border-blue-600 text-[9px] flex items-center justify-center font-bold animate-pulse">
                  {pendingOrderCount}
                </span>
              )}
            </button>

            {isNotificationsOpen && (
              <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-2xl border border-gray-200 z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-2">
                <div className="p-3 border-b bg-gray-50 flex items-center justify-between text-[12px] font-bold text-gray-700 uppercase">
                  <div className="flex items-center"><BellRing className="h-4 w-4 mr-2 text-blue-600" /> Thông báo mới</div>
                  {displayedNotifications.filter(n => !n.isRead).length > 0 && (
                    <span className="text-[10px] bg-blue-100 text-blue-600 px-1.5 py-0.5 rounded uppercase">{displayedNotifications.filter(n => !n.isRead).length} Mới</span>
                  )}
                </div>
                <div className="max-h-96 overflow-y-auto">
                   {notificationError && <div role="status" className="border-b border-amber-200 bg-amber-50 px-3 py-2 text-xs text-amber-800">{notificationError}</div>}
                   {loadingNoti && displayedNotifications.length === 0 ? (
                     <div className="py-10 text-center"><Loader2 className="animate-spin mx-auto text-blue-600" /></div>
                   ) : displayedNotifications.length === 0 ? (
                     <div className="py-10 text-center text-gray-400 text-xs italic uppercase">Không có thông báo mới</div>
                   ) : (
                     displayedNotifications.map((n) => (
                       <Link
                         key={n.id}
                         to={n.route || n.link || '#'}
                         onClick={() => {
                           setIsNotificationsOpen(false);
                           if (!n.isPendingOrder && !n.isRead) void fetch(`${API_URL}/api/Notification/${n.id}/read`, { method: 'PATCH' });
                           if (n.type === 'order') {
                              window.dispatchEvent(new CustomEvent('pos-open-notification', { detail: { orderId: n.id } }));
                           }
                         }}
                         className="p-3 border-b border-gray-50 hover:bg-blue-50/50 cursor-pointer transition-colors group block"
                       >
                          <div className="flex justify-between items-start mb-0.5">
                             <p className="text-xs font-black text-gray-800 group-hover:text-blue-700 transition-colors uppercase tracking-tight">{n.title}</p>
                             <span className="text-[9px] text-gray-400 font-bold uppercase">{n.time || new Date(n.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</span>
                          </div>
                          <p className="text-[11px] text-gray-500 italic font-medium leading-tight uppercase">{n.message || n.desc}</p>
                       </Link>
                     ))
                   )}
                </div>
                <div className="p-2 text-center bg-gray-50">
                   <button onClick={() => setIsNotificationsOpen(false)} className="text-[10px] font-black text-blue-600 uppercase tracking-widest hover:underline">Đóng thông báo</button>
                </div>
              </div>
            )}
          </div>

          {/* Settings Icon & Dropdown */}
          {userRole === 'admin' && <div className="group relative">
            <button
              onClick={() => {
                setIsSettingsOpen(!isSettingsOpen);
                setIsMobileMenuOpen(false);
                setIsNotificationsOpen(false);
                setIsHelpOpen(false);
                setIsUserMenuOpen(false);
              }}
              className={`flex items-center gap-2 rounded-xl px-3 py-2 text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 lg:w-full ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${isSettingsOpen || ['/settings', '/settings/receipt', '/print-templates'].some(path => isActive(path)) ? 'bg-blue-700' : ''}`}
              title="Thiết lập"
            >
              <Settings size={18} /><span className={sidebarCollapsed ? 'lg:hidden' : ''}>Thiết lập</span>
            </button>

            {(
              <div className={`absolute bottom-0 left-full z-50 ml-0 max-h-[calc(100vh-2rem)] w-72 overflow-y-auto rounded-lg border border-gray-200 bg-white text-gray-800 shadow-2xl animate-in fade-in slide-in-from-left-2 ${isSettingsOpen ? 'block' : 'hidden'} group-hover:block`}>
                <div className="p-3 border-b bg-gray-50 flex items-center text-[12px] font-bold text-gray-700 uppercase">
                  <Settings className="h-4 w-4 mr-2 text-blue-600" /> Cấu hình chủ quán
                </div>
                <div className="p-1">
                   {[
                     { icon: <Settings className="h-4 w-4" />, label: "Nhà hàng & tài chính", path: '/settings' },
                     { icon: <Printer className="h-4 w-4" />, label: "Hóa đơn & in ấn", path: '/settings/receipt' },
                     { icon: <Store className="h-4 w-4" />, label: "Quản lý chi nhánh", path: '/branches' },
                   ].map((item, i) => (
                     <Link
                       key={i}
                       to={item.path}
                       onClick={() => setIsSettingsOpen(false)}
                       className="w-full flex items-center justify-between px-3 py-2 hover:bg-blue-50 rounded group transition-colors"
                     >
                       <div className="flex items-center text-[13px] text-gray-600 group-hover:text-blue-700 font-bold uppercase">
                         <span className="mr-3 text-gray-400 group-hover:text-blue-500">{item.icon}</span> {item.label}
                       </div>
                       <ChevronRight className="h-3 w-3 text-gray-300 group-hover:text-blue-400" />
                     </Link>
                   ))}
                   <div className="mt-1 border-t border-gray-100 px-3 py-2 text-[10px] font-black uppercase tracking-widest text-gray-400">Sắp ra mắt</div>
                   <div className="flex items-center justify-between px-3 py-2 text-[13px] font-bold uppercase text-gray-400">
                     <span className="flex items-center"><ShieldCheck className="mr-3 h-4 w-4" /> Bảo mật & hệ thống</span>
                     <span className="text-[9px]">Sắp ra mắt</span>
                   </div>
                   <div className="flex items-center justify-between px-3 py-2 text-[13px] font-bold uppercase text-gray-400">
                     <span className="flex items-center"><Settings2 className="mr-3 h-4 w-4" /> Chốt sổ & khóa dữ liệu</span>
                     <span className="text-[9px]">Sắp ra mắt</span>
                   </div>
                </div>

                <div className="p-1 border-t bg-gray-50 text-center">
                   <Link
                     to="/settings"
                     onClick={() => setIsSettingsOpen(false)}
                     className="text-[11px] font-black text-blue-600 py-1.5 uppercase w-full hover:bg-blue-100 transition-colors block"
                   >
                     Xem tất cả thiết lập
                   </Link>
                </div>
              </div>
            )}
          </div>}

          {/* Help & Support */}
          <div className="group relative">
            <button
              onClick={() => {
                setIsHelpOpen(!isHelpOpen);
                setIsNotificationsOpen(false);
                setIsSettingsOpen(false);
                setIsUserMenuOpen(false);
              }}
              className={`flex items-center gap-2 rounded-xl px-3 py-2 text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 lg:w-full ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${isHelpOpen ? 'bg-blue-700' : ''}`}
              title="Hướng dẫn"
            >
              <HelpCircle size={18} /><span className={sidebarCollapsed ? 'lg:hidden' : ''}>Hỗ trợ</span>
            </button>

            {(
              <div className={`absolute bottom-0 left-full z-50 ml-0 max-h-[calc(100vh-2rem)] w-64 overflow-y-auto rounded-lg border border-gray-200 bg-white text-gray-800 shadow-2xl animate-in fade-in slide-in-from-left-2 ${isHelpOpen ? 'block' : 'hidden'} group-hover:block`}>
                <div className="p-3 border-b bg-gray-50 flex items-center text-sm font-bold text-gray-700 uppercase tracking-tighter italic">
                   <HelpCircle className="h-4 w-4 mr-2 text-blue-600" /> Hỗ trợ kỹ thuật
                </div>
                <div className="p-2 space-y-1">
                   <Link to="/support" onClick={() => setIsHelpOpen(false)} className="flex items-center px-3 py-2 text-xs font-bold text-gray-600 hover:bg-blue-50 rounded transition-all">
                      <Book size={14} className="mr-3 text-gray-400"/> Tài liệu hướng dẫn
                   </Link>
                   <Link to="/support" onClick={() => setIsHelpOpen(false)} className="flex items-center px-3 py-2 text-xs font-bold text-gray-600 hover:bg-blue-50 rounded transition-all">
                      <Info size={14} className="mr-3 text-gray-400"/> Câu hỏi thường gặp
                   </Link>
                   <div className="p-3 bg-blue-50 rounded-xl mt-2 border border-blue-100">
                      <p className="text-[10px] font-black text-blue-700 uppercase tracking-widest mb-1">Hotline hỗ trợ 24/7</p>
                      <p className="text-sm font-black text-gray-800 tracking-tighter">1900.88.99.00</p>
                      <Link to="/support" onClick={() => setIsHelpOpen(false)} className="block w-full mt-2 py-2 bg-blue-600 text-white text-center rounded-lg text-[10px] font-black uppercase tracking-widest hover:bg-blue-700 shadow-md shadow-blue-500/20">
                         Mở trang hỗ trợ
                      </Link>
                   </div>
                </div>
              </div>
            )}
          </div>

          {/* QR Code for management and staff login */}
          <div className="group relative">
            <button
              onClick={() => setIsQRModalOpen(open => !open)}
              className={`flex items-center gap-2 rounded-xl px-3 py-2 text-[12px] font-bold uppercase tracking-widest text-blue-100 transition-colors hover:bg-blue-600 lg:w-full ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''}`}
              title="Mã QR đăng nhập quản lý và nhân viên"
            >
              <QrCode size={18} /><span className={sidebarCollapsed ? 'lg:hidden' : ''}>Mã QR</span>
            </button>
            {(
              <div className={`absolute bottom-0 left-full z-50 ml-0 max-h-[calc(100vh-2rem)] w-80 overflow-y-auto rounded-2xl border border-gray-200 bg-white p-6 text-center text-gray-800 shadow-2xl animate-in fade-in slide-in-from-left-2 ${isQRModalOpen ? 'block' : 'hidden'} group-hover:block`}>
                <button onClick={() => setIsQRModalOpen(false)} className="absolute right-3 top-3 text-gray-400 hover:text-gray-600" aria-label="Đóng mã QR"><X size={20} /></button>
                <h3 className="mb-4 flex items-center justify-center text-sm font-black uppercase tracking-tight"><QrCode className="mr-2 text-blue-600" size={20}/>QR ĐĂNG NHẬP</h3>
                <div className="inline-block rounded-xl border-2 border-dashed border-blue-200 bg-white p-4">
                  <img src={`https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(ADMIN_LOGIN_URL)}`} alt="QR Code" className="mx-auto h-48 w-48" />
                </div>
                <p className="mt-4 text-[10px] font-bold uppercase italic tracking-widest leading-tight text-gray-500">Dùng điện thoại quét mã<br/>để mở trang đăng nhập hệ thống</p>
                <div className="mt-3 rounded-lg border border-gray-100 bg-gray-50 px-3 py-2"><p className="mb-1 text-[9px] font-black uppercase text-gray-400">ĐỊA CHỈ ĐĂNG NHẬP</p><p className="break-all font-mono text-xs text-blue-600">{ADMIN_LOGIN_URL}</p></div>
              </div>
            )}
          </div>

          <div className="absolute inset-x-4 bottom-0 flex flex-col items-stretch gap-1 border-t border-blue-400/30 bg-[#0070f4] pt-3">
          {/* User Profile & Dropdown */}
          <div className="group relative">
            <button
              onClick={() => {
                setIsUserMenuOpen(!isUserMenuOpen);
                setIsSettingsOpen(false);
                setIsNotificationsOpen(false);
                setIsHelpOpen(false);
              }}
              className={`flex items-center gap-2 rounded-xl px-3 py-2 transition-colors hover:bg-blue-600 lg:w-full lg:justify-start ${sidebarCollapsed ? 'lg:justify-center lg:px-2' : ''} ${isUserMenuOpen || location.pathname === '/profile' ? 'bg-blue-700' : ''}`}
            >
              <div className="w-7 h-7 bg-blue-300 rounded-full flex items-center justify-center text-blue-700">
                <User size={16} />
              </div>
              <span className={`hidden max-w-[132px] truncate text-[12px] font-bold uppercase tracking-widest sm:block ${sidebarCollapsed ? 'lg:hidden' : ''}`} title={userName || 'Admin'}>{userName || 'Quản trị viên'}</span>
            </button>

            {(
              <div className={`absolute bottom-0 left-full z-50 ml-0 max-h-[calc(100vh-2rem)] w-64 overflow-y-auto rounded-lg border border-gray-200 bg-white text-gray-800 shadow-2xl animate-in fade-in slide-in-from-left-2 ${isUserMenuOpen ? 'block' : 'hidden'} group-hover:block`}>
                <div className="p-4 border-b flex items-center bg-gray-50">
                  <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center mr-3">
                    <User className="h-6 w-6 text-blue-500" />
                  </div>
                  <div>
                    <p className="text-sm font-bold">{userName || 'Quản trị viên'}</p>
                    <p className="text-[10px] text-orange-500 font-medium">Xác thực 2 lớp — Sắp ra mắt</p>
                  </div>
                </div>

                <div className="p-1">
                  <Link to="/profile" onClick={() => setIsUserMenuOpen(false)} className="w-full flex items-center justify-between px-3 py-2 text-sm text-gray-600 hover:bg-blue-50 rounded group">
                    <span className="flex items-center"><Store className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" /> Hồ sơ cửa hàng</span>
                    <ChevronRight className="h-4 w-4 text-gray-300" />
                  </Link>
                  {userRole === 'admin' && (
                    <Link to="/branches" onClick={() => setIsUserMenuOpen(false)} className="w-full flex items-center justify-between px-3 py-2 text-sm text-gray-600 hover:bg-blue-50 rounded group">
                      <span className="flex items-center"><Users className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" /> Chi nhánh</span>
                      <ChevronRight className="h-4 w-4 text-gray-300" />
                    </Link>
                  )}
                </div>

                <div className="p-1 border-t">
                  <button
                    type="button"
                    onClick={() => setIsLanguageMenuOpen(open => !open)}
                    className="w-full flex items-center justify-between px-3 py-2 text-sm text-gray-600 hover:bg-blue-50 rounded"
                  >
                    <span className="flex items-center"><Globe className="h-4 w-4 mr-3 text-gray-400" /> Ngôn ngữ</span>
                    <span className="text-[10px] font-bold text-gray-400 flex items-center uppercase">Tiếng Việt <ChevronRight className={`h-3 w-3 ml-1 transition-transform ${isLanguageMenuOpen ? 'rotate-90' : ''}`} /></span>
                  </button>
                  {isLanguageMenuOpen && (
                    <div className="mx-3 mb-1 rounded-lg bg-gray-50 p-1 text-xs">
                      <div className="flex items-center justify-between rounded px-2 py-1.5 text-gray-700">
                        <span>Tiếng Việt</span>
                        <span className="font-bold text-blue-600">Đang dùng</span>
                      </div>
                      <div className="flex items-center justify-between rounded px-2 py-1.5 text-gray-400">
                        <span>English</span>
                        <span className="font-bold uppercase">Sắp ra mắt</span>
                      </div>
                    </div>
                  )}
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
      </div>
      </div>

    </nav>
  );
};

export default Navbar;
