import React, { useState, useEffect } from 'react';
import { User, Phone, MapPin, Briefcase, Calendar, DollarSign, LogOut, CheckCircle2, Shield, Loader2 } from 'lucide-react';
import { API_URL } from '../../../config';

interface Employee {
  id: string;
  employeeCode: string;
  fullName: string;
  phoneNumber?: string;
  position?: string;
  department?: string;
  branchName?: string;
  startDate: string;
  username?: string;
}

const EmployeeProfile = ({ userName, onLogout }: { userName: string, onLogout: () => void }) => {
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const empId = localStorage.getItem('employeeId');
        if (!empId) return;

        const response = await fetch(`${API_URL}/api/Employee/${empId}`);
        if (response.ok) {
          const data = await response.json();
          setEmployee(data);
        }
      } catch (err) {
        console.error("Lỗi tải hồ sơ:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, []);

  if (loading) return (
    <div className="flex items-center justify-center h-screen bg-[#f0f2f5]">
       <Loader2 className="animate-spin text-blue-600" size={32} />
    </div>
  );

  return (
    <div className="min-h-screen bg-[#f0f2f5] p-6 font-sans">
      <div className="max-w-2xl mx-auto space-y-6">
        {/* Profile Card */}
        <div className="bg-white rounded-3xl shadow-xl overflow-hidden border border-gray-100">
           <div className="h-32 bg-gradient-to-r from-blue-600 to-indigo-700 relative">
              <div className="absolute -bottom-10 left-8 flex items-end">
                 <div className="w-24 h-24 bg-white rounded-2xl p-1 shadow-xl">
                    <div className="w-full h-full bg-blue-100 rounded-xl flex items-center justify-center text-blue-600">
                       <User size={40} />
                    </div>
                 </div>
                 <div className="ml-5 mb-2">
                    <h2 className="text-xl font-bold text-gray-800">{employee?.fullName || userName}</h2>
                    <p className="text-xs text-blue-600 font-bold uppercase tracking-wider">{employee?.position || 'Nhân viên'}</p>
                 </div>
              </div>
           </div>

           <div className="pt-16 p-8 space-y-8">
              <div className="grid grid-cols-2 gap-x-12 gap-y-6">
                 <div>
                    <label className="block text-[10px] font-bold text-gray-400 uppercase mb-1">Mã nhân viên</label>
                    <p className="text-sm font-bold text-gray-700">{employee?.employeeCode || '---'}</p>
                 </div>
                 <div>
                    <label className="block text-[10px] font-bold text-gray-400 uppercase mb-1">Số điện thoại</label>
                    <p className="text-sm font-bold text-blue-600">{employee?.phoneNumber || '---'}</p>
                 </div>
                 <div>
                    <label className="block text-[10px] font-bold text-gray-400 uppercase mb-1">Chi nhánh</label>
                    <p className="text-sm font-bold text-gray-700">{employee?.branchName || 'Toàn hệ thống'}</p>
                 </div>
                 <div>
                    <label className="block text-[10px] font-bold text-gray-400 uppercase mb-1">Ngày vào làm</label>
                    <p className="text-sm font-bold text-gray-700">
                       {employee?.startDate ? new Date(employee.startDate).toLocaleDateString('vi-VN') : '---'}
                    </p>
                 </div>
              </div>

              <div className="border-t pt-6">
                 <h3 className="text-xs font-black text-gray-400 uppercase mb-4 tracking-widest flex items-center">
                    <Shield size={14} className="mr-2" /> Bảo mật & Tài khoản
                 </h3>
                 <div className="space-y-3">
                    <div className="p-4 bg-gray-50 rounded-xl flex items-center justify-between">
                       <div>
                          <p className="text-[10px] text-gray-400 font-bold uppercase">Tên đăng nhập</p>
                          <p className="text-sm font-bold text-gray-700">{employee?.username}</p>
                       </div>
                       <Shield size={16} className="text-green-500" />
                    </div>
                    <button
                      onClick={onLogout}
                      className="w-full flex items-center justify-center p-4 bg-red-50 rounded-xl hover:bg-red-100 transition-colors text-sm font-black uppercase tracking-widest text-red-600"
                    >
                       <LogOut size={16} className="mr-2" /> ĐĂNG XUẤT TÀI KHOẢN
                    </button>
                 </div>
              </div>
           </div>
        </div>

        {/* Note info */}
        <div className="bg-blue-50 p-6 rounded-3xl border border-blue-100">
           <div className="flex items-start space-x-3">
              <CheckCircle2 className="text-blue-600 mt-1" size={18} />
              <div>
                 <p className="text-xs font-bold text-blue-800 uppercase tracking-tight">Hồ sơ đã xác thực</p>
                 <p className="text-[11px] text-blue-600 mt-1 leading-relaxed italic">Thông tin cá nhân và lịch làm việc của bạn được quản lý bởi hệ thống KiotViet Restaurant. Vui lòng liên hệ Quản trị viên nếu cần thay đổi thông tin.</p>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
};

export default EmployeeProfile;

