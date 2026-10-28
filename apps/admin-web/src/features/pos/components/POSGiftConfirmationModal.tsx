import { CheckCircle2, Gift, X } from 'lucide-react';
import type { POSPendingOrderRequest } from './POSPendingRequests';

interface Props {
  order: POSPendingOrderRequest | null;
  isSaving: boolean;
  onClose: () => void;
  onConfirm: () => void;
  onReject: () => void;
}

const promotionIdFromOrder = (order: POSPendingOrderRequest) => {
  const options = order.details?.[0]?.options || '';
  return options.match(/(?:^|;)PromotionId=([^;]+)/i)?.[1] || '';
};

export { promotionIdFromOrder };

export default function POSGiftConfirmationModal({ order, isSaving, onClose, onConfirm, onReject }: Props) {
  if (!order) return null;
  const detail = order.details?.[0];
  const pointsText = detail?.options?.match(/Trừ\s+([\d.,]+)\s+điểm/i)?.[1] || '—';

  return <div className="fixed inset-0 z-[310] flex items-center justify-center bg-black/70 p-4 backdrop-blur-sm" onClick={event => { if (event.target === event.currentTarget && !isSaving) onClose(); }}>
    <div className="w-full max-w-lg overflow-hidden rounded-[2rem] bg-white shadow-2xl">
      <div className="flex items-center justify-between bg-pink-600 p-5 text-white">
        <div className="flex items-center gap-3"><Gift size={22} /><div><h3 className="font-black uppercase tracking-tight">Xác nhận đổi quà</h3><p className="text-[10px] font-bold uppercase tracking-widest text-pink-100">Không chuyển qua quy trình món ăn</p></div></div>
        <button type="button" onClick={onClose} disabled={isSaving} className="rounded-full p-2 hover:bg-pink-500 disabled:opacity-50"><X size={20} /></button>
      </div>
      <div className="space-y-4 p-6">
        <div className="rounded-2xl border border-pink-100 bg-pink-50 p-4">
          <p className="text-[10px] font-black uppercase tracking-widest text-pink-500">Khách hàng</p>
          <p className="mt-1 text-lg font-black text-gray-800">{order.customerName || 'Thành viên'}</p>
          <p className="mt-3 text-sm font-black text-pink-700">{detail?.productName || 'Quà tặng'}</p>
          <p className="mt-1 text-xs font-bold text-gray-500">Điểm sử dụng: <span className="text-gray-800">{pointsText}</span></p>
        </div>
        <p className="text-xs leading-5 text-gray-500">Khi xác nhận, hệ thống sẽ kiểm tra lại chương trình và số điểm hiện tại rồi mới trừ điểm. Nếu từ chối, yêu cầu sẽ được hủy và khách không bị trừ điểm.</p>
      </div>
      <div className="flex items-center justify-between border-t bg-gray-50 p-5">
        <button type="button" onClick={onReject} disabled={isSaving} className="rounded-xl bg-red-50 px-4 py-2.5 text-xs font-black uppercase text-red-600 hover:bg-red-600 hover:text-white disabled:opacity-50">Từ chối</button>
        <button type="button" onClick={onConfirm} disabled={isSaving || !promotionIdFromOrder(order) || !order.customerId} className="inline-flex items-center gap-2 rounded-xl bg-pink-600 px-5 py-2.5 text-xs font-black uppercase text-white hover:bg-pink-700 disabled:cursor-not-allowed disabled:opacity-50"><CheckCircle2 size={16} />{isSaving ? 'Đang xử lý...' : 'Xác nhận đổi quà'}</button>
      </div>
    </div>
  </div>;
}
