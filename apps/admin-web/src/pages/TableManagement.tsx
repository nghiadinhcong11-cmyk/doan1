import React, { useState } from 'react';
import { Plus, Search, Filter, QrCode, MoreVertical, Edit2, Trash2, MapPin } from 'lucide-react';

const TableManagement = () => {
  const [tables] = useState([
    { id: 1, name: 'Bàn 01', area: 'Tầng 1', seats: 4, status: 'Trống', isActive: true },
    { id: 2, name: 'Bàn 02', area: 'Tầng 1', seats: 4, status: 'Có khách', isActive: true },
    { id: 3, name: 'VIP 01', area: 'Tầng 2', seats: 8, status: 'Trống', isActive: true },
    { id: 4, name: 'Sân vườn 01', area: 'Ngoài trời', seats: 2, status: 'Đã đặt', isActive: true },
  ]);

  const [areas] = useState(['Tầng 1', 'Tầng 2', 'Ngoài trời']);

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar Filters */}
      <div className="w-64 bg-white border-r p-4 hidden md:block">
        <h2 className="font-bold text-lg mb-6">Phòng/Bàn</h2>

        <div className="space-y-6">
          <div>
            <div className="flex justify-between items-center mb-2">
              <label className="text-sm font-semibold text-gray-600 uppercase">Khu vực</label>
              <button className="text-xs text-blue-600 font-medium hover:underline">Tạo mới</button>
            </div>
            <div className="space-y-2">
              <label className="flex items-center text-sm cursor-pointer">
                <input type="radio" name="area" className="mr-2" defaultChecked /> Tất cả
              </label>
              {areas.map(area => (
                <label key={area} className="flex items-center text-sm cursor-pointer hover:text-blue-600">
                  <input type="radio" name="area" className="mr-2" /> {area}
                </label>
              ))}
            </div>
          </div>

          <div>
            <label className="text-sm font-semibold text-gray-600 uppercase">Trạng thái</label>
            <div className="mt-2 space-y-2">
              <label className="flex items-center text-sm cursor-pointer"><input type="radio" name="status" className="mr-2" defaultChecked /> Đang hoạt động</label>
              <label className="flex items-center text-sm cursor-pointer"><input type="radio" name="status" className="mr-2" /> Ngừng hoạt động</label>
              <label className="flex items-center text-sm cursor-pointer"><input type="radio" name="status" className="mr-2" /> Tất cả</label>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header Actions */}
        <div className="bg-white border-b p-4 flex justify-between items-center">
          <div className="relative w-80">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-gray-400" />
            </span>
            <input
              type="text"
              className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-1 focus:ring-blue-500"
              placeholder="Theo tên phòng bàn, số ghế"
            />
          </div>
          <div className="flex space-x-2">
            <button className="bg-blue-600 text-white px-4 py-2 rounded-md flex items-center text-sm font-medium hover:bg-blue-700">
              <Plus className="h-4 w-4 mr-1" /> Thêm phòng/bàn
            </button>
            <button className="border border-gray-300 px-3 py-2 rounded-md text-sm hover:bg-gray-50 flex items-center">
              <QrCode className="h-4 w-4 mr-1" /> Tải tất cả mã QR
            </button>
          </div>
        </div>

        {/* Table List */}
        <div className="flex-1 overflow-auto p-4">
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">Tên phòng/bàn</th>
                  <th className="px-6 py-3 text-left text-xs font-bold text-gray-500 uppercase">Khu vực</th>
                  <th className="px-6 py-3 text-center text-xs font-bold text-gray-500 uppercase">Số ghế</th>
                  <th className="px-6 py-3 text-center text-xs font-bold text-gray-500 uppercase">Trạng thái</th>
                  <th className="px-6 py-3 text-center text-xs font-bold text-gray-500 uppercase">Mã QR</th>
                  <th className="px-6 py-3 text-right text-xs font-bold text-gray-500 uppercase">Thao tác</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {tables.map((t) => (
                  <tr key={t.id} className="hover:bg-blue-50/30">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-blue-600">{t.name}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      <div className="flex items-center">
                        <MapPin className="h-3 w-3 mr-1 text-gray-400" />
                        {t.area}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-center font-medium">{t.seats}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <span className={`px-2 py-1 text-xs rounded-full font-medium ${
                        t.status === 'Trống' ? 'bg-green-100 text-green-700' :
                        t.status === 'Có khách' ? 'bg-red-100 text-red-700' : 'bg-orange-100 text-orange-700'
                      }`}>
                        {t.status}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <button className="text-gray-400 hover:text-blue-600"><QrCode className="h-5 w-5 mx-auto"/></button>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                      <div className="flex justify-end space-x-3">
                        <button className="text-gray-400 hover:text-blue-600"><Edit2 className="h-4 w-4"/></button>
                        <button className="text-gray-400 hover:text-red-600"><Trash2 className="h-4 w-4"/></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Empty State Mockup (Nếu không có bàn) */}
          {tables.length === 0 && (
            <div className="mt-20 flex flex-col items-center justify-center text-gray-400">
              <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center mb-4">
                <MapPin className="h-10 w-10 text-gray-300" />
              </div>
              <p className="text-lg font-medium">Bắt đầu quản lý không gian của bạn</p>
              <div className="mt-4 flex space-x-2">
                <button className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm">Tạo phòng bàn</button>
                <button className="bg-blue-100 text-blue-600 px-4 py-2 rounded-md text-sm font-medium">Tạo khu vực</button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default TableManagement;
