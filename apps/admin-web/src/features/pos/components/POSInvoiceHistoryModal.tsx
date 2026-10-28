import { ClipboardList, ChevronRight, History, Loader2, Printer, RotateCcw, Search, Star, X } from 'lucide-react';
import ReceiptPreview, { ReceiptSettingsModel } from '../../../components/ReceiptPreview';
import type { Branch } from '../types';

interface InvoiceDetail {
  productName: string;
  quantity: number;
  unitPrice: number;
  options?: string;
}

export interface POSInvoiceRecord {
  id: string;
  invoiceCode: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
  paymentAt?: string;
  createdAt: string;
  paymentMethod?: string;
  tableName?: string;
  customerName?: string;
  customerPhone?: string;
  branchId: string;
  branchName?: string;
  subTotal?: number;
  discount?: number;
  serviceFeeAmount?: number;
  serviceFeePercent?: number;
  vatAmount?: number;
  vatPercent?: number;
  createdBy?: string;
  details?: InvoiceDetail[];
}

export interface POSOrderLoyaltyInfo {
  pointsEarned: number;
  pointsRedeemed: number;
  balanceAfter: number;
}

interface POSInvoiceHistoryModalProps {
  open: boolean;
  invoices: POSInvoiceRecord[];
  loading: boolean;
  search: string;
  fromDate: string;
  toDate: string;
  statusFilter: string;
  selectedOrder: POSInvoiceRecord | null;
  receiptSettings: ReceiptSettingsModel;
  allBranches: Branch[];
  loyaltyInfo: POSOrderLoyaltyInfo | null;
  onSearchChange: (value: string) => void;
  onFromDateChange: (value: string) => void;
  onToDateChange: (value: string) => void;
  onStatusFilterChange: (value: string) => void;
  onRefresh: () => void;
  onPrint: (order?: POSInvoiceRecord) => void;
  onClose: () => void;
  onSelectOrder: (order: POSInvoiceRecord | null) => void;
}

const statusOptions = [
  { id: 'all', label: 'Tất cả' },
  { id: 'Đang xử lý', label: 'Xử lý' },
  { id: 'Đổi quà', label: 'Đổi quà' },
  { id: 'Hoàn thành', label: 'Xong' },
  { id: 'Đã hủy', label: 'Đã hủy' }
];

const isGiftOrder = (order: POSInvoiceRecord) =>
  order.totalAmount === 0 && order.details?.some(detail => detail.productName?.startsWith('[ĐỔI QUÀ]')) === true;

