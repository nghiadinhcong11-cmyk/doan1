import React, { useState, useEffect, useMemo } from 'react';
import { ChevronLeft, ChevronRight, Plus, Users, Calendar as CalendarIcon, Clock, Filter, Loader2, X, Search, Trash2, MapPin, Store } from 'lucide-react';
import { API_URL } from '../../../config';
import { notifyFeedback } from '../../../components/ui';

interface Schedule {
  id: string;
  employeeId: string;
  employeeName: string;
  date: string;
  shiftName: string;
  startTime: string;
  endTime: string;
  branchId?: string;
  branchName?: string;
}

interface WorkSchedulePageProps {
  readOnly?: boolean;
}

const WorkSchedulePage: React.FC<WorkSchedulePageProps> = ({ readOnly = false }) => {
  const userRole = localStorage.getItem('userRole');
  const assignedBranchId = localStorage.getItem('selectedBranchId');
  const [employees, setEmployees] = useState<any[]>([]);
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [branches, setBranches] = useState<any[]>([]);
  const [filterBranchId, setFilterBranchId] = useState<string>('all');

  // Quản lý tuần hiện tại
  const [startOfWeek, setStartOfWeek] = useState(() => {
    const d = new Date();
    const day = d.getDay(), diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return new Date(d.setDate(diff));
  });

  const [newSchedule, setNewSchedule] = useState({
    employeeId: '',
    date: new Date().toISOString().split('T')[0],
    shiftName: 'Sáng',
    startTime: '08:00',
    endTime: '12:00'
  });

  const getWeekDays = () => {
    const days = [];
    for (let i = 0; i < 7; i++) {
      const d = new Date(startOfWeek);
      d.setDate(startOfWeek.getDate() + i);
      days.push(d);
    }
    return days;
  };

  const weekDays = getWeekDays();

  const fetchSchedules = async () => {
    try {
      setLoading(true);
      const endOfWeekDate = new Date(startOfWeek);
      endOfWeekDate.setDate(startOfWeek.getDate() + 6);

      const res = await fetch(`${API_URL}/api/WorkSchedule?startDate=${startOfWeek.toISOString()}&endDate=${endOfWeekDate.toISOString()}`);
      const data = await res.json();
      setSchedules(data);

      const resEmp = await fetch(`${API_URL}/api/Employee`);
      const dataEmp = await resEmp.json();
      setEmployees(dataEmp);

      const resBranch = await fetch(`${API_URL}/api/Branch`);
      const dataBranch = await resBranch.json();
      setBranches(userRole === 'manager'
        ? dataBranch.filter((branch: any) => branch.id === assignedBranchId)
        : dataBranch);

      const savedBranchId = localStorage.getItem('selectedBranchId');
      if (savedBranchId && filterBranchId === 'all') {
        setFilterBranchId(savedBranchId);
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSchedules();
  }, [startOfWeek]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    const emp = employees.find(e => e.id === newSchedule.employeeId);

    try {
      const res = await fetch(`${API_URL}/api/WorkSchedule`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...newSchedule,
          employeeName: emp?.fullName,
          branchId: emp?.branchId,
          branchName: emp?.branchName
        })
      });
      if (res.ok) {
        setIsModalOpen(false);
        fetchSchedules();
      }
    } catch (err) {
      notifyFeedback("Lỗi lưu lịch làm");
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Xóa lịch làm này?")) return;
    try {
      await fetch(`${API_URL}/api/WorkSchedule/${id}`, { method: 'DELETE' });
      fetchSchedules();
    } catch (err) { console.error(err); }
  };

  const changeWeek = (offset: number) => {
    const next = new Date(startOfWeek);
    next.setDate(startOfWeek.getDate() + offset * 7);
    setStartOfWeek(next);
  };

  const filteredEmployees = useMemo(() => {
    return employees.filter(e => {
        const matchName = e.fullName.toLowerCase().includes(searchTerm.toLowerCase());
        const matchBranch = filterBranchId === 'all' || e.branchId === filterBranchId;
        return matchName && matchBranch;
    });
  }, [employees, searchTerm, filterBranchId]);

  const dayLabels = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ Nhật'];

  return (
    <div className="flex flex-col h-[calc(100vh-48px)] bg-[#f8f9fa] font-sans">
      {/* Header & Filters */}
      <div className="bg-white p-4 border-b shadow-sm flex flex-wrap justify-between items-center gap-4">
         <div className="flex items-center space-x-6">
            <h2 className="font-black text-xl uppercase tracking-tighter italic text-gray-800 flex items-center">
               <CalendarIcon className="mr-3 text-blue-600" size={24}/> Lịch làm việc tuần
            </h2>

            <div className="flex items-center bg-gray-50 rounded-xl p-1 border border-gray-200 shadow-inner">
               <button onClick={() => changeWeek(-1)} className="p-2 hover:bg-white rounded-lg transition-all text-gray-400 hover:text-blue-600"><ChevronLeft size={20}/></button>
               <span className="px-6 text-[10px] font-black text-gray-700 uppercase tracking-widest">
                  {weekDays[0].toLocaleDateString('vi-VN', {day:'2-digit', month:'2-digit'})} — {weekDays[6].toLocaleDateString('vi-VN', {day:'2-digit', month:'2-digit'})}
               </span>
               <button onClick={() => changeWeek(1)} className="p-2 hover:bg-white rounded-lg transition-all text-gray-400 hover:text-blue-600"><ChevronRight size={20}/></button>
            </div>
         </div>

         <div className="flex items-center space-x-3">
            <div className="relative group">
               <Filter className="absolute left-3 top-2.5 h-3.5 w-3.5 text-gray-300 group-focus-within:text-blue-500" />
               <select
                  className="pl-9 pr-4 py-2 bg-gray-50 border border-gray-100 rounded-xl text-xs outline-none focus:ring-2 focus:ring-blue-500/10 focus:border-blue-500 transition-all font-black text-gray-700 uppercase tracking-tighter"
                  value={filterBranchId}
                  onChange={(e) => setFilterBranchId(e.target.value)}
               >
                  {userRole === 'admin' && <option value="all">Toàn bộ chi nhánh</option>}
                  {branches.map(b => (
                    <option key={b.id} value={b.id}>{b.name}</option>
                  ))}
               </select>
            </div>
            <div className="relative group">
               <Search className="absolute left-3 top-2.5 h-3.5 w-3.5 text-gray-300 group-focus-within:text-blue-500" />
               <input
                  type="text"
                  placeholder="Tìm nhân viên..."
                  className="pl-9 pr-4 py-2 bg-gray-50 border border-gray-100 rounded-xl text-xs outline-none focus:ring-2 focus:ring-blue-500/10 focus:border-blue-500 transition-all w-48 font-bold"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
               />
            </div>
            {!readOnly && (
              <button
                onClick={() => {
                  setNewSchedule({
                    employeeId: '',
                    date: new Date().toISOString().split('T')[0],
                    shiftName: 'Sáng',
                    startTime: '08:00',
                    endTime: '12:00'
                  });
                  setIsModalOpen(true);
                }}
                className="bg-[#0070f4] text-white px-8 py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest shadow-lg shadow-blue-500/30 hover:bg-blue-700 active:scale-95 transition-all flex items-center"
              >
                 <Plus size={18} className="mr-2" /> XẾP CA LÀM
              </button>
            )}
         </div>
      </div>

      {/* Weekly Grid */}
      <div className="flex-1 overflow-auto p-8 bg-[#f8f9fa]">
         {loading ? (
           <div className="h-full flex flex-col items-center justify-center text-blue-600">
              <Loader2 className="animate-spin mb-2" size={32}/>
              <p className="text-[10px] font-black uppercase tracking-widest">Đang tải lịch biểu...</p>
           </div>
         ) : (
           <div className="bg-white rounded-[2.5rem] shadow-2xl shadow-blue-500/5 border border-white overflow-hidden min-w-[1200px]">
              <table className="w-full border-collapse">
                 <thead>
                    <tr className="bg-gray-50/50 border-b">
                       <th className="p-6 border-r w-64 text-left text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] sticky left-0 bg-gray-50 z-10 shadow-sm">Thông tin nhân sự</th>
                       {weekDays.map((date, idx) => (
                         <th key={idx} className={`p-4 border-r text-center ${date.toDateString() === new Date().toDateString() ? 'bg-blue-50/30' : ''}`}>
                            <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">{dayLabels[idx]}</p>
                            <p className={`text-sm font-black ${date.toDateString() === new Date().toDateString() ? 'text-blue-600' : 'text-gray-700'}`}>
                               {date.toLocaleDateString('vi-VN', {day:'2-digit', month:'2-digit'})}
                            </p>
                         </th>
                       ))}
                    </tr>
                 </thead>
                 <tbody className="divide-y divide-gray-50">
                    {filteredEmployees.map(emp => (
                      <tr key={emp.id} className="hover:bg-gray-50/50 transition-colors group">
                         <td className="p-6 border-r font-bold text-gray-700 sticky left-0 bg-white z-10 shadow-xl shadow-gray-100">
                            <div className="flex items-center">
                               <div className="w-10 h-10 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center mr-4 text-xs font-black shadow-sm border border-blue-100 uppercase italic">
                                  {emp.fullName.charAt(0)}
                               </div>
                               <div>
                                  <p className="text-sm font-black text-gray-800 tracking-tight leading-none mb-1.5 uppercase">{emp.fullName}</p>
                                  <div className="flex flex-col space-y-1">
                                     <p className="text-[8px] font-black text-gray-400 uppercase tracking-widest">{emp.position || 'Nhân sự'}</p>
                                     <p className="text-[8px] text-blue-500 font-black flex items-center uppercase tracking-tighter">
                                        <MapPin size={10} className="mr-1 opacity-50"/> {emp.branchName || 'Tổng hệ thống'}
                                     </p>
                                  </div>
                               </div>
                            </div>
                         </td>
                         {weekDays.map((date, idx) => {
                            const dateStr = date.toISOString().split('T')[0];
                            const daySchedules = schedules.filter(s => s.employeeId === emp.id && s.date.split('T')[0] === dateStr);

                            return (
                               <td key={idx} className={`p-3 border-r align-top min-h-[120px] h-40 group/cell transition-all ${date.toDateString() === new Date().toDateString() ? 'bg-blue-50/10' : ''}`}>
                                  <div className="space-y-2">
                                     {daySchedules.map(s => (
                                       <div key={s.id} className={`group/item p-3 rounded-2xl text-[9px] font-black border relative shadow-sm transition-all hover:scale-105 hover:shadow-lg ${
                                          s.shiftName === 'Sáng' ? 'bg-blue-50 text-blue-700 border-blue-100' :
                                          s.shiftName === 'Chiều' ? 'bg-orange-50 text-orange-700 border-orange-100' :
                                          'bg-purple-50 text-purple-700 border-purple-100'
                                       }`}>
                                          <div className="flex justify-between items-start mb-1">
                                             <span className="uppercase tracking-widest italic opacity-80">Ca {s.shiftName}</span>
                                             {!readOnly && (
                                               <button onClick={(e) => { e.stopPropagation(); handleDelete(s.id); }} className="opacity-0 group-hover/item:opacity-100 text-blue-400 hover:text-blue-600 transition-opacity bg-white p-0.5 rounded-full shadow-sm">
                                                  <X size={10}/>
                                               </button>
                                             )}
                                          </div>
                                          <p className="flex items-center text-gray-600 font-black"><Clock size={10} className="mr-1.5 opacity-40"/> {s.startTime} — {s.endTime}</p>
                                       </div>
                                     ))}
                                  </div>
                                  {!readOnly && (
                                    <button
                                       onClick={() => {
                                          setNewSchedule({...newSchedule, employeeId: emp.id, date: dateStr});
                                          setIsModalOpen(true);
                                       }}
                                       className="w-full mt-3 py-2 border-2 border-dashed border-gray-100 rounded-2xl text-gray-200 opacity-0 group-hover/cell:opacity-100 hover:border-blue-200 hover:text-blue-500 transition-all flex items-center justify-center bg-transparent"
                                    >
                                       <Plus size={16}/>
                                    </button>
                                  )}
                               </td>
                            );
                         })}
                      </tr>
                    ))}
                 </tbody>
              </table>
           </div>
         )}
      </div>

      {/* Modal FORM */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-md animate-in fade-in duration-200">
           <form onSubmit={handleSave} className="bg-white w-full max-w-md rounded-[3rem] overflow-hidden shadow-2xl animate-in zoom-in-95 duration-200 border border-white">
              <div className="bg-[#0070f4] p-8 text-white flex justify-between items-center">
                 <div className="flex items-center space-x-3">
                    <CalendarIcon size={24}/>
                    <div>
                       <h3 className="font-black uppercase italic tracking-tighter text-lg leading-none">Xếp ca làm mới</h3>
                       <p className="text-[10px] font-bold opacity-70 uppercase tracking-widest mt-1">Hệ thống phân lịch thông minh</p>
                    </div>
                 </div>
                 <button type="button" onClick={() => setIsModalOpen(false)} className="hover:rotate-90 transition-transform bg-white/10 p-2 rounded-full"><X size={24}/></button>
              </div>
              <div className="p-10 space-y-8">
                 <div className="space-y-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Nhân viên trực ca *</label>
                    <select className="w-full border-b-2 border-gray-100 py-2 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent text-sm" value={newSchedule.employeeId} onChange={e => {
                       setNewSchedule({...newSchedule, employeeId: e.target.value});
                    }} required>
                       <option value="">-- CHỌN NHÂN SỰ --</option>
                       {employees.map(e => <option key={e.id} value={e.id}>{e.fullName}</option>)}
                    </select>
                 </div>

                 <div className="space-y-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Ngày làm việc *</label>
                    <input type="date" className="w-full border-b-2 border-gray-100 py-2 outline-none font-black text-gray-700 focus:border-blue-500 transition-all text-sm" value={newSchedule.date} onChange={e => setNewSchedule({...newSchedule, date: e.target.value})} required/>
                 </div>

                 <div className="grid grid-cols-2 gap-10">
                    <div className="space-y-2">
                       <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Buổi làm</label>
                       <select className="w-full border-b-2 border-gray-100 py-2 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent text-sm" value={newSchedule.shiftName} onChange={e => {
                          const val = e.target.value;
                          let start = '08:00', end = '12:00';
                          if (val === 'Chiều') { start = '13:00'; end = '17:00'; }
                          else if (val === 'Tối') { start = '18:00'; end = '22:00'; }
                          setNewSchedule({...newSchedule, shiftName: val, startTime: start, endTime: end});
                       }}>
                          <option>Sáng</option><option>Chiều</option><option>Tối</option>
                       </select>
                    </div>
                    <div className="space-y-2">
                       <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Khung giờ</label>
                       <div className="flex items-center space-x-3 pt-2">
                          <input type="text" className="w-full border-b-2 border-gray-100 py-1 text-center text-[11px] font-black text-gray-700 outline-none focus:border-blue-500" value={newSchedule.startTime} onChange={e => setNewSchedule({...newSchedule, startTime: e.target.value})}/>
                          <span className="text-gray-300">—</span>
                          <input type="text" className="w-full border-b-2 border-gray-100 py-1 text-center text-[11px] font-black text-gray-700 outline-none focus:border-blue-500" value={newSchedule.endTime} onChange={e => setNewSchedule({...newSchedule, endTime: e.target.value})}/>
                       </div>
                    </div>
                 </div>
                 <button type="submit" className="w-full py-5 bg-[#0070f4] text-white rounded-[2rem] font-black uppercase tracking-[0.2em] text-[10px] shadow-2xl shadow-blue-500/30 hover:bg-blue-700 transition-all active:scale-95 mt-6">XÁC NHẬN LƯU LỊCH BIỂU</button>
              </div>
           </form>
        </div>
      )}
    </div>
  );
};

export default WorkSchedulePage;

