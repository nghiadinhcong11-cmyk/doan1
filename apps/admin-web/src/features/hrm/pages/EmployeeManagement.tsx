import React, { useState, useEffect } from 'react';
import { Plus, Search, Filter, MoreVertical, Edit2, Trash2, Loader2, X, ChevronDown, User, Phone, Mail, MapPin, Briefcase, Calendar, CreditCard, Facebook, Info, Settings, Clock, DollarSign, Store, Key, UserCheck, ShieldCheck } from 'lucide-react';
import { API_URL } from '../../../config';
import { useRef } from 'react';
import { Feedback, FormField, notifyFeedback } from '../../../components/ui';
import { fetchBranchList } from '../../../api/branchQuery';

interface Employee {
  id?: string;
  employeeCode: string;
  employeeType: string;
  fullName: string;
  phoneNumber?: string;
  position?: string;
  department?: string;
  basicSalary: number;
  branchId?: string;
  branchName?: string;
  citizenId?: string;
  birthDate?: string;
  gender?: string;
  address?: string;
  startDate: string;
  isActive: boolean;
  role: string;
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
  const [formError, setFormError] = useState('');
  const [listError, setListError] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [pendingActions, setPendingActions] = useState<Set<string>>(new Set());
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null);
  const [expandedRow, setExpandedRow] = useState<string | null>(null);
  const userRole = localStorage.getItem('userRole');
  const ownBranchId = localStorage.getItem('selectedBranchId');
  const currentEmployeeId = localStorage.getItem('employeeId');

  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('active'); // active, inactive, all
  const [filterDept, setFilterDept] = useState('');
  const [filterPos, setFilterPos] = useState('');
  const [filterBranch, setFilterBranch] = useState(userRole === 'manager' && ownBranchId ? ownBranchId : 'all');
  const requestGeneration = useRef(0);
  const abortController = useRef<AbortController | null>(null);
  const saveInFlight = useRef(false);
  const actionInFlight = useRef(new Set<string>());
  const employeeQueryRef = useRef({ searchTerm, filterStatus, filterDept, filterPos, filterBranch });
  employeeQueryRef.current = { searchTerm, filterStatus, filterDept, filterPos, filterBranch };
  const searchEffectInitialized = useRef(false);
  const employeeSearchTimer = useRef<number | null>(null);

  // Dropdown lists
  const departments = ['Phòng bàn', 'Bếp', 'Kho', 'Quản lý'];
  const positions = ['Thu ngân', 'Phục vụ', 'Đầu bếp', 'Quản lý', 'Pha chế'];
  const allRoles = [
    { value: 'admin', label: 'Quản trị hệ thống' },
    { value: 'manager', label: 'Quản lý chi nhánh' },
    { value: 'cashier', label: 'Thu ngân' },
    { value: 'kitchen', label: 'Nhân viên bếp' },
    { value: 'employee', label: 'Nhân viên phục vụ' }
  ];

  // Manager không được tạo admin
  const roles = userRole === 'admin' ? allRoles : allRoles.filter(r => r.value !== 'admin');

  // Form State
  const [newEmployee, setNewEmployee] = useState<Employee>({
    employeeCode: '',
    employeeType: 'FullTime',
    fullName: '',
    phoneNumber: '',
    position: 'Phục vụ',
    department: 'Phòng bàn',
    basicSalary: 0,
    branchId: '',
    branchName: '',
    citizenId: '',
    gender: 'Nam',
    address: '',
    startDate: new Date().toISOString().split('T')[0],
    isActive: true,
    role: 'employee',
    note: '',
    username: '',
    password: ''
  });

  const fetchBranches = async () => {
    try {
      const data = await fetchBranchList();
      setBranches(data as Branch[]);
    } catch (err) {
      setListError('Không thể tải danh sách chi nhánh. Vui lòng thử lại.');
    }
  };

  const fetchEmployees = async (): Promise<boolean> => {
    const { searchTerm: currentSearch, filterStatus: currentStatus, filterDept: currentDept, filterPos: currentPos, filterBranch: currentBranch } = employeeQueryRef.current;
    const generation = ++requestGeneration.current;
    abortController.current?.abort();
    const controller = new AbortController();
    abortController.current = controller;
    try {
      setLoading(true);
      let url = `${API_URL}/api/Employee`;
      const params = new URLSearchParams();
      if (currentSearch) params.append('search', currentSearch);
      if (currentStatus === 'active') params.append('isActive', 'true');
      if (currentStatus === 'inactive') params.append('isActive', 'false');
      if (currentDept) params.append('department', currentDept);
      if (currentPos) params.append('position', currentPos);

      const response = await fetch(`${url}${params.toString() ? `?${params.toString()}` : ''}`, { signal: controller.signal });
      if (!response.ok) throw new Error('employee request failed');
      const data: Employee[] = await response.json();

      if (generation !== requestGeneration.current) return false;
      const finalData = currentBranch === 'all' ? data : data.filter(e => e.branchId === currentBranch);
      setEmployees(finalData);
      return true;
    } catch (err) {
      if (!(err instanceof DOMException && err.name === 'AbortError') && generation === requestGeneration.current) setListError('Không thể tải danh sách nhân viên. Vui lòng thử lại.');
      return false;
    } finally {
      if (generation === requestGeneration.current) setLoading(false);
      if (abortController.current === controller) abortController.current = null;
    }
  };

  useEffect(() => {
    fetchBranches();
  }, []);

  useEffect(() => {
    if (employeeSearchTimer.current !== null) {
      window.clearTimeout(employeeSearchTimer.current);
      employeeSearchTimer.current = null;
    }
    void fetchEmployees();
  }, [filterStatus, filterDept, filterPos, filterBranch]);

  useEffect(() => {
    if (!searchEffectInitialized.current) {
      searchEffectInitialized.current = true;
      return;
    }
    employeeSearchTimer.current = window.setTimeout(() => {
      employeeSearchTimer.current = null;
      void fetchEmployees();
    }, 350);
    return () => {
      if (employeeSearchTimer.current !== null) {
        window.clearTimeout(employeeSearchTimer.current);
        employeeSearchTimer.current = null;
      }
    };
  }, [searchTerm]);

  useEffect(() => () => abortController.current?.abort(), []);

  const handleSaveEmployee = async (e: React.FormEvent) => {
    e.preventDefault();
    if (saveInFlight.current) return;
    setFormError('');
    if (newEmployee.password && (newEmployee.password.length < 8 || newEmployee.password.length > 128)) {
      setFormError('Mật khẩu phải dài từ 8 đến 128 ký tự.');
      return;
    }
    saveInFlight.current = true;
    setIsSaving(true);
    try {
      const isEditing = !!editingEmployee;
      const url = isEditing
        ? `${API_URL}/api/Employee/${editingEmployee.id}`
        : `${API_URL}/api/Employee`;

      const payload = {
        ...(isEditing ? { ...editingEmployee, ...newEmployee } : newEmployee),
        birthDate: newEmployee.birthDate || null,
        branchName: branches.find(b => b.id === newEmployee.branchId)?.name
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
        const refreshed = await fetchEmployees();
        if (!refreshed) notifyFeedback('Đã lưu hồ sơ, nhưng không thể tải lại danh sách.');
      } else {
        const errorData = await response.json().catch(() => ({}));
        setFormError(typeof errorData.message === 'string' ? errorData.message : 'Không thể lưu nhân viên.');
      }
    } catch (err) {
      setFormError('Không thể kết nối máy chủ. Dữ liệu nhập chưa được thay đổi.');
    } finally {
      saveInFlight.current = false;
      setIsSaving(false);
    }
  };

  const resetForm = () => {
    setNewEmployee({
      employeeCode: '',
      employeeType: 'FullTime',
      fullName: '',
      phoneNumber: '',
      position: 'Phục vụ',
      department: 'Phòng bàn',
      basicSalary: 0,
      branchId: branches.length > 0 ? branches[0].id : '',
      branchName: branches.length > 0 ? branches[0].name : '',
      citizenId: '',
      gender: 'Nam',
      address: '',
      startDate: new Date().toISOString().split('T')[0],
      isActive: true,
      role: 'employee',
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
    const actionKey = `${id}:delete`;
    if (id === currentEmployeeId || Array.from(actionInFlight.current).some(key => key.startsWith(`${id}:`))) return;
    if (!window.confirm('Bạn có chắc chắn muốn xóa nhân viên này?')) return;
    actionInFlight.current.add(actionKey);
    setPendingActions(new Set(actionInFlight.current));
    try {
      const response = await fetch(`${API_URL}/api/Employee/${id}`, {
        method: 'DELETE'
      });
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        notifyFeedback(typeof errorData.message === 'string' ? errorData.message : 'Không thể xóa nhân viên.');
        return;
      }
      const refreshed = await fetchEmployees();
      if (!refreshed) notifyFeedback('Đã xóa nhân viên, nhưng không thể tải lại danh sách.');
    } catch (err) {
      notifyFeedback('Không thể kết nối máy chủ khi xóa nhân viên.');
    } finally {
      actionInFlight.current.delete(actionKey);
      setPendingActions(new Set(actionInFlight.current));
    }
  };

  const handleToggleStatus = async (id: string) => {
    const actionKey = `${id}:toggle`;
    if (id === currentEmployeeId || Array.from(actionInFlight.current).some(key => key.startsWith(`${id}:`))) return;
    actionInFlight.current.add(actionKey);
    setPendingActions(new Set(actionInFlight.current));
    try {
      const response = await fetch(`${API_URL}/api/Employee/${id}/toggle-status`, {
        method: 'PATCH'
      });
      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        notifyFeedback(typeof errorData.message === 'string' ? errorData.message : 'Không thể cập nhật trạng thái.');
        return;
      }
      const refreshed = await fetchEmployees();
      if (!refreshed) notifyFeedback('Đã cập nhật trạng thái, nhưng không thể tải lại danh sách.');
    } catch (err) {
      notifyFeedback('Không thể kết nối máy chủ khi cập nhật trạng thái.');
    } finally {
      actionInFlight.current.delete(actionKey);
      setPendingActions(new Set(actionInFlight.current));
    }
  };

  const isEmployeeActionPending = (id?: string) => !!id && Array.from(pendingActions).some(key => key.startsWith(`${id}:`));

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f8f9fa] text-[13px] font-sans">
      {/* SIDEBAR FILTER */}
      <div className="w-72 bg-white border-r overflow-y-auto p-6 space-y-8 shadow-sm">
        <h2 className="font-black text-xl text-gray-800 uppercase italic tracking-tighter flex items-center">
           <UserCheck className="mr-3 text-blue-600" size={24}/> Nhân sự
        </h2>

        <div className="space-y-6">
          {/* Status Filter */}
          <div>
            <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3 ml-1">Lọc trạng thái</p>
            <div className="space-y-2 rounded-2xl border border-gray-200 bg-gray-50 p-2">
              {[
                { id: 'active', label: 'Đang làm việc' },
                { id: 'inactive', label: 'Đã nghỉ việc' },
                { id: 'all', label: 'Tất cả nhân viên' }
              ].map(s => (
                <label key={s.id} className="group flex cursor-pointer items-center rounded-xl px-3 py-2.5 transition-all hover:bg-blue-50">
                  <input
                    type="radio"
                    name="status"
                    className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                    checked={filterStatus === s.id}
                    onChange={() => setFilterStatus(s.id)}
                  />
                  <span className={`font-bold transition-colors ${filterStatus === s.id ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-600'}`}>{s.label}</span>
                </label>
              ))}
            </div>
          </div>

          {/* Branch Filter */}
          <div className="space-y-3 pt-6 border-t">
            <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Cơ sở trực thuộc</p>
            <select
              className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-5 py-3 text-sm font-bold text-gray-700 outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-100 disabled:opacity-50"
              value={filterBranch}
              onChange={(e) => setFilterBranch(e.target.value)}
              disabled={userRole === 'manager'}
            >
              {userRole === 'admin' && <option value="all">Tất cả chi nhánh</option>}
              {branches.filter(b => userRole === 'admin' || b.id === ownBranchId).map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
            </select>
          </div>

          {/* Department Filter */}
          <div className="space-y-3 pt-6 border-t">
            <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Phòng ban</p>
            <select
              className="w-full rounded-2xl border border-gray-200 bg-gray-50 px-5 py-3 text-sm font-bold text-gray-700 outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
              value={filterDept}
              onChange={(e) => setFilterDept(e.target.value)}
            >
              <option value="">Tất cả phòng ban</option>
              {departments.map(d => <option key={d} value={d}>{d}</option>)}
            </select>
          </div>
        </div>

        <div className="pt-8 border-t">
           <div className="bg-blue-50 p-4 rounded-3xl border border-blue-100">
              <p className="text-blue-700 font-black text-[10px] flex items-center mb-2 uppercase tracking-widest">
                 <ShieldCheck size={14} className="mr-2"/> Bảo mật dữ liệu
              </p>
              <p className="text-[10px] leading-relaxed italic text-blue-600/70 font-medium">
                Mỗi nhân viên chỉ có thể đăng nhập vào chi nhánh được chỉ định.
              </p>
           </div>
        </div>
      </div>

      {/* MAIN CONTENT */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Actions Bar */}
        <div className="bg-white p-4 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-96">
            <Search className="absolute left-4 top-3 h-4 w-4 text-gray-300" />
            <input
              type="text"
              className="w-full rounded-2xl border border-gray-200 bg-gray-50 py-3 pl-12 pr-4 text-sm font-bold outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
              placeholder="Tìm theo mã, tên nhân viên, SĐT..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button
            onClick={() => { resetForm(); setEditingEmployee(null); setIsModalOpen(true); }}
            className="flex min-h-0 items-center rounded bg-[#0070f4] px-4 py-1.5 text-[10px] font-black uppercase tracking-widest text-white shadow-lg shadow-blue-500/30 transition-all hover:bg-blue-700 active:scale-95"
          >
            <Plus size={18} className="mr-2" /> THÊM NHÂN VIÊN
          </button>
        </div>

        {/* Employee List */}
        <div className="flex-1 overflow-auto p-8">
           {listError && <div className="mb-4"><Feedback tone="error" onDismiss={() => setListError('')}>{listError}</Feedback></div>}
           <div className="bg-white rounded-[2.5rem] shadow-2xl shadow-blue-500/5 border border-white overflow-hidden">
              <table className="w-full text-left border-collapse">
                <thead className="bg-gray-50/50 border-b text-gray-400 font-black text-[10px] uppercase tracking-[0.2em]">
                  <tr>
                    <th className="px-8 py-5">Mã nhân sự</th>
                    <th className="px-8 py-5">Tên nhân viên</th>
                    <th className="px-8 py-5">Chức danh</th>
                    <th className="px-8 py-5">Cơ sở làm việc</th>
                    <th className="px-8 py-5 text-center">Trạng thái</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {loading ? (
                    <tr><td colSpan={5} className="py-20 text-center flex flex-col items-center justify-center">
                       <Loader2 className="animate-spin text-blue-600 mb-2" size={32} />
                       <p className="text-[10px] font-black uppercase tracking-widest text-gray-400">Đang truy xuất hồ sơ...</p>
                    </td></tr>
                  ) : employees.length === 0 ? (
                    <tr><td colSpan={5} className="py-32 text-center text-gray-300 italic font-bold uppercase tracking-widest text-[10px]">Chưa có hồ sơ nhân viên nào</td></tr>
                  ) : employees.map(e => (
                    <React.Fragment key={e.id}>
                      <tr
                        className={`hover:bg-blue-50/30 cursor-pointer transition-all ${expandedRow === e.id ? 'bg-blue-50/50' : ''}`}
                        onClick={() => setExpandedRow(expandedRow === e.id ? null : e.id!)}
                      >
                        <td className="px-8 py-5 font-black text-blue-600 italic tracking-tighter">{e.employeeCode}</td>
                        <td className="px-8 py-5">
                           <div className="flex items-center space-x-3">
                              <div className="w-8 h-8 rounded-xl bg-gray-50 border border-gray-100 flex items-center justify-center font-black text-[11px] text-gray-400 uppercase">
                                 {e.fullName.charAt(0)}
                              </div>
                              <span className="font-bold text-gray-700">{e.fullName}</span>
                           </div>
                        </td>
                        <td className="px-8 py-5 text-xs font-black uppercase text-gray-400">{e.position || '---'}</td>
                        <td className="px-8 py-5 text-[11px] font-bold text-gray-400 italic">{e.branchName || 'Toàn hệ thống'}</td>
                        <td className="px-8 py-5 text-center">
                           <span className={`px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-tighter shadow-sm border ${
                              e.isActive ? 'bg-green-50 text-green-700 border-green-100' : 'bg-gray-100 text-gray-400 border-gray-200'
                           }`}>
                              {e.isActive ? 'Đang làm' : 'Đã nghỉ'}
                           </span>
                        </td>
                      </tr>
                      {expandedRow === e.id && (
                        <tr className="bg-white">
                          <td colSpan={5} className="p-0">
                            <div className="border-l-4 border-blue-500 m-4 shadow-inner bg-gray-50 p-8 rounded-3xl animate-in slide-in-from-top-2 duration-300">
                              <div className="flex space-x-12">
                                 <div className="w-32 h-32 bg-white rounded-[2rem] flex items-center justify-center border-2 border-dashed border-gray-200 shrink-0 shadow-sm relative overflow-hidden">
                                    <User size={56} className="text-gray-100" />
                                    <div className="absolute inset-0 bg-blue-600/5"></div>
                                 </div>
                                 <div className="flex-1">
                                    <div className="flex justify-between items-start mb-8">
                                       <div>
                                          <h3 className="text-2xl font-black text-gray-800 uppercase italic tracking-tighter">{e.fullName}</h3>
                                          <p className="text-[10px] font-black text-blue-600 uppercase tracking-[0.2em] mt-1">{e.position} — {e.department}</p>
                                       </div>
                                       <div className="flex space-x-2">
                                          <button onClick={(ev) => { ev.stopPropagation(); openEditModal(e); }} className="rounded border border-gray-300 bg-white px-3 py-1.5 text-xs font-bold text-gray-700 shadow-sm transition-all hover:bg-gray-50">Sửa hồ sơ</button>
                                          <button disabled={e.id === currentEmployeeId || isEmployeeActionPending(e.id)} title={e.id === currentEmployeeId ? 'Không thể tự vô hiệu hóa tài khoản đang đăng nhập' : undefined} onClick={(ev) => { ev.stopPropagation(); handleToggleStatus(e.id!); }} className="rounded border border-orange-200 bg-white px-3 py-1.5 text-xs font-bold text-orange-600 shadow-sm transition-all hover:bg-orange-50 disabled:cursor-not-allowed disabled:opacity-40">{e.isActive ? 'Cho nghỉ' : 'Kích hoạt'}</button>
                                          <button disabled={e.id === currentEmployeeId || isEmployeeActionPending(e.id)} title={e.id === currentEmployeeId ? 'Không thể tự xóa tài khoản đang đăng nhập' : undefined} onClick={(ev) => { ev.stopPropagation(); handleDeleteEmployee(e.id!); }} className="rounded bg-red-600 px-3 py-1.5 text-xs font-bold text-white shadow-lg shadow-red-500/20 transition-all hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-40">Xóa vĩnh viễn</button>
                                       </div>
                                    </div>
                                    {e.id === currentEmployeeId && <p className="mb-5 text-[10px] font-bold text-orange-600">Không thể tự vô hiệu hóa hoặc xóa tài khoản đang đăng nhập.</p>}
                                    <div className="grid grid-cols-4 gap-8">
                                       <div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Số điện thoại</p><p className="font-bold text-gray-700">{e.phoneNumber || '---'}</p></div>
                                       <div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Tài khoản</p><p className="font-bold text-blue-600 italic tracking-tight">{e.username || '---'}</p></div>
                                       <div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">CMND/CCCD</p><p className="font-bold text-gray-700">{e.citizenId || '---'}</p></div>
                                       <div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Ngày vào làm</p><p className="font-bold text-gray-700">{e.startDate ? new Date(e.startDate).toLocaleDateString('vi-VN') : '---'}</p></div>
                                       <div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Loại nhân viên</p><p className="font-bold text-gray-700">{e.employeeType || '---'}</p></div>
                                       <div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Lương cơ bản</p><p className="font-bold text-gray-700">{e.basicSalary.toLocaleString('vi-VN')}</p></div>
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
      </div>

      {/* MODAL FORM */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-start pt-10 overflow-y-auto pb-10 px-4 backdrop-blur-sm">
           <div className="w-full max-w-4xl overflow-hidden rounded-[2.5rem] bg-white shadow-2xl animate-in slide-in-from-bottom-4 duration-300">
              <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
                 <div>
                    <h3 className="font-black text-xl uppercase italic tracking-tighter">Hồ sơ nhân sự</h3>
                    <p className="text-[10px] font-bold opacity-80 uppercase tracking-widest">{editingEmployee ? 'Cập nhật thông tin nhân viên' : 'Thiết lập nhân viên mới'}</p>
                 </div>
                 <button onClick={() => setIsModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <form onSubmit={handleSaveEmployee} className="flex flex-col md:flex-row">
                 <div className="flex-1 p-10 grid grid-cols-1 md:grid-cols-2 gap-x-10 gap-y-8">
                    <div className="col-span-1 md:col-span-2 flex items-center space-x-8 mb-4">
                       <div className="w-28 h-28 bg-gray-50 border-2 border-dashed border-gray-200 rounded-[2rem] flex flex-col items-center justify-center text-gray-400 shadow-inner">
                          <User size={40} />
                          <span className="text-[9px] mt-2 font-black tracking-widest">ẢNH ĐẠI DIỆN</span>
                       </div>
                       <div className="flex-1 grid grid-cols-1 md:grid-cols-2 gap-6">
                          <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                             <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Họ tên nhân viên *</label>
                             <input type="text" className="w-full py-2 outline-none text-lg font-black text-gray-800 bg-transparent" placeholder="VD: NGUYỄN VĂN A" value={newEmployee.fullName} onChange={ev => setNewEmployee({...newEmployee, fullName: ev.target.value})} required/>
                          </div>
                          <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                             <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Mã nhân viên</label>
                             <input type="text" className="w-full py-2 outline-none font-black text-blue-600 italic bg-transparent" placeholder="TỰ ĐỘNG" value={newEmployee.employeeCode} onChange={ev => setNewEmployee({...newEmployee, employeeCode: ev.target.value})}/>
                          </div>
                       </div>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Số điện thoại</label>
                       <input type="text" className="w-full py-2 outline-none font-bold text-gray-700 bg-transparent" placeholder="09xxxx" value={newEmployee.phoneNumber} onChange={ev => setNewEmployee({...newEmployee, phoneNumber: ev.target.value})}/>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Số CMND/CCCD</label>
                       <input type="text" className="w-full py-2 outline-none font-bold text-gray-700 bg-transparent" value={newEmployee.citizenId} onChange={ev => setNewEmployee({...newEmployee, citizenId: ev.target.value})}/>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Cơ sở làm việc *</label>
                       <div className="flex items-center">
                          <Store size={14} className="mr-3 text-blue-500" />
                          <select
                            className="w-full py-2 outline-none bg-transparent font-black text-gray-700"
                            value={newEmployee.branchId}
                            onChange={ev => setNewEmployee({...newEmployee, branchId: ev.target.value})}
                            required
                          >
                             <option value="">Chọn chi nhánh</option>
                             {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
                          </select>
                       </div>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Chức danh</label>
                       <select className="w-full py-2 outline-none bg-transparent font-black text-gray-700" value={newEmployee.position} onChange={ev => setNewEmployee({...newEmployee, position: ev.target.value})}>
                          {positions.map(p => <option key={p} value={p}>{p}</option>)}
                       </select>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Loại nhân viên</label>
                       <input type="text" className="w-full py-2 outline-none font-bold text-gray-700 bg-transparent" value={newEmployee.employeeType} onChange={ev => setNewEmployee({...newEmployee, employeeType: ev.target.value})}/>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Lương cơ bản</label>
                       <input type="number" className="w-full py-2 outline-none font-bold text-gray-700 bg-transparent" value={newEmployee.basicSalary} onChange={ev => setNewEmployee({...newEmployee, basicSalary: Number(ev.target.value)})}/>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Quyền hệ thống</label>
                       <select className="w-full py-2 outline-none bg-transparent font-black text-blue-600" value={newEmployee.role} onChange={ev => setNewEmployee({...newEmployee, role: ev.target.value})}>
                          {roles.map(r => (
                            <option key={r.value} value={r.value}>{r.label}</option>
                          ))}
                       </select>
                    </div>

                    <div className="border-b-2 border-gray-100 focus-within:border-blue-500 transition-all pb-1">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest">Ngày vào làm</label>
                       <input type="date" className="w-full py-2 outline-none font-bold text-gray-700 bg-transparent" value={newEmployee.startDate} onChange={ev => setNewEmployee({...newEmployee, startDate: ev.target.value})}/>
                    </div>

                    {formError && <div className="col-span-1 md:col-span-2"><Feedback tone="error" onDismiss={() => setFormError('')}>{formError}</Feedback></div>}
                    <div className="col-span-1 md:col-span-2 bg-blue-50/50 p-8 rounded-[2rem] border border-blue-100 grid grid-cols-2 gap-8 mt-4 shadow-inner">
                       <div className="col-span-2 flex items-center text-blue-700 font-black text-xs uppercase tracking-[0.2em] italic">
                          <Key size={16} className="mr-3" /> Tài khoản truy cập
                       </div>
                       <div className="border-b-2 border-blue-200">
                          <label className="block text-[9px] text-blue-500 font-black uppercase tracking-widest">Tên đăng nhập</label>
                          <input type="text" className="w-full py-2 outline-none font-black bg-transparent text-gray-800" placeholder="VD: t_nghia" value={newEmployee.username} onChange={ev => setNewEmployee({...newEmployee, username: ev.target.value})}/>
                       </div>
                       <FormField
                         label="Mật khẩu"
                         type="password"
                         placeholder="••••••••"
                         value={newEmployee.password}
                         onChange={ev => setNewEmployee({...newEmployee, password: ev.target.value})}
                         minLength={8}
                         maxLength={128}
                         helperText="8-128 ký tự; để trống khi giữ mật khẩu hiện tại."
                         className="bg-transparent"
                       />
                    </div>
                 </div>

                 <div className="w-full md:w-80 bg-gray-50 p-10 border-l border-gray-100 flex flex-col shadow-inner">
                    <div className="flex items-center space-x-3 text-blue-600 mb-8 font-black text-xs uppercase tracking-widest italic">
                       <Info size={18} /> <span>Quy định nội bộ</span>
                    </div>
                    <ul className="space-y-6 text-[11px] text-gray-500 leading-relaxed italic font-medium">
                       <li className="flex items-start"><div className="w-1.5 h-1.5 bg-blue-400 rounded-full mt-1.5 mr-3 shrink-0"></div> Cập nhật đúng cơ sở làm việc để nhân viên xem được lịch ca và thông báo từ Web Khách.</li>
                       <li className="flex items-start"><div className="w-1.5 h-1.5 bg-blue-400 rounded-full mt-1.5 mr-3 shrink-0"></div> Tài khoản sẽ tự động bị khóa nếu nhân viên bị chuyển sang trạng thái "Đã nghỉ việc".</li>
                    </ul>

                    <div className="mt-auto space-y-4 pt-10">
                       <button onClick={() => setIsModalOpen(false)} type="button" className="w-full rounded-full border-2 border-gray-200 bg-white py-4 text-[10px] font-black uppercase tracking-[0.2em] text-gray-400 shadow-sm transition-all hover:bg-white hover:text-gray-600 active:scale-95">BỎ QUA</button>
                       <button type="submit" disabled={isSaving} className="w-full rounded-full bg-[#0070f4] py-4 text-[10px] font-black uppercase tracking-[0.2em] text-white shadow-xl shadow-blue-500/30 transition-all hover:bg-blue-700 active:scale-95 disabled:cursor-wait disabled:opacity-60">{isSaving ? 'ĐANG LƯU...' : 'LƯU HỒ SƠ'}</button>
                    </div>
                 </div>
              </form>
           </div>
        </div>
      )}
    </div>
  );
};

export default EmployeeManagement;

