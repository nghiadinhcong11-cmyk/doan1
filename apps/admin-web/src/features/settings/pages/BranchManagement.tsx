import React, { useState, useEffect } from 'react';
import { Plus, Search, Store, MapPin, Phone, Globe, Edit2, Trash2, CheckCircle2, X, Loader2, Save, Lock, Unlock } from 'lucide-react';
import { API_URL } from '../../../config';
import { notifyFeedback } from '../../../components/ui';

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
  imageUrl?: string;
  taxCode?: string;
  representativeName?: string;
  representativeEmail?: string;
  industry?: string;
  businessType?: string;
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
    accountHolder: '',
    taxCode: '',
    representativeName: '',
    representativeEmail: '',
    industry: '',
    businessType: ''
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
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
      return;
    }
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

      if (response.status === 403) {
        notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
        return;
      }

      if (response.ok) {
        setIsModalOpen(false);
        setEditingBranch(null);
        resetForm();
        fetchBranches();
      } else {
        const error = await response.json();
        notifyFeedback(error.message || 'Lỗi khi lưu chi nhánh');
      }
    } catch (err) {
      notifyFeedback('Lỗi kết nối đến server');
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
      accountHolder: '',
      imageUrl: '',
      taxCode: '',
      representativeName: '',
      representativeEmail: '',
      industry: '',
      businessType: ''
    });
  };

  const openEditModal = (branch: Branch) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
      return;
    }
    setEditingBranch(branch);
    setNewBranch({
      name: branch.name,
      address: branch.address || '',
      phoneNumber: branch.phoneNumber || '',
      isMain: branch.isMain,
      isActive: branch.isActive,
      bankName: branch.bankName || '',
      accountNumber: branch.accountNumber || '',
      accountHolder: branch.accountHolder || '',
      imageUrl: branch.imageUrl || '',
      taxCode: branch.taxCode || '',
      representativeName: branch.representativeName || '',
      representativeEmail: branch.representativeEmail || '',
      industry: branch.industry || '',
      businessType: branch.businessType || ''
    });
    setIsModalOpen(true);
  };

  const handleDeleteBranch = async (id: string) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
      return;
    }
    if (!window.confirm('Bạn có chắc chắn muốn xóa chi nhánh này?')) return;
    try {
      const response = await fetch(`${API_URL}/api/Branch/${id}`, {
        method: 'DELETE'
      });
      if (response.status === 403) {
        notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
        return;
      }
      if (response.ok) {
        fetchBranches();
      } else {
        const error = await response.json();
        notifyFeedback(error.message || 'Lỗi khi xóa chi nhánh');
      }
    } catch (err) {
      notifyFeedback('Lỗi khi xóa chi nhánh');
    }
  };

  const handleToggleStatus = async (id: string) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
      return;
    }
    try {
      const response = await fetch(`${API_URL}/api/Branch/${id}/toggle-status`, {
        method: 'PATCH'
      });
      if (response.status === 403) {
        notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
        return;
      }
      if (response.ok) {
        fetchBranches();
      }
    } catch (err) {
      notifyFeedback('Lỗi khi cập nhật trạng thái');
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
    <div className="flex h-[calc(100vh-48px)] bg-[#f8f9fa] text-[13px] font-sans">
      <div className="w-64 bg-white border-r p-5 space-y-8 shadow-sm">
        <h2 className="font-black text-lg text-gray-800 uppercase italic tracking-tighter">Cơ cấu tổ chức</h2>

        <div className="space-y-6">
          <div>
            <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">Lọc trạng thái</p>
            <div className="space-y-2 ml-1">
              {[
                { id: 'all', label: 'Tất cả' },
                { id: 'active', label: 'Đang hoạt động' },
                { id: 'inactive', label: 'Ngừng hoạt động' }
              ].map(status => (
                <label key={status.id} className="flex items-center cursor-pointer group">
                  <input
                    type="radio"
                    name="status"
                    className="mr-2 h-3.5 w-3.5 text-[#0070f4] border-gray-300 focus:ring-blue-500"
                    checked={filterStatus === status.id}
                    onChange={() => setFilterStatus(status.id as any)}
                  />
                  <span className={`text-xs ${filterStatus === status.id ? 'text-[#0070f4] font-bold' : 'text-gray-500 group-hover:text-blue-500 transition-colors font-medium'}`}>
                    {status.label}
                  </span>
                </label>
              ))}
            </div>
          </div>

          <div className="pt-6 border-t">
            <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">Phân loại</p>
            <div className="space-y-2 ml-1">
              {[
                { id: 'all', label: 'Tất cả' },
                { id: 'main', label: 'Trụ sở chính' },
                { id: 'branch', label: 'Cửa hàng chi nhánh' }
              ].map(type => (
                <label key={type.id} className="flex items-center cursor-pointer group">
                  <input
                    type="radio"
                    name="type"
                    className="mr-2 h-3.5 w-3.5 text-[#0070f4] border-gray-300 focus:ring-blue-500"
                    checked={filterType === type.id}
                    onChange={() => setFilterType(type.id as any)}
                  />
                  <span className={`text-xs ${filterType === type.id ? 'text-[#0070f4] font-bold' : 'text-gray-500 group-hover:text-blue-500 transition-colors font-medium'}`}>
                    {type.label}
                  </span>
                </label>
              ))}
            </div>
          </div>
        </div>

        <div className="p-4 bg-blue-50 border border-blue-100 rounded-2xl">
           <p className="text-[#0070f4] font-black text-[10px] uppercase tracking-widest mb-1 flex items-center"><Globe size={14} className="mr-2"/> ĐA CHI NHÁNH</p>
           <p className="text-[10px] leading-relaxed italic text-blue-700/70 font-medium">
             Hệ thống hỗ trợ quản lý chuỗi. Doanh thu và kho hàng sẽ được thống kê riêng biệt cho từng cơ sở.
           </p>
        </div>
      </div>

      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="bg-white p-3 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-80">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-1.5 bg-gray-50 border-none rounded-lg outline-none focus:ring-1 focus:ring-blue-500 font-bold text-xs"
              placeholder="Tìm theo tên hoặc địa chỉ chi nhánh..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button
            onClick={() => {
              if (localStorage.getItem('userRole') !== 'admin') {
                notifyFeedback('Bạn không có quyền thực hiện chức năng này.');
                return;
              }
              resetForm(); setEditingBranch(null); setIsModalOpen(true);
            }}
            className="bg-[#0070f4] text-white px-5 py-2 rounded-xl flex items-center font-black text-xs hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/30 active:scale-95 uppercase tracking-widest"
          >
            <Plus size={16} className="mr-2" /> Thêm chi nhánh
          </button>
        </div>

        <div className="p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 overflow-auto bg-[#f8f9fa]">
          {loading ? (
            <div className="col-span-full py-20 flex flex-col items-center justify-center text-blue-600">
               <Loader2 className="animate-spin mb-2" size={32}/>
               <p className="text-[10px] font-black uppercase tracking-widest">Đang tải dữ liệu...</p>
            </div>
          ) : filteredBranches.length === 0 ? (
             <div className="col-span-full py-32 text-center bg-white rounded-3xl border-2 border-dashed border-gray-100">
                <Store size={48} className="mx-auto text-gray-200 mb-4" />
                <p className="text-sm font-bold text-gray-400 uppercase tracking-widest">Không tìm thấy chi nhánh nào</p>
             </div>
          ) : filteredBranches.map(b => (
            <div key={b.id} className={`bg-white rounded-[2rem] shadow-xl shadow-blue-500/5 border-2 p-6 hover:border-blue-500 transition-all relative overflow-hidden group ${!b.isActive ? 'opacity-70 bg-gray-50 border-gray-100' : 'border-white'}`}>
              {b.isMain && (
                <div className="absolute top-0 right-0 bg-[#0070f4] text-white text-[9px] font-black px-4 py-1 rounded-bl-2xl uppercase z-10 tracking-widest italic">
                  Trụ sở chính
                </div>
              )}
              <div className="flex items-start space-x-5">
                <div className={`w-14 h-14 rounded-2xl flex items-center justify-center shrink-0 shadow-lg relative overflow-hidden ${b.isActive ? 'bg-blue-50 text-[#0070f4]' : 'bg-gray-200 text-gray-500'}`}>
                   {b.imageUrl ? (
                      <img src={b.imageUrl} className="absolute inset-0 w-full h-full object-cover" alt="" />
                   ) : (
                      <Store size={28} />
                   )}
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="font-black text-lg text-gray-800 mb-3 truncate leading-none uppercase tracking-tight">{b.name}</h3>
                  <div className="space-y-2.5 text-gray-500">
                    <div className="flex items-center text-[11px] font-medium">
                      <MapPin size={14} className="mr-2 shrink-0 text-blue-400" />
                      <span className="truncate">{b.address || 'Chưa cập nhật địa chỉ'}</span>
                    </div>
                    <div className="flex items-center text-[11px] font-medium">
                      <Phone size={14} className="mr-2 shrink-0 text-blue-400" />
                      <span>{b.phoneNumber || '---'}</span>
                    </div>
                    <div className={`flex items-center text-[10px] font-black uppercase tracking-wider ${b.isActive ? 'text-green-600' : 'text-gray-400'}`}>
                      {b.isActive ? <CheckCircle2 size={14} className="mr-2 shrink-0" /> : <Lock size={14} className="mr-2 shrink-0" />}
                      <span>{b.isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="mt-8 pt-5 border-t border-gray-50 flex justify-end space-x-2 opacity-0 group-hover:opacity-100 transition-all">
                <button
                  onClick={() => openEditModal(b)}
                  className="px-4 py-2 text-[#0070f4] bg-blue-50 hover:bg-blue-100 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all"
                >
                  <Edit2 size={12} className="mr-1 inline" /> Sửa
                </button>
                <button
                  onClick={() => handleToggleStatus(b.id!)}
                  className={`px-4 py-2 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all ${b.isActive ? 'text-orange-500 bg-orange-50 hover:bg-orange-100' : 'text-green-600 bg-green-50 hover:bg-green-100'}`}
                >
                  {b.isActive ? <Lock size={12} className="mr-1 inline" /> : <Unlock size={12} className="mr-1 inline" />}
                  {b.isActive ? 'Khóa' : 'Mở'}
                </button>
                {!b.isMain && (
                  <button
                    onClick={() => handleDeleteBranch(b.id!)}
                    className="px-4 py-2 text-white bg-blue-400 hover:bg-blue-600 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all shadow-md shadow-blue-200"
                  >
                    <Trash2 size={12} className="mr-1 inline" /> Xóa
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* MODAL FORM */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/60 z-[300] flex justify-center items-center p-4 backdrop-blur-sm animate-in fade-in duration-200">
           <div className="bg-white w-full max-w-md rounded-[3rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
                 <h3 className="font-black text-xl uppercase italic tracking-tighter flex items-center"><Store size={24} className="mr-3"/> {editingBranch ? 'Cập nhật chi nhánh' : 'Thêm chi nhánh mới'}</h3>
                 <button onClick={() => setIsModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-transform"><X size={24}/></button>
              </div>

              <form onSubmit={handleSaveBranch} className="p-10 space-y-6">
                 <div className="space-y-1">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Tên chi nhánh <span className="text-blue-500">*</span></label>
                    <input
                      type="text"
                      className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-black text-gray-700 transition-all"
                      placeholder="VD: Chi nhánh Quận 1"
                      value={newBranch.name}
                      onChange={e => setNewBranch({...newBranch, name: e.target.value})}
                      required
                    />
                 </div>

                 <div className="space-y-1">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Link ảnh chi nhánh</label>
                    <input
                      type="text"
                      className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all"
                      placeholder="Dán link ảnh (https://...)"
                      value={newBranch.imageUrl}
                      onChange={e => setNewBranch({...newBranch, imageUrl: e.target.value})}
                    />
                 </div>
                 <div className="grid grid-cols-2 gap-6">
                    <div className="space-y-1">
                        <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Số điện thoại</label>
                        <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700"
                        placeholder="028.xxxx.xxxx"
                        value={newBranch.phoneNumber}
                        onChange={e => setNewBranch({...newBranch, phoneNumber: e.target.value})}
                        />
                    </div>
                    <div className="space-y-1">
                        <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Loại hình</label>
                        <div className="pt-2 flex items-center">
                           <label className="flex items-center cursor-pointer mr-4">
                              <input
                                type="checkbox"
                                className="mr-2 h-4 w-4 text-blue-600 rounded"
                                checked={newBranch.isMain}
                                onChange={e => setNewBranch({...newBranch, isMain: e.target.checked})}
                              />
                              <span className="text-[11px] font-black text-gray-600 uppercase">Trụ sở</span>
                           </label>
                           <input
                              type="text"
                              className="flex-1 border-b border-gray-100 py-1 outline-none focus:border-blue-500 text-[10px] font-bold text-gray-600"
                              placeholder="Cá nhân/Doanh nghiệp..."
                              value={newBranch.businessType}
                              onChange={e => setNewBranch({...newBranch, businessType: e.target.value})}
                           />
                        </div>
                    </div>
                 </div>
                 <div className="space-y-1">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Địa chỉ</label>
                    <textarea
                      className="w-full border-2 border-gray-100 rounded-2xl p-4 outline-none focus:border-blue-500 text-xs font-bold text-gray-600 h-24 mt-2"
                      placeholder="Địa chỉ cụ thể của cơ sở..."
                      value={newBranch.address}
                      onChange={e => setNewBranch({...newBranch, address: e.target.value})}
                    />
                 </div>

                 <div className="grid grid-cols-2 gap-6">
                    <div className="space-y-1">
                        <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Người đại diện</label>
                        <input
                          type="text"
                          className="w-full border-b-2 border-gray-100 py-1.5 focus:border-blue-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                          placeholder="Tên chủ cơ sở..."
                          value={newBranch.representativeName}
                          onChange={e => setNewBranch({...newBranch, representativeName: e.target.value})}
                        />
                    </div>
                    <div className="space-y-1">
                        <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Mã số thuế</label>
                        <input
                          type="text"
                          className="w-full border-b-2 border-gray-100 py-1.5 focus:border-blue-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                          placeholder="MST..."
                          value={newBranch.taxCode}
                          onChange={e => setNewBranch({...newBranch, taxCode: e.target.value})}
                        />
                    </div>
                 </div>

                 <div className="grid grid-cols-2 gap-6">
                    <div className="space-y-1">
                        <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Email đại diện</label>
                        <input
                          type="email"
                          className="w-full border-b-2 border-gray-100 py-1.5 focus:border-blue-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                          placeholder="owner@example.com"
                          value={newBranch.representativeEmail}
                          onChange={e => setNewBranch({...newBranch, representativeEmail: e.target.value})}
                        />
                    </div>
                    <div className="space-y-1">
                        <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Ngành nghề</label>
                        <input
                          type="text"
                          className="w-full border-b-2 border-gray-100 py-1.5 focus:border-blue-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                          placeholder="VD: F&B..."
                          value={newBranch.industry}
                          onChange={e => setNewBranch({...newBranch, industry: e.target.value})}
                        />
                    </div>
                 </div>

                 <div className="bg-gray-50 p-6 rounded-[2rem] space-y-4 border border-gray-100 shadow-inner">
                    <p className="text-[10px] font-black text-blue-600 uppercase tracking-widest">Thông tin chuyển khoản (VietQR)</p>
                    <div className="grid grid-cols-2 gap-4">
                       <div className="space-y-1">
                          <label className="text-[10px] font-black text-gray-400 uppercase">Ngân hàng</label>
                          <input
                            type="text"
                            className="w-full border-b-2 border-transparent bg-transparent py-1 outline-none focus:border-blue-300 text-xs font-bold"
                            placeholder="VD: MB Bank..."
                            value={newBranch.bankName}
                            onChange={e => setNewBranch({...newBranch, bankName: e.target.value})}
                          />
                       </div>
                       <div className="space-y-1">
                          <label className="text-[10px] font-black text-gray-400 uppercase">Số tài khoản</label>
                          <input
                            type="text"
                            className="w-full border-b-2 border-transparent bg-transparent py-1 outline-none focus:border-blue-300 text-xs font-bold"
                            placeholder="Số tài khoản..."
                            value={newBranch.accountNumber}
                            onChange={e => setNewBranch({...newBranch, accountNumber: e.target.value})}
                          />
                       </div>
                    </div>
                 </div>

                 <button
                   type="submit"
                   className="w-full py-5 bg-[#0070f4] text-white rounded-3xl font-black uppercase tracking-[0.2em] shadow-xl shadow-blue-500/30 hover:bg-blue-700 active:scale-95 transition-all flex items-center justify-center text-xs"
                 >
                   <Save size={18} className="mr-2"/> XÁC NHẬN LƯU CƠ SỞ
                 </button>
              </form>
           </div>
        </div>
      )}
    </div>
  );
};

export default BranchManagement;

