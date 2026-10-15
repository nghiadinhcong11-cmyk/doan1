import React, { useState } from 'react';
import { ArrowRight } from 'lucide-react';
import { API_URL } from '../config';
import { Button, Feedback, FormField } from '../components/ui';

interface CustomerLoginProps {
  onLogin: (customer: any) => void;
  onGuestEntry: () => void;
}

const CustomerLogin: React.FC<CustomerLoginProps> = ({ onLogin, onGuestEntry }) => {
  const [phoneNumber, setPhoneNumber] = useState('');
  const [fullName, setFullName] = useState('');
  const [password, setPassword] = useState('');
  const [existingCustomer, setExistingCustomer] = useState<any>(null);
  const [loading, setLoading] = useState(false);
  const [step, setStep] = useState(1); // 1: phone, 2: registration, 3: login
  const [error, setError] = useState('');

  const handleCheckPhone = async (e: React.FormEvent) => {
    e.preventDefault();
    if (phoneNumber.length < 10) {
      setError('Số điện thoại cần có ít nhất 10 chữ số.');
      return;
    }

    setLoading(true);
    setError('');
    try {
      const response = await fetch(`${API_URL}/api/Customer/exists/${phoneNumber}`);
      if (response.ok) {
        const data = await response.json();
        if (data.exists) {
          setExistingCustomer(data);
          setStep(3);
        } else {
          setStep(2);
        }
      } else {
        setStep(2);
      }
    } catch (err) {
      console.error(err);
      setError('Không thể kết nối hệ thống. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  const handleCustomerLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!password) {
      setError('Vui lòng nhập mật khẩu.');
      return;
    }

    setLoading(true);
    setError('');
    try {
      const response = await fetch(`${API_URL}/api/Auth/customer-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ phoneNumber, password })
      });
      const data = await response.json().catch(() => null);
      if (!response.ok || !data?.token) {
        setError(data?.message || 'Số điện thoại hoặc mật khẩu không chính xác.');
        return;
      }

      onLogin({ ...existingCustomer, phoneNumber, token: data.token, role: data.role });
    } catch (err) {
      console.error(err);
      setError('Không thể kết nối hệ thống. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password.length < 8) {
      setError('Mật khẩu phải có ít nhất 8 ký tự.');
      return;
    }

    setLoading(true);
    setError('');
    try {
      const response = await fetch(`${API_URL}/api/Customer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ fullName, phoneNumber, password })
      });
      const data = await response.json().catch(() => null);
      if (!response.ok) {
        setError(data?.message || 'Không thể đăng ký. Vui lòng thử lại.');
        return;
      }

      const tokenResponse = await fetch(`${API_URL}/api/Auth/customer-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ phoneNumber, password })
      });
      const tokenData = await tokenResponse.json().catch(() => null);
      if (!tokenResponse.ok || !tokenData?.token) {
        setError(tokenData?.message || 'Không thể đăng nhập sau khi đăng ký.');
        return;
      }

      onLogin({ ...data, token: tokenData.token, role: tokenData.role });
    } catch (err) {
      console.error(err);
      setError('Không thể kết nối hệ thống. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="customer-surface flex min-h-screen flex-col items-center justify-center p-5 sm:p-6">
      <div className="w-full max-w-md">
        <div className="mb-8 text-center sm:mb-10">
          <p className="mb-2 text-[11px] font-black uppercase tracking-[0.25em] text-[#3e8069]">DOAN Restaurant</p>
          <h1 className="text-3xl font-black tracking-tight text-[#22252b]">Đặt món thật ngon</h1>
          <p className="mt-2 text-sm font-medium text-[#85796d]">Đặt món ngon, nhận ưu đãi riêng mỗi ngày</p>
        </div>

        <div className="rounded-[2rem] border border-[#ebe5dc] bg-white p-6 shadow-[0_24px_60px_-30px_rgba(68,46,31,.45)] sm:p-8">
           {step === 1 ? (
             <form onSubmit={handleCheckPhone} className="space-y-6">
                {error && <Feedback tone="error">{error}</Feedback>}
                <FormField
                  label="Số điện thoại của bạn"
                  type="tel"
                  placeholder="09xx xxx xxx"
                  value={phoneNumber}
                  onChange={e => setPhoneNumber(e.target.value.replace(/\D/g, ''))}
                  required
                  maxLength={15}
                  className="py-4 text-lg tracking-wider"
                />
                <Button
                  type="submit"
                  disabled={loading || phoneNumber.length < 10}
                  loading={loading}
                  className="w-full uppercase tracking-widest"
                >
                   TIẾP TỤC <ArrowRight aria-hidden="true" size={20}/>
                </Button>

                <div className="pt-4 text-center">
                   <button
                     type="button"
                     onClick={() => {
                       onGuestEntry();
                       window.history.pushState({}, '', '/');
                       window.dispatchEvent(new PopStateEvent('popstate'));
                     }}
                     disabled={loading}
                     className="min-h-11 rounded-xl px-4 text-[10px] font-black uppercase tracking-widest text-[#85796d] transition-colors hover:bg-orange-50 hover:text-orange-600"
                   >
                      Tiếp tục với tư cách khách vãng lai
                   </button>
                </div>
             </form>
           ) : step === 3 ? (
             <form onSubmit={handleCustomerLogin} className="space-y-6 animate-in slide-in-from-right duration-300">
                {error && <Feedback tone="error">{error}</Feedback>}
                <p className="text-center text-xs font-bold text-blue-600 bg-blue-50 py-2 rounded-xl">Vui lòng nhập mật khẩu để tiếp tục</p>
                <FormField
                  label="Mật khẩu"
                  type="password"
                  placeholder="Mật khẩu tài khoản"
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  required
                  className="px-6 py-4"
                />
                <Button
                  type="submit"
                  disabled={loading}
                  loading={loading}
                  className="w-full uppercase tracking-widest"
                >
                   ĐĂNG NHẬP
                </Button>
             </form>
           ) : (
             <form onSubmit={handleRegister} className="space-y-6 animate-in slide-in-from-right duration-300">
                {error && <Feedback tone="error">{error}</Feedback>}
                <p className="text-center text-xs font-bold text-blue-600 bg-blue-50 py-2 rounded-xl">Chào mừng bạn mới! Vui lòng tạo tài khoản</p>
                <FormField
                  label="Họ và tên"
                  type="text"
                  placeholder="Ví dụ: Nguyễn Văn A"
                  value={fullName}
                  onChange={e => setFullName(e.target.value)}
                  required
                  className="px-6 py-4"
                />
                <FormField
                  label="Mật khẩu"
                  type="password"
                  placeholder="Ít nhất 8 ký tự"
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  required
                  minLength={8}
                  className="px-6 py-4"
                />
                <Button
                  type="submit"
                  disabled={loading}
                  loading={loading}
                  className="w-full uppercase tracking-widest"
                >
                   HOÀN TẤT ĐĂNG KÝ
                </Button>
             </form>
           )}
        </div>

        <p className="text-center mt-10 text-[10px] text-gray-400 font-bold uppercase tracking-widest italic opacity-50">
           © 2026 DOAN Restaurant • Security Secured
        </p>
      </div>
    </div>
  );
};

export default CustomerLogin;
