import React, { useState } from 'react';
import { Lock, User, Eye, EyeOff, LogIn } from 'lucide-react';

interface LoginPageProps {
  onLogin: () => void;
}

const LoginPage: React.FC<LoginPageProps> = ({ onLogin }) => {
  const [showPassword, setShowPassword] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // Giả lập đăng nhập đơn giản
    if (username === 'admin' && password === '123456') {
      onLogin();
    } else {
      setError('Tên đăng nhập hoặc mật khẩu không đúng!');
    }
  };

  return (
    <div className="min-h-screen bg-[#f0f2f5] flex items-center justify-center p-4 font-sans">
      <div className="max-w-md w-full">
        {/* Logo Section */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-[#0070f4] rounded-2xl shadow-lg mb-4 rotate-3 transform transition-transform hover:rotate-0 cursor-default">
            <span className="text-white text-3xl font-black italic">K</span>
          </div>
          <h1 className="text-3xl font-black text-gray-800 tracking-tighter uppercase italic">KiotViet Admin</h1>
          <p className="text-gray-500 mt-2 text-sm font-medium">Hệ thống quản lý nhà hàng chuyên nghiệp</p>
        </div>

        {/* Login Card */}
        <div className="bg-white rounded-2xl shadow-xl shadow-blue-500/5 p-8 border border-gray-100">
          <h2 className="text-xl font-bold text-gray-800 mb-6 flex items-center">
            <LogIn className="mr-2 text-[#0070f4]" size={20} /> Đăng nhập hệ thống
          </h2>

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2 tracking-wider">Tên đăng nhập</label>
              <div className="relative group">
                <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-gray-400 group-focus-within:text-[#0070f4] transition-colors">
                  <User size={18} />
                </span>
                <input
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  className="block w-full pl-10 pr-3 py-3 border border-gray-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all bg-gray-50 focus:bg-white"
                  placeholder="Nhập tên đăng nhập (admin)"
                  required
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-gray-500 uppercase mb-2 tracking-wider">Mật khẩu</label>
              <div className="relative group">
                <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-gray-400 group-focus-within:text-[#0070f4] transition-colors">
                  <Lock size={18} />
                </span>
                <input
                  type={showPassword ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="block w-full pl-10 pr-10 py-3 border border-gray-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all bg-gray-50 focus:bg-white"
                  placeholder="Nhập mật khẩu (123456)"
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors"
                >
                  {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                </button>
              </div>
            </div>

            {error && (
              <div className="bg-red-50 text-red-600 text-xs p-3 rounded-lg font-bold flex items-center animate-shake">
                <span className="mr-2 italic">⚠️</span> {error}
              </div>
            )}

            <div className="flex items-center justify-between">
              <label className="flex items-center text-xs text-gray-500 cursor-pointer">
                <input type="checkbox" className="mr-2 rounded border-gray-300 text-[#0070f4] focus:ring-blue-500" />
                Ghi nhớ đăng nhập
              </label>
              <a href="#" className="text-xs text-[#0070f4] font-bold hover:underline">Quên mật khẩu?</a>
            </div>

            <button
              type="submit"
              className="w-full bg-[#0070f4] text-white py-3 rounded-xl font-bold shadow-lg shadow-blue-500/30 hover:bg-blue-700 active:scale-95 transition-all flex items-center justify-center space-x-2"
            >
              <span>VÀO HỆ THỐNG</span>
              <ChevronRight size={18} />
            </button>
          </form>
        </div>

        {/* Footer info */}
        <p className="text-center mt-8 text-xs text-gray-400">
          © 2026 KiotViet Restaurant POS. All rights reserved.
        </p>
      </div>
    </div>
  );
};

// Internal icon to avoid import issues
const ChevronRight = ({size}: {size: number}) => (
  <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round"><path d="m9 18 6-6-6-6"/></svg>
);

export default LoginPage;
