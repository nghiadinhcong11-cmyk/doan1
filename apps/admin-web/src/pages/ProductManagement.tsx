import React, { useState, useEffect } from 'react';
import { Plus, Search, Filter, MoreVertical, Edit2, Trash2, Loader2, X, ChevronDown, Image as ImageIcon, Download, Upload, HelpCircle } from 'lucide-react';

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
}

const ProductManagement = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<Product | null>(null);
  const [expandedRow, setExpandedRow] = useState<string | null>(null);
  const [groups, setGroups] = useState<string[]>(['Cà phê', 'Trà trái cây', 'Đồ ăn']);

  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterGroup, setFilterGroup] = useState('');
  const [filterCategory, setFilterCategory] = useState<string[]>(['Đồ ăn', 'Đồ uống']);

  // Form State
  const [newProduct, setNewProduct] = useState<Product>({
    code: '',
    name: '',
    category: 'Đồ ăn',
    price: 0,
    costPrice: 0,
    group: 'Cà phê',
    isActive: true
  });

  const fetchProducts = async () => {
    try {
      setLoading(true);
      let url = 'http://localhost:5000/api/Product';
      const params = new URLSearchParams();
      if (searchTerm) params.append('search', searchTerm);
      if (filterGroup) params.append('group', filterGroup);

      if (params.toString()) {
        url += `?${params.toString()}`;
      }

      const response = await fetch(url);
      const data: Product[] = await response.json();
      setProducts(data);

      // Tự động cập nhật danh sách nhóm món từ dữ liệu sản phẩm
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
  }, [filterGroup, searchTerm]);

  const handleAddProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const isEditing = !!editingProduct;
      const url = isEditing
        ? `http://localhost:5000/api/Product/${editingProduct.id}`
        : 'http://localhost:5000/api/Product';

      const response = await fetch(url, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(isEditing ? {...editingProduct, ...newProduct} : newProduct)
      });

      if (response.ok) {
        setIsModalOpen(false);
        setEditingProduct(null);
        setNewProduct({ code: '', name: '', category: 'Đồ ăn', price: 0, costPrice: 0, group: 'Cà phê', isActive: true });
        fetchProducts();
      } else {
        const errorData = await response.json();
        alert(`Lỗi: ${errorData.message || response.statusText}`);
      }
    } catch (err) {
      console.error('Error saving product:', err);
      alert('Lỗi kết nối đến server.');
    }
  };

  const openEditModal = (product: Product) => {
    setEditingProduct(product);
    setNewProduct({
      code: product.code,
      name: product.name,
      category: product.category,
      price: product.price,
      costPrice: product.costPrice || 0,
      group: product.group || '',
      isActive: product.isActive
    });
    setIsModalOpen(true);
  };

  const handleAddGroup = () => {
    const groupName = prompt('Nhập tên nhóm món mới:');
    if (groupName && !groups.includes(groupName)) {
      setGroups([...groups, groupName]);
    }
  };

  const handleDeleteProduct = async (id: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa món này không?')) return;
    try {
      const response = await fetch(`http://localhost:5000/api/Product/${id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        fetchProducts();
      }
    } catch (err) {
      alert('Lỗi khi xóa món');
    }
  };

  const handleToggleStatus = async (id: string) => {
    try {
      const response = await fetch(`http://localhost:5000/api/Product/${id}/toggle-status`, {
        method: 'PATCH'
      });
      if (response.ok) {
        fetchProducts();
      }
    } catch (err) {
      alert('Lỗi khi cập nhật trạng thái');
    }
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      {/* SIDEBAR FILTER (Chuẩn KiotViet) */}
      <div className="w-64 bg-white border-r overflow-y-auto p-4 space-y-6">
        <h2 className="font-bold text-base">Món</h2>

        {/* Loại thực đơn */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Loại thực đơn</p>
          <div className="space-y-1.5 ml-1">
            {['Đồ ăn', 'Đồ uống', 'Dịch vụ', 'Khác'].map(item => (
              <label key={item} className="flex items-center cursor-pointer">
                <input type="checkbox" className="mr-2 h-3.5 w-3.5 border-gray-300 rounded" defaultChecked={item === 'Đồ ăn' || item === 'Đồ uống'} />
                <span>{item}</span>
              </label>
            ))}
          </div>
        </div>

        {/* Loại món */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Loại món</p>
          <select className="w-full border-b border-gray-200 py-1 outline-none focus:border-blue-500 bg-transparent">
            <option>Chọn loại món</option>
          </select>
        </div>

        {/* Nhóm món */}
        <div className="space-y-2">
          <div className="flex justify-between items-center">
            <p className="font-bold text-gray-700">Nhóm món</p>
            <button
              onClick={handleAddGroup}
              className="text-blue-600 text-[11px] hover:underline"
            >
              Tạo mới
            </button>
          </div>
          <select
            className="w-full border-b border-gray-200 py-1 outline-none focus:border-blue-500 bg-transparent"
            value={filterGroup}
            onChange={(e) => setFilterGroup(e.target.value)}
          >
            <option value="">Tất cả nhóm món</option>
            {groups.map(group => (
              <option key={group} value={group}>{group}</option>
            ))}
          </select>
        </div>

        {/* Bán trực tiếp */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Bán trực tiếp</p>
          <div className="flex border rounded overflow-hidden">
             <button className="flex-1 py-1 bg-blue-600 text-white font-bold">Tất cả</button>
             <button className="flex-1 py-1 hover:bg-gray-50 border-l">Có</button>
             <button className="flex-1 py-1 hover:bg-gray-50 border-l">Không</button>
          </div>
        </div>

        {/* Trạng thái */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Trạng thái</p>
          <div className="space-y-1.5 ml-1">
            <label className="flex items-center cursor-pointer text-blue-600 font-bold">
              <input type="radio" name="status" className="mr-2" defaultChecked /> Đang kinh doanh
            </label>
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" /> Ngừng kinh doanh
            </label>
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" /> Tất cả
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
              onClick={() => setIsModalOpen(true)}
              className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700 transition-colors"
            >
              <Plus size={16} className="mr-1" /> Thêm mới
            </button>
            <button className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50">
              <Upload size={14} className="mr-1 text-gray-400" /> Import
            </button>
            <button className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50">
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
                <th className="px-4 py-2 w-32 uppercase text-[11px]">Mã món</th>
                <th className="px-4 py-2 uppercase text-[11px]">Tên món</th>
                <th className="px-4 py-2 w-40 uppercase text-[11px]">Nhóm món</th>
                <th className="px-4 py-2 w-40 uppercase text-[11px]">Loại thực đơn</th>
                <th className="px-4 py-2 w-32 uppercase text-[11px] text-right">Giá bán</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {loading ? (
                <tr><td colSpan={6} className="text-center py-20"><Loader2 className="animate-spin inline mr-2"/> Đang tải...</td></tr>
              ) : products.map(p => (
                <React.Fragment key={p.id}>
                  <tr
                    className={`hover:bg-[#eef6ff] cursor-pointer transition-colors ${expandedRow === p.id ? 'bg-[#eef6ff]' : ''}`}
                    onClick={() => setExpandedRow(expandedRow === p.id ? null : p.id!)}
                  >
                    <td className="px-4 py-3"><input type="checkbox" onClick={e => e.stopPropagation()}/></td>
                    <td className="px-4 py-3 text-blue-600 font-medium">{p.code}</td>
                    <td className="px-4 py-3 font-medium">{p.name}</td>
                    <td className="px-4 py-3 text-gray-500">{p.group || 'Cà phê'}</td>
                    <td className="px-4 py-3 text-gray-500">{p.category}</td>
                    <td className="px-4 py-3 text-right font-bold text-gray-800">{p.price.toLocaleString()}</td>
                  </tr>
                  {/* Expanded Detail View (Y hệt trong ảnh) */}
                  {expandedRow === p.id && (
                    <tr className="bg-white">
                      <td colSpan={6} className="p-0">
                        <div className="border-l-4 border-blue-500 ml-4 my-2 shadow-inner bg-[#fcfdfe]">
                          <div className="flex border-b text-[12px] font-bold text-gray-500">
                             {['Thông tin', 'Thành phần/Topping', 'Mô tả, ghi chú', 'Thẻ kho'].map((tab, i) => (
                               <button key={tab} className={`px-6 py-2 ${i === 0 ? 'text-blue-600 border-b-2 border-blue-600' : ''}`}>{tab}</button>
                             ))}
                          </div>
                          <div className="p-6 flex space-x-10">
                             <div className="w-32 h-32 bg-gray-100 rounded flex items-center justify-center border border-dashed border-gray-300">
                                <ImageIcon size={40} className="text-gray-300" />
                             </div>
                             <div className="flex-1">
                                <h3 className="text-lg font-bold mb-1 capitalize">{p.name}</h3>
                                <p className="text-orange-500 text-[11px] mb-4">✨ Sữa nóng <span className="ml-4 text-gray-400 font-normal">Món chế biến (Không tính định mức)</span></p>

                                <div className="grid grid-cols-3 gap-y-4 text-[12px]">
                                   <div><p className="text-gray-400">Mã món</p><p className="font-medium">{p.code}</p></div>
                                   <div><p className="text-gray-400">Nhóm món</p><p className="font-medium">{p.group || 'Cà phê'}</p></div>
                                   <div><p className="text-gray-400">Giá vốn</p><p className="font-medium">0</p></div>
                                   <div><p className="text-gray-400">Giá bán</p><p className="font-medium text-blue-600">{p.price.toLocaleString()}</p></div>
                                   <div><p className="text-gray-400">Vị trí</p><p className="font-medium text-gray-300">Chưa có</p></div>
                                   <div><p className="text-gray-400">Trọng lượng</p><p className="font-medium text-gray-300">Chưa có</p></div>
                                </div>
                                <div className="mt-8 flex justify-end space-x-2">
                                   <button
                                     onClick={(e) => { e.stopPropagation(); openEditModal(p); }}
                                     className="bg-blue-600 text-white px-4 py-1.5 rounded font-bold flex items-center hover:bg-blue-700"
                                   >
                                     <Edit2 size={14} className="mr-1"/> Chỉnh sửa
                                   </button>
                                   <button
                                     onClick={(e) => { e.stopPropagation(); handleToggleStatus(p.id!); }}
                                     className="border border-gray-300 px-4 py-1.5 rounded font-medium flex items-center hover:bg-gray-50"
                                   >
                                     <X size={14} className="mr-1"/> {p.isActive ? 'Ngừng kinh doanh' : 'Cho phép kinh doanh'}
                                   </button>
                                   <button
                                     onClick={(e) => { e.stopPropagation(); handleDeleteProduct(p.id!); }}
                                     className="border border-gray-300 px-4 py-1.5 rounded font-medium flex items-center hover:bg-red-50 hover:text-red-600 hover:border-red-200"
                                   >
                                     <Trash2 size={14} className="mr-1"/> Xóa
                                   </button>
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

      {/* FORM THÊM MỚI (Overlay như KiotViet) */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/40 z-[100] flex justify-center items-start pt-10 animate-in fade-in duration-200">
           <div className="bg-white w-full max-w-4xl rounded-lg shadow-2xl overflow-hidden animate-in slide-in-from-bottom-4 duration-300">
              <div className="bg-[#0070f4] p-4 text-white flex justify-between items-center">
                 <div className="flex items-center space-x-4">
                    <h3 className="font-bold text-lg">{editingProduct ? 'Cập nhật món ăn' : 'Thêm món mới'}</h3>
                    <div className="flex text-xs font-medium bg-white/20 rounded p-1">
                       <button className="px-3 py-1 bg-white text-blue-600 rounded shadow-sm">Món chế biến</button>
                       <button className="px-3 py-1 hover:bg-white/10 transition-colors">Hàng hóa</button>
                       <button className="px-3 py-1 hover:bg-white/10 transition-colors">Dịch vụ</button>
                    </div>
                 </div>
                 <button onClick={() => setIsModalOpen(false)} className="hover:rotate-90 transition-transform"><X size={24}/></button>
              </div>

              <div className="flex">
                 {/* Left Form fields */}
                 <div className="flex-1 p-8 grid grid-cols-2 gap-x-8 gap-y-6">
                    <div className="col-span-2 flex items-center space-x-4 mb-4">
                       <div className="w-24 h-24 bg-gray-50 border-2 border-dashed border-gray-200 rounded-lg flex flex-col items-center justify-center text-gray-400 cursor-pointer hover:bg-gray-100">
                          <ImageIcon size={24} />
                          <span className="text-[10px] mt-1 font-bold">Thêm ảnh</span>
                       </div>
                       <div className="flex-1 space-y-4">
                          <div className="border-b focus-within:border-blue-500 transition-colors">
                             <label className="block text-[11px] text-gray-400 font-bold uppercase">Tên món</label>
                             <input type="text" className="w-full py-1 outline-none text-base font-medium" placeholder="VD: Cà phê muối" value={newProduct.name} onChange={e => setNewProduct({...newProduct, name: e.target.value})} required/>
                          </div>
                          <div className="border-b focus-within:border-blue-500 transition-colors">
                             <label className="block text-[11px] text-gray-400 font-bold uppercase">Mã món</label>
                             <input type="text" className="w-full py-1 outline-none font-medium" placeholder="Mã tự động" value={newProduct.code} onChange={e => setNewProduct({...newProduct, code: e.target.value})}/>
                          </div>
                       </div>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Nhóm món</label>
                       <div className="flex items-center">
                          <select
                            className="w-full py-1 outline-none bg-transparent"
                            value={newProduct.group}
                            onChange={e => setNewProduct({...newProduct, group: e.target.value})}
                          >
                             <option value="">Chọn nhóm món</option>
                             {groups.map(group => (
                               <option key={group} value={group}>{group}</option>
                             ))}
                          </select>
                          <button
                            type="button"
                            onClick={handleAddGroup}
                            className="p-1 text-blue-600 hover:bg-blue-50 rounded"
                            title="Thêm nhóm mới"
                          >
                            <Plus size={16} />
                          </button>
                       </div>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Loại thực đơn</label>
                       <select className="w-full py-1 outline-none bg-transparent" value={newProduct.category} onChange={e => setNewProduct({...newProduct, category: e.target.value})}>
                          <option>Đồ uống</option>
                          <option>Đồ ăn</option>
                          <option>Dịch vụ</option>
                       </select>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Giá vốn</label>
                       <input type="number" className="w-full py-1 outline-none font-medium text-red-600" value={newProduct.costPrice} onChange={e => setNewProduct({...newProduct, costPrice: parseInt(e.target.value) || 0})}/>
                    </div>

                    <div className="border-b">
                       <label className="block text-[11px] text-gray-400 font-bold uppercase">Giá bán</label>
                       <input type="number" className="w-full py-1 outline-none font-medium text-blue-600 text-lg" value={newProduct.price} onChange={e => setNewProduct({...newProduct, price: parseInt(e.target.value) || 0})}/>
                    </div>
                 </div>

                 {/* Right Guide/Tips */}
                 <div className="w-64 bg-gray-50 p-6 border-l text-gray-500">
                    <div className="flex items-center space-x-2 text-blue-600 mb-4 font-bold">
                       <HelpCircle size={16} /> <span>Gợi ý</span>
                    </div>
                    <p className="text-[11px] leading-relaxed italic">
                       Hãy đặt Mã món dễ nhớ để thu ngân có thể tìm kiếm nhanh nhất tại màn hình POS. Ví dụ: CP001 cho Cà phê đá.
                    </p>
                 </div>
              </div>

              <div className="p-4 bg-gray-50 border-t flex justify-end space-x-3">
                 <button
                   onClick={() => { setIsModalOpen(false); setEditingProduct(null); }}
                   className="px-6 py-2 border rounded font-bold text-gray-600 hover:bg-white transition-colors"
                 >
                   Bỏ qua
                 </button>
                 <button onClick={handleAddProduct} className="px-6 py-2 bg-[#0070f4] text-white rounded font-bold shadow-lg hover:bg-blue-700 active:scale-95 transition-all">
                    {editingProduct ? 'Cập nhật' : 'Lưu món ăn'}
                 </button>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default ProductManagement;
