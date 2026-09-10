import React, { useState, useEffect, useMemo } from 'react';
import { Search, Calendar, Clock, LogIn, LogOut, Loader2, User, CheckCircle2, AlertCircle, Filter, Download, QrCode, X, Printer, MapPin, Building2, Save, ChevronRight } from 'lucide-react';
import { API_URL } from '../../../config';

interface AttendanceRecord {
  id: string;
  employeeName: string;
  employeeCode: string;
  checkInTime: string;
  checkOutTime?: string;
  branchName: string; // Chi nhánh làm việc (quét QR)
  branchId?: string;
  homeBranchName?: string; // Chi nhánh trực thuộc của nhân viên
  status: string;
  note?: string;
}

interface Branch {
  id: string;
  name: string;
}

const AttendanceManagement = () => {
  const [records, setRecords] = useState<AttendanceRecord[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterBranch, setFilterBranch] = useState('all');
  const [filterStatus, setFilterStatus] = useState('all'); // all, Đúng giờ, Muộn

  const [isQrModalOpen, setIsQrModalOpen] = useState(false);
  const [selectedBranchForQr, setSelectedBranchForQr] = useState<Branch | null>(null);
  const [showInlineQr, setShowInlineQr] = useState(false);

  const fetchAttendances = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${API_URL}/api/Attendance`);
      const data = await response.json();

      // Giả sử API chưa trả về homeBranchName, ta có thể bổ sung logic map nếu cần
      // Hoặc cập nhật API sau. Ở đây ta chuẩn bị sẵn interface.
      setRecords(data);
    } catch (err) {
      console.error("Lỗi lấy dữ liệu chấm công:", err);
    } finally {
      setLoading(false);
    }
  };

  const fetchBranches = async () => {
    try {
      const response = await fetch(`${API_URL}/api/Branch`);
      const data = await response.json();
      setBranches(data);

      const savedBranchId = localStorage.getItem('attendance_qr_branch_id');
      if (savedBranchId) {
        const b = data.find((x: Branch) => x.id === savedBranchId);
        if (b) setSelectedBranchForQr(b);
        else if (data.length > 0) setSelectedBranchForQr(data[0]);
      } else if (data.length > 0) {
        setSelectedBranchForQr(data[0]);
      }
    } catch (err) {
      console.error(err);
    }
  };

  useEffect(() => {
    fetchAttendances();
    fetchBranches();

    const lastDate = localStorage.getItem('attendance_qr_date');
    const today = new Date().toLocaleDateString();
    if (lastDate === today) {
      setShowInlineQr(true);
    } else {
      setShowInlineQr(false);
      localStorage.removeItem('attendance_qr_date');
    }
  }, []);

  const filteredRecords = useMemo(() => {
    return records.filter(r => {
      const matchSearch = r.employeeName.toLowerCase().includes(searchTerm.toLowerCase());
      const matchBranch = filterBranch === 'all' || r.branchId === filterBranch || r.branchName === branches.find(b=>b.id===filterBranch)?.name;
      const matchStatus = filterStatus === 'all' || r.status === filterStatus;

      return matchSearch && matchBranch && matchStatus;
    });
  }, [records, searchTerm, filterBranch, filterStatus, branches]);

  const generateAttendanceQr = () => {
    if (!selectedBranchForQr) return '';
    const today = new Date().toLocaleDateString('en-GB');
    const qrData = encodeURIComponent(JSON.stringify({
      type: 'ATTENDANCE_POINT',
      branchId: selectedBranchForQr.id,
      branchName: selectedBranchForQr.name,
      date: today
    }));
    return `https://api.qrserver.com/v1/create-qr-code/?size=400x400&qzone=4&ecc=M&data=${qrData}`;
  };

  const handleActivateQr = () => {
    const today = new Date().toLocaleDateString();
    localStorage.setItem('attendance_qr_date', today);
    if (selectedBranchForQr) {
      localStorage.setItem('attendance_qr_branch_id', selectedBranchForQr.id);
    }
    setShowInlineQr(true);
    setIsQrModalOpen(true);
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f8f9fa] text-[13px] font-sans">
      {/* SIDEBAR FILTER */}
      <div className="w-72 bg-white border-r p-6 space-y-8 shadow-sm overflow-y-auto">
        <h2 className="font-black text-xl text-gray-800 uppercase tracking-tighter italic flex items-center">
           <Clock className="mr-2 text-blue-600" size={24}/> Chấm công
        </h2>

        <div className="space-y-6">
           {/* Branch Filter */}
           <div>
              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">Lọc theo cơ sở</p>
              <select
                className="w-full bg-gray-50 border border-gray-100 rounded-xl py-2 px-3 outline-none focus:ring-2 focus:ring-blue-500/10 focus:border-blue-500 font-bold text-gray-700"
                value={filterBranch}
                onChange={(e) => setFilterBranch(e.target.value)}
              >
                 <option value="all">Tất cả chi nhánh</option>
                 {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
              </select>
           </div>

           {/* Status Filter */}
           <div>
              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">Trạng thái vào ca</p>
              <div className="space-y-2">
                 {['all', 'Đúng giờ', 'Muộn'].map(s => (
                   <label key={s} className="flex items-center cursor-pointer group">
                      <input
                        type="radio"
                        className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                        checked={filterStatus === s}
                        onChange={() => setFilterStatus(s)}
                      />
                      <span className={`font-bold transition-colors ${filterStatus === s ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-500'}`}>
                         {s === 'all' ? 'Tất cả trạng thái' : s}
                      </span>
                   </label>
                 ))}
              </div>
           </div>
        </div>

        <div className="pt-8 border-t space-y-4">
           <div className="bg-blue-50 p-4 rounded-3xl border border-blue-100">
              <p className="text-blue-700 font-black text-[10px] flex items-center mb-2 uppercase tracking-widest">
                 <QrCode size={14} className="mr-2"/> QR Điểm danh
              </p>

              {showInlineQr && selectedBranchForQr ? (
                <div className="mt-3 space-y-3 animate-in fade-in zoom-in duration-500">
                   <div className="bg-white p-2 rounded-2xl shadow-sm border border-blue-100 group relative overflow-hidden">
                      <img src={generateAttendanceQr()} alt="QR" className="w-full h-auto" />
                      <div className="absolute inset-0 bg-blue-600/5 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center space-x-2">
                         <button onClick={() => setIsQrModalOpen(true)} className="bg-white p-2 rounded-full shadow-md text-blue-600 hover:scale-110 transition-transform">
                            <QrCode size={18} />
                         </button>
                      </div>
                   </div>
                   <div className="text-center">
                      <p className="text-[10px] text-blue-800 font-black uppercase tracking-tighter truncate px-2">
                         {selectedBranchForQr.name}
                      </p>
                      <button
                        onClick={() => setIsQrModalOpen(true)}
                        className="mt-2 text-[9px] text-blue-500 font-black uppercase tracking-widest hover:underline decoration-2 underline-offset-4"
                      >
                        [ Đổi cơ sở / Tạo lại ]
                      </button>
                   </div>
                </div>
              ) : (
                <>
                  <p className="text-[10px] text-blue-600/70 leading-relaxed italic font-medium">
                     Tạo mã QR riêng cho từng cơ sở để nhân viên quét khi bắt đầu ca làm.
                  </p>
                  <button
                    onClick={handleActivateQr}
                    className="w-full mt-4 py-3 bg-[#0070f4] text-white rounded-2xl text-[10px] font-black uppercase tracking-widest hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/20 active:scale-95"
                  >
                    KÍCH HOẠT QR
                  </button>
                </>
              )}
           </div>
        </div>
      </div>

      {/* MAIN CONTENT */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="bg-white p-4 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-96">
            <Search className="absolute left-4 top-3 h-4 w-4 text-gray-300" />
            <input
              type="text"
              className="w-full pl-12 pr-4 py-2.5 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:ring-2 focus:ring-blue-500/10 focus:border-blue-500 font-bold text-xs"
              placeholder="Nhập tên nhân viên cần tra cứu..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex space-x-3">
            <button onClick={fetchAttendances} className="bg-white border-2 border-gray-100 text-gray-600 px-6 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-gray-50 transition-all active:scale-95">
              LÀM MỚI
            </button>
            <button className="bg-white border-2 border-gray-100 text-gray-600 px-6 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-gray-50 transition-all active:scale-95 flex items-center">
              <Download size={14} className="mr-2 text-gray-400"/> XUẤT FILE
            </button>
          </div>
        </div>

        <div className="flex-1 overflow-auto p-8 bg-[#f8f9fa]">
           <div className="bg-white rounded-[1rem] shadow-2xl shadow-blue-500/5 border border-gray-200 overflow-hidden">
              <table className="w-full text-left border-collapse table-fixed">
                 <thead className="bg-gray-100 border-b-2 border-gray-200 text-gray-600 font-black text-[11px] uppercase tracking-wider">
                    <tr>
                       <th className="px-4 py-4 border-r border-gray-200 w-32 text-center">Ngày làm</th>
                       <th className="px-4 py-4 border-r border-gray-200 w-56">Nhân sự</th>
                       <th className="px-4 py-4 border-r border-gray-200 w-48">CN Trực thuộc</th>
                       <th className="px-4 py-4 border-r border-gray-200 w-48">CN Làm việc</th>
                       <th className="px-4 py-4 border-r border-gray-200 w-36 text-center">Giờ vào</th>
                       <th className="px-4 py-4 border-r border-gray-200 w-36 text-center">Giờ ra</th>
                       <th className="px-4 py-4 border-r border-gray-200 w-32 text-center">Xác nhận</th>
                       <th className="px-4 py-4 w-48">Ghi chú</th>
                    </tr>
                 </thead>
                 <tbody className="divide-y divide-gray-200 text-[12px]">
                    {loading ? (
                      <tr><td colSpan={8} className="py-20 text-center">
                         <div className="flex flex-col items-center justify-center">
                            <Loader2 className="animate-spin text-blue-600 mb-2" size={32} />
                            <p className="text-[10px] font-black uppercase tracking-widest text-gray-400">Đang tải bảng công...</p>
                         </div>
                      </td></tr>
                    ) : filteredRecords.length === 0 ? (
                      <tr><td colSpan={8} className="py-32 text-center text-gray-300 italic font-bold uppercase tracking-widest text-[10px]">Chưa có dữ liệu chấm công phù hợp</td></tr>
                    ) : filteredRecords.map(r => (
                      <tr key={r.id} className="hover:bg-blue-50/50 transition-colors group">
                         <td className="px-4 py-3 border-r border-gray-100 text-center font-bold text-gray-500">
                            {new Date(r.checkInTime).toLocaleDateString('vi-VN')}
                         </td>
                         <td className="px-4 py-3 border-r border-gray-100 font-black text-gray-700 uppercase truncate">
                            {r.employeeName}
                         </td>
                         <td className="px-4 py-3 border-r border-gray-100 text-gray-500 italic font-medium">
                            {r.homeBranchName || '---'}
                         </td>
                         <td className="px-4 py-3 border-r border-gray-100 font-bold text-blue-600 italic">
                            {r.branchName}
                         </td>
                         <td className="px-4 py-3 border-r border-gray-100 text-center">
                            <span className="bg-green-50 text-green-700 px-3 py-1 rounded-md font-black border border-green-100">
                               {new Date(r.checkInTime).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})}
                            </span>
                         </td>
                         <td className="px-4 py-3 border-r border-gray-100 text-center">
                            {r.checkOutTime ? (
                               <span className="bg-orange-50 text-orange-700 px-3 py-1 rounded-md font-black border border-orange-100">
                                  {new Date(r.checkOutTime).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})}
                               </span>
                            ) : (
                               <span className="text-gray-300 font-bold italic lowercase opacity-50">đang trực...</span>
                            )}
                         </td>
                         <td className="px-4 py-3 border-r border-gray-100 text-center">
                            <span className={`inline-flex items-center px-2 py-0.5 rounded text-[10px] font-black uppercase shadow-sm ${r.status === 'Đúng giờ' ? 'bg-blue-600 text-white' : 'bg-gray-800 text-white'}`}>
                               {r.status}
                            </span>
                         </td>
                         <td className="px-4 py-3 text-gray-400 font-medium italic truncate">
                            {r.note || '---'}
                         </td>
                      </tr>
                    ))}
                 </tbody>
              </table>
           </div>
        </div>
      </div>

      {/* QR ATTENDANCE MODAL */}
      {isQrModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-md animate-in fade-in duration-300">
           <div className="bg-white rounded-[3rem] p-12 max-w-sm w-full text-center shadow-2xl relative overflow-hidden">
              <div className="absolute top-0 left-0 w-full h-3 bg-[#0070f4]"></div>
              <button onClick={() => setIsQrModalOpen(false)} className="absolute top-6 right-6 text-gray-300 hover:text-gray-800 hover:rotate-90 transition-all"><X size={32}/></button>

              <span className="bg-blue-50 text-[#0070f4] text-[10px] font-black px-6 py-2 rounded-full uppercase tracking-[0.2em] border border-blue-100 italic">Security Point</span>
              <h3 className="text-3xl font-black text-gray-800 uppercase tracking-tighter mt-8 mb-8 italic">MÃ QR CHẤM CÔNG</h3>

              <div className="mb-8 space-y-3">
                 <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest block text-left ml-4 opacity-70">Xác nhận cơ sở phát hành</label>
                 <div className="relative">
                    <select
                      className="w-full border-2 border-gray-100 rounded-2xl py-4 px-6 outline-none focus:border-blue-500 font-black text-gray-700 bg-gray-50 appearance-none transition-all shadow-inner"
                      value={selectedBranchForQr?.id}
                      onChange={(e) => {
                         const b = branches.find(x => x.id === e.target.value);
                         if (b) {
                           setSelectedBranchForQr(b);
                           localStorage.setItem('attendance_qr_branch_id', b.id);
                         }
                      }}
                    >
                       {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
                    </select>
                    <ChevronRight size={18} className="absolute right-6 top-5 rotate-90 text-gray-300" />
                 </div>
              </div>

              <div className="bg-gray-50 p-8 rounded-[3rem] border-4 border-white shadow-2xl mb-10 group relative transition-all hover:bg-white">
                 <img src={generateAttendanceQr()} alt="QR Attendance" className="w-full aspect-square" />
                 <div className="absolute inset-0 bg-blue-600/10 opacity-0 group-hover:opacity-100 transition-opacity rounded-[3rem] flex items-center justify-center">
                    <button className="bg-white text-blue-600 p-4 rounded-full shadow-2xl active:scale-90 transition-all"><Download size={28}/></button>
                 </div>
              </div>

              <div className="flex space-x-4">
                 <button className="flex-1 py-5 bg-gray-100 text-gray-600 rounded-2xl font-black text-[10px] uppercase tracking-widest hover:bg-gray-200 transition-all flex items-center justify-center active:scale-95">
                    <Save size={18} className="mr-2" /> TẢI VỀ
                 </button>
                 <button className="flex-[2] py-5 bg-[#0070f4] text-white rounded-2xl font-black text-[10px] uppercase tracking-widest shadow-xl shadow-blue-500/30 hover:bg-blue-700 transition-all flex items-center justify-center active:scale-95">
                    <Printer size={18} className="mr-2" /> IN NHÃN QR
                 </button>
              </div>
              <p className="mt-8 text-[10px] text-gray-400 font-medium italic leading-relaxed px-4">Lưu ý: Mã QR này có hiệu lực trong ngày hôm nay cho chi nhánh đã chọn.</p>
           </div>
        </div>
      )}
    </div>
  );
};

export default AttendanceManagement;

