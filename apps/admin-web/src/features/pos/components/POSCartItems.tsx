import { ClipboardList, Minus, Plus, X } from 'lucide-react';
import type { CartItem } from '../types';
import { kitchenStatusClass, kitchenStatusLabel } from '../utils';

interface POSCartItemsProps {
  items: CartItem[];
  onRemove: (productId: string, index: number) => void;
  onUpdateQuantity: (productId: string, delta: number, index: number) => void;
  onNoteChange: (index: number, note: string) => void;
}

const POSCartItems = ({ items, onRemove, onUpdateQuantity, onNoteChange }: POSCartItemsProps) => (
  <div className="flex-1 overflow-auto px-4">
    {items.length > 0 ? items.map((item, idx) => (
      <div key={`${item.id}-${idx}`} className="flex items-start py-3 border-b border-gray-100 group transition-all">
        <div className="flex-1">
          <div className="flex items-center">
            <button type="button" onClick={() => onRemove(item.id, idx)} aria-label={`Xóa ${item.name} khỏi đơn`} title={`Xóa ${item.name}`} className="mr-2 min-w-8 min-h-8 flex items-center justify-center text-red-300 hover:text-red-500 opacity-100 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity">
              <X size={14} />
            </button>
            <div>
              <div className="flex items-center">
                <p className="text-sm font-bold text-gray-800 capitalize">{idx + 1}. {item.name}</p>
                {item.quantity > item.sentQuantity && (
                  <span className="ml-2 bg-blue-100 text-blue-600 text-[8px] px-1 rounded font-black animate-pulse uppercase">Mới</span>
                )}
              </div>
              <div className="text-[10px] text-gray-400 font-medium">
                {item.sentQuantity > 0 && `Đã gửi bếp: ${item.sentQuantity}`}
                {item.quantity > item.sentQuantity && ` | Chờ gửi: ${item.quantity - item.sentQuantity}`}
              </div>
              {item.kitchenStatus && item.sentQuantity > 0 && (
                <span className={`mt-1 inline-flex rounded-full px-2 py-0.5 text-[9px] font-black uppercase ${kitchenStatusClass(item.kitchenStatus)}`}>
                  Bếp: {kitchenStatusLabel(item.kitchenStatus)}
                </span>
              )}
              {(item.selectedSize || (item.selectedToppings && item.selectedToppings.length > 0) || (item as any).optionsText) && (
                <div className="flex flex-wrap gap-1 mt-0.5">
                  {item.selectedSize && <span className="bg-blue-50 text-blue-600 text-[9px] px-1.5 py-0.5 rounded font-black uppercase">Size {item.selectedSize.name}</span>}
                  {item.selectedToppings?.map((t, i) => (
                    <span key={i} className="bg-orange-50 text-orange-600 text-[9px] px-1.5 py-0.5 rounded font-bold">+{t.name}</span>
                  ))}
                  {(item as any).optionsText && (
                    <span className="bg-orange-50 text-orange-600 text-[9px] px-1.5 py-0.5 rounded font-bold italic">{(item as any).optionsText}</span>
                  )}
                </div>
              )}
            </div>
          </div>
          <input
            type="text"
            className="text-[10px] text-orange-500 mt-1 bg-transparent border-b border-transparent hover:border-orange-200 outline-none w-full"
            placeholder="Ghi chú món..."
            value={item.note}
            onChange={(e) => onNoteChange(idx, e.target.value)}
          />
        </div>
        <div className="flex items-center space-x-4">
          <div className="flex items-center border rounded-md overflow-hidden bg-gray-50">
            <button type="button" onClick={() => onUpdateQuantity(item.id, -1, idx)} aria-label={`Giảm số lượng ${item.name}`} className="min-w-9 min-h-9 px-1.5 py-1 hover:bg-gray-200 text-gray-400 transition-colors"><Minus size={12} /></button>
            <input type="text" className="w-8 bg-transparent text-center text-xs font-bold outline-none" value={item.quantity} readOnly />
            <button type="button" onClick={() => onUpdateQuantity(item.id, 1, idx)} aria-label={`Tăng số lượng ${item.name}`} className="min-w-9 min-h-9 px-1.5 py-1 hover:bg-gray-200 text-blue-600 transition-colors"><Plus size={12} /></button>
          </div>
          <div className="text-right w-20">
            <p className="text-[11px] text-gray-400 font-medium">{item.totalItemPrice.toLocaleString()}</p>
            <p className="text-sm font-black text-gray-800">{(item.totalItemPrice * item.quantity).toLocaleString()}</p>
          </div>
        </div>
      </div>
    )) : (
      <div className="h-full flex flex-col items-center justify-center text-gray-300 opacity-50">
        <ClipboardList size={64} />
        <p className="mt-2 text-sm font-medium">Chưa có món nào được chọn</p>
      </div>
    )}
  </div>
);

export default POSCartItems;
