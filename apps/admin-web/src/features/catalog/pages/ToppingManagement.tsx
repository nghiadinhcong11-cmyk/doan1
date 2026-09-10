import React, { useState, useEffect, useRef } from 'react';
import { Plus, Trash2, Edit2, Loader2, X, Settings2, Search, Image as ImageIcon, Camera, CheckCircle2, MoreHorizontal, Filter, Utensils } from 'lucide-react';
import { API_URL } from '../../../config';

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
}

const ToppingManagement = () => {
  const [toppings, setToppings] = useState<Topping[]>([]);
  const [loading, setLoading] = useState(true);
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

  useEffect(() => { fetchToppings(); }, []);

  const fetchToppings = async () => {
    try {
      setLoading(true);
      const res = await fetch(`${API_URL}/api/Topping`);
      const data = await res.json();
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
        fetchToppings();
      }
    } catch (err) { alert('Lỗi kết nối'); }
  };

  const deleteTopping = async (id: string) => {
    if (!window.confirm('Xóa Topping này?')) return;
    await fetch(`${API_URL}/api/Topping/${id}`, { method: 'DELETE' });
    fetchToppings();
  };

  const openEditModal = (topping: Topping) => {
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
          fetchToppings();
        }
      }
    }
  };

  const handleDeleteGroup = (groupName: string) => {
    const hasToppings = toppings.some(t => t.category === groupName);
    if (hasToppings) {
      alert('Không thể xóa nhóm này vì vẫn còn Topping đang thuộc nhóm. Hãy đổi nhóm cho Topping trước.');
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
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px] font-sans">
      {/* SIDEBAR FILTER */}
      <div className="w-64 bg-white border-r overflow-y-auto p-5 space-y-8 shadow-sm">
        <div className="flex items-center space-x-2 text-blue-600 mb-2">
           <Settings2 size={20} />
           <h2 className="font-black text-base uppercase tracking-tighter italic">Lọc Topping</h2>
        </div>

        {/* Phân loại lớn */}
        <div className="space-y-3">
          <p className="font-black text-gray-400 uppercase text-[10px] tracking-widest">Loại Topping</p>
          <div className="space-y-1">
            {['Tất cả', 'Topping Đồ uống', 'Topping Đồ ăn', 'Chưa phân loại'].map(item => (
              <label key={item} className="flex items-center cursor-pointer group py-1">
                <input
                  type="radio"
                  name="type"
                  className="mr-3 h-4 w-4 border-gray-300 text-blue-600 focus:ring-0"
                  checked={(item === 'Tất cả' && filterType === '') || (filterType === item)}
                  onChange={() => setFilterType(item === 'Tất cả' ? '' : item)}
                />
                <span className={`transition-all ${(item === 'Tất cả' && filterType === '') || (filterType === item) ? 'text-blue-700 font-black' : 'text-gray-500 group-hover:text-blue-600 font-bold'}`}>
                  {item}
                </span>
              </label>
            ))}
          </div>
        </div>

        {/* Nhóm Topping */}
        <div className="space-y-4 pt-6 border-t border-gray-100">
          <div className="flex justify-between items-center">
            <p className="font-black text-gray-400 uppercase text-[10px] tracking-widest">Nhóm Topping</p>
            <div className="flex space-x-1">
              <button
                onClick={() => setIsManageGroupsMode(!isManageGroupsMode)}
                className={`p-1 rounded hover:bg-gray-100 transition-colors ${isManageGroupsMode ? 'text-blue-600 bg-blue-50' : 'text-gray-400'}`}
              >
                <Settings2 size={12}/>
              </button>
              <button onClick={() => {const n = prompt('Tên nhóm:'); if(n) setGroups([...groups, n])}} className="text-blue-600 text-[10px] font-black uppercase hover:underline">Tạo mới</button>
            </div>
          </div>

          <div className="space-y-1">
             <div
               onClick={() => setFilterGroup('')}
               className={`px-3 py-2.5 rounded-xl cursor-pointer transition-all ${filterCategory === '' ? 'bg-blue-50 text-blue-700 font-black shadow-sm' : 'text-gray-500 hover:bg-gray-50 font-bold'}`}
             >
                Tất cả nhóm
             </div>
             {groups.map(group => (
               <div
                 key={group}
                 className={`flex items-center justify-between group/grp px-3 py-1.5 rounded-xl hover:bg-blue-50 transition-all ${filterCategory === group ? 'bg-blue-50' : ''}`}
               >
                 <div
                   onClick={() => setFilterGroup(group)}
                   className={`flex-1 cursor-pointer transition-all ${filterCategory === group ? 'text-blue-700 font-black' : 'text-gray-500 hover:text-blue-600 font-bold'}`}
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
        <div className="bg-white p-4 flex justify-between items-center border-b shadow-sm relative z-10">
          <div className="relative w-96">
            <Search className="absolute left-4 top-2.5 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-11 pr-4 py-2.5 bg-gray-50 border-none rounded-xl outline-none font-bold text-gray-700 focus:ring-2 focus:ring-blue-500/20 transition-all"
              placeholder="Tìm theo tên Topping hoặc mô tả..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button
            onClick={() => { resetForm(); setIsModalOpen(true); }}
            className="bg-[#0070f4] text-white px-6 py-2.5 rounded-xl flex items-center font-black uppercase tracking-widest text-[11px] shadow-lg shadow-blue-500/20 hover:bg-blue-700 transition-all active:scale-95"
          >
            <Plus size={18} className="mr-2" /> Thêm Topping mới
          </button>
        </div>

        {/* Content List */}
        <div className="flex-1 overflow-auto p-6">
          {loading ? (
            <div className="flex flex-col items-center justify-center h-full opacity-40">
               <Loader2 size={48} className="animate-spin text-blue-600 mb-4" />
               <p className="font-black uppercase tracking-widest">Đang đồng bộ dữ liệu...</p>
            </div>
          ) : filteredToppings.length === 0 ? (
             <div className="h-full flex flex-col items-center justify-center bg-white rounded-[3rem] border-2 border-dashed border-gray-200">
                <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center text-gray-200 mb-4"><Search size={40}/></div>
                <p className="font-black text-gray-400 uppercase tracking-widest">Không có kết quả phù hợp</p>
             </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {filteredToppings.map(t => (
                <div key={t.id} className="bg-white p-6 rounded-[2.5rem] shadow-sm border border-gray-100 group hover:shadow-2xl hover:shadow-blue-500/5 transition-all flex flex-col">
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
                        <h3 className="font-black text-gray-800 uppercase tracking-tight text-sm truncate">{t.name}</h3>
                        <p className="text-[10px] text-gray-400 font-bold italic line-clamp-1">{t.description || 'Hương vị tuyệt hảo...'}</p>
                     </div>
                  </div>

                  <div className="mt-auto pt-4 border-t border-dashed border-gray-100 flex justify-between items-center">
                     <div>
                        <p className="text-[9px] font-black text-gray-400 uppercase mb-0.5">Giá bán</p>
                        <p className="text-xl font-black text-blue-700 tracking-tighter">{t.price.toLocaleString()}đ</p>
                     </div>
                     {!t.isActive && <span className="text-[8px] font-black text-red-400 bg-red-50 px-2 py-1 rounded uppercase italic">Tạm ngưng</span>}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* FORM MODAL */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-[200] backdrop-blur-sm p-4">
           <form onSubmit={handleSave} className="bg-white rounded-[3rem] w-full max-w-2xl shadow-2xl animate-in zoom-in-95 overflow-hidden">
              <div className="bg-[#0070f4] p-8 text-white flex justify-between items-center relative">
                 <div className="relative z-10">
                    <h3 className="text-2xl font-black uppercase italic tracking-tighter flex items-center"><Settings2 className="mr-3"/> {editingTopping ? 'Cập nhật' : 'Thêm mới'} Topping</h3>
                    <p className="text-[10px] font-bold opacity-70 uppercase tracking-widest mt-1">Đồng bộ lựa chọn cho toàn hệ thống thực đơn</p>
                 </div>
                 <button type="button" onClick={() => setIsModalOpen(false)} className="bg-white/10 p-3 rounded-full hover:rotate-90 transition-all z-10"><X size={24}/></button>
              </div>

              <div className="p-10 space-y-8">
                 <div className="flex gap-10">
                    <div className="space-y-4 shrink-0 flex flex-col items-center">
                       <div onClick={() => fileInputRef.current?.click()} className="w-40 h-40 bg-gray-50 border-2 border-dashed border-gray-200 rounded-[2.5rem] flex items-center justify-center text-gray-300 cursor-pointer hover:border-blue-400 overflow-hidden shadow-inner group">
                          {formData.imageUrl ? <img src={formData.imageUrl} className="w-full h-full object-cover" /> : <Camera size={40} className="opacity-20 group-hover:scale-110 transition-transform"/>}
                       </div>
                       <input type="file" ref={fileInputRef} className="hidden" accept="image/*" onChange={handleImageUpload} />
                       <label className="flex items-center cursor-pointer bg-gray-50 px-6 py-2.5 rounded-2xl border border-gray-100 shadow-sm">
                          <input type="checkbox" className="mr-3 h-5 w-5 text-blue-600 rounded-lg border-gray-300" checked={formData.isActive} onChange={e => setFormData({...formData, isActive: e.target.checked})} />
                          <span className="text-[10px] font-black text-gray-700 uppercase">Đang bán</span>
                       </label>
                    </div>

                    <div className="flex-1 space-y-6">
                       <div className="space-y-1">
                          <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Tên gọi Topping</label>
                          <input className="w-full px-6 py-4 bg-gray-50 rounded-2xl border-none outline-none font-black text-lg text-gray-800 focus:ring-2 focus:ring-blue-500/20" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} required/>
                       </div>

                       <div className="grid grid-cols-2 gap-4">
                          <div className="space-y-1">
                             <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Loại Topping</label>
                             <select className="w-full px-5 py-3.5 bg-gray-50 rounded-2xl border-none font-bold text-gray-600 outline-none appearance-none" value={formData.toppingType} onChange={e => setFormData({...formData, toppingType: e.target.value})}>
                                <option>Topping Đồ uống</option>
                                <option>Topping Đồ ăn</option>
                                <option>Khác</option>
                             </select>
                          </div>
                          <div className="space-y-1">
                             <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Nhóm (Group)</label>
                             <select className="w-full px-5 py-3.5 bg-gray-50 rounded-2xl border-none font-bold text-gray-600 outline-none appearance-none" value={formData.category} onChange={e => setFormData({...formData, category: e.target.value})}>
                                {groups.map(g => <option key={g} value={g}>{g}</option>)}
                             </select>
                          </div>
                       </div>

                       <div className="space-y-1">
                          <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Mô tả topping</label>
                          <textarea className="w-full px-6 py-4 bg-gray-50 rounded-2xl border-none outline-none font-bold text-gray-500 text-xs focus:ring-2 focus:ring-blue-500/20" rows={2} value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})}/>
                       </div>
                    </div>
                 </div>

                 <div className="bg-blue-50/50 p-8 rounded-[2.5rem] border border-blue-100 flex gap-8">
                    <div className="flex-1 space-y-1">
                       <label className="text-[10px] font-black text-blue-600 uppercase tracking-widest ml-1">Giá bán thêm</label>
                       <div className="relative">
                          <input type="number" className="w-full px-6 py-4 bg-white border-2 border-blue-100 rounded-2xl outline-none font-black text-2xl text-blue-700" value={formData.price} onChange={e => setFormData({...formData, price: parseInt(e.target.value) || 0})} required/>
                          <span className="absolute right-4 top-5 text-[10px] font-black text-blue-200 uppercase">VNĐ</span>
                       </div>
                    </div>
                    <div className="flex-1 space-y-1">
                       <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Giá vốn (Ước tính)</label>
                       <input type="number" className="w-full px-6 py-4 bg-white border-2 border-gray-100 rounded-2xl outline-none font-bold text-xl text-red-400" value={formData.costPrice} onChange={e => setFormData({...formData, costPrice: parseInt(e.target.value) || 0})}/>
                    </div>
                 </div>

                 <div className="flex gap-4 pt-4">
                    <button type="button" onClick={() => setIsModalOpen(false)} className="flex-1 py-4 bg-gray-100 rounded-full font-black text-gray-400 uppercase tracking-widest text-[10px]">Hủy bỏ</button>
                    <button type="submit" className="flex-[2] py-4 bg-blue-600 text-white rounded-full font-black uppercase tracking-widest text-[10px] shadow-xl shadow-blue-500/40 hover:bg-blue-700">Lưu thông tin Topping</button>
                 </div>
              </div>
           </form>
        </div>
      )}
    </div>
  );
};

export default ToppingManagement;

