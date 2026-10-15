import { CheckCircle2, X } from 'lucide-react';
import type { Product } from '../types';

interface POSProductOption {
  name: string;
  price: number;
}

interface POSProductOptionsModalProps {
  open: boolean;
  product: Product | null;
  selectedSize: POSProductOption | null;
  selectedToppings: POSProductOption[];
  onSelectSize: (size: POSProductOption) => void;
  onToggleTopping: (topping: POSProductOption) => void;
  onClose: () => void;
  onConfirm: () => void;
}

function parseOptions(value?: string): POSProductOption[] {
  if (!value) return [];
  return JSON.parse(value) as POSProductOption[];
}

export default function POSProductOptionsModal({ open, product, selectedSize, selectedToppings, onSelectSize, onToggleTopping, onClose, onConfirm }: POSProductOptionsModalProps) {
  if (!open || !product) return null;
  const sizes = parseOptions(product.sizesJson);
  const toppings = parseOptions(product.toppingsJson);

  return (
    <div className="fixed inset-0 bg-black/80 z-[500] flex items-center justify-center p-4 backdrop-blur-md">
      <div className="bg-white w-full max-w-lg rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center"><div><h3 className="font-black text-xl uppercase italic tracking-tighter">{product.name}</h3><p className="text-[10px] font-bold opacity-80 uppercase tracking-widest mt-1">Tùy chọn kích cỡ & Topping</p></div><button onClick={onClose} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button></div>
        <div className="p-8 space-y-8 max-h-[60vh] overflow-y-auto no-scrollbar">
          {sizes.length > 0 && <div className="space-y-4"><label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] block ml-1">Chọn kích cỡ (Size)</label><div className="grid grid-cols-3 gap-3">{sizes.map((size, index) => <button key={index} onClick={() => onSelectSize(size)} className={`py-4 px-2 rounded-2xl border-2 transition-all flex flex-col items-center ${selectedSize?.name === size.name ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100' : 'border-gray-100 bg-gray-50/50 hover:border-blue-200'}`}><span className={`text-sm font-black uppercase ${selectedSize?.name === size.name ? 'text-blue-600' : 'text-gray-500'}`}>{size.name}</span><span className="text-[10px] font-bold text-gray-400 mt-1">+{size.price.toLocaleString()}đ</span></button>)}</div></div>}
          {toppings.length > 0 && <div className="space-y-4"><label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] block ml-1">Thêm Topping (Tùy chọn)</label><div className="grid grid-cols-2 gap-3">{toppings.map((topping, index) => { const selected = selectedToppings.some(item => item.name === topping.name); return <button key={index} onClick={() => onToggleTopping(topping)} className={`py-4 px-5 rounded-2xl border-2 transition-all flex items-center justify-between ${selected ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100' : 'border-gray-100 bg-gray-50/50 hover:border-blue-200'}`}><div className="text-left"><p className={`text-xs font-black uppercase tracking-tight ${selected ? 'text-blue-700' : 'text-gray-700'}`}>{topping.name}</p><p className="text-[10px] font-bold text-gray-400 mt-0.5">+{topping.price.toLocaleString()}đ</p></div>{selected && <CheckCircle2 size={18} className="text-blue-600" />}</button>; })}</div></div>}
        </div>
        <div className="p-8 bg-gray-50 border-t flex items-center justify-between"><div><p className="text-[10px] font-bold text-gray-400 uppercase tracking-widest mb-1">Đơn giá món</p><p className="text-3xl font-black text-blue-700 tracking-tighter">{(product.price + (selectedSize?.price || 0) + selectedToppings.reduce((sum, topping) => sum + topping.price, 0)).toLocaleString()}đ</p></div><div className="flex space-x-3"><button onClick={onClose} className="px-8 py-4 bg-white border-2 border-gray-200 rounded-2xl font-black text-gray-400 text-xs uppercase tracking-widest hover:bg-gray-100 transition-all">Bỏ qua</button><button onClick={onConfirm} className="px-12 py-4 bg-blue-600 text-white rounded-2xl font-black text-xs uppercase tracking-widest shadow-xl shadow-blue-500/20 hover:bg-blue-700 transition-all active:scale-95">XÁC NHẬN CHỌN</button></div></div>
      </div>
    </div>
  );
}