export default function POSInvoiceHistoryModal({
  open,
  invoices,
  loading,
  search,
  fromDate,
  toDate,
  statusFilter,
  selectedOrder,
  receiptSettings,
  allBranches,
  loyaltyInfo,
  onSearchChange,
  onFromDateChange,
  onToDateChange,
  onStatusFilterChange,
  onRefresh,
  onPrint,
  onClose,
  onSelectOrder
}: POSInvoiceHistoryModalProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 bg-black/80 z-[600] flex justify-center items-center p-4 backdrop-blur-md">
      <div className="bg-white w-full max-w-4xl h-[80vh] rounded-[2.5rem] shadow-2xl overflow-hidden flex flex-col animate-in zoom-in-95 duration-200">
        <div className="bg-[#1e293b] p-6 text-white flex justify-between items-center">
          <div className="flex items-center space-x-3"><div className="p-3 bg-white/10 rounded-2xl"><History size={24} /></div><div><h3 className="font-black text-xl uppercase italic tracking-tighter">Lịch sử hóa đơn</h3><p className="text-[10px] font-bold opacity-60 uppercase tracking-widest">Tra cứu các giao dịch đã thực hiện</p></div></div>
          <button onClick={onClose} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
        </div>

        <div className="p-6 bg-gray-50 border-b flex flex-col md:flex-row md:items-center gap-4">
          <div className="relative flex-1"><Search className="absolute left-4 top-3 text-gray-400" size={18} /><input type="text" placeholder="Mã hóa đơn hoặc tên khách..." className="w-full pl-12 pr-4 py-3 bg-white border border-gray-200 rounded-2xl text-sm font-bold outline-none focus:ring-2 focus:ring-blue-500/20" value={search} onChange={(event) => onSearchChange(event.target.value)} /></div>
          <div className="flex bg-white p-1 rounded-2xl border border-gray-200">{statusOptions.map((status) => <button key={status.id} onClick={() => onStatusFilterChange(status.id)} className={`px-4 py-2 rounded-xl text-[10px] font-black uppercase transition-all ${statusFilter === status.id ? 'bg-blue-600 text-white shadow-md' : 'text-gray-400'}`}>{status.label}</button>)}</div>
          <div className="flex items-center space-x-3 bg-white p-1 rounded-2xl border border-gray-200">
            <div className="flex items-center px-3 border-r"><span className="text-[9px] font-black text-gray-400 uppercase mr-2">Từ:</span><input type="date" className="text-[11px] font-bold text-gray-700 outline-none border-none bg-transparent" value={fromDate} onChange={(event) => onFromDateChange(event.target.value)} /></div>
            <div className="flex items-center px-3"><span className="text-[9px] font-black text-gray-400 uppercase mr-2">Đến:</span><input type="date" className="text-[11px] font-bold text-gray-700 outline-none border-none bg-transparent" value={toDate} onChange={(event) => onToDateChange(event.target.value)} /></div>
          </div>
          <button onClick={onRefresh} className="bg-white p-3 rounded-2xl border border-gray-200 hover:bg-gray-100 transition-all"><RotateCcw size={20} className="text-gray-400" /></button>
        </div>

        <div className="flex-1 overflow-auto p-6">
          {loading ? <div className="flex justify-center py-20"><Loader2 className="animate-spin text-blue-600" /></div> : invoices.length === 0 ? <div className="text-center py-20 text-gray-400 font-bold uppercase text-xs tracking-widest">Không tìm thấy hóa đơn nào</div> : (
            <div className="space-y-4">{invoices.map((invoice) => (
              <div key={invoice.id} onClick={() => onSelectOrder(invoice)} className="bg-white p-5 rounded-[2rem] border border-gray-100 shadow-sm hover:shadow-md cursor-pointer transition-all flex flex-col md:flex-row md:items-center justify-between gap-4 group">
                <div className="flex items-center space-x-4"><div className="p-3 bg-blue-50 rounded-2xl text-blue-600 group-hover:bg-blue-600 group-hover:text-white transition-colors"><ClipboardList size={24} /></div><div><p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-0.5">{invoice.invoiceCode}<span className={`ml-2 px-1.5 py-0.5 rounded text-[8px] font-black uppercase ${invoice.status === 'Đã hủy' ? 'bg-red-100 text-red-600' : (invoice.status === 'Hoàn thành' || (invoice.totalAmount > 0 && invoice.paidAmount >= invoice.totalAmount)) ? 'bg-green-100 text-green-600' : 'bg-blue-100 text-blue-600'}`}>{(invoice.totalAmount > 0 && invoice.paidAmount >= invoice.totalAmount) ? 'Hoàn thành' : invoice.status}</span></p><h4 className="font-black text-gray-800 text-sm uppercase tracking-tight">{invoice.tableName || 'Mang về'}</h4><p className="text-[11px] text-gray-500 font-medium">Khách: <span className="text-blue-600 font-bold">{invoice.customerName}</span></p></div></div>
                <div className="grid grid-cols-2 md:grid-cols-3 gap-8 flex-1 md:max-w-md text-right md:text-left"><div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Thời gian</p><p className="text-xs font-bold text-gray-700">{new Date(invoice.paymentAt || invoice.createdAt).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})}</p></div><div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Phương thức</p><span className={`text-[10px] font-black px-2 py-0.5 rounded-full uppercase ${invoice.paymentMethod === 'Tiền mặt' ? 'bg-orange-100 text-orange-600' : 'bg-purple-100 text-purple-600'}`}>{invoice.paymentMethod || 'Chưa thanh toán'}</span></div><div><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Tổng tiền</p><p className="text-sm font-black text-blue-700 tracking-tighter">{invoice.totalAmount.toLocaleString()}đ</p></div></div>
                <div className="flex items-center space-x-2"><button onClick={(event) => { event.stopPropagation(); onPrint(invoice); }} title="In hóa đơn" aria-label="In hóa đơn" className="p-2.5 bg-gray-50 text-gray-400 hover:text-blue-600 rounded-xl transition-all"><Printer size={18} /></button><div className="p-2.5 bg-blue-600 text-white rounded-xl shadow-lg shadow-blue-500/20 group-hover:scale-110 transition-all"><ChevronRight size={18} /></div></div>
              </div>
            ))}</div>
          )}
        </div>

        {selectedOrder && (
          <div className="absolute inset-0 bg-white z-[650] flex flex-col animate-in slide-in-from-right duration-300">
            <div className="bg-[#1e293b] p-6 text-white flex justify-between items-center print-hide"><button onClick={() => onSelectOrder(null)} className="flex items-center text-xs font-bold uppercase tracking-widest opacity-60 hover:opacity-100"><X size={18} className="mr-2" /> Quay lại danh sách</button><div className="text-right"><h3 className="font-semibold text-lg tracking-tight">Chi tiết hóa đơn</h3><p className="mt-1 text-[10px] font-medium text-white/60">{selectedOrder.invoiceCode}</p></div><button onClick={() => onPrint()} title="In hóa đơn" aria-label="In hóa đơn" className="bg-white/10 p-2 rounded-xl"><Printer size={20}/></button></div>
            <div id="invoice-print" data-paper-width={receiptSettings.paperWidth || 80} className="flex-1 overflow-auto p-8 flex flex-col items-center">
              <ReceiptPreview settings={receiptSettings} branch={allBranches.find(branch => branch.id === selectedOrder.branchId) || { name: selectedOrder.branchName }} documentType={isGiftOrder(selectedOrder) ? 'GiftRedemption' : 'Invoice'} order={selectedOrder} />
              <div className="hidden">
                <div className="bg-white w-full max-w-2xl print:max-w-none">
                  <div className="grid grid-cols-2 gap-12 mb-10 pb-10 border-b border-gray-100"><div className="space-y-4"><div><p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Thông tin khách hàng</p><p className="text-lg font-black text-gray-800 uppercase italic">{selectedOrder.customerName}</p>{selectedOrder.customerPhone && <p className="text-xs font-bold text-blue-600 mt-1">{selectedOrder.customerPhone}</p>}</div><div className="flex space-x-8"><div><p className="text-[10px] font-semibold text-gray-400 uppercase tracking-widest mb-1">Lập hóa đơn</p><p className="text-sm font-bold text-gray-700">{new Date(selectedOrder.paymentAt || selectedOrder.createdAt).toLocaleString('vi-VN')}</p></div><div><p className="text-[10px] font-semibold text-gray-400 uppercase tracking-widest mb-1">Thanh toán</p><span className="text-xs font-semibold text-blue-600">{selectedOrder.paymentMethod || 'Chưa thanh toán'}</span></div></div></div><div className="text-right space-y-4"><div><p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Bàn / Vị trí</p><p className="text-lg font-black text-blue-600 uppercase italic">{selectedOrder.tableName || 'Mang về'}</p></div><div><p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Nhân viên thực hiện</p><p className="text-sm font-bold text-gray-700">{selectedOrder.createdBy || '---'}</p></div></div></div>
                  {loyaltyInfo && <div className="bg-orange-50/50 rounded-[2rem] p-6 mb-8 border border-orange-100/50 flex justify-between items-center shadow-sm print:rounded-none print:border-dashed"><div className="flex items-center"><div className="p-3 bg-orange-100 rounded-2xl text-orange-600 mr-4 print:hidden"><Star size={20} className="fill-orange-600" /></div><div><p className="text-[10px] font-black text-orange-600/60 uppercase tracking-widest">Hội viên tích điểm</p><p className="text-sm font-black text-gray-800 uppercase tracking-tight">Thành viên thân thiết</p></div></div><div className="flex space-x-12">{loyaltyInfo.pointsRedeemed > 0 && <div className="text-right"><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Điểm đã tiêu</p><p className="text-sm font-black text-red-600">-{loyaltyInfo.pointsRedeemed} PTS</p></div>}{loyaltyInfo.pointsEarned > 0 && <div className="text-right"><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Điểm tích mới</p><p className="text-sm font-black text-green-600">+{loyaltyInfo.pointsEarned} PTS</p></div>}<div className="text-right"><p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Số dư hiện tại</p><p className="text-sm font-black text-blue-700">{loyaltyInfo.balanceAfter} PTS</p></div></div></div>}
                  <div className="bg-gray-50 rounded-[3rem] p-8 border border-gray-100 shadow-inner print:bg-white print:rounded-none print:border-none print:shadow-none"><table className="w-full"><thead><tr className="border-b-2 border-gray-200"><th className="text-left pb-4 text-[11px] font-black text-gray-400 uppercase">Mặt hàng</th><th className="text-center pb-4 text-[11px] font-black text-gray-400 uppercase">SL</th><th className="text-right pb-4 text-[11px] font-black text-gray-400 uppercase">Đơn giá</th><th className="text-right pb-4 text-[11px] font-black text-gray-400 uppercase">Thành tiền</th></tr></thead><tbody className="divide-y divide-gray-100">{selectedOrder.details?.map((detail, index) => <tr key={index}><td className="py-5 text-sm font-black text-gray-800 capitalize">{detail.productName}</td><td className="py-5 text-center text-sm font-black text-blue-600">{detail.quantity}</td><td className="py-5 text-right text-xs font-bold text-gray-400">{detail.unitPrice.toLocaleString()}đ</td><td className="py-5 text-right text-sm font-black text-gray-800">{(detail.quantity * detail.unitPrice).toLocaleString()}đ</td></tr>)}</tbody></table><div className="mt-8 pt-8 border-t-2 border-dashed border-gray-200 space-y-3"><div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]"><span>Tạm tính</span><span className="text-gray-700">{(selectedOrder.subTotal ?? (selectedOrder.totalAmount + (selectedOrder.discount || 0))).toLocaleString()}đ</span></div>{selectedOrder.serviceFeeAmount != null && selectedOrder.serviceFeeAmount > 0 && <div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]"><span>Phí phục vụ ({selectedOrder.serviceFeePercent || 0}%)</span><span className="text-gray-700">{selectedOrder.serviceFeeAmount.toLocaleString()}đ</span></div>}{selectedOrder.vatAmount != null && selectedOrder.vatAmount > 0 && <div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]"><span>VAT ({selectedOrder.vatPercent || 0}%)</span><span className="text-gray-700">{selectedOrder.vatAmount.toLocaleString()}đ</span></div>}<div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]"><span>Giảm giá khuyến mãi</span><span className="text-red-500">-{(selectedOrder.discount || 0).toLocaleString()}đ</span></div><div className="flex justify-between items-center pt-4"><span className="text-blue-600 font-black text-sm uppercase italic tracking-widest">Khách đã thanh toán</span><span className="text-3xl font-black text-blue-700 tracking-tighter">{(selectedOrder.paidAmount || 0).toLocaleString()}đ</span></div></div></div>
                </div>
              </div>
            </div>
            <div className="p-8 bg-gray-100 border-t flex justify-end print-hide"><button onClick={() => onSelectOrder(null)} className="px-12 py-4 bg-white border-2 border-gray-200 rounded-[2rem] font-black text-gray-400 text-[10px] uppercase tracking-[0.2em] hover:bg-gray-50 active:scale-95 transition-all">Đóng chi tiết</button></div>
          </div>
        )}

        <div className="p-6 bg-gray-50 border-t text-center"><p className="text-[10px] text-gray-400 font-medium italic">* Chỉ hiển thị hóa đơn thuộc chi nhánh bạn đang làm việc.</p></div>
      </div>
    </div>
  );
}
