import React, { useState } from 'react';
import { Lock, Eye, EyeOff, Loader2, CheckCircle2, ShieldCheck, User } from 'lucide-react';
import { API_URL } from '../config';

const ChangePassword = ({ id, type, onClose }: { id: string, type: 'Employee' | 'Customer', onClose: () => void }) => {
  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (newPassword !== confirmPassword) {
      setError('Mật khẩu mới không khớp!');
      return;
    }

    try {
      setLoading(true);
      const response = await fetch(`${API_URL}/api/Auth/change-password`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(localStorage.getItem('token') ? { Authorization: `Bearer ${localStorage.getItem('token')}` } : {})
        },
        body: JSON.stringify({ id, type, oldPassword, newPassword })
      });

      const data = await response.json();
      if (response.ok) {
        setSuccess(true);
        setTimeout(() => onClose(), 2000);
      } else {
        setError(data.message || 'Đổi mật khẩu thất bại');
      }
    } catch (err) {
      setError('Lỗi kết nối đến máy chủ');
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="p-8 text-center">
        <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <CheckCircle2 className="text-green-600" size={32} />
        </div>
        <h3 className="font-black text-gray-800 uppercase">Thành công!</h3>
        <p className="text-xs text-gray-500 mt-1">Mật khẩu đã được cập nhật.</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="p-6 space-y-5">
      <div className="flex items-center space-x-3 mb-6">
         <div className="p-3 bg-blue-50 rounded-2xl text-blue-600"><Lock size={20}/></div>
         <div>
            <h3 className="font-black text-gray-800 uppercase italic tracking-tighter">Đổi mật khẩu</h3>
            <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest">Vui lòng bảo mật tài khoản của bạn</p>
         </div>
      </div>

      <div className="space-y-4">
        <div>
           <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1.5 ml-1">Mật khẩu hiện tại</label>
           <div className="relative">
              <input 
                type={showPass ? 'text' : 'password'}
                className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/10 outline-none"
                value={oldPassword}
                onChange={(e) => setOldPassword(e.target.value)}
                required
              />
           </div>
        </div>

        <div>
           <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1.5 ml-1">Mật khẩu mới</label>
           <input 
              type={showPass ? 'text' : 'password'}
              className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/10 outline-none"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
           />
        </div>

        <div>
           <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1.5 ml-1">Nhập lại mật khẩu mới</label>
           <input 
              type={showPass ? 'text' : 'password'}
              className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-2xl text-sm font-bold focus:ring-2 focus:ring-blue-500/10 outline-none"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
           />
        </div>
      </div>

      {error && <p className="text-[10px] font-black text-red-500 uppercase text-center">{error}</p>}

      <div className="flex items-center justify-between pt-4">
         <button 
           type="button"
           onClick={() => setShowPass(!showPass)}
           className="text-[10px] font-black text-gray-400 uppercase hover:text-blue-600 transition-colors"
         >
           {showPass ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
         </button>
         <button 
           type="submit"
           disabled={loading}
           className="bg-blue-600 text-white px-8 py-3 rounded-2xl font-black text-xs uppercase tracking-widest shadow-lg shadow-blue-500/20 active:scale-95 transition-all flex items-center"
         >
           {loading ? <Loader2 className="animate-spin mr-2" size={16} /> : 'Cập nhật ngay'}
         </button>
      </div>
    </form>
  );
};

export default ChangePassword;
