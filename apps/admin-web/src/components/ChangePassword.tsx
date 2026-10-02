import React, { useState } from 'react';
import { Lock, CheckCircle2 } from 'lucide-react';
import { API_URL } from '../config';
import { Button, Feedback, FormField } from './ui';

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
    if (newPassword.length < 8 || newPassword.length > 128) {
      setError('Mật khẩu mới phải dài từ 8 đến 128 ký tự.');
      return;
    }

    try {
      setLoading(true);
      const response = await fetch(`${API_URL}/api/Auth/change-password`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(localStorage.getItem('adminToken') ? { Authorization: `Bearer ${localStorage.getItem('adminToken')}` } : {})
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
        <FormField label="Mật khẩu hiện tại" type={showPass ? 'text' : 'password'} value={oldPassword} onChange={(e) => setOldPassword(e.target.value)} required maxLength={128} />
        <FormField label="Mật khẩu mới" type={showPass ? 'text' : 'password'} value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required minLength={8} maxLength={128} helperText="Sử dụng từ 8 đến 128 ký tự." />
        <FormField label="Nhập lại mật khẩu mới" type={showPass ? 'text' : 'password'} value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required minLength={8} maxLength={128} />
      </div>

      {error && <Feedback tone="error">{error}</Feedback>}

      <div className="flex items-center justify-between pt-4">
         <Button
           type="button"
           onClick={() => setShowPass(!showPass)}
           variant="ghost"
           size="sm"
           aria-pressed={showPass}
         >
           {showPass ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
         </Button>
         <Button
           type="submit"
           loading={loading}
           size="md"
         >
           Cập nhật ngay
         </Button>
      </div>
    </form>
  );
};

export default ChangePassword;
