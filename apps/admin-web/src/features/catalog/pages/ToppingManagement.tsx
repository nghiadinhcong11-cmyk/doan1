import React, { useState, useEffect, useRef } from 'react';
import { Plus, Trash2, Edit2, Loader2, X, Settings2, Search, Image as ImageIcon, Camera, CheckCircle2, MoreHorizontal, Filter, Utensils, Download, Upload } from 'lucide-react';
import { API_URL } from '../../../config';
import { getCachedJson, invalidateCachedJson } from '../../../api/queryCache';
import { notifyFeedback } from '../../../components/ui';

interface Topping {
  id?: string;
  name: string;
  price: number;
  costPrice: number;
  toppingType: string;
  category: string;
  description: string;
  imageUrl: string;
  isActive: boolean;
  availabilityStatus?: 'Available' | 'TemporarilyUnavailable' | 'OutOfStock' | string;
}

interface Branch {
  id: string;
  name: string;
  isActive: boolean;
}

const csvEscape = (value: unknown) => {
  const text = value == null ? '' : String(value);
  return /[",\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
};

const parseCsvLine = (line: string) => {
  const values: string[] = [];
  let value = '';
  let quoted = false;
  for (let index = 0; index < line.length; index += 1) {
    const character = line[index];
    if (character === '"' && line[index + 1] === '"' && quoted) {
      value += '"';
      index += 1;
    } else if (character === '"') {
      quoted = !quoted;
    } else if (character === ',' && !quoted) {
      values.push(value.trim());
      value = '';
    } else {
      value += character;
    }
  }
  values.push(value.trim());
  return values;
};

const ToppingManagement = () => {
  const [toppings, setToppings] = useState<Topping[]>([]);
  const [loading, setLoading] = useState(true);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [selectedBranchId, setSelectedBranchId] = useState(() => localStorage.getItem('selectedBranchId') || '');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTopping, setEditingTopping] = useState<Topping | null>(null);

  // Sidebar Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState(''); // Topping Đồ uống, Đồ ăn...
  const [filterCategory, setFilterGroup] = useState(''); // Nhóm cụ thể
  const [groups, setGroups] = useState<string[]>(['Trân châu', 'Kem Cheese', 'Thạch/Pudding']);
  const [isManageGroupsMode, setIsManageGroupsMode] = useState(false);

  // Form State
  const [formData, setFormData] = useState<Topping>({
    name: '',
    price: 0,
    costPrice: 0,
    toppingType: 'Topping Đồ uống',
    category: 'Trân châu',
    description: '',
    imageUrl: '',
    isActive: true
  });

  const fileInputRef = useRef<HTMLInputElement>(null);
  const importInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    void fetchBranches();
    void fetchToppings();
    const handleCatalogChanged = () => void fetchToppings();
    const handleBranchChanged = () => setSelectedBranchId(localStorage.getItem('selectedBranchId') || '');
    const handleAvailabilityChanged = (event: Event) => {
      const detail = (event as CustomEvent<{ id?: string; availabilityStatus?: string; branchId?: string }>).detail;
      if (!detail?.id || !detail.availabilityStatus || detail.branchId !== (localStorage.getItem('selectedBranchId') || '')) return;
      invalidateCachedJson('/api/Topping');
      setToppings(previous => previous.map(topping => topping.id === detail.id ? { ...topping, availabilityStatus: detail.availabilityStatus } : topping));
    };
    window.addEventListener('catalog-item-created', handleCatalogChanged);
    window.addEventListener('branch:changed', handleBranchChanged);
    window.addEventListener('topping-availability-changed', handleAvailabilityChanged);
    return () => {
      window.removeEventListener('catalog-item-created', handleCatalogChanged);
      window.removeEventListener('branch:changed', handleBranchChanged);
      window.removeEventListener('topping-availability-changed', handleAvailabilityChanged);
    };
  }, []);

  useEffect(() => {
    void fetchToppings();
  }, [selectedBranchId]);

  const fetchBranches = async () => {
    try {
      const data = await getCachedJson<Branch[]>(`${API_URL}/api/Branch/management`);
      const activeBranches = data.filter(branch => branch.isActive !== false);
      setBranches(activeBranches);
      const currentIsVisible = activeBranches.some(branch => branch.id === selectedBranchId);
      if ((!selectedBranchId || !currentIsVisible) && activeBranches[0]?.id) {
        setSelectedBranchId(activeBranches[0].id);
        localStorage.setItem('selectedBranchId', activeBranches[0].id);
      }
    } catch (error) {
      console.error('Error fetching branches:', error);
    }
  };

  const fetchToppings = async () => {
    try {
      setLoading(true);
      const query = selectedBranchId ? `?branchId=${encodeURIComponent(selectedBranchId)}` : '';
      const data = await getCachedJson<Topping[]>(`${API_URL}/api/Topping${query}`);
      setToppings(data);

      // Cập nhật danh sách nhóm từ dữ liệu thực tế
      const uniqueGroups = Array.from(new Set(data.map((t: any) => t.category).filter((g: any) => !!g))) as string[];
      if (uniqueGroups.length > 0) {
        setGroups(prev => Array.from(new Set([...prev, ...uniqueGroups])));
      }
    } catch (e) { console.error(e); }
    finally { setLoading(false); }
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setFormData({ ...formData, imageUrl: reader.result as string });
      };
      reader.readAsDataURL(file);
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      price: 0,
      costPrice: 0,
      toppingType: 'Topping Đồ uống',
      category: 'Trân châu',
      description: '',
      imageUrl: '',
      isActive: true
    });
    setEditingTopping(null);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    try {
      const url = editingTopping ? `${API_URL}/api/Topping/${editingTopping.id}` : `${API_URL}/api/Topping`;
      const method = editingTopping ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(editingTopping ? { ...editingTopping, ...formData } : formData)
      });

      if (response.ok) {
        setIsModalOpen(false);
        resetForm();
        invalidateCachedJson('/api/Topping');
        fetchToppings();
      } else if (response.status !== 403) {
        notifyFeedback("Lỗi khi lưu Topping");
      }
    } catch (err) { notifyFeedback('Lỗi kết nối'); }
  };

  const deleteTopping = async (id: string) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    if (!window.confirm('Xóa Topping này?')) return;
    const response = await fetch(`${API_URL}/api/Topping/${id}`, { method: 'DELETE' });
    if (response.ok) { invalidateCachedJson('/api/Topping'); fetchToppings(); }
  };

  const updateAvailability = async (id: string, status: 'Available' | 'TemporarilyUnavailable' | 'OutOfStock') => {
    if (!selectedBranchId) {
      notifyFeedback('Vui lòng chọn chi nhánh trước khi đổi trạng thái topping.');
      return;
    }
    const response = await fetch(`${API_URL}/api/Topping/${id}/availability?branchId=${encodeURIComponent(selectedBranchId)}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status })
    });
    if (!response.ok) {
      notifyFeedback('Không thể cập nhật trạng thái topping tại chi nhánh.');
      return;
    }
    invalidateCachedJson('/api/Topping');
    await fetchToppings();
  };

  const handleExportToppings = () => {
    const headers = ['name', 'price', 'costPrice', 'toppingType', 'category', 'isActive', 'description'];
    const rows = toppings.map(topping => [
      topping.name,
      topping.price,
      topping.costPrice,
      topping.toppingType,
      topping.category,
      topping.isActive ? 'true' : 'false',
      topping.description
    ]);
    const csv = [headers, ...rows].map(row => row.map(csvEscape).join(',')).join('\n');
    const url = URL.createObjectURL(new Blob([`\ufeff${csv}`], { type: 'text/csv;charset=utf-8' }));
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `topping-${new Date().toISOString().slice(0, 10)}.csv`;
    anchor.click();
    URL.revokeObjectURL(url);
    notifyFeedback(`Đã xuất ${toppings.length} topping ra file CSV.`);
  };

  const handleImportToppings = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      notifyFeedback('File CSV quá lớn. Vui lòng chọn file dưới 2 MB.');
      return;
    }
    const lines = (await file.text()).replace(/^\ufeff/, '').split(/\r?\n/).filter(line => line.trim());
    if (lines.length < 2) {
      notifyFeedback('File CSV cần có dòng tiêu đề và ít nhất một topping.');
      return;
    }
    const headers = parseCsvLine(lines[0]).map(header => header.toLowerCase());
    if (!['name', 'price'].every(header => headers.includes(header))) {
      notifyFeedback('CSV phải có các cột bắt buộc: name, price.');
      return;
    }

    let created = 0;
    let failed = 0;
    for (const line of lines.slice(1)) {
      const values = parseCsvLine(line);
      const row = Object.fromEntries(headers.map((header, index) => [header, values[index] ?? '']));
      const price = Number(row.price);
      if (!row.name || !Number.isFinite(price) || price <= 0) {
        failed += 1;
        continue;
      }
      const response = await fetch(`${API_URL}/api/Topping`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: row.name,
          price,
          costPrice: Number(row.costPrice) || 0,
          toppingType: row.toppingType || 'Topping Đồ uống',
          category: row.category || groups[0] || 'Chưa phân loại',
          isActive: row.isActive.toLowerCase() !== 'false',
          description: row.description || ''
        })
      });
      if (response.ok) created += 1;
      else failed += 1;
    }
    notifyFeedback(`Đã nhập ${created} topping${failed ? `, ${failed} dòng bị bỏ qua hoặc lỗi` : ''}.`);
    invalidateCachedJson('/api/Topping');
    await fetchToppings();
  };

  const openEditModal = (topping: Topping) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    setEditingTopping(topping);
    setFormData({ ...topping });
    setIsModalOpen(true);
  };

  const handleEditGroup = async (oldName: string) => {
    const newName = prompt(`Sửa tên nhóm "${oldName}" thành:`, oldName);
    if (newName && newName !== oldName) {
      setGroups(groups.map(g => g === oldName ? newName : g));

      // Cập nhật tất cả Topping thuộc nhóm này (nếu có trong DB)
      const toppingsToUpdate = toppings.filter(t => t.category === oldName);
      if (toppingsToUpdate.length > 0) {
        if (window.confirm(`Có ${toppingsToUpdate.length} Topping thuộc nhóm này. Bạn có muốn cập nhật tên nhóm cho tất cả không?`)) {
          for (const t of toppingsToUpdate) {
            await fetch(`${API_URL}/api/Topping/${t.id}`, {
              method: 'PUT',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ ...t, category: newName })
            });
          }
          invalidateCachedJson('/api/Topping');
          fetchToppings();
        }
      }
    }
  };

  const handleDeleteGroup = (groupName: string) => {
    const hasToppings = toppings.some(t => t.category === groupName);
    if (hasToppings) {
      notifyFeedback('Không thể xóa nhóm này vì vẫn còn Topping đang thuộc nhóm. Hãy đổi nhóm cho Topping trước.');
      return;
    }
    if (window.confirm(`Bạn có chắc chắn muốn xóa nhóm "${groupName}"?`)) {
      setGroups(groups.filter(g => g !== groupName));
    }
  };

  const filteredToppings = toppings.filter(t => {
    const matchesSearch = t.name.toLowerCase().includes(searchTerm.toLowerCase());

    // Xử lý thông minh: Nếu t.toppingType bị null/trống, coi như là 'Khác' hoặc cho phép hiện khi lọc 'Tất cả'
    const currentType = t.toppingType || 'Chưa phân loại';
    const matchesType = filterType === '' || currentType === filterType;

    const matchesCategory = filterCategory === '' || t.category === filterCategory;
    return matchesSearch && matchesType && matchesCategory;
  });

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      {/* SIDEBAR FILTER */}
      <div className="w-64 bg-white border-r overflow-y-auto p-4 space-y-6">
        <div className="flex items-center space-x-2 text-blue-600 mb-2">
           <Settings2 size={18} />
           <h2 className="font-bold text-base">Topping</h2>
        </div>

        {/* Phân loại lớn */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Loại Topping</p>
          <div className="space-y-1">
            {['Tất cả', 'Topping Đồ uống', 'Topping Đồ ăn', 'Chưa phân loại'].map(item => (
              <label key={item} className="flex items-center cursor-pointer group py-0.5">
                <input
                  type="radio"
                  name="type"
                  className="mr-2 h-3.5 w-3.5 border-gray-300 text-blue-600 focus:ring-blue-500"
                  checked={(item === 'Tất cả' && filterType === '') || (filterType === item)}
                  onChange={() => setFilterType(item === 'Tất cả' ? '' : item)}
                />
                <span className={`transition-colors ${(item === 'Tất cả' && filterType === '') || (filterType === item) ? 'text-blue-600 font-bold' : 'text-gray-600 group-hover:text-blue-600'}`}>
                  {item}
                </span>
              </label>
            ))}
          </div>
        </div>

        {/* Nhóm Topping */}
        <div className="space-y-2 pt-4 border-t">
          <div className="flex justify-between items-center">
            <p className="font-bold text-gray-700">Nhóm Topping</p>
            <div className="flex space-x-1">
              <button
                onClick={() => setIsManageGroupsMode(!isManageGroupsMode)}
                className={`p-1 rounded hover:bg-gray-100 transition-colors ${isManageGroupsMode ? 'text-blue-600 bg-blue-50' : 'text-gray-400'}`}
              >
                <Settings2 size={12}/>
              </button>
              <button onClick={() => {const n = prompt('Tên nhóm:'); if(n) setGroups([...groups, n])}} className="text-blue-600 text-[11px] font-bold hover:underline">Tạo mới</button>
            </div>
          </div>

          <div className="space-y-1">
             <div
               onClick={() => setFilterGroup('')}
                className={`px-3 py-2 rounded-xl cursor-pointer transition-all ${filterCategory === '' ? 'bg-blue-50 text-blue-600 font-bold' : 'text-gray-500 hover:bg-gray-50'}`}
             >
                Tất cả nhóm
             </div>
             {groups.map(group => (
               <div
                 key={group}
                  className={`flex items-center justify-between group/grp py-0.5 px-3 rounded-xl hover:bg-blue-50 transition-all ${filterCategory === group ? 'bg-blue-50' : ''}`}
               >
                 <div
                   onClick={() => setFilterGroup(group)}
                    className={`flex-1 cursor-pointer transition-colors ${filterCategory === group ? 'text-blue-600 font-bold' : 'text-gray-500 hover:text-blue-600'}`}
                 >
                   {group}
                 </div>
                 {isManageGroupsMode && (
                    <div className="flex space-x-1 animate-in slide-in-from-right-2 duration-200">
                       <button onClick={() => handleEditGroup(group)} className="p-1 text-gray-400 hover:text-blue-600"><Edit2 size={10}/></button>
                       <button onClick={() => handleDeleteGroup(group)} className="p-1 text-gray-400 hover:text-red-600"><Trash2 size={10}/></button>
                    </div>
                 )}
               </div>
             ))}
          </div>
        </div>
      </div>

      {/* MAIN CONTENT Area */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Actions Bar */}
        <div className="bg-white p-3 flex justify-between items-center border-b gap-3">
          <div className="relative w-80">
            <Search className="absolute left-3 top-2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-1.5 bg-gray-100 border-none rounded-md outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Tìm theo tên Topping hoặc mô tả..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex flex-wrap items-center gap-2">
          <label className="flex items-center gap-2 rounded border border-gray-300 bg-white px-3 py-1.5 text-xs font-bold text-gray-600">
            <span>Chi nhánh:</span>
            <select value={selectedBranchId} onChange={event => { setSelectedBranchId(event.target.value); localStorage.setItem('selectedBranchId', event.target.value); window.dispatchEvent(new Event('branch:changed')); }} className="max-w-[150px] bg-transparent outline-none" disabled={branches.length === 0}>
              {branches.length === 0 && <option value="">Không có dữ liệu</option>}
              {branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}
            </select>
          </label>
          <button
            onClick={() => {
              if (localStorage.getItem('userRole') !== 'admin') {
                notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
                return;
              }
              resetForm(); setIsModalOpen(true);
            }}
            className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700 transition-colors shadow-lg shadow-blue-500/20"
          >
            <Plus size={16} className="mr-1" /> Thêm Topping mới
          </button>
          <button type="button" onClick={() => importInputRef.current?.click()} className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50 text-xs font-bold text-gray-600 shadow-sm">
            <Upload size={14} className="mr-1 text-gray-400" /> Nhập file
          </button>
          <input ref={importInputRef} type="file" accept=".csv,text/csv" className="hidden" onChange={event => void handleImportToppings(event)} />
          <button type="button" onClick={handleExportToppings} disabled={toppings.length === 0} className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50 text-xs font-bold text-gray-600 shadow-sm disabled:cursor-not-allowed disabled:opacity-50">
            <Download size={14} className="mr-1 text-gray-400" /> Xuất file
          </button>
          </div>
        </div>

        {/* Content List */}
        <div className="flex-1 overflow-auto p-4">
          {loading ? (
            <div className="flex flex-col items-center justify-center h-full opacity-40">
               <Loader2 size={48} className="animate-spin text-blue-600 mb-4" />
               <p className="font-black uppercase tracking-widest">Đang đồng bộ dữ liệu...</p>
            </div>
          ) : filteredToppings.length === 0 ? (
             <div className="h-full flex flex-col items-center justify-center bg-white rounded-xl border-2 border-dashed border-gray-200">
                <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center text-gray-200 mb-4"><Search size={40}/></div>
                <p className="font-black text-gray-400 uppercase tracking-widest">Không có kết quả phù hợp</p>
             </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {filteredToppings.map(t => (
                <div key={t.id} className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 group hover:shadow-md transition-all flex flex-col">
                  <div className="flex justify-between items-start mb-4">
                     <div className="flex flex-col space-y-1">
                        <span className="text-[8px] font-black text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full uppercase tracking-widest w-fit">{t.toppingType}</span>
                        <span className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{t.category}</span>
                     </div>
                     <div className="flex space-x-1 opacity-0 group-hover:opacity-100 transition-opacity">
                        <button onClick={() => openEditModal(t)} className="p-2 text-gray-400 hover:text-blue-600 bg-gray-50 rounded-xl"><Edit2 size={14}/></button>
                        <button onClick={() => deleteTopping(t.id!)} className="p-2 text-gray-400 hover:text-red-500 bg-gray-50 rounded-xl"><Trash2 size={14}/></button>
                     </div>
                  </div>

                  <div className="flex items-center space-x-4 mb-5">
                     <div className="w-16 h-16 bg-gray-50 rounded-2xl border border-gray-100 overflow-hidden flex items-center justify-center flex-shrink-0 shadow-inner">
                        {t.imageUrl ? <img src={t.imageUrl} className="w-full h-full object-cover" /> : <ImageIcon size={24} className="text-gray-200"/>}
                     </div>
                     <div className="min-w-0">
                         <h3 className="font-medium text-gray-800 text-sm truncate">{t.name}</h3>
                        <p className="text-[10px] text-gray-400 font-bold italic line-clamp-1">{t.description || 'Hương vị tuyệt hảo...'}</p>
                     </div>
                  </div>

                  <div className="mt-auto pt-4 border-t border-dashed border-gray-100 flex justify-between items-center">
                     <div>
                        <p className="text-[9px] font-black text-gray-400 uppercase mb-0.5">Giá bán</p>
                        <p className="text-xl font-black text-blue-700 tracking-tighter">{t.price.toLocaleString()}đ</p>
                     </div>
                     <div className="flex flex-col items-end gap-2">
                       {!t.isActive && <span className="text-[8px] font-black text-red-400 bg-red-50 px-2 py-1 rounded uppercase italic">Ngừng kinh doanh</span>}
                       <select value={t.availabilityStatus || 'Available'} onChange={event => void updateAvailability(t.id!, event.target.value as 'Available' | 'TemporarilyUnavailable' | 'OutOfStock')} className="rounded-lg border border-gray-200 bg-white px-2 py-1 text-[9px] font-bold text-gray-600">
                         <option value="Available">Chi nhánh: Đang bán</option>
                         <option value="TemporarilyUnavailable">Chi nhánh: Tạm hết</option>
                         <option value="OutOfStock">Chi nhánh: Hết món</option>
                       </select>
                      </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* FORM MODAL */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-start pt-10 pb-10 overflow-y-auto backdrop-blur-sm animate-in fade-in duration-200">
           <form onSubmit={handleSave} className="bg-white w-full max-w-3xl rounded-[2.5rem] shadow-2xl overflow-hidden animate-in slide-in-from-bottom-4 duration-300 mb-10">
              <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
                 <div className="relative z-10">
                    <h3 className="font-black text-xl uppercase tracking-tight italic flex items-center"><Settings2 size={24} className="mr-3"/> {editingTopping ? 'Cập nhật' : 'Thêm mới'} Topping</h3>
                    <p className="text-[10px] font-bold opacity-70 uppercase tracking-widest mt-1">Thiết lập thông tin topping và các tùy chọn đi kèm</p>
                 </div>
                 <button type="button" onClick={() => setIsModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <div className="p-10 space-y-10">
                 <div className="flex gap-10">
                     <div className="space-y-3 shrink-0 flex flex-col items-center">
                        <div onClick={() => fileInputRef.current?.click()} className="w-44 h-44 bg-gray-50 border-2 border-dashed border-gray-200 rounded-[2rem] flex items-center justify-center text-gray-300 cursor-pointer hover:bg-blue-50 hover:border-blue-400 overflow-hidden shadow-inner group">
                          {formData.imageUrl ? <img src={formData.imageUrl} className="w-full h-full object-cover" /> : <Camera size={40} className="opacity-20 group-hover:scale-110 transition-transform"/>}
                       </div>
                       <input type="file" ref={fileInputRef} className="hidden" accept="image/*" onChange={handleImageUpload} />
                        <label className="flex items-center cursor-pointer bg-gray-50 px-6 py-3 rounded-2xl border border-gray-100 shadow-sm">
                          <input type="checkbox" className="mr-3 h-5 w-5 text-blue-600 rounded-lg border-gray-300" checked={formData.isActive} onChange={e => setFormData({...formData, isActive: e.target.checked})} />
                           <span className="text-xs font-black text-gray-700 uppercase">Đang kinh doanh</span>
                       </label>
                    </div>

                     <div className="flex-1 space-y-6">
                       <div className="space-y-1">
                          <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Tên gọi Topping</label>
                           <input className="w-full px-5 py-3 bg-gray-50 rounded-2xl border-none outline-none text-lg font-black text-gray-800 focus:ring-2 focus:ring-blue-500/20" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} required/>
                       </div>

                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Loại Topping</label>
                              <select className="w-full px-5 py-3 bg-gray-50 rounded-2xl border-none font-bold text-gray-600 outline-none appearance-none" value={formData.toppingType} onChange={e => setFormData({...formData, toppingType: e.target.value})}>
                                <option>Topping Đồ uống</option>
                                <option>Topping Đồ ăn</option>
                                <option>Khác</option>
                             </select>
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Nhóm (Group)</label>
                              <select className="w-full px-5 py-3 bg-gray-50 rounded-2xl border-none font-bold text-gray-600 outline-none appearance-none" value={formData.category} onChange={e => setFormData({...formData, category: e.target.value})}>
                                {groups.map(g => <option key={g} value={g}>{g}</option>)}
                             </select>
                          </div>
                       </div>

                       <div className="space-y-1">
                          <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Mô tả topping</label>
                           <textarea className="w-full px-5 py-3 bg-gray-50 rounded-2xl border-none outline-none font-medium text-gray-600 text-xs focus:ring-2 focus:ring-blue-500/20" rows={2} value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})}/>
                       </div>
                    </div>
                 </div>

                  <div className="bg-blue-50/50 p-8 rounded-[2.5rem] border border-blue-100 flex gap-8">
                    <div className="flex-1 space-y-1">
                       <label className="text-[10px] font-black text-blue-600 uppercase tracking-widest ml-1">Giá bán thêm</label>
                       <div className="relative">
                           <input type="number" className="w-full px-5 py-4 bg-white border-2 border-blue-100 rounded-2xl outline-none font-black text-2xl text-blue-700 focus:border-blue-500" value={formData.price} onChange={e => setFormData({...formData, price: parseInt(e.target.value) || 0})} required/>
                          <span className="absolute right-4 top-5 text-[10px] font-black text-blue-200 uppercase">VNĐ</span>
                       </div>
                    </div>
                    <div className="flex-1 space-y-1">
                       <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Giá vốn (Ước tính)</label>
                        <input type="number" className="w-full px-5 py-4 bg-white border-2 border-gray-100 rounded-2xl outline-none font-bold text-xl text-red-400 focus:border-red-400" value={formData.costPrice} onChange={e => setFormData({...formData, costPrice: parseInt(e.target.value) || 0})}/>
                    </div>
                 </div>

                  <div className="flex gap-4 pt-4">
                     <button type="button" onClick={() => setIsModalOpen(false)} className="flex-1 py-3.5 bg-gray-100 rounded-full font-black text-gray-400 uppercase tracking-widest text-[10px]">Hủy bỏ</button>
                     <button type="submit" className="flex-[2] py-3.5 bg-blue-600 text-white rounded-full font-black uppercase tracking-widest text-[10px] shadow-xl shadow-blue-500/40 hover:bg-blue-700">Lưu thông tin Topping</button>
                 </div>
              </div>
           </form>
        </div>
      )}
    </div>
  );
};

export default ToppingManagement;

