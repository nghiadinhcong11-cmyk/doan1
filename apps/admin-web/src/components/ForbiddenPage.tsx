import React from 'react';
import { ShieldAlert, ArrowLeft, Home } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

const ForbiddenPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="min-h-[calc(100vh-48px)] flex items-center justify-center bg-gray-50 p-6 font-sans">
      <div className="max-w-md w-full bg-white rounded-[3rem] shadow-2xl shadow-red-500/10 p-12 text-center border border-red-50 border-t-8 border-t-red-500 animate-in zoom-in-95 duration-300">
        <div className="w-24 h-24 bg-red-50 rounded-full flex items-center justify-center mx-auto mb-8 text-red-500 shadow-inner">
          <ShieldAlert size={48} strokeWidth={2.5} />
        </div>

        <h1 className="text-3xl font-black text-gray-800 uppercase italic tracking-tighter mb-4">TRUY CẬP BỊ CHẶN</h1>
        <div className="h-1 w-20 bg-red-500 mx-auto rounded-full mb-6"></div>

        <p className="text-gray-500 font-medium mb-10 leading-relaxed text-sm uppercase tracking-wide">
          Bạn không có quyền truy cập chức năng này.<br/>
          Vui lòng liên hệ <span className="text-red-500 font-black">Quản trị viên hệ thống</span> nếu bạn cần cấp quyền.
        </p>

        <div className="space-y-3">
          <button
            onClick={() => navigate(-1)}
            className="w-full py-4 bg-gray-800 text-white rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] flex items-center justify-center hover:bg-gray-900 transition-all active:scale-95 shadow-xl shadow-gray-500/20"
          >
            <ArrowLeft size={16} className="mr-3" /> QUAY LẠI TRANG TRƯỚC
          </button>

          <button
            onClick={() => navigate('/dashboard')}
            className="w-full py-4 bg-white text-gray-400 border-2 border-gray-100 rounded-2xl font-black text-[10px] uppercase tracking-[0.2em] flex items-center justify-center hover:bg-gray-50 transition-all active:scale-95"
          >
            <Home size={16} className="mr-3" /> VỀ BẢNG ĐIỀU KHIỂN
          </button>
        </div>

        <div className="mt-12 pt-8 border-t border-dashed border-gray-100">
           <p className="text-[9px] font-black text-gray-300 uppercase tracking-widest leading-none">Security Enforcement Policy</p>
           <p className="text-[8px] font-bold text-gray-300 uppercase tracking-widest mt-1">Error Code: 403 Forbidden</p>
        </div>
      </div>
    </div>
  );
};

export default ForbiddenPage;
