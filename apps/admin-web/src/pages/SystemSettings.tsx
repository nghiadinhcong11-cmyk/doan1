import React, { useState } from 'react';
import { Search, Filter, Download, History, ChevronRight, Settings, Users, Store, Printer, CreditCard, ShieldCheck, Bell, Database, Lock, Trash2, ShoppingBag, Utensils, ClipboardList, Users2, BarChart3, Receipt, QrCode, Truck, MessageSquare, Info, Smartphone, Eye, Save } from 'lucide-react';

const SystemSettings = ({ setActiveTab }: { setActiveTab: (tab: string) => void }) => {
  const [activeSubTab, setActiveSubTab] = useState('history');

  const auditLogs = [
    { user: 'Đinh Công Nghĩa', action: 'Thông báo bếp', time: '19/07/2026 11:23', detail: 'Báo bếp đơn hàng: 1-3 Mang về, Bao gồm: - SP000001 cà phê muối: 2' },
    { user: 'Đinh Công Nghĩa', action: 'Gọi món', time: '19/07/2026 11:18', detail: 'Cập nhật thông tin đơn (Thêm giá trị thanh toán): 1-1 + Transfer, 25000' },
    { user: 'Đinh Công Nghĩa', action: 'Gọi món', time: '19/07/2026 11:18', detail: 'Cập nhật thông tin đơn (Hủy giá trị thanh toán): 1-1 + Card, 25000' },
    { user: 'Đinh Công Nghĩa', action: 'Gọi món', time: '19/07/2026 11:18', detail: 'Tạo đơn hàng 1-3, bàn Mang về, Bảng giá: bảng giá chung, Người bán: Đinh Công Nghĩa' },
    { user: 'Đinh Công Nghĩa', action: 'Quản lý bảng lương', time: '19/07/2026 11:13', detail: 'Thêm mới bảng lương - Mã BL: BL000001, Tên BL: Bảng lương tháng 7/2026' },
  ];

  const sidebarItems = [
    { group: 'Quản lý', items: [
      { id: 'menu-inventory', icon: <ShoppingBag size={14}/>, label: 'Thực đơn & Kho hàng' },
      { id: 'orders', icon: <ClipboardList size={14}/>, label: 'Đơn hàng' },
      { id: 'table-kitchen', icon: <Utensils size={14}/>, label: 'Bàn/Bếp' },
      { id: 'customers', icon: <Users2 size={14}/>, label: 'Khách hàng' },
      { id: 'reports', icon: <BarChart3 size={14}/>, label: 'Báo cáo' },
    ]},
    { group: 'Thuế & Kế toán', items: [
      { id: 'invoice-out', icon: <Receipt size={14}/>, label: 'Hóa đơn đầu ra' },
      { id: 'print-templates', icon: <Printer size={14}/>, label: 'Mẫu in' },
    ]},
    { group: 'Tiện ích', items: [
      { id: 'qr-payment', icon: <QrCode size={14}/>, label: 'Thanh toán mã QR' },
      { id: 'delivery', icon: <Truck size={14}/>, label: 'Giao hàng' },
      { id: 'zalo', icon: <MessageSquare size={14}/>, label: 'Gói Zalo' },
    ]},
    { group: 'Cửa hàng', items: [
      { id: 'store-info', icon: <Settings size={14}/>, label: 'Thông tin cửa hàng' },
      { id: 'user-mgmt', icon: <Users size={14}/>, label: 'Quản lý người dùng' },
      { id: 'branch-mgmt', icon: <Store size={14}/>, label: 'Quản lý chi nhánh' },
    ]},
    { group: 'Dữ liệu', items: [
      { id: 'lock-book', icon: <Lock size={14}/>, label: 'Khóa sổ' },
      { id: 'history', icon: <History size={14}/>, label: 'Lịch sử thao tác' },
      { id: 'delete-trial', icon: <Trash2 size={14}/>, label: 'Xóa dữ liệu dùng thử' },
    ]},
  ];

  const renderContent = () => {
    switch (activeSubTab) {
      case 'history':
        return (
          <>
            <div className="p-4 border-b bg-white">
              <h3 className="font-bold text-base mb-1">Lịch sử thao tác</h3>
              <p className="text-[11px] text-gray-400">Theo dõi tất cả các thao tác của nhân viên trên hệ thống.</p>
              <div className="mt-4 flex justify-between items-center">
                <div className="flex space-x-2">
                  <button className="flex items-center px-3 py-1.5 border border-gray-200 rounded text-xs font-medium hover:bg-gray-50 text-gray-600 shadow-sm">
                    <Filter size={14} className="mr-2 text-gray-400" /> Bộ lọc
                  </button>
                </div>
                <button className="flex items-center px-3 py-1.5 border border-gray-200 rounded text-xs font-medium hover:bg-gray-50 text-gray-600 shadow-sm">
                  <Download size={14} className="mr-2 text-gray-400" /> Xuất file
                </button>
              </div>
            </div>
            <div className="flex-1 overflow-auto bg-white">
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
                      <td className="px-4 py-3 border-r text-gray-600 font-medium">{log.user}</td>
                      <td className="px-4 py-3 border-r text-gray-600">{log.action}</td>
                      <td className="px-4 py-3 border-r text-gray-600">{log.time}</td>
                      <td className="px-4 py-3 text-gray-500 leading-relaxed">{log.detail}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        );
      case 'store-info':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-2xl mx-auto space-y-8">
                <div className="flex items-center space-x-4 border-b pb-6">
                   <div className="w-20 h-20 bg-blue-50 rounded-full flex items-center justify-center text-blue-500 border border-blue-100">
                      <Store size={40} />
                   </div>
                   <div>
                      <h3 className="text-xl font-bold">Cửa hàng Kiot POS</h3>
                      <p className="text-sm text-gray-400">ID Cửa hàng: 102938</p>
                   </div>
                </div>

                <div className="grid grid-cols-2 gap-6">
                   <div className="space-y-1">
                      <label className="text-[11px] font-bold text-gray-400 uppercase">Tên cửa hàng</label>
                      <input type="text" className="w-full border-b py-2 outline-none focus:border-blue-500 font-medium" defaultValue="Kiot POS - Hệ thống nhà hàng" />
                   </div>
                   <div className="space-y-1">
                      <label className="text-[11px] font-bold text-gray-400 uppercase">Số điện thoại</label>
                      <input type="text" className="w-full border-b py-2 outline-none focus:border-blue-500 font-medium" defaultValue="0987.654.321" />
                   </div>
                   <div className="col-span-2 space-y-1">
                      <label className="text-[11px] font-bold text-gray-400 uppercase">Địa chỉ</label>
                      <input type="text" className="w-full border-b py-2 outline-none focus:border-blue-500 font-medium" defaultValue="123 Đường Số 1, Phường Tân Phong, Quận 7, TP.HCM" />
                   </div>
                   <div className="space-y-1">
                      <label className="text-[11px] font-bold text-gray-400 uppercase">Website</label>
                      <input type="text" className="w-full border-b py-2 outline-none focus:border-blue-500 font-medium" placeholder="https://..." />
                   </div>
                   <div className="space-y-1">
                      <label className="text-[11px] font-bold text-gray-400 uppercase">Lĩnh vực kinh doanh</label>
                      <input type="text" className="w-full border-b py-2 outline-none focus:border-blue-500 font-medium" defaultValue="Nhà hàng & Cafe" disabled />
                   </div>
                </div>

                <div className="pt-6">
                   <button className="bg-blue-600 text-white px-6 py-2 rounded font-bold flex items-center shadow-lg hover:bg-blue-700 transition-all active:scale-95">
                      <Save size={16} className="mr-2" /> Lưu thay đổi
                   </button>
                </div>
             </div>
          </div>
        );
      case 'menu-inventory':
        return (
          <div className="p-8 bg-white flex-1">
             <div className="flex items-center space-x-2 mb-6">
                <ShoppingBag className="text-blue-600" />
                <h3 className="text-lg font-bold">Thiết lập Thực đơn & Kho hàng</h3>
             </div>
             <div className="space-y-6">
                {[
                  { title: 'Cho phép bán hàng khi hết tồn kho', desc: 'Cho phép thu ngân vẫn tạo được đơn hàng ngay cả khi số lượng tồn kho bằng 0.' },
                  { title: 'Quản lý theo bảng giá', desc: 'Sử dụng nhiều bảng giá khác nhau cho từng nhóm khách hàng hoặc thời điểm.' },
                  { title: 'Tự động tính giá vốn', desc: 'Hệ thống tự động tính toán lại giá vốn khi có giao dịch nhập kho.' },
                  { title: 'Quản lý lô, hạn sử dụng', desc: 'Theo dõi hàng hóa theo số lô và ngày hết hạn.' },
                ].map((item, i) => (
                  <div key={i} className="flex items-center justify-between p-4 border rounded-lg hover:bg-gray-50 transition-colors">
                     <div className="max-w-md">
                        <p className="font-bold text-sm text-gray-700">{item.title}</p>
                        <p className="text-xs text-gray-400 mt-0.5">{item.desc}</p>
                     </div>
                     <div className="w-10 h-5 bg-blue-600 rounded-full relative cursor-pointer shadow-inner">
                        <div className="absolute right-0.5 top-0.5 w-4 h-4 bg-white rounded-full shadow"></div>
                     </div>
                  </div>
                ))}
             </div>
          </div>
        );
      default:
        return (
          <div className="flex-1 flex flex-col items-center justify-center bg-white text-gray-400">
             <div className="p-6 bg-gray-50 rounded-full mb-4">
                <Info size={48} className="text-gray-200" />
             </div>
             <p className="text-base font-bold text-gray-600">Tính năng đang được cập nhật</p>
             <p className="text-xs max-w-xs text-center mt-2 leading-relaxed italic">
                Trang thiết lập cho "{sidebarItems.flatMap(g => g.items).find(i => i.id === activeSubTab)?.label}" hiện đang trong quá trình hoàn thiện. Vui lòng quay lại sau!
             </p>
          </div>
        );
    }
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] overflow-hidden text-gray-700">
      {/* Sidebar Settings Menu */}
      <div className="w-64 bg-white border-r flex flex-col shadow-sm">
        <div className="p-4 border-b">
          <h2 className="font-bold text-lg">Thiết lập</h2>
          <div className="relative mt-3">
            <Search className="absolute left-2.5 top-2.5 h-3.5 w-3.5 text-gray-400" />
            <input
              type="text"
              placeholder="Tìm kiếm thiết lập"
              className="w-full pl-8 pr-2 py-1.5 bg-gray-50 border border-gray-100 rounded text-[11px] outline-none focus:ring-1 focus:ring-blue-500 transition-all"
            />
          </div>
        </div>

        <div className="flex-1 overflow-y-auto p-2 space-y-4 custom-scrollbar">
          {sidebarItems.map((group, idx) => (
            <div key={idx}>
              <p className="px-3 text-[10px] font-bold text-gray-400 uppercase tracking-wider mb-1 mt-2">{group.group}</p>
              <div className="space-y-0.5">
                {group.items.map((item, i) => (
                  <button
                    key={i}
                    onClick={() => {
                      if (item.id === 'user-mgmt') setActiveTab('users');
                      else if (item.id === 'branch-mgmt') setActiveTab('branches');
                      else if (item.id === 'print-templates') setActiveTab('print-templates');
                      else setActiveSubTab(item.id);
                    }}
                    className={`w-full flex items-center px-3 py-1.5 text-xs rounded transition-all ${
                      activeSubTab === item.id
                        ? 'bg-blue-600 text-white font-bold shadow-md transform scale-[1.02]'
                        : 'hover:bg-gray-100 text-gray-600'
                    }`}
                  >
                    <span className={`mr-2.5 ${activeSubTab === item.id ? 'text-white' : 'text-gray-400'}`}>{item.icon}</span>
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
        <div className="bg-white rounded-lg shadow-xl border border-gray-200 flex-1 flex flex-col overflow-hidden animate-in fade-in duration-300">
           {renderContent()}
        </div>
      </div>
    </div>
  );
};

export default SystemSettings;
