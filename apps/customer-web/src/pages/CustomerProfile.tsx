import React, { useState, useEffect } from 'react';
import { User, LogOut, Package, Star, TrendingUp, ChevronRight, Clock, MapPin, Loader2, X, Edit3, Save, CheckCircle2, Lock, Calendar, Users, LayoutGrid } from 'lucide-react';
import { API_URL } from '../config';

const CustomerProfile = ({ onLogout }: { onLogout: () => void }) => {
  const [customer, setCustomer] = useState<any>(null);
  const [orders, setOrders] = useState<any[]>([]);
  const [reservations, setReservations] = useState<any[]>([]);
  const [loyaltyHistory, setLoyaltyHistory] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<any>(null);
  const [selectedReservation, setSelectedReservation] = useState<any>(null);

  // States cho việc mở rộng/thu hẹp danh sách
  const [isOrdersExpanded, setIsOrdersExpanded] = useState(false);
  const [isReservationsExpanded, setIsReservationsExpanded] = useState(false);
  const [isLoyaltyExpanded, setIsLoyaltyExpanded] = useState(false);
  const INITIAL_SHOW_COUNT = 3;

  // States cho cập nhật Profile
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editData, setEditData] = useState({
    fullName: '',
    email: '',
    address: '',
    gender: 'Nam',
    birthday: '',
    newPassword: ''
  });
  const [updating, setUpdating] = useState(false);
  const [showUpdateSuccess, setShowUpdateSuccess] = useState(false);

  useEffect(() => {
    const savedCustomer = localStorage.getItem('customerInfo');
    if (savedCustomer) {
      try {
        const data = JSON.parse(savedCustomer);
        setCustomer(data);
        setEditData({
          fullName: data.fullName || '',
          email: data.email || '',
          address: data.address || '',
          gender: data.gender || 'Nam',
          birthday: data.birthday ? new Date(data.birthday).toISOString().split('T')[0] : '',
          newPassword: ''
        });
        if (data.phoneNumber) {
          fetchLatestProfile(data.phoneNumber);
        }
      } catch (e) {
        console.error("Lỗi parse customerInfo:", e);
      }
    }
  }, []);

  const fetchLatestProfile = async (phone: string) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`${API_URL}/api/Customer/${phone}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (response.ok) {
        const data = await response.json();
        setCustomer(data);
        setEditData({
          fullName: data.fullName || '',
          email: data.email || '',
          address: data.address || '',
          gender: data.gender || 'Nam',
          birthday: data.birthday ? new Date(data.birthday).toISOString().split('T')[0] : '',
          newPassword: ''
        });
        localStorage.setItem('customerInfo', JSON.stringify(data));
        await fetchLoyaltyHistory(data.id);
      }
      await fetchOrderHistory(phone);
      await fetchReservationHistory(phone);
    } catch (err) {
      console.error("Lỗi tải hồ sơ:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async () => {
    if (!editData.fullName.trim()) return;

    setUpdating(true);
    try {
      const response = await fetch(`${API_URL}/api/Customer/${customer.phoneNumber}/profile`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          ...(localStorage.getItem('token') ? { Authorization: `Bearer ${localStorage.getItem('token')}` } : {})
        },
        body: JSON.stringify(editData)
      });

      if (response.ok) {
        const updatedCustomer = await response.json();
        setCustomer(updatedCustomer);
        localStorage.setItem('customerInfo', JSON.stringify(updatedCustomer));
        setIsEditModalOpen(false);
        setShowUpdateSuccess(true);
        setTimeout(() => setShowUpdateSuccess(false), 3000);
      }
    } catch (err) {
      alert("Lỗi cập nhật thông tin");
    } finally {
      setUpdating(false);
    }
  };

  const fetchOrderHistory = async (phone: string) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`${API_URL}/api/Order?customerPhone=${encodeURIComponent(phone)}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      const data = await response.json();
      setOrders(data);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchLoyaltyHistory = async (customerId: string) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`${API_URL}/api/Customer/${customerId}/loyalty-history`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (response.ok) {
        const data = await response.json();
        setLoyaltyHistory(data);
      }
    } catch (err) {
      console.error("Lỗi tải lịch sử tích điểm:", err);
    }
  };

  const fetchReservationHistory = async (phone: string) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`${API_URL}/api/Reservation?customerPhone=${encodeURIComponent(phone)}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (response.ok) {
        const data = await response.json();
        setReservations(data);
      }
    } catch (err) {
      console.error(err);
    }
  };

  if (loading && !customer) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50">
        <Loader2 className="animate-spin text-blue-600 mb-4" size={48} />
        <p className="text-xs font-black uppercase text-gray-400 tracking-widest">Đang tải hồ sơ...</p>
      </div>
    );
  }

  if (!customer || customer.isGuest) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 p-6 text-center">
        <div className="w-20 h-20 bg-gray-100 rounded-[2rem] flex items-center justify-center text-gray-300 mb-4">
          <User size={40} />
        </div>
        <p className="text-gray-500 font-black uppercase text-sm tracking-tighter">Bạn đang dùng quyền khách vãng lai</p>
        <p className="text-xs text-gray-400 mt-2 mb-8">Đăng nhập để xem lịch sử đơn hàng, tích điểm và nhận ưu đãi riêng nhé!</p>
        <button
          onClick={onLogout}
          className="px-8 py-3 bg-blue-600 text-white rounded-2xl font-black text-xs uppercase tracking-widest shadow-lg shadow-blue-500/20"
        >
          ĐĂNG NHẬP NGAY
        </button>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 pb-24 font-sans relative">
      {/* Toast Success */}
      {showUpdateSuccess && (
        <div className="fixed top-6 left-6 right-6 z-[300] bg-green-600 text-white p-4 rounded-2xl shadow-2xl flex items-center space-x-3 animate-in slide-in-from-top-10 duration-500">
           <CheckCircle2 size={24}/>
           <p className="font-black uppercase text-xs tracking-widest">Cập nhật thành công!</p>
        </div>
      )}

      {/* Profile Header */}
      <div className="bg-blue-600 text-white p-8 rounded-b-[3rem] shadow-2xl relative overflow-hidden">
         <div className="absolute -top-10 -right-10 w-40 h-40 bg-white/10 rounded-full blur-3xl"></div>

         <div className="flex justify-between items-start mb-8">
            <div className="flex items-center space-x-4">
                <div className="w-16 h-16 bg-white rounded-2xl flex items-center justify-center text-blue-600 shadow-lg">
                   <User size={32} />
                </div>
                <div>
                   <h1 className="text-xl font-black uppercase italic tracking-tighter">{customer.fullName}</h1>
                   <p className="text-xs opacity-70 font-bold">{customer.phoneNumber}</p>
                </div>
            </div>
            <button
              onClick={() => {
                setEditData({
                  fullName: customer.fullName || '',
                  email: customer.email || '',
                  address: customer.address || '',
                  gender: customer.gender || 'Nam',
                  birthday: customer.birthday ? new Date(customer.birthday).toISOString().split('T')[0] : '',
                  newPassword: ''
                });
                setIsEditModalOpen(true);
              }}
              className="bg-white/20 p-3 rounded-2xl hover:bg-white/30 transition-all border border-white/20 active:scale-90"
              title="Chỉnh sửa hồ sơ"
            >
               <Edit3 size={18} />
            </button>
         </div>

         <div className="grid grid-cols-2 gap-4">
            <div className="bg-white/20 backdrop-blur-md p-4 rounded-3xl border border-white/30">
               <p className="text-[10px] font-black uppercase tracking-widest opacity-70 mb-1">Điểm tích lũy</p>
               <div className="flex items-center space-x-2">
                  <Star size={16} className="fill-yellow-400 text-yellow-400" />
                  <span className="text-xl font-black">{customer.loyaltyPoints?.toLocaleString() || 0}</span>
               </div>
            </div>
            <div className="bg-white/20 backdrop-blur-md p-4 rounded-3xl border border-white/30">
               <p className="text-[10px] font-black uppercase tracking-widest opacity-70 mb-1">Hạng thành viên</p>
               <div className="flex items-center space-x-2">
                  <TrendingUp size={16} />
                  <span className="text-xl font-black italic">{customer.customerGroup || 'Mới'}</span>
               </div>
            </div>
         </div>
      </div>

      <div className="p-6 space-y-6">
         {/* Personal Info Summary */}
         <div className="bg-white p-6 rounded-[2rem] shadow-xl shadow-blue-500/5 border border-gray-100 space-y-4 relative overflow-hidden">
            <div className="flex justify-between items-center mb-2">
                <h2 className="text-xs font-black uppercase tracking-widest text-gray-400">Thông tin cá nhân</h2>
                <button
                    onClick={() => {
                        setEditData({
                            fullName: customer.fullName || '',
                            email: customer.email || '',
                            address: customer.address || '',
                            gender: customer.gender || 'Nam',
                            birthday: customer.birthday ? new Date(customer.birthday).toISOString().split('T')[0] : '',
                            newPassword: ''
                        });
                        setIsEditModalOpen(true);
                    }}
                    className="p-2 bg-blue-50 text-blue-600 rounded-xl hover:bg-blue-100 transition-colors"
                >
                    <Edit3 size={14} />
                </button>
            </div>
            <div className="grid grid-cols-2 gap-4">
               <div>
                  <p className="text-[9px] font-black text-gray-400 uppercase">Email</p>
                  <p className="text-xs font-bold text-gray-700 truncate">{customer.email || 'Chưa cập nhật'}</p>
               </div>
               <div>
                  <p className="text-[9px] font-black text-gray-400 uppercase">Ngày sinh</p>
                  <p className="text-xs font-bold text-gray-700">{customer.birthday ? new Date(customer.birthday).toLocaleDateString('vi-VN') : 'Chưa cập nhật'}</p>
               </div>
               <div className="col-span-2">
                  <p className="text-[9px] font-black text-gray-400 uppercase">Địa chỉ</p>
                  <p className="text-xs font-bold text-gray-700">{customer.address || 'Chưa cập nhật'}</p>
               </div>
            </div>
         </div>

         {/* Order History */}
         <div className="flex justify-between items-center">
            <h2 className="text-lg font-black uppercase italic tracking-tighter text-gray-800">Lịch sử đặt món</h2>
            <span className="text-[10px] font-bold text-gray-400 bg-gray-200/50 px-2 py-1 rounded uppercase tracking-widest">{orders.length} đơn</span>
         </div>
         {/* ... (phần render orders cũ) ... */}

         {loading ? (
            <div className="flex justify-center py-10"><Loader2 className="animate-spin text-blue-600" /></div>
         ) : orders.length === 0 ? (
            <div className="bg-white p-10 rounded-[2rem] text-center border-2 border-dashed border-gray-100">
               <Package className="mx-auto text-gray-200 mb-3" size={48} />
               <p className="text-sm font-bold text-gray-400">Bạn chưa có đơn hàng nào.</p>
            </div>
         ) : (
            <div className="space-y-4">
               {(isOrdersExpanded ? orders : orders.slice(0, INITIAL_SHOW_COUNT)).map((o) => (
                  <div
                    key={o.id}
                    onClick={() => setSelectedOrder(o)}
                    className="bg-white p-5 rounded-[2rem] shadow-xl shadow-blue-500/5 border border-gray-100 flex items-center justify-between group active:scale-95 transition-all cursor-pointer"
                  >
                     <div className="flex items-center space-x-4">
                        <div className="w-12 h-12 bg-blue-50 rounded-2xl flex items-center justify-center text-blue-600 font-black text-xs group-hover:bg-blue-600 group-hover:text-white transition-colors">
                           HD
                        </div>
                        <div>
                           <p className="text-sm font-black text-gray-800">{o.invoiceCode}</p>
                           <p className="text-[10px] text-gray-400 font-bold flex items-center">
                              <Clock size={10} className="mr-1" /> {new Date(o.createdAt).toLocaleDateString('vi-VN')}
                           </p>
                        </div>
                     </div>
                     <div className="text-right flex items-center space-x-3">
                        <div>
                           <p className="text-sm font-black text-blue-600 tracking-tighter">{o.totalAmount.toLocaleString()}đ</p>
                           <span className={`text-[9px] font-black uppercase px-2 py-0.5 rounded-full ${o.status === 'Hoàn thành' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'}`}>
                              {o.status}
                           </span>
                        </div>
                        <ChevronRight size={16} className="text-gray-300 group-hover:text-blue-500 transition-colors" />
                     </div>
                  </div>
               ))}

               {orders.length > INITIAL_SHOW_COUNT && (
                  <button
                     onClick={() => setIsOrdersExpanded(!isOrdersExpanded)}
                     className="w-full py-3 bg-white text-blue-600 rounded-2xl font-black text-[10px] uppercase tracking-widest border border-blue-50 shadow-sm active:scale-95 transition-all"
                  >
                     {isOrdersExpanded ? 'Thu nhỏ danh sách' : `Xem thêm ${orders.length - INITIAL_SHOW_COUNT} đơn hàng khác`}
                  </button>
               )}
            </div>
         )}

         {/* Loyalty History */}
         <div className="flex justify-between items-center pt-4">
            <h2 className="text-lg font-black uppercase italic tracking-tighter text-gray-800">Lịch sử tích điểm</h2>
            <span className="text-[10px] font-bold text-gray-400 bg-gray-200/50 px-2 py-1 rounded uppercase tracking-widest">{loyaltyHistory.length} giao dịch</span>
         </div>

         {loyaltyHistory.length === 0 ? (
            <div className="bg-white p-10 rounded-[2rem] text-center border-2 border-dashed border-gray-100">
               <Star className="mx-auto text-gray-200 mb-3" size={48} />
               <p className="text-sm font-bold text-gray-400">Bạn chưa có giao dịch điểm nào.</p>
            </div>
         ) : (
            <div className="space-y-4">
               {(isLoyaltyExpanded ? loyaltyHistory : loyaltyHistory.slice(0, INITIAL_SHOW_COUNT)).map((log) => (
                  <div
                    key={log.id}
                    className="bg-white p-5 rounded-[2rem] shadow-xl shadow-blue-500/5 border border-gray-100 flex items-center justify-between"
                  >
                     <div className="flex items-center space-x-4">
                        <div className={`w-12 h-12 rounded-2xl flex items-center justify-center font-black text-xs ${
                           (log.type === 'Earn' || log.type === 'Refund') ? 'bg-green-50 text-green-600' :
                           (log.type === 'Redeem' || log.type === 'Reversal') ? 'bg-red-50 text-red-600' :
                           'bg-gray-50 text-gray-600'
                        }`}>
                           {(log.type === 'Earn' || log.type === 'Refund') ? '+' : ((log.type === 'Redeem' || log.type === 'Reversal') ? '-' : '±')}
                        </div>
                        <div>
                           <p className="text-sm font-black text-gray-800">{log.description || (log.type === 'Earn' ? 'Tích điểm đơn hàng' : log.type === 'Redeem' ? 'Đổi quà' : 'Điều chỉnh điểm')}</p>
                           <p className="text-[10px] text-gray-400 font-bold flex items-center">
                              <Clock size={10} className="mr-1" /> {new Date(log.createdAt).toLocaleDateString('vi-VN')} {new Date(log.createdAt).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})}
                           </p>
                        </div>
                     </div>
                     <div className="text-right">
                        <p className={`text-sm font-black tracking-tighter ${
                           (log.type === 'Earn' || log.type === 'Refund') ? 'text-green-600' :
                           (log.type === 'Redeem' || log.type === 'Reversal') ? 'text-red-600' :
                           'text-gray-800'
                        }`}>
                           {log.points > 0 ? '+' : ''}{log.points.toLocaleString()}
                        </p>
                        <p className="text-[9px] font-black text-gray-400 uppercase">Dư: {log.balanceAfter.toLocaleString()}</p>
                     </div>
                  </div>
               ))}

               {loyaltyHistory.length > INITIAL_SHOW_COUNT && (
                  <button
                     onClick={() => setIsLoyaltyExpanded(!isLoyaltyExpanded)}
                     className="w-full py-3 bg-white text-blue-600 rounded-2xl font-black text-[10px] uppercase tracking-widest border border-blue-50 shadow-sm active:scale-95 transition-all"
                  >
                     {isLoyaltyExpanded ? 'Thu nhỏ danh sách' : `Xem thêm ${loyaltyHistory.length - INITIAL_SHOW_COUNT} giao dịch khác`}
                  </button>
               )}
            </div>
         )}

         {/* Reservations Section */}
         <div className="pt-4">
            <div className="flex justify-between items-center mb-4">
               <h2 className="text-lg font-black uppercase italic tracking-tighter text-gray-800">Lịch hẹn của tôi</h2>
               <span className="text-[10px] font-bold text-gray-400 bg-gray-200/50 px-2 py-1 rounded uppercase tracking-widest">{reservations.length} lịch</span>
            </div>

            {reservations.length === 0 ? (
               <div className="bg-white p-8 rounded-[2rem] text-center border-2 border-dashed border-gray-100">
                  <Calendar className="mx-auto text-gray-200 mb-2" size={32} />
                  <p className="text-xs font-bold text-gray-400">Bạn chưa có lịch hẹn nào.</p>
               </div>
            ) : (
               <div className="space-y-4">
                  {(isReservationsExpanded ? reservations : reservations.slice(0, INITIAL_SHOW_COUNT)).map((res: any) => (
                     <div
                        key={res.id}
                        onClick={() => setSelectedReservation(res)}
                        className="bg-white p-5 rounded-[2rem] shadow-xl shadow-blue-500/5 border border-gray-100 relative overflow-hidden active:scale-95 transition-all cursor-pointer group"
                     >
                        <div className={`absolute top-0 right-0 px-3 py-1 text-[8px] font-black uppercase tracking-widest rounded-bl-xl ${
                           res.status === 'Pending' ? 'bg-orange-100 text-orange-600' :
                           res.status === 'Confirmed' ? 'bg-blue-600 text-white' :
                           res.status === 'Completed' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                        }`}>
                           {res.status === 'Pending' ? 'Chờ duyệt' : res.status === 'Confirmed' ? 'Đã xác nhận' : res.status === 'Completed' ? 'Đã đến' : 'Đã hủy'}
                        </div>
                        <div className="flex items-center space-x-4">
                           <div className="w-10 h-10 bg-gray-50 rounded-xl flex items-center justify-center text-blue-600 group-hover:bg-blue-600 group-hover:text-white transition-colors">
                              <Calendar size={20} />
                           </div>
                           <div>
                              <p className="text-xs font-black text-gray-800 uppercase italic">{new Date(res.reservationTime).toLocaleDateString('vi-VN')}</p>
                              <p className="text-[10px] font-bold text-blue-600 flex items-center">
                                 <Clock size={10} className="mr-1"/> {new Date(res.reservationTime).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'})}
                              </p>
                           </div>
                        </div>
                        <div className="mt-3 flex justify-between items-end border-t border-gray-50 pt-3">
                           <div>
                              <p className="text-[9px] text-gray-400 uppercase font-black">Địa điểm</p>
                              <p className="text-[10px] font-bold text-gray-700 truncate max-w-[150px]">{res.branchName}</p>
                           </div>
                           <div className="text-right flex items-center space-x-2">
                              <div>
                                 <p className="text-[9px] text-gray-400 uppercase font-black">Số khách</p>
                                 <p className="text-[10px] font-black text-gray-800">{res.numberOfGuests} người</p>
                              </div>
                              <ChevronRight size={14} className="text-gray-300" />
                           </div>
                        </div>
                     </div>
                  ))}

                  {reservations.length > INITIAL_SHOW_COUNT && (
                     <button
                        onClick={() => setIsReservationsExpanded(!isReservationsExpanded)}
                        className="w-full py-3 bg-white text-blue-600 rounded-2xl font-black text-[10px] uppercase tracking-widest border border-blue-50 shadow-sm active:scale-95 transition-all"
                     >
                        {isReservationsExpanded ? 'Thu nhỏ danh sách' : `Xem thêm ${reservations.length - INITIAL_SHOW_COUNT} lịch hẹn khác`}
                     </button>
                  )}
               </div>
            )}
         </div>

         {/* Logout Button */}
         <button
           onClick={onLogout}
           className="w-full mt-10 py-4 bg-gray-100 text-gray-500 rounded-3xl font-black uppercase tracking-widest text-xs hover:bg-red-50 hover:text-red-600 transition-all flex items-center justify-center"
         >
            <LogOut size={16} className="mr-2" /> Đăng xuất tài khoản
         </button>
      </div>

      {/* ORDER DETAILS MODAL */}
      {selectedOrder && (
        <div className="fixed inset-0 bg-black/80 z-[200] flex items-end justify-center animate-in fade-in duration-300 backdrop-blur-sm">
           <div className="bg-white w-full max-w-lg rounded-t-[3rem] p-8 pb-12 shadow-2xl animate-in slide-in-from-bottom-20 duration-500 max-h-[90vh] overflow-y-auto no-scrollbar">
              <div className="flex justify-between items-start mb-8">
                 <div>
                    <h3 className="text-2xl font-black text-gray-800 uppercase italic tracking-tighter">Chi tiết đơn hàng</h3>
                    <p className="text-xs font-bold text-blue-600 uppercase tracking-widest mt-1">{selectedOrder.invoiceCode}</p>
                 </div>
                 <button onClick={() => setSelectedOrder(null)} className="bg-gray-100 p-3 rounded-full hover:bg-gray-200 transition-all text-gray-400 hover:text-gray-800"><X size={24}/></button>
              </div>

              <div className="space-y-6">
                 {/* Thông tin phục vụ */}
                 <div className="grid grid-cols-2 gap-4">
                    <div className="bg-gray-50 p-4 rounded-2xl border border-gray-100">
                       <p className="text-[9px] font-black text-gray-400 uppercase mb-1">Thời gian đặt</p>
                       <div className="flex items-center text-xs font-bold text-gray-700">
                          <Clock size={12} className="mr-1.5 text-blue-500" />
                          {new Date(selectedOrder.createdAt).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})} - {new Date(selectedOrder.createdAt).toLocaleDateString('vi-VN')}
                       </div>
                    </div>
                    <div className="bg-gray-50 p-4 rounded-2xl border border-gray-100">
                       <p className="text-[9px] font-black text-gray-400 uppercase mb-1">Địa điểm</p>
                       <div className="flex items-center text-xs font-bold text-gray-700">
                          <MapPin size={12} className="mr-1.5 text-red-500" />
                          <span className="truncate">{selectedOrder.branchName || 'Tổng hệ thống'}</span>
                       </div>
                    </div>
                    <div className="bg-gray-50 p-4 rounded-2xl border border-gray-100 col-span-2">
                       <p className="text-[9px] font-black text-gray-400 uppercase mb-1">Nhân viên phục vụ</p>
                       <div className="flex items-center text-xs font-bold text-gray-700">
                          <User size={12} className="mr-1.5 text-green-500" />
                          {selectedOrder.createdBy || 'Hệ thống tự động'}
                       </div>
                    </div>
                 </div>

                 <div className="bg-gray-50 rounded-[2rem] p-6 space-y-4">
                    {selectedOrder.details?.map((item: any, i: number) => (
                       <div key={i} className="flex justify-between items-center">
                          <div className="flex items-center space-x-3">
                             <div className="w-8 h-8 bg-white rounded-lg flex items-center justify-center text-[10px] font-black text-gray-400 border border-gray-100">
                                {item.quantity}x
                             </div>
                             <p className="text-sm font-bold text-gray-700 capitalize">{item.productName}</p>
                          </div>
                          <p className="text-sm font-black text-gray-800">{(item.unitPrice * item.quantity).toLocaleString()}đ</p>
                       </div>
                    ))}
                 </div>

                 <div className="px-4 space-y-3">
                    <div className="flex justify-between text-xs font-bold text-gray-400 uppercase tracking-widest">
                       <span>Tổng tiền món</span>
                        <span>{(selectedOrder.subTotal ?? selectedOrder.totalAmount).toLocaleString()}đ</span>
                     </div>
                     {selectedOrder.serviceFeeAmount != null && selectedOrder.serviceFeeAmount > 0 && (
                       <div className="flex justify-between text-xs font-bold text-gray-400 uppercase tracking-widest">
                         <span>Phí phục vụ ({selectedOrder.serviceFeePercent || 0}%)</span>
                         <span>{selectedOrder.serviceFeeAmount.toLocaleString()}đ</span>
                       </div>
                     )}
                     {selectedOrder.vatAmount != null && selectedOrder.vatAmount > 0 && (
                       <div className="flex justify-between text-xs font-bold text-gray-400 uppercase tracking-widest">
                         <span>VAT ({selectedOrder.vatPercent || 0}%)</span>
                         <span>{selectedOrder.vatAmount.toLocaleString()}đ</span>
                       </div>
                     )}
                    <div className="flex justify-between text-xs font-bold text-gray-400 uppercase tracking-widest border-b pb-3">
                       <span>Giảm giá</span>
                       <span className="text-red-500">-{(selectedOrder.discount || 0).toLocaleString()}đ</span>
                    </div>
                    <div className="flex justify-between items-center pt-2">
                       <span className="text-lg font-black text-gray-800 uppercase italic tracking-tighter">Thanh toán</span>
                        <span className="text-2xl font-black text-blue-700 tracking-tighter">{selectedOrder.totalAmount.toLocaleString()}đ</span>
                    </div>
                 </div>

                 <div className="bg-blue-50 rounded-2xl p-4 flex items-center space-x-3">
                    <div className="bg-blue-600 text-white p-2 rounded-xl"><Star size={16} className="fill-white" /></div>
                    <div>
                       <p className="text-[10px] font-black text-blue-800 uppercase tracking-widest">Điểm nhận được</p>
                       <p className="text-xs font-bold text-blue-600">+{(selectedOrder.totalAmount / 10000).toLocaleString()} điểm</p>
                    </div>
                 </div>
              </div>
           </div>
        </div>
      )}

      {/* RESERVATION DETAILS MODAL */}
      {selectedReservation && (
        <div className="fixed inset-0 bg-black/80 z-[200] flex items-center justify-center p-6 animate-in fade-in duration-300 backdrop-blur-sm">
           <div className="bg-white w-full max-w-sm rounded-[3rem] p-8 shadow-2xl relative animate-in zoom-in-95 duration-300">
              <button onClick={() => setSelectedReservation(null)} className="absolute top-6 right-6 text-gray-300 hover:text-gray-600"><X size={20}/></button>

              <div className="text-center mb-8">
                 <div className="w-20 h-20 bg-blue-50 rounded-[2rem] flex items-center justify-center text-blue-600 mx-auto mb-4 border-4 border-white shadow-xl">
                    <Calendar size={32} />
                 </div>
                 <h3 className="text-xl font-black text-gray-800 uppercase italic tracking-tighter">Chi tiết lịch hẹn</h3>
                 <div className={`inline-block px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest mt-2 ${
                    selectedReservation.status === 'Pending' ? 'bg-orange-100 text-orange-600' :
                    selectedReservation.status === 'Confirmed' ? 'bg-blue-600 text-white' :
                    selectedReservation.status === 'Completed' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-600'
                 }`}>
                    {selectedReservation.status === 'Pending' ? 'Đang chờ duyệt' :
                     selectedReservation.status === 'Confirmed' ? 'Đã xác nhận' :
                     selectedReservation.status === 'Completed' ? 'Hoàn thành' : 'Đã hủy'}
                 </div>
              </div>

              <div className="space-y-6">
                 <div className="grid grid-cols-2 gap-4">
                    <div className="bg-gray-50 p-4 rounded-2xl border border-gray-100 text-center">
                       <p className="text-[9px] font-black text-gray-400 uppercase mb-1">Ngày đến</p>
                       <p className="text-sm font-black text-gray-700">{new Date(selectedReservation.reservationTime).toLocaleDateString('vi-VN')}</p>
                    </div>
                    <div className="bg-gray-50 p-4 rounded-2xl border border-gray-100 text-center">
                       <p className="text-[9px] font-black text-gray-400 uppercase mb-1">Giờ hẹn</p>
                       <p className="text-sm font-black text-blue-600">{new Date(selectedReservation.reservationTime).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'})}</p>
                    </div>
                 </div>

                 <div className="space-y-4">
                    <div className="flex items-center space-x-4 p-4 bg-gray-50 rounded-2xl border border-gray-100">
                       <div className="p-2 bg-white rounded-xl text-red-500 shadow-sm"><MapPin size={18}/></div>
                       <div>
                          <p className="text-[9px] font-black text-gray-400 uppercase">Địa điểm</p>
                          <p className="text-xs font-bold text-gray-700">{selectedReservation.branchName}</p>
                       </div>
                    </div>

                    <div className="flex items-center space-x-4 p-4 bg-gray-50 rounded-2xl border border-gray-100">
                       <div className="p-2 bg-white rounded-xl text-blue-500 shadow-sm"><Users size={18}/></div>
                       <div>
                          <p className="text-[9px] font-black text-gray-400 uppercase">Số lượng khách</p>
                          <p className="text-xs font-bold text-gray-700">{selectedReservation.numberOfGuests} người</p>
                       </div>
                    </div>

                    {selectedReservation.tableName && (
                       <div className="flex items-center space-x-4 p-4 bg-blue-50 rounded-2xl border border-blue-100">
                          <div className="p-2 bg-blue-600 text-white rounded-xl shadow-sm"><LayoutGrid size={18}/></div>
                          <div>
                             <p className="text-[9px] font-black text-blue-400 uppercase">Vị trí mong muốn</p>
                             <p className="text-xs font-black text-blue-700">{selectedReservation.tableName}</p>
                          </div>
                       </div>
                    )}

                    {selectedReservation.note && (
                       <div className="p-4 bg-orange-50/50 rounded-2xl border border-orange-100">
                          <p className="text-[9px] font-black text-orange-400 uppercase mb-1">Ghi chú của bạn</p>
                          <p className="text-xs text-gray-600 italic">"{selectedReservation.note}"</p>
                       </div>
                    )}
                 </div>

                 {selectedReservation.status === 'Confirmed' && (
                    <div className="bg-green-50 p-4 rounded-2xl border border-green-100 flex items-center space-x-3">
                       <CheckCircle2 size={20} className="text-green-600 shrink-0" />
                       <p className="text-[10px] text-green-700 font-bold leading-tight">Yêu cầu đã được xác nhận. Vui lòng đến đúng giờ để có trải nghiệm tốt nhất!</p>
                    </div>
                 )}

                 <button
                    onClick={() => setSelectedReservation(null)}
                    className="w-full py-4 bg-gray-100 text-gray-500 rounded-2xl font-black uppercase tracking-widest text-xs hover:bg-gray-200 transition-all"
                 >
                    Đóng cửa sổ
                 </button>
              </div>
           </div>
        </div>
      )}

      {/* EDIT PROFILE MODAL */}
      {isEditModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex items-center justify-center p-6 backdrop-blur-sm" onClick={() => setIsEditModalOpen(false)}>
           <div className="bg-white w-full max-w-md rounded-[3rem] p-8 shadow-2xl relative max-h-[90vh] overflow-y-auto no-scrollbar" onClick={e => e.stopPropagation()}>
              <button onClick={() => setIsEditModalOpen(false)} className="absolute top-6 right-6 text-gray-300 hover:text-gray-600"><X size={20}/></button>
              <div className="text-center mb-8">
                 <div className="w-16 h-16 bg-blue-50 rounded-2xl flex items-center justify-center text-blue-600 mx-auto mb-4">
                    <User size={32} />
                 </div>
                 <h3 className="text-xl font-black text-gray-800 uppercase italic tracking-tighter">Cập nhật hồ sơ</h3>
                 <p className="text-xs text-gray-400 font-bold uppercase tracking-widest mt-1">Thay đổi thông tin cá nhân của bạn</p>
              </div>

              <div className="space-y-5">
                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Họ và tên</label>
                    <input
                      type="text"
                      className="w-full px-6 py-3 bg-gray-50 border-none rounded-2xl outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-gray-700"
                      value={editData.fullName}
                      onChange={(e) => setEditData({...editData, fullName: e.target.value})}
                      placeholder="Họ và tên..."
                    />
                 </div>

                 <div className="grid grid-cols-2 gap-4">
                    <div>
                        <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Giới tính</label>
                        <select
                           className="w-full px-6 py-3 bg-gray-50 border-none rounded-2xl outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-gray-700 appearance-none"
                           value={editData.gender}
                           onChange={(e) => setEditData({...editData, gender: e.target.value})}
                        >
                           <option value="Nam">Nam</option>
                           <option value="Nữ">Nữ</option>
                           <option value="Khác">Khác</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Ngày sinh</label>
                        <input
                           type="date"
                           className="w-full px-6 py-3 bg-gray-50 border-none rounded-2xl outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-gray-700"
                           value={editData.birthday}
                           onChange={(e) => setEditData({...editData, birthday: e.target.value})}
                        />
                    </div>
                 </div>

                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Email</label>
                    <input
                      type="email"
                      className="w-full px-6 py-3 bg-gray-50 border-none rounded-2xl outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-gray-700"
                      value={editData.email}
                      onChange={(e) => setEditData({...editData, email: e.target.value})}
                      placeholder="email@example.com"
                    />
                 </div>

                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Địa chỉ</label>
                    <textarea
                      className="w-full px-6 py-3 bg-gray-50 border-none rounded-2xl outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-gray-700 resize-none"
                      rows={2}
                      value={editData.address}
                      onChange={(e) => setEditData({...editData, address: e.target.value})}
                      placeholder="Địa chỉ của bạn..."
                    />
                 </div>

                 <div className="pt-4 border-t border-gray-100">
                    <label className="block text-[10px] font-black text-red-400 uppercase tracking-widest mb-2 ml-2">Đổi mật khẩu (nếu cần)</label>
                    <div className="relative">
                        <input
                        type="password"
                        className="w-full px-6 py-3 bg-red-50/30 border-none rounded-2xl outline-none focus:ring-2 focus:ring-red-500/20 font-bold text-gray-700"
                        value={editData.newPassword}
                        onChange={(e) => setEditData({...editData, newPassword: e.target.value})}
                        placeholder="Mật khẩu mới..."
                        />
                        <Lock className="absolute right-4 top-3.5 text-red-200" size={16} />
                    </div>
                 </div>

                 <div className="flex space-x-3 pt-4">
                    <button
                      onClick={() => setIsEditModalOpen(false)}
                      className="flex-1 py-4 bg-gray-100 text-gray-400 rounded-2xl font-black uppercase tracking-widest text-xs"
                    >
                       Hủy
                    </button>
                    <button
                      onClick={handleUpdateProfile}
                      disabled={updating || !editData.fullName.trim()}
                      className="flex-[2] py-4 bg-blue-600 text-white rounded-2xl font-black uppercase tracking-widest text-xs shadow-lg shadow-blue-500/20 disabled:opacity-50 flex items-center justify-center"
                    >
                       {updating ? <Loader2 className="animate-spin" size={18} /> : <><Save size={18} className="mr-2"/> Lưu thay đổi</>}
                    </button>
                 </div>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default CustomerProfile;
