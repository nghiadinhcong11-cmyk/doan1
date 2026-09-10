import React, { useState, useEffect, useRef } from 'react';
import { User, Store, Package, History, Shield, Settings, LogOut, ChevronRight, Moon, Globe, Camera, Save, CheckCircle2, X, CreditCard, Lock, Eye, EyeOff, AlertCircle, MapPin, Phone, Zap } from 'lucide-react';
import { API_URL } from '../../../config';

interface ProfileData {
  businessType: string;
  birthDate: string;
  phone: string;
  representative: string;
  address: string;
  storeName: string;
  expiryDate: string;
  industry: string;
  servicePackage: string;
  avatarUrl?: string;
  email?: string;
  taxCode?: string;
}

const ProfilePage = () => {
  const [activeTab, setActiveTab] = useState('account'); // account, package, billing, security
  const [isEditing, setIsEditing] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [profile, setProfile] = useState<ProfileData>({
    businessType: 'Cá nhân',
    birthDate: '1995-01-01',
    phone: '0949774303',
    representative: 'Đinh Công Nghĩa',
    address: '123 Đường ABC, Quận 1, TP.HCM',
    storeName: 'DOAN RESTAURANT',
    expiryDate: '20/07/2026',
    industry: 'F&B - Bar, Coffee & Restaurant',
    servicePackage: 'Gói Chuyên nghiệp',
    avatarUrl: '',
    email: 'contact@doan-pos.vn',
    taxCode: '0102030405'
  });

  const [passwords, setPasswords] = useState({ current: '', new: '', confirm: '' });
  const [showPass, setShowPass] = useState({ current: false, new: false, confirm: false });
  const [authUserId, setAuthUserId] = useState('');
  const [passwordSaving, setPasswordSaving] = useState(false);
  const [passwordError, setPasswordError] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [branchData, setBranchData] = useState<any>(null);
  const userRole = localStorage.getItem('userRole');

  // Tải dữ liệu từ API để đồng bộ
  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        // 1. Lấy thông tin User hiện tại
        const meRes = await fetch(`${API_URL}/api/Auth/me`, {
          headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
        });
        const user = meRes.ok ? await meRes.json() : null;
        if (user) setAuthUserId(user.userId || '');

        // 2. Lấy thông tin Chi nhánh (Trụ sở chính)
        const branchRes = await fetch(`${API_URL}/api/Branch`);
        const branches = branchRes.ok ? await branchRes.json() : [];
        const main = branches.find((b: any) => b.isMain) || branches[0];

        if (main) {
          setBranchData(main);
          setProfile(current => ({
            ...current,
            storeName: main.name || current.storeName,
            address: main.address || current.address,
            phone: main.phoneNumber || current.phone,
            representative: main.representativeName || current.representative,
            email: main.representativeEmail || current.email,
            taxCode: main.taxCode || current.taxCode,
            industry: main.industry || current.industry,
            businessType: main.businessType || current.businessType,
            avatarUrl: main.imageUrl || current.avatarUrl
          }));
        }
      } catch (err) {
        console.error("Lỗi tải thông tin hồ sơ:", err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  const handleSave = async () => {
    if (!branchData) return;

    try {
      const payload = {
        ...branchData,
        name: profile.storeName,
        address: profile.address,
        phoneNumber: profile.phone,
        representativeName: profile.representative,
        representativeEmail: profile.email,
        taxCode: profile.taxCode,
        industry: profile.industry,
        businessType: profile.businessType,
        imageUrl: profile.avatarUrl
      };

      const response = await fetch(`${API_URL}/api/Branch/${branchData.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        setIsEditing(false);
        setShowSuccess(true);
        setTimeout(() => setShowSuccess(false), 3000);
      } else {
        const err = await response.json();
        alert("Lỗi: " + (err.message || "Không thể lưu thay đổi"));
      }
    } catch (err) {
      alert("Lỗi kết nối máy chủ khi lưu hồ sơ");
    }
  };

  const handleImageUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setProfile({ ...profile, avatarUrl: reader.result as string });
      };
      reader.readAsDataURL(file);
    }
  };

  const handleChangePassword = async () => {
    setPasswordError('');
    if (!authUserId) return setPasswordError('Không xác định được tài khoản. Vui lòng đăng nhập lại.');
    if (passwords.new.length < 8) return setPasswordError('Mật khẩu mới phải có ít nhất 8 ký tự.');
    if (passwords.new !== passwords.confirm) return setPasswordError('Mật khẩu xác nhận không khớp.');
    try {
      setPasswordSaving(true);
      const response = await fetch(`${API_URL}/api/Auth/change-password`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: authUserId, oldPassword: passwords.current, newPassword: passwords.new, type: 'Employee' })
      });
      if (!response.ok) {
        const body = await response.json().catch(() => ({}));
        throw new Error(body.message || 'Không thể cập nhật mật khẩu.');
      }
      setPasswords({ current: '', new: '', confirm: '' });
      setShowSuccess(true);
      window.setTimeout(() => setShowSuccess(false), 3000);
    } catch (error: any) { setPasswordError(error.message || 'Không thể kết nối máy chủ.'); }
    finally { setPasswordSaving(false); }
  };

  const renderAccountInfo = () => (
    <div className="space-y-12 animate-in fade-in duration-300">
      <section>
        <div className="flex items-center space-x-2 mb-6 border-b pb-2 border-blue-100">
           <div className="p-1.5 bg-blue-50 text-[#0070f4] rounded-md"><User size={16}/></div>
           <h3 className="text-sm font-black text-gray-700 uppercase tracking-widest italic">Hồ sơ chủ quán</h3>
        </div>

        <div className="grid grid-cols-2 gap-x-12 gap-y-8">
          <div className="space-y-1">
            <label className="block text-[11px] font-black text-gray-400 uppercase tracking-widest">Người đại diện</label>
            {isEditing ? (
              <input
                type="text"
                className="w-full border-b-2 border-gray-100 py-1.5 focus:border-blue-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                value={profile.representative}
                onChange={e => setProfile({...profile, representative: e.target.value})}
              />
            ) : (
              <p className="text-sm font-black text-gray-800 py-1.5">{profile.representative}</p>
            )}
          </div>

          <div className="space-y-1">
            <label className="block text-[11px] font-black text-gray-400 uppercase tracking-widest">Số điện thoại liên hệ</label>
            {isEditing ? (
              <input
                type="text"
                className="w-full border-b-2 border-gray-100 py-1.5 focus:border-blue-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                value={profile.phone}
                onChange={e => setProfile({...profile, phone: e.target.value})}
              />
            ) : (
              <p className="text-sm font-black text-blue-600 py-1.5 flex items-center"><Phone size={14} className="mr-2" /> {profile.phone}</p>
            )}
          </div>

          <div className="space-y-1">
            <label className="block text-[11px] font-black text-gray-400 uppercase tracking-widest">Email đăng ký</label>
            {isEditing ? (
              <input
                type="email"
                className="w-full border-b-2 border-gray-100 py-1.5 focus:border-red-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                value={profile.email}
                onChange={e => setProfile({...profile, email: e.target.value})}
              />
            ) : (
              <p className="text-sm font-bold text-gray-600 py-1.5">{profile.email}</p>
            )}
          </div>

          <div className="space-y-1">
            <label className="block text-[11px] font-black text-gray-400 uppercase tracking-widest">Mã số thuế</label>
            {isEditing ? (
              <input
                type="text"
                className="w-full border-b-2 border-gray-100 py-1.5 focus:border-red-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                value={profile.taxCode}
                onChange={e => setProfile({...profile, taxCode: e.target.value})}
              />
            ) : (
              <p className="text-sm font-bold text-gray-700 py-1.5">{profile.taxCode || '---'}</p>
            )}
          </div>
        </div>
      </section>

      <section>
        <div className="flex items-center space-x-2 mb-6 border-b pb-2 border-blue-100">
           <div className="p-1.5 bg-blue-50 text-[#0070f4] rounded-md"><Store size={16}/></div>
           <h3 className="text-sm font-black text-gray-700 uppercase tracking-widest italic">Thông tin thương hiệu</h3>
        </div>

        <div className="grid grid-cols-2 gap-x-12 gap-y-8">
          <div className="space-y-1">
            <label className="block text-[11px] font-black text-gray-400 uppercase tracking-widest">Tên gian hàng</label>
            {isEditing ? (
              <input
                type="text"
                className="w-full border-b-2 border-gray-100 py-1.5 focus:border-blue-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                value={profile.storeName}
                onChange={e => setProfile({...profile, storeName: e.target.value})}
              />
            ) : (
              <p className="text-lg font-black text-[#0070f4] py-1.5 uppercase tracking-tighter italic">{profile.storeName}</p>
            )}
          </div>

          <div className="space-y-1">
            <label className="block text-[11px] font-black text-gray-400 uppercase tracking-widest">Địa chỉ văn phòng</label>
            {isEditing ? (
              <input
                type="text"
                className="w-full border-b-2 border-gray-100 py-1.5 focus:border-red-500 outline-none font-bold text-gray-700 transition-all bg-transparent"
                value={profile.address}
                onChange={e => setProfile({...profile, address: e.target.value})}
              />
            ) : (
              <p className="text-sm font-medium text-gray-600 py-1.5 flex items-start"><MapPin size={14} className="mr-2 mt-0.5 text-gray-400" /> {profile.address}</p>
            )}
          </div>

          <div className="space-y-1">
            <label className="block text-[11px] font-black text-gray-400 uppercase tracking-widest">Lĩnh vực hoạt động</label>
            <p className="text-sm font-bold text-gray-700 py-1.5 bg-gray-50 px-3 rounded-xl inline-block">{profile.industry}</p>
          </div>
        </div>
      </section>
    </div>
  );

  const renderPackageInfo = () => (
    <div className="space-y-8 animate-in slide-in-from-right-4 duration-300">
       <div className="bg-gradient-to-br from-[#0070f4] to-blue-900 rounded-[2rem] p-10 text-white relative overflow-hidden shadow-2xl shadow-blue-500/20">
          <div className="absolute -top-10 -right-10 opacity-10 rotate-12"><Package size={280} /></div>
          <div className="relative z-10">
             <div className="flex items-center space-x-3 mb-4">
                <span className="bg-white/20 backdrop-blur-md px-4 py-1 rounded-full text-[10px] font-black uppercase tracking-[0.2em] border border-white/30">Contracted</span>
                <span className="bg-green-400 text-green-900 px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest">Active</span>
             </div>
             <h3 className="text-4xl font-black mb-2 italic tracking-tighter">{profile.servicePackage}</h3>
             <p className="text-blue-100 text-sm mb-10 italic font-medium opacity-80">Thời hạn sử dụng đến: <span className="font-black text-white">{profile.expiryDate}</span></p>

             <div className="grid grid-cols-3 gap-10 pt-10 border-t border-white/10">
                <div className="group">
                    <p className="text-[10px] uppercase font-black text-blue-200 mb-2 tracking-widest opacity-70">Chi nhánh</p>
                    <p className="text-2xl font-black italic">05 <span className="text-sm opacity-50 not-italic">/ max</span></p>
                </div>
                <div>
                    <p className="text-[10px] uppercase font-black text-blue-200 mb-2 tracking-widest opacity-70">Nhân sự</p>
                    <p className="text-2xl font-black italic">UNLIMITED</p>
                </div>
                <div>
                    <p className="text-[10px] uppercase font-black text-blue-200 mb-2 tracking-widest opacity-70">Giao dịch/tháng</p>
                    <p className="text-2xl font-black italic">5,000</p>
                </div>
             </div>
          </div>
       </div>

       <div className="grid grid-cols-2 gap-8">
          <div className="bg-white border-2 border-gray-50 rounded-[2rem] p-8 hover:border-blue-100 transition-all shadow-xl shadow-blue-500/5">
             <h4 className="font-black text-gray-800 uppercase italic tracking-tighter mb-6 flex items-center">
                <CheckCircle2 className="text-green-500 mr-3" size={20}/> Tính năng hiện có
             </h4>
             <ul className="space-y-4">
                {['Bán hàng POS đa nền tảng', 'Quản lý kho hàng & Định lượng', 'Báo cáo phân tích chuyên sâu', 'Tích điểm & CSKH tự động', 'Quản lý đa chi nhánh'].map((item, i) => (
                    <li key={i} className="flex items-center text-xs font-bold text-gray-500">
                        <div className="w-1.5 h-1.5 bg-green-500 rounded-full mr-3"></div>
                        {item}
                    </li>
                ))}
             </ul>
          </div>
          <div className="bg-blue-50 rounded-[2rem] p-8 border border-blue-100 relative overflow-hidden group">
             <div className="absolute top-0 right-0 p-4 opacity-5 group-hover:rotate-12 transition-transform"><Zap size={100} /></div>
             <h4 className="font-black text-blue-800 uppercase italic tracking-tighter mb-4 flex items-center">
                <Zap className="text-blue-500 mr-3" size={20}/> Nâng cấp hệ thống
             </h4>
             <p className="text-xs text-blue-700/70 mb-8 italic leading-relaxed font-medium">Bạn đang sử dụng đầy đủ các tính năng tốt nhất. Liên hệ đội ngũ kỹ thuật để yêu cầu thêm các tính năng tùy chỉnh riêng cho thương hiệu của bạn.</p>
             <button className="w-full py-4 bg-[#0070f4] text-white rounded-2xl font-black text-xs uppercase tracking-widest shadow-xl shadow-blue-500/30 hover:bg-blue-700 transition-all active:scale-95">LIÊN HỆ TƯ VẤN</button>
          </div>
       </div>
    </div>
  );

  const renderBillingHistory = () => (
    <div className="animate-in slide-in-from-right-4 duration-300">
       <div className="flex justify-between items-center mb-8">
          <h3 className="text-sm font-black text-gray-700 uppercase tracking-widest italic">Lịch sử thanh toán dịch vụ</h3>
          <button className="text-[10px] font-black text-[#0070f4] flex items-center uppercase tracking-widest hover:underline bg-blue-50 px-4 py-2 rounded-xl transition-all">
             <History size={14} className="mr-2"/> Xuất sao kê
          </button>
       </div>

       <div className="bg-white rounded-[2rem] overflow-hidden border border-gray-100 shadow-xl shadow-blue-500/5">
          <table className="w-full text-left border-collapse">
             <thead className="bg-gray-50 border-b text-[10px] font-black text-gray-400 uppercase tracking-[0.2em]">
                <tr>
                   <th className="px-8 py-5">Mã Giao dịch</th>
                   <th className="px-8 py-5">Nội dung</th>
                   <th className="px-8 py-5">Ngày thanh toán</th>
                   <th className="px-8 py-5 text-right">Số tiền (VNĐ)</th>
                   <th className="px-8 py-5 text-center">Trạng thái</th>
                </tr>
             </thead>
             <tbody className="divide-y divide-gray-50 text-xs font-bold text-gray-600">
                {[
                  { code: 'DP-2026-001', title: 'Gia hạn Gói Chuyên nghiệp (12 tháng)', date: '20/03/2026', amount: 5000000, status: 'Thành công' },
                  { code: 'DP-2026-000', title: 'Phí thiết lập hệ thống ban đầu', date: '20/03/2026', amount: 1000000, status: 'Thành công' },
                ].map((bill, i) => (
                  <tr key={i} className="hover:bg-blue-50/30 transition-colors">
                     <td className="px-8 py-5 font-black text-[#0070f4] tracking-tighter">{bill.code}</td>
                     <td className="px-8 py-5">{bill.title}</td>
                     <td className="px-8 py-5 text-gray-400">{bill.date}</td>
                     <td className="px-8 py-5 text-right font-black text-gray-800">{bill.amount.toLocaleString()}</td>
                     <td className="px-8 py-5 text-center">
                        <span className="bg-green-50 text-green-700 px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-tighter border border-green-100">
                           {bill.status}
                        </span>
                     </td>
                  </tr>
                ))}
             </tbody>
          </table>
       </div>
    </div>
  );

  const renderSecurity = () => (
    <div className="max-w-md animate-in slide-in-from-right-4 duration-300 space-y-10">
       <section>
          <div className="flex items-center space-x-2 mb-10 border-b pb-3 border-blue-100">
             <div className="p-1.5 bg-blue-50 text-[#0070f4] rounded-md"><Lock size={16}/></div>
             <h3 className="text-sm font-black text-gray-700 uppercase tracking-widest italic">Thay đổi mật khẩu đăng nhập</h3>
          </div>

          <div className="space-y-8">
             {[
                { label: 'Mật khẩu hiện tại', key: 'current' as const },
                { label: 'Mật khẩu mới', key: 'new' as const },
                { label: 'Xác nhận mật khẩu mới', key: 'confirm' as const }
             ].map((f) => (
                <div key={f.key} className="space-y-2">
                   <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">{f.label}</label>
                   <div className="relative group">
                      <input
                        type={showPass[f.key] ? "text" : "password"}
                        className="w-full border-b-2 border-gray-100 py-2.5 focus:border-blue-500 outline-none font-black text-gray-700 bg-transparent pr-12 transition-all"
                        value={passwords[f.key]}
                        onChange={e => setPasswords({...passwords, [f.key]: e.target.value})}
                        placeholder="••••••••"
                      />
                      <button
                        type="button"
                        onClick={() => setShowPass({...showPass, [f.key]: !showPass[f.key]})}
                        className="absolute right-2 top-2.5 text-gray-300 hover:text-blue-500 transition-colors"
                      >
                         {showPass[f.key] ? <EyeOff size={18}/> : <Eye size={18}/>}
                      </button>
                   </div>
                </div>
             ))}

             {passwordError && <div className="rounded-xl border border-red-100 bg-red-50 p-3 text-xs font-bold text-red-600">{passwordError}</div>}
             <button onClick={() => void handleChangePassword()} disabled={passwordSaving} className="w-full py-5 bg-[#0070f4] text-white rounded-2xl font-black shadow-xl shadow-blue-500/20 hover:bg-blue-700 transition-all uppercase tracking-[0.2em] text-[10px] mt-6 active:scale-95 disabled:cursor-wait disabled:opacity-60">
                {passwordSaving ? 'ĐANG CẬP NHẬT...' : 'CẬP NHẬT MẬT KHẨU'}
             </button>
          </div>
       </section>

       <div className="bg-gray-50 p-6 rounded-[2rem] border-2 border-white shadow-inner">
          <div className="flex items-start space-x-3">
             <AlertCircle size={18} className="text-blue-500 mt-0.5 shrink-0"/>
             <div>
                <p className="text-[10px] font-black text-gray-800 uppercase mb-1">Mẹo bảo mật</p>
                <p className="text-[11px] text-gray-500 italic leading-relaxed font-medium">Mật khẩu mạnh nên có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt (@, !, #...) để bảo vệ dữ liệu kinh doanh của bạn tốt nhất.</p>
             </div>
          </div>
       </div>
    </div>
  );

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f8f9fa] overflow-hidden font-sans">
      {/* Sidebar Menu */}
      <div className="w-80 bg-white border-r p-8 overflow-y-auto shadow-sm flex flex-col">
        <h2 className="font-black text-xl mb-10 text-gray-800 uppercase italic tracking-tighter">Cài đặt hệ thống</h2>

        <div className="space-y-2 flex-1">
          <p className="text-[10px] font-black text-gray-300 uppercase mb-3 px-4 tracking-[0.2em]">Cấu hình chung</p>
          <button
            onClick={() => setActiveTab('account')}
            className={`w-full flex items-center px-4 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${activeTab === 'account' ? 'bg-blue-50 text-[#0070f4] shadow-sm' : 'text-gray-400 hover:bg-gray-50'}`}
          >
            <Shield className="h-5 w-5 mr-4" /> Hồ sơ chủ quán
          </button>

          <p className="text-[10px] font-black text-gray-300 uppercase mt-10 mb-3 px-4 tracking-[0.2em]">Hợp đồng dịch vụ</p>
          <button
            onClick={() => setActiveTab('package')}
            className={`w-full flex items-center px-4 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${activeTab === 'package' ? 'bg-blue-50 text-[#0070f4] shadow-sm' : 'text-gray-400 hover:bg-gray-50'}`}
          >
            <Package className="h-5 w-5 mr-4" /> Gói dịch vụ
          </button>
          <button
            onClick={() => setActiveTab('billing')}
            className={`w-full flex items-center px-4 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${activeTab === 'billing' ? 'bg-blue-50 text-[#0070f4] shadow-sm' : 'text-gray-400 hover:bg-gray-50'}`}
          >
            <History className="h-5 w-5 mr-4" /> Lịch sử thanh toán
          </button>

          <p className="text-[10px] font-black text-gray-300 uppercase mt-10 mb-3 px-4 tracking-[0.2em]">An toàn bảo mật</p>
          <button
            onClick={() => setActiveTab('security')}
            className={`w-full flex items-center px-4 py-4 rounded-2xl font-black text-xs uppercase tracking-widest transition-all ${activeTab === 'security' ? 'bg-blue-50 text-[#0070f4] shadow-sm' : 'text-gray-400 hover:bg-gray-50'}`}
          >
            <Settings className="h-5 w-5 mr-4" /> Đổi mật khẩu
          </button>
        </div>

        <div className="mt-10 pt-10 border-t border-gray-50">
           <div className="bg-blue-50 p-6 rounded-[2rem] border border-blue-100 shadow-sm relative overflow-hidden group">
              <div className="absolute -bottom-4 -right-4 text-blue-100 group-hover:scale-110 transition-transform"><Package size={80} /></div>
              <p className="text-[#0070f4] font-black text-xs flex items-center mb-2 uppercase tracking-tighter italic">Hợp đồng VIP PRO</p>
              <p className="text-[10px] text-blue-700/60 leading-relaxed font-medium italic relative z-10">Tận hưởng tối đa sức mạnh quản lý không giới hạn chi nhánh.</p>
           </div>
        </div>
      </div>

      {/* Main Content Area */}
      <div className="flex-1 overflow-auto p-10 bg-[#f8f9fa]">
        <div className="max-w-5xl mx-auto">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center min-h-[600px] bg-white rounded-[3rem] shadow-2xl shadow-blue-500/5 border border-white">
              <Zap className="h-12 w-12 text-blue-600 animate-pulse mb-4" />
              <p className="text-[10px] font-black uppercase tracking-[0.3em] text-gray-400">Đang tải hồ sơ hệ thống...</p>
            </div>
          ) : (
            <>
              {/* Header with Actions */}
              <div className="flex justify-between items-end mb-10">
             <div>
                <h1 className="text-3xl font-black text-gray-800 tracking-tighter uppercase italic">
                   {activeTab === 'account' ? 'Thông tin chủ sở hữu' :
                    activeTab === 'package' ? 'Chi tiết gói dịch vụ' :
                    activeTab === 'billing' ? 'Hóa đơn dịch vụ' : 'Bảo mật hệ thống'}
                </h1>
                <p className="text-xs text-gray-400 font-bold uppercase tracking-widest mt-2">
                   {activeTab === 'account' ? 'Định danh người sở hữu thương hiệu và gian hàng' :
                    activeTab === 'package' ? 'Kiểm tra thời hạn và các tính năng được phép sử dụng' :
                    activeTab === 'billing' ? 'Theo dõi các khoản phí duy trì phần mềm' : 'Thiết lập các lớp bảo vệ cho tài khoản quản trị'}
                </p>
             </div>
             <div className="flex items-center space-x-4 pb-1">
                {showSuccess && (
                  <div className="flex items-center text-green-600 text-[10px] font-black uppercase tracking-widest animate-in fade-in slide-in-from-right-4 bg-green-50 px-4 py-2 rounded-xl border border-green-100">
                    <CheckCircle2 size={14} className="mr-2" /> Đã cập nhật thành công
                  </div>
                )}

                {activeTab === 'account' && userRole === 'admin' && (
                  !isEditing ? (
                    <button
                      onClick={() => setIsEditing(true)}
                      className="px-8 py-3 bg-[#dc2626] text-white rounded-2xl font-black text-[10px] uppercase tracking-widest shadow-xl shadow-red-500/20 hover:bg-red-700 transition-all active:scale-95"
                    >
                      CHỈNH SỬA HỒ SƠ
                    </button>
                  ) : (
                    <div className="flex space-x-3 animate-in fade-in zoom-in-95 duration-200">
                      <button
                        onClick={() => setIsEditing(false)}
                        className="px-6 py-3 bg-white border-2 border-gray-100 text-gray-400 rounded-2xl font-black text-[10px] uppercase tracking-widest hover:bg-gray-50 transition-all"
                      >
                        HỦY BỎ
                      </button>
                      <button
                        onClick={handleSave}
                        className="px-8 py-3 bg-green-600 text-white rounded-2xl font-black text-[10px] uppercase tracking-widest shadow-xl shadow-green-500/20 hover:bg-green-700 transition-all flex items-center"
                      >
                        <Save size={16} className="mr-2" /> LƯU THAY ĐỔI
                      </button>
                    </div>
                  )
                )}
             </div>
          </div>

          <div className="bg-white rounded-[3rem] shadow-2xl shadow-blue-500/5 border border-white overflow-hidden min-h-[600px]">
            {/* Show Profile Header ONLY for Account Tab */}
            {activeTab === 'account' && (
              <div className="h-40 bg-gradient-to-r from-[#0070f4] to-blue-800 relative">
                 <div className="absolute -bottom-14 left-12 flex items-end">
                    <div className="w-32 h-32 bg-white rounded-[2.5rem] p-1.5 shadow-2xl relative">
                       <div
                          onClick={() => fileInputRef.current?.click()}
                          className="w-full h-full bg-blue-50 rounded-[2rem] flex items-center justify-center text-[#0070f4] relative group overflow-hidden cursor-pointer border-2 border-white shadow-inner"
                        >
                          {profile.avatarUrl ? (
                            <img src={profile.avatarUrl} alt="" className="w-full h-full object-cover" />
                          ) : (
                            <User size={56} className="opacity-80" />
                          )}
                          <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                             <Camera size={24} className="text-white" />
                          </div>
                       </div>
                       <input
                         type="file"
                         ref={fileInputRef}
                         className="hidden"
                         accept="image/*"
                         onChange={handleImageUpload}
                       />
                       <div className="absolute -bottom-2 -right-2 bg-green-500 text-white p-2 rounded-full border-4 border-white shadow-lg">
                          <CheckCircle2 size={16} />
                       </div>
                    </div>
                    <div className="ml-8 mb-4">
                       <h2 className="text-2xl font-black text-gray-800 uppercase italic tracking-tighter">{profile.representative}</h2>
                       <p className="text-[10px] text-blue-600 font-black uppercase tracking-widest flex items-center mt-1 opacity-80">
                          <Shield size={12} className="mr-2" /> Chủ sở hữu hệ thống
                       </p>
                    </div>
                 </div>
              </div>
            )}

            <div className={`${activeTab === 'account' ? 'pt-24' : 'pt-12'} px-12 pb-12`}>
               {activeTab === 'account' && renderAccountInfo()}
               {activeTab === 'package' && renderPackageInfo()}
               {activeTab === 'billing' && renderBillingHistory()}
               {activeTab === 'security' && renderSecurity()}
            </div>
          </div>

            </>
          )}

          <p className="text-center mt-12 text-[10px] text-gray-300 font-black uppercase tracking-[0.3em] italic">
            DOAN Restaurant POS • Enterprise Edition v2.4.0
          </p>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage;

