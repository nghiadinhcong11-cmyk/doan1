import React, { useState, useEffect } from 'react';
import { Plus, Search, Store, MapPin, Phone, Globe, Edit2, Trash2, CheckCircle2, X, Loader2, Save, Lock, Unlock } from 'lucide-react';
import { API_URL } from '../config';

interface Branch {
  id?: string;
  name: string;
  address?: string;
  phoneNumber?: string;
  isMain: boolean;
  isActive: boolean;
  bankName?: string;
  accountNumber?: string;
  accountHolder?: string;
  createdAt?: string;
}

const BranchManagement = () => {
  const [branches, setBranches] = useState<Branch[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingBranch, setEditingBranch] = useState<Branch | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all');
  const [filterType, setFilterType] = useState<'all' | 'main' | 'branch'>('all');

  const [newBranch, setNewBranch] = useState<Branch>({
    name: '',
    address: '',
    phoneNumber: '',
    isMain: false,
    isActive: true,
    bankName: '',
    accountNumber: '',
    accountHolder: ''
  });

  const fetchBranches = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${API_URL}/api/Branch`);
      const data = await response.json();
      setBranches(data);
    } catch (err) {
      console.error('Error fetching branches:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBranches();
  }, []);

  const handleSaveBranch = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const isEditing = !!editingBranch;
      const url = isEditing
        ? `${API_URL}/api/Branch/${editingBranch.id}`
        : `${API_URL}/api/Branch`;

      const payload = isEditing
        ? { ...newBranch, id: editingBranch.id, createdAt: editingBranch.createdAt }
        : newBranch;

      const response = await fetch(url, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        setIsModalOpen(false);
        setEditingBranch(null);
        resetForm();
        fetchBranches();
      } else {
        const error = await response.json();
        alert(error.message || 'Lỗi khi lưu chi nhánh');
      }
    } catch (err) {
      alert('Lỗi kết nối đến server');
    }
  };

  const resetForm = () => {
    setNewBranch({
      name: '',
      address: '',
      phoneNumber: '',
      isMain: false,
      isActive: true,
      bankName: '',
      accountNumber: '',
      accountHolder: ''
    });
  };

  const openEditModal = (branch: Branch) => {
    setEditingBranch(branch);
    setNewBranch({
      name: branch.name,
      address: branch.address || '',
      phoneNumber: branch.phoneNumber || '',
      isMain: branch.isMain,
      isActive: branch.isActive,
      bankName: branch.bankName || '',
      accountNumber: branch.accountNumber || '',
      accountHolder: branch.accountHolder || ''
    });
    setIsModalOpen(true);
  };

  const handleDeleteBranch = async (id: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa chi nhánh này?')) return;
    try {
      const response = await fetch(`${API_URL}/api/Branch/${id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        fetchBranches();
      } else {
        const error = await response.json();
        alert(error.message || 'Lỗi khi xóa chi nhánh');
      }
    } catch (err) {
      alert('Lỗi khi xóa chi nhánh');
    }
  };

  const handleToggleStatus = async (id: string) => {
    try {
      const response = await fetch(`${API_URL}/api/Branch/${id}/toggle-status`, {
        method: 'PATCH'
      });
      if (response.ok) {
        fetchBranches();
      }
    } catch (err) {
      alert('Lỗi khi cập nhật trạng thái');
    }
  };

  const filteredBranches = branches.filter(b => {
    const matchesSearch = b.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         (b.address && b.address.toLowerCase().includes(searchTerm.toLowerCase()));

    const matchesStatus = filterStatus === 'all' ||
                         (filterStatus === 'active' && b.isActive) ||
                         (filterStatus === 'inactive' && !b.isActive);

    const matchesType = filterType === 'all' ||
                       (filterType === 'main' && b.isMain) ||
                       (filterType === 'branch' && !b.isMain);

    return matchesSearch && matchesStatus && matchesType;
  });

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      <div className="w-64 bg-white border-r p-4 space-y-6">
        <h2 className="font-bold text-base text-gray-800">Quản lý chi nhánh</h2>

        <div className="space-y-4">
          <div>
            <p className="text-[11px] font-bold text-gray-400 uppercase mb-2">Trạng thái</p>
            <div className="space-y-1.5 ml-1">
              {[
                { id: 'all', label: 'Tất cả' },
                { id: 'active', label: 'Đang hoạt động' },
                { id: 'inactive', label: 'Ngừng hoạt động' }
              ].map(status => (
                <label key={status.id} className="flex items-center cursor-pointer group">
                  <input
                    type="radio"
                    name="status"
                    className="mr-2 h-3.5 w-3.5 text-blue-600 border-gray-300 focus:ring-blue-500"
                    checked={filterStatus === status.id}
                    onChange={() => setFilterStatus(status.id as any)}
                  />
                  <span className={`text-xs ${filterStatus === status.id ? 'text-blue-600 font-bold' : 'text-gray-600 group-hover:text-blue-500 transition-colors'}`}>
                    {status.label}
                  </span>
                </label>
              ))}
            </div>
          </div>

          <div className="pt-4 border-t">
            <p className="text-[11px] font-bold text-gray-400 uppercase mb-2">Loại chi nhánh</p>
            <div className="space-y-1.5 ml-1">
              {[
                { id: 'all', label: 'Tất cả' },
                { id: 'main', label: 'Trụ sở chính' },
                { id: 'branch', label: 'Chi nhánh con' }
              ].map(type => (
                <label key={type.id} className="flex items-center cursor-pointer group">
                  <input
                    type="radio"
                    name="type"
                    className="mr-2 h-3.5 w-3.5 text-blue-600 border-gray-300 focus:ring-blue-500"
                    checked={filterType === type.id}
                    onChange={() => setFilterType(type.id as any)}
                  />
                  <span className={`text-xs ${filterType === type.id ? 'text-blue-600 font-bold' : 'text-gray-600 group-hover:text-blue-500 transition-colors'}`}>
                    {type.label}
                  </span>
                </label>
              ))}
            </div>
          </div>
        </div>

        <div className="p-3 bg-blue-50 border border-blue-100 rounded-lg text-blue-800">
           <p className="font-bold mb-1 flex items-center"><Globe size={14} className="mr-1.5"/> Thông tin</p>
           <p className="text-[11px] leading-relaxed italic text-blue-600">
             Hệ thống hỗ trợ quản lý đa chi nhánh. Bạn có thể thiết lập kho hàng và báo cáo doanh thu riêng biệt cho từng cơ sở.
           </p>
        </div>
      </div>

      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="bg-white p-3 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-80">
            <Search className="absolute left-3 top-2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-1.5 bg-gray-100 border-none rounded-md outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Tìm theo tên hoặc địa chỉ chi nhánh..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button
            onClick={() => { resetForm(); setEditingBranch(null); setIsModalOpen(true); }}
            className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700 transition-colors"
          >
            <Plus size={16} className="mr-1" /> Thêm chi nhánh
          </button>
        </div>

        <div className="p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 overflow-auto">
          {loading ? (
            <div className="col-span-full py-20 flex justify-center"><Loader2 className="animate-spin text-blue-600" size={32}/></div>
          ) : filteredBranches.map(b => (
            <div key={b.id} className={`bg-white rounded-xl shadow-sm border p-5 hover:shadow-md transition-all relative overflow-hidden group ${!b.isActive ? 'opacity-70 bg-gray-50' : 'border-gray-200'}`}>
              {b.isMain && (
                <div className="absolute top-0 right-0 bg-orange-500 text-white text-[10px] font-bold px-3 py-1 rounded-bl-lg uppercase z-10">
                  Trụ sở chính
                </div>
              )}
              <div className="flex items-start space-x-4">
                <div className={`w-12 h-12 rounded-lg flex items-center justify-center shrink-0 ${b.isActive ? 'bg-blue-50 text-blue-600' : 'bg-gray-200 text-gray-500'}`}>
                  <Store size={24} />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="font-bold text-base text-gray-800 mb-2 truncate">{b.name}</h3>
                  <div className="space-y-2 text-gray-500">
                    <div className="flex items-center text-[12px]">
                      <MapPin size={14} className="mr-2 shrink-0 text-gray-400" />
                      <span className="truncate">{b.address || 'Chưa cập nhật địa chỉ'}</span>
                    </div>
                    <div className="flex items-center text-[12px]">
                      <Phone size={14} className="mr-2 shrink-0 text-gray-400" />
                      <span>{b.phoneNumber || '---'}</span>
                    </div>
                    <div className={`flex items-center text-[12px] font-bold ${b.isActive ? 'text-green-600' : 'text-gray-400'}`}>
                      {b.isActive ? <CheckCircle2 size={14} className="mr-2 shrink-0" /> : <Lock size={14} className="mr-2 shrink-0" />}
                      <span>{b.isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="mt-6 pt-4 border-t flex justify-end space-x-1 opacity-0 group-hover:opacity-100 transition-opacity">
                <button
                  onClick={() => openEditModal(b)}
                  className="p-2 text-blue-600 hover:bg-blue-50 rounded font-bold text-[11px] flex items-center"
                >
                  <Edit2 size={14} className="mr-1" /> Sửa
                </button>
                <button
                  onClick={() => handleToggleStatus(b.id!)}
                  className={`p-2 rounded font-bold text-[11px] flex items-center ${b.isActive ? 'text-orange-500 hover:bg-orange-50' : 'text-green-600 hover:bg-green-50'}`}
                >
                  {b.isActive ? <Lock size={14} className="mr-1" /> : <Unlock size={14} className="mr-1" />}
                  {b.isActive ? 'Khóa' : 'Mở'}
                </button>
                {!b.isMain && (
                  <button
                    onClick={() => handleDeleteBranch(b.id!)}
                    className="p-2 text-red-500 hover:bg-red-50 rounded font-bold text-[11px] flex items-center"
                  >
                    <Trash2 size={14} className="mr-1" /> Xóa
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* MODAL FORM */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/40 z-[100] flex justify-center items-center p-4">
           <div className="bg-white w-full max-w-md rounded-xl shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-[#0070f4] p-4 text-white flex justify-between items-center">
                 <h3 className="font-bold text-lg flex items-center"><Store size={20} className="mr-2"/> {editingBranch ? 'Cập nhật chi nhánh' : 'Thêm chi nhánh mới'}</h3>
                 <button onClick={() => setIsModalOpen(false)} className="hover:rotate-90 transition-transform"><X size={24}/></button>
              </div>

              <form onSubmit={handleSaveBranch} className="p-6 space-y-5">
                 <div className="space-y-1">
                    <label className="text-[11px] font-bold text-gray-400 uppercase">Tên chi nhánh</label>
                    <input
                      type="text"
                      className="w-full border-b py-2 outline-none focus:border-blue-500 font-medium"
                      placeholder="VD: Chi nhánh Quận 1"
                      value={newBranch.name}
                      onChange={e => setNewBranch({...newBranch, name: e.target.value})}
                      required
                    />
                 </div>
                 <div className="space-y-1">
                    <label className="text-[11px] font-bold text-gray-400 uppercase">Số điện thoại</label>
                    <input
                      type="text"
                      className="w-full border-b py-2 outline-none focus:border-blue-500 font-medium"
                      placeholder="028.xxxx.xxxx"
                      value={newBranch.phoneNumber}
                      onChange={e => setNewBranch({...newBranch, phoneNumber: e.target.value})}
                    />
                 </div>
                 <div className="space-y-1">
                    <label className="text-[11px] font-bold text-gray-400 uppercase">Địa chỉ</label>
                    <textarea
                      className="w-full border rounded-lg p-3 outline-none focus:border-blue-500 text-xs h-20"
                      placeholder="Địa chỉ cụ thể của cơ sở..."
                      value={newBranch.address}
                      onChange={e => setNewBranch({...newBranch, address: e.target.value})}
                    />
                 </div>
                 <div className="flex items-center space-x-6 pt-2">
                    <label className="flex items-center cursor-pointer">
                       <input
                         type="checkbox"
                         className="mr-2 h-4 w-4 text-blue-600 rounded"
                         checked={newBranch.isMain}
                         onChange={e => setNewBranch({...newBranch, isMain: e.target.checked})}
                       />
                       <span className="font-bold text-gray-700">Trụ sở chính</span>
                    </label>
                    <label className="flex items-center cursor-pointer">
                       <input
                         type="checkbox"
                         className="mr-2 h-4 w-4 text-blue-600 rounded"
                         checked={newBranch.isActive}
                         onChange={e => setNewBranch({...newBranch, isActive: e.target.checked})}
                       />
                       <span className="font-bold text-gray-700">Đang hoạt động</span>
                    </label>
                 </div>

                 <div className="bg-gray-50 p-4 rounded-lg space-y-4 border border-gray-200">
                    <p className="text-[11px] font-bold text-blue-600 uppercase">Thông tin chuyển khoản (VietQR)</p>
                    <div className="grid grid-cols-2 gap-4">
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-gray-400 uppercase">Ngân hàng</label>
                          <input
                            type="text"
                            className="w-full border-b bg-transparent py-1 outline-none focus:border-blue-500 text-sm"
                            placeholder="VD: MB Bank, VCB..."
                            value={newBranch.bankName}
                            onChange={e => setNewBranch({...newBranch, bankName: e.target.value})}
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-bold text-gray-400 uppercase">Số tài khoản</label>
                          <input
                            type="text"
                            className="w-full border-b bg-transparent py-1 outline-none focus:border-blue-500 text-sm"
                            placeholder="Số tài khoản..."
                            value={newBranch.accountNumber}
                            onChange={e => setNewBranch({...newBranch, accountNumber: e.target.value})}
                          />
                       </div>
                    </div>
                    <div className="space-y-1">
                       <label className="text-[10px] font-bold text-gray-400 uppercase">Chủ tài khoản</label>
                       <input
                         type="text"
                         className="w-full border-b bg-transparent py-1 outline-none focus:border-blue-500 text-sm"
                         placeholder="Tên người thụ hưởng..."
                         value={newBranch.accountHolder}
                         onChange={e => setNewBranch({...newBranch, accountHolder: e.target.value})}
                       />
                    </div>
                 </div>

                 <div className="pt-4 flex justify-end space-x-3">
                   <button
                     type="button"
                     onClick={() => setIsModalOpen(false)}
                     className="px-6 py-2 border rounded-lg font-bold text-gray-600 hover:bg-gray-50 transition-colors"
                   >
                     Bỏ qua
                   </button>
                   <button
                     type="submit"
                     className="px-6 py-2 bg-[#0070f4] text-white rounded-lg font-bold shadow-lg hover:bg-blue-700 active:scale-95 transition-all flex items-center"
                   >
                     <Save size={16} className="mr-2"/> {editingBranch ? 'Cập nhật' : 'Lưu cơ sở'}
                   </button>
                 </div>
              </form>
           </div>
        </div>
      )}
    </div>
  );
};

export default BranchManagement;
