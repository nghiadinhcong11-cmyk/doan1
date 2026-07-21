import React from 'react';
import { Info, ChevronRight, Zap, CreditCard, Landmark, LayoutGrid } from 'lucide-react';

const Dashboard = () => {
  return (
    <div className="p-4 bg-[#f0f2f5] min-h-screen font-sans text-gray-800">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-xl font-bold">Bức tranh kinh doanh</h1>
        <select className="bg-white border border-gray-300 text-sm rounded px-2 py-1 outline-none">
          <option>Tất cả chi nhánh</option>
        </select>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4">
        {/* Left Column: Main Stats and Charts */}
        <div className="lg:col-span-3 space-y-4">

          {/* Top 3 Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Doanh thu hôm nay */}
            <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg p-4 text-white shadow-sm relative overflow-hidden group">
               <div className="absolute right-[-10px] top-[-10px] opacity-10 group-hover:scale-110 transition-transform">
                  <Info size={100} />
               </div>
               <div className="flex justify-between items-start mb-2">
                 <span className="text-sm font-medium">Doanh thu hôm nay</span>
                 <span className="text-[10px] opacity-80 flex items-center">Bao gồm VAT <Info size={12} className="ml-1"/></span>
               </div>
               <div className="text-3xl font-bold mb-4">0</div>
               <div className="text-xs opacity-90 mb-2 italic">Không phát sinh doanh thu</div>
               <div className="border-t border-white/20 pt-2 space-y-1">
                 <div className="flex justify-between text-[11px]">
                   <span>Giảm giá hóa đơn</span>
                   <span>0</span>
                 </div>
                 <div className="flex justify-between text-[11px]">
                   <span>Trả hàng (0)</span>
                   <span>0</span>
                 </div>
               </div>
            </div>

            {/* Số lượng đơn hôm nay */}
            <div className="bg-gradient-to-br from-green-500 to-green-600 rounded-lg p-4 text-white shadow-sm relative overflow-hidden group">
               <div className="absolute right-[-10px] top-[-10px] opacity-10 group-hover:scale-110 transition-transform">
                  <Zap size={100} />
               </div>
               <div className="flex justify-between items-start mb-2">
                 <span className="text-sm font-medium">Số lượng đơn hôm nay</span>
               </div>
               <div className="text-3xl font-bold mb-4">0</div>
               <div className="text-xs opacity-90 mb-2 italic">Không phát sinh đơn</div>
               <div className="border-t border-white/20 pt-2 space-y-1">
                 <div className="flex justify-between text-[11px]">
                   <span>Trọng lạnh đơn</span>
                   <span>0</span>
                 </div>
                 <div className="flex justify-between text-[11px]">
                   <span>Số khách/đơn</span>
                   <span className="flex items-center">0 <Info size={12} className="ml-1"/></span>
                 </div>
               </div>
            </div>

            {/* Tỷ lệ phủ bàn */}
            <div className="bg-gradient-to-br from-orange-400 to-orange-500 rounded-lg p-4 text-white shadow-sm relative overflow-hidden group">
               <div className="absolute right-[-10px] top-[-10px] opacity-10 group-hover:scale-110 transition-transform">
                  <LayoutGrid size={100} />
               </div>
               <div className="flex justify-between items-start mb-2">
                 <span className="text-sm font-medium">Tỷ lệ phủ bàn</span>
                 <span className="text-lg font-bold">100%</span>
               </div>
               <div className="text-sm mb-4">1/1 đang sử dụng</div>
               <div className="border-t border-white/20 pt-2 space-y-1">
                 <div className="flex justify-between text-[11px]">
                   <span>Đơn đang phục vụ (1)</span>
                   <span>75,000</span>
                 </div>
                 <div className="flex justify-between text-[11px]">
                   <span>Khách đang phục vụ</span>
                   <span>2</span>
                 </div>
               </div>
            </div>
          </div>

          {/* Charts Row */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Doanh thu thuần */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200">
              <div className="p-3 border-b flex justify-between items-center">
                <span className="text-sm font-bold flex items-center">Doanh thu thuần <Info size={14} className="ml-1 text-gray-400"/></span>
                <select className="text-xs bg-gray-100 rounded px-1 py-0.5 outline-none border-none">
                  <option>7 ngày qua</option>
                </select>
              </div>
              <div className="p-3">
                 <div className="flex space-x-4 border-b mb-12">
                   <button className="text-xs font-bold text-blue-600 border-b-2 border-blue-600 pb-2 px-1">Theo giờ</button>
                   <button className="text-xs text-gray-500 pb-2 px-1">Theo ngày</button>
                   <button className="text-xs text-gray-500 pb-2 px-1">Theo thứ</button>
                 </div>
                 <div className="h-40 flex flex-col items-center justify-center text-gray-400">
                    <div className="flex items-end space-x-2 mb-2">
                       <div className="w-1.5 h-4 bg-blue-500 rounded-t"></div>
                       <div className="w-1.5 h-8 bg-blue-500 rounded-t"></div>
                       <div className="w-1.5 h-5 bg-blue-500 rounded-t"></div>
                    </div>
                    <span className="text-xs italic">Bạn chưa bán đơn nào</span>
                 </div>
              </div>
            </div>

            {/* Lượng khách hàng */}
            <div className="bg-white rounded-lg shadow-sm border border-gray-200">
              <div className="p-3 border-b flex justify-between items-center">
                <span className="text-sm font-bold">Lượng khách hàng</span>
                <select className="text-xs bg-gray-100 rounded px-1 py-0.5 outline-none border-none">
                  <option>7 ngày qua</option>
                </select>
              </div>
              <div className="p-3">
                 <div className="flex space-x-4 border-b mb-12">
                   <button className="text-xs font-bold text-blue-600 border-b-2 border-blue-600 pb-2 px-1">Theo giờ</button>
                   <button className="text-xs text-gray-500 pb-2 px-1">Theo ngày</button>
                   <button className="text-xs text-gray-500 pb-2 px-1">Theo thứ</button>
                 </div>
                 <div className="h-40 flex flex-col items-center justify-center text-gray-400">
                    <div className="flex items-center justify-center mb-2">
                      <svg width="60" height="30" viewBox="0 0 60 30" className="text-blue-200">
                         <path d="M0 25 Q15 10 30 20 T60 5" fill="none" stroke="currentColor" strokeWidth="2" />
                      </svg>
                    </div>
                    <span className="text-xs italic">Chưa có lượt khách nào</span>
                 </div>
              </div>
            </div>
          </div>

          {/* Bottom Row */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
             {/* Hiệu quả thực đơn */}
             <div className="bg-white rounded-lg shadow-sm border border-gray-200">
                <div className="p-3 border-b flex justify-between items-center">
                   <span className="text-sm font-bold">Hiệu quả thực đơn</span>
                   <div className="flex bg-blue-600 rounded p-0.5">
                      <button className="text-[10px] text-white px-2 py-0.5 font-bold bg-blue-700 rounded shadow-sm">Theo nhóm</button>
                      <button className="text-[10px] text-white px-2 py-0.5">Theo loại</button>
                   </div>
                </div>
                <div className="p-6 h-48 flex items-center justify-center">
                   <div className="w-24 h-24 rounded-full border-8 border-gray-100 flex items-center justify-center">
                      <div className="w-12 h-12 rounded-full bg-gray-50"></div>
                   </div>
                </div>
             </div>

             {/* Chi tiết hàng nhân viên */}
             <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
                <div className="p-3 border-b flex justify-between items-center bg-white">
                   <span className="text-sm font-bold">Chi tiết hàng nhân viên</span>
                   <select className="text-xs bg-gray-100 rounded px-1 py-0.5 border-none outline-none">
                      <option>Chọn nhóm</option>
                   </select>
                </div>
                <div className="p-0">
                   <table className="w-full text-[11px]">
                      <thead className="bg-gray-50 text-gray-500 border-b">
                         <tr>
                            <th className="px-3 py-2 text-left font-medium">STT</th>
                            <th className="px-3 py-2 text-left font-medium">Top 10 món bán chạy</th>
                            <th className="px-3 py-2 text-right font-medium">Số lượng bán</th>
                            <th className="px-3 py-2 text-right font-medium">Doanh thu thuần</th>
                         </tr>
                      </thead>
                      <tbody>
                         <tr className="border-b italic text-gray-400">
                            <td colSpan={4} className="px-3 py-8 text-center">Chưa có dữ liệu</td>
                         </tr>
                      </tbody>
                   </table>
                </div>
             </div>
          </div>

        </div>

        {/* Right Column: Quick Actions and Recent Activity */}
        <div className="space-y-4">
           {/* Quick Actions */}
           <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-2 space-y-1">
              <button className="w-full flex items-center p-2 hover:bg-gray-50 rounded transition-colors text-left group">
                 <div className="w-8 h-8 rounded bg-blue-100 flex items-center justify-center mr-3 group-hover:bg-blue-200 transition-colors">
                    <Zap size={16} className="text-blue-600" />
                 </div>
                 <div className="flex-1">
                    <p className="text-[11px] font-bold">Giao món siêu tốc</p>
                    <p className="text-[9px] text-gray-400">Grab, ShopeeFood, Loship...</p>
                 </div>
                 <ChevronRight size={14} className="text-gray-300" />
              </button>

              <button className="w-full flex items-center p-2 hover:bg-gray-50 rounded transition-colors text-left group">
                 <div className="w-8 h-8 rounded bg-green-100 flex items-center justify-center mr-3 group-hover:bg-green-200 transition-colors">
                    <CreditCard size={16} className="text-green-600" />
                 </div>
                 <div className="flex-1">
                    <p className="text-[11px] font-bold">Thanh toán</p>
                    <p className="text-[9px] text-gray-400">Cài đặt QR tích hợp miễn phí</p>
                 </div>
                 <ChevronRight size={14} className="text-gray-300" />
              </button>

              <button className="w-full flex items-center p-2 hover:bg-gray-50 rounded transition-colors text-left group">
                 <div className="w-8 h-8 rounded bg-blue-50 flex items-center justify-center mr-3 group-hover:bg-blue-100 transition-colors">
                    <Landmark size={16} className="text-blue-500" />
                 </div>
                 <div className="flex-1">
                    <p className="text-[11px] font-bold">Vay vốn</p>
                    <p className="text-[9px] text-gray-400">Giải ngân chỉ 1 tỷ đồng trong 24H</p>
                 </div>
                 <ChevronRight size={14} className="text-gray-300" />
              </button>
           </div>

           {/* Hoạt động gần đây */}
           <div className="bg-white rounded-lg shadow-sm border border-gray-200 flex flex-col h-[400px]">
              <div className="p-3 border-b">
                 <span className="text-sm font-bold">Hoạt động gần đây</span>
              </div>
              <div className="flex-1 flex flex-col items-center justify-center p-8 text-center">
                 <div className="w-16 h-16 bg-blue-50 rounded-full flex items-center justify-center mb-4">
                    <Zap size={32} className="text-blue-200" />
                 </div>
                 <p className="text-xs text-gray-400">Chưa có hoạt động nào</p>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
