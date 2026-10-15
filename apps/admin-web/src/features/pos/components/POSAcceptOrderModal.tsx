import { Trash2, Utensils, X } from 'lucide-react';
import type { Table } from '../types';

interface OrderDetail {
  quantity: number;
  productName: string;
  options?: string;
  unitPrice: number;
}

export interface POSPendingOrder {
  id: string;
  tableName: string;
  customerName: string;
  details: OrderDetail[];
  totalAmount: number;
}

interface POSAcceptOrderModalProps {
  order: POSPendingOrder | null;
  tables: Table[];
  isSaving: boolean;
  onClose: () => void;
  onAccept: (tableId: string) => void;
  onReject: (orderId: string, tableName: string) => void;
}

export default function POSAcceptOrderModal({ order, tables, isSaving, onClose, onAccept, onReject }: POSAcceptOrderModalProps) {
  if (!order) return null;

  return (
    <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-md">
      <div className="bg-white w-full max-w-2xl rounded-[3rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
          <div>
            <h3 className="font-black text-xl uppercase italic tracking-tighter">Xác nhận nhận đơn</h3>
            <p className="text-[10px] font-bold opacity-80 uppercase tracking-widest">Chọn bàn để bắt đầu phục vụ món</p>
          </div>
          <button onClick={onClose} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
        </div>

        <div className="p-8 flex flex-col md:flex-row gap-8">
          <div className="w-full md:w-64 space-y-4">
            <div className="bg-gray-50 p-5 rounded-3xl border border-gray-100">
              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">Thông tin từ khách</p>
              <h4 className="text-lg font-black text-blue-600 uppercase tracking-tight mb-1">{order.tableName}</h4>
              <p className="text-xs font-bold text-gray-700">{order.customerName}</p>
              <div className="mt-4 pt-4 border-t border-dashed border-gray-200">
                {order.details.map((detail, index) => (
                  <div key={index} className="flex justify-between items-center text-[11px] mb-1">
                    <div className="flex-1">
                      <p className="font-bold text-gray-700">{detail.quantity}x {detail.productName}</p>
                      <p className="text-[9px] text-gray-400 italic">{detail.options}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-[9px] text-gray-400">{detail.unitPrice.toLocaleString()}đ</p>
                      <p className="font-black text-blue-700">{(detail.quantity * detail.unitPrice).toLocaleString()}đ</p>
                    </div>
                  </div>
                ))}
                <div className="mt-4 pt-2 border-t font-black text-right text-lg text-blue-800">Tổng: {order.totalAmount.toLocaleString()}đ</div>
              </div>
            </div>
          </div>

          <div className="flex-1">
            <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-4">Gán vào bàn thực tế:</p>
            <div className="grid grid-cols-3 sm:grid-cols-4 gap-3 max-h-[300px] overflow-auto pr-2 no-scrollbar">
              <button onClick={() => onAccept('delivery')} disabled={isSaving} className={`p-3 rounded-2xl border-2 flex flex-col items-center justify-center transition-all disabled:cursor-not-allowed disabled:opacity-50 ${order.tableName === 'Mang về' ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100' : 'border-gray-100 hover:border-blue-200'}`}>
                <Utensils size={18} className="text-blue-500 mb-1" />
                <span className="text-[10px] font-black uppercase">Mang về</span>
              </button>
              {tables.map((table, index) => (
                <button key={`${table.id}-${index}`} onClick={() => onAccept(table.id)} disabled={isSaving || (table.status === 'Có khách' && table.name !== order.tableName)} className={`p-3 rounded-2xl border-2 flex flex-col items-center justify-center transition-all disabled:cursor-not-allowed disabled:opacity-50 ${table.name === order.tableName ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100' : table.status === 'Có khách' ? 'bg-gray-50 border-gray-200 opacity-50 cursor-not-allowed' : 'border-gray-100 hover:border-blue-200'}`}>
                  <span className="text-[9px] font-bold uppercase tracking-wide text-gray-400 text-center">{table.areaName || 'Khu vực chưa đặt tên'}</span>
                  <span className="text-xs font-black uppercase mt-0.5">{table.name}</span>
                  <span className="text-[8px] font-bold text-gray-400 mt-1 uppercase">{table.status}</span>
                </button>
              ))}
            </div>
          </div>
        </div>

        <div className="p-6 bg-gray-50 border-t flex items-center justify-between">
          <button onClick={() => { onReject(order.id, order.tableName); onClose(); }} className="bg-red-50 text-red-600 px-6 py-3 rounded-2xl font-black text-xs uppercase hover:bg-red-600 hover:text-white transition-all flex items-center">
            <Trash2 size={16} className="mr-2" /> Từ chối / Hủy đơn
          </button>
          <p className="text-[10px] text-gray-400 italic font-medium ml-4 text-right">* Chọn một bàn trống hoặc "Mang về" để đồng bộ giỏ hàng.</p>
        </div>
      </div>
    </div>
  );
}
