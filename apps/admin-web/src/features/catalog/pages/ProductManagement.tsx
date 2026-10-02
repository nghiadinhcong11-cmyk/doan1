import React, { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Search, Filter, MoreVertical, Edit2, Trash2, Loader2, X, ChevronDown, Image as ImageIcon, Download, Upload, HelpCircle, Camera, Utensils, Settings2 } from 'lucide-react';
import { API_URL } from '../../../config';
import { notifyFeedback } from '../../../components/ui';

interface Product {
  id?: string;
  code: string;
  name: string;
  category: string;
  price: number;
  costPrice?: number;
  group?: string;
  type?: string;
  isActive?: boolean;
  imageUrl?: string;
  sizesJson?: string;
  toppingsJson?: string;
  description?: string;
}

interface ProductOption {
  name: string;
  price: number;
}

const ProductManagement = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [expandedRow, setExpandedRow] = useState<string | null>(null);
  const [groups, setGroups] = useState<string[]>(['Cà phê', 'Trà trái cây', 'Đồ ăn']);
  const [isManageGroupsMode, setIsManageGroupsMode] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterGroup, setFilterGroup] = useState('');
  const [filterCategory, setFilterCategory] = useState<string>(''); // Đồ uống, Đồ ăn...
  const [filterStatus, setFilterStatus] = useState<string>('active'); // active, inactive, all

  // Options State
  const [sizes, setSizes] = useState<ProductOption[]>([]);
  const [availableToppings, setAvailableToppings] = useState<any[]>([]);
  const [selectedToppingIds, setSelectedToppingIds] = useState<string[]>([]);

  // Form State
  const [newProduct, setNewProduct] = useState<Product>({
    code: '',
    name: '',
    category: 'Đồ uống',
    price: 0,
    costPrice: 0,
    group: 'Cà phê',
    isActive: true,
    imageUrl: '',
    description: ''
  });

  const fetchToppings = async () => {
    try {
      const res = await fetch(`${API_URL}/api/Topping`);
      const data = await res.json();
      setAvailableToppings(data);
    } catch (e) { console.error(e); }
  };

  const fetchProducts = async () => {
    try {
      setLoading(true);
      fetchToppings(); // Fetch toppings list for selection
      let url = `${API_URL}/api/Product`;
      const params = new URLSearchParams();
      if (searchTerm) params.append('search', searchTerm);
      if (filterGroup) params.append('group', filterGroup);
      if (filterCategory) params.append('category', filterCategory);
      if (filterStatus === 'active') params.append('isActive', 'true');
      if (filterStatus === 'inactive') params.append('isActive', 'false');

      if (params.toString()) {
        url += `?${params.toString()}`;
      }

      const response = await fetch(url);
      const data: Product[] = await response.json();
      setProducts(data);

      const uniqueGroups = Array.from(new Set(data.map(p => p.group).filter((g): g is string => !!g)));
      if (uniqueGroups.length > 0) {
        setGroups(prev => {
          const combined = [...prev, ...uniqueGroups];
          return Array.from(new Set(combined));
        });
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [filterGroup, searchTerm, filterCategory, filterStatus]);

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validation: Size < 500KB
      if (file.size > 500 * 1024) {
        notifyFeedback("Ảnh quá lớn. Vui lòng chọn ảnh dưới 500 KB.");
        return;
      }
      // Validation: MIME Type
      const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
      notifyFeedback("Định dạng ảnh không hỗ trợ. Vui lòng chọn JPG, PNG hoặc WebP.");
        return;
      }

      const reader = new FileReader();
      reader.onloadend = () => {
        setNewProduct({ ...newProduct, imageUrl: reader.result as string });
      };
      reader.readAsDataURL(file);
    }
  };

  const handleAddProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    try {
      const isEditing = !!editingProduct;
      const url = isEditing
        ? `${API_URL}/api/Product/${editingProduct.id}`
        : `${API_URL}/api/Product`;

      const payload = {
        ...(isEditing ? editingProduct : {}),
        ...newProduct,
        sizesJson: JSON.stringify(sizes),
        toppingsJson: JSON.stringify(availableToppings.filter(t => selectedToppingIds.includes(t.id)))
      };

      const response = await fetch(url, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        setIsModalOpen(false);
        setEditingProduct(null);
        resetForm();
        fetchProducts();
      } else if (response.status !== 403) {
        const errorData = await response.json();
        notifyFeedback(`Lỗi: ${errorData.message || response.statusText}`);
      }
    } catch (err) {
      console.error('Error saving product:', err);
      notifyFeedback('Lỗi kết nối đến server.');
    }
  };

  const resetForm = () => {
    setNewProduct({
      code: '',
      name: '',
      category: 'Đồ uống',
      price: 0,
      costPrice: 0,
      group: 'Cà phê',
      isActive: true,
      imageUrl: ''
    });
    setSizes([]);
    setSelectedToppingIds([]);
    setNewProduct({
      code: '',
      name: '',
      category: 'Đồ uống',
      price: 0,
      costPrice: 0,
      group: 'Cà phê',
      isActive: true,
      imageUrl: '',
      description: ''
    });
  };

  const openEditModal = (product: Product) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    setEditingProduct(product);
    setNewProduct({
      code: product.code,
      name: product.name,
      category: product.category,
      price: product.price,
      costPrice: product.costPrice || 0,
      group: product.group || '',
      isActive: product.isActive,
      imageUrl: product.imageUrl || '',
      description: product.description || ''
    });
    setSizes(product.sizesJson ? JSON.parse(product.sizesJson) : []);
    const currentToppings = product.toppingsJson ? JSON.parse(product.toppingsJson) : [];
    setSelectedToppingIds(currentToppings.map((t: any) => t.id));
    setIsModalOpen(true);
  };

  const handleAddGroup = () => {
    const groupName = prompt('Nhập tên nhóm món mới:');
    if (groupName && !groups.includes(groupName)) {
      setGroups([...groups, groupName]);
    }
  };

  const handleEditGroup = (oldName: string) => {
    const newName = prompt(`Sửa tên nhóm "${oldName}" thành:`, oldName);
    if (newName && newName !== oldName) {
      setGroups(groups.map(g => g === oldName ? newName : g));
      const productsToUpdate = products.filter(p => p.group === oldName);
      if (productsToUpdate.length > 0) {
        if (window.confirm(`Có ${productsToUpdate.length} sản phẩm thuộc nhóm này. Bạn có muốn cập nhật tên nhóm cho tất cả sản phẩm này không?`)) {
          productsToUpdate.forEach(async (p) => {
            await fetch(`${API_URL}/api/Product/${p.id}`, {
              method: 'PUT',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ ...p, group: newName })
            });
          });
          setTimeout(fetchProducts, 500);
        }
      }
    }
  };

  const handleDeleteGroup = (groupName: string) => {
    const hasProducts = products.some(p => p.group === groupName);
    if (hasProducts) {
      notifyFeedback('Không thể xóa nhóm này vì vẫn còn sản phẩm đang thuộc nhóm. Hãy đổi nhóm cho sản phẩm trước.');
      return;
    }
    if (window.confirm(`Bạn có chắc chắn muốn xóa nhóm "${groupName}"?`)) {
      setGroups(groups.filter(g => g !== groupName));
    }
  };

  const handleDeleteProduct = async (id: string) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    if (!window.confirm('Bạn có chắc chắn muốn xóa món này không?')) return;
    try {
      const response = await fetch(`${API_URL}/api/Product/${id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        fetchProducts();
      }
    } catch (err) {
      notifyFeedback('Lỗi khi xóa món');
    }
  };

  const handleToggleStatus = async (id: string) => {
    try {
      const response = await fetch(`${API_URL}/api/Product/${id}/toggle-status`, {
        method: 'PATCH'
      });
      if (response.ok) {
        fetchProducts();
      }
    } catch (err) {
      notifyFeedback('Lỗi khi cập nhật trạng thái');
    }
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      {/* SIDEBAR FILTER */}
      <div className="w-64 bg-white border-r overflow-y-auto p-4 space-y-6">
        <h2 className="font-bold text-base">Món</h2>

        {/* Loại thực đơn */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Loại thực đơn</p>
          <div className="space-y-1.5 ml-1">
            {['Tất cả', 'Đồ ăn', 'Đồ uống', 'Dịch vụ'].map(item => (
              <label key={item} className="flex items-center cursor-pointer group">
                <input
                  type="radio"
                  name="category"
                  className="mr-2 h-3.5 w-3.5 border-gray-300 text-blue-600 focus:ring-blue-500"
                  checked={(item === 'Tất cả' && filterCategory === '') || (filterCategory === item)}
                  onChange={() => setFilterCategory(item === 'Tất cả' ? '' : item)}
                />
                <span className={`transition-colors ${(item === 'Tất cả' && filterCategory === '') || (filterCategory === item) ? 'text-blue-600 font-bold' : 'text-gray-600 group-hover:text-blue-600'}`}>
                  {item}
                </span>
              </label>
            ))}
          </div>
        </div>

        {/* Nhóm món */}
        <div className="space-y-2">
          <div className="flex justify-between items-center">
            <p className="font-bold text-gray-700">Nhóm món</p>
            <div className="flex space-x-1">
              <button
                onClick={() => setIsManageGroupsMode(!isManageGroupsMode)}
                className={`p-1 rounded hover:bg-gray-100 transition-colors ${isManageGroupsMode ? 'text-blue-600 bg-blue-50' : 'text-gray-400'}`}
                title="Quản lý nhóm"
              >
                <Settings2 size={12}/>
              </button>
              <button onClick={handleAddGroup} className="text-blue-600 text-[11px] hover:underline font-bold">Tạo mới</button>
            </div>
          </div>

          <div className="space-y-1">
             <div
               onClick={() => setFilterGroup('')}
               className={`flex items-center px-3 py-2 rounded-xl cursor-pointer transition-all ${filterGroup === '' ? 'bg-blue-50 text-blue-600 font-bold' : 'text-gray-500 hover:bg-gray-50'}`}
             >
                <span className="text-xs">Tất cả nhóm món</span>
             </div>
             {groups.map(group => (
               <div key={group} className="flex items-center justify-between group/grp py-0.5 px-3 rounded-xl hover:bg-blue-50 transition-all">
                  <div
                    onClick={() => setFilterGroup(group)}
                    className={`flex-1 text-xs cursor-pointer ${filterGroup === group ? 'text-blue-600 font-bold' : 'text-gray-500 group-hover:text-blue-600'}`}
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

        {/* Trạng thái */}
        <div className="space-y-2 pt-4 border-t">
          <p className="font-bold text-gray-700">Trạng thái</p>
          <div className="space-y-1.5 ml-1">
            <label className="flex items-center cursor-pointer group">
              <input
                type="radio"
                name="status"
                className="mr-2 h-3.5 w-3.5 text-blue-600 focus:ring-blue-500"
                checked={filterStatus === 'active'}
                onChange={() => setFilterStatus('active')}
              />
              <span className={`transition-colors ${filterStatus === 'active' ? 'text-blue-600 font-bold' : 'text-gray-600 group-hover:text-blue-600'}`}>Đang kinh doanh</span>
            </label>
            <label className="flex items-center cursor-pointer group">
              <input
                type="radio"
                name="status"
                className="mr-2 h-3.5 w-3.5 text-blue-600 focus:ring-blue-500"
                checked={filterStatus === 'inactive'}
                onChange={() => setFilterStatus('inactive')}
              />
              <span className={`transition-colors ${filterStatus === 'inactive' ? 'text-blue-600 font-bold' : 'text-gray-600 group-hover:text-blue-600'}`}>Ngừng kinh doanh</span>
            </label>
            <label className="flex items-center cursor-pointer group">
              <input
                type="radio"
                name="status"
                className="mr-2 h-3.5 w-3.5 text-blue-600 focus:ring-blue-500"
                checked={filterStatus === 'all'}
                onChange={() => setFilterStatus('all')}
              />
              <span className={`transition-colors ${filterStatus === 'all' ? 'text-blue-600 font-bold' : 'text-gray-600 group-hover:text-blue-600'}`}>Tất cả</span>
            </label>
          </div>
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
              placeholder="Theo mã hoặc tên món"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex space-x-2">
            <button
              onClick={() => {
                if (localStorage.getItem('userRole') !== 'admin') {
                  notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
                  return;
                }
                resetForm(); setEditingProduct(null); setIsModalOpen(true);
              }}
              className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700 transition-colors shadow-lg shadow-blue-500/20"
            >
              <Plus size={16} className="mr-1" /> Thêm mới
            </button>
            <button className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50 text-xs font-bold text-gray-600 shadow-sm">
              <Upload size={14} className="mr-1 text-gray-400" /> Nhập file
            </button>
            <button className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50 text-xs font-bold text-gray-600 shadow-sm">
              <Download size={14} className="mr-1 text-gray-400" /> Xuất file
            </button>
          </div>
        </div>

        {/* Table List */}
        <div className="flex-1 overflow-auto bg-white">
          <table className="w-full text-left border-collapse">
            <thead className="bg-[#f9fafb] border-b text-gray-500 font-bold sticky top-0 z-10">
              <tr>
                <th className="px-4 py-2 w-10"><input type="checkbox" /></th>
                <th className="px-4 py-2 w-16 uppercase text-[10px]">Ảnh</th>
                <th className="px-4 py-2 w-32 uppercase text-[10px]">Mã món</th>
                <th className="px-4 py-2 uppercase text-[10px]">Tên món</th>
                <th className="px-4 py-2 w-40 uppercase text-[10px]">Nhóm món</th>
                <th className="px-4 py-2 w-32 uppercase text-[10px] text-right">Giá bán</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr><td colSpan={6} className="text-center py-20"><Loader2 className="animate-spin inline mr-2 text-blue-600"/> Đang tải...</td></tr>
              ) : products.length === 0 ? (
                <tr>
                  <td colSpan={6} className="text-center py-32 bg-gray-50/50">
                    <div className="flex flex-col items-center justify-center space-y-4">
                      <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center text-gray-300">
                        <Utensils size={40} />
                      </div>
                      <div>
                        <p className="text-lg font-black text-gray-400 uppercase tracking-tighter italic">Chưa có thực đơn nào</p>
                        <p className="text-xs text-gray-400 font-medium">Bắt đầu bằng cách nhấn nút "Thêm mới" để tạo món ăn đầu tiên của bạn.</p>
                      </div>
                      <button
                        onClick={() => { resetForm(); setEditingProduct(null); setIsModalOpen(true); }}
                        className="mt-4 bg-blue-600 text-white px-6 py-2 rounded-xl font-bold text-xs shadow-lg shadow-blue-500/30 hover:bg-blue-700 transition-all flex items-center"
                      >
                        <Plus size={16} className="mr-1" /> TẠO MÓN NGAY
                      </button>
                    </div>
                  </td>
                </tr>
              ) : products.map(p => (
                <React.Fragment key={p.id}>
                  <tr
                    className={`hover:bg-blue-50 cursor-pointer transition-colors ${expandedRow === p.id ? 'bg-blue-50' : ''}`}
                    onClick={() => setExpandedRow(expandedRow === p.id ? null : p.id!)}
                  >
                    <td className="px-4 py-3"><input type="checkbox" onClick={e => e.stopPropagation()}/></td>
                    <td className="px-4 py-3">
                       <div className="w-10 h-10 bg-gray-100 rounded border flex items-center justify-center overflow-hidden">
                          {p.imageUrl && p.imageUrl !== 'string' ? (
                            <img src={p.imageUrl} alt="" className="w-full h-full object-cover" />
                          ) : (
                            <ImageIcon size={16} className="text-gray-300" />
                          )}
                       </div>
                    </td>
                    <td className="px-4 py-3 text-blue-600 font-bold">{p.code}</td>
                    <td className="px-4 py-3 font-medium">{p.name}</td>
                    <td className="px-4 py-3 text-gray-500">{p.group}</td>
                    <td className="px-4 py-3 text-right font-black text-gray-800">{p.price.toLocaleString()}</td>
                  </tr>
                  {/* Expanded Detail View */}
                  {expandedRow === p.id && (
                    <tr className="bg-white">
                      <td colSpan={6} className="p-0">
                        <div className="border-l-4 border-blue-500 ml-4 my-2 shadow-inner bg-gray-50 p-6">
                          <div className="flex space-x-10">
                             <div className="w-40 h-40 bg-gray-50 rounded-xl flex items-center justify-center border border-dashed border-gray-300 overflow-hidden shadow-sm">
                                {p.imageUrl && p.imageUrl !== 'string' ? (
                                  <img src={p.imageUrl} alt={p.name} className="w-full h-full object-cover" />
                                ) : (
                                  <ImageIcon size={48} className="text-gray-200" />
                                )}
                             </div>
                             <div className="flex-1">
                                <h3 className="text-lg font-black mb-4 uppercase text-gray-800 tracking-tight">{p.name}</h3>
                                <div className="grid grid-cols-3 gap-y-6 text-[12px]">
                                   <div><p className="text-gray-400 font-bold uppercase text-[9px] mb-1">Mã món</p><p className="font-bold text-blue-600">{p.code}</p></div>
                                   <div><p className="text-gray-400 font-bold uppercase text-[9px] mb-1">Nhóm món</p><p className="font-bold">{p.group}</p></div>
                                   <div><p className="text-gray-400 font-bold uppercase text-[9px] mb-1">Giá bán</p><p className="font-black text-blue-700 text-base">{p.price.toLocaleString()} đ</p></div>
                                   <div><p className="text-gray-400 font-bold uppercase text-[9px] mb-1">Loại món</p><p className="font-bold">{p.category}</p></div>
                                   <div><p className="text-gray-400 font-bold uppercase text-[9px] mb-1">Trạng thái</p><p className={p.isActive ? 'text-green-600 font-bold' : 'text-red-500 font-bold'}>{p.isActive ? 'Đang bán' : 'Ngừng bán'}</p></div>
                                </div>
                                <div className="mt-8 flex justify-end space-x-2">
                                   <button onClick={(e) => { e.stopPropagation(); openEditModal(p); }} className="bg-blue-600 text-white px-5 py-2 rounded-full font-bold flex items-center hover:bg-blue-700 shadow-lg shadow-blue-500/20"><Edit2 size={14} className="mr-2"/> Cập nhật</button>
                                   <button onClick={(e) => { e.stopPropagation(); handleToggleStatus(p.id!); }} className="border border-gray-300 px-5 py-2 rounded-full font-bold flex items-center hover:bg-gray-50">{p.isActive ? 'Ngừng bán' : 'Cho phép bán'}</button>
                                   <button onClick={(e) => { e.stopPropagation(); handleDeleteProduct(p.id!); }} className="border border-red-200 text-red-600 px-5 py-2 rounded-full font-bold flex items-center hover:bg-red-50"><Trash2 size={14} className="mr-2"/> Xóa món</button>
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

      {/* FORM MODAL */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-start pt-10 pb-10 overflow-y-auto backdrop-blur-sm animate-in fade-in duration-200">
           <div className="bg-white w-full max-w-3xl rounded-[2.5rem] shadow-2xl overflow-hidden animate-in slide-in-from-bottom-4 duration-300 mb-10">
              <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
                 <div>
                    <h3 className="font-black text-xl uppercase tracking-tight italic flex items-center"><ImageIcon size={24} className="mr-3"/> {editingProduct ? 'Cập nhật món ăn' : 'Thêm món mới'}</h3>
                    <p className="text-[10px] font-bold opacity-70 uppercase tracking-widest mt-1">Thiết lập thông tin thực đơn và các tùy chọn đi kèm</p>
                 </div>
                 <button onClick={() => setIsModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <form onSubmit={handleAddProduct} className="p-10 space-y-10">
                 {/* PHẦN 1: THÔNG TIN CƠ BẢN */}
                 <div className="flex gap-10">
                    {/* Image Area */}
                    <div className="space-y-3 shrink-0">
                       <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest ml-1">Ảnh đại diện</label>
                       <div
                         onClick={() => fileInputRef.current?.click()}
                         className="w-44 h-44 bg-gray-50 border-2 border-dashed border-gray-200 rounded-[2rem] flex flex-col items-center justify-center text-gray-300 cursor-pointer hover:bg-blue-50 hover:border-blue-400 hover:text-blue-500 transition-all overflow-hidden relative group shadow-inner"
                       >
                          {newProduct.imageUrl ? (
                             <>
                                <img src={newProduct.imageUrl} alt="" className="w-full h-full object-cover" />
                                <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                   <Camera size={32} className="text-white" />
                                </div>
                             </>
                          ) : (
                             <>
                                <ImageIcon size={48} className="opacity-20" />
                                <span className="text-[9px] mt-2 font-black uppercase tracking-widest">Tải ảnh lên</span>
                             </>
                          )}
                       </div>
                       <input type="file" ref={fileInputRef} className="hidden" accept="image/*" onChange={handleImageUpload} />
                       {newProduct.imageUrl && (
                         <button type="button" onClick={() => setNewProduct({...newProduct, imageUrl: ''})} className="text-red-500 text-[10px] font-black uppercase hover:underline w-full text-center">Xóa ảnh hiện tại</button>
                       )}
                    </div>

                    {/* Basic Inputs */}
                    <div className="flex-1 space-y-6">
                       <div className="group">
                          <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest mb-1 ml-1 group-focus-within:text-blue-600 transition-colors">Tên món ăn <span className="text-red-500">*</span></label>
                          <input type="text" className="w-full px-5 py-3 bg-gray-50 border-none rounded-2xl outline-none text-lg font-black text-gray-800 focus:ring-2 focus:ring-blue-500/20" placeholder="VD: Cà phê muối, Bánh ngọt..." value={newProduct.name} onChange={e => setNewProduct({...newProduct, name: e.target.value})} required/>
                       </div>

                       <div className="grid grid-cols-2 gap-4">
                          <div>
                             <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest mb-1 ml-1">Mã món (ID)</label>
                             <input type="text" className="w-full px-5 py-3 bg-gray-50 border-none rounded-2xl outline-none font-bold text-blue-600 focus:ring-2 focus:ring-blue-500/20" placeholder="Mã tự động" value={newProduct.code} onChange={e => setNewProduct({...newProduct, code: e.target.value})}/>
                          </div>
                          <div>
                             <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest mb-1 ml-1">Loại món</label>
                             <select className="w-full px-5 py-3 bg-gray-50 border-none rounded-2xl outline-none font-bold text-gray-700 appearance-none focus:ring-2 focus:ring-blue-500/20" value={newProduct.category} onChange={e => setNewProduct({...newProduct, category: e.target.value})}>
                                <option>Đồ uống</option>
                                <option>Đồ ăn</option>
                                <option>Dịch vụ</option>
                             </select>
                          </div>
                       </div>

                       <div>
                          <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest mb-1 ml-1">Nhóm thực đơn</label>
                          <select className="w-full px-5 py-3 bg-gray-50 border-none rounded-2xl outline-none font-bold text-gray-700 appearance-none focus:ring-2 focus:ring-blue-500/20" value={newProduct.group} onChange={e => setNewProduct({...newProduct, group: e.target.value})}>
                             {groups.map(g => <option key={g} value={g}>{g}</option>)}
                          </select>
                       </div>
                       <div>
                          <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest mb-1 ml-1">Mô tả món ăn</label>
                          <textarea
                             className="w-full px-5 py-3 bg-gray-50 border-none rounded-2xl outline-none font-medium text-gray-600 focus:ring-2 focus:ring-blue-500/20"
                             placeholder="Nhập mô tả ngắn về hương vị, nguyên liệu..."
                             rows={2}
                             value={newProduct.description}
                             onChange={e => setNewProduct({...newProduct, description: e.target.value})}
                          />
                       </div>
                    </div>
                 </div>

                 {/* PHẦN 2: THIẾT LẬP GIÁ */}
                 <div className="bg-blue-50/50 p-8 rounded-[2.5rem] border border-blue-100">
                    <div className="grid grid-cols-3 gap-8">
                       <div className="space-y-1">
                          <label className="block text-[10px] text-blue-600 font-black uppercase tracking-widest ml-1">Giá bán mặc định</label>
                          <div className="relative">
                             <input type="number" className="w-full px-5 py-4 bg-white border-2 border-blue-100 rounded-2xl outline-none font-black text-blue-700 text-2xl focus:border-blue-500 transition-all" value={newProduct.price} onChange={e => setNewProduct({...newProduct, price: parseInt(e.target.value) || 0})} required/>
                             <span className="absolute right-4 top-5 text-[10px] font-black text-blue-300 uppercase">VNĐ</span>
                          </div>
                       </div>
                       <div className="space-y-1">
                          <label className="block text-[10px] text-gray-400 font-black uppercase tracking-widest ml-1">Giá vốn (Ước tính)</label>
                          <div className="relative">
                             <input type="number" className="w-full px-5 py-4 bg-white border-2 border-gray-100 rounded-2xl outline-none font-bold text-red-400 text-xl focus:border-red-400 transition-all" value={newProduct.costPrice} onChange={e => setNewProduct({...newProduct, costPrice: parseInt(e.target.value) || 0})}/>
                          </div>
                       </div>
                       <div className="flex flex-col justify-center items-center">
                          <label className="text-[10px] text-gray-400 font-black uppercase tracking-widest mb-3">Trạng thái bán</label>
                          <label className="flex items-center cursor-pointer bg-white px-6 py-3 rounded-2xl border shadow-sm hover:shadow-md transition-all">
                             <input type="checkbox" className="mr-3 h-5 w-5 text-blue-600 rounded-lg border-gray-300" checked={newProduct.isActive} onChange={e => setNewProduct({...newProduct, isActive: e.target.checked})}/>
                             <span className="text-xs font-black text-gray-700 uppercase tracking-tighter italic">Đang kinh doanh</span>
                          </label>
                       </div>
                    </div>
                 </div>

                 {/* PHẦN 3: TÙY CHỌN SIZE & TOPPING */}
                 <div className="space-y-6">
                    <div className="flex items-center space-x-2 border-b pb-2">
                       <Settings2 size={16} className="text-gray-400" />
                       <h4 className="text-xs font-black text-gray-700 uppercase tracking-widest italic">Tùy chọn bán hàng</h4>
                    </div>

                    <div className="grid grid-cols-2 gap-10">
                       {/* Kích cỡ */}
                       <div className="bg-gray-50 p-6 rounded-[2rem] border border-gray-100 space-y-4">
                          <div className="flex justify-between items-center">
                             <label className="text-[10px] font-black text-blue-600 uppercase tracking-widest ml-2">Phân loại Size</label>
                             <button type="button" onClick={() => setSizes([...sizes, {name: '', price: 0}])} className="bg-blue-600 text-white px-3 py-1 rounded-lg text-[9px] font-black uppercase tracking-widest hover:bg-blue-700 transition-all shadow-md shadow-blue-500/20">+ Thêm Size</button>
                          </div>
                          <div className="space-y-3">
                             {sizes.map((s, i) => (
                                <div key={i} className="grid grid-cols-12 gap-2 items-center animate-in slide-in-from-left-2">
                                   <div className="col-span-7">
                                      <input
                                         type="text"
                                         placeholder="Tên (M, L...)"
                                         className="w-full px-4 py-3 bg-white border border-gray-200 rounded-xl text-xs font-black uppercase outline-none focus:border-blue-400 shadow-sm"
                                         value={s.name}
                                         onChange={e => {
                                            const newSizes = [...sizes];
                                            newSizes[i].name = e.target.value;
                                            setSizes(newSizes);
                                         }}
                                      />
                                   </div>
                                   <div className="col-span-4 relative">
                                      <input
                                         type="number"
                                         placeholder="0"
                                         className="w-full pl-8 pr-3 py-3 bg-white border border-gray-200 rounded-xl text-xs font-black text-blue-600 outline-none focus:border-blue-400 shadow-sm [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                                         value={s.price}
                                         onChange={e => {
                                            const newSizes = [...sizes];
                                            newSizes[i].price = parseInt(e.target.value) || 0;
                                            setSizes(newSizes);
                                         }}
                                      />
                                      <div className="absolute left-3 top-3.5 flex items-center pointer-events-none">
                                         <Plus size={14} className="text-gray-300" />
                                      </div>
                                   </div>
                                   <div className="col-span-1 flex justify-center">
                                      <button type="button" onClick={() => setSizes(sizes.filter((_, idx) => idx !== i))} className="p-2 text-red-300 hover:text-red-500 hover:bg-red-50 rounded-lg transition-all"><X size={16}/></button>
                                   </div>
                                </div>
                             ))}
                             {sizes.length === 0 && <p className="text-[9px] text-gray-400 font-bold italic text-center py-4 bg-white/50 rounded-2xl border border-dashed">Chưa thiết lập các kích cỡ khác</p>}
                          </div>
                       </div>

                       {/* Topping (Dạng lựa chọn từ danh sách toàn cục) */}
                       <div className="bg-gray-50 p-6 rounded-[2rem] border border-gray-100 space-y-4">
                          <div className="flex justify-between items-center">
                             <label className="text-[10px] font-black text-orange-600 uppercase tracking-widest ml-2">Chọn Topping khả dụng</label>
                             <Link to="/toppings" className="text-blue-600 text-[9px] font-black uppercase hover:underline">Quản lý Topping</Link>
                          </div>

                          <div className="bg-white rounded-2xl p-4 max-h-[180px] overflow-y-auto border border-gray-100 space-y-2">
                             {availableToppings.length === 0 ? (
                                <p className="text-[10px] text-gray-400 italic text-center py-4">Chưa có topping nào. Hãy tạo trong mục Quản lý Topping.</p>
                             ) : availableToppings.map(t => (
                                <label key={t.id} className={`flex items-center justify-between p-3 rounded-xl border transition-all cursor-pointer ${selectedToppingIds.includes(t.id) ? 'bg-orange-50 border-orange-200' : 'hover:bg-gray-50 border-gray-50'}`}>
                                   <div className="flex items-center">
                                      <input
                                         type="checkbox"
                                         className="mr-3 h-4 w-4 rounded text-orange-600 border-gray-300"
                                         checked={selectedToppingIds.includes(t.id)}
                                         onChange={() => {
                                            if (selectedToppingIds.includes(t.id)) {
                                               setSelectedToppingIds(selectedToppingIds.filter(id => id !== t.id));
                                            } else {
                                               setSelectedToppingIds([...selectedToppingIds, t.id]);
                                            }
                                         }}
                                      />
                                      <span className={`text-[11px] font-bold ${selectedToppingIds.includes(t.id) ? 'text-orange-700' : 'text-gray-600'}`}>{t.name}</span>
                                   </div>
                                   <span className="text-[10px] font-black text-gray-400">+{t.price.toLocaleString()}đ</span>
                                </label>
                             ))}
                          </div>
                       </div>
                    </div>
                    <p className="text-[10px] text-gray-400 italic text-center">* Topping được chọn sẽ hiện ra cho khách hàng tùy chọn khi đặt món này.</p>
                 </div>

                 <div className="flex justify-end space-x-4 pt-6 border-t">
                    <button type="button" onClick={() => setIsModalOpen(false)} className="px-10 py-3.5 bg-gray-100 rounded-full font-black text-gray-400 text-[10px] uppercase tracking-[0.2em] hover:bg-gray-200 transition-all">Bỏ qua</button>
                    <button type="submit" className="px-16 py-3.5 bg-[#0070f4] text-white rounded-full font-black text-[10px] uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/40 hover:bg-blue-700 active:scale-95 transition-all">
                       {editingProduct ? 'LƯU THAY ĐỔI' : 'TẠO MÓN NGAY'}
                    </button>
                 </div>
              </form>
           </div>
        </div>
      )}
    </div>
  );
};

export default ProductManagement;

