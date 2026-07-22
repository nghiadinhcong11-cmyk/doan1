import React, { useState, useEffect } from 'react';
import { Plus, Search, User, Shield, ShieldCheck, Mail, Phone, Edit2, Trash2, Key, ChevronDown, CheckCircle2, Loader2 } from 'lucide-react';

interface EmployeeUser {
  id: string;
  fullName: string;
  employeeCode: string;
  username?: string;
  position: string;
  department: string;
  isActive: boolean;
  createdAt: string;
}

const UserManagement = () => {
  const [users, setUsers] = useState<EmployeeUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const response = await fetch('http://localhost:5000/api/Employee');
      const data = await response.json();
      setUsers(data);
    } catch (err) {
      console.error('Error fetching users:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, []);

  const filteredUsers = users.filter(u =>
    u.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    u.employeeCode.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      <div className="w-64 bg-white border-r p-4 space-y-6 shadow-sm">
        <h2 className="font-bold text-base text-gray-800">Quản lý người dùng</h2>
        <div className="space-y-4">
          <div>
            <p className="font-bold text-gray-400 text-[11px] uppercase mb-2 tracking-wider">Nhóm người dùng</p>
            <div className="space-y-1.5 ml-1">
              {['Tất cả', 'Quản trị viên', 'Quản lý', 'Thu ngân', 'Phục vụ'].map(role => (
                <label key={role} className="flex items-center cursor-pointer py-0.5 group">
                  <input type="radio" name="role" className="mr-2 h-3.5 w-3.5 text-blue-600" defaultChecked={role === 'Tất cả'} />
                  <span className="text-gray-600 group-hover:text-blue-600 transition-colors">{role}</span>
                </label>
              ))}
            </div>
          </div>

          <div className="p-3 bg-orange-50 border border-orange-100 rounded-lg text-orange-800">
             <p className="font-bold text-[11px] mb-1 flex items-center"><Shield size={12} className="mr-1"/> Bảo mật</p>
             <p className="text-[10px] leading-relaxed italic">
               Tài khoản người dùng được liên kết trực tiếp với hồ sơ nhân viên để quản lý quyền hạn.
             </p>
          </div>
        </div>
      </div>

      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="bg-white p-3 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-80">
            <Search className="absolute left-3 top-2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-1.5 bg-gray-100 border-none rounded-md outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Tìm theo tên hoặc mã nhân viên..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700 transition-colors shadow-sm">
            <Plus size={16} className="mr-1" /> Thêm người dùng
          </button>
        </div>

        <div className="flex-1 overflow-auto bg-white m-4 rounded-lg shadow-sm border border-gray-200">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b text-gray-500 font-bold sticky top-0 z-10">
              <tr>
                <th className="px-6 py-3 uppercase text-[11px]">Tên người dùng</th>
                <th className="px-6 py-3 uppercase text-[11px]">Mã NV / Tài khoản</th>
                <th className="px-6 py-3 uppercase text-[11px]">Vai trò</th>
                <th className="px-6 py-3 uppercase text-[11px]">Trạng thái</th>
                <th className="px-6 py-3 uppercase text-[11px]">Ngày tạo</th>
                <th className="px-6 py-3 text-right"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr><td colSpan={6} className="py-20 text-center"><Loader2 className="animate-spin inline mr-2 text-blue-600" /> Đang tải...</td></tr>
              ) : filteredUsers.length === 0 ? (
                <tr><td colSpan={6} className="py-20 text-center text-gray-400 italic">Không tìm thấy người dùng nào.</td></tr>
              ) : filteredUsers.map(u => (
                <tr key={u.id} className="hover:bg-blue-50/50 transition-colors group">
                  <td className="px-6 py-4">
                    <div className="flex items-center">
                      <div className={`w-8 h-8 rounded-full flex items-center justify-center mr-3 ${u.isActive ? 'bg-blue-100 text-blue-600' : 'bg-gray-200 text-gray-400'}`}>
                        <User size={16} />
                      </div>
                      <span className="font-bold text-gray-700">{u.fullName}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex flex-col">
                       <span className="text-gray-600 font-medium">{u.employeeCode}</span>
                       <span className="text-[10px] text-blue-500 font-bold">{u.username || 'Chưa tạo tài khoản'}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-0.5 rounded text-[10px] font-black uppercase tracking-tight ${u.position === 'Quản lý' ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'}`}>
                      {u.position || 'Nhân viên'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`flex items-center text-[11px] font-bold ${u.isActive ? 'text-green-600' : 'text-gray-400'}`}>
                       {u.isActive ? <CheckCircle2 size={12} className="mr-1"/> : <X size={12} className="mr-1"/>}
                       {u.isActive ? 'Hoạt động' : 'Đã khóa'}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-gray-400 text-[12px]">{new Date(u.createdAt).toLocaleDateString('vi-VN')}</td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex justify-end space-x-2 opacity-0 group-hover:opacity-100 transition-opacity">
                      <button className="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Sửa"><Edit2 size={14}/></button>
                      <button className="p-1.5 text-gray-400 hover:bg-gray-50 rounded" title="Đổi mật khẩu"><Key size={14}/></button>
                      <button className="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa"><Trash2 size={14}/></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default UserManagement;
