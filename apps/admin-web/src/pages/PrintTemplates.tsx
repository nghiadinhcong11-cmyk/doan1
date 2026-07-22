import React from 'react';
import { Plus, Search, Printer, FileText, ChevronRight, Settings2, Trash2, Eye } from 'lucide-react';

const PrintTemplates = () => {
  const templates = [
    { id: 1, name: 'Hóa đơn thanh toán (K80)', type: 'Hóa đơn', isDefault: true, lastUpdated: '18/07/2026' },
    { id: 2, name: 'Phiếu báo bếp (K57)', type: 'Báo bếp', isDefault: true, lastUpdated: '15/07/2026' },
    { id: 3, name: 'Tem nhãn dán ly (40x30)', type: 'Tem nhãn', isDefault: false, lastUpdated: '10/07/2026' },
    { id: 4, name: 'Hóa đơn khổ A5', type: 'Hóa đơn', isDefault: false, lastUpdated: '01/07/2026' },
  ];

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      <div className="w-64 bg-white border-r p-4 space-y-6">
        <h2 className="font-bold text-base">Quản lý mẫu in</h2>
        <div className="space-y-4">
           <div className="bg-gray-50 rounded-lg p-3">
              <p className="text-[11px] font-bold text-gray-400 uppercase mb-2">Phân loại</p>
              <div className="space-y-1">
                 {['Tất cả', 'Hóa đơn', 'Báo bếp', 'Tem nhãn', 'Phiếu chi'].map(cat => (
                   <button key={cat} className={`w-full text-left px-2 py-1.5 rounded text-xs ${cat === 'Tất cả' ? 'bg-white shadow-sm text-blue-600 font-bold' : 'text-gray-600 hover:bg-gray-100'}`}>
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
            <Search className="absolute left-3 top-2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-1.5 bg-gray-100 border-none rounded-md outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Tìm tên mẫu in..."
            />
          </div>
          <button className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700 transition-colors">
            <Plus size={16} className="mr-1" /> Thêm mẫu mới
          </button>
        </div>

        <div className="p-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 overflow-auto">
          {templates.map(t => (
            <div key={t.id} className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden flex flex-col hover:border-blue-300 transition-colors group">
              <div className="p-4 flex items-start justify-between">
                <div className="flex items-center">
                  <div className="w-10 h-10 bg-gray-50 rounded flex items-center justify-center text-gray-400 mr-3">
                    <Printer size={20} />
                  </div>
                  <div>
                    <h3 className="font-bold text-gray-800">{t.name}</h3>
                    <p className="text-[11px] text-gray-400">{t.type}</p>
                  </div>
                </div>
                {t.isDefault && (
                  <span className="bg-green-100 text-green-700 text-[9px] font-bold px-1.5 py-0.5 rounded uppercase">Mặc định</span>
                )}
              </div>

              <div className="flex-1 px-4 py-8 bg-gray-50 flex flex-col items-center justify-center border-y border-gray-100 relative">
                 <FileText size={40} className="text-gray-200" />
                 <div className="absolute inset-0 bg-black/5 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                    <button className="bg-white text-gray-700 px-3 py-1.5 rounded shadow text-xs font-bold flex items-center hover:bg-blue-50">
                       <Eye size={14} className="mr-1.5" /> Xem trước
                    </button>
                 </div>
              </div>

              <div className="p-2 bg-white flex justify-between items-center">
                <span className="text-[10px] text-gray-400 ml-2">Cập nhật: {t.lastUpdated}</span>
                <div className="flex">
                  <button className="p-1.5 text-blue-600 hover:bg-blue-50 rounded" title="Thiết kế"><Settings2 size={16}/></button>
                  <button className="p-1.5 text-red-600 hover:bg-red-50 rounded" title="Xóa"><Trash2 size={16}/></button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default PrintTemplates;
