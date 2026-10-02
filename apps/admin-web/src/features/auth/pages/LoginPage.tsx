import React, { useState } from 'react';
import { Eye, EyeOff, LogIn, ShieldCheck, Users, ChevronRight } from 'lucide-react';
import { API_URL } from '../../../config';
import { Button, Feedback, FormField } from '../../../components/ui';

type EmployeeSystemRole = 'admin' | 'manager' | 'employee' | 'cashier' | 'kitchen';
type LoginGroup = 'management' | 'staff';

interface LoginPageProps {
  onLogin: (role: EmployeeSystemRole, fullName: string, branchId?: string, employeeId?: string, branchName?: string, position?: string, token?: string) => void;
}

const LoginPage: React.FC<LoginPageProps> = ({ onLogin }) => {
  const [showPassword, setShowPassword] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [loginGroup, setLoginGroup] = useState<LoginGroup>('management');

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError('');
    setLoading(true);
    try {
      const response = await fetch(`${API_URL}/api/Auth/login`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password, mode: loginGroup })
      });
      const data = await response.json();
      if (!response.ok) {
        setError(data.message || 'Đăng nhập thất bại');
        return;
      }
      onLogin(data.role as EmployeeSystemRole, data.fullName, data.branchId, data.employeeId, data.branchName, data.position, data.token);
    } catch {
      setError('Lỗi kết nối đến máy chủ');
    } finally {
      setLoading(false);
    }
  };

  const isManagement = loginGroup === 'management';
  return (
    <div className="min-h-screen bg-[#f8f9fa] flex items-center justify-center p-4 font-sans">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-[#0070f4] rounded-2xl shadow-lg mb-4 rotate-3 transform transition-transform hover:rotate-0 cursor-default"><span className="text-white text-3xl font-black italic">D</span></div>
          <h1 className="text-3xl font-black text-gray-800 tracking-tighter uppercase italic">DOAN RESTAURANT</h1>
          <p className="text-gray-500 mt-2 text-sm font-medium">Hệ thống quản lý nhà hàng</p>
        </div>
        <div className="flex bg-white p-1 rounded-xl mb-4 shadow-sm border border-gray-100" role="tablist" aria-label="Loại đăng nhập">
          <button type="button" onClick={() => setLoginGroup('management')} role="tab" aria-selected={isManagement} className={`flex-1 flex items-center justify-center py-2.5 rounded-lg text-[10px] font-bold transition-all ${isManagement ? 'bg-[#0070f4] text-white shadow-md' : 'text-gray-500 hover:bg-gray-50'}`}><ShieldCheck size={14} className="mr-1" /> ĐĂNG NHẬP QUẢN LÝ</button>
          <button type="button" onClick={() => setLoginGroup('staff')} role="tab" aria-selected={!isManagement} className={`flex-1 flex items-center justify-center py-2.5 rounded-lg text-[10px] font-bold transition-all ${!isManagement ? 'bg-[#0070f4] text-white shadow-md' : 'text-gray-500 hover:bg-gray-50'}`}><Users size={14} className="mr-1" /> ĐĂNG NHẬP NHÂN VIÊN</button>
        </div>
        <div className="bg-white rounded-2xl shadow-xl shadow-blue-500/5 p-8 border border-gray-100">
          <h2 className="text-xl font-bold text-gray-800 mb-6 flex items-center"><LogIn className="mr-2 text-[#0070f4]" size={20} />{isManagement ? 'Đăng nhập Quản lý' : 'Đăng nhập Nhân viên'}</h2>
          <form onSubmit={handleSubmit} className="space-y-5">
            <FormField label="Tên đăng nhập" required type="text" value={username} onChange={(event) => setUsername(event.target.value)} placeholder={isManagement ? 'Tài khoản quản lý' : 'Tài khoản nhân viên'} />
            <div className="relative">
              <FormField label="Mật khẩu" required type={showPassword ? 'text' : 'password'} value={password} onChange={(event) => setPassword(event.target.value)} maxLength={128} placeholder="Nhập mật khẩu" className="pr-10" />
              <button type="button" onClick={() => setShowPassword(!showPassword)} aria-label={showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'} className="absolute right-2 top-7 rounded-md p-2 text-gray-400 hover:text-gray-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 transition-colors">{showPassword ? <EyeOff size={18} /> : <Eye size={18} />}</button>
            </div>
            {error && <Feedback tone="error">{error}</Feedback>}
            <Button type="submit" loading={loading} className="w-full"><span>{loading ? 'ĐANG ĐĂNG NHẬP...' : 'VÀO HỆ THỐNG'}</span><ChevronRight size={18} /></Button>
          </form>
        </div>
        <p className="text-center mt-8 text-xs text-gray-400">© 2026 DOAN Restaurant POS. All rights reserved.</p>
      </div>
    </div>
  );
};

export default LoginPage;
