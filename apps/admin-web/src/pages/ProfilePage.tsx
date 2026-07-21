import React from 'react';
import { User, Store, Package, History, Shield, Settings, LogOut, ChevronRight, Moon, Globe } from 'lucide-react';

const ProfilePage = () => {
  return (
    <div className="flex h-screen bg-gray-50 overflow-hidden">
      {/* Sidebar Menu */}
      <div className="w-72 bg-white border-r p-6 overflow-y-auto">
        <h2 className="font-bold text-lg mb-6">Cài đặt</h2>

        <div className="space-y-1">
          <p className="text-xs font-bold text-gray-400 uppercase mb-2 px-3">Gian hàng</p>
          <button className="w-full flex items-center px-3 py-2 bg-blue-50 text-blue-600 rounded-md font-medium">
            <Shield className="h-4 w-4 mr-3" /> Thông tin gian hàng
          </button>

          <p className="text-xs font-bold text-gray-400 uppercase mt-6 mb-2 px-3">Gói dịch vụ KiotViet</p>
          <button className="w-full flex items-center px-3 py-2 text-gray-600 hover:bg-gray-100 rounded-md transition-colors">
            <Package className="h-4 w-4 mr-3" /> Gói dịch vụ
          </button>
          <button className="w-full flex items-center px-3 py-2 text-gray-600 hover:bg-gray-100 rounded-md transition-colors">
            <History className="h-4 w-4 mr-3" /> Lịch sử mua hàng
          </button>

          <p className="text-xs font-bold text-gray-400 uppercase mt-6 mb-2 px-3">Khác</p>
          <button className="w-full flex items-center px-3 py-2 text-gray-600 hover:bg-gray-100 rounded-md transition-colors">
            <Settings className="h-4 w-4 mr-3" /> Điều khoản sử dụng
          </button>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 overflow-auto p-8">
        <div className="max-w-4xl bg-white rounded-xl shadow-sm border border-gray-200">
          <div className="border-b px-8 py-4 flex space-x-8">
            <button className="border-b-2 border-blue-600 pb-4 text-blue-600 font-bold text-sm">Thông tin chung</button>
            <button className="pb-4 text-gray-500 font-medium text-sm hover:text-gray-700">Lịch sử cập nhật</button>
          </div>

          <div className="p-8 space-y-10">
            {/* Profile Info Section */}
            <section>
              <h3 className="text-sm font-bold text-gray-800 mb-6 uppercase tracking-wider">Thông tin hồ sơ</h3>
              <div className="grid grid-cols-2 gap-x-12 gap-y-6">
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Loại hình kinh doanh</label>
                  <p className="text-sm font-medium border-b pb-1">Cá nhân</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Ngày sinh</label>
                  <p className="text-sm font-medium border-b pb-1 text-gray-300">---</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Điện thoại</label>
                  <p className="text-sm font-medium border-b pb-1">+84949774303</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Người đại diện</label>
                  <p className="text-sm font-medium border-b pb-1">Đinh Công Nghĩa</p>
                </div>
                <div className="col-span-2">
                  <label className="block text-xs text-gray-400 mb-1">Địa chỉ</label>
                  <p className="text-sm font-medium border-b pb-1 text-gray-300">Chưa có thông tin</p>
                </div>
              </div>
            </section>

            {/* Store Info Section */}
            <section>
              <h3 className="text-sm font-bold text-gray-800 mb-6 uppercase tracking-wider">Thông tin gian hàng</h3>
              <div className="grid grid-cols-2 gap-x-12 gap-y-6">
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Tên gian hàng</label>
                  <p className="text-sm font-medium border-b pb-1">kabo</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Ngày hết hạn</label>
                  <p className="text-sm font-medium border-b pb-1">20/07/2026</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Ngành hàng</label>
                  <p className="text-sm font-medium border-b pb-1">Bar, Coffee & Restaurant</p>
                </div>
                <div>
                  <label className="block text-xs text-gray-400 mb-1">Gói dịch vụ</label>
                  <p className="text-sm font-medium border-b pb-1 text-blue-600">Gói dùng thử</p>
                </div>
              </div>
            </section>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;
