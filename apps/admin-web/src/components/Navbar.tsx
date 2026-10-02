import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { Link, useLocation } from 'react-router-dom';
import { Bell, Settings, User, Search, HelpCircle, ChevronRight, Globe, LogOut, Printer, Users, Store, ShieldCheck, CreditCard, BellRing, Users2, ChevronDown, Clock, Calendar, Info, Loader2, Book, Banknote, Utensils, Settings2, QrCode, X } from 'lucide-react';
import { API_URL, CUSTOMER_WEB_URL } from '../config';
import { fetchPendingWebOrderCount, PENDING_ORDER_CHANGED_EVENT } from '../features/notifications/pendingOrderQuery';

interface Branch {
  id: string;
  name: string;
  isMain?: boolean;
}

interface NavbarProps {
  onLogout: () => void;
  userName?: string;
  userRole?: 'admin' | 'manager' | 'cashier' | null;
}

const Navbar: React.FC<NavbarProps> = ({ onLogout, userName, userRole }) => {
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);
  const [isSettingsOpen, setIsSettingsOpen] = useState(false);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [isHelpOpen, setIsHelpOpen] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isQRModalOpen, setIsQRModalOpen] = useState(false);
  const [notifications, setNotifications] = useState<any[]>([]);
  const [pendingOrderCount, setPendingOrderCount] = useState(0);
  const [loadingNoti, setLoadingNoti] = useState(false);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [selectedBranch, setSelectedBranch] = useState<Branch | null>(null);
  const [insightToast, setInsightToast] = useState<any | null>(null);
  const location = useLocation();

  const fetchBranches = async () => {
    try {
      const res = await fetch(`${API_URL}/api/Branch`);
      const data: Branch[] = await res.json();
      setBranches(data);

      const savedBranchId = localStorage.getItem('selectedBranchId');
      const branch = data.find(b => b.id === savedBranchId) || data.find(b => b.isMain) || data[0];
      if (branch) {
        setSelectedBranch(branch);
        localStorage.setItem('selectedBranchId', branch.id);
        void refreshPendingOrderCount();
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchBranches();
  }, []);

  const handleBranchChange = (branch: Branch) => {
    if (userRole === 'manager') return;
    setSelectedBranch(branch);
    localStorage.setItem('selectedBranchId', branch.id);
    // Trigger a page reload or event to update other components
    window.location.reload();
  };

  const isActive = (path: string) => location.pathname === path;

  const navItems = [
    { id: 'dashboard', label: 'TỔNG QUAN', path: '/dashboard' },
    { id: 'insights', label: 'CẢNH BÁO', path: '/business-insights' },
    { id: 'pos', label: 'BÁN HÀNG', path: '/pos' },
    { id: 'kitchen', label: 'NHÀ BẾP', path: '/kitchen' },
    { id: 'reservations', label: 'ĐẶT BÀN', path: '/reservations' },
    { id: 'tables', label: 'PHÒNG BÀN', path: '/tables' },
    { id: 'invoices', label: 'HÓA ĐƠN', path: '/invoices' },
    { id: 'expenses', label: 'CHI PHÍ', path: '/expenses' },
    ...(userRole === 'admin' || userRole === 'manager' ? [{ id: 'inventory', label: 'KHO HÀNG', path: '/inventory' }] : []),
  ];

  const fetchNotifications = async () => {
    try {
      setLoadingNoti(true);
      const branchId = localStorage.getItem('selectedBranchId');
      const branchParam = branchId ? `&branchId=${branchId}` : '';

      const res = await fetch(`${API_URL}/api/Order?status=Đang xử lý,Đổi quà${branchParam}`);
      if (res.ok) {
        const data = await res.json();
        // Chỉ lấy đơn từ Web (không có createdBy) hoặc đơn đổi quà
        const webOrders = Array.isArray(data) ? data.filter((o: any) => o.status === 'Đổi quà' || (!o.createdBy || o.createdBy === '')) : [];

        const formatted = webOrders.map((o: any) => ({
          id: o.id,
          title: o.status === 'Đổi quà' ? 'Yêu cầu đổi quà' : 'Đơn hàng mới',
          desc: o.status === 'Đổi quà'
            ? `Khách ${o.customerName || o.customerPhone} muốn đổi: ${o.details[0]?.productName || 'Quà tặng'}`
            : `Bàn ${o.tableName || 'vãng lai'} vừa đặt món: ${o.totalAmount.toLocaleString()}đ`,
          time: new Date(o.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
          type: o.status === 'Đổi quà' ? 'gift' : 'order',
          link: o.status === 'Đổi quà' ? '/customers' : `/pos?reviewOrderId=${o.id}`
        }));
        setNotifications(formatted);
      }
    } catch (err) {
      console.error("Lỗi lấy thông báo:", err);
    } finally {
      setLoadingNoti(false);
    }
  };

  const fetchPersistentNotifications = async () => {
    try {
      setLoadingNoti(true);
      const response = await fetch(`${API_URL}/api/Notification`);
      if (response.ok) setNotifications(await response.json());
    } catch (err) { console.error('Lỗi lấy notification persistent:', err); }
    finally { setLoadingNoti(false); }
  };

  const refreshPendingOrderCount = async () => {
    try {
      setPendingOrderCount(await fetchPendingWebOrderCount());
    } catch (err) {
      console.error('Lỗi lấy số đơn web đang chờ:', err);
    }
  };

  useEffect(() => {
    fetchPersistentNotifications();
    const interval = setInterval(fetchPersistentNotifications, 30000);
    refreshPendingOrderCount();
    const pendingInterval = setInterval(refreshPendingOrderCount, 30000);
    const connection = new signalR.HubConnectionBuilder().withUrl(`${API_URL}/kitchenHub`, { accessTokenFactory: () => localStorage.getItem('adminToken') || '' }).withAutomaticReconnect().build();
    const onNotification = (notification: any) => {
      setNotifications(prev => [notification, ...prev.filter(item => item.id !== notification.id)].slice(0, 100));

      // Special handling for Business Insights - Show Toast
      if (notification.type === 'BusinessInsight') {
        setInsightToast(notification);
        setTimeout(() => setInsightToast(null), 10000); // Auto hide after 10s

        // Notify BusinessInsights page if open
        window.dispatchEvent(new CustomEvent('business-insight-created', { detail: notification }));
      }
    };
    connection.on('NotificationCreated', onNotification);
    const onPendingOrderChanged = () => { void refreshPendingOrderCount(); };
    connection.on('PendingOrderChanged', onPendingOrderChanged);
    void connection.start().catch(() => undefined);

    // Lắng nghe sự kiện yêu cầu cập nhật thông báo ngay lập tức
    const handleRefresh = () => fetchPersistentNotifications();
    window.addEventListener('refresh-notifications', handleRefresh);
    window.addEventListener(PENDING_ORDER_CHANGED_EVENT, onPendingOrderChanged);

    return () => {
      clearInterval(interval);
      clearInterval(pendingInterval);
      connection.off('NotificationCreated', onNotification);
      connection.off('PendingOrderChanged', onPendingOrderChanged);
      void connection.stop();
      window.removeEventListener('refresh-notifications', handleRefresh);
      window.removeEventListener(PENDING_ORDER_CHANGED_EVENT, onPendingOrderChanged);
    };
  }, []);

  return (
    <nav className="bg-[#0070f4] text-white h-12 flex items-center justify-between px-4 shadow-md sticky top-0 z-50">
      <div className="flex items-center h-full">
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
        <Link to="/dashboard" className="flex items-center mr-4 font-bold text-lg italic tracking-tighter cursor-pointer shrink-0">
          <div className="bg-white text-[#0070f4] rounded-full w-6 h-6 flex items-center justify-center mr-1 not-italic text-sm font-black">D</div>
          <span className="hidden sm:inline">DOAN POS</span>
        </Link>

        {/* Branch Selector Global */}
        {branches.length > 0 && (
           <div className="relative group mr-4 h-full flex items-center">
              <button className="flex items-center bg-blue-600/50 hover:bg-blue-600 px-3 py-1.5 rounded-lg transition-all border border-blue-400/30">
                 <Store size={14} className="mr-2 text-blue-200" />
                 <div className="text-left hidden md:block">
                    <p className="text-[10px] font-black text-blue-200 uppercase leading-none mb-0.5">Cơ sở</p>
                    <p className="text-[11px] font-bold truncate max-w-[100px]">{selectedBranch?.name || 'Chọn chi nhánh'}</p>
                 </div>
                 <ChevronDown size={12} className="ml-2 text-blue-200 opacity-60 group-hover:rotate-180 transition-transform"/>
              </button>

              {/* Dropdown */}
              <div className="absolute top-full left-0 w-64 bg-white rounded-b-lg shadow-2xl border-t border-blue-500 hidden group-hover:block z-[60] overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-1">
                 <div className="p-3 border-b bg-gray-50">
                    <p className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Hệ thống chi nhánh</p>
                 </div>
                 <div className="max-h-80 overflow-y-auto">
                    {(userRole === 'manager' ? branches.filter(b => b.id === selectedBranch?.id) : branches).map(b => (
                       <button
                          key={b.id}
                          onClick={() => handleBranchChange(b)}
                          className={`w-full text-left px-4 py-3 hover:bg-blue-50 flex items-center justify-between border-b border-gray-50 last:border-0 transition-colors ${selectedBranch?.id === b.id ? 'bg-blue-50/80' : ''}`}
                       >
                          <div>
                             <p className={`text-[13px] font-bold ${selectedBranch?.id === b.id ? 'text-blue-600' : 'text-gray-700'}`}>{b.name}</p>
                             {b.isMain && <span className="text-[8px] bg-orange-100 text-orange-600 px-1 rounded-sm font-black uppercase">Trụ sở chính</span>}
                          </div>
                          {selectedBranch?.id === b.id && <ShieldCheck size={16} className="text-blue-600" />}
                       </button>
                    ))}
                 </div>
                 {userRole === 'admin' && (
                   <Link to="/branches" className="block p-2 text-center text-[10px] text-blue-600 font-bold hover:bg-gray-50 transition-colors uppercase border-t">Quản lý cơ sở</Link>
                 )}
              </div>
           </div>
        )}

        {/* Desktop Nav Links */}
        <div className="hidden lg:flex h-full items-center space-x-1">
          {navItems.map((item) => (
            <Link
              key={item.id}
              to={item.path}
              className={`px-3 h-full flex items-center text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${
                isActive(item.path) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              {item.label}
            </Link>
          ))}

          {/* THỰC ĐƠN DROPDOWN */}
          <div className="relative group h-full">
            <button
              className={`px-3 h-full flex items-center text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${
                ['/products', '/toppings'].includes(location.pathname) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              Thực đơn <ChevronDown size={14} className="ml-1 opacity-60 group-hover:rotate-180 transition-transform"/>
            </button>

            {/* Dropdown Menu */}
            <div className="absolute top-full left-0 w-56 bg-white rounded-b-lg shadow-2xl border-t border-blue-500 hidden group-hover:block z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-1">
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
          <div className="relative group h-full">
            <button
              className={`px-3 h-full flex items-center text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${
                ['/customers', '/promotions'].includes(location.pathname) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              Khách hàng <ChevronDown size={14} className="ml-1 opacity-60 group-hover:rotate-180 transition-transform"/>
            </button>

            {/* Dropdown Menu */}
            <div className="absolute top-full left-0 w-56 bg-white rounded-b-lg shadow-2xl border-t border-blue-500 hidden group-hover:block z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-1">
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
          <div className="relative group h-full">
            <button
              className={`px-3 h-full flex items-center text-[12px] font-bold uppercase tracking-widest transition-colors hover:bg-blue-600 ${
                ['/employees', '/attendance', '/schedule'].includes(location.pathname) ? 'bg-blue-700 shadow-inner' : ''
              }`}
            >
              Nhân viên <ChevronDown size={14} className="ml-1 opacity-60 group-hover:rotate-180 transition-transform"/>
            </button>

            {/* Dropdown Menu */}
            <div className="absolute top-full left-0 w-56 bg-white rounded-b-lg shadow-2xl border-t border-blue-500 hidden group-hover:block z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-1">
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
               <Link to="/shifts" className="flex items-center px-4 py-3 hover:bg-blue-50 transition-colors border-t border-gray-50">
                  <Banknote size={16} className="mr-3 text-blue-600"/>
                  <div>
                     <p className="text-[13px] font-bold text-gray-700 uppercase tracking-tight">Lịch sử chốt ca</p>
                     <p className="text-[10px] text-gray-400 font-medium uppercase">Đối soát tiền mặt, bàn giao</p>
                  </div>
               </Link>
            </div>
          </div>
        </div>
      </div>

      {/* Mobile Menu Overlay */}
      {isMobileMenuOpen && (
        <div className="lg:hidden fixed inset-0 top-12 bg-blue-700 z-[100] animate-in slide-in-from-left duration-300">
           <div className="p-6 space-y-6">
              {navItems.map((item) => (
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
                 <Link to="/reservations" onClick={() => setIsMobileMenuOpen(false)} className="block text-blue-100 font-black uppercase italic tracking-tighter">Đặt bàn</Link>
                 <Link to="/customers" onClick={() => setIsMobileMenuOpen(false)} className="block text-blue-100 font-black uppercase italic tracking-tighter">Khách hàng</Link>
                 <Link to="/promotions" onClick={() => setIsMobileMenuOpen(false)} className="block text-blue-100 font-black uppercase italic tracking-tighter">Khuyến mãi</Link>
                 <Link to="/employees" onClick={() => setIsMobileMenuOpen(false)} className="block text-blue-100 font-black uppercase italic tracking-tighter">Nhân viên</Link>
                 <Link to="/attendance" onClick={() => setIsMobileMenuOpen(false)} className="block text-blue-100 font-black uppercase italic tracking-tighter">Chấm công</Link>
                 <button onClick={onLogout} className="text-red-300 font-black uppercase italic tracking-tighter">Đăng xuất</button>
              </div>
           </div>
        </div>
      )}

      <div className="flex items-center space-x-2 sm:space-x-4">
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
          {/* Notifications */}
          <div className="relative">
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
                  {notifications.filter(n => !n.isRead).length > 0 && (
                    <span className="text-[10px] bg-blue-100 text-blue-600 px-1.5 py-0.5 rounded uppercase">{notifications.filter(n => !n.isRead).length} Mới</span>
                  )}
                </div>
                <div className="max-h-96 overflow-y-auto">
                   {loadingNoti && notifications.length === 0 ? (
                     <div className="py-10 text-center"><Loader2 className="animate-spin mx-auto text-blue-600" /></div>
                   ) : notifications.length === 0 ? (
                     <div className="py-10 text-center text-gray-400 text-xs italic uppercase">Không có thông báo mới</div>
                   ) : (
                     notifications.map((n) => (
                       <Link
                         key={n.id}
                         to={n.route || n.link || '#'}
                         onClick={() => {
                           setIsNotificationsOpen(false);
                           if (!n.isRead) void fetch(`${API_URL}/api/Notification/${n.id}/read`, { method: 'PATCH' });
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
          <div className="relative">
            <button
              onClick={() => {
                setIsSettingsOpen(!isSettingsOpen);
                setIsNotificationsOpen(false);
                setIsHelpOpen(false);
                setIsUserMenuOpen(false);
              }}
              className={`p-1 hover:bg-blue-600 rounded transition-colors ${isSettingsOpen || location.pathname === '/settings' ? 'bg-blue-700' : ''}`}
              title="Thiết lập"
            >
              <Settings size={18} />
            </button>

            {isSettingsOpen && (
              <div className="absolute right-0 mt-2 w-72 bg-white rounded-lg shadow-2xl border border-gray-200 z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-2">
                <div className="p-3 border-b bg-gray-50 flex items-center text-[12px] font-bold text-gray-700 uppercase">
                  <Settings className="h-4 w-4 mr-2 text-blue-600" /> Thiết lập hệ thống
                </div>
                <div className="p-1">
                   {[
                     { icon: <Settings className="h-4 w-4" />, label: "Thiết lập tính năng", path: '/settings' },
                     { icon: <Printer className="h-4 w-4" />, label: "Quản lý mẫu in", path: '/print-templates' },
                     { icon: <Printer className="h-4 w-4" />, label: "Cấu hình in bill", path: '/settings/receipt' },
                     userRole === 'admin' && { icon: <Store className="h-4 w-4" />, label: "Quản lý chi nhánh", path: '/branches' },
                     { icon: <Users className="h-4 w-4" />, label: "Quản lý nhân viên", path: '/employees' },
                   ].filter(Boolean).map((item: any, i) => (
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
          </div>

          {/* Help & Support */}
          <div className="relative">
            <button
              onClick={() => {
                setIsHelpOpen(!isHelpOpen);
                setIsNotificationsOpen(false);
                setIsSettingsOpen(false);
                setIsUserMenuOpen(false);
              }}
              className={`p-1 hover:bg-blue-600 rounded transition-colors ${isHelpOpen ? 'bg-blue-700' : ''}`}
              title="Hướng dẫn"
            >
              <HelpCircle size={18} />
            </button>

            {isHelpOpen && (
              <div className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-2xl border border-gray-200 z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-2">
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

          <div className="h-6 w-[1px] bg-blue-400 mx-1"></div>

          {/* QR Code for Customer */}
          <button
            onClick={() => setIsQRModalOpen(true)}
            className="p-1 hover:bg-blue-600 rounded transition-colors text-white"
            title="Mã QR cho khách"
          >
            <QrCode size={18} />
          </button>

          {/* User Profile & Dropdown */}
          <div className="relative">
            <button
              onClick={() => {
                setIsUserMenuOpen(!isUserMenuOpen);
                setIsSettingsOpen(false);
                setIsNotificationsOpen(false);
                setIsHelpOpen(false);
              }}
              className={`flex items-center space-x-2 hover:bg-blue-600 px-2 py-1 rounded transition-colors ${isUserMenuOpen || location.pathname === '/profile' ? 'bg-blue-700' : ''}`}
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

      {/* QR Modal */}
      {isQRModalOpen && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-[100] backdrop-blur-sm p-4 text-left">
          <div className="bg-white p-6 rounded-2xl shadow-2xl w-full max-w-xs text-center relative animate-in zoom-in duration-200">
            <button
              onClick={() => setIsQRModalOpen(false)}
              className="absolute top-3 right-3 text-gray-400 hover:text-gray-600 transition-colors"
            >
              <X size={20} />
            </button>
            <h3 className="text-gray-800 font-black uppercase mb-4 tracking-tighter flex items-center justify-center">
              <QrCode className="mr-2 text-blue-600" size={20}/>
              QR ĐẶT MÓN TẠI BÀN
            </h3>
            <div className="bg-white p-4 border-2 border-dashed border-blue-200 rounded-xl inline-block">
              <img
                src={`https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(CUSTOMER_WEB_URL)}`}
                alt="QR Code"
                className="w-48 h-48 mx-auto"
              />
            </div>
            <p className="mt-4 text-[10px] text-gray-500 font-bold uppercase italic tracking-widest leading-tight">
              Dùng điện thoại quét mã<br/>để truy cập menu và đặt món
            </p>
            <div className="mt-3 py-2 px-3 bg-gray-50 rounded-lg border border-gray-100">
               <p className="text-[9px] text-gray-400 uppercase font-black mb-1">Địa chỉ nội bộ</p>
              <p className="text-xs font-mono text-blue-600 break-all">{CUSTOMER_WEB_URL}</p>
            </div>
          </div>
        </div>
      )}

      {/* BUSINESS INSIGHT TOAST */}
      {insightToast && (
        <div className="fixed bottom-6 right-6 z-[200] bg-blue-900 text-white p-5 rounded-2xl shadow-2xl flex items-center space-x-4 animate-in slide-in-from-right-10 duration-500 max-w-sm border-l-4 border-blue-400">
           <div className="bg-blue-600 p-3 rounded-xl">
              <BellRing size={24} className="animate-pulse" />
           </div>
           <div className="flex-1 min-w-0">
              <p className="font-black uppercase text-[10px] text-blue-300 tracking-widest mb-1 italic">Cảnh báo kinh doanh mới</p>
              <p className="text-sm font-bold truncate uppercase">{insightToast.title}</p>
              <p className="text-[11px] opacity-70 line-clamp-2 italic">{insightToast.message}</p>
              <Link
                to="/business-insights"
                onClick={() => setInsightToast(null)}
                className="mt-3 inline-block bg-white text-blue-900 px-4 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-widest hover:bg-blue-50 transition-all shadow-sm"
              >
                 XEM NHẬN ĐỊNH AI
              </Link>
           </div>
           <button onClick={() => setInsightToast(null)} className="text-white/50 hover:text-white transition-colors self-start">
              <X size={16} />
           </button>
        </div>
      )}
    </nav>
  );
};

export default Navbar;
