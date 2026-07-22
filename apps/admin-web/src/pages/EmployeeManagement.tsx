import React, { useState, useEffect } from 'react';
import { Plus, Search, Filter, MoreVertical, Edit2, Trash2, Loader2, X, ChevronDown, User, Phone, Mail, MapPin, Briefcase, Calendar, CreditCard, Facebook, Info, Settings, Clock, DollarSign, Store, Key } from 'lucide-react';
import { API_URL } from '../config';

interface Employee {
  id?: string;
  employeeCode: string;
  fullName: string;
  phoneNumber?: string;
  position?: string;
  department?: string;
  branchName?: string;
  citizenId?: string;
  birthDate?: string;
  gender?: string;
  address?: string;
  startDate: string;
  isActive: boolean;
  basicSalary: number;
  note?: string;
  username?: string;
  password?: string;
}

interface Branch {
  id: string;
  name: string;
}

const EmployeeManagement = () => {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null);
  const [expandedRow, setExpandedRow] = useState<string | null>(null);

  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('active'); // active, inactive, all
  const [filterDept, setFilterDept] = useState('');
  const [filterPos, setFilterPos] = useState('');

  // Dropdown lists
  const departments = ['Phòng bàn', 'Bếp', 'Kho', 'Quản lý'];
  const positions = ['Thu ngân', 'Phục vụ', 'Đầu bếp', 'Quản lý', 'Pha chế'];

  // Form State
  const [newEmployee, setNewEmployee] = useState<Employee>({
    employeeCode: '',
    fullName: '',
    phoneNumber: '',
    position: 'Phục vụ',
    department: 'Phòng bàn',
    branchName: '',
    citizenId: '',
    gender: 'Nam',
    address: '',
    startDate: new Date().toISOString().split('T')[0],
    isActive: true,
    basicSalary: 0,
    note: '',
    username: '',
    password: ''
  });

  const fetchBranches = async () => {
    try {
      const response = await fetch(`${API_URL}/api/Branch`);
      const data = await response.json();
      setBranches(data);
      if (data.length > 0 && !newEmployee.branchName) {
        setNewEmployee(prev => ({ ...prev, branchName: data[0].name }));
      }
    } catch (err) {
      console.error('Error fetching branches:', err);
    }
  };

  const fetchEmployees = async () => {
    try {
      setLoading(true);
      let url = `${API_URL}/api/Employee`;
      const params = new URLSearchParams();
      if (searchTerm) params.append('search', searchTerm);
      if (filterStatus === 'active') params.append('isActive', 'true');
      if (filterStatus === 'inactive') params.append('isActive', 'false');
      if (filterDept) params.append('department', filterDept);
      if (filterPos) params.append('position', filterPos);

      if (params.toString()) {
        url += `?${params.toString()}`;
      }

      const response = await fetch(url);
      const data: Employee[] = await response.json();
      setEmployees(data);
    } catch (err) {
      console.error('Error fetching employees:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBranches();
  }, []);

  useEffect(() => {
    fetchEmployees();
  }, [filterStatus, filterDept, filterPos, searchTerm]);

  const handleSaveEmployee = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const isEditing = !!editingEmployee;
      const url = isEditing
        ? `${API_URL}/api/Employee/${editingEmployee.id}`
        : `${API_URL}/api/Employee`;

      const payload = {
        ...(isEditing ? { ...editingEmployee, ...newEmployee } : newEmployee),
        birthDate: newEmployee.birthDate || null,
        basicSalary: Number(newEmployee.basicSalary) || 0
      };

      const response = await fetch(url, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        setIsModalOpen(false);
        setEditingEmployee(null);
        resetForm();
        fetchEmployees();
      } else {
        const errorData = await response.json();
        alert(`Lỗi: ${errorData.message || 'Không thể lưu nhân viên'}`);
      }
    } catch (err) {
      alert('Lỗi kết nối đến server.');
    }
  };

  const resetForm = () => {
    setNewEmployee({
      employeeCode: '',
      fullName: '',
      phoneNumber: '',
      position: 'Phục vụ',
      department: 'Phòng bàn',
      branchName: branches.length > 0 ? branches[0].name : '',
      citizenId: '',
      gender: 'Nam',
      address: '',
      startDate: new Date().toISOString().split('T')[0],
      isActive: true,
      basicSalary: 0,
      note: '',
      username: '',
      password: ''
    });
  };

  const openEditModal = (emp: Employee) => {
    setEditingEmployee(emp);
    setNewEmployee({
      ...emp,
      startDate: (emp.startDate && typeof emp.startDate === 'string') ? emp.startDate.split('T')[0] : new Date().toISOString().split('T')[0],
      birthDate: (emp.birthDate && typeof emp.birthDate === 'string') ? emp.birthDate.split('T')[0] : '',
      username: emp.username || '',
      password: emp.password || ''
    });
    setIsModalOpen(true);
  };

  const handleDeleteEmployee = async (id: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa nhân viên này?')) return;
    try {
      const response = await fetch(`${API_URL}/api/Employee/${id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        fetchEmployees();
      }
    } catch (err) {
      alert('Lỗi khi xóa nhân viên');
    }
  };

  const handleToggleStatus = async (id: string) => {
    try {
      const response = await fetch(`${API_URL}/api/Employee/${id}/toggle-status`, {
        method: 'PATCH'
      });
      if (response.ok) {
        fetchEmployees();
      }
    } catch (err) {
      alert('Lỗi khi cập nhật trạng thái');
    }
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      {/* SIDEBAR FILTER */}
      <div className="w-64 bg-white border-r overflow-y-auto p-4 space-y-6">
        <h2 className="font-bold text-base text-gray-800">Danh sách nhân viên</h2>

        {/* Trạng thái */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Trạng thái</p>
          <div className="space-y-1.5 ml-1">
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" checked={filterStatus === 'active'} onChange={() => setFilterStatus('active')} />
              <span className={filterStatus === 'active' ? 'text-blue-600 font-bold' : ''}>Đang làm việc</span>
            </label>
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" checked={filterStatus === 'inactive'} onChange={() => setFilterStatus('inactive')} />
              <span className={filterStatus === 'inactive' ? 'text-blue-600 font-bold' : ''}>Đã nghỉ</span>
            </label>
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" checked={filterStatus === 'all'} onChange={() => setFilterStatus('all')} />
              <span className={filterStatus === 'all' ? 'text-blue-600 font-bold' : ''}>Tất cả</span>
            </label>
          </div>
        </div>

        {/* Phòng ban */}
        <div className="space-y-2">
          <div className="flex justify-between items-center">
            <p className="font-bold text-gray-700">Phòng ban</p>
            <button className="text-blue-600 text-[11px] hover:underline">Tạo mới</button>
          </div>
          <select
            className="w-full border-b border-gray-200 py-1 outline-none focus:border-blue-500 bg-transparent"
            value={filterDept}
            onChange={(e) => setFilterDept(e.target.value)}
          >
            <option value="">Chọn phòng ban</option>
            {departments.map(d => <option key={d} value={d}>{d}</option>)}
          </select>
        </div>

        {/* Chức danh */}
        <div className="space-y-2">
          <div className="flex justify-between items-center">
            <p className="font-bold text-gray-700">Chức danh</p>
            <button className="text-blue-600 text-[11px] hover:underline">Tạo mới</button>
          </div>
          <select
            className="w-full border-b border-gray-200 py-1 outline-none focus:border-blue-500 bg-transparent"
            value={filterPos}
            onChange={(e) => setFilterPos(e.target.value)}
          >
            <option value="">Chọn chức danh</option>
            {positions.map(p => <option key={p} value={p}>{p}</option>)}
          </select>
        </div>
      </div>

      {/* MAIN CONTENT */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Actions Bar */}
        <div className="bg-white p-3 flex justify-between items-center border-b">
          <div className="relative w-80">
            <Search className="absolute left-3 top-2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-1.5 bg-gray-100 border-none rounded-md outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Tìm theo mã, tên nhân viên, SĐT"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex space-x-2">
            <button
              onClick={() => { resetForm(); setEditingEmployee(null); setIsModalOpen(true); }}
              className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700"
            >
              <Plus size={16} className="mr-1" /> Thêm nhân viên
            </button>
          </div>
        </div>

        {/* Employee List */}
        <div className="flex-1 overflow-auto bg-white">
          <table className="w-full text-left border-collapse">
            <thead className="bg-[#f9fafb] border-b text-gray-500 font-bold sticky top-0 z-10">
              <tr>
                <th className="px-4 py-2 w-10"><input type="checkbox" /></th>
                <th className="px-4 py-2 w-32 uppercase text-[11px]">Mã nhân viên</th>
                <th className="px-4 py-2 uppercase text-[11px]">Tên nhân viên</th>
                <th className="px-4 py-2 w-40 uppercase text-[11px]">Chức danh</th>
                <th className="px-4 py-2 w-40 uppercase text-[11px]">Số điện thoại</th>
                <th className="px-4 py-2 w-40 uppercase text-[11px]">Chi nhánh</th>
                <th className="px-4 py-2 w-32 uppercase text-[11px] text-right">Nợ tạm ứng</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr><td colSpan={7} className="text-center py-20"><Loader2 className="animate-spin inline mr-2 text-blue-600"/> Đang tải...</td></tr>
              ) : employees.length === 0 ? (
                <tr><td colSpan={7} className="text-center py-20 text-gray-400 italic">Chưa có nhân viên nào trong danh sách.</td></tr>
              ) : employees.map(e => (
                <React.Fragment key={e.id}>
                  <tr
                    className={`hover:bg-[#eef6ff] cursor-pointer transition-colors ${expandedRow === e.id ? 'bg-[#eef6ff]' : ''}`}
                    onClick={() => setExpandedRow(expandedRow === e.id ? null : e.id!)}
                  >
                    <td className="px-4 py-3"><input type="checkbox" onClick={ev => ev.stopPropagation()}/></td>
                    <td className="px-4 py-3 text-blue-600 font-medium">{e.employeeCode}</td>
                    <td className="px-4 py-3 font-medium">{e.fullName}</td>
                    <td className="px-4 py-3 text-gray-500">{e.position || '---'}</td>
                    <td className="px-4 py-3 text-gray-500">{e.phoneNumber || '---'}</td>
                    <td className="px-4 py-3 text-gray-500">{e.branchName || '---'}</td>
                    <td className="px-4 py-3 text-right font-bold text-gray-800">0</td>
                  </tr>
                  {/* Expanded Detail View */}
                  {expandedRow === e.id && (
                    <tr className="bg-white">
                      <td colSpan={7} className="p-0">
                        <div className="border-l-4 border-blue-500 ml-4 my-2 shadow-inner bg-[#fcfdfe]">
                          <div className="flex border-b text-[12px] font-bold text-gray-500">
                             {['Thông tin', 'Lịch làm việc', 'Thiết lập lương'].map((tab, i) => (
                               <button key={tab} className={`px-6 py-2 ${i === 0 ? 'text-blue-600 border-b-2 border-blue-600' : ''}`}>{tab}</button>
                             ))}
                          </div>
                          <div className="p-6 flex space-x-10">
                             <div className="w-24 h-24 bg-blue-50 rounded-full flex items-center justify-center border border-blue-100">
                                <User size={48} className="text-blue-200" />
                             </div>
                             <div className="flex-1">
                                <div className="flex justify-between items-start mb-6">
                                   <div>
                                      <h3 className="text-lg font-bold capitalize">{e.fullName}</h3>
                                      <p className="text-gray-400 text-[11px]">Mã nhân viên: <span className="font-bold text-gray-600">{e.employeeCode}</span></p>
                                   </div>
                                </div>

                                <div className="grid grid-cols-3 gap-x-10 gap-y-6 text-[12px]">
                                   <div>
                                      <p className="text-gray-400 mb-1">Số điện thoại</p>
                                      <p className="font-medium text-blue-600">{e.phoneNumber || 'Chưa cập nhật'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Chi nhánh trả lương</p>
                                      <p className="font-medium">{e.branchName || 'Chi nhánh trung tâm'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Chi nhánh làm việc</p>
                                      <p className="font-medium">{e.branchName || 'Chi nhánh trung tâm'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Phòng ban</p>
                                      <p className="font-medium">{e.department || '---'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Chức danh</p>
                                      <p className="font-medium">{e.position || '---'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Tài khoản đăng nhập</p>
                                      <p className="font-bold text-blue-600">{e.username || 'Chưa thiết lập'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Mật khẩu</p>
                                      <p className="font-medium">••••••••</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Số CMND/CCCD</p>
                                      <p className="font-medium">{e.citizenId || '---'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Ngày sinh</p>
                                      <p className="font-medium">{e.birthDate ? new Date(e.birthDate).toLocaleDateString('vi-VN') : '---'}</p>
                                   </div>
                                   <div>
                                      <p className="text-gray-400 mb-1">Giới tính</p>
                                      <p className="font-medium">{e.gender || '---'}</p>
                                   </div>
                                </div>

                                <div className="mt-8 flex justify-between items-center border-t pt-4">
                                   <div className="flex items-center text-gray-500 italic">
                                      <Edit2 size={12} className="mr-1"/> <span className="mr-4">Ghi chú: {e.note || 'Không có ghi chú'}</span>
                                   </div>
                                   <div className="flex space-x-2">
                                      <button
                                        onClick={(ev) => { ev.stopPropagation(); openEditModal(e); }}
                                        className="bg-blue-600 text-white px-4 py-1.5 rounded font-bold flex items-center hover:bg-blue-700 transition-all"
                                      >
                                        <Edit2 size={14} className="mr-1"/> Cập nhật
                                      </button>
                                      <button
                                        onClick={(ev) => { ev.stopPropagation(); handleToggleStatus(e.id!); }}
                                        className="border border-gray-300 px-4 py-1.5 rounded font-medium flex items-center hover:bg-gray-50"
                                      >
                                        <Clock size={14} className="mr-1"/> {e.isActive ? 'Ngừng làm việc' : 'Cho phép làm việc'}
                                      </button>
                                      <button
                                        onClick={(ev) => { ev.stopPropagation(); handleDeleteEmployee(e.id!); }}
                                        className="border border-gray-300 px-4 py-1.5 rounded font-medium flex items-center hover:bg-red-50 hover:text-red-600 hover:border-red-200"
                                      >
                                        <Trash2 size={14} className="mr-1"/> Xóa
                                      </button>
                                   </div>
                                </div>
                             </div>
                          </div>
                        </div>
                      </td>
                    </tr>
                  )}
                </React.Fragment>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* MODAL FORM */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/40 z-[100] flex justify-center items-start pt-10 overflow-y-auto pb-10 px-4">
           <div className="bg-white w-full max-w-4xl rounded-lg shadow-2xl overflow-hidden animate-in slide-in-from-bottom-4 duration-300">
              <div className="bg-[#0070f4] p-4 text-white flex justify-between items-center">
                 <h3 className="font-bold text-lg flex items-center"><User size={20} className="mr-2"/> {editingEmployee ? 'Cập nhật nhân viên' : 'Thêm nhân viên mới'}</h3>
                 <button onClick={() => setIsModalOpen(false)} className="hover:rotate-90 transition-transform"><X size={24}/></button>
              </div>

              <form onSubmit={handleSaveEmployee} className="flex flex-col md:flex-row">
                 <div className="flex-1 p-8 grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6">
                    <div className="col-span-1 md:col-span-2 flex items-center space-x-6 mb-4">
                       <div className="w-24 h-24 bg-gray-50 border-2 border-dashed border-gray-200 rounded-full flex flex-col items-center justify-center text-gray-400 cursor-pointer hover:bg-gray-100">
                          <User size={32} />
                          <span className="text-[10px] mt-1 font-bold">Thêm ảnh</span>
                       </div>
                       <div className="flex-1 grid grid-cols-1 md:grid-cols-2 gap-4">
                          <div className="border-b focus-within:border-blue-500 transition-colors">
                             <label className="block text-[11px] text-gray-400 font-bold uppercase">Họ tên nhân viên</label>
                             <input type="text" className="w-full py-1 outline-none text-base font-medium" placeholder="VD: Nguyễn Văn A" value={newEmployee.fullName} onChange={ev => setNewEmployee({...newEmployee, fullName: ev.target.value})} required/>
                          </div>
                          <div className="border-b focus-within:border-blue-500 transition-colors">
                             <label className="block text-[11px] text-gray-400 font-bold uppercase">Mã nhân viên</label>
                             <input type="text" className="w-full py-1 outline-none font-medium" placeholder="Mã tự động" value={newEmployee.employeeCode} onChange={ev => setNewEmployee({...newEmployee, employeeCode: ev.target.value})}/>
                          </div>
                       </div>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Số điện thoại</label>
                       <input type="text" className="w-full py-1 outline-none font-medium" placeholder="09xxx" value={newEmployee.phoneNumber} onChange={ev => setNewEmployee({...newEmployee, phoneNumber: ev.target.value})}/>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Số CMND/CCCD</label>
                       <input type="text" className="w-full py-1 outline-none font-medium" value={newEmployee.citizenId} onChange={ev => setNewEmployee({...newEmployee, citizenId: ev.target.value})}/>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Chi nhánh làm việc</label>
                       <div className="flex items-center">
                          <Store size={14} className="mr-2 text-blue-500" />
                          <select
                            className="w-full py-1 outline-none bg-transparent font-medium"
                            value={newEmployee.branchName}
                            onChange={ev => setNewEmployee({...newEmployee, branchName: ev.target.value})}
                          >
                             {branches.map(b => <option key={b.id} value={b.name}>{b.name}</option>)}
                          </select>
                       </div>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Phòng ban</label>
                       <select className="w-full py-1 outline-none bg-transparent font-medium" value={newEmployee.department} onChange={ev => setNewEmployee({...newEmployee, department: ev.target.value})}>
                          {departments.map(d => <option key={d} value={d}>{d}</option>)}
                       </select>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Chức danh</label>
                       <select className="w-full py-1 outline-none bg-transparent font-medium" value={newEmployee.position} onChange={ev => setNewEmployee({...newEmployee, position: ev.target.value})}>
                          {positions.map(p => <option key={p} value={p}>{p}</option>)}
                       </select>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Lương cơ bản</label>
                       <div className="flex items-center">
                          <input type="number" className="w-full py-1 outline-none font-medium text-blue-600" value={newEmployee.basicSalary} onChange={ev => setNewEmployee({...newEmployee, basicSalary: parseInt(ev.target.value) || 0})}/>
                          <span className="text-gray-400 text-xs font-bold">VNĐ</span>
                       </div>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Ngày vào làm</label>
                       <input type="date" className="w-full py-1 outline-none font-medium" value={newEmployee.startDate} onChange={ev => setNewEmployee({...newEmployee, startDate: ev.target.value})}/>
                    </div>

                    <div className="col-span-1 md:col-span-2 border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Ghi chú</label>
                       <textarea className="w-full py-1 outline-none h-16 text-[12px]" placeholder="Thông tin thêm..." value={newEmployee.note} onChange={ev => setNewEmployee({...newEmployee, note: ev.target.value})}/>
                    </div>

                    <div className="col-span-1 md:col-span-2 bg-blue-50/50 p-4 rounded-lg grid grid-cols-1 md:grid-cols-2 gap-6 border border-blue-100">
                       <div className="col-span-1 md:col-span-2 flex items-center text-blue-700 font-bold text-[12px]">
                          <Key size={16} className="mr-2" /> Thông tin tài khoản đăng nhập
                       </div>
                       <div className="border-b border-blue-200">
                          <label className="block text-[11px] text-blue-500 font-bold uppercase">Tên đăng nhập</label>
                          <input type="text" className="w-full py-1 outline-none font-medium bg-transparent" placeholder="VD: nv_nguyenvana" value={newEmployee.username} onChange={ev => setNewEmployee({...newEmployee, username: ev.target.value})}/>
                       </div>
                       <div className="border-b border-blue-200">
                          <label className="block text-[11px] text-blue-500 font-bold uppercase">Mật khẩu</label>
                          <input type="password" name="password" className="w-full py-1 outline-none font-medium bg-transparent" placeholder="••••••••" value={newEmployee.password} onChange={ev => setNewEmployee({...newEmployee, password: ev.target.value})}/>
                       </div>
                    </div>
                 </div>

                 {/* Right Guide */}
                 <div className="w-full md:w-64 bg-gray-50 p-6 border-l text-gray-500 text-[11px]">
                    <div className="flex items-center space-x-2 text-blue-600 mb-4 font-bold text-[12px]">
                       <Info size={16} /> <span>Ghi chú quan trọng</span>
                    </div>
                    <ul className="space-y-4 italic leading-relaxed">
                       <li>Tên nhân viên là bắt buộc. Mã nhân viên sẽ được tạo tự động nếu để trống.</li>
                       <li>Nên cập nhật đúng **Chi nhánh làm việc** để nhân viên có thể đăng nhập vào đúng cơ sở.</li>
                       <li>Nên cập nhật CMND/CCCD để làm hồ sơ nhân sự và tính lương chính xác hơn.</li>
                    </ul>
                 </div>
              </form>

              <div className="p-4 bg-gray-50 border-t flex justify-end space-x-3">
                 <button onClick={() => { setIsModalOpen(false); setEditingEmployee(null); }} type="button" className="px-6 py-2 border rounded font-bold text-gray-600 hover:bg-white transition-colors">Bỏ qua</button>
                 <button onClick={handleSaveEmployee} className="px-6 py-2 bg-[#0070f4] text-white rounded font-bold shadow-lg hover:bg-blue-700 active:scale-95 transition-all">
                    {editingEmployee ? 'Cập nhật' : 'Lưu nhân viên'}
                 </button>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default EmployeeManagement;
