import React from 'react';
import { Search, Filter, Download, History, ChevronRight, Settings, Users, Store, Printer, CreditCard, ShieldCheck, Bell, Database, Lock, Trash2, ShoppingBag, Utensils, ClipboardList, Users2, BarChart3, Receipt, QrCode, Truck, MessageSquare } from 'lucide-react';

const SystemSettings = () => {
  const auditLogs = [
    { user: 'Đinh Công Nghĩa', action: 'Thông báo bếp', time: '19/07/2026 11:23', detail: 'Báo bếp đơn hàng: 1-3 Mang về, Bao gồm: - SP000001 cà phê muối: 2' },
    { user: 'Đinh Công Nghĩa', action: 'Gọi món', time: '19/07/2026 11:18', detail: 'Cập nhật thông tin đơn (Thêm giá trị thanh toán): 1-1 + Transfer, 25000' },
    { user: 'Đinh Công Nghĩa', action: 'Gọi món', time: '19/07/2026 11:18', detail: 'Cập nhật thông tin đơn (Hủy giá trị thanh toán): 1-1 + Card, 25000' },
    { user: 'Đinh Công Nghĩa', action: 'Gọi món', time: '19/07/2026 11:18', detail: 'Tạo đơn hàng 1-3, bàn Mang về, Bảng giá: bảng giá chung, Người bán: Đinh Công Nghĩa' },
    { user: 'Đinh Công Nghĩa', action: 'Quản lý bảng lương', time: '19/07/2026 11:13', detail: 'Thêm mới bảng lương - Mã BL: BL000001, Tên BL: Bảng lương tháng 7/2026' },
  ];

  const sidebarItems = [
    { group: 'Quản lý', items: [
      { icon: <ShoppingBag size={14}/>, label: 'Thực đơn & Kho hàng' },
      { icon: <ClipboardList size={14}/>, label: 'Đơn hàng' },
      { icon: <Utensils size={14}/>, label: 'Bàn/Bếp' },
      { icon: <Users2 size={14}/>, label: 'Khách hàng' },
      { icon: <BarChart3 size={14}/>, label: 'Báo cáo' },
    ]},
    { group: 'Thuế & Kế toán', items: [
      { icon: <Receipt size={14}/>, label: 'Hóa đơn đầu ra' },
      { icon: <Printer size={14}/>, label: 'Mẫu in' },
    ]},
    { group: 'Tiện ích', items: [
      { icon: <QrCode size={14}/>, label: 'Thanh toán mã QR' },
      { icon: <Truck size={14}/>, label: 'Giao hàng' },
      { icon: <MessageSquare size={14}/>, label: 'Gói Zalo' },
    ]},
    { group: 'Cửa hàng', items: [
      { icon: <Settings size={14}/>, label: 'Thông tin cửa hàng' },
      { icon: <Users size={14}/>, label: 'Quản lý người dùng' },
      { icon: <Store size={14}/>, label: 'Quản lý chi nhánh' },
    ]},
    { group: 'Dữ liệu', items: [
      { icon: <Lock size={14}/>, label: 'Khóa sổ' },
      { icon: <History size={14}/>, label: 'Lịch sử thao tác', active: true },
      { icon: <Trash2 size={14}/>, label: 'Xóa dữ liệu dùng thử' },
    ]},
  ];

  return (
    <div className="flex h-[calc(100vh-48px)] bg-gray-50 overflow-hidden text-gray-700">
      {/* Sidebar Settings Menu */}
      <div className="w-64 bg-white border-r flex flex-col">
        <div className="p-4 border-b">
          <h2 className="font-bold text-lg">Thiết lập</h2>
          <div className="relative mt-3">
            <Search className="absolute left-2 top-2.5 h-3.5 w-3.5 text-gray-400" />
            <input
              type="text"
              placeholder="Tìm kiếm thiết lập"
              className="w-full pl-8 pr-2 py-1.5 bg-gray-50 border border-gray-200 rounded text-xs outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>
        </div>

        <div className="flex-1 overflow-y-auto p-2 space-y-4">
          {sidebarItems.map((group, idx) => (
            <div key={idx}>
              <p className="px-3 text-[10px] font-bold text-gray-400 uppercase tracking-wider mb-1">{group.group}</p>
              <div className="space-y-0.5">
                {group.items.map((item, i) => (
                  <button
                    key={i}
                    className={`w-full flex items-center px-3 py-1.5 text-xs rounded transition-colors ${
                      item.active ? 'bg-blue-50 text-blue-700 font-bold' : 'hover:bg-gray-100 text-gray-600'
                    }`}
                  >
                    <span className={`mr-2 ${item.active ? 'text-blue-600' : 'text-gray-400'}`}>{item.icon}</span>
                    {item.label}
                  </button>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Content Area */}
      <div className="flex-1 overflow-hidden flex flex-col p-4">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 flex-1 flex flex-col overflow-hidden">
          {/* Header */}
          <div className="p-4 border-b">
             <h3 className="font-bold text-base mb-1">Lịch sử thao tác</h3>
             <p className="text-[11px] text-gray-400">Theo dõi tất cả các thao tác của nhân viên trên KiotViet.</p>

             <div className="mt-4 flex justify-between items-center">
                <div className="flex space-x-2">
                   <button className="flex items-center px-3 py-1.5 border border-gray-200 rounded text-xs font-medium hover:bg-gray-50">
                      <Filter size={14} className="mr-2 text-gray-400" /> Lọc
                   </button>
                </div>
                <div className="flex space-x-2">
                   <button className="flex items-center px-3 py-1.5 border border-gray-200 rounded text-xs font-medium hover:bg-gray-50 text-gray-600">
                      <Download size={14} className="mr-2 text-gray-400" /> Xuất file
                   </button>
                </div>
             </div>
          </div>

          {/* Table */}
          <div className="flex-1 overflow-auto">
             <table className="w-full text-xs text-left border-collapse">
                <thead className="bg-gray-50 text-gray-500 sticky top-0 z-10 border-b">
                   <tr>
                      <th className="px-4 py-3 font-medium border-r w-32">Nhân viên</th>
                      <th className="px-4 py-3 font-medium border-r w-32">Tính năng</th>
                      <th className="px-4 py-3 font-medium border-r w-40">Thời gian</th>
                      <th className="px-4 py-3 font-medium">Chi tiết</th>
                   </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                   {auditLogs.map((log, i) => (
                      <tr key={i} className="hover:bg-blue-50/30 transition-colors">
                         <td className="px-4 py-3 border-r text-gray-600">{log.user}</td>
                         <td className="px-4 py-3 border-r text-gray-600">{log.action}</td>
                         <td className="px-4 py-3 border-r text-gray-600">{log.time}</td>
                         <td className="px-4 py-3 text-gray-500 leading-relaxed">{log.detail}</td>
                      </tr>
                   ))}
                   {/* Dữ liệu trống placeholder */}
                   {[1,2,3,4,5,6,7,8].map(i => (
                     <tr key={`empty-${i}`} className="border-b border-transparent">
                        <td className="px-4 py-3 border-r">&nbsp;</td>
                        <td className="px-4 py-3 border-r">&nbsp;</td>
                        <td className="px-4 py-3 border-r">&nbsp;</td>
                        <td className="px-4 py-3">&nbsp;</td>
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

export default SystemSettings;
