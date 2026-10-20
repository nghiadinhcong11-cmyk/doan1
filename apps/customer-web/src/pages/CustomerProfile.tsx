import React, { useState, useEffect, useRef } from 'react';
import { User, LogOut, Package, Star, TrendingUp, ChevronRight, Clock, MapPin, Loader2, X, Edit3, Save, CheckCircle2, Calendar, Users, LayoutGrid } from 'lucide-react';
import { API_URL } from '../config';
import { Button, Feedback, FormField } from '../components/ui';
import ReceiptPreview, { ReceiptSettingsModel } from '../../../shared/ReceiptPreview';

interface CustomerOrderDetail {
  id: string;
  invoiceCode?: string;
  branchId?: string;
  branchName?: string;
  tableName?: string | null;
  createdAt: string;
  paymentAt?: string | null;
  status?: string | null;
  paymentMethod?: string | null;
  totalAmount: number;
  paidAmount?: number | null;
  subTotal?: number | null;
  discount?: number | null;
  serviceFeeAmount?: number | null;
  serviceFeePercent?: number | null;
  vatAmount?: number | null;
  vatPercent?: number | null;
  note?: string | null;
  details?: Array<{ productName?: string; quantity: number; unitPrice: number; options?: string | null }>;
}

const CustomerProfile = ({ isGuest, onLoginRequest, onLogout }: { isGuest: boolean; onLoginRequest: () => void; onLogout: () => void }) => {
  const [customer, setCustomer] = useState<any>(null);
  const [orders, setOrders] = useState<CustomerOrderDetail[]>([]);
  const [branches, setBranches] = useState<Array<{ id: string; imageUrl?: string }>>([]);
  const [reservations, setReservations] = useState<any[]>([]);
  const [loyaltyHistory, setLoyaltyHistory] = useState<any[]>([]);
  const [loading, setLoading] = useState(!isGuest);
  const [profileError, setProfileError] = useState('');
  const [selectedOrder, setSelectedOrder] = useState<CustomerOrderDetail | null>(null);
  const [orderDetail, setOrderDetail] = useState<CustomerOrderDetail | null>(null);
  const [orderDetailLoading, setOrderDetailLoading] = useState(false);
  const [orderDetailError, setOrderDetailError] = useState('');
  const orderDetailRequest = useRef(0);
  const [selectedReservation, setSelectedReservation] = useState<any>(null);
  const receiptSettings: ReceiptSettingsModel = { paperWidth: 80, showLogo: true, showAddress: true, showPhone: true, showStaff: true, showPaymentMethod: true, showOrderNote: true, showThankYou: true, thankYouText: 'Cảm ơn quý khách và hẹn gặp lại!', fontSize: 11 };

  useEffect(() => {
    if (!selectedOrder) return;
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    return () => { document.body.style.overflow = previousOverflow; };
  }, [selectedOrder]);

  useEffect(() => {
    if (!selectedOrder) {
      setOrderDetail(null);
      setOrderDetailError('');
      return;
    }

    const requestId = ++orderDetailRequest.current;
    const controller = new AbortController();
    setOrderDetail(null);
    setOrderDetailError('');
    setOrderDetailLoading(true);

    const token = localStorage.getItem('customerToken');
    fetch(`${API_URL}/api/Order/${encodeURIComponent(selectedOrder.id)}`, {
      signal: controller.signal,
      headers: token ? { Authorization: `Bearer ${token}` } : {}
    })
      .then(async response => {
        if (!response.ok) throw new Error(response.status === 403 ? 'Bạn không có quyền xem đơn hàng này.' : 'Không thể tải chi tiết đơn hàng.');
        return response.json() as Promise<CustomerOrderDetail>;
      })
      .then(data => {
        if (requestId === orderDetailRequest.current && data.id === selectedOrder.id) setOrderDetail(data);
      })
      .catch(error => {
        if (error?.name !== 'AbortError' && requestId === orderDetailRequest.current) setOrderDetailError(error?.message || 'Không thể tải chi tiết đơn hàng.');
      })
      .finally(() => {
        if (requestId === orderDetailRequest.current) setOrderDetailLoading(false);
      });

    return () => controller.abort();
  }, [selectedOrder]);

  useEffect(() => {
    fetch(`${API_URL}/api/Branch`)
      .then(response => response.ok ? response.json() : [])
      .then(data => setBranches(Array.isArray(data) ? data.map((branch: { id?: string; Id?: string; imageUrl?: string; ImageUrl?: string }) => ({
        id: String(branch.id ?? branch.Id ?? ''),
        imageUrl: branch.imageUrl ?? branch.ImageUrl
      })).filter(branch => branch.id) : []))
      .catch(() => setBranches([]));
  }, []);

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
  const [updateError, setUpdateError] = useState('');

  useEffect(() => {
    if (isGuest) {
      setLoading(false);
      return;
    }

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
        setProfileError('Không thể đọc phiên hồ sơ hiện tại. Vui lòng đăng nhập lại.');
        setLoading(false);
      }
    }
    else {
      setLoading(false);
    }
  }, [isGuest]);

  const fetchLatestProfile = async (phone: string) => {
    try {
      const token = localStorage.getItem('customerToken');
      const response = await fetch(`${API_URL}/api/Customer/${phone}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (!response.ok) {
        if (response.status === 401 || response.status === 403) {
          throw new Error('Phiên đăng nhập không còn quyền xem hồ sơ. Vui lòng đăng nhập lại.');
        }
        throw new Error(`Không thể tải hồ sơ (HTTP ${response.status}).`);
      }
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
      await fetchOrderHistory();
      await fetchReservationHistory(phone);
    } catch (err) {
      setProfileError(err instanceof Error ? err.message : 'Không thể tải hồ sơ.');
      console.error("Lỗi tải hồ sơ:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async () => {
    setUpdateError('');
    if (!editData.fullName.trim()) {
      setUpdateError('Vui lòng nhập họ và tên.');
      return;
    }
    if (editData.newPassword && (editData.newPassword.length < 8 || editData.newPassword.length > 128)) {
      setUpdateError('Mật khẩu mới phải dài từ 8 đến 128 ký tự.');
      return;
    }

    setUpdating(true);
    try {
      const response = await fetch(`${API_URL}/api/Customer/${customer.phoneNumber}/profile`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
          ...(localStorage.getItem('customerToken') ? { Authorization: `Bearer ${localStorage.getItem('customerToken')}` } : {})
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
      } else {
        const body = await response.json().catch(() => ({}));
        setUpdateError(body.message || 'Không thể cập nhật thông tin.');
      }
    } catch (err) {
      setUpdateError('Lỗi kết nối máy chủ khi cập nhật thông tin.');
    } finally {
      setUpdating(false);
    }
  };

  const fetchOrderHistory = async () => {
    try {
      const token = localStorage.getItem('customerToken');
      const response = await fetch(`${API_URL}/api/Order/customer-summary`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (!response.ok) throw new Error('Không thể tải lịch sử đơn hàng.');
      const data = await response.json();
      setOrders(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchLoyaltyHistory = async (customerId: string) => {
    try {
      const token = localStorage.getItem('customerToken');
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
      const token = localStorage.getItem('customerToken');
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
          onClick={onLoginRequest}
          className="px-8 py-3 bg-blue-600 text-white rounded-2xl font-black text-xs uppercase tracking-widest shadow-lg shadow-blue-500/20"
        >
          ĐĂNG NHẬP NGAY
        </button>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 pb-24 font-sans relative">
      {profileError && (
        <div role="alert" className="mx-6 mt-6 rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm font-bold text-amber-800">
          {profileError}
        </div>
      )}
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
        <div className="customer-order-dialog-overlay fixed inset-0 z-[250] flex items-end justify-center bg-black/60 p-2 backdrop-blur-sm animate-in fade-in duration-300 sm:items-center sm:p-6" onClick={() => setSelectedOrder(null)}>
           <div id="invoice-print" data-paper-width="80" className="customer-order-dialog max-h-[calc(100dvh-1rem)] w-full max-w-lg overflow-y-auto overscroll-contain rounded-[2rem] bg-white p-4 pb-6 shadow-2xl animate-in slide-in-from-bottom-20 duration-300 sm:max-h-[calc(100dvh-3rem)] sm:rounded-[2.5rem] sm:p-8 sm:pb-10" onClick={event => event.stopPropagation()}>
              <div className="flex justify-between items-start mb-8">
                 <div>
                    <h3 className="text-2xl font-black text-gray-800 uppercase italic tracking-tighter">Chi tiết đơn hàng</h3>
                    <p className="text-xs font-bold text-blue-600 uppercase tracking-widest mt-1">{selectedOrder.invoiceCode}</p>
                 </div>
                 <button onClick={() => setSelectedOrder(null)} className="print-hide bg-gray-100 p-3 rounded-full hover:bg-gray-200 transition-all text-gray-400 hover:text-gray-800"><X size={24}/></button>
              </div>

              {orderDetailLoading && <div className="flex flex-col items-center justify-center gap-3 py-16 text-gray-400"><Loader2 className="animate-spin text-blue-600" size={32} /><p className="text-xs font-bold uppercase tracking-widest">Đang tải chi tiết...</p></div>}
              {!orderDetailLoading && orderDetailError && <div className="rounded-2xl bg-red-50 p-5 text-center text-sm font-bold text-red-700">{orderDetailError}</div>}
              {!orderDetailLoading && !orderDetailError && orderDetail && (
                <>
                  <div className="mb-5 grid grid-cols-2 gap-3 text-xs">
                    <div className="rounded-2xl border border-gray-100 bg-gray-50 p-3"><p className="mb-1 text-[9px] font-black uppercase text-gray-400">Thời gian đặt</p><p className="font-bold text-gray-700">{new Date(orderDetail.createdAt).toLocaleString('vi-VN')}</p></div>
                    <div className="rounded-2xl border border-gray-100 bg-gray-50 p-3"><p className="mb-1 text-[9px] font-black uppercase text-gray-400">Thanh toán lúc</p><p className="font-bold text-gray-700">{orderDetail.paymentAt ? new Date(orderDetail.paymentAt).toLocaleString('vi-VN') : 'Chưa có thông tin'}</p></div>
                    <div className="rounded-2xl border border-gray-100 bg-gray-50 p-3"><p className="mb-1 text-[9px] font-black uppercase text-gray-400">Chi nhánh</p><p className="truncate font-bold text-gray-700">{orderDetail.branchName || 'Chưa có thông tin'}</p></div>
                    <div className="rounded-2xl border border-gray-100 bg-gray-50 p-3"><p className="mb-1 text-[9px] font-black uppercase text-gray-400">Phục vụ</p><p className="font-bold text-gray-700">{orderDetail.tableName || 'Mang về'}</p></div>
                    <div className="rounded-2xl border border-gray-100 bg-gray-50 p-3"><p className="mb-1 text-[9px] font-black uppercase text-gray-400">Trạng thái đơn</p><p className="font-bold text-gray-700">{orderDetail.status || 'Chưa có thông tin'}</p></div>
                    <div className="rounded-2xl border border-gray-100 bg-gray-50 p-3"><p className="mb-1 text-[9px] font-black uppercase text-gray-400">Thanh toán</p><p className="font-bold text-gray-700">{orderDetail.paymentMethod || 'Chưa thanh toán'}</p></div>
                  </div>
                  <div className="flex justify-center overflow-x-auto">
                    <ReceiptPreview settings={receiptSettings} branch={{ name: orderDetail.branchName, imageUrl: branches.find(branch => branch.id.toLowerCase() === String(orderDetail.branchId ?? '').toLowerCase())?.imageUrl }} order={orderDetail} />
                  </div>
                  {orderDetail.note && <p className="mt-4 rounded-2xl bg-amber-50 p-4 text-sm text-amber-900"><span className="font-black">Ghi chú đơn:</span> {orderDetail.note}</p>}
                  {(() => {
                    const earned = loyaltyHistory.find(log => log.orderId === orderDetail.id && log.type === 'Earn');
                    const redeemed = loyaltyHistory.find(log => log.orderId === orderDetail.id && log.type === 'Redeem');
                    if (!earned && !redeemed) return null;
                    return <div className="mt-4 rounded-2xl bg-blue-50 p-4 text-sm text-blue-800"><div className="flex items-center gap-2 font-black"><Star size={16} className="fill-blue-600 text-blue-600" /> Điểm của đơn hàng</div>{earned && <p className="mt-2">Tích được: +{Number(earned.points || 0).toLocaleString('vi-VN')} điểm</p>}{redeemed && <p>Đã sử dụng: {Math.abs(Number(redeemed.points || 0)).toLocaleString('vi-VN')} điểm</p>}</div>;
                  })()}
                </>
              )}
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

                 <div className="border-t border-gray-100 pt-4">
                    <FormField
                      label="Đổi mật khẩu (nếu cần)"
                      type="password"
                      value={editData.newPassword}
                      onChange={(e) => setEditData({...editData, newPassword: e.target.value})}
                      placeholder="Mật khẩu mới..."
                      minLength={8}
                      maxLength={128}
                      helperText="Sử dụng từ 8 đến 128 ký tự."
                      className="bg-red-50/30"
                    />
                 </div>

                 {updateError && <Feedback tone="error">{updateError}</Feedback>}

                 <div className="flex space-x-3 pt-4">
                    <Button
                      type="button"
                      onClick={() => setIsEditModalOpen(false)}
                      variant="secondary"
                      className="flex-1 uppercase tracking-widest"
                    >
                       Hủy
                    </Button>
                    <Button
                      type="button"
                      onClick={handleUpdateProfile}
                      disabled={updating || !editData.fullName.trim()}
                      loading={updating}
                      className="flex-[2] uppercase tracking-widest"
                    >
                       <Save aria-hidden="true" size={18}/> Lưu thay đổi
                    </Button>
                 </div>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default CustomerProfile;
