import { Bell, History, LogIn, Printer, RotateCcw, Search, Signal, Trash2, UserPlus, Utensils, WifiOff, X } from 'lucide-react';
import type { RefObject } from 'react';
import type { CartItem, POSUserRole, Table } from '../types';

interface POSOrderHeaderProps {
  selectedTable: Table | null;
  selectedTableId: string;
  currentCart: CartItem[];
  currentCustomerPhone: string;
  customerPhoneInputRef: RefObject<HTMLInputElement>;
  activeShift: boolean;
  userRole?: POSUserRole;
  isSignalRConnected: boolean;
  isSaving: boolean;
  onCloseCurrentOrder: () => void;
  onLogout: () => void;
  onViewNotifications: () => void;
  onPrint: () => void;
  onOpenInvoiceHistory: () => void;
  onCloseShift: () => void;
  onOpenShiftHistory: () => void;
  onRefreshTable: () => void;
  onCancelCurrentOrder: () => void | Promise<void>;
  onCustomerPhoneChange: (value: string) => void;
  onFocusCustomerPhone: () => void;
  onClearUnsavedItems: () => void;
}

export default function POSOrderHeader({
  selectedTable,
  selectedTableId,
  currentCart,
  currentCustomerPhone,
  customerPhoneInputRef,
  activeShift,
  userRole,
  isSignalRConnected,
  isSaving,
  onCloseCurrentOrder,
  onLogout,
  onViewNotifications,
  onPrint,
  onOpenInvoiceHistory,
  onCloseShift,
  onOpenShiftHistory,
  onRefreshTable,
  onCancelCurrentOrder,
  onCustomerPhoneChange,
  onFocusCustomerPhone,
  onClearUnsavedItems
}: POSOrderHeaderProps) {
  const tableLabel = selectedTable ? `${selectedTable.areaName || 'Khu vực chưa đặt tên'} · ${selectedTable.name}` : 'Mang về';

  return (
    <>
      <div className="bg-[#1e293b] flex items-center p-1 space-x-1">
        <div className="bg-white text-gray-800 px-4 py-1.5 rounded-t text-xs font-bold flex items-center shadow-sm">
          {tableLabel} <button onClick={onCloseCurrentOrder} title="Đóng đơn đang chọn" aria-label="Đóng đơn đang chọn" className="ml-2 text-gray-400 hover:text-red-500"><X size={12} /></button>
        </div>
        <div className="flex-1"></div>
        <div className="flex items-center space-x-3 pr-2">
          <button onClick={onLogout} className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded text-[11px] font-bold flex items-center transition-all mr-2 shadow-lg border border-blue-400" title="Đăng xuất khỏi phiên bán hàng"><LogIn size={14} className="mr-1" /> Đăng xuất</button>
          <button onClick={onViewNotifications} title="Thông báo" aria-label="Thông báo" className="text-white opacity-60 hover:opacity-100"><Bell size={18} /></button>
          <button onClick={onPrint} title="In đơn hiện tại" aria-label="In đơn hiện tại" className="text-white opacity-60 hover:opacity-100"><Printer size={18} /></button>
          <button onClick={onOpenInvoiceHistory} title="Lịch sử hóa đơn" aria-label="Lịch sử hóa đơn" className="text-white opacity-60 hover:opacity-100"><History size={18} /></button>
        </div>
      </div>

      <div className="p-2 flex items-center border-b space-x-2">
        {activeShift && userRole === 'cashier' && (
          <div className="flex space-x-1">
            <button onClick={onCloseShift} className="bg-orange-100 text-orange-600 px-3 py-1 rounded text-[10px] font-black uppercase hover:bg-orange-200 transition-all border border-orange-200 flex items-center"><RotateCcw size={12} className="mr-1" /> Chốt ca</button>
            <button onClick={onOpenShiftHistory} className="bg-gray-100 text-gray-500 px-3 py-1 rounded text-[10px] font-black uppercase hover:bg-gray-200 transition-all border border-gray-200 flex items-center"><History size={12} className="mr-1" /> Lịch sử ca</button>
          </div>
        )}
        <div className="bg-blue-100 text-blue-700 px-3 py-1 rounded text-xs font-bold flex items-center whitespace-nowrap"><Utensils size={14} className="mr-1" /> {tableLabel}</div>

        <div className={`hidden sm:flex items-center px-2 py-1 rounded-full text-[9px] font-black uppercase tracking-tighter border ${isSignalRConnected ? 'bg-emerald-50 text-emerald-600 border-emerald-100' : 'bg-amber-50 text-amber-600 border-amber-100'}`}>
          {isSignalRConnected ? <Signal size={10} className="mr-1" /> : <WifiOff size={10} className="mr-1" />}
          {isSignalRConnected ? 'Đang trực tuyến' : 'Mất kết nối'}
        </div>

        {selectedTable?.status === 'Có khách' && (
          <button onClick={onRefreshTable} className="p-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors border border-blue-100 shadow-sm" title="Đồng bộ lại món từ máy chủ"><RotateCcw size={14} className={isSaving ? 'animate-spin' : ''} /></button>
        )}

        {selectedTableId !== 'delivery' && currentCart.length > 0 && (
          <button onClick={onCancelCurrentOrder} className="p-1.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors border border-red-100 shadow-sm" title="Hủy/Xóa đơn bàn này"><Trash2 size={16} /></button>
        )}

        <div className="flex-1 relative">
          <span className="absolute inset-y-0 left-0 pl-3 flex items-center"><Search className="h-4 w-4 text-gray-400" /></span>
          <input id="pos-product-search" type="text" className="w-full bg-gray-50 border border-gray-200 rounded py-1 pl-10 pr-8 text-sm outline-none focus:ring-1 focus:ring-blue-500" placeholder="Tìm khách hàng (F4)" ref={customerPhoneInputRef} aria-label="Tìm khách hàng theo số điện thoại" value={currentCustomerPhone} onChange={event => onCustomerPhoneChange(event.target.value)} />
          <button onClick={onFocusCustomerPhone} title="Nhập thông tin khách hàng" aria-label="Nhập thông tin khách hàng" className="absolute right-2 top-1.5 rounded p-1 text-blue-600 hover:bg-blue-50"><UserPlus size={16} /></button>
        </div>
        <button type="button" onClick={onClearUnsavedItems} title="Xóa món chưa gửi" aria-label="Xóa món chưa gửi" className="p-1.5 text-gray-400 hover:bg-gray-100 rounded"><RotateCcw size={18}/></button>
      </div>
    </>
  );
}
