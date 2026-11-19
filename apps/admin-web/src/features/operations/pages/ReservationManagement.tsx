import React, { useState, useEffect } from 'react';
import { Calendar, Clock, Users, MapPin, Loader2, RotateCcw, RefreshCw, MessageSquare, Armchair } from 'lucide-react';
import { API_URL } from '../../../config';
import { fetchBranchList, type BranchSummary } from '../../../api/branchQuery';
import { notifyFeedback, StatusBadge } from '../../../components/ui';
import { useRef } from 'react';

interface Reservation {
  id: string;
  customerName: string;
  customerPhone: string;
  reservationTime: string;
  numberOfGuests: number;
  tableName?: string | null;
  branchName: string;
  status: string;
  note: string;
  createdAt: string;
}

interface ReservationManagementProps {
  readOnly?: boolean;
}

const reservationTone = (status: string) => status === 'Pending' ? 'pending' as const : status === 'Confirmed' ? 'confirmed' as const : status === 'Completed' ? 'completed' as const : 'cancelled' as const;
const reservationStatusLabel = (status: string) => status === 'Pending' ? 'Chờ duyệt' : status === 'Confirmed' ? 'Đã xác nhận' : status === 'Completed' ? 'Đã đến' : 'Đã hủy';

const ReservationManagement: React.FC<ReservationManagementProps> = ({ readOnly = false }) => {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('all');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [selectedBranchId, setSelectedBranchId] = useState(() => localStorage.getItem('selectedBranchId') || '');
  const [branches, setBranches] = useState<BranchSummary[]>([]);
  const [branchLoading, setBranchLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [updatingId, setUpdatingId] = useState<string | null>(null);
  const requestVersion = useRef(0);
  const isGlobalAdmin = localStorage.getItem('userRole') === 'admin';

  const fetchReservations = async (signal?: AbortSignal) => {
    const version = ++requestVersion.current;
    try {
      setLoading(true);
      const params = new URLSearchParams();
      if (filter !== 'all') params.append('status', filter);
      if (selectedBranchId) params.append('branchId', selectedBranchId);
      if (fromDate) params.append('fromDate', fromDate);
      if (toDate) params.append('toDate', toDate);

      const response = await fetch(`${API_URL}/api/Reservation?${params.toString()}`, { signal, cache: 'no-store' });
      if (!response.ok) throw new Error('Không thể tải danh sách đặt bàn.');
      const data: unknown = await response.json();
      if (!Array.isArray(data)) throw new Error('Dữ liệu đặt bàn không hợp lệ.');
      if (version === requestVersion.current) {
        setReservations(data as Reservation[]);
        setError(null);
      }
    } catch (reason) {
      if (reason instanceof DOMException && reason.name === 'AbortError') return;
      if (version === requestVersion.current) setError(reason instanceof Error ? reason.message : 'Không thể tải danh sách đặt bàn.');
    } finally {
      if (version === requestVersion.current) setLoading(false);
    }
  };

  useEffect(() => {
    let disposed = false;
    void fetchBranchList()
      .then(data => { if (!disposed) setBranches(data); })
      .catch(() => undefined)
      .finally(() => { if (!disposed) setBranchLoading(false); });
    return () => { disposed = true; };
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void fetchReservations(controller.signal);
    return () => controller.abort();
  }, [filter, fromDate, toDate, selectedBranchId]);

  useEffect(() => {
    const handleBranchChanged = () => setSelectedBranchId(localStorage.getItem('selectedBranchId') || '');
    window.addEventListener('branch:changed', handleBranchChanged);
    const handleReservationCreated = () => { void fetchReservations(); };
    window.addEventListener('reservation-created', handleReservationCreated);
    return () => {
      window.removeEventListener('branch:changed', handleBranchChanged);
      window.removeEventListener('reservation-created', handleReservationCreated);
    };
  }, [filter, fromDate, toDate, selectedBranchId]);

  const updateStatus = async (id: string, newStatus: string) => {
    if (updatingId) return;
    if (newStatus === 'Cancelled' && !window.confirm('Bạn có chắc muốn hủy lịch đặt bàn này không?')) return;
    setUpdatingId(id);
    try {
      const response = await fetch(`${API_URL}/api/Reservation/${id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newStatus)
      });
      if (response.ok) {
        await fetchReservations();
      } else {
        const body = await response.json().catch(() => null) as { message?: string } | null;
        throw new Error(body?.message || 'Không thể cập nhật trạng thái đặt bàn.');
      }
    } catch (reason) {
      notifyFeedback(reason instanceof Error ? reason.message : 'Lỗi khi cập nhật trạng thái');
    } finally {
      setUpdatingId(null);
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
          {isGlobalAdmin && <label className="flex items-center gap-2 rounded-xl border border-gray-100 bg-white px-3 py-2 text-[10px] font-black uppercase tracking-widest text-gray-500 shadow-sm">
            <span>Chi nhánh</span>
            <select value={selectedBranchId} disabled={branchLoading || branches.length === 0} onChange={event => { const branchId = event.target.value; const branch = branches.find(item => item.id === branchId); setSelectedBranchId(branchId); localStorage.setItem('selectedBranchId', branchId); if (branch) localStorage.setItem('selectedBranchName', branch.name); window.dispatchEvent(new Event('branch:changed')); }} className="max-w-[180px] bg-transparent text-xs font-black normal-case text-gray-700 outline-none">
              {branches.length === 0 && <option value="">Đang tải...</option>}
              {branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
            </select>
          </label>}
          <button type="button" onClick={() => void fetchReservations()} className="inline-flex items-center gap-2 rounded-xl bg-white px-3 py-2 text-[10px] font-black uppercase tracking-widest text-gray-500 shadow-sm border border-gray-100 hover:text-blue-600" title="Làm mới danh sách">
            <RefreshCw size={14} /> Làm mới
          </button>
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

      {error && <div className="mb-6 flex items-center justify-between gap-3 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"><span>{error}</span><button type="button" onClick={() => void fetchReservations()} className="font-black text-red-800 underline">Thử lại</button></div>}

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {loading ? (
          <div className="col-span-full py-20 flex justify-center"><Loader2 className="animate-spin text-blue-600" /></div>
        ) : reservations.length === 0 ? (
          <div className="col-span-full py-20 text-center text-gray-400 font-bold uppercase text-xs tracking-widest">Không có lịch hẹn nào</div>
        ) : (
          reservations.map((res) => (
            <div key={res.id} className="bg-white rounded-[2rem] shadow-xl shadow-blue-500/5 border border-white p-6 relative group overflow-hidden">
               <div className="absolute right-4 top-4"><StatusBadge tone={reservationTone(res.status)}>{reservationStatusLabel(res.status)}</StatusBadge></div>

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
                  {res.tableName && (
                    <div className="flex items-center text-xs font-medium text-gray-500">
                       <Armchair size={14} className="mr-3 text-gray-400" />
                       <span>Bàn: <span className="font-black text-gray-800">{res.tableName}</span></span>
                    </div>
                  )}
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
                      <button disabled={updatingId === res.id} onClick={() => updateStatus(res.id, 'Confirmed')} className="bg-blue-600 text-white py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-blue-700 transition-all disabled:cursor-wait disabled:opacity-60">{updatingId === res.id ? 'Đang lưu...' : 'Xác nhận'}</button>
                    )}
                    {res.status === 'Confirmed' && (
                      <button disabled={updatingId === res.id} onClick={() => updateStatus(res.id, 'Completed')} className="bg-green-600 text-white py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-green-700 transition-all disabled:cursor-wait disabled:opacity-60">{updatingId === res.id ? 'Đang lưu...' : 'Đã đến'}</button>
                    )}
                    {(res.status === 'Pending' || res.status === 'Confirmed') && (
                      <button disabled={updatingId === res.id} onClick={() => updateStatus(res.id, 'Cancelled')} className="bg-gray-100 text-gray-500 py-2.5 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-gray-200 transition-all disabled:cursor-wait disabled:opacity-60">Hủy hẹn</button>
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

