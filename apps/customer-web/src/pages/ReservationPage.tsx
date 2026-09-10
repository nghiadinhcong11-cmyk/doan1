import React, { useState, useEffect } from 'react';
import { Calendar, Users, Clock, MapPin, MessageSquare, CheckCircle2, Loader2, ChevronRight, User, Phone, LayoutGrid } from 'lucide-react';
import { API_URL } from '../config';

const ReservationPage = () => {
  const [branches, setBranches] = useState<any[]>([]);
  const [loadingBranches, setLoadingBranches] = useState(false);
  const [tables, setTables] = useState<any[]>([]);
  const [loadingTables, setLoadingTables] = useState(false);

  const [formData, setFormData] = useState({
    customerName: '',
    customerPhone: '',
    reservationTime: '',
    numberOfGuests: 2,
    branchId: '',
    branchName: '',
    tableId: '',
    tableName: '',
    note: ''
  });
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    fetchBranches();
    const saved = localStorage.getItem('customerInfo');
    if (saved) {
      const info = JSON.parse(saved);
      setFormData(prev => ({ ...prev, customerName: info.fullName || '', customerPhone: info.phoneNumber || '' }));
    }
  }, []);

  useEffect(() => {
    if (formData.branchId) {
      fetchTables(formData.branchId);
    }
  }, [formData.branchId]);

  const fetchBranches = async () => {
    try {
      setLoadingBranches(true);
      const response = await fetch(`${API_URL}/api/Branch`);
      const data = await response.json();
      setBranches(data);
      if (data.length > 0) {
        setFormData(prev => ({ ...prev, branchId: data[0].id, branchName: data[0].name }));
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingBranches(false);
    }
  };

  const fetchTables = async (branchId: string) => {
    try {
      setLoadingTables(true);
      const response = await fetch(`${API_URL}/api/Table?branchId=${branchId}&isActive=true`);
      const data = await response.json();
      // Lọc bỏ bàn "Mang về" cho việc đặt bàn trước
      setTables(data.filter((t: any) => t.name !== 'Mang về'));
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingTables(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.reservationTime) {
      alert('Vui lòng chọn thời gian!');
      return;
    }

    try {
      setSubmitting(true);

      // Đảm bảo dữ liệu gửi đi đúng định dạng
      const submissionData = {
        ...formData,
        branchId: formData.branchId || null,
        tableId: formData.tableId || null,
        reservationTime: new Date(formData.reservationTime).toISOString(),
        numberOfGuests: Number(formData.numberOfGuests)
      };

      const response = await fetch(`${API_URL}/api/Reservation`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(submissionData)
      });

      if (response.ok) {
        setSuccess(true);
      } else {
        const errorData = await response.json();
        alert('Lỗi: ' + (errorData.message || 'Có lỗi xảy ra, vui lòng thử lại.'));
      }
    } catch (err) {
      console.error(err);
    } finally {
      setSubmitting(false);
    }
  };

  // Nhóm bàn theo khu vực
  const groupedTables = tables.reduce((acc: any, table: any) => {
    const area = table.areaName || 'Khu vực chung';
    if (!acc[area]) acc[area] = [];
    acc[area].push(table);
    return acc;
  }, {});

  if (success) {
    return (
      <div className="min-h-screen bg-white flex flex-col items-center justify-center p-6 text-center">
        <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mb-6 animate-bounce">
          <CheckCircle2 className="text-green-600" size={40} />
        </div>
        <h2 className="text-2xl font-black text-gray-800 uppercase italic tracking-tighter mb-2">Đặt bàn thành công!</h2>
        <p className="text-gray-500 text-sm mb-8 font-medium">Cảm ơn bạn đã lựa chọn DOAN Restaurant. Chúng tôi sẽ sớm liên hệ xác nhận.</p>
        <button 
          onClick={() => window.location.href = '/'}
          className="bg-[#0070f4] text-white px-8 py-3 rounded-2xl font-black uppercase text-xs tracking-widest shadow-lg active:scale-95 transition-all"
        >
          QUAY LẠI THỰC ĐƠN
        </button>
      </div>
    );
  }

  return (
    <div className="pb-24 pt-6 px-4 font-sans max-w-lg mx-auto">
      <div className="mb-8">
        <h1 className="text-3xl font-black text-gray-800 uppercase italic tracking-tighter">Đặt bàn trước</h1>
        <p className="text-xs text-gray-400 font-bold uppercase tracking-widest mt-1">Trải nghiệm dịch vụ tốt nhất</p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Thông tin liên hệ */}
        <div className="bg-white rounded-[2rem] p-6 shadow-xl shadow-blue-500/5 border border-gray-100">
           <h3 className="text-[10px] font-black text-blue-600 uppercase tracking-widest mb-4">Thông tin liên hệ</h3>
           <div className="space-y-5">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Họ và tên</label>
                    <div className="relative">
                        <User className="absolute left-4 top-3.5 text-gray-300" size={18} />
                        <input
                        type="text"
                        required
                        className="w-full pl-12 pr-4 py-3.5 bg-gray-50 border-none rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/20 outline-none"
                        value={formData.customerName}
                        onChange={(e) => setFormData({...formData, customerName: e.target.value})}
                        />
                    </div>
                  </div>

                  <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Số điện thoại</label>
                    <div className="relative">
                        <Phone className="absolute left-4 top-3.5 text-gray-300" size={18} />
                        <input
                        type="tel"
                        required
                        className="w-full pl-12 pr-4 py-3.5 bg-gray-50 border-none rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/20 outline-none"
                        value={formData.customerPhone}
                        onChange={(e) => setFormData({...formData, customerPhone: e.target.value})}
                        />
                    </div>
                  </div>
              </div>

              <div>
                 <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Chọn chi nhánh</label>
                 <div className="relative">
                    <MapPin className="absolute left-4 top-3.5 text-gray-300" size={18} />
                    <select 
                      className="w-full pl-12 pr-4 py-3.5 bg-gray-50 border-none rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/20 outline-none appearance-none"
                      value={formData.branchId}
                      onChange={(e) => {
                        const b = branches.find(x => x.id === e.target.value);
                        setFormData({...formData, branchId: e.target.value, branchName: b?.name || '', tableId: '', tableName: ''});
                      }}
                    >
                       {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
                    </select>
                 </div>
              </div>
           </div>
        </div>

        {/* Thời gian & Số lượng */}
        <div className="bg-white rounded-[2rem] p-6 shadow-xl shadow-blue-500/5 border border-gray-100">
           <h3 className="text-[10px] font-black text-blue-600 uppercase tracking-widest mb-4">Thời gian & Số lượng</h3>
           <div className="grid grid-cols-2 gap-4">
              <div>
                 <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Số khách</label>
                 <div className="relative">
                    <Users className="absolute left-4 top-3.5 text-gray-300" size={18} />
                    <input 
                      type="number"
                      min="1"
                      required
                      className="w-full pl-12 pr-4 py-3.5 bg-gray-50 border-none rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/20 outline-none"
                      value={formData.numberOfGuests}
                      onChange={(e) => setFormData({...formData, numberOfGuests: parseInt(e.target.value)})}
                    />
                 </div>
              </div>
              <div>
                 <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Giờ đến</label>
                 <div className="relative">
                    <Clock className="absolute left-4 top-3.5 text-gray-300" size={18} />
                    <input 
                      type="datetime-local"
                      required
                      className="w-full pl-12 pr-2 py-3.5 bg-gray-50 border-none rounded-2xl text-[11px] font-bold focus:ring-2 focus:ring-blue-500/20 outline-none"
                      value={formData.reservationTime}
                      onChange={(e) => setFormData({...formData, reservationTime: e.target.value})}
                    />
                 </div>
              </div>
           </div>
        </div>

        {/* Chọn vị trí bàn */}
        <div className="bg-white rounded-[2rem] p-6 shadow-xl shadow-blue-500/5 border border-gray-100">
           <div className="flex justify-between items-center mb-4">
              <h3 className="text-[10px] font-black text-blue-600 uppercase tracking-widest">Chọn vị trí bàn (Tùy chọn)</h3>
              {loadingTables && <Loader2 className="animate-spin text-blue-600" size={14} />}
           </div>
           
           {tables.length === 0 && !loadingTables ? (
              <p className="text-[10px] text-gray-400 italic text-center py-4">Chi nhánh này hiện chưa có dữ liệu bàn.</p>
           ) : (
              <div className="space-y-6 max-h-[300px] overflow-y-auto pr-2 no-scrollbar">
                 {Object.keys(groupedTables).map(area => (
                    <div key={area} className="space-y-3">
                       <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest flex items-center">
                          <LayoutGrid size={12} className="mr-2" /> {area}
                       </p>
                       <div className="grid grid-cols-3 sm:grid-cols-4 gap-3">
                          {groupedTables[area].map((table: any) => (
                             <button
                                key={table.id}
                                type="button"
                                onClick={() => setFormData({...formData, tableId: table.id, tableName: table.name})}
                                className={`py-3 rounded-2xl border-2 flex flex-col items-center justify-center transition-all ${
                                   formData.tableId === table.id
                                   ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100'
                                   : 'border-gray-50 bg-gray-50/50 hover:border-blue-200'
                                }`}
                             >
                                <span className={`text-xs font-black ${formData.tableId === table.id ? 'text-blue-600' : 'text-gray-700'}`}>{table.name}</span>
                                <span className="text-[8px] font-bold text-gray-400 uppercase mt-0.5">{table.seatCount} chỗ</span>
                             </button>
                          ))}
                       </div>
                    </div>
                 ))}
              </div>
           )}
        </div>

        <div className="bg-white rounded-[2rem] p-6 shadow-xl shadow-blue-500/5 border border-gray-100">
           <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Ghi chú thêm</label>
           <div className="relative">
              <MessageSquare className="absolute left-4 top-3.5 text-gray-300" size={18} />
              <textarea
                className="w-full pl-12 pr-4 py-3.5 bg-gray-50 border-none rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/20 outline-none"
                rows={3}
                placeholder="Ví dụ: Bàn gần cửa sổ, kỷ niệm ngày cưới..."
                value={formData.note}
                onChange={(e) => setFormData({...formData, note: e.target.value})}
              />
           </div>
        </div>

        <button 
          type="submit"
          disabled={submitting}
          className="w-full bg-[#0070f4] text-white py-5 rounded-[2rem] font-black uppercase text-sm tracking-[0.2em] shadow-xl shadow-blue-500/30 active:scale-95 transition-all flex items-center justify-center space-x-3"
        >
          {submitting ? <Loader2 className="animate-spin" size={20} /> : (
            <>
              <span>XÁC NHẬN ĐẶT BÀN</span>
              <ChevronRight size={20} />
            </>
          )}
        </button>
      </form>
    </div>
  );
};

export default ReservationPage;
