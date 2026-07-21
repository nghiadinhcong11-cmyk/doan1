import React from 'react';
import { User, Clock, PlusCircle } from 'lucide-react';

const TableMap = () => {
  const tables = [
    { id: 1, name: 'Bàn 01', status: 'Có khách', time: '15p', total: '150k' },
    { id: 2, name: 'Bàn 02', status: 'Trống', time: '', total: '' },
    { id: 3, name: 'Bàn 03', status: 'Có khách', time: '45p', total: '420k' },
    { id: 4, name: 'Bàn 04', status: 'Trống', time: '', total: '' },
    { id: 5, name: 'Bàn 05', status: 'Trống', time: '', total: '' },
    { id: 6, name: 'Bàn 06', status: 'Có khách', time: '5p', total: '85k' },
  ];

  return (
    <div className="min-h-screen bg-gray-100 p-4 pb-20">
      <header className="flex justify-between items-center mb-6">
        <h1 className="text-xl font-bold text-gray-800">Sơ đồ bàn</h1>
        <div className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center text-white font-bold">
          H
        </div>
      </header>

      <div className="grid grid-cols-2 gap-4">
        {tables.map((table) => (
          <div
            key={table.id}
            className={`p-4 rounded-2xl shadow-sm border-2 transition-all active:scale-95 ${
              table.status === 'Có khách'
              ? 'bg-white border-blue-500'
              : 'bg-white border-transparent'
            }`}
          >
            <div className="flex justify-between items-start mb-4">
              <span className="font-bold text-lg">{table.name}</span>
              {table.status === 'Có khách' && (
                <span className="bg-blue-100 text-blue-600 p-1 rounded-lg">
                  <User size={16} />
                </span>
              )}
            </div>

            {table.status === 'Có khách' ? (
              <div className="space-y-1">
                <p className="text-blue-600 font-bold">{table.total}</p>
                <p className="text-gray-400 text-xs flex items-center">
                  <Clock size={12} className="mr-1" /> {table.time}
                </p>
              </div>
            ) : (
              <div className="text-gray-300 flex flex-col items-center py-2">
                <PlusCircle size={24} />
                <span className="text-xs mt-1">Mở bàn</span>
              </div>
            )}
          </div>
        ))}
      </div>

      {/* Bottom Navigation */}
      <div className="fixed bottom-0 left-0 right-0 bg-white border-t flex justify-around py-3">
        <button className="flex flex-col items-center text-blue-600">
          <Grid size={20} />
          <span className="text-[10px] mt-1 font-bold">Bàn</span>
        </button>
        <button className="flex flex-col items-center text-gray-400">
          <ClipboardList size={20} />
          <span className="text-[10px] mt-1">Đơn hàng</span>
        </button>
        <button className="flex flex-col items-center text-gray-400">
          <Bell size={20} />
          <span className="text-[10px] mt-1">Thông báo</span>
        </button>
      </div>
    </div>
  );
};

const Grid = ({size}: {size: number}) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect width="7" height="7" x="3" y="3" rx="1"/><rect width="7" height="7" x="14" y="3" rx="1"/><rect width="7" height="7" x="14" y="14" rx="1"/><rect width="7" height="7" x="3" y="14" rx="1"/></svg>
);
const ClipboardList = ({size}: {size: number}) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect width="8" height="4" x="8" y="2" rx="1" ry="1"/><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><path d="M12 11h4"/><path d="M12 16h4"/><path d="M8 11h.01"/><path d="M8 16h.01"/></svg>
);
const Bell = ({size}: {size: number}) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9"/><path d="M10.3 21a1.94 1.94 0 0 0 3.4 0"/></svg>
);

export default TableMap;
