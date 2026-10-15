import { CheckCircle2, ChevronRight, Loader2, MapPin, Star, X } from 'lucide-react';
import CustomerCartItems from './CustomerCartItems';
import type { CartItem } from '../types';

interface BranchOption {
  id: string;
  name: string;
  address?: string;
  imageUrl?: string;
}

interface TableInfo {
  name: string;
  branchId?: string;
}

interface CustomerOrderConfirmationModalProps {
  cart: CartItem[];
  totalItems: number;
  totalAmount: number;
  branches: BranchOption[];
  selectedBranch: BranchOption | null;
  tableInfo: TableInfo | null;
  customerInfo: { isGuest?: boolean } | null;
  currentLoyaltyPoints: number;
  isSubmittingOrder: boolean;
  onClose: () => void;
  onBranchSelect: (branch: BranchOption) => void;
  onNoteChange: (item: CartItem, note: string) => void;
  onRemoveItem: (item: CartItem) => void;
  onConfirm: (branch: BranchOption | null) => void;
}

export default function CustomerOrderConfirmationModal({
  cart,
  totalItems,
  totalAmount,
  branches,
  selectedBranch,
  tableInfo,
  customerInfo,
  currentLoyaltyPoints,
  isSubmittingOrder,
  onClose,
  onBranchSelect,
  onNoteChange,
  onRemoveItem,
  onConfirm
}: CustomerOrderConfirmationModalProps) {
  return (
    <div className="fixed inset-0 bg-black/80 z-[200] flex items-end justify-center animate-in fade-in duration-300 backdrop-blur-sm">
      <div className="bg-white w-full max-w-lg max-h-[100dvh] overflow-y-auto rounded-t-[3rem] p-4 sm:p-8 pb-12 shadow-2xl animate-in slide-in-from-bottom-20 duration-500">
        <div className="flex justify-between items-start mb-6">
          <div><h3 className="text-2xl font-black text-gray-800 uppercase italic tracking-tighter">Xác nhận đơn hàng</h3><p className="text-xs font-bold text-blue-600 uppercase tracking-widest mt-1">Kiểm tra lại món & chọn cơ sở</p></div>
          <button type="button" onClick={onClose} aria-label="Đóng xác nhận đơn hàng" className="min-h-11 min-w-11 touch-manipulation bg-gray-100 p-3 rounded-full hover:bg-gray-200 transition-all text-gray-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><X size={24}/></button>
        </div>

        <div className="mb-6">
          <div className="flex justify-between items-center mb-3"><h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Danh sách món đã chọn</h4><span className="text-[9px] bg-blue-100 text-blue-600 px-2 py-0.5 rounded-full font-black">{totalItems} món</span></div>
          <CustomerCartItems cart={cart} onNoteChange={onNoteChange} onRemoveItem={onRemoveItem} />
        </div>

        <div className="mb-4">
          <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">{tableInfo ? 'Chi nhánh phục vụ (Cố định theo bàn)' : 'Chọn chi nhánh phục vụ'}</h4>
          <div className="space-y-3 max-h-[25vh] overflow-y-auto pr-2 no-scrollbar">
            {branches.filter(branch => !tableInfo || branch.id === tableInfo.branchId).map(branch => (
              <button type="button" key={branch.id} disabled={!!tableInfo} onClick={() => onBranchSelect(branch)} className={`w-full p-4 rounded-2xl border-2 flex items-center justify-between transition-all ${selectedBranch?.id === branch.id ? 'border-blue-600 bg-blue-50 shadow-md ring-4 ring-blue-100' : 'border-gray-50 bg-gray-50/50 hover:border-blue-200'} ${tableInfo ? 'cursor-default' : 'cursor-pointer'}`}>
                <div className="flex items-center">
                  <div className={`w-12 h-12 rounded-xl mr-3 overflow-hidden border ${selectedBranch?.id === branch.id ? 'border-blue-200 shadow-sm' : 'border-white'} flex items-center justify-center bg-white`}>{branch.imageUrl ? <img src={branch.imageUrl} className="w-full h-full object-cover" alt="" /> : <MapPin size={18} className={selectedBranch?.id === branch.id ? 'text-blue-600' : 'text-gray-400'}/>}</div>
                  <div className="text-left"><p className={`text-xs font-black uppercase tracking-tight ${selectedBranch?.id === branch.id ? 'text-blue-700' : 'text-gray-700'}`}>{branch.name}</p><p className="text-[9px] text-gray-400 font-bold mt-0.5 line-clamp-1">{branch.address || 'Địa chỉ đang cập nhật...'}</p></div>
                </div>
                {selectedBranch?.id === branch.id && <CheckCircle2 size={18} className="text-blue-600" />}
              </button>
            ))}
          </div>
        </div>

        <div className="mt-8 space-y-4">
          <div className="bg-gray-50 p-4 rounded-2xl border border-dashed border-gray-200">
            <div className="flex justify-between text-[11px] font-bold text-gray-400 uppercase tracking-widest"><span>Đơn tại: {tableInfo?.name || 'Mang về'}</span><span>{totalItems} món</span></div>
            <div className="flex justify-between items-center mt-2"><span className="text-sm font-black text-gray-800 italic uppercase">Tổng thanh toán:</span><div className="text-right"><span className="text-xl font-black text-blue-700">{totalAmount.toLocaleString()}đ</span><div className="flex flex-col items-end mt-1 space-y-1">{totalAmount >= 10000 && <p className="text-[9px] font-black text-orange-500 uppercase flex items-center justify-end"><Star size={10} className="fill-orange-500 mr-1" />Tích lũy: +{Math.floor(totalAmount / 10000)} điểm</p>}{customerInfo && !customerInfo.isGuest && <p className="text-[9px] font-black text-blue-600 uppercase flex items-center justify-end bg-blue-50 px-2 py-0.5 rounded-full border border-blue-100"><Star size={10} className="fill-blue-600 mr-1" />Hiện có: {currentLoyaltyPoints.toLocaleString()} điểm</p>}</div></div></div>
          </div>
          <button type="button" disabled={!selectedBranch || isSubmittingOrder} onClick={() => onConfirm(selectedBranch)} aria-label="Gửi yêu cầu gọi món" className="w-full min-h-12 touch-manipulation py-5 bg-blue-600 text-white rounded-[2rem] font-black text-sm uppercase tracking-[0.2em] shadow-xl shadow-blue-500/30 hover:bg-blue-700 transition-all active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 disabled:opacity-30 disabled:grayscale flex items-center justify-center">{isSubmittingOrder ? 'ĐANG GỬI YÊU CẦU...' : 'GỬI YÊU CẦU GỌI MÓN'}{isSubmittingOrder ? <Loader2 size={20} className="ml-2 animate-spin" /> : <ChevronRight size={20} className="ml-2" />}</button>
        </div>
      </div>
    </div>
  );
}
