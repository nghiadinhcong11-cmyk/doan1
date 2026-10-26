import React, { useEffect, useState } from 'react';

export interface ReceiptSettingsModel {
  paperWidth?: 58 | 80 | number;
  showLogo?: boolean;
  showAddress?: boolean;
  showPhone?: boolean;
  showStaff?: boolean;
  showPaymentMethod?: boolean;
  showOrderNote?: boolean;
  showThankYou?: boolean;
  showSupplier?: boolean;
  showWarehouse?: boolean;
  showUnit?: boolean;
  showUnitPrice?: boolean;
  showPreparedBy?: boolean;
  showApprovedBy?: boolean;
  showSignatures?: boolean;
  thankYouText?: string;
  fontFamily?: string;
  fontSize?: number;
}

export interface ReceiptBranch { name?: string; address?: string; phoneNumber?: string; taxCode?: string; imageUrl?: string; logoUrl?: string; }
export interface ReceiptOrderItem { productName?: string; quantity: number; unitPrice: number; options?: string | null; }
export interface ReceiptOrder {
  invoiceCode?: string; createdAt?: string; paymentAt?: string | null; customerName?: string; customerPhone?: string;
  totalAmount: number; subTotal?: number | null; vatAmount?: number | null; vatPercent?: number | null;
  serviceFeeAmount?: number | null; serviceFeePercent?: number | null; discount?: number | null;
  paidAmount?: number | null; paymentMethod?: string | null; tableName?: string | null; createdBy?: string;
  note?: string | null; status?: string | null; details?: ReceiptOrderItem[];
}

export type ReceiptStatusTone = 'waiting' | 'processing' | 'completed' | 'cancelled' | 'neutral' | 'paid' | 'unpaid' | 'partial';

export interface ReceiptStatusLabel {
  label: string;
  tone: ReceiptStatusTone;
  icon: string;
}

const normalized = (value?: string | null) => (value || '').trim().toLocaleLowerCase('vi-VN');

export const mapReceiptOrderStatus = (status?: string | null): ReceiptStatusLabel => {
  const value = normalized(status);
  if (['đang xử lý', 'processing'].includes(value)) return { label: status!.trim(), tone: 'processing', icon: '◷' };
  if (['pending', 'chờ xác nhận', 'chờ duyệt', 'đổi quà'].includes(value)) return { label: status!.trim(), tone: 'waiting', icon: '!' };
  if (['hoàn thành', 'completed'].includes(value)) return { label: status!.trim(), tone: 'completed', icon: '✓' };
  if (['đã hủy', 'cancelled', 'canceled'].includes(value)) return { label: status!.trim(), tone: 'cancelled', icon: '×' };
  return { label: status?.trim() || 'Không xác định', tone: 'neutral', icon: '?' };
};

export const mapReceiptPaymentStatus = (paidAmount?: number | null, totalAmount?: number | null): ReceiptStatusLabel => {
  const paid = paidAmount ?? 0;
  const total = totalAmount ?? 0;
  if (total > 0 && paid >= total) return { label: 'Đã thanh toán', tone: 'paid', icon: '✓' };
  if (paid > 0 && total > 0) return { label: 'Thanh toán một phần', tone: 'partial', icon: '◐' };
  if (total > 0) return { label: 'Chưa thanh toán', tone: 'unpaid', icon: '!' };
  return { label: 'Không xác định', tone: 'neutral', icon: '?' };
};

const statusClass: Record<ReceiptStatusTone, string> = {
  waiting: 'border-amber-200 bg-amber-50 text-amber-800',
  processing: 'border-blue-200 bg-blue-50 text-blue-800',
  completed: 'border-green-200 bg-green-50 text-green-800',
  cancelled: 'border-red-200 bg-red-50 text-red-800',
  neutral: 'border-slate-200 bg-slate-50 text-slate-700',
  paid: 'border-green-200 bg-green-50 text-green-800',
  unpaid: 'border-amber-200 bg-amber-50 text-amber-800',
  partial: 'border-blue-200 bg-blue-50 text-blue-800'
};

const money = (value: number | null | undefined) => (value ?? 0).toLocaleString('vi-VN');
const dateTime = (value?: string) => value ? new Date(value).toLocaleString('vi-VN') : '—';

