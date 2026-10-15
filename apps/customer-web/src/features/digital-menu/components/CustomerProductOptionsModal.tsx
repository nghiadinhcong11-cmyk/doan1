import { CheckCircle2, ChevronRight, Minus, Plus, X } from 'lucide-react';
import type { Product } from '../types';

export interface ProductOption {
  name: string;
  price: number;
}

interface CustomerProductOptionsModalProps {
  product: Product;
  selectedSize: ProductOption | null;
  selectedToppings: ProductOption[];
  customizingQuantity: number;
  totalAmount: number;
  sizes: ProductOption[];
  toppings: ProductOption[];
  onClose: () => void;
  onSizeChange: (size: ProductOption) => void;
  onToggleTopping: (topping: ProductOption) => void;
  onDecreaseQuantity: () => void;
  onIncreaseQuantity: () => void;
  onConfirm: () => void;
}

export default function CustomerProductOptionsModal({
  product,
  selectedSize,
  selectedToppings,
  customizingQuantity,
  totalAmount,
  sizes,
  toppings,
  onClose,
  onSizeChange,
  onToggleTopping,
  onDecreaseQuantity,
  onIncreaseQuantity,
  onConfirm
}: CustomerProductOptionsModalProps) {
  return (
    <div className="fixed inset-0 bg-black/80 z-[300] flex items-end justify-center animate-in fade-in duration-300 backdrop-blur-sm">
      <div className="bg-white w-full max-w-lg rounded-t-[3rem] shadow-2xl animate-in slide-in-from-bottom-20 duration-500 max-h-[100dvh] flex flex-col">
        <div className="relative h-48 flex-shrink-0">
          <img src={product.imageUrl || 'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800'} className="w-full h-full object-cover rounded-t-[3rem]" alt="" />
          <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent"></div>
          <button type="button" aria-label="Đóng cấu hình món" onClick={onClose} className="absolute top-6 right-6 min-h-11 min-w-11 touch-manipulation bg-white/20 backdrop-blur-md p-2 rounded-full text-white hover:bg-white/40 transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white">
            <X size={20}/>
          </button>
          <div className="absolute bottom-6 left-8 text-white">
            <h3 className="text-2xl font-black uppercase italic tracking-tighter">{product.name}</h3>
            <p className="text-xs opacity-80 font-bold uppercase tracking-widest">{product.category}</p>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto p-4 sm:p-8 pt-6 no-scrollbar space-y-10">
          {sizes.length > 0 && (
            <div className="space-y-4">
              <div className="flex justify-between items-end">
                <label className="text-[11px] font-black text-gray-400 uppercase tracking-[0.2em]">Chọn kích cỡ</label>
                <span className="text-[9px] bg-blue-100 text-blue-600 px-2 py-0.5 rounded font-black uppercase">Bắt buộc</span>
              </div>
              <div className="grid grid-cols-1 gap-3">
                {sizes.map((size, index) => (
                  <button type="button" key={index} onClick={() => onSizeChange(size)} className={`p-4 rounded-[1.5rem] border-2 transition-all flex items-center justify-between group ${selectedSize?.name === size.name ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100' : 'border-gray-50 bg-gray-50/50 hover:border-blue-200'}`}>
                    <div className="flex items-center">
                      <div className={`w-10 h-10 rounded-xl flex items-center justify-center mr-4 transition-colors ${selectedSize?.name === size.name ? 'bg-blue-600 text-white' : 'bg-white text-gray-400 shadow-sm'}`}>
                        <span className="font-black text-sm uppercase">{size.name.charAt(0)}</span>
                      </div>
                      <div className="text-left">
                        <p className={`text-sm font-black uppercase tracking-tight ${selectedSize?.name === size.name ? 'text-blue-700' : 'text-gray-700'}`}>{size.name}</p>
                        <p className="text-[10px] text-gray-400 font-bold uppercase">Kích thước chuẩn</p>
                      </div>
                    </div>
                    <div className="text-right"><p className={`text-sm font-black ${selectedSize?.name === size.name ? 'text-blue-700' : 'text-gray-400'}`}>+{size.price.toLocaleString()}đ</p></div>
                  </button>
                ))}
              </div>
            </div>
          )}

          {toppings.length > 0 && (
            <div className="space-y-4 pb-4">
              <div className="flex justify-between items-end">
                <label className="text-[11px] font-black text-gray-400 uppercase tracking-[0.2em]">Thêm Topping</label>
                <span className="text-[9px] bg-gray-100 text-gray-400 px-2 py-0.5 rounded font-black uppercase">Tùy chọn</span>
              </div>
              <div className="grid grid-cols-2 gap-3">
                {toppings.map((topping, index) => {
                  const isSelected = selectedToppings.some(item => item.name === topping.name);
                  return (
                    <button type="button" key={index} onClick={() => onToggleTopping(topping)} className={`p-4 rounded-3xl border-2 transition-all flex flex-col items-center text-center ${isSelected ? 'border-orange-500 bg-orange-50 ring-4 ring-orange-100' : 'border-gray-50 bg-gray-50/50 hover:border-orange-200'}`}>
                      <p className={`text-[11px] font-black uppercase tracking-tight mb-1 ${isSelected ? 'text-orange-700' : 'text-gray-700'}`}>{topping.name}</p>
                      <p className={`text-[10px] font-bold ${isSelected ? 'text-orange-600' : 'text-gray-400'}`}>+{topping.price.toLocaleString()}đ</p>
                      {isSelected && <div className="mt-2 bg-orange-500 rounded-full p-0.5"><CheckCircle2 size={12} className="text-white" /></div>}
                    </button>
                  );
                })}
              </div>
            </div>
          )}
        </div>

        <div className="p-4 sm:p-8 pt-6 border-t border-gray-100 bg-white/80 backdrop-blur-md rounded-t-[2.5rem] shadow-[0_-10px_40px_-15px_rgba(0,0,0,0.1)]">
          <div className="flex items-center justify-between mb-6">
            <div className="flex items-center bg-gray-100 rounded-2xl p-1.5 border border-gray-200">
              <button type="button" aria-label="Giảm số lượng món đang cấu hình" onClick={onDecreaseQuantity} className="min-h-11 min-w-11 touch-manipulation w-10 h-10 flex items-center justify-center text-gray-400 hover:text-blue-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><Minus size={18}/></button>
              <span className="px-5 font-black text-lg text-gray-800">{customizingQuantity.toString().padStart(2, '0')}</span>
              <button type="button" aria-label="Tăng số lượng món đang cấu hình" onClick={onIncreaseQuantity} className="min-h-11 min-w-11 touch-manipulation w-10 h-10 flex items-center justify-center text-blue-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><Plus size={18}/></button>
            </div>
            <div className="text-right">
              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Thành tiền</p>
              <p className="text-3xl font-black text-blue-700 tracking-tighter">{totalAmount.toLocaleString()}đ</p>
            </div>
          </div>
          <button type="button" onClick={onConfirm} className="w-full bg-blue-600 text-white py-5 rounded-[2rem] font-black text-sm uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/40 hover:bg-blue-700 active:scale-95 transition-all flex items-center justify-center">XÁC NHẬN THÊM VÀO GIỎ <ChevronRight size={20} className="ml-2" /></button>
        </div>
      </div>
    </div>
  );
}
