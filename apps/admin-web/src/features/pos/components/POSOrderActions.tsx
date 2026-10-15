import { Loader2, MoreVertical, RotateCcw, Utensils } from 'lucide-react';
import type { POSUserRole } from '../types';

interface POSOrderActionsProps {
  userName?: string;
  userRole?: POSUserRole;
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
  currentCartLength: number;
  isSendingToKitchen: boolean;
  isSaving: boolean;
  kitchenMessage: { type: 'success' | 'error'; text: string } | null;
  onSendToKitchen: () => void;
  onPayment: () => void;
}

const POSOrderActions = ({
  userName,
  userRole,
  totalAmount,
  previewTotal,
  hasAuthoritativeSnapshot,
  financialSnapshot,
  currentCartLength,
  isSendingToKitchen,
  isSaving,
  kitchenMessage,
  onSendToKitchen,
  onPayment
}: POSOrderActionsProps) => (
  <div className="p-4 border-t bg-white shadow-[0_-10px_15px_-3px_rgba(0,0,0,0.1)]">
    <div className="flex justify-between items-center mb-4">
      <div className="flex items-center">
        <span className="text-xs font-bold text-gray-500 mr-2">{userName || 'Nhân viên'}</span>
        <MoreVertical size={14} className="text-gray-400 cursor-pointer" />
      </div>
      <div className="flex items-center space-x-2">
        <span className="text-sm font-bold text-gray-500">{hasAuthoritativeSnapshot ? 'Tổng thanh toán' : 'Tạm tính món'}</span>
        <div className="flex items-center">
          <span className="bg-blue-600 text-white rounded-full w-4 h-4 flex items-center justify-center text-[10px] mr-2 shadow-sm font-bold">?</span>
          <span className="text-2xl font-black text-blue-700 tracking-tighter">{(hasAuthoritativeSnapshot ? totalAmount : previewTotal).toLocaleString()}</span>
        </div>
      </div>
    </div>
    {!hasAuthoritativeSnapshot ? (
      <p className="-mt-2 mb-3 text-right text-[10px] font-medium text-gray-400">Thuế, phí và giảm giá sẽ được xác nhận trước thanh toán.</p>
    ) : (
      <div className="-mt-2 mb-3 grid grid-cols-2 gap-x-4 gap-y-1 text-right text-[10px] font-bold text-gray-500 sm:grid-cols-4">
        <span>Tạm tính: {financialSnapshot?.subTotal == null ? '—' : financialSnapshot.subTotal.toLocaleString()}</span>
        <span>Giảm giá: {financialSnapshot?.discount == null ? '—' : `-${financialSnapshot.discount.toLocaleString()}`}</span>
        <span>Phí: {financialSnapshot?.serviceFeeAmount == null ? '—' : financialSnapshot.serviceFeeAmount.toLocaleString()}</span>
        <span>VAT: {financialSnapshot?.vatAmount == null ? '—' : financialSnapshot.vatAmount.toLocaleString()}</span>
      </div>
    )}

    <div className="grid grid-cols-12 gap-3">
      <button
        onClick={onSendToKitchen}
        disabled={isSendingToKitchen || currentCartLength === 0}
        className={`${userRole === 'employee' ? 'col-span-12' : 'col-span-4'} py-4 rounded-xl font-bold flex flex-col items-center justify-center transition-all shadow-lg active:scale-[0.98] border-b-4 ${
          'bg-orange-500 text-white hover:bg-orange-600 border-orange-700 shadow-orange-500/20'
        }`}
      >
        {isSendingToKitchen ? <Loader2 size={20} className="animate-spin" /> : <Utensils size={20} />}
        <span className="text-[10px] uppercase mt-1 font-black">Gửi bếp</span>
      </button>

      {userRole !== 'employee' && <button
        onClick={onPayment}
        disabled={isSaving || currentCartLength === 0}
        className="col-span-8 bg-[#0070f4] text-white py-4 rounded-xl font-bold flex items-center justify-center space-x-3 hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/30 active:scale-[0.98] border-b-4 border-blue-800 disabled:cursor-not-allowed disabled:opacity-50"
      >
        <span className="bg-white/20 p-2 rounded-full"><RotateCcw size={24} className="rotate-90"/></span>
        <span className="text-xl uppercase tracking-tighter font-black italic">Thanh toán</span>
      </button>}
    </div>
    {kitchenMessage && (
      <div
        role="status"
        className={`mt-3 rounded-lg px-3 py-2 text-center text-xs font-bold ${kitchenMessage.type === 'success' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}
      >
        {kitchenMessage.text}
      </div>
    )}
  </div>
);

export default POSOrderActions;
