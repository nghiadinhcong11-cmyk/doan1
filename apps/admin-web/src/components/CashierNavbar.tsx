import React, { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { Link, useLocation } from 'react-router-dom';
import { Utensils, User, Camera, Calendar, LogOut, ChevronDown, Store, ShieldCheck, Clock, ChevronRight, Bell, BellRing, Loader2, Receipt } from 'lucide-react';
import { API_URL } from '../config';

interface CashierNavbarProps {
  onLogout: () => void;
  userName?: string;
  userPosition?: string;
}

const CashierNavbar: React.FC<CashierNavbarProps> = ({ onLogout, userName, userPosition }) => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isNotificationsOpen, setIsNotificationsOpen] = useState(false);
  const [notifications, setNotifications] = useState<any[]>([]);
  const [loadingNoti, setLoadingNoti] = useState(false);
  const location = useLocation();
  const branchName = localStorage.getItem('selectedBranchName') || 'Toàn hệ thống';
  const branchId = localStorage.getItem('selectedBranchId');

  useEffect(() => {
    fetchPersistentNotifications();
    const interval = setInterval(fetchPersistentNotifications, 30000); // 30s cập nhật 1 lần
    const connection = new signalR.HubConnectionBuilder().withUrl(`${API_URL}/kitchenHub`, { accessTokenFactory: () => localStorage.getItem('token') || '' }).withAutomaticReconnect().build();
    const onNotification = (notification: any) => setNotifications(prev => [notification, ...prev.filter(item => item.id !== notification.id)].slice(0, 100));
    connection.on('NotificationCreated', onNotification);
    void connection.start().catch(() => undefined);

    // Lắng nghe sự kiện yêu cầu cập nhật thông báo ngay lập tức
    const handleRefresh = () => fetchNotifications();
    window.addEventListener('refresh-notifications', handleRefresh);

    return () => {
      clearInterval(interval);
      connection.off('NotificationCreated', onNotification);
      void connection.stop();
      window.removeEventListener('refresh-notifications', handleRefresh);
    };
  }, []);

  const fetchNotifications = async () => {
    try {
      setLoadingNoti(true);
      // Chỉ lấy đơn hàng/lịch hẹn của chi nhánh hiện tại mà nhân viên đang trực
      const branchParam = branchId ? `branchId=${branchId}` : '';

      const orderRes = await fetch(`${API_URL}/api/Order?status=Đang xử lý&${branchParam}`);
      const orders = await orderRes.json();

      const resvRes = await fetch(`${API_URL}/api/Reservation?status=Pending&${branchParam}`);
      const reservations = await resvRes.json();

      // Chỉ lấy đơn từ Web (không có createdBy)
      const webOrdersOnly = Array.isArray(orders) ? orders.filter((o: any) => !o.createdBy || o.createdBy === '') : [];

      const orderNotis = webOrdersOnly.map((o: any) => ({
        id: o.id,
        title: 'Đơn hàng mới',
        desc: `Bàn ${o.tableName || 'vãng lai'} vừa đặt món: ${o.totalAmount.toLocaleString()}đ`,
        time: new Date(o.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
        type: 'order',
        tableName: o.tableName || 'vãng lai',
        link: `/pos?reviewOrderId=${o.id}`
      }));

      const resvNotis = reservations.map((r: any) => ({
        id: r.id,
        title: 'Lịch hẹn mới',
        desc: `Khách ${r.customerName} đặt bàn vào ${new Date(r.reservationTime).toLocaleString('vi-VN', {hour:'2-digit', minute:'2-digit'})}`,
        time: new Date(r.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }),
        type: 'reservation',
        link: '/pos/reservations'
      }));

      const combined = [...orderNotis, ...resvNotis].sort((a, b) => b.id.localeCompare(a.id));
      setNotifications(combined);
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

  const navItems = [
    { path: '/pos', label: 'Màn hình bán hàng', icon: <Utensils size={16} className="text-blue-500" /> },
    { path: '/pos/invoices', label: 'Lịch sử hóa đơn', icon: <Receipt size={16} className="text-emerald-500" /> },
    ...(userPosition === 'Quản lý' ? [{ path: '/pos/settings/receipt', label: 'Cấu hình in bill', icon: <Receipt size={16} className="text-purple-500" /> }] : []),
    { path: '/pos/attendance', label: 'Chấm công nhận diện', icon: <Camera size={16} className="text-orange-500" /> },
    { path: '/pos/reservations', label: 'Lịch đặt bàn trước', icon: <Calendar size={16} className="text-blue-500" /> },
    { path: '/pos/schedule', label: 'Lịch làm việc toàn quán', icon: <Calendar size={16} className="text-green-500" /> },
    { path: '/pos/shifts', label: 'Lịch sử ca làm việc', icon: <Clock size={16} className="text-blue-500" /> },
    { path: '/pos/profile', label: 'Hồ sơ cá nhân', icon: <User size={16} className="text-purple-500" /> },
  ];

  return (
    <nav className="bg-[#0070f4] text-white h-12 flex items-center justify-between px-4 shadow-md z-[200] relative border-b border-blue-700">
       {/* Brand Logo */}
       <Link to="/pos" className="flex items-center font-bold text-lg italic tracking-tighter cursor-pointer">
          <div className="bg-white text-[#0070f4] rounded-full w-6 h-6 flex items-center justify-center mr-2 not-italic text-sm">D</div>
          DOAN POS
       </Link>

       <div className="flex-1"></div>

       {/* Quick Status */}
       <div className="hidden md:flex items-center bg-white/10 px-3 py-1 rounded-full text-[10px] font-bold space-x-4 border border-white/10">
          <div className="flex items-center"><Clock size={12} className="mr-1 opacity-60"/> {new Date().toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})}</div>
          <div className="flex items-center"><Store size={12} className="mr-1 opacity-60"/> {branchName}</div>
       </div>

       {/* Right Section: Notifications + User */}
       <div className="flex items-center space-x-4">
          {/* Notifications */}
          <div className="relative">
            <button
              onClick={() => {
                setIsNotificationsOpen(!isNotificationsOpen);
                setIsMenuOpen(false);
              }}
              className={`p-2 hover:bg-blue-600 rounded-xl transition-colors relative ${isNotificationsOpen ? 'bg-blue-700' : ''}`}
            >
              <Bell size={20} />
              {notifications.filter(n => !n.isRead).length > 0 && (
                <span className="absolute top-1.5 right-1.5 w-4 h-4 bg-red-500 rounded-full border-2 border-[#0070f4] text-[9px] flex items-center justify-center font-black animate-pulse">
                  {notifications.filter(n => !n.isRead).length}
                </span>
              )}
            </button>

            {isNotificationsOpen && (
              <div className="absolute right-0 mt-2 w-80 bg-white rounded-2xl shadow-2xl border border-gray-100 z-50 overflow-hidden text-gray-800 animate-in fade-in slide-in-from-top-2 duration-200">
                <div className="p-4 border-b bg-gray-50 flex items-center justify-between">
                  <div className="flex items-center font-black text-sm text-gray-700">
                    <BellRing className="h-4 w-4 mr-2 text-blue-600" /> THÔNG BÁO MỚI
                  </div>
                  {notifications.filter(n => !n.isRead).length > 0 && (
                    <span className="text-[10px] bg-blue-100 text-blue-600 px-2 py-0.5 rounded-full font-black uppercase">{notifications.filter(n => !n.isRead).length} Mới</span>
                  )}
                </div>

                <div className="max-h-[400px] overflow-y-auto no-scrollbar">
                   {loadingNoti && notifications.length === 0 ? (
                     <div className="py-12 text-center"><Loader2 className="animate-spin mx-auto text-blue-600" /></div>
                   ) : notifications.length === 0 ? (
                     <div className="py-12 text-center">
                        <p className="text-gray-400 text-xs font-bold uppercase tracking-widest italic">Tuyệt vời! Không có yêu cầu chờ</p>
                     </div>
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
                           } else if (n.type === 'reservation') {
                              // Chuyển đến trang đặt bàn nếu là lịch hẹn
                           }
                         }}
                         className="p-4 border-b border-gray-50 hover:bg-blue-50/50 cursor-pointer transition-all group block"
                       >
                          <div className="flex justify-between items-start mb-1">
                             <p className="text-xs font-black text-gray-800 group-hover:text-blue-700 transition-colors uppercase tracking-tight">{n.title}</p>
                             <span className="text-[9px] text-gray-400 font-bold bg-gray-100 px-1.5 py-0.5 rounded">{n.time || new Date(n.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</span>
                          </div>
                          <p className="text-[11px] text-gray-500 font-medium leading-tight">{n.message || n.desc}</p>
                       </Link>
                     ))
                   )}
                </div>
                <div className="p-3 text-center bg-gray-50 border-t">
                   <button onClick={() => setIsNotificationsOpen(false)} className="text-[10px] font-black text-blue-600 uppercase tracking-widest hover:underline">Đóng thông báo</button>
                </div>
              </div>
            )}
          </div>

          <div className="h-6 w-[1px] bg-white/20 mx-1"></div>

          {/* User Dropdown Section */}
          <div className="relative">
             <button
               onClick={() => {
                 setIsMenuOpen(!isMenuOpen);
                 setIsNotificationsOpen(false);
               }}
               className={`flex items-center space-x-2 px-3 py-1.5 rounded-lg transition-all ${isMenuOpen ? 'bg-blue-700' : 'hover:bg-blue-600'}`}
             >
                <div className="w-7 h-7 bg-blue-300 rounded-full flex items-center justify-center text-blue-700 font-black text-xs border border-white/20 shadow-inner">
                   {(userName?.charAt(0) || 'U').toUpperCase()}
                </div>
                <div className="text-left hidden sm:block">
                   <p className="text-[10px] font-bold text-blue-200 uppercase leading-none mb-0.5">Thu ngân</p>
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
                       <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center text-blue-600 mr-3">
                          <User size={20} />
                       </div>
                       <div>
                          <p className="font-black text-sm text-gray-800">{userName}</p>
                          <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest">Nhân viên chính thức</p>
                       </div>
                    </div>

                    <div className="p-2">
                       {navItems.map((item) => (
                         <Link
                           key={item.path}
                           to={item.path}
                           onClick={() => setIsMenuOpen(false)}
                           className={`flex items-center justify-between px-4 py-3 rounded-xl transition-all group ${
                             location.pathname === item.path ? 'bg-blue-50 text-blue-600' : 'hover:bg-gray-50 text-gray-600'
                           }`}
                         >
                            <div className="flex items-center">
                               <span className="mr-3">{item.icon}</span>
                               <span className="text-sm font-bold">{item.label}</span>
                            </div>
                            <ChevronRight size={14} className="opacity-0 group-hover:opacity-100 -translate-x-2 group-hover:translate-x-0 transition-all text-blue-400" />
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
       </div>
    </nav>
  );
};

export default CashierNavbar;
