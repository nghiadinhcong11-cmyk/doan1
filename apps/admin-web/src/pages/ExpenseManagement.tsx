import React, { useState } from 'react';
import { Plus, Search, Calendar, Filter, DollarSign, Download, Trash2 } from 'lucide-react';

const ExpenseManagement = () => {
  const [expenses] = useState([
    { id: 1, date: '15/07/2026', description: 'Thanh toán tiền điện tháng 6', amount: 1250000, category: 'Điện nước', method: 'Chuyển khoản' },
    { id: 2, date: '17/07/2026', description: 'Nhập bia và nước ngọt', amount: 3500000, category: 'Nhập hàng', method: 'Tiền mặt' },
    { id: 3, date: '18/07/2026', description: 'Trả lương nhân viên tháng 6', amount: 15000000, category: 'Lương', method: 'Chuyển khoản' },
  ]);

  return (
    <div className="flex h-screen bg-gray-100 text-gray-800">
      {/* Sidebar Filters */}
      <div className="w-72 bg-white border-r p-5 hidden md:block overflow-y-auto">
        <h2 className="font-bold text-xl mb-6 text-blue-700">Quản lý chi tiêu</h2>

        <div className="space-y-6">
          <div>
            <label className="text-sm font-bold text-gray-500 uppercase tracking-wider">Thời gian</label>
            <div className="mt-3 space-y-2">
              <select className="w-full p-2 border border-gray-300 rounded-md text-sm">
                <option>Tháng này</option>
                <option>Tháng trước</option>
                <option>Hôm nay</option>
                <option>Lựa chọn khác</option>
              </select>
            </div>
          </div>

          <div>
            <label className="text-sm font-bold text-gray-500 uppercase tracking-wider">Loại chi phí</label>
            <div className="mt-3 space-y-2">
              {['Tất cả', 'Nhập hàng', 'Lương nhân viên', 'Điện nước', 'Mặt bằng', 'Khác'].map(cat => (
                <label key={cat} className="flex items-center text-sm cursor-pointer hover:text-blue-600">
                  <input type="checkbox" className="mr-3 h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
                  {cat}
                </label>
              ))}
            </div>
          </div>

          <div>
            <label className="text-sm font-bold text-gray-500 uppercase tracking-wider">Phương thức</label>
            <div className="mt-3 space-y-2">
              <label className="flex items-center text-sm"><input type="checkbox" className="mr-3 h-4 w-4" /> Tiền mặt</label>
              <label className="flex items-center text-sm"><input type="checkbox" className="mr-3 h-4 w-4" /> Chuyển khoản</label>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header Actions */}
        <div className="bg-white border-b p-4 flex flex-wrap justify-between items-center gap-4">
          <div className="relative flex-1 max-w-md">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-gray-400" />
            </span>
            <input
              type="text"
              className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md bg-gray-50 focus:bg-white focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm transition-all"
              placeholder="Tìm kiếm theo nội dung chi..."
            />
          </div>

          <div className="flex items-center space-x-3">
            <button className="bg-blue-600 text-white px-5 py-2 rounded-md flex items-center text-sm font-bold shadow-sm hover:bg-blue-700 active:transform active:scale-95 transition-all">
              <Plus className="h-4 w-4 mr-2" /> Lập phiếu chi
            </button>
            <button className="bg-white border border-gray-300 text-gray-700 px-4 py-2 rounded-md flex items-center text-sm font-medium hover:bg-gray-50 shadow-sm transition-all">
              <Download className="h-4 w-4 mr-2" /> Xuất file
            </button>
          </div>
        </div>

        {/* Expense List */}
        <div className="flex-1 overflow-auto p-6">
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Ngày chi</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Nội dung</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Loại chi</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">Phương thức</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Số tiền</th>
                  <th className="px-6 py-4 text-center text-xs font-bold text-gray-500 uppercase tracking-wider"></th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {expenses.map((e) => (
                  <tr key={e.id} className="hover:bg-blue-50/50 transition-colors group">
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">{e.date}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">{e.description}</td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="px-3 py-1 text-xs font-medium rounded-full bg-orange-100 text-orange-700">
                        {e.category}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{e.method}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-black text-red-600">
                      {e.amount.toLocaleString()} đ
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <button className="text-gray-300 hover:text-red-600 transition-colors opacity-0 group-hover:opacity-100">
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-gray-50 font-bold">
                <tr>
                  <td colSpan={4} className="px-6 py-4 text-right text-sm">Tổng cộng chi:</td>
                  <td className="px-6 py-4 text-right text-red-600 text-lg">
                    {expenses.reduce((sum, item) => sum + item.amount, 0).toLocaleString()} đ
                  </td>
                  <td></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ExpenseManagement;