const ReceiptPreview = ({ settings, branch, order, preview = false }: {
  settings: ReceiptSettingsModel;
  branch?: ReceiptBranch | null;
  order: ReceiptOrder;
  preview?: boolean;
}) => {
  const paperWidth = settings.paperWidth === 58 ? 58 : 80;
  const details = order.details || [];
  // Historical orders can lack financial snapshots. Do not infer subtotal from
  // total/discount because service fee and VAT may also be missing.
  const subTotal = order.subTotal ?? (preview ? order.totalAmount + (order.discount || 0) : null);
  const discount = order.discount;
  const orderStatus = mapReceiptOrderStatus(order.status);
  const paymentStatus = mapReceiptPaymentStatus(order.paidAmount, order.totalAmount);
  const logoUrl = branch?.logoUrl || branch?.imageUrl;
  const [logoError, setLogoError] = useState(false);

  useEffect(() => { setLogoError(false); }, [logoUrl]);

  return <div className="receipt-paper bg-white p-4 font-mono text-black shadow-lg" data-paper-width={paperWidth} style={{ width: paperWidth === 58 ? 230 : 300, fontSize: `${settings.fontSize || 11}px` }}>
    <div className="text-center">
      {settings.showLogo !== false && logoUrl && !logoError && <img src={logoUrl} alt="Logo nhà hàng" className="receipt-logo mx-auto mb-2 max-h-14 max-w-[80%] object-contain" onError={() => setLogoError(true)} />}
      <p className="text-sm font-bold">{branch?.name || (preview ? 'Tên nhà hàng' : '—')}</p>
      {settings.showAddress !== false && <p>{branch?.address || (preview ? 'Địa chỉ nhà hàng' : '—')}</p>}
      {settings.showPhone !== false && <p>ĐT: {branch?.phoneNumber || (preview ? 'Chưa cập nhật' : '—')}</p>}
      {branch?.taxCode && <p>MST: {branch.taxCode}</p>}
      <p className="mt-3 border-y border-dashed py-2 font-bold">PHIẾU TÍNH TIỀN</p>
      <p className="mt-1 font-bold">{order.invoiceCode || (preview ? 'HD000027' : '—')}</p>
    </div>
    <div className="my-3 flex flex-wrap justify-center gap-1 text-[0.9em] font-bold" aria-label="Trạng thái hóa đơn">
      <span className={`receipt-status inline-flex items-center gap-1 rounded border px-2 py-1 ${statusClass[orderStatus.tone]}`}><span aria-hidden="true">{orderStatus.icon}</span> Đơn: {orderStatus.label}</span>
      <span className={`receipt-status inline-flex items-center gap-1 rounded border px-2 py-1 ${statusClass[paymentStatus.tone]}`}><span aria-hidden="true">{paymentStatus.icon}</span> TT: {paymentStatus.label}</span>
    </div>
    <div className="my-3 space-y-1 text-left">
      <p>Thời gian: {dateTime(order.paymentAt || order.createdAt)}</p>
      <p>Bàn: {order.tableName || 'Mang về'}</p>
      {settings.showStaff !== false && <p>Nhân viên: {order.createdBy || (preview ? 'Nhân viên' : '—')}</p>}
      {order.customerName && <p>Khách hàng: {order.customerName}</p>}
    </div>
    <div className="my-3 space-y-1">
      {details.length > 0 ? details.map((item, index) => <div className="flex justify-between gap-2" key={`${item.productName || 'item'}-${index}`}><span className="min-w-0 flex-1">{item.productName || 'Món ăn'} ×{item.quantity}{item.options && <span className="block text-[0.9em]">+ {item.options}</span>}</span><span>{money(item.unitPrice * item.quantity)}</span></div>) : preview ? <><div className="flex justify-between gap-2"><span>Vũ heo nướng ×1</span><span>79.000</span></div><div className="flex justify-between gap-2"><span>Ốc hương sốt trứng ×1</span><span>89.000</span></div><div className="flex justify-between gap-2"><span>Nước suối ×2</span><span>30.000</span></div></> : <p>Không có chi tiết món.</p>}
    </div>
    <div className="border-t border-dashed pt-2 text-right">
      <p className="flex justify-between"><span>Tạm tính</span><span>{subTotal == null ? 'Chưa có thông tin' : money(subTotal)}</span></p>
      {discount != null && discount > 0 && <p className="flex justify-between"><span>Giảm giá</span><span>-{money(discount)}</span></p>}
      {order.serviceFeeAmount != null && order.serviceFeeAmount > 0 && <p className="flex justify-between"><span>Phí phục vụ ({order.serviceFeePercent || 0}%)</span><span>{money(order.serviceFeeAmount)}</span></p>}
      {order.vatAmount != null && order.vatAmount > 0 && <p className="flex justify-between"><span>VAT ({order.vatPercent || 0}%)</span><span>{money(order.vatAmount)}</span></p>}
      <p className="mt-1 flex justify-between border-t pt-1 text-sm font-bold"><span>TỔNG CỘNG</span><span>{money(order.totalAmount)}</span></p>
      <p className="flex justify-between"><span>Đã thanh toán</span><span>{money(order.paidAmount)}</span></p>
      {order.paidAmount != null && order.totalAmount > order.paidAmount && <p className="flex justify-between"><span>Còn phải trả</span><span>{money(order.totalAmount - order.paidAmount)}</span></p>}
    </div>
    {settings.showPaymentMethod !== false && <p className="mt-3 text-center">Thanh toán: {order.paymentMethod || 'Chưa thanh toán'}</p>}
    {settings.showOrderNote !== false && order.note && <p className="mt-2 border-t border-dashed pt-2 text-left">Ghi chú: {order.note}</p>}
    {settings.showThankYou !== false && <p className="mt-5 text-center">{settings.thankYouText || 'Cảm ơn quý khách và hẹn gặp lại!'}</p>}
  </div>;
};

export default ReceiptPreview;
