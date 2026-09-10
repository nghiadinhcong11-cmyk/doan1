import React, { useState, useMemo } from 'react';
import { Plus, Search, Printer, FileText, ChevronRight, Settings2, Trash2, Eye, X, CheckCircle2, Save } from 'lucide-react';

interface PrintTemplate { id: number; name: string; type: string; isDefault: boolean; lastUpdated: string; }

const PrintTemplates = () => {
  const [filterType, setFilterType] = useState('Tất cả');
  const [searchTerm, setSearchTerm] = useState('');
  const [templates, setTemplates] = useState<PrintTemplate[]>(() => {
    const saved = localStorage.getItem('print_templates');
    return saved ? JSON.parse(saved) : [
      { id: 1, name: 'Hóa đơn thanh toán (K80)', type: 'Hóa đơn khách', isDefault: true, lastUpdated: '18/07/2026' },
      { id: 2, name: 'Phiếu nhập kho (A5)', type: 'Nhập kho', isDefault: true, lastUpdated: '15/07/2026' },
    ];
  });

  const [selectedTemplate, setSelectedTemplate] = useState<any>(null);
  const [isPreviewOpen, setIsPreviewOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editForm, setEditForm] = useState({ name: '', type: '' });

  const filteredTemplates = useMemo(() => {
    return templates.filter(t => {
      const matchesType = filterType === 'Tất cả' || t.type === filterType;
      const matchesSearch = t.name.toLowerCase().includes(searchTerm.toLowerCase());
      return matchesType && matchesSearch;
    });
  }, [filterType, searchTerm, templates]);

  const handleDelete = (id: number) => {
    if (window.confirm('Bạn có chắc chắn muốn xóa mẫu in này?')) {
      setTemplates(prev => {
        const next = prev.filter(t => t.id !== id);
        localStorage.setItem('print_templates', JSON.stringify(next));
        return next;
      });
    }
  };

  const handleAdd = () => {
    setSelectedTemplate({ id: 0 });
    setEditForm({ name: '', type: 'Hóa đơn khách' });
    setIsEditOpen(true);
  };

  const handleEdit = (template: any) => {
    setSelectedTemplate(template);
    setEditForm({ name: template.name, type: template.type });
    setIsEditOpen(true);
  };

  const handleSaveEdit = () => {
    if (!editForm.name.trim()) return;
    setTemplates(prev => {
      const next = selectedTemplate.id === 0
        ? [...prev, { id: Date.now(), name: editForm.name.trim(), type: editForm.type, isDefault: false, lastUpdated: new Date().toLocaleDateString('vi-VN') }]
        : prev.map(t => t.id === selectedTemplate.id ? { ...t, name: editForm.name.trim(), type: editForm.type, lastUpdated: new Date().toLocaleDateString('vi-VN') } : t);
      localStorage.setItem('print_templates', JSON.stringify(next));
      return next;
    });
    setIsEditOpen(false);
    setSelectedTemplate(null);
  };

  const handlePreview = (template: any) => {
    setSelectedTemplate(template);
    setIsPreviewOpen(true);
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px] font-sans">
      <div className="w-64 bg-white border-r p-6 space-y-8 shadow-sm">
        <h2 className="font-black text-xl text-gray-800 uppercase tracking-tighter italic flex items-center">
           <Printer className="mr-2 text-blue-600" size={24}/> Mẫu in
        </h2>
        <div className="space-y-4">
           <div className="bg-gray-50 rounded-2xl p-4 border border-gray-100">
              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">Phân loại mẫu</p>
              <div className="space-y-1">
                 {['Tất cả', 'Hóa đơn khách', 'Nhập kho'].map(cat => (
                   <button
                     key={cat}
                     onClick={() => setFilterType(cat)}
                     className={`w-full text-left px-3 py-2 rounded-xl text-xs font-bold transition-all ${filterType === cat ? 'bg-white shadow-md text-blue-600' : 'text-gray-500 hover:bg-white/50'}`}
                   >
                     {cat}
                   </button>
                 ))}
              </div>
           </div>
        </div>
      </div>

      <div className="flex-1 flex flex-col overflow-hidden">
        <div className="bg-white p-3 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-80">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-2 bg-gray-100 border-none rounded-xl outline-none focus:ring-1 focus:ring-blue-500 text-xs font-bold"
              placeholder="Tìm tên mẫu in..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button onClick={handleAdd} className="bg-[#0070f4] text-white px-6 py-2 rounded-xl flex items-center text-[11px] font-black uppercase tracking-widest hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/20 active:scale-95">
            <Plus size={16} className="mr-2" /> Thêm mẫu mới
          </button>
        </div>

        <div className="p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 overflow-auto">
          {filteredTemplates.length === 0 ? (
            <div className="col-span-full py-20 text-center">
              <FileText size={48} className="mx-auto text-gray-200 mb-4" />
              <p className="text-gray-400 font-bold uppercase tracking-widest text-[10px]">Không tìm thấy mẫu in phù hợp</p>
            </div>
          ) : (
            filteredTemplates.map(t => (
              <div key={t.id} className="bg-white rounded-[2rem] shadow-xl shadow-blue-500/5 border border-gray-100 overflow-hidden flex flex-col hover:border-blue-300 transition-all group">
                <div className="p-6 flex items-start justify-between">
                  <div className="flex items-center">
                    <div className="w-12 h-12 bg-blue-50 rounded-2xl flex items-center justify-center text-blue-600 mr-4 shadow-inner">
                      <Printer size={24} />
                    </div>
                    <div>
                      <h3 className="font-black text-gray-800 uppercase tracking-tight">{t.name}</h3>
                      <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest mt-0.5">{t.type}</p>
                    </div>
                  </div>
                  {t.isDefault && (
                    <span className="bg-green-50 text-green-600 text-[8px] font-black px-2 py-1 rounded-lg uppercase tracking-widest border border-green-100">Mặc định</span>
                  )}
                </div>

                <div className="flex-1 px-6 py-10 bg-gray-50/50 flex flex-col items-center justify-center border-y border-gray-100 relative">
                   <FileText size={48} className="text-gray-200" />
                   <div className="absolute inset-0 bg-blue-600/5 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                      <button
                        onClick={() => handlePreview(t)}
                        className="bg-white text-blue-600 px-6 py-2 rounded-xl shadow-xl text-[10px] font-black uppercase tracking-widest flex items-center hover:scale-105 transition-transform"
                      >
                         <Eye size={14} className="mr-2" /> Xem trước
                      </button>
                   </div>
                </div>

                <div className="p-4 bg-white flex justify-between items-center px-6">
                  <span className="text-[10px] text-gray-400 font-bold italic">Cập nhật: {t.lastUpdated}</span>
                  <div className="flex space-x-1">
                    <button
                      onClick={() => handleEdit(t)}
                      className="p-2 text-blue-500 hover:bg-blue-50 rounded-xl transition-colors"
                      title="Thiết kế"
                    >
                      <Settings2 size={18}/>
                    </button>
                    <button
                      onClick={() => handleDelete(t.id)}
                      className="p-2 text-red-500 hover:bg-red-50 rounded-xl transition-colors"
                      title="Xóa"
                    >
                      <Trash2 size={18}/>
                    </button>
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      {/* PREVIEW MODAL */}
      {isPreviewOpen && selectedTemplate && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-sm animate-in fade-in duration-300">
           <div className="bg-white rounded-[2rem] w-full max-w-md overflow-hidden shadow-2xl flex flex-col max-h-[90vh]">
              <div className="p-6 border-b flex justify-between items-center bg-gray-50">
                 <h3 className="font-black uppercase italic tracking-tighter text-gray-800 flex items-center">
                    <Eye size={18} className="mr-2 text-blue-600" /> Xem trước mẫu in
                 </h3>
                 <button onClick={() => setIsPreviewOpen(false)} className="text-gray-400 hover:text-gray-800 transition-all"><X size={24}/></button>
              </div>
              <div className="flex-1 overflow-auto p-8 bg-gray-200/50 flex justify-center">
                 {/* Simulated Paper */}
                 <div className={`bg-white shadow-lg p-6 font-mono text-[11px] text-black ${selectedTemplate.type === 'Nhập kho' ? 'w-full aspect-[1/1.4]' : 'w-[280px]'}`}>
                    <div className="text-center space-y-1 mb-6">
                       <p className="font-bold text-sm uppercase">DOAN RESTAURANT</p>
                       <p>Địa chỉ: 123 Đường Số 1, Tân Phong, Q7</p>
                       <p>SĐT: 0987.654.321</p>
                    </div>

                    <div className="text-center mb-4">
                       <p className="font-bold text-xs uppercase border-y py-1">{selectedTemplate.name}</p>
                    </div>

                    <div className="space-y-1 mb-4">
                       <p>Mã đơn: HD20260723-0001</p>
                       <p>Ngày: {new Date().toLocaleString()}</p>
                       <p>Nhân viên: Quản trị viên</p>
                    </div>

                    <table className="w-full border-b pb-2 mb-2">
                       <thead className="border-b">
                          <tr>
                             <th className="text-left py-1">Tên món</th>
                             <th className="text-center py-1">SL</th>
                             <th className="text-right py-1">Tiền</th>
                          </tr>
                       </thead>
                       <tbody>
                          <tr>
                             <td className="py-1">Cà phê muối</td>
                             <td className="text-center py-1">2</td>
                             <td className="text-right py-1">70,000</td>
                          </tr>
                          <tr>
                             <td className="py-1">Bạc xỉu</td>
                             <td className="text-center py-1">1</td>
                             <td className="text-right py-1">30,000</td>
                          </tr>
                       </tbody>
                    </table>

                    <div className="space-y-1 text-right">
                       <p className="flex justify-between"><span>Tổng cộng:</span> <b>100,000</b></p>
                       <p className="flex justify-between"><span>VAT (8%):</span> <b>8,000</b></p>
                       <p className="flex justify-between text-xs font-bold pt-2 border-t mt-2"><span>THANH TOÁN:</span> 108,000</p>
                    </div>

                    <div className="mt-8 text-center space-y-1 opacity-50 italic">
                       <p>Cám ơn quý khách. Hẹn gặp lại!</p>
                       <p>Powered by DOAN POS</p>
                    </div>
                 </div>
              </div>
              <div className="p-6 border-t bg-gray-50 flex space-x-3">
                 <button onClick={() => window.print()} className="flex-1 py-3 bg-blue-600 text-white rounded-xl font-black text-[10px] uppercase tracking-widest shadow-lg shadow-blue-500/20 hover:bg-blue-700 transition-all flex items-center justify-center">
                    <Printer size={16} className="mr-2" /> In thử ngay
                 </button>
              </div>
           </div>
        </div>
      )}

      {/* EDIT MODAL */}
      {isEditOpen && selectedTemplate && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-sm animate-in fade-in duration-300">
           <div className="bg-white rounded-[2rem] w-full max-w-sm overflow-hidden shadow-2xl flex flex-col">
              <div className="p-6 border-b flex justify-between items-center bg-gray-50">
                 <h3 className="font-black uppercase italic tracking-tighter text-gray-800 flex items-center">
                    <Settings2 size={18} className="mr-2 text-blue-600" /> Chỉnh sửa mẫu in
                 </h3>
                 <button onClick={() => setIsEditOpen(false)} className="text-gray-400 hover:text-gray-800 transition-all"><X size={24}/></button>
              </div>
              <div className="p-8 space-y-6">
                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Tên mẫu in</label>
                    <input
                      type="text"
                      className="w-full px-5 py-3.5 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-gray-700 transition-all"
                      value={editForm.name}
                      onChange={e => setEditForm({...editForm, name: e.target.value})}
                    />
                 </div>
                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2 ml-2">Loại mẫu</label>
                    <select
                       className="w-full px-5 py-3.5 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-gray-700 transition-all appearance-none"
                       value={editForm.type}
                       onChange={e => setEditForm({...editForm, type: e.target.value})}
                    >
                       <option value="Hóa đơn khách">Hóa đơn khách</option>
                       <option value="Nhập kho">Nhập kho</option>
                    </select>
                 </div>

                 <div className="pt-4 flex space-x-3">
                    <button onClick={() => setIsEditOpen(false)} className="flex-1 py-4 bg-gray-100 text-gray-400 rounded-2xl font-black uppercase tracking-widest text-[10px]">Hủy</button>
                    <button onClick={handleSaveEdit} className="flex-[2] py-4 bg-blue-600 text-white rounded-2xl font-black uppercase tracking-widest text-[10px] shadow-lg shadow-blue-500/20 hover:bg-blue-700 transition-all flex items-center justify-center">
                       <Save size={16} className="mr-2" /> Lưu thay đổi
                    </button>
                 </div>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default PrintTemplates;

