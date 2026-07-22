import React, { useState, useEffect } from 'react';
import { Info, ChevronRight, Zap, CreditCard, Landmark, LayoutGrid, Loader2, TrendingUp, Users, ShoppingBag, Clock, ArrowUpRight, MoreHorizontal } from 'lucide-react';
import { API_URL } from '../config';

interface DashboardData {
  todayRevenue: number;
  totalOrders: number;
  customerCount: number;
  estimatedProfit: number;
  topProducts: Array<{ name: string, quantity: number, revenue: number }>;
  chartData: Array<{ time: string, amount: number }>;
  recentOrders: Array<{ invoiceCode: string, customerName: string, totalAmount: number, createdAt: string, tableName: string }>;
}

const Dashboard = () => {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(`${API_URL}/api/Dashboard/summary`)
      .then(res => res.json())
      .then(json => {
        setData(json);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  if (loading) return (
    <div className="flex items-center justify-center h-screen bg-[#f0f2f5]">
      <Loader2 className="animate-spin text-blue-600" size={40} />
    </div>
  );

  return (
    <div className="p-4 bg-[#f0f2f5] min-h-screen font-sans text-gray-800">
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-xl font-black text-gray-800 uppercase tracking-tight">Bức tranh kinh doanh</h1>
        <div className="flex space-x-2">
           <select className="bg-white border border-gray-200 text-xs font-bold rounded shadow-sm px-3 py-1.5 outline-none text-gray-600">
             <option>Hôm nay</option>
             <option>Hôm qua</option>
             <option>7 ngày qua</option>
           </select>
           <button className="bg-blue-600 text-white px-4 py-1.5 rounded text-xs font-bold shadow-md hover:bg-blue-700 transition-all">
             Tải lại dữ liệu
           </button>
        </div>
      </div>

      {/* Top 3 Summary Cards (Chuẩn KiotViet) */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-xl p-5 shadow-sm border-l-4 border-blue-500 relative overflow-hidden group hover:shadow-md transition-all">
           <div className="flex justify-between items-center mb-3">
             <span className="text-[11px] font-bold text-gray-400 uppercase tracking-wider">Doanh thu thuần</span>
             <div className="p-1.5 bg-blue-50 rounded-lg text-blue-600"><TrendingUp size={16}/></div>
           </div>
           <div className="text-2xl font-black text-blue-700">{(data?.todayRevenue || 0).toLocaleString()}</div>
           <p className="text-[10px] text-gray-400 mt-2 flex items-center">
              <span className="text-green-500 font-bold mr-1">↑ 12%</span> so với hôm qua
           </p>
        </div>

        <div className="bg-white rounded-xl p-5 shadow-sm border-l-4 border-green-500 relative overflow-hidden group hover:shadow-md transition-all">
           <div className="flex justify-between items-center mb-3">
             <span className="text-[11px] font-bold text-gray-400 uppercase tracking-wider">Số lượng đơn hàng</span>
             <div className="p-1.5 bg-green-50 rounded-lg text-green-600"><ShoppingBag size={16}/></div>
           </div>
           <div className="text-2xl font-black text-green-700">{data?.totalOrders || 0}</div>
           <p className="text-[10px] text-gray-400 mt-2 flex items-center">
              Chưa có đơn hủy
           </p>
        </div>

        <div className="bg-white rounded-xl p-5 shadow-sm border-l-4 border-orange-500 relative overflow-hidden group hover:shadow-md transition-all">
           <div className="flex justify-between items-center mb-3">
             <span className="text-[11px] font-bold text-gray-400 uppercase tracking-wider">Lượng khách hàng</span>
             <div className="p-1.5 bg-orange-50 rounded-lg text-orange-600"><Users size={16}/></div>
           </div>
           <div className="text-2xl font-black text-orange-700">{data?.customerCount || 0}</div>
           <p className="text-[10px] text-gray-400 mt-2 italic">Tính trên số hóa đơn định danh</p>
        </div>
      </div>

      {/* Main Grid Content */}
      <div className="grid grid-cols-12 gap-6">

        {/* Left Column: Charts and Tables */}
        <div className="col-span-12 lg:col-span-8 space-y-6">

           {/* Doanh thu theo giờ Chart */}
           <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
              <div className="flex justify-between items-center mb-8">
                 <h3 className="font-bold text-sm text-gray-700 flex items-center">
                    <Clock size={16} className="mr-2 text-blue-500" /> Doanh thu theo giờ
                 </h3>
                 <button className="text-[10px] font-bold text-blue-600 hover:underline uppercase">Xem báo cáo chi tiết</button>
              </div>
              <div className="h-48 flex items-end justify-between px-2">
                 {[...Array(24)].map((_, hour) => {
                   const hourStr = `${hour}:00`;
                   const hourData = data?.chartData.find(d => d.time === hourStr);
                   const height = hourData ? Math.min((hourData.amount / (data?.todayRevenue || 1)) * 100, 100) : 0;

                   return (
                     <div key={hour} className="flex-1 flex flex-col items-center group relative h-full justify-end">
                        {hourData && (
                          <div className="absolute bottom-full mb-2 bg-gray-800 text-white text-[9px] py-1 px-2 rounded opacity-0 group-hover:opacity-100 transition-opacity z-10 whitespace-nowrap">
                             {hourData.amount.toLocaleString()} đ
                          </div>
                        )}
                        <div
                          className={`w-full max-w-[12px] rounded-t-sm transition-all duration-500 ${height > 0 ? 'bg-blue-500 group-hover:bg-blue-600' : 'bg-gray-100'}`}
                          style={{ height: height > 0 ? `${height}%` : '4px' }}
                        ></div>
                        <span className="text-[8px] text-gray-400 mt-2 hidden md:block">{hour % 4 === 0 ? hourStr : ''}</span>
                     </div>
                   );
                 })}
              </div>
              {(!data?.chartData || data.chartData.length === 0) && (
                <div className="text-center py-10 text-gray-300 italic text-xs">Chưa có dữ liệu bán hàng trong hôm nay</div>
              )}
           </div>

           {/* Top Products & Efficiency */}
           <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-5">
                 <h3 className="font-bold text-sm text-gray-700 mb-4 border-b pb-3">Top mặt hàng bán chạy</h3>
                 <div className="space-y-4">
                    {data?.topProducts.map((p, i) => (
                      <div key={i} className="flex items-center justify-between">
                         <div className="flex items-center">
                            <span className="w-5 h-5 rounded-full bg-gray-100 text-[10px] flex items-center justify-center font-bold text-gray-500 mr-3">{i+1}</span>
                            <span className="text-xs font-medium text-gray-700 capitalize">{p.name}</span>
                         </div>
                         <div className="text-right">
                            <p className="text-xs font-bold text-gray-800">{p.quantity}</p>
                            <p className="text-[9px] text-gray-400">{p.revenue.toLocaleString()} đ</p>
                         </div>
                      </div>
                    ))}
                    {(!data?.topProducts || data.topProducts.length === 0) && (
                       <div className="text-center py-8 text-gray-300 italic text-xs">Chưa có dữ liệu</div>
                    )}
                 </div>
              </div>

              <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-5">
                 <h3 className="font-bold text-sm text-gray-700 mb-4 border-b pb-3">Hiệu quả kinh doanh</h3>
                 <div className="space-y-6">
                    <div className="flex justify-between items-center">
                       <span className="text-xs text-gray-500">Doanh thu mục tiêu</span>
                       <span className="text-xs font-bold">10,000,000 đ</span>
                    </div>
                    <div className="w-full bg-gray-100 h-2 rounded-full overflow-hidden">
                       <div className="bg-blue-500 h-full rounded-full" style={{ width: `${Math.min(((data?.todayRevenue || 0) / 10000000) * 100, 100)}%` }}></div>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                       <div className="bg-blue-50 p-3 rounded-lg text-center">
                          <p className="text-[9px] text-blue-600 font-bold uppercase mb-1">Lợi nhuận dự tính</p>
                          <p className="text-sm font-black text-blue-700">{(data?.estimatedProfit || 0).toLocaleString()}</p>
                       </div>
                       <div className="bg-green-50 p-3 rounded-lg text-center">
                          <p className="text-[9px] text-green-600 font-bold uppercase mb-1">Tỷ lệ lấp đầy</p>
                          <p className="text-sm font-black text-green-700">65%</p>
                       </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        {/* Right Column: Recent Activities */}
        <div className="col-span-12 lg:col-span-4">
           <div className="bg-white rounded-xl shadow-sm border border-gray-200 flex flex-col h-full overflow-hidden">
              <div className="p-4 border-b bg-gray-50/50 flex justify-between items-center">
                 <h3 className="font-bold text-sm text-gray-700 flex items-center uppercase tracking-tight">
                    <Zap size={16} className="mr-2 text-orange-400 fill-orange-400" /> Hoạt động gần đây
                 </h3>
                 <button className="text-gray-400 hover:text-blue-600 transition-colors"><MoreHorizontal size={18}/></button>
              </div>
              <div className="flex-1 overflow-auto p-4 space-y-4">
                 {data?.recentOrders.map((order, i) => (
                   <div key={i} className="flex items-start space-x-3 group cursor-pointer hover:bg-gray-50 p-2 rounded-lg transition-colors">
                      <div className="w-8 h-8 rounded-lg bg-blue-50 flex items-center justify-center text-blue-600 shrink-0 border border-blue-100 group-hover:bg-blue-600 group-hover:text-white transition-all">
                         <CreditCard size={16} />
                      </div>
                      <div className="flex-1 border-b border-gray-100 pb-3 group-last:border-none">
                         <div className="flex justify-between items-start">
                            <p className="text-xs font-bold text-gray-800">{order.invoiceCode}</p>
                            <span className="text-[9px] text-gray-400">{new Date(order.createdAt).toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit'})}</span>
                         </div>
                         <p className="text-[11px] text-gray-500 mt-1">
                            <span className="font-medium text-blue-600">{order.customerName}</span> vừa thanh toán đơn hàng tại <span className="font-bold">{order.tableName || 'Mang về'}</span>
                         </p>
                         <p className="text-xs font-black text-gray-800 mt-2 tracking-tight">{order.totalAmount.toLocaleString()} đ</p>
                      </div>
                   </div>
                 ))}
                 {(!data?.recentOrders || data.recentOrders.length === 0) && (
                   <div className="text-center py-20">
                      <Info size={32} className="mx-auto text-gray-100 mb-2" />
                      <p className="text-[11px] text-gray-400 italic">Chưa có giao dịch phát sinh</p>
                   </div>
                 )}
              </div>
              <div className="p-4 bg-gray-50 border-t text-center">
                 <button className="text-xs font-bold text-blue-600 hover:underline flex items-center justify-center w-full">
                    Xem tất cả hoạt động <ChevronRight size={14} className="ml-1" />
                 </button>
              </div>
           </div>
        </div>

      </div>
    </div>
  );
};

export default Dashboard;
