import { Banknote, CheckCircle2, Loader2, QrCode, X } from 'lucide-react';
import type { Branch } from '../types';

export interface POSPaymentCustomer {
  id?: string;
  phone: string;
  name: string;
  email?: string;
  loyaltyPoints?: number;
  lookupStatus?: 'found' | 'not-found' | 'error';
  lookupError?: string;
  memberConsent?: boolean;
}

interface POSPaymentModalProps {
  totalAmount: number;
  previewTotal: number;
  hasAuthoritativeSnapshot: boolean;
  financialSnapshot: {
    subTotal?: number | null;
    serviceFeeAmount?: number | null;
    serviceFeePercent?: number | null;
    vatAmount?: number | null;
    vatPercent?: number | null;
    discount?: number | null;
  } | null;
  isPreparingPayment: boolean;
  preparationError: string | null;
  estimatedEarnedPoints: number;
  currentCartLength: number;
  currentCustomer: POSPaymentCustomer;
  isCheckingPhone: boolean;
  isSaving: boolean;
  pointsToRedeem: number;
  paymentMethod: 'Tiền mặt' | 'Chuyển khoản';
  branchInfo?: Branch | null;
  branchPaymentInfoError: string | null;
  qrImageError: boolean;
  qrImageUrl: string | null;
  onClose: () => void;
  onPhoneChange: (value: string) => void;
  onNameChange: (value: string) => void;
  onRedeemPoints: () => void;
  onPointsChange: (value: number) => void;
  onMemberConsentChange: (checked: boolean) => void;
  onRetryCustomerLookup: () => void;
  onPaymentMethodChange: (method: 'Tiền mặt' | 'Chuyển khoản') => void;
  onConfirmPayment: () => void;
  onRetryPreparation: () => void;
  onQrImageLoad: () => void;
  onQrImageError: () => void;
}

