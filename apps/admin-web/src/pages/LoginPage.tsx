import React, { useState, useEffect } from 'react';
import { Lock, User, Eye, EyeOff, LogIn, Store, ShieldCheck, MapPin, ChevronRight, Loader2 } from 'lucide-react';
import { API_URL } from '../config';

interface Branch {
  id: string;
  name: string;
}

interface LoginPageProps {
  onLogin: (role: 'admin' | 'cashier', fullName: string, branchId?: string) => void;
}

const LoginPage: React.FC<LoginPageProps> = ({ onLogin }) => {
  const [showPassword, setShowPassword] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loginMode, setLoginMode] = useState<'admin' | 'cashier'>('admin');
  const [branches, setBranches] = useState<Branch[]>([]);
  const [selectedBranchId, setSelectedBranchId] = useState('');
  const [loadingBranches, setLoadingBranches] = useState(false);

  useEffect(() => {
    if (loginMode === 'cashier') {
      fetchBranches();
    }
  }, [loginMode]);

  const fetchBranches = async () => {
    try {
      setLoadingBranches(true);
      const response = await fetch(`${API_URL}/api/Branch`);
      const data = await response.json();
      setBranches(data);
      if (data.length > 0) {
        setSelectedBranchId(data[0].id);
      }
    } catch (err) {
      console.error('Error fetching branches:', err);
    } finally {
      setLoadingBranches(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      const response = await fetch(`${API_URL}/api/Auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          username,
          password,
          mode: loginMode
        })
      });

      if (response.ok) {
        const data = await response.json();

        if (loginMode === 'cashier' && !selectedBranchId) {
          setError('Vui lòng chọn chi nhánh làm việc!');
          return;
        }

        onLogin(data.role, data.fullName, selectedBranchId);
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Đăng nhập thất bại');
      }
    } catch (err) {
      setError('Lỗi kết nối đến máy chủ');
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
          <h1 className="text-3xl font-black text-gray-800 tracking-tighter uppercase italic">KiotViet RESTAURANT</h1>
          <p className="text-gray-500 mt-2 text-sm font-medium">Hệ thống quản lý nhà hàng chuyên nghiệp</p>
        </div>

        {/* Mode Selector */}
        <div className="flex bg-white p-1 rounded-xl mb-4 shadow-sm border border-gray-100">
          <button
            onClick={() => setLoginMode('admin')}
            className={`flex-1 flex items-center justify-center py-2.5 rounded-lg text-xs font-bold transition-all ${loginMode === 'admin' ? 'bg-[#0070f4] text-white shadow-md' : 'text-gray-500 hover:bg-gray-50'}`}
          >
            <ShieldCheck size={16} className="mr-2" /> QUẢN TRỊ VIÊN
          </button>
          <button
            onClick={() => setLoginMode('cashier')}
            className={`flex-1 flex items-center justify-center py-2.5 rounded-lg text-xs font-bold transition-all ${loginMode === 'cashier' ? 'bg-[#0070f4] text-white shadow-md' : 'text-gray-500 hover:bg-gray-50'}`}
          >
            <Store size={16} className="mr-2" /> BÁN HÀNG (POS)
          </button>
        </div>

        {/* Login Card */}
        <div className="bg-white rounded-2xl shadow-xl shadow-blue-500/5 p-8 border border-gray-100">
          <h2 className="text-xl font-bold text-gray-800 mb-6 flex items-center">
            <LogIn className="mr-2 text-[#0070f4]" size={20} />
            {loginMode === 'admin' ? 'Đăng nhập Quản trị' : 'Đăng nhập Thu ngân'}
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
                  placeholder={loginMode === 'admin' ? 'Nhập tên đăng nhập (admin)' : 'Nhập tài khoản thu ngân (pos)'}
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

            {loginMode === 'cashier' && (
              <div className="animate-in fade-in slide-in-from-top-2 duration-300">
                <label className="block text-xs font-bold text-gray-500 uppercase mb-2 tracking-wider">Chọn chi nhánh làm việc</label>
                <div className="relative group">
                  <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-gray-400 group-focus-within:text-[#0070f4] transition-colors">
                    <MapPin size={18} />
                  </span>
                  {loadingBranches ? (
                    <div className="w-full pl-10 pr-3 py-3 border border-gray-200 rounded-xl bg-gray-50 flex items-center">
                       <Loader2 size={16} className="animate-spin text-blue-600 mr-2" />
                       <span className="text-xs text-gray-400">Đang tải danh sách cơ sở...</span>
                    </div>
                  ) : (
                    <select
                      value={selectedBranchId}
                      onChange={(e) => setSelectedBranchId(e.target.value)}
                      className="block w-full pl-10 pr-3 py-3 border border-gray-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-500 outline-none transition-all bg-gray-50 focus:bg-white appearance-none font-bold text-gray-700"
                    >
                      {branches.map(b => (
                        <option key={b.id} value={b.id}>{b.name}</option>
                      ))}
                    </select>
                  )}
                </div>
              </div>
            )}

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

export default LoginPage;
