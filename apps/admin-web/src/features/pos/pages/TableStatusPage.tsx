import React, { useState, useEffect } from 'react';
import { LayoutGrid, Loader2, Store, MapPin, User, Clock, RotateCcw } from 'lucide-react';
import { API_URL } from '../../../config';

interface Table {
  id: string;
  name: string;
  status: string;
  areaName: string;
  isActive: boolean;
}

const TableStatusPage = () => {
  const [tables, setTables] = useState<Table[]>([]);
  const [loading, setLoading] = useState(true);
  const branchName = localStorage.getItem('selectedBranchName') || 'Toàn hệ thống';
  const branchId = localStorage.getItem('selectedBranchId');

  const fetchTables = async () => {
    try {
      if (!branchId) return;
      const response = await fetch(`${API_URL}/api/Table?isActive=true&branchId=${branchId}`);
      const data: Table[] = await response.json();
      setTables(data);
    } catch (err) {
      console.error('Error fetching tables:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTables();
    const interval = setInterval(fetchTables, 10000); // 10s cập nhật 1 lần
    return () => clearInterval(interval);
  }, [branchId]);

  if (loading) return (
    <div className="flex items-center justify-center h-full bg-gray-50">
       <Loader2 size={32} className="animate-spin text-blue-600" />
    </div>
  );

  const areas = Array.from(new Set(tables.map(t => t.areaName)));

  return (
    <div className="p-6 bg-gray-50 min-h-full font-sans">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-2xl font-black text-gray-800 uppercase tracking-tighter italic flex items-center">
            <LayoutGrid className="mr-3 text-blue-600" /> Sơ đồ bàn & Trạng thái phục vụ
          </h1>
          <p className="text-xs text-gray-400 font-bold uppercase tracking-widest mt-1">
            <Store size={12} className="inline mr-1" /> {branchName} | Cập nhật tự động mỗi 10 giây
          </p>
        </div>
        <button onClick={fetchTables} className="p-2 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 text-gray-400 transition-all">
          <RotateCcw size={20} />
        </button>
      </div>

      <div className="space-y-12">
        {areas.map(area => (
          <div key={area} className="space-y-6">
            <h3 className="text-xs font-black text-gray-400 uppercase tracking-[0.2em] flex items-center">
              <div className="h-px bg-gray-200 flex-1 mr-4"></div>
              Khu vực: {area}
              <div className="h-px bg-gray-200 flex-1 ml-4"></div>
            </h3>

            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 xl:grid-cols-8 gap-6">
               {tables.filter(t => t.areaName === area).map((table) => (
                 <div
                   key={table.id}
                   className={`rounded-[2rem] p-6 flex flex-col items-center justify-center border-2 transition-all shadow-sm ${
                     table.status === 'Có khách'
                       ? 'bg-orange-50 border-orange-200 text-orange-700 shadow-orange-100'
                       : table.status === 'Đã đặt'
                       ? 'bg-blue-50 border-blue-200 text-blue-700'
                       : 'bg-white border-white text-gray-300 hover:border-gray-100'
                   }`}
                 >
                    <span className="text-lg font-black uppercase tracking-tight mb-1">{table.name}</span>
                    <div className={`text-[10px] font-black uppercase px-3 py-1 rounded-full border ${
                       table.status === 'Có khách' ? 'bg-orange-100 border-orange-200' :
                       table.status === 'Đã đặt' ? 'bg-blue-100 border-blue-200' : 'bg-gray-50 border-gray-100'
                    }`}>
                       {table.status}
                    </div>

                    {table.status === 'Có khách' && (
                       <div className="mt-4 flex space-x-2">
                          <div className="w-8 h-8 rounded-full bg-white flex items-center justify-center shadow-sm">
                             <User size={14} className="text-orange-500" />
                          </div>
                          <div className="w-8 h-8 rounded-full bg-white flex items-center justify-center shadow-sm">
                             <Clock size={14} className="text-orange-500" />
                          </div>
                       </div>
                    )}
                 </div>
               ))}
            </div>
          </div>
        ))}
      </div>

      {/* Legend */}
      <div className="mt-12 pt-8 border-t border-gray-200 flex flex-wrap gap-8">
         <div className="flex items-center space-x-3">
            <div className="w-4 h-4 rounded-full bg-white border-2 border-gray-100"></div>
            <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Bàn trống</span>
         </div>
         <div className="flex items-center space-x-3">
            <div className="w-4 h-4 rounded-full bg-orange-500"></div>
            <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Đang phục vụ</span>
         </div>
         <div className="flex items-center space-x-3">
            <div className="w-4 h-4 rounded-full bg-blue-500"></div>
            <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Đã đặt trước</span>
         </div>
      </div>
    </div>
  );
};

export default TableStatusPage;

