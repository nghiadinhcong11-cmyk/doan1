import React, { useState, useEffect } from 'react';
import { Plus, Trash2, Edit2, Loader2, X, Gift, Search, Percent, Calendar, CheckCircle2 } from 'lucide-react';
import { API_URL } from '../../../config';
import { notifyFeedback } from '../../../components/ui';

interface Promotion {
  id?: string;
  name: string;
  description: string;
  requiredPoints: number;
  discountValue: number;
  promotionType: 'Fixed' | 'Percentage' | 'Gift';
  isActive: boolean;
  endDate?: string;
}

const PromotionManagement = () => {
  const [promotions, setPromotions] = useState<Promotion[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPromo, setEditingToEdit] = useState<Promotion | null>(null);

  const [formData, setFormData] = useState<Promotion>({
    name: '',
    description: '',
    requiredPoints: 0,
    discountValue: 0,
    promotionType: 'Gift',
    isActive: true
  });

  const fetchPromotions = async () => {
    try {
      setLoading(true);
      // Giả sử API này sẽ được viết sau, hiện tại dùng data mẫu hoặc try catch để tránh lỗi
      const res = await fetch(`${API_URL}/api/Promotion`);
      if (res.ok) {
        const data = await res.json();
        setPromotions(data);
      }
    } catch (e) {
      console.error("Lỗi lấy danh sách khuyến mãi:", e);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchPromotions(); }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    try {
      const url = editingPromo ? `${API_URL}/api/Promotion/${editingPromo.id}` : `${API_URL}/api/Promotion`;
      const method = editingPromo ? 'PUT' : 'POST';

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(editingPromo ? { ...editingPromo, ...formData } : formData)
      });

      if (response.ok) {
        setIsModalOpen(false);
        resetForm();
        fetchPromotions();
      } else if (response.status !== 403) {
        notifyFeedback("Lỗi khi lưu chương trình");
      }
    } catch (err) {
      console.error(err);
      notifyFeedback("Lỗi kết nối server");
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      description: '',
      requiredPoints: 0,
      discountValue: 0,
      promotionType: 'Gift',
      isActive: true
    });
    setEditingToEdit(null);
  };

  const handleDelete = async (id: string) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    if (!window.confirm("Xóa chương trình này?")) return;
    try {
      const res = await fetch(`${API_URL}/api/Promotion/${id}`, { method: 'DELETE' });
      if (res.ok) fetchPromotions();
    } catch (e) { console.error(e); }
  };

  const openEditModal = (p: Promotion) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    setEditingToEdit(p);
    setFormData({ ...p });
    setIsModalOpen(true);
  };

  const toggleStatus = async (id: string) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    try {
      await fetch(`${API_URL}/api/Promotion/${id}/toggle`, { method: 'PATCH' });
      fetchPromotions();
    } catch (e) { console.error(e); }
  };

  return (
    <div className="p-6 bg-[#f0f2f5] min-h-screen font-sans text-gray-800">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-2xl font-black uppercase italic tracking-tighter">Quản lý Khuyến mãi & Đổi điểm</h1>
          <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest mt-1">Thiết lập các chương trình ưu đãi dành cho khách hàng thân thiết</p>
        </div>
        <button
          onClick={() => {
            if (localStorage.getItem('userRole') !== 'admin') {
              notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
              return;
            }
            setEditingToEdit(null); setIsModalOpen(true);
          }}
          className="bg-orange-500 text-white px-8 py-3 rounded-2xl font-black text-xs uppercase tracking-widest shadow-lg shadow-orange-500/20 hover:bg-orange-600 transition-all active:scale-95 flex items-center"
        >
          <Plus size={18} className="mr-2"/> TẠO CHƯƠNG TRÌNH MỚI
        </button>
      </div>

      {loading ? (
        <div className="flex justify-center py-20"><Loader2 className="animate-spin text-orange-500" size={40} /></div>
      ) : promotions.length === 0 ? (
        <div className="bg-white rounded-[3rem] p-20 text-center border-2 border-dashed border-gray-200">
           <Gift size={64} className="mx-auto text-orange-200 mb-6" />
           <h2 className="text-2xl font-black uppercase italic tracking-tighter text-gray-300">Chưa có chương trình khuyến mãi</h2>
           <p className="text-gray-400 text-sm font-medium mt-2 mb-8 max-w-md mx-auto">Hãy tạo các món quà hoặc mã giảm giá để khuyến khích khách hàng tích điểm và quay lại nhà hàng thường xuyên hơn.</p>
           <button
             onClick={() => setIsModalOpen(true)}
             className="bg-orange-500 text-white px-10 py-4 rounded-[2rem] font-black text-xs uppercase tracking-[0.2em] shadow-2xl shadow-orange-500/40 hover:bg-orange-600 transition-all flex items-center mx-auto"
           >
             <Plus size={18} className="mr-2"/> THIẾT LẬP NGAY
           </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {promotions.map((p) => (
            <div key={p.id} className="bg-white rounded-[2.5rem] p-6 shadow-sm border border-white hover:shadow-xl hover:shadow-orange-500/5 transition-all group flex flex-col">
              <div className="flex justify-between items-start mb-4">
                <div className={`p-3 rounded-2xl ${p.promotionType === 'Gift' ? 'bg-orange-50 text-orange-600' : 'bg-blue-50 text-blue-600'}`}>
                   {p.promotionType === 'Gift' ? <Gift size={24}/> : <Percent size={24}/>}
                </div>
                <div className="flex space-x-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button onClick={() => openEditModal(p)} className="p-2 text-gray-400 hover:text-blue-600 bg-gray-50 rounded-xl"><Edit2 size={14}/></button>
                  <button onClick={() => handleDelete(p.id!)} className="p-2 text-gray-400 hover:text-red-500 bg-gray-50 rounded-xl"><Trash2 size={14}/></button>
                </div>
              </div>

              <h3 className="text-lg font-black text-gray-800 uppercase tracking-tight mb-2 leading-tight">{p.name}</h3>
              <p className="text-xs text-gray-400 font-medium italic mb-6 flex-1">{p.description || 'Chương trình ưu đãi tri ân khách hàng thân thiết.'}</p>

              <div className="mt-auto pt-6 border-t border-dashed border-gray-100 flex items-center justify-between">
                <div>
                   <p className="text-[9px] font-black text-gray-400 uppercase mb-0.5">Yêu cầu</p>
                   <p className="text-xl font-black text-orange-600 tracking-tighter">{p.requiredPoints.toLocaleString()} ĐIỂM</p>
                </div>
                <button
                  onClick={() => toggleStatus(p.id!)}
                  className={`px-4 py-2 rounded-xl text-[9px] font-black uppercase tracking-widest border transition-all ${
                    p.isActive
                    ? 'bg-green-50 text-green-600 border-green-100'
                    : 'bg-gray-50 text-gray-400 border-gray-200'
                  }`}
                >
                  {p.isActive ? 'ĐANG CHẠY' : 'TẠM DỪNG'}
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {isModalOpen && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-[200] backdrop-blur-sm p-4">
           <form onSubmit={handleSave} className="bg-white rounded-[3rem] w-full max-w-xl shadow-2xl animate-in zoom-in-95 overflow-hidden">
              <div className="bg-orange-500 p-8 text-white flex justify-between items-center">
                 <div>
                    <h3 className="text-2xl font-black uppercase italic tracking-tighter flex items-center"><Gift className="mr-3"/> {editingPromo ? 'Cập nhật' : 'Tạo mới'} Khuyến mãi</h3>
                    <p className="text-[10px] font-bold opacity-70 uppercase tracking-widest mt-1">Cấu hình điều kiện đổi thưởng và giá trị ưu đãi</p>
                 </div>
                 <button type="button" onClick={() => setIsModalOpen(false)} className="bg-white/10 p-3 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <div className="p-10 space-y-6">
                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1 ml-1">Tên chương trình ưu đãi</label>
                    <input className="w-full px-6 py-4 bg-gray-50 rounded-2xl border-none outline-none font-black text-gray-800 focus:ring-2 focus:ring-orange-500/20" placeholder="VD: Đổi 100 điểm lấy 1 Ly Cà phê muối" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} required/>
                 </div>

                 <div className="grid grid-cols-2 gap-4">
                    <div>
                       <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1 ml-1">Loại ưu đãi</label>
                       <select className="w-full px-6 py-3.5 bg-gray-50 rounded-2xl border-none font-bold text-gray-600 appearance-none" value={formData.promotionType} onChange={e => setFormData({...formData, promotionType: e.target.value as any})}>
                          <option value="Gift">Quà tặng (Món ăn/Đồ uống)</option>
                          <option value="Percentage">Giảm giá % hóa đơn</option>
                          <option value="Fixed">Giảm số tiền cố định</option>
                       </select>
                    </div>
                    <div>
                       <label className="block text-[10px] font-black text-orange-600 uppercase tracking-widest mb-1 ml-1">Điểm yêu cầu</label>
                       <input type="number" className="w-full px-6 py-3.5 bg-orange-50 rounded-2xl border-none outline-none font-black text-orange-600" value={formData.requiredPoints} onChange={e => setFormData({...formData, requiredPoints: parseInt(e.target.value) || 0})} required/>
                    </div>
                 </div>

                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1 ml-1">Mô tả chi tiết</label>
                    <textarea className="w-full px-6 py-4 bg-gray-50 rounded-2xl border-none outline-none font-medium text-gray-600 text-xs" rows={3} placeholder="Mô tả nội dung khuyến mãi để khách hàng nắm rõ..." value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})}/>
                 </div>

                 <div className="flex gap-4 pt-4">
                    <button type="button" onClick={() => setIsModalOpen(false)} className="flex-1 py-4 bg-gray-100 rounded-full font-black text-gray-400 uppercase tracking-widest text-[10px]">Bỏ qua</button>
                    <button type="submit" className="flex-[2] py-4 bg-orange-500 text-white rounded-full font-black uppercase tracking-widest text-[10px] shadow-xl shadow-orange-500/40 hover:bg-orange-600">Lưu chương trình</button>
                 </div>
              </div>
           </form>
        </div>
      )}
    </div>
  );
};

export default PromotionManagement;

