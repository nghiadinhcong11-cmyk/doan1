import type { CartItem, Table } from '../types';

interface POSTableMapProps {
  tables: Table[];
  tableCarts: Record<string, CartItem[]>;
  selectedTableId: string;
  onSelectTable: (tableId: string) => void;
}

export default function POSTableMap({ tables, tableCarts, selectedTableId, onSelectTable }: POSTableMapProps) {
  const areas = Array.from(new Set(tables.map(table => table.areaName)));

  return (
    <div className="space-y-10">
      {areas.map(area => (
        <div key={area} className="space-y-4">
          <h3 className="text-[10px] font-black text-[#0070f4] uppercase tracking-[0.2em] flex items-center">
            <div className="h-px bg-blue-100 flex-1 mr-4"></div>
            Khu vực: {area}
            <div className="h-px bg-blue-100 flex-1 ml-4"></div>
          </h3>
          <div className="grid grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-4">
            {tables.filter(table => table.areaName === area).map((table, index) => {
              const hasItems = (tableCarts[table.id] || []).length > 0;
              const itemCount = (tableCarts[table.id] || []).reduce((sum, item) => sum + item.quantity, 0);

              return (
                <button
                  type="button"
                  key={`${table.id}-${index}`}
                  onClick={() => onSelectTable(table.id)}
                  aria-label={`Chọn ${table.areaName || 'khu vực'} ${table.name}`}
                  className={`rounded-2xl shadow-sm aspect-square flex flex-col items-center justify-center cursor-pointer transition-all border-2 relative ${
                    selectedTableId === table.id
                      ? 'bg-blue-50 border-[#0070f4] text-[#0070f4] ring-4 ring-blue-100 scale-105 z-10 shadow-xl'
                      : table.status !== 'Trống' || hasItems
                        ? 'bg-orange-50 border-orange-200 text-orange-700'
                        : 'bg-white border-gray-100 text-gray-400 hover:border-blue-200 hover:text-[#0070f4]'
                  }`}
                >
                  <span className="text-[9px] font-bold uppercase tracking-wide text-gray-400">{table.areaName || 'Khu vực chưa đặt tên'}</span>
                  <span className="text-xs font-black uppercase tracking-widest mt-0.5">{table.name}</span>
                  <span className={`text-[9px] font-bold mt-1 uppercase ${table.status === 'Trống' && !hasItems ? 'text-green-500' : 'text-orange-500'}`}>
                    {hasItems || table.status === 'Có khách' ? 'Đang xử lý' : table.status}
                  </span>
                  {(hasItems || table.status === 'Có khách') && (
                    <span className="absolute -top-2 -right-1 bg-blue-600 text-white text-[10px] font-black px-2 py-0.5 rounded-full border-2 border-white shadow-lg">
                      {itemCount > 0 ? itemCount : '!'}
                    </span>
                  )}
                  {table.status === 'Có khách' && (
                    <div className="absolute -bottom-2 bg-[#0070f4] text-white text-[7px] font-black px-2 py-0.5 rounded-full uppercase tracking-tighter shadow-sm">Đang dùng</div>
                  )}
                </button>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}
