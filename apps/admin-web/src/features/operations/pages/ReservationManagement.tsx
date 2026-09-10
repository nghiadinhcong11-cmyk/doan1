import React, { useState, useEffect } from 'react';
import { Calendar, Clock, Users, MapPin, Phone, User, CheckCircle2, XCircle, Clock4, Trash2, Loader2, RotateCcw, Filter, MessageSquare } from 'lucide-react';
import { API_URL } from '../../../config';

interface Reservation {
  id: string;
  customerName: string;
  customerPhone: string;
  reservationTime: string;
  numberOfGuests: number;
  branchName: string;
  status: string;
  note: string;
  createdAt: string;
}

interface ReservationManagementProps {
  readOnly?: boolean;
}

const ReservationManagement: React.FC<ReservationManagementProps> = ({ readOnly = false }) => {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const fetchReservations = async () => {
    try {
      setLoading(true);
      const branchId = localStorage.getItem('selectedBranchId');
      const params = new URLSearchParams();
      if (filter !== 'all') params.append('status', filter);
      if (branchId) params.append('branchId', branchId);
      if (fromDate) params.append('fromDate', fromDate);
      if (toDate) params.append('toDate', toDate);

      const response = await fetch(`${API_URL}/api/Reservation?${params.toString()}`);
      const data = await response.json();
      setReservations(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReservations();
  }, [filter, fromDate, toDate]);

  const updateStatus = async (id: string, newStatus: string) => {
    try {
      const response = await fetch(`${API_URL}/api/Reservation/${id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newStatus)
      });
      if (response.ok) {
        fetchReservations();
      }
    } catch (err) {
      alert('Lỗi khi cập nhật trạng thái');
    }
  };

  const deleteReservation = async (id: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa lịch hẹn này?')) return;
    try {
      const response = await fetch(`${API_URL}/api/Reservation/${id}`, { method: 'DELETE' });
      if (response.ok) fetchReservations();
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="p-6 bg-[#f0f2f5] min-h-screen font-sans">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 gap-4">
        <div>
          <h1 className="text-2xl font-black text-gray-800 uppercase tracking-tighter italic">Quản lý đặt bàn</h1>
          <p className="text-xs text-gray-400 font-bold uppercase tracking-widest mt-1">Theo dõi lịch hẹn khách hàng</p>
        </div>

        <div className="flex flex-wrap items-center gap-4">
          <div className="flex bg-white p-1 rounded-xl shadow-sm border border-gray-100">
            <div className="flex items-center px-3 border-r">
              <span className="text-[9px] font-black text-gray-400 uppercase mr-2">Từ:</span>
              <input
                type="date"
                className="text-[11px] font-bold text-gray-700 outline-none border-none bg-transparent"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
              />
            </div>
            <div className="flex items-center px-3">
              <span className="text-[9px] font-black text-gray-400 uppercase mr-2">Đến:</span>
              <input
                type="date"
                className="text-[11px] font-bold text-gray-700 outline-none border-none bg-transparent"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
              />
            </div>
            <button
              onClick={() => { setFromDate(''); setToDate(''); }}
              className="p-1.5 text-gray-400 hover:text-red-500 transition-colors"
              title="Xóa lọc ngày"
            >
              <RotateCcw size={14} />
            </button>
          </div>

          <div className="flex bg-white p-1 rounded-xl shadow-sm border border-gray-100">
             {['all', 'Pending', 'Confirmed', 'Completed', 'Cancelled'].map((s) => (
             <button
               key={s}
               onClick={() => setFilter(s)}
               className={`px-4 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all ${filter === s ? 'bg-blue-600 text-white shadow-md' : 'text-gray-400 hover:text-gray-600'}`}
             >
               {s === 'all' ? 'Tất cả' : s === 'Pending' ? 'Chờ duyệt' : s === 'Confirmed' ? 'Đã xác nhận' : s === 'Completed' ? 'Đã đến' : 'Đã hủy'}
             </button>
           ))}
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {loading ? (
          <div className="col-span-full py-20 flex justify-center"><Loader2 className="animate-spin text-blue-600" /></div>
        ) : reservations.length === 0 ? (
          <div className="col-span-full py-20 text-center text-gray-400 font-bold uppercase text-xs tracking-widest">Không có lịch hẹn nào</div>
        ) : (
          reservations.map((res) => (
            <div key={res.id} className="bg-white rounded-[2rem] shadow-xl shadow-blue-500/5 border border-white p-6 relative group overflow-hidden">
               <div className={`absolute top-0 right-0 px-4 py-1.5 rounded-bl-2xl text-[9px] font-black uppercase tracking-widest ${
                  res.status === 'Pending' ? 'bg-orange-100 text-orange-600' :
                  res.status === 'Confirmed' ? 'bg-blue-100 text-blue-600' :
                  res.status === 'Completed' ? 'bg-green-100 text-green-600' : 'bg-red-100 text-red-600'
               }`}>
                  {res.status}
               </div>

               <div className="flex items-center space-x-4 mb-6">
                  <div className="w-12 h-12 bg-blue-50 rounded-2xl flex items-center justify-center text-blue-600 font-black text-lg">
                     {res.customerName.charAt(0)}
                  </div>
                  <div>
                     <h3 className="font-black text-gray-800 uppercase tracking-tight">{res.customerName}</h3>
                     <p className="text-xs text-blue-600 font-bold">{res.customerPhone}</p>
                  </div>
               </div>

               <div className="space-y-3 mb-6">
                  <div className="flex items-center text-xs font-medium text-gray-500">
                     <Calendar size={14} className="mr-3 text-gray-400" />
                     <span className="font-bold">{new Date(res.reservationTime).toLocaleDateString('vi-VN')}</span>
                     <span className="mx-2">|</span>
                     <Clock size={14} className="mr-2 text-gray-400" />
                     <span className="font-bold text-blue-700">{new Date(res.reservationTime).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})}</span>
                  </div>
                  <div className="flex items-center text-xs font-medium text-gray-500">
                     <Users size={14} className="mr-3 text-gray-400" />
                     <span>Số lượng: <span className="font-black text-gray-800">{res.numberOfGuests} người</span></span>
                  </div>
                  <div className="flex items-center text-xs font-medium text-gray-500">
                     <MapPin size={14} className="mr-3 text-gray-400" />
                     <span className="truncate">{res.branchName}</span>
                  </div>
                  {res.note && (
                    <div className="flex items-start text-xs font-medium text-gray-500">
                       <MessageSquare size={14} className="mr-3 text-gray-400 mt-0.5" />
                       <span className="italic">"{res.note}"</span>
                    </div>
                  )}
               </div>

               {!readOnly && (
                 <div className="grid grid-cols-2 gap-3">
                    {res.status === 'Pending' && (
                      <button onClick={() => updateStatus(res.id, 'Confirmed')} className="bg-blue-600 text-white py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-blue-700 transition-all">Xác nhận</button>
                    )}
                    {res.status === 'Confirmed' && (
                      <button onClick={() => updateStatus(res.id, 'Completed')} className="bg-green-600 text-white py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-green-700 transition-all">Đã đến</button>
                    )}
                    {(res.status === 'Pending' || res.status === 'Confirmed') && (
                      <button onClick={() => updateStatus(res.id, 'Cancelled')} className="bg-gray-100 text-gray-500 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-gray-200 transition-all">Hủy hẹn</button>
                    )}
                    {(res.status === 'Cancelled' || res.status === 'Completed') && (
                      <button onClick={() => deleteReservation(res.id)} className="col-span-2 bg-red-50 text-red-500 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-red-100 transition-all">Xóa lịch sử</button>
                    )}
                 </div>
               )}
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default ReservationManagement;

