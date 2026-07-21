import React, { useState } from 'react';
import { Plus, Search, Filter, MoreVertical, Edit2, Trash2 } from 'lucide-react';

const ProductManagement = () => {
  const [products] = useState([
    { id: 1, code: 'SP00001', name: 'Cà phê muối', category: 'Đồ uống', type: 'Món chế biến', price: 25000 },
    { id: 2, code: 'SP00002', name: 'Bạc xỉu', category: 'Đồ uống', type: 'Món chế biến', price: 22000 },
    { id: 3, code: 'SP00003', name: 'Bánh mì thịt', category: 'Đồ ăn', type: 'Món chế biến', price: 30000 },
  ]);

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar Filters */}
      <div className="w-64 bg-white border-r p-4 hidden md:block">
        <h2 className="font-bold text-lg mb-4">Hàng hóa</h2>

        <div className="space-y-6">
          <div>
            <label className="text-sm font-semibold text-gray-600">Loại thực đơn</label>
            <div className="mt-2 space-y-2">
              <label className="flex items-center text-sm"><input type="checkbox" className="mr-2" defaultChecked/> Đồ ăn</label>
              <label className="flex items-center text-sm"><input type="checkbox" className="mr-2" defaultChecked/> Đồ uống</label>
              <label className="flex items-center text-sm"><input type="checkbox" className="mr-2"/> Dịch vụ</label>
            </div>
          </div>

          <div>
            <label className="text-sm font-semibold text-gray-600">Trạng thái</label>
            <div className="mt-2 space-y-2">
              <label className="flex items-center text-sm"><input type="radio" name="status" className="mr-2" defaultChecked/> Đang kinh doanh</label>
              <label className="flex items-center text-sm"><input type="radio" name="status" className="mr-2"/> Ngừng kinh doanh</label>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Top Header/Actions */}
        <div className="bg-white border-b p-4 flex justify-between items-center">
          <div className="relative w-96">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-gray-400" />
            </span>
            <input
              type="text"
              className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-blue-500 sm:text-sm"
              placeholder="Theo mã hoặc tên món"
            />
          </div>
          <div className="flex space-x-2">
            <button className="bg-blue-600 text-white px-4 py-2 rounded-md flex items-center text-sm font-medium hover:bg-blue-700">
              <Plus className="h-4 w-4 mr-2" /> Thêm mới
            </button>
            <button className="border border-gray-300 px-4 py-2 rounded-md text-sm font-medium hover:bg-gray-50">
              Import
            </button>
          </div>
        </div>

        {/* Product Table */}
        <div className="flex-1 overflow-auto p-4">
          <table className="min-w-full bg-white rounded-lg overflow-hidden shadow-sm">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Mã món</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tên món</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Loại thực đơn</th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Giá bán</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {products.map((p) => (
                <tr key={p.id} className="hover:bg-blue-50 transition-colors">
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-blue-600 font-medium">{p.code}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-medium">{p.name}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{p.category}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 text-right font-bold">{p.price.toLocaleString()} đ</td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <div className="flex justify-center space-x-2">
                      <button className="text-gray-400 hover:text-blue-600"><Edit2 className="h-4 w-4"/></button>
                      <button className="text-gray-400 hover:text-red-600"><Trash2 className="h-4 w-4"/></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default ProductManagement;
