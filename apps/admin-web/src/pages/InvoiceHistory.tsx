import React, { useState } from 'react';
import { Search, Filter, FileText, Download, ChevronRight, Calendar } from 'lucide-react';

const InvoiceHistory = () => {
  const [orders] = useState([
    { id: 1, code: 'HD00001', time: '14:20', customer: 'Khách lẻ', total: 150000, discount: 0, paid: 150000, status: 'Hoàn thành', method: 'Tiền mặt' },
    { id: 2, code: 'HD00002', time: '15:45', customer: 'Anh Tuấn', total: 450000, discount: 50000, paid: 400000, status: 'Hoàn thành', method: 'Chuyển khoản' },
    { id: 3, code: 'HD00003', time: '16:10', customer: 'Khách lẻ', total: 85000, discount: 5000, paid: 80000, status: 'Đang xử lý', method: 'Thẻ' },
  ]);

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar Filters */}
      <div className="w-72 bg-white border-r p-4 hidden md:block overflow-y-auto">
        <h2 className="font-bold text-lg mb-6">Hóa đơn</h2>

        <div className="space-y-6">
          <div>
            <label className="text-xs font-bold text-gray-400 uppercase">Tìm kiếm</label>
            <input type="text" placeholder="Theo mã hóa đơn" className="mt-2 w-full p-2 border rounded-md text-sm" />
            <input type="text" placeholder="Theo mã, tên hàng" className="mt-2 w-full p-2 border rounded-md text-sm" />
          </div>

          <div>
            <label className="text-xs font-bold text-gray-400 uppercase">Thời gian</label>
            <div className="mt-2 space-y-2">
              <label className="flex items-center text-sm cursor-pointer"><input type="radio" name="time" className="mr-2" defaultChecked /> Hôm nay</label>
              <label className="flex items-center text-sm cursor-pointer"><input type="radio" name="time" className="mr-2" /> Lựa chọn khác</label>
            </div>
          </div>

          <div>
            <label className="text-xs font-bold text-gray-400 uppercase">Trạng thái</label>
            <div className="mt-2 space-y-2">
              <label className="flex items-center text-sm cursor-pointer"><input type="checkbox" className="mr-2" defaultChecked /> Đang xử lý</label>
              <label className="flex items-center text-sm cursor-pointer"><input type="checkbox" className="mr-2" defaultChecked /> Hoàn thành</label>
              <label className="flex items-center text-sm cursor-pointer"><input type="checkbox" className="mr-2" /> Đã hủy</label>
            </div>
          </div>

          <div>
            <label className="text-xs font-bold text-gray-400 uppercase">Phương thức</label>
            <div className="mt-2 space-y-2">
              <label className="flex items-center text-sm cursor-pointer"><input type="checkbox" className="mr-2" /> Tiền mặt</label>
              <label className="flex items-center text-sm cursor-pointer"><input type="checkbox" className="mr-2" /> Thẻ / Chuyển khoản</label>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <div className="bg-white border-b p-4 flex justify-between items-center">
          <h1 className="text-xl font-bold">Lịch sử hóa đơn</h1>
          <div className="flex space-x-2">
            <button className="bg-blue-600 text-white px-4 py-2 rounded-md flex items-center text-sm font-medium hover:bg-blue-700 transition-colors">
              <Download className="h-4 w-4 mr-2" /> Xuất file
            </button>
          </div>
        </div>

        {/* Table */}
        <div className="flex-1 overflow-auto p-4">
          <div className="bg-white rounded-lg shadow-sm border border-gray-200">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">Mã hóa đơn</th>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">Thời gian</th>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">Khách hàng</th>
                  <th className="px-6 py-3 text-right text-xs font-bold text-gray-500 uppercase">Tổng tiền</th>
                  <th className="px-6 py-3 text-right text-xs font-bold text-gray-500 uppercase">Giảm giá</th>
                  <th className="px-6 py-3 text-right text-xs font-bold text-gray-500 uppercase">Khách đã trả</th>
                  <th className="px-6 py-3 text-center text-xs font-bold text-gray-500 uppercase">Trạng thái</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {orders.map((o) => (
                  <tr key={o.id} className="hover:bg-gray-50 cursor-pointer group">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-blue-600">{o.code}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{o.time}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{o.customer}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-medium">{o.total.toLocaleString()}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-right text-red-500">{o.discount.toLocaleString()}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-bold text-green-600">{o.paid.toLocaleString()}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <span className={`px-2 py-1 text-xs rounded-full font-medium ${
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