export default function POSPaymentModal({
  totalAmount,
  previewTotal,
  hasAuthoritativeSnapshot,
  financialSnapshot,
  isPreparingPayment,
  preparationError,
  estimatedEarnedPoints,
  currentCartLength,
  currentCustomer,
  isCheckingPhone,
  isSaving,
  pointsToRedeem,
  paymentMethod,
  branchInfo,
  branchPaymentInfoError,
  qrImageError,
  qrImageUrl,
  onClose,
  onPhoneChange,
  onNameChange,
  onRedeemPoints,
  onPointsChange,
  onMemberConsentChange,
  onRetryCustomerLookup,
  onPaymentMethodChange,
  onConfirmPayment,
  onRetryPreparation,
  onQrImageLoad,
  onQrImageError
}: POSPaymentModalProps) {
  return (
    <div className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-start md:items-center p-2 sm:p-4 backdrop-blur-sm">
      <div className="bg-white w-full max-w-2xl max-h-[calc(100dvh-1rem)] md:max-h-[calc(100dvh-2rem)] rounded-2xl shadow-2xl overflow-y-auto flex flex-col md:flex-row animate-in zoom-in-95 duration-200">
        <div className="flex-1 p-4 sm:p-6 border-r">
          <div className="sticky top-0 z-10 -mx-4 sm:-mx-6 px-4 sm:px-6 py-3 mb-3 bg-white/95 backdrop-blur flex justify-between items-center border-b border-gray-100">
            <h3 className="font-black text-xl text-gray-800 uppercase italic tracking-tighter">Thanh toán hóa đơn</h3>
            <button type="button" onClick={onClose} aria-label="Đóng thanh toán" className="shrink-0 ml-3 p-2 rounded-full bg-gray-100 text-gray-600 hover:bg-red-100 hover:text-red-600"><X size={24}/></button>
          </div>

          <div className="bg-blue-50 p-4 rounded-xl mb-6">
            <div className="flex justify-between items-center mb-1">
              <span className="text-gray-500 text-xs font-bold uppercase">{hasAuthoritativeSnapshot ? 'Tổng thanh toán' : 'Tạm tính món'}</span>
              <span className="bg-blue-600 text-white text-[10px] px-2 py-0.5 rounded-full font-bold">VNĐ</span>
            </div>
            <div className="text-3xl font-black text-blue-700">{(hasAuthoritativeSnapshot ? totalAmount : previewTotal).toLocaleString()}</div>
            {!hasAuthoritativeSnapshot ? (
              <p className="mt-2 text-[10px] font-medium text-blue-700/70">Thuế, phí và giảm giá sẽ được máy chủ xác nhận trước khi thanh toán.</p>
            ) : (
              <div className="mt-3 space-y-1 border-t border-blue-100 pt-2 text-[10px] font-bold text-gray-600">
                <div className="flex justify-between"><span>Tạm tính</span><span>{financialSnapshot?.subTotal == null ? 'Chưa có dữ liệu' : financialSnapshot.subTotal.toLocaleString()}</span></div>
                <div className="flex justify-between"><span>Giảm giá</span><span>{financialSnapshot?.discount == null ? 'Chưa có dữ liệu' : `-${financialSnapshot.discount.toLocaleString()}`}</span></div>
                <div className="flex justify-between"><span>Phí dịch vụ{financialSnapshot?.serviceFeePercent == null ? '' : ` (${financialSnapshot.serviceFeePercent}%)`}</span><span>{financialSnapshot?.serviceFeeAmount == null ? 'Chưa có dữ liệu' : financialSnapshot.serviceFeeAmount.toLocaleString()}</span></div>
                <div className="flex justify-between"><span>VAT{financialSnapshot?.vatPercent == null ? '' : ` (${financialSnapshot.vatPercent}%)`}</span><span>{financialSnapshot?.vatAmount == null ? 'Chưa có dữ liệu' : financialSnapshot.vatAmount.toLocaleString()}</span></div>
              </div>
            )}
          </div>
          {preparationError && <div className="mb-4 rounded-xl border border-amber-200 bg-amber-50 p-3 text-[10px] font-bold text-amber-800"><p>{preparationError}</p><button type="button" onClick={onRetryPreparation} disabled={isPreparingPayment || isSaving} className="mt-2 underline disabled:opacity-50">Chuẩn bị lại tổng tiền</button></div>}

          <div className="bg-white border border-orange-100 rounded-xl p-4 mb-6 shadow-sm">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center">
                <span className="bg-orange-500 text-white text-[9px] font-black px-2 py-0.5 rounded mr-2 uppercase">Member</span>
                <h4 className="text-xs font-bold text-gray-700 uppercase tracking-wider">Tích điểm khách hàng</h4>
              </div>
              {currentCustomer.loyaltyPoints !== undefined && (
                <div className="text-[10px] font-black text-orange-600 bg-orange-50 px-2 py-0.5 rounded-full">ĐIỂM HIỆN CÓ: {currentCustomer.loyaltyPoints}</div>
              )}
            </div>
            <div className="space-y-3">
              <div className="relative">
                <input
                  type="text"
                  placeholder="Nhập số điện thoại khách..."
                  className="w-full pl-3 pr-10 py-2.5 bg-gray-50 border border-gray-100 rounded-lg text-sm outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-500 font-bold"
                  value={currentCustomer.phone}
                  onChange={event => onPhoneChange(event.target.value)}
                />
                {isCheckingPhone && <Loader2 size={16} className="absolute right-3 top-3 animate-spin text-orange-500" />}
              </div>
              {currentCustomer.phone && (
                <div className="animate-in slide-in-from-top-2 duration-300 space-y-3">
                  <input
                    type="text"
                    placeholder="Họ tên khách hàng..."
                    className="w-full px-3 py-2 bg-white border border-gray-100 rounded-lg text-xs outline-none focus:border-orange-300 font-medium"
                    value={currentCustomer.name}
                    onChange={event => onNameChange(event.target.value)}
                  />

                  {currentCustomer.lookupStatus === 'found' && currentCustomer.loyaltyPoints !== undefined && currentCustomer.loyaltyPoints > 0 && (
                    <div className="pt-2 border-t border-orange-50">
                      <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest block mb-2">Đổi điểm giảm giá</label>
                      <div className="flex space-x-2">
                        <input
                          type="number"
                          className="flex-1 px-3 py-2 bg-gray-50 border border-gray-100 rounded-lg text-xs font-black text-orange-600 outline-none focus:border-orange-500"
                          placeholder="Số điểm đổi..."
                          value={pointsToRedeem || ''}
                          max={currentCustomer.loyaltyPoints}
                          min={0}
                          onChange={event => onPointsChange(Number(event.target.value))}
                        />
                        <button type="button" onClick={onRedeemPoints} disabled={isSaving || pointsToRedeem <= 0} className="bg-orange-600 text-white px-4 py-2 rounded-lg text-[10px] font-black uppercase hover:bg-orange-700 transition-all disabled:opacity-50">Áp dụng</button>
                      </div>
                    </div>
                  )}

                  {currentCustomer.lookupStatus === 'not-found' && (
                    <label className="flex items-start gap-2 rounded-lg bg-amber-50 p-3 text-[10px] text-amber-800">
                      <input type="checkbox" checked={Boolean(currentCustomer.memberConsent)} onChange={event => onMemberConsentChange(event.target.checked)} />
                      <span>Khách đồng ý tạo tài khoản thành viên để nhận điểm.</span>
                    </label>
                  )}
                  {currentCustomer.lookupStatus === 'found' && <p className="text-[10px] text-emerald-600 font-bold">Đã xác nhận: {currentCustomer.name} sẽ nhận điểm sau khi thanh toán thành công.</p>}
                  {currentCustomer.lookupStatus === 'not-found' && <p className="text-[10px] text-amber-600 font-medium">Chưa có tài khoản. Nếu không xác nhận tạo mới, đơn sẽ là khách vãng lai và không nhận điểm.</p>}
                  {currentCustomer.lookupStatus === 'error' && <div className="flex items-center justify-between gap-2 text-[10px] text-red-600 font-medium"><p>{currentCustomer.lookupError}</p><button type="button" className="shrink-0 underline" onClick={onRetryCustomerLookup}>Thử lại</button></div>}
                  {currentCustomer.lookupStatus === 'found' && <p className="text-[10px] text-orange-600 italic font-medium">* {hasAuthoritativeSnapshot ? <>Hệ thống sẽ tích <b>+{estimatedEarnedPoints} điểm</b> mới khi hoàn tất thanh toán (1 điểm / 10.000đ).</> : 'Điểm tích lũy sẽ được xác nhận sau khi máy chủ trả về tổng thanh toán.'}</p>}
                </div>
              )}
            </div>
          </div>

          <div className="space-y-3">
            <p className="text-[11px] font-bold text-gray-400 uppercase mb-2">Phương thức thanh toán</p>
            <button type="button" disabled={isPreparingPayment || isSaving} onClick={() => onPaymentMethodChange('Tiền mặt')} className={`w-full p-4 rounded-xl border-2 flex items-center justify-between transition-all ${paymentMethod === 'Tiền mặt' ? 'border-blue-600 bg-blue-50 shadow-md' : 'border-gray-100 hover:border-blue-200'} disabled:cursor-not-allowed disabled:opacity-60`}>
              <div className="flex items-center"><div className={`p-2 rounded-lg mr-3 ${paymentMethod === 'Tiền mặt' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-400'}`}><Banknote size={24} /></div><div className="text-left"><p className="font-bold text-gray-800">Tiền mặt</p><p className="text-[10px] text-gray-400">Thanh toán trực tiếp tại quầy</p></div></div>
              {paymentMethod === 'Tiền mặt' && <CheckCircle2 size={20} className="text-blue-600" />}
            </button>
            <button type="button" disabled={isPreparingPayment || isSaving} onClick={() => onPaymentMethodChange('Chuyển khoản')} className={`w-full p-4 rounded-xl border-2 flex items-center justify-between transition-all ${paymentMethod === 'Chuyển khoản' ? 'border-blue-600 bg-blue-50 shadow-md' : 'border-gray-100 hover:border-blue-200'} disabled:cursor-not-allowed disabled:opacity-60`}>
              <div className="flex items-center"><div className={`p-2 rounded-lg mr-3 ${paymentMethod === 'Chuyển khoản' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-400'}`}><QrCode size={24} /></div><div className="text-left"><p className="font-bold text-gray-800">Chuyển khoản / QR</p><p className="text-[10px] text-gray-400">Quét mã VietQR nhanh chóng</p></div></div>
              {paymentMethod === 'Chuyển khoản' && <CheckCircle2 size={20} className="text-blue-600" />}
            </button>
          </div>

          <button type="button" onClick={onConfirmPayment} disabled={isSaving || isPreparingPayment || Boolean(preparationError) || !hasAuthoritativeSnapshot || currentCartLength === 0} className="w-full mt-8 bg-[#0070f4] text-white py-4 rounded-xl font-bold text-lg hover:bg-blue-700 transition-all shadow-lg active:scale-95 flex items-center justify-center disabled:cursor-not-allowed disabled:opacity-60">
            {isPreparingPayment ? <><Loader2 size={20} className="mr-2 animate-spin" />Đang chuẩn bị...</> : isSaving ? <><Loader2 size={20} className="mr-2 animate-spin" />Đang thanh toán...</> : 'Xác nhận thanh toán'}
          </button>
        </div>

        <div className={`w-full md:w-72 bg-gray-50 p-6 flex flex-col items-center justify-center transition-all ${paymentMethod === 'Chuyển khoản' ? 'opacity-100' : 'opacity-30 grayscale pointer-events-none'}`}>
          <p className="text-xs font-bold text-gray-400 uppercase mb-4">Mã QR Thanh toán</p>
          <div className="bg-white p-3 rounded-2xl shadow-inner border-2 border-dashed border-gray-200 mb-4 relative">
            {isPreparingPayment ? (
              <div className="w-48 h-48 flex flex-col items-center justify-center text-blue-500 text-center p-4"><Loader2 size={36} className="mb-2 animate-spin" /><p className="text-[10px]">Đang xác nhận tổng tiền...</p></div>
            ) : preparationError ? (
              <div className="w-48 h-48 flex flex-col items-center justify-center text-amber-600 text-center p-4"><QrCode size={42} className="mb-2 opacity-50" /><p className="text-[10px]">Chưa hiển thị QR khi tổng tiền chưa được xác nhận.</p></div>
            ) : !hasAuthoritativeSnapshot ? (
              <div className="w-48 h-48 flex flex-col items-center justify-center text-gray-400 text-center p-4"><QrCode size={42} className="mb-2 opacity-30" /><p className="text-[10px]">Đang chờ tổng tiền từ máy chủ.</p></div>
            ) : branchPaymentInfoError ? (
              <div className="w-48 h-48 flex flex-col items-center justify-center text-red-500 text-center p-4"><QrCode size={48} className="mb-2 opacity-40" /><p className="text-[10px]">{branchPaymentInfoError}</p></div>
            ) : branchInfo?.bankName && branchInfo?.accountNumber && !qrImageError ? (
              <img src={qrImageUrl || undefined} alt="VietQR" className="w-48 h-48 object-contain" onLoad={onQrImageLoad} onError={onQrImageError} />
            ) : qrImageError ? (
              <div className="w-48 h-48 flex flex-col items-center justify-center text-red-500 text-center p-4"><QrCode size={48} className="mb-2 opacity-40" /><p className="text-[10px]">Không thể tải ảnh VietQR. Vui lòng kiểm tra cấu hình ngân hàng hoặc kết nối mạng.</p></div>
            ) : (
              <div className="w-48 h-48 flex flex-col items-center justify-center text-gray-300 text-center p-4"><QrCode size={48} className="mb-2 opacity-20" /><p className="text-[10px]">Chưa thiết lập thông tin ngân hàng trong Quản lý chi nhánh</p></div>
            )}
          </div>
          <div className="text-center"><p className="font-bold text-sm text-gray-700">{branchInfo?.accountHolder || 'CHƯA CẬP NHẬT'}</p><p className="text-blue-600 font-black text-base">{branchInfo?.accountNumber || '---'}</p><p className="text-[10px] text-gray-400 font-bold uppercase">{branchInfo?.bankName || 'Ngân hàng'}</p></div>
          <p className="mt-6 text-[10px] text-gray-400 text-center italic">Khách hàng quét mã bằng ứng dụng Ngân hàng hoặc Ví điện tử để thanh toán.</p>
        </div>
      </div>
    </div>
  );
}
