import React, { useState } from 'react';
import { Phone, ArrowRight, Loader2, Star } from 'lucide-react';
import { API_URL } from '../config';

interface CustomerLoginProps {
  onLogin: (customer: any) => void;
}

const CustomerLogin: React.FC<CustomerLoginProps> = ({ onLogin }) => {
  const [phoneNumber, setPhoneNumber] = useState('');
  const [fullName, setFullName] = useState('');
  const [loading, setLoading] = useState(false);
  const [step, setStep] = useState(1); // 1: Phone, 2: Name (if new)
  const [error, setError] = useState('');

  const continueAsGuest = async () => {
    setLoading(true);
    setError('');
    try {
      const response = await fetch(`${API_URL}/api/Auth/customer-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ phoneNumber: '', fullName: 'Khách vãng lai' })
      });
      if (!response.ok) throw new Error('Guest token request failed.');
      const data = await response.json();
      onLogin({ fullName: 'Khách vãng lai', phoneNumber: '', isGuest: true, token: data.token });
    } catch (err) {
      console.error(err);
      setError('Không thể kết nối hệ thống. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  const handleCheckPhone = async (e: React.FormEvent) => {
    e.preventDefault();
    if (phoneNumber.length < 10) return;

    setLoading(true);
    setError('');
    try {
      const response = await fetch(`${API_URL}/api/Customer/exists/${phoneNumber}`);
      if (response.ok) {
        const data = await response.json();
        if (data.exists) {
          const tokenResponse = await fetch(`${API_URL}/api/Auth/customer-token`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ phoneNumber })
          });
          const tokenData = tokenResponse.ok ? await tokenResponse.json() : {};
          onLogin({ ...data, phoneNumber, token: tokenData.token });
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

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const response = await fetch(`${API_URL}/api/Customer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ fullName, phoneNumber, totalSpending: 0 })
      });
      if (response.ok) {
        const data = await response.json();
        const tokenResponse = await fetch(`${API_URL}/api/Auth/customer-token`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ phoneNumber, fullName })
        });
        const tokenData = tokenResponse.ok ? await tokenResponse.json() : {};
        onLogin({ ...data, token: tokenData.token });
      } else {
        const data = await response.json().catch(() => null);
        setError(data?.message || 'Không thể đăng ký. Vui lòng thử lại.');
      }
    } catch (err) {
      console.error(err);
      setError('Không thể kết nối hệ thống. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-6">
      <div className="w-full max-w-sm">
        <div className="text-center mb-10">
          <div className="inline-flex items-center justify-center w-20 h-20 bg-blue-600 rounded-[2.5rem] shadow-xl mb-6 rotate-3">
             <Star className="text-white fill-white" size={40}/>
          </div>
          <h1 className="text-3xl font-black text-gray-800 uppercase italic tracking-tighter">DOAN Member</h1>
          <p className="text-gray-400 text-sm font-medium mt-2">Tích điểm đổi quà - Nhận ngàn ưu đãi</p>
        </div>

        <div className="bg-white p-8 rounded-[3rem] shadow-2xl shadow-blue-500/10 border border-gray-100">
           {step === 1 ? (
             <form onSubmit={handleCheckPhone} className="space-y-6">
                {error && <p role="alert" className="rounded-xl bg-red-50 px-4 py-3 text-center text-xs font-bold text-red-600">{error}</p>}
                <div>
                   <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3 ml-2">Số điện thoại của bạn</label>
                   <div className="relative">
                      <Phone className="absolute left-4 top-4 text-gray-400" size={20}/>
                      <input
                        type="tel"
                        className="w-full pl-12 pr-4 py-4 bg-gray-50 border-none rounded-2xl outline-none focus:ring-2 focus:ring-blue-500/20 font-black text-lg text-gray-700 tracking-wider"
                        placeholder="09xx xxx xxx"
                        value={phoneNumber}
                        onChange={e => setPhoneNumber(e.target.value.replace(/\D/g, ''))}
                        required
                      />
                   </div>
                </div>
                <button
                  disabled={loading || phoneNumber.length < 10}
                  className="w-full py-4 bg-blue-600 text-white rounded-2xl font-black uppercase tracking-widest shadow-lg shadow-blue-600/30 hover:bg-blue-700 transition-all flex items-center justify-center disabled:opacity-50"
                >
                   {loading ? <Loader2 className="animate-spin" /> : <>TIẾP TỤC <ArrowRight size={20} className="ml-2"/></>}
                </button>

                <div className="pt-4 text-center">
                   <button
                     type="button"
                     onClick={continueAsGuest}
                     disabled={loading}
                     className="text-[10px] font-black text-gray-400 uppercase tracking-widest hover:text-blue-600 transition-colors"
                   >
                      Tiếp tục với tư cách khách vãng lai
                   </button>
                </div>
             </form>
           ) : (
             <form onSubmit={handleRegister} className="space-y-6 animate-in slide-in-from-right duration-300">
                {error && <p role="alert" className="rounded-xl bg-red-50 px-4 py-3 text-center text-xs font-bold text-red-600">{error}</p>}
                <p className="text-center text-xs font-bold text-blue-600 bg-blue-50 py-2 rounded-xl">Chào mừng bạn mới! Vui lòng cho biết tên nhé</p>
                <div>
                   <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3 ml-2">Họ và tên</label>
                   <input
                     type="text"
                     className="w-full px-6 py-4 bg-gray-50 border-none rounded-2xl outline-none focus:ring-2 focus:ring-blue-500/20 font-black text-gray-700"
                     placeholder="Ví dụ: Nguyễn Văn A"
                     value={fullName}
                     onChange={e => setFullName(e.target.value)}
                     required
                   />
                </div>
                <button
                  disabled={loading}
                  className="w-full py-4 bg-blue-600 text-white rounded-2xl font-black uppercase tracking-widest shadow-lg shadow-blue-600/30 hover:bg-blue-700 transition-all flex items-center justify-center"
                >
                   {loading ? <Loader2 className="animate-spin" /> : "HOÀN TẤT ĐĂNG KÝ"}
                </button>
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
