import type { CartItem } from '../types';

interface CustomerCartItemsProps {
  cart: CartItem[];
  onNoteChange: (item: CartItem, note: string) => void;
  onRemoveItem: (item: CartItem) => void;
}

export default function CustomerCartItems({ cart, onNoteChange, onRemoveItem }: CustomerCartItemsProps) {
  return (
    <div className="space-y-3 max-h-[25vh] overflow-y-auto pr-2 no-scrollbar">
      {cart.map((item, index) => (
        <div key={index} className="flex justify-between items-center bg-gray-50/50 p-3 rounded-2xl border border-gray-100 group">
          <div className="flex items-center space-x-3"><div className="w-10 h-10 bg-white rounded-xl flex items-center justify-center text-blue-600 font-black text-xs border border-gray-100 shadow-sm">{item.quantity}x</div><div><p className="text-xs font-black text-gray-800 leading-tight">{item.name}</p><div className="flex flex-wrap gap-1 mt-0.5">{item.selectedSize && <span className="text-[8px] bg-blue-50 text-blue-500 px-1.5 py-0.5 rounded font-black uppercase border border-blue-100">Size: {item.selectedSize.name}</span>}{item.selectedToppings && item.selectedToppings.length > 0 && <span className="text-[8px] bg-orange-50 text-orange-600 px-1.5 py-0.5 rounded font-bold italic border border-orange-100">+ {item.selectedToppings.map(topping => topping.name).join(', ')}</span>}</div><input type="text" placeholder="Ghi chú (VD: Bỏ vào trà sữa...)" className="w-full mt-2 bg-white border border-dashed border-gray-200 rounded-lg px-2 py-1 text-[10px] italic font-medium text-orange-600 outline-none focus:border-orange-300" value={item.note || ''} onChange={event => onNoteChange(item, event.target.value)} /></div></div>
          <div className="text-right flex flex-col items-end"><p className="text-[10px] text-gray-400 font-bold">{item.totalItemPrice.toLocaleString()}đ</p><p className="text-xs font-black text-gray-700 tracking-tighter">{(item.totalItemPrice * item.quantity).toLocaleString()}đ</p><button type="button" onClick={() => onRemoveItem(item)} className="text-[9px] font-bold text-red-400 hover:text-red-600 mt-1 uppercase">Xóa</button></div>
        </div>
      ))}
    </div>
  );
}
