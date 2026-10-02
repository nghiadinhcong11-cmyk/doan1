import React, { useState, useEffect } from 'react';
import { ChevronLeft, ChevronRight, Users, Calendar as CalendarIcon, Clock, Filter, Loader2, X, Search, Trash2, MapPin } from 'lucide-react';
import { API_URL } from '../../../config';

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

const EmployeeSchedule = () => {
  const [employees, setEmployees] = useState<any[]>([]);
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const myEmployeeId = localStorage.getItem('employeeId');
  const userRole = localStorage.getItem('userRole');
  const canSeeAll = userRole === 'admin' || userRole === 'manager';

  // Quản lý tuần hiện tại
  const [startOfWeek, setStartOfWeek] = useState(() => {
    const d = new Date();
    const day = d.getDay(), diff = d.getDate() - day + (day === 0 ? -6 : 1);
    return new Date(d.setDate(diff));
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

      const employeeParam = !canSeeAll && myEmployeeId ? `&employeeId=${myEmployeeId}` : '';
      const res = await fetch(`${API_URL}/api/WorkSchedule?startDate=${startOfWeek.toISOString()}&endDate=${endOfWeekDate.toISOString()}${employeeParam}`);
      const data = await res.json();
      setSchedules(data);

      if (canSeeAll) {
        const resEmp = await fetch(`${API_URL}/api/Employee`);
        const dataEmp = await resEmp.json();
        setEmployees(dataEmp);
      } else {
        setEmployees([{ id: myEmployeeId, fullName: localStorage.getItem('userName') || 'Tôi', position: localStorage.getItem('userPosition') }]);
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

  const changeWeek = (offset: number) => {
    const next = new Date(startOfWeek);
    next.setDate(startOfWeek.getDate() + offset * 7);
    setStartOfWeek(next);
  };

  const filteredEmployees = employees.filter(e =>
    e.fullName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const dayLabels = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ Nhật'];

  return (
    <div className="flex flex-col h-full bg-[#f0f2f5] font-sans">
      {/* Header & Filters */}
      <div className="bg-white p-4 border-b shadow-sm flex flex-wrap justify-between items-center gap-4">
         <div className="flex items-center space-x-6">
            <h2 className="font-black text-xl uppercase tracking-tighter italic text-gray-800 flex items-center">
               <CalendarIcon className="mr-2 text-blue-600" size={20}/> Lịch làm việc toàn quán
            </h2>

            <div className="flex items-center bg-gray-100 rounded-xl p-1 border border-gray-200">
               <button onClick={() => changeWeek(-1)} className="p-1.5 hover:bg-white rounded-lg transition-all text-gray-500 hover:text-blue-600"><ChevronLeft size={18}/></button>
               <span className="px-4 text-[11px] font-black text-gray-600 uppercase tracking-widest">
                  {weekDays[0].toLocaleDateString('vi-VN', {day:'2-digit', month:'2-digit'})} - {weekDays[6].toLocaleDateString('vi-VN', {day:'2-digit', month:'2-digit'})}
               </span>
               <button onClick={() => changeWeek(1)} className="p-1.5 hover:bg-white rounded-lg transition-all text-gray-500 hover:text-blue-600"><ChevronRight size={18}/></button>
            </div>
         </div>

         <div className="flex items-center space-x-3">
            <div className="relative group">
               <Search className="absolute left-3 top-2.5 h-3.5 w-3.5 text-gray-400 group-focus-within:text-blue-500" />
               <input
                  type="text"
                  placeholder="Tìm đồng nghiệp..."
                  className="pl-9 pr-4 py-2 bg-gray-50 border border-gray-100 rounded-xl text-xs outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all w-48 font-bold"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
               />
            </div>
         </div>
      </div>

      {/* Weekly Grid */}
      <div className="flex-1 overflow-auto p-4">
         {loading ? (
           <div className="h-full flex items-center justify-center"><Loader2 className="animate-spin text-blue-600" size={32}/></div>
         ) : (
           <div className="bg-white rounded-2xl shadow-xl shadow-blue-500/5 border border-gray-200 overflow-hidden min-w-[1000px]">
              <table className="w-full border-collapse">
                 <thead>
                    <tr className="bg-gray-50 border-b">
                       <th className="p-4 border-r w-48 text-left text-[10px] font-black text-gray-400 uppercase tracking-widest sticky left-0 bg-gray-50 z-10">Nhân viên</th>
                       {weekDays.map((date, idx) => (
                         <th key={idx} className={`p-3 border-r text-center ${date.toDateString() === new Date().toDateString() ? 'bg-blue-50/50' : ''}`}>
                            <p className="text-[10px] font-black text-gray-400 uppercase tracking-tighter">{dayLabels[idx]}</p>
                            <p className={`text-sm font-black ${date.toDateString() === new Date().toDateString() ? 'text-blue-600' : 'text-gray-700'}`}>
                               {date.toLocaleDateString('vi-VN', {day:'2-digit', month:'2-digit'})}
                            </p>
                         </th>
                       ))}
                    </tr>
                 </thead>
                 <tbody>
                    {filteredEmployees.map(emp => (
                      <tr key={emp.id} className={`border-b last:border-0 hover:bg-gray-50 transition-colors ${emp.id === myEmployeeId ? 'bg-blue-50/10' : ''}`}>
                         <td className={`p-4 border-r font-bold text-gray-700 sticky left-0 z-10 shadow-[2px_0_5px_rgba(0,0,0,0.02)] ${emp.id === myEmployeeId ? 'bg-blue-50' : 'bg-white'}`}>
                            <div className="flex items-center">
                               <div className={`w-7 h-7 rounded-lg flex items-center justify-center mr-3 text-[11px] font-black ${emp.id === myEmployeeId ? 'bg-blue-600 text-white shadow-md' : 'bg-blue-100 text-blue-600'}`}>
                                  {emp.fullName.charAt(0).toUpperCase()}
                               </div>
                               <div>
                                  <p className={`text-xs ${emp.id === myEmployeeId ? 'text-blue-700 font-black' : ''}`}>{emp.fullName} {emp.id === myEmployeeId && '(Tôi)'}</p>
                                  <div className="flex flex-col space-y-0.5">
                                     <p className="text-[9px] text-gray-400 font-bold uppercase">{emp.position || 'Nhân viên'}</p>
                                     <p className="text-[9px] text-blue-500 font-bold flex items-center">
                                        <MapPin size={10} className="mr-1"/> {emp.branchName || 'Toàn hệ thống'}
                                     </p>
                                  </div>
                               </div>
                            </div>
                         </td>
                         {weekDays.map((date, idx) => {
                            const dateStr = date.toISOString().split('T')[0];
                            const daySchedules = schedules.filter(s => s.employeeId === emp.id && s.date.split('T')[0] === dateStr);

                            return (
                               <td key={idx} className={`p-2 border-r align-top min-h-[100px] h-32 group transition-all ${date.toDateString() === new Date().toDateString() ? 'bg-blue-50/20' : ''}`}>
                                  <div className="space-y-1.5">
                                     {daySchedules.map(s => (
                                       <div key={s.id} className={`p-2 rounded-xl text-[10px] font-black border relative shadow-sm transition-all hover:scale-105 ${
                                          s.shiftName === 'Sáng' ? 'bg-blue-50 text-blue-700 border-blue-100' :
                                          s.shiftName === 'Chiều' ? 'bg-orange-50 text-orange-700 border-orange-100' :
                                          'bg-purple-50 text-purple-700 border-purple-100'
                                       }`}>
                                          <div className="flex justify-between items-start">
                                             <span className="uppercase tracking-tighter">Ca {s.shiftName}</span>
                                          </div>
                                          <p className="mt-1 opacity-70 flex items-center"><Clock size={10} className="mr-1"/> {s.startTime} - {s.endTime}</p>
                                       </div>
                                     ))}
                                  </div>
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

      {/* Legend */}
      <div className="p-4 bg-white border-t flex items-center space-x-6">
         <div className="flex items-center space-x-2">
            <div className="w-3 h-3 rounded-full bg-blue-500"></div>
            <span className="text-[10px] font-bold text-gray-500 uppercase">Ca Sáng</span>
         </div>
         <div className="flex items-center space-x-2">
            <div className="w-3 h-3 rounded-full bg-orange-500"></div>
            <span className="text-[10px] font-bold text-gray-500 uppercase">Ca Chiều</span>
         </div>
         <div className="flex items-center space-x-2">
            <div className="w-3 h-3 rounded-full bg-purple-500"></div>
            <span className="text-[10px] font-bold text-gray-500 uppercase">Ca Tối</span>
         </div>
         <div className="flex-1"></div>
         <div className="text-[10px] text-gray-400 font-bold italic">* Lịch làm việc do quản lý sắp xếp. Vui lòng liên hệ nếu có thay đổi.</div>
      </div>
    </div>
  );
};

export default EmployeeSchedule;

