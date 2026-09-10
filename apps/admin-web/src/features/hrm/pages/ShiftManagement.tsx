import React, { useState, useEffect } from 'react';
import { Search, Calendar, User, MapPin, Banknote, RotateCcw, CreditCard, ArrowRight, Download, Filter, Loader2, CheckCircle2, Clock, AlertCircle, Store } from 'lucide-react';
import { API_URL } from '../../../config';

interface Shift {
  id: string;
  employeeName: string;
  branchId: string;
  branchName: string;
  startTime: string;
  endTime: string;
  startingCash: number;
  endingCash: number;
  totalRevenue: number;
  cashRevenue: number;
  transferRevenue: number;
  status: string;
  note: string;
}

const ShiftManagement = () => {
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [loading, setLoading] = useState(true);
  const [filterBranch, setFilterBranch] = useState('all'); // Store Branch ID here
  const [branches, setBranches] = useState<any[]>([]);
  const isAdmin = localStorage.getItem('userRole') === 'admin';

  // Date filters
  const [fromDate, setFromDate] = useState(new Date().toISOString().split('T')[0]);
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);

  const fetchData = async () => {
    try {
      setLoading(true);
      const url = `${API_URL}/api/Shift?fromDate=${fromDate}&toDate=${toDate}`;
      const [resShifts, resBranches] = await Promise.all([
        fetch(url),
        isAdmin ? fetch(`${API_URL}/api/Branch`) : Promise.resolve(null)
      ]);
      const dataShifts = await resShifts.json();
      setShifts(Array.isArray(dataShifts) ? dataShifts : []);
      if (resBranches) {
        const dataBranches = await resBranches.json();
        setBranches(Array.isArray(dataBranches) ? dataBranches : []);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [fromDate, toDate]);

  const getDifference = (shift: Shift) => {
    // Tiền mặt kỳ vọng = Tiền mặt đầu ca + Doanh thu tiền mặt trong ca
    const expectedCash = shift.startingCash + shift.cashRevenue;
    return (shift.endingCash || 0) - expectedCash;
  };

  const filteredShifts = shifts.filter(s => filterBranch === 'all' || s.branchId === filterBranch);

  return (
    <div className="p-6 bg-[#f0f2f5] min-h-screen font-sans">
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 gap-4">
        <div>
          <h1 className="text-2xl font-black text-gray-800 uppercase tracking-tighter italic">Báo cáo chốt ca chi tiết</h1>
          <p className="text-xs text-gray-400 font-bold uppercase tracking-widest mt-1">Đối soát tiền mặt & Doanh thu theo cơ sở</p>
        </div>

        <div className="flex items-center space-x-3">
           <div className="flex items-center bg-white p-2 rounded-xl shadow-sm border border-gray-100 space-x-2">
              <div className="flex items-center space-x-2">
                 <span className="text-[10px] font-black text-gray-400 uppercase">Từ:</span>
                 <input
                    type="date"
                    value={fromDate}
                    onChange={(e) => setFromDate(e.target.value)}
                    className="text-[11px] font-bold text-gray-700 outline-none border-none bg-gray-50 rounded px-2 py-1"
                 />
              </div>
              <div className="flex items-center space-x-2">
                 <span className="text-[10px] font-black text-gray-400 uppercase">Đến:</span>
                 <input
                    type="date"
                    value={toDate}
                    onChange={(e) => setToDate(e.target.value)}
                    className="text-[11px] font-bold text-gray-700 outline-none border-none bg-gray-50 rounded px-2 py-1"
                 />
              </div>
           </div>

           <div className="flex bg-white p-1 rounded-xl shadow-sm border border-gray-100">
              <button
                onClick={() => setFilterBranch('all')}
                className={`px-4 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all ${filterBranch === 'all' ? 'bg-blue-600 text-white shadow-md' : 'text-gray-400'}`}
              >Tất cả</button>
              {branches.map(b => (
                <button
                  key={b.id}
                  onClick={() => setFilterBranch(b.id)}
                  className={`px-4 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all ${filterBranch === b.id ? 'bg-blue-600 text-white shadow-md' : 'text-gray-400'}`}
                >{b.name}</button>
              ))}
           </div>
           <button onClick={fetchData} className="bg-white p-2.5 rounded-xl shadow-sm border border-gray-100 hover:bg-gray-50 text-gray-400 transition-all">
              <RotateCcw size={18} />
           </button>
        </div>
      </div>

      <div className="space-y-6">
        {loading ? (
          <div className="flex justify-center py-20"><Loader2 className="animate-spin text-blue-600" /></div>
        ) : filteredShifts.length === 0 ? (
          <div className="bg-white rounded-[2rem] p-20 text-center text-gray-400 font-bold uppercase text-xs tracking-widest">Không có dữ liệu ca làm việc</div>
        ) : (
          filteredShifts.map((shift) => {
            const diff = getDifference(shift);
            return (
              <div key={shift.id} className="bg-white rounded-[2.5rem] shadow-xl shadow-blue-500/5 border border-white overflow-hidden group hover:scale-[1.005] transition-all">
                <div className="p-8">
                   {/* Header Row */}
                   <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6 mb-8 pb-6 border-b border-gray-50">
                      <div className="flex items-center space-x-5">
                         <div className="w-16 h-16 bg-blue-50 rounded-2xl flex items-center justify-center text-blue-600 font-black text-xl border border-blue-100 shadow-sm">
                            {shift.employeeName.charAt(0)}
                         </div>
                         <div>
                            <div className="flex items-center space-x-2 mb-1">
                               <h3 className="font-black text-xl text-gray-800 uppercase italic tracking-tighter">{shift.employeeName}</h3>
                               <span className={`text-[9px] font-black px-2 py-0.5 rounded-full uppercase tracking-widest ${shift.status === 'Open' ? 'bg-blue-100 text-blue-600' : 'bg-green-100 text-green-600'}`}>
                                  {shift.status === 'Open' ? 'Đang trực' : 'Đã chốt'}
                               </span>
                            </div>
                            <div className="flex items-center text-[10px] text-gray-400 font-bold uppercase tracking-widest space-x-4">
                               <span className="flex items-center text-blue-500"><Store size={12} className="mr-1.5"/> {shift.branchName || 'Chưa xác định'}</span>
                               <span className="flex items-center"><Calendar size={12} className="mr-1.5"/> {new Date(shift.startTime).toLocaleDateString('vi-VN')}</span>
                               <span className="flex items-center"><Clock size={12} className="mr-1.5"/> {new Date(shift.startTime).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'})} - {shift.endTime ? new Date(shift.endTime).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'}) : '...'}</span>
                            </div>
                         </div>
                      </div>

                      <div className="flex flex-wrap gap-3">
                         <div className="bg-blue-50/50 px-6 py-3 rounded-2xl border border-blue-100/50">
                            <p className="text-[9px] font-black text-blue-400 uppercase tracking-widest mb-1">Tổng doanh thu ca</p>
                            <p className="text-lg font-black text-blue-700 tracking-tighter">{(shift.totalRevenue || 0).toLocaleString()}đ</p>
                         </div>
                         <div className="bg-orange-50/50 px-6 py-3 rounded-2xl border border-orange-100/50">
                            <p className="text-[9px] font-black text-orange-400 uppercase tracking-widest mb-1">Thực tế bàn giao</p>
                            <p className="text-lg font-black text-orange-700 tracking-tighter">{(shift.endingCash || 0).toLocaleString()}đ</p>
                         </div>
                      </div>
                   </div>

                   {/* Detail Grid */}
                   <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
                      <div className="space-y-4">
                         <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest border-l-2 border-blue-500 pl-3">Dòng tiền mặt (Cash)</h4>
                         <div className="space-y-2 pl-3">
                            <div className="flex justify-between items-center text-xs">
                               <span className="text-gray-500 font-medium">Tiền lẻ đầu ca:</span>
                               <span className="font-bold text-gray-700">{shift.startingCash.toLocaleString()}đ</span>
                            </div>
                            <div className="flex justify-between items-center text-xs">
                               <span className="text-gray-500 font-medium text-green-600">+ Doanh thu mặt:</span>
                               <span className="font-black text-green-600">{shift.cashRevenue.toLocaleString()}đ</span>
                            </div>
                            <div className="flex justify-between items-center text-sm pt-2 border-t border-dashed border-gray-100">
                               <span className="text-gray-800 font-bold uppercase text-[10px]">Kỳ vọng trong két:</span>
                               <span className="font-black text-blue-600">{(shift.startingCash + shift.cashRevenue).toLocaleString()}đ</span>
                            </div>
                         </div>
                      </div>

                      <div className="space-y-4">
                         <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest border-l-2 border-purple-500 pl-3">Doanh thu Online</h4>
                         <div className="space-y-2 pl-3">
                            <div className="flex justify-between items-center text-xs">
                               <span className="text-gray-500 font-medium">Chuyển khoản / QR:</span>
                               <span className="font-black text-purple-600">{shift.transferRevenue.toLocaleString()}đ</span>
                            </div>
                            <p className="text-[9px] text-gray-400 italic mt-2">* Tiền này đã chuyển thẳng vào tài khoản ngân hàng của quán.</p>
                         </div>
                      </div>

                      <div className="space-y-4">
                         <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest border-l-2 border-orange-500 pl-3">Kết quả đối soát</h4>
                         <div className="bg-gray-50 p-4 rounded-2xl border border-gray-100">
                            <div className="flex justify-between items-center mb-1">
                               <span className="text-[9px] font-black text-gray-500 uppercase">Chênh lệch két:</span>
                               {shift.status === 'Closed' ? (
                                  <span className={`text-sm font-black ${diff === 0 ? 'text-green-500' : 'text-red-500'}`}>
                                     {diff > 0 ? '+' : ''}{diff.toLocaleString()}đ
                                  </span>
                               ) : (
                                  <span className="text-[10px] font-bold text-blue-500 italic">Đang cập nhật...</span>
                               )}
                            </div>
                            {diff !== 0 && shift.status === 'Closed' && (
                               <div className="flex items-center text-[9px] font-bold text-red-400 mt-1">
                                  <AlertCircle size={10} className="mr-1"/> {diff < 0 ? 'Hụt tiền mặt' : 'Thừa tiền mặt'}
                               </div>
                            )}
                         </div>
                      </div>

                      <div className="space-y-4">
                         <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest border-l-2 border-gray-300 pl-3">Ghi chú bàn giao</h4>
                         <div className="bg-gray-50 p-4 rounded-2xl border border-gray-100 min-h-[60px]">
                            <p className="text-xs text-gray-600 italic leading-relaxed">
                               {shift.note || "Không có ghi chú nào từ nhân viên."}
                            </p>
                         </div>
                      </div>
                   </div>
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
};

export default ShiftManagement;

