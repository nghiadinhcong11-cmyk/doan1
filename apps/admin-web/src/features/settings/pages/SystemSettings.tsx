import React, { useState, useEffect } from 'react';
import { Search, ChevronRight, Settings, Users, Store, Printer, CreditCard, ShieldCheck, Bell, Database, Lock, Trash2, ShoppingBag, Utensils, ClipboardList, Users2, BarChart3, Receipt, QrCode, Truck, MessageSquare, Info, Smartphone, Eye, AlertTriangle, CheckCircle2, Loader2, RefreshCw } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { API_URL } from '../../../config';
import { getCachedJson, invalidateCachedJson } from '../../../api/queryCache';
import { notifyFeedback } from '../../../components/ui';
import type { BranchManagementRecord } from '../branchTypes';

const SystemSettings = () => {
  const navigate = useNavigate();
  const [activeSubTab, setActiveSubTab] = useState('store-info');
  const [showSaveSuccess, setShowSaveSuccess] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  const [selectedBranch, setSelectedBranch] = useState<BranchManagementRecord | null>(null);
  const [loadingBranch, setLoadingBranch] = useState(false);
  const [branchError, setBranchError] = useState<string | null>(null);

  const DEFAULT_SETTINGS = {
    vatPercent: 8,
    serviceFee: 0,
  };

  const [settings, setSettings] = useState<any>(DEFAULT_SETTINGS);
  const [serviceFeeKey, setServiceFeeKey] = useState<'serviceFee' | 'serviceFeePercent'>('serviceFee');
  const [selectedBranchId, setSelectedBranchId] = useState(() => localStorage.getItem('selectedBranchId') || '');
  const userRole = localStorage.getItem('userRole');

  useEffect(() => {
    const handleBranchChanged = () => setSelectedBranchId(localStorage.getItem('selectedBranchId') || '');
    window.addEventListener('branch:changed', handleBranchChanged);
    const syncTimer = window.setTimeout(handleBranchChanged, 0);
    return () => {
      window.clearTimeout(syncTimer);
      window.removeEventListener('branch:changed', handleBranchChanged);
    };
  }, []);

  useEffect(() => {
    const init = async (force = false) => {
      setBranchError(null);
      const branch = await fetchSelectedBranch(force);
      if (branch) await fetchSettings(force);
      else {
        setSettings(DEFAULT_SETTINGS);
        setIsLoading(false);
      }
    };
    void init();
  }, [selectedBranchId]);

  const fetchSettings = async (force = false) => {
    try {
      setIsLoading(true);
      const token = localStorage.getItem('adminToken');
      const data = await getCachedJson<Record<string, unknown>>(`${API_URL}/api/SystemSettings?branchId=${selectedBranchId}`, {
        headers: { 'Authorization': `Bearer ${token}` }
      }, { force });
      {
        // Parse strings back to their types
        const parsedSettings = { ...DEFAULT_SETTINGS };
        Object.keys(data).forEach(key => {
          const val = data[key];
          if (val === 'true') (parsedSettings as any)[key] = true;
          else if (val === 'false') (parsedSettings as any)[key] = false;
          else if ((typeof val === 'number') || (typeof val === 'string' && val !== '' && !Number.isNaN(Number(val)))) (parsedSettings as any)[key] = Number(val);
          else (parsedSettings as any)[key] = val;
        });
        const effectiveServiceFeeKey = Object.prototype.hasOwnProperty.call(data, 'serviceFeePercent')
          ? 'serviceFeePercent'
          : 'serviceFee';
        setServiceFeeKey(effectiveServiceFeeKey);
        if (effectiveServiceFeeKey === 'serviceFeePercent' && data.serviceFeePercent !== undefined) {
          parsedSettings.serviceFee = Number(data.serviceFeePercent) || 0;
        }
        setSettings(parsedSettings);
      }
    } catch (err) {
      console.error("Lỗi lấy thiết lập hệ thống:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchSelectedBranch = async (force = false): Promise<BranchManagementRecord | null> => {
    if (!selectedBranchId) {
      setSelectedBranch(null);
      setBranchError('Vui lòng chọn một chi nhánh trước khi mở cấu hình.');
      return null;
    }
    try {
      setLoadingBranch(true);
      const data = await getCachedJson<BranchManagementRecord[]>(`${API_URL}/api/Branch/management`, undefined, { force });
      const branch = data.find(item => item.id === selectedBranchId && item.isActive) || null;
      setSelectedBranch(branch);
      if (!branch) setBranchError('Chi nhánh đang chọn không hợp lệ hoặc đã ngừng hoạt động. Vui lòng chọn lại chi nhánh.');
      return branch;
    } catch (err) {
      console.error("Lỗi lấy thông tin trụ sở:", err);
      setSelectedBranch(null);
      setBranchError(err instanceof Error ? err.message : 'Không thể tải thông tin chi nhánh.');
      return null;
    } finally {
      setLoadingBranch(false);
    }
  };

  const refreshSettings = async () => {
    invalidateCachedJson('/api/SystemSettings');
    invalidateCachedJson('/api/Branch/management');
    setBranchError(null);
    const branch = await fetchSelectedBranch(true);
    if (branch) await fetchSettings(true);
  };


  const handleSave = async () => {
    if (!selectedBranchId) {
      notifyFeedback('Vui lòng chọn một chi nhánh trước khi lưu thuế và phí.');
      return;
    }
    try {
      setIsSaving(true);
      const token = localStorage.getItem('adminToken');

      // Convert all values to strings for the API
      const payload: Record<string, string> = { vatPercent: String(settings.vatPercent) };
      payload[serviceFeeKey] = String(settings.serviceFee);

      const response = await fetch(`${API_URL}/api/SystemSettings?branchId=${selectedBranchId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        invalidateCachedJson('/api/SystemSettings');
        setShowSaveSuccess(true);
        setTimeout(() => setShowSaveSuccess(false), 3000);
      } else {
        notifyFeedback("Lỗi lưu thiết lập hệ thống");
      }
    } catch (err) {
      console.error("Lỗi lưu thiết lập:", err);
      notifyFeedback("Lỗi kết nối máy chủ");
    } finally {
      setIsSaving(false);
    }
  };

  const ToggleItem = ({
    title,
    desc,
    settingKey,
    defaultOn
  }: {
    title: string,
    desc: string,
    settingKey?: string,
    defaultOn?: boolean
  }) => {
    const isOn = settingKey ? (settings as any)[settingKey] : !!defaultOn;
    return (
      <div className="flex items-center justify-between p-4 border rounded-xl hover:bg-gray-50 transition-colors border-gray-100">
        <div className="max-w-md">
          <p className="font-bold text-sm text-gray-700">{title}</p>
          <p className="text-[11px] text-gray-400 mt-1">{desc}</p>
        </div>
        <div
          onClick={() => {
            if (settingKey) setSettings({ ...settings, [settingKey]: !isOn });
          }}
          className={`w-12 h-6 rounded-full p-1 cursor-pointer transition-colors ${isOn ? 'bg-blue-600' : 'bg-gray-200'}`}
        >
          <div className={`w-4 h-4 bg-white rounded-full shadow-md transform transition-transform ${isOn ? 'translate-x-6' : 'translate-x-0'}`}></div>
        </div>
      </div>
    );
  };

  const renderContent = () => {
    if (isLoading && activeSubTab !== 'store-info') {
      return (
        <div className="flex-1 flex flex-col items-center justify-center bg-white">
          <Loader2 className="animate-spin text-blue-600 mb-2" size={32} />
          <p className="text-[10px] font-black uppercase tracking-widest text-gray-400">Đang tải thiết lập...</p>
        </div>
      );
    }

    if (!selectedBranch) {
      return <div className="p-10 bg-white"><div className="mx-auto max-w-xl rounded-2xl border border-amber-200 bg-amber-50 p-6 text-center"><h3 className="text-lg font-black text-amber-900">Chưa có chi nhánh hợp lệ</h3><p className="mt-2 text-sm text-amber-800">{branchError || 'Vui lòng chọn chi nhánh từ bộ chọn trên thanh điều hướng.'}</p></div></div>;
    }

    switch (activeSubTab) {
      case 'store-info':
        if (loadingBranch) {
           return <div className="flex-1 flex flex-col items-center justify-center"><Loader2 className="animate-spin text-blue-600 mb-2" size={32} /><p className="text-[10px] font-black uppercase tracking-widest text-gray-400">Đang tải thông tin chi nhánh...</p></div>;
        }
        return (
          <div className="p-10 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto space-y-10">
                <div className="flex items-center space-x-6 border-b pb-8">
                   <div className="w-24 h-24 bg-gradient-to-br from-blue-50 to-blue-100 rounded-3xl flex items-center justify-center text-blue-600 border border-blue-200 shadow-inner overflow-hidden">
                      {selectedBranch.imageUrl ? <img src={selectedBranch.imageUrl} alt="Logo chi nhánh" className="h-full w-full object-cover" /> : <Store size={48} />}
                   </div>
                   <div>
                      <h3 className="text-2xl font-black text-gray-800 tracking-tight uppercase">Chi nhánh đang chọn</h3>
                      <p className="text-sm text-gray-400">Thông tin chỉ đọc của chi nhánh hiện tại; chỉnh sửa tại Quản lý chi nhánh.</p>
                      <div className="mt-2 flex space-x-3">
                         <span className="bg-blue-600 text-white text-[10px] font-bold px-2 py-0.5 rounded tracking-tighter uppercase">ID: {selectedBranch.id?.substring(0,8).toUpperCase()}</span>
                         {selectedBranch.isMain && <span className="bg-green-100 text-green-700 text-[10px] font-bold px-2 py-0.5 rounded tracking-tighter uppercase italic">Trụ sở chính</span>}
                      </div>
                   </div>
                </div>

                <div className="grid grid-cols-2 gap-x-10 gap-y-8">
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Tên cửa hàng</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.name || ''}
                        readOnly
                      />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Số điện thoại</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.phoneNumber || ''}
                        readOnly
                      />
                   </div>
                   <div className="col-span-2 space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Địa chỉ trụ sở</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.address || ''}
                        readOnly
                      />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Người đại diện</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.representativeName || ''}
                        readOnly
                      />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider">Email đại diện</label>
                      <input type="email" className="w-full border-b-2 border-gray-100 py-2 outline-none font-bold text-gray-700 bg-transparent" value={selectedBranch.representativeEmail || ''} readOnly />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Mã số thuế</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.taxCode || ''}
                        readOnly
                      />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Tên ngân hàng</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.bankName || ''}
                        readOnly
                      />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Số tài khoản</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.accountNumber || ''}
                        readOnly
                      />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider">Chủ tài khoản</label>
                      <input type="text" className="w-full border-b-2 border-gray-100 py-2 outline-none font-bold text-gray-700 bg-transparent" value={selectedBranch.accountHolder || ''} readOnly />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider group-focus-within:text-blue-500 transition-colors">Lĩnh vực hoạt động</label>
                      <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 py-2 outline-none focus:border-blue-500 font-bold text-gray-700 transition-all bg-transparent"
                        value={selectedBranch.industry || ''}
                        readOnly
                      />
                   </div>
                   <div className="space-y-1 group">
                      <label className="text-[11px] font-black text-gray-400 uppercase tracking-wider">Loại hình kinh doanh</label>
                      <input type="text" className="w-full border-b-2 border-gray-100 py-2 outline-none font-bold text-gray-700 bg-transparent" value={selectedBranch.businessType || ''} readOnly />
                   </div>
                </div>

                <div className="pt-10 flex items-center justify-between">
                   <div className="flex items-center text-gray-400 text-xs italic">
                      <Info size={14} className="mr-2" /> Dữ liệu này được đọc trực tiếp từ Quản lý chi nhánh.
                   </div>
                   <button type="button" onClick={() => navigate(`/branches?branchId=${encodeURIComponent(selectedBranch.id || '')}`)} className="bg-blue-600 text-white px-6 py-3 rounded-full font-black shadow-xl shadow-blue-500/20 hover:bg-blue-700 transition-all active:scale-95 uppercase tracking-widest text-xs">Chỉnh sửa thông tin chi nhánh</button>
                </div>
             </div>
          </div>
        );

      case 'loyalty':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto space-y-6">
                <div className="flex items-center space-x-3 mb-8 border-b pb-4">
                   <div className="p-2 bg-pink-50 text-pink-600 rounded-lg"><Users2 size={20}/></div>
                   <h3 className="text-lg font-black uppercase text-gray-800 tracking-tight">Chương trình Khách hàng thân thiết</h3>
                </div>
                <div className="grid grid-cols-1 gap-4">
                   <ToggleItem title="Cho phép tích điểm" desc="Khách hàng được tích lũy điểm dựa trên giá trị hóa đơn để đổi quà hoặc giảm giá." settingKey="loyaltyEnabled" />
                   <div className="p-4 border rounded-xl border-gray-100 bg-gray-50/50 space-y-3">
                      <p className="text-[11px] font-black text-gray-400 uppercase tracking-widest">Tỷ lệ quy đổi điểm</p>
                      <div className="flex items-center space-x-4">
                         <div className="flex-1 bg-white p-3 rounded-lg border border-gray-100 font-bold text-sm">10.000 VNĐ</div>
                         <ChevronRight size={16} className="text-gray-300" />
                         <div className="flex-1 bg-white p-3 rounded-lg border border-gray-100 font-bold text-sm text-pink-600">1 Điểm</div>
                      </div>
                   </div>
                   <ToggleItem title="Tự động nâng hạng thành viên" desc="Hệ thống tự động chuyển hạng khách hàng (Bạc, Vàng, Kim cương) khi đạt ngưỡng chi tiêu." settingKey="autoRank" />
                   <ToggleItem title="Gửi tin nhắn cảm ơn sau mua hàng" desc="Tự động gửi thông báo qua Zalo/SMS kèm số điểm vừa tích lũy cho khách." settingKey="loyaltyMessages" />
                </div>
             </div>
          </div>
        );

      case 'menu-mgmt':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto space-y-6">
                <div className="flex items-center space-x-3 mb-8 border-b pb-4">
                   <div className="p-2 bg-blue-50 text-blue-600 rounded-lg"><ShoppingBag size={20}/></div>
                   <h3 className="text-lg font-black uppercase text-gray-800 tracking-tight">Quy tắc bán hàng</h3>
                </div>
                <div className="grid grid-cols-1 gap-4">
                   <ToggleItem title="Quản lý theo bảng giá" desc="Sử dụng nhiều bảng giá khác nhau cho từng nhóm khách hàng hoặc khung giờ vàng." settingKey="priceBookEnabled" />
                   <ToggleItem title="Cho phép trả hàng" desc="Người dùng có thể tạo phiếu trả hàng từ hóa đơn đã thanh toán." settingKey="allowReturn" />
                   <ToggleItem title="Cảnh báo khi hết hàng" desc="Hiển thị thông báo trên màn hình POS khi sản phẩm trong kho sắp hết." settingKey="stockWarning" />
                   {userRole === 'admin' && (
                     <div className="pt-6 flex justify-end">
                        <button
                          onClick={handleSave}
                          disabled={isSaving}
                          className="bg-blue-600 text-white px-8 py-2 rounded-xl font-bold text-xs uppercase tracking-widest hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/20 disabled:opacity-50 flex items-center"
                        >
                          {isSaving && <Loader2 size={12} className="animate-spin mr-2" />}
                          Lưu thiết lập
                        </button>
                     </div>
                   )}
                </div>
             </div>
          </div>
        );

      case 'orders':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto space-y-6">
                <div className="flex items-center space-x-3 mb-8 border-b pb-4">
                   <div className="p-2 bg-purple-50 text-purple-600 rounded-lg"><ClipboardList size={20}/></div>
                   <h3 className="text-lg font-black uppercase text-gray-800 tracking-tight">Thuế & phí dịch vụ</h3>
                </div>
                <div className="grid grid-cols-1 gap-4">
                   <div className="p-4 border rounded-xl border-gray-100 bg-gray-50/50 space-y-4">
                      <p className="text-[11px] font-black text-gray-400 uppercase tracking-widest">Thuế & Phí dịch vụ</p>
                      <div className="grid grid-cols-2 gap-6">
                         <div className="space-y-1">
                            <label className="text-[10px] font-bold text-gray-500 uppercase">VAT (%)</label>
                            <input
                              type="number"
                              className="w-full bg-white border border-gray-200 rounded-lg px-3 py-2 font-bold text-sm outline-none focus:border-purple-500"
                              value={settings.vatPercent}
                              onChange={(e) => setSettings({...settings, vatPercent: parseInt(e.target.value) || 0})}
                            />
                         </div>
                         <div className="space-y-1">
                            <label className="text-[10px] font-bold text-gray-500 uppercase">Phí phục vụ (%)</label>
                            <input
                              type="number"
                              className="w-full bg-white border border-gray-200 rounded-lg px-3 py-2 font-bold text-sm outline-none focus:border-purple-500"
                              value={settings.serviceFee}
                              onChange={(e) => setSettings({...settings, serviceFee: parseInt(e.target.value) || 0})}
                            />
                         </div>
                      </div>
                   </div>
                   {userRole === 'admin' && (
                     <div className="pt-6 flex justify-end">
                        <button
                          onClick={handleSave}
                          disabled={isSaving}
                          className="bg-blue-600 text-white px-8 py-2 rounded-xl font-bold text-xs uppercase tracking-widest hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/20 disabled:opacity-50 flex items-center"
                        >
                          {isSaving && <Loader2 size={12} className="animate-spin mr-2" />}
                          Lưu thiết lập
                        </button>
                     </div>
                   )}
                </div>
             </div>
          </div>
        );

      case 'qr-payment':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto space-y-8">
                <div className="flex items-center space-x-3 mb-4 border-b pb-4">
                   <div className="p-2 bg-blue-50 text-blue-600 rounded-lg"><QrCode size={20}/></div>
                   <h3 className="text-lg font-black uppercase text-gray-800 tracking-tight">Thanh toán mã QR (VietQR)</h3>
                </div>

                <div className="bg-blue-50 p-6 rounded-2xl flex items-start space-x-4 border border-blue-100">
                   <div className="p-3 bg-white rounded-xl shadow-sm text-blue-600"><Smartphone size={32}/></div>
                   <div>
                      <p className="font-bold text-blue-800 mb-1 text-sm">Tính năng quét QR động chuyên nghiệp</p>
                      <p className="text-xs text-blue-600/80 leading-relaxed italic font-medium">Hệ thống tự tạo QR kèm số tiền và mã đơn. Khách chỉ cần quét và bấm chuyển tiền, không thể nhập sai số tiền.</p>
                   </div>
                </div>

                <div className="space-y-6 pt-4">
                   <ToggleItem title="Bật thanh toán QR động" desc="Tạo mã QR riêng cho từng đơn hàng bao gồm cả mã đơn trong nội dung." settingKey="qrPaymentEnabled" />

                   <div className="p-6 border rounded-xl border-gray-100 space-y-4">
                      <p className="font-bold text-[11px] text-gray-400 uppercase tracking-widest">Cấu hình nội dung chuyển khoản</p>
                      <div className="flex items-center space-x-2 bg-gray-50 p-3 rounded-lg border border-dashed border-gray-300">
                         <span className="text-xs font-bold text-gray-700">THANHTOAN</span>
                         <span className="text-xs font-black text-blue-600 bg-blue-100 px-2 py-0.5 rounded">[MA_DON_HANG]</span>
                         <span className="text-xs font-bold text-gray-700">CN</span>
                         <span className="text-xs font-black text-orange-600 bg-orange-100 px-2 py-0.5 rounded">[TEN_CHI_NHANH]</span>
                      </div>
                      <p className="text-[10px] text-gray-400 italic">* Nội dung này sẽ tự động hiển thị trên ứng dụng ngân hàng của khách hàng.</p>
                   </div>
                </div>
             </div>
          </div>
        );

      case 'reports':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto space-y-6">
                <div className="flex items-center space-x-3 mb-8 border-b pb-4">
                   <div className="p-2 bg-green-50 text-green-600 rounded-lg"><BarChart3 size={20}/></div>
                   <h3 className="text-lg font-black uppercase text-gray-800 tracking-tight">Cấu hình Báo cáo doanh thu</h3>
                </div>
                <div className="grid grid-cols-1 gap-4">
                   <ToggleItem title="Gửi báo cáo doanh thu cuối ngày" desc="Tự động tổng hợp và gửi báo cáo qua email cho chủ quản sau khi khóa sổ." settingKey="dailyRevenueReport" />
                   <div className="p-4 border rounded-xl border-gray-100 bg-gray-50/50 space-y-3">
                      <p className="text-[11px] font-black text-gray-400 uppercase tracking-widest">Email nhận báo cáo</p>
                      <input type="email" className="w-full bg-white border border-gray-200 rounded-lg px-3 py-2 font-bold text-sm outline-none focus:border-green-500" defaultValue="admin@restaurant.com" />
                   </div>
                   <ToggleItem title="Hiển thị doanh thu trên màn hình POS" desc="Cho phép thu ngân xem nhanh tổng tiền bán được trong ca làm việc." settingKey="showRevenuePos" />
                   <ToggleItem title="Ẩn giá vốn trên báo cáo nhân viên" desc="Đảm bảo tính bảo mật, nhân viên chỉ nhìn thấy doanh thu, không thấy lợi nhuận." settingKey="hideCostForStaff" />
                </div>
             </div>
          </div>
        );

      case 'lock-book':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto space-y-8 text-center py-10">
                <div className="w-20 h-20 bg-gray-50 rounded-full flex items-center justify-center mx-auto mb-6 text-gray-400 border-2 border-dashed border-gray-200">
                   <Lock size={32} />
                </div>
                <div>
                   <h3 className="text-xl font-black uppercase text-gray-800 tracking-tight">Chốt sổ & Khóa dữ liệu</h3>
                   <p className="text-sm text-gray-400 max-w-md mx-auto mt-2">Ngăn chặn việc sửa đổi dữ liệu hóa đơn, nhập kho sau khi đã chốt báo cáo ngày hoặc tháng.</p>
                </div>

                <div className="flex justify-center space-x-4 mt-8">
                   <button className="px-8 py-3 border border-gray-200 rounded-full font-bold text-xs text-gray-600 hover:bg-gray-50 transition-all uppercase tracking-widest">Khóa sổ hôm nay</button>
                   <button className="px-8 py-3 bg-gray-800 text-white rounded-full font-bold text-xs hover:bg-black transition-all uppercase tracking-widest shadow-lg">Cấu hình tự động khóa</button>
                </div>
             </div>
          </div>
        );

      case 'delete-trial':
        return (
          <div className="p-8 bg-white flex-1 overflow-y-auto">
             <div className="max-w-3xl mx-auto border-2 border-red-50 border-dashed rounded-3xl p-10 text-center">
                <div className="w-16 h-16 bg-red-50 text-red-500 rounded-2xl flex items-center justify-center mx-auto mb-6">
                   <AlertTriangle size={32} />
                </div>
                <h3 className="text-lg font-black uppercase text-red-600 tracking-tight mb-2">Xóa toàn bộ dữ liệu dùng thử</h3>
                <p className="text-sm text-gray-500 mb-8 max-w-sm mx-auto leading-relaxed italic font-medium">Hành động này sẽ xóa sạch: Sản phẩm, Hóa đơn, Nhân viên và các cài đặt để bạn bắt đầu kinh doanh thật sự. <br/><span className="font-bold text-red-500 underline">Thao tác này không thể hoàn tác!</span></p>

                <button className="px-12 py-3 bg-red-600 text-white rounded-full font-black shadow-xl shadow-red-600/20 hover:bg-red-700 transition-all active:scale-95 uppercase tracking-widest text-xs">
                   Xác nhận xóa dữ liệu
                </button>
             </div>
          </div>
        );

      default:
        return (
          <div className="flex-1 flex flex-col items-center justify-center bg-white text-gray-400 p-10 text-center">
             <div className="p-8 bg-gray-50 rounded-full mb-6 border-2 border-dashed border-gray-100">
                <Database size={64} className="text-gray-200" />
             </div>
             <h3 className="text-lg font-black text-gray-600 uppercase tracking-tighter">Dữ liệu đang được đồng bộ</h3>
             <p className="text-xs max-w-xs mt-3 leading-relaxed italic font-medium">
                Thiết lập này chưa có nội dung khả dụng. Vui lòng quay lại trang cấu hình chung.
             </p>
             <button onClick={() => setActiveSubTab('store-info')} className="mt-8 text-[11px] font-bold text-blue-600 uppercase tracking-widest hover:underline">Quay lại trang chủ thiết lập</button>
          </div>
        );
    }
  };

  return (
    <div className="relative min-w-0 bg-[#f0f2f5] text-gray-700 font-sans">

      {/* Toast Success Message */}
      {showSaveSuccess && (
        <div className="absolute top-4 right-4 bg-green-600 text-white px-6 py-3 rounded-xl shadow-2xl z-[200] flex items-center font-bold animate-in slide-in-from-top-4 duration-300">
           <CheckCircle2 size={18} className="mr-2"/> ĐÃ LƯU THIẾT LẬP THÀNH CÔNG!
        </div>
      )}

      <div className="flex items-center gap-2 overflow-x-auto border-b border-slate-200 bg-white px-4 py-3 sm:px-6">
        <button type="button" onClick={() => setActiveSubTab('store-info')} className={`shrink-0 rounded-lg px-3 py-2 text-sm font-semibold ${activeSubTab === 'store-info' ? 'bg-blue-600 text-white' : 'text-slate-600 hover:bg-slate-100'}`}>Thông tin nhà hàng</button>
        <button type="button" onClick={() => setActiveSubTab('orders')} className={`shrink-0 rounded-lg px-3 py-2 text-sm font-semibold ${activeSubTab === 'orders' ? 'bg-blue-600 text-white' : 'text-slate-600 hover:bg-slate-100'}`}>Thuế & phí dịch vụ</button>
        <button type="button" onClick={() => void refreshSettings()} disabled={isLoading || loadingBranch} className="ml-auto inline-flex shrink-0 items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm font-semibold text-slate-600 hover:bg-slate-100 disabled:opacity-60"><RefreshCw size={15} className={isLoading || loadingBranch ? 'animate-spin' : ''} /> Làm mới</button>
      </div>

      {/* Content Area */}
      <div className="min-w-0 overflow-hidden">
        {renderContent()}
      </div>
    </div>
  );
};

export default SystemSettings;

