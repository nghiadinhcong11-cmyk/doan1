import React from 'react';
import { Settings, Printer, Users, Store, CreditCard, Bell, ShieldCheck, ChevronRight } from 'lucide-react';

const SettingsDropdown = () => {
  return (
    <div className="fixed top-12 right-12 w-80 bg-white rounded-lg shadow-2xl border border-gray-200 z-50 overflow-hidden animate-in fade-in zoom-in duration-200">
      <div className="p-4 border-b bg-gray-50">
        <h3 className="text-sm font-bold text-gray-700 flex items-center">
          <Settings className="h-4 w-4 mr-2 text-blue-600" /> Thiết lập hệ thống
        </h3>
      </div>

      <div className="p-2">
        <div className="grid grid-cols-1 gap-1">
          <button className="flex items-center justify-between px-3 py-2.5 hover:bg-blue-50 rounded-md group transition-colors">
            <div className="flex items-center text-sm text-gray-600 group-hover:text-blue-700 font-medium">
              <Settings className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" />
              Thiết lập tính năng
            </div>
            <ChevronRight className="h-4 w-4 text-gray-300 group-hover:text-blue-400" />
          </button>

          <button className="flex items-center justify-between px-3 py-2.5 hover:bg-blue-50 rounded-md group transition-colors">
            <div className="flex items-center text-sm text-gray-600 group-hover:text-blue-700 font-medium">
              <Printer className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" />
              Quản lý mẫu in
            </div>
            <ChevronRight className="h-4 w-4 text-gray-300 group-hover:text-blue-400" />
          </button>

          <button className="flex items-center justify-between px-3 py-2.5 hover:bg-blue-50 rounded-md group transition-colors">
            <div className="flex items-center text-sm text-gray-600 group-hover:text-blue-700 font-medium">
              <Store className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" />
              Quản lý chi nhánh
            </div>
            <ChevronRight className="h-4 w-4 text-gray-300 group-hover:text-blue-400" />
          </button>

          <button className="flex items-center justify-between px-3 py-2.5 hover:bg-blue-50 rounded-md group transition-colors">
            <div className="flex items-center text-sm text-gray-600 group-hover:text-blue-700 font-medium">
              <Users className="h-4 w-4 mr-3 text-gray-400 group-hover:text-blue-500" />
              Quản lý người dùng
            </div>
            <ChevronRight className="h-4 w-4 text-gray-300 group-hover:text-blue-400" />
          </button>
        </div>

        <div className="mt-2 pt-2 border-t">
          <p className="px-3 py-1 text-[10px] font-bold text-gray-400 uppercase tracking-widest">Tiện ích</p>

          <button className="w-full flex items-center px-3 py-2.5 hover:bg-gray-50 text-sm text-gray-600 rounded-md transition-colors">
            <Bell className="h-4 w-4 mr-3 text-gray-400" /> Thông báo hệ thống
          </button>

          <button className="w-full flex items-center px-3 py-2.5 hover:bg-gray-50 text-sm text-gray-600 rounded-md transition-colors">
            <ShieldCheck className="h-4 w-4 mr-3 text-gray-400" /> Bảo mật & Phân quyền
          </button>

          <button className="w-full flex items-center px-3 py-2.5 hover:bg-gray-50 text-sm text-gray-600 rounded-md transition-colors">
            <CreditCard className="h-4 w-4 mr-3 text-gray-400" /> Lịch sử thanh toán gói
          </button>
        </div>
      </div>

      <div className="p-3 bg-blue-50 text-center">
        <button className="text-xs font-bold text-blue-700 hover:underline">
          Xem tất cả thiết lập
        </button>
      </div>
    </div>
  );
};

export default SettingsDropdown;
