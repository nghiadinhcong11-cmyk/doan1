import React, { useState } from 'react';
import { Search, Plus, User, Phone, Briefcase, MapPin, Calendar, CreditCard, ChevronDown, MoreHorizontal } from 'lucide-react';

const EmployeeManagement = () => {
  const [employees] = useState([
    { id: 1, code: 'NV00001', name: 'Trần Văn Hoàng', phone: '0987654321', position: 'Thu ngân', department: 'Bán hàng', status: 'Đang làm việc' },
    { id: 2, code: 'NV00002', name: 'Nguyễn Thị Mai', phone: '0912345678', position: 'Phục vụ', department: 'Bán hàng', status: 'Đang làm việc' },
  ]);

  return (
    <div className="flex h-screen bg-gray-100">
      {/* Sidebar Filters */}
      <div className="w-64 bg-white border-r p-4 hidden md:block">
        <h2 className="font-bold text-lg mb-6">Nhân viên</h2>

        <div className="space-y-6">
          <div>
            <label className="text-xs font-bold text-gray-400 uppercase">Trạng thái</label>
            <div className="mt-2 space-y-2">
              <label className="flex items-center text-sm cursor-pointer"><input type="radio" name="status" className="mr-2" defaultChecked /> Đang làm việc</label>
              <label className="flex items-center text-sm cursor-pointer"><input type="radio" name="status" className="mr-2" /> Đã nghỉ</label>
            </div>
          </div>

          <div>
            <label className="text-xs font-bold text-gray-400 uppercase">Phòng ban</label>
            <select className="mt-2 w-full p-2 border rounded-md text-sm">
              <option>Chọn phòng ban</option>
              <option>Bán hàng</option>
              <option>Bếp</option>
            </select>
          </div>

          <div>
            <label className="text-xs font-bold text-gray-400 uppercase">Chức danh</label>
            <select className="mt-2 w-full p-2 border rounded-md text-sm">
              <option>Chọn chức danh</option>
              <option>Quản lý</option>
              <option>Thu ngân</option>
              <option>Phục vụ</option>
            </select>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <div className="bg-white border-b p-4 flex justify-between items-center shadow-sm">
          <div className="relative w-96">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-gray-400" />
            </span>
            <input
              type="text"
              className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
              placeholder="Tìm theo mã, tên nhân viên"
            />
          </div>
          <div className="flex space-x-2">
            <button className="bg-blue-600 text-white px-4 py-2 rounded-md flex items-center text-sm font-medium hover:bg-blue-700">
              <Plus className="h-4 w-4 mr-1" /> Nhân viên
            </button>
            <button className="bg-green-600 text-white px-4 py-2 rounded-md flex items-center text-sm font-medium hover:bg-green-700">
              Duyệt yêu cầu
            </button>
          </div>
        </div>

        {/* List & Detail (Split View Mockup) */}
        <div className="flex-1 overflow-hidden flex">
          {/* Table List */}
          <div className="w-full overflow-auto p-4">
            <div className="bg-white rounded-lg shadow border border-gray-200">
              <table className="min-w-full divide-y divide-gray-200 text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left font-bold text-gray-500 uppercase">Mã nhân viên</th>
                    <th className="px-6 py-3 text-left font-bold text-gray-500 uppercase">Tên nhân viên</th>
                    <th className="px-6 py-3 text-left font-bold text-gray-500 uppercase">Số điện thoại</th>
                    <th className="px-6 py-3 text-left font-bold text-gray-500 uppercase">Chức danh</th>
                    <th className="px-6 py-3 text-left font-bold text-gray-500 uppercase">Ghi chú</th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {employees.map((emp) => (
                    <React.Fragment key={emp.id}>
                      <tr className="hover:bg-blue-50 cursor-pointer">
                        <td className="px-6 py-4 font-medium text-blue-600">{emp.code}</td>
                        <td className="px-6 py-4 font-bold">{emp.name}</td>
                        <td className="px-6 py-4">{emp.phone}</td>
                        <td className="px-6 py-4">{emp.position}</td>
                        <td className="px-6 py-4 text-gray-400">---</td>
                      </tr>
                      {/* Expanded Info Mockup for the first item like in the image */}
                      {emp.id === 1 && (
                        <tr>
                          <td colSpan={5} className="bg-gray-50 p-6">
                            <div className="flex">
                              <div className="w-32 h-32 bg-blue-100 rounded-full flex items-center justify-center mr-8 border-4 border-white shadow-sm">
                                <User className="h-16 w-16 text-blue-400" />
                              </div>
                              <div className="flex-1 grid grid-cols-3 gap-y-4 gap-x-8 text-sm">
                                <div>
                                  <p className="text-gray-400">Số điện thoại</p>
                                  <p className="font-medium">{emp.phone}</p>
                                </div>
                                <div>
                                  <p className="text-gray-400">Phòng ban</p>
                                  <p className="font-medium">{emp.department}</p>
                                </div>
                                <div>
                                  <p className="text-gray-400">Chức danh</p>
                                  <p className="font-medium">{emp.position}</p>
                                </div>
                                <div>
                                  <p className="text-gray-400">Số CMND/CCCD</p>
                                  <p className="font-medium">---</p>
                                </div>
                                <div>
                                  <p className="text-gray-400">Ngày sinh</p>
                                  <p className="font-medium">---</p>
                                </div>
                                <div>
                                  <p className="text-gray-400">Giới tính</p>
                                  <p className="font-medium">---</p>
                                </div>
                              </div>
                            </div>
                            <div className="mt-6 flex justify-end space-x-2 border-t pt-4">
                              <button className="bg-blue-600 text-white px-4 py-2 rounded text-sm">Cập nhật</button>
                              <button className="border border-gray-300 px-4 py-2 rounded text-sm">Ngừng làm việc</button>
                            </div>
                          </td>
                        </tr>
                      )}
                    </React.Fragment>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default EmployeeManagement;
