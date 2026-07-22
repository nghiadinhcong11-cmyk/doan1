import React, { useState, useEffect } from 'react';
import { Search, Filter, FileText, Download, ChevronRight, Calendar, Loader2 } from 'lucide-react';

interface OrderDetail {
  id: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

interface Order {
  id: string;
  invoiceCode: string;
  createdAt: string;
  customerName: string;
  totalAmount: number;
  discount: number;
  paidAmount: number;
  status: string;
  paymentMethod: string;
  tableName?: string;
  details?: OrderDetail[];
}

const InvoiceHistory = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const response = await fetch('http://localhost:5000/api/Order');
      const data = await response.json();
      setOrders(data);
    } catch (err) {
      console.error('Error fetching orders:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, []);

  const filteredOrders = orders.filter(o =>
    o.invoiceCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
    o.customerName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="flex h-[calc(100vh-48px)] bg-gray-100">
      {/* Sidebar Filters */}
      <div className="w-72 bg-white border-r p-4 hidden md:block overflow-y-auto shadow-sm">
        <h2 className="font-bold text-lg mb-6 text-gray-800">Hóa đơn</h2>

        <div className="space-y-6">
          <div>
            <label className="text-[11px] font-bold text-gray-400 uppercase tracking-wider">Tìm kiếm</label>
            <input
              type="text"
              placeholder="Theo mã hóa đơn, tên khách"
              className="mt-2 w-full p-2 border border-gray-200 rounded text-xs outline-none focus:ring-1 focus:ring-blue-500"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>

          <div>
            <label className="text-[11px] font-bold text-gray-400 uppercase tracking-wider">Thời gian</label>
            <div className="mt-3 space-y-2">
              <label className="flex items-center text-xs cursor-pointer text-blue-600 font-bold">
                <input type="radio" name="time" className="mr-2" defaultChecked /> Hôm nay
              </label>
              <label className="flex items-center text-xs cursor-pointer text-gray-600">
                <input type="radio" name="time" className="mr-2" /> Lựa chọn khác
              </label>
            </div>
          </div>

          <div>
            <label className="text-[11px] font-bold text-gray-400 uppercase tracking-wider">Trạng thái</label>
            <div className="mt-3 space-y-2">
              <label className="flex items-center text-xs cursor-pointer text-gray-600 font-medium">
                <input type="checkbox" className="mr-2 border-gray-300 rounded" defaultChecked /> Hoàn thành
              </label>
              <label className="flex items-center text-xs cursor-pointer text-gray-600 font-medium">
                <input type="checkbox" className="mr-2 border-gray-300 rounded" defaultChecked /> Đang xử lý
              </label>
              <label className="flex items-center text-xs cursor-pointer text-gray-600 font-medium">
                <input type="checkbox" className="mr-2 border-gray-300 rounded" /> Đã hủy
              </label>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <div className="bg-white border-b p-4 flex justify-between items-center shadow-sm">
          <h1 className="text-xl font-bold text-gray-800">Lịch sử hóa đơn</h1>
          <div className="flex space-x-2">
            <button className="bg-white border border-gray-300 text-gray-700 px-4 py-1.5 rounded text-xs font-bold hover:bg-gray-50 transition-colors flex items-center shadow-sm">
               <Download size={14} className="mr-2" /> Xuất file
            </button>
          </div>
        </div>

        {/* Table */}
        <div className="flex-1 overflow-auto p-4 bg-[#f0f2f5]">
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-[11px] font-bold text-gray-500 uppercase">Mã hóa đơn</th>
                  <th className="px-6 py-3 text-left text-[11px] font-bold text-gray-500 uppercase">Thời gian</th>
                  <th className="px-6 py-3 text-left text-[11px] font-bold text-gray-500 uppercase">Phòng/Bàn</th>
                  <th className="px-6 py-3 text-left text-[11px] font-bold text-gray-500 uppercase">Khách hàng</th>
                  <th className="px-6 py-3 text-right text-[11px] font-bold text-gray-500 uppercase">Tổng tiền</th>
                  <th className="px-6 py-3 text-right text-[11px] font-bold text-gray-500 uppercase">Đã trả</th>
                  <th className="px-6 py-3 text-center text-[11px] font-bold text-gray-500 uppercase">Trạng thái</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-100">
                {loading ? (
                  <tr><td colSpan={7} className="py-20 text-center"><Loader2 className="animate-spin inline mr-2 text-blue-600" /> Đang tải dữ liệu...</td></tr>
                ) : filteredOrders.length === 0 ? (
                  <tr><td colSpan={7} className="py-20 text-center text-gray-400 italic text-sm">Chưa có hóa đơn nào phù hợp.</td></tr>
                ) : filteredOrders.map((o) => (
                  <tr key={o.id} className="hover:bg-blue-50/50 cursor-pointer group transition-colors">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-blue-600">{o.invoiceCode}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-xs text-gray-500">
                       {new Date(o.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - {new Date(o.createdAt).toLocaleDateString('vi-VN')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600 font-medium">{o.tableName || 'Mang về'}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">{o.customerName}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-medium text-gray-600">{o.totalAmount.toLocaleString()}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-black text-blue-700">{o.paidAmount.toLocaleString()}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <span className={`px-2.5 py-0.5 text-[11px] rounded-full font-bold shadow-sm ${
                        o.status === 'Hoàn thành' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'
                      }`}>
                        {o.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};

export default InvoiceHistory;
