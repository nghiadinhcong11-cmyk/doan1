import { Loader2, Minus, Plus, RefreshCw, ShoppingBag, Star, X } from 'lucide-react';
import type { CartItem, Product } from '../types';

interface CustomerProductListProps {
  products: Product[];
  cart: CartItem[];
  activeCategory: string;
  loading: boolean;
  onRefresh: () => void;
  onAddProduct: (product: Product) => void;
  onRemoveProduct: (productId: string) => void;
  onDecreaseProduct: (product: Product) => void;
}

export default function CustomerProductList({ products, cart, activeCategory, loading, onRefresh, onAddProduct, onRemoveProduct, onDecreaseProduct }: CustomerProductListProps) {
  return (
    <div className="mx-auto max-w-6xl space-y-6 p-4 sm:p-6">
      <div className="flex justify-between items-center">
        <div>
          <p className="mb-1 text-[10px] font-black uppercase tracking-[0.22em] text-[#3e8069]">Hôm nay ăn gì?</p>
          <h2 className="text-2xl font-black tracking-tight text-[#22252b]">{activeCategory === 'Tất cả' ? 'Gợi ý cho bạn' : activeCategory}</h2>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-[10px] font-bold text-gray-400 bg-gray-100 px-2 py-1 rounded uppercase tracking-widest">{products.length} món</span>
          <button type="button" onClick={onRefresh} disabled={loading} className="inline-flex min-h-10 items-center gap-1.5 rounded-xl border border-[#eadfd2] bg-white px-3 text-[10px] font-black uppercase tracking-wider text-[#3e8069] transition hover:bg-[#f8f3ec] disabled:cursor-wait disabled:opacity-60" aria-label="Làm mới thực đơn">
            <RefreshCw size={14} className={loading ? 'animate-spin' : ''} /> Làm mới
          </button>
        </div>
      </div>

      {loading ? (
        <div className="py-20 flex flex-col items-center justify-center text-gray-400"><Loader2 className="animate-spin mb-2 text-blue-600" /><p className="text-xs font-bold uppercase tracking-widest">Đang tải thực đơn...</p></div>
      ) : products.length === 0 ? (
        <div className="rounded-3xl border-2 border-dashed border-gray-200 bg-white px-6 py-16 text-center"><ShoppingBag size={42} className="mx-auto mb-3 text-gray-200" /><p className="font-bold text-gray-500">Không có món trong danh mục này</p><p className="mt-1 text-xs text-gray-400">Hãy thử chọn danh mục khác.</p></div>
      ) : products.map((product, index) => (
        <div key={product.id} style={{ animationDelay: `${Math.min(index, 8) * 55}ms` }} className="product-card group flex gap-4 rounded-[1.6rem] border border-[#ebe5dc] bg-white p-3 shadow-[0_16px_35px_-24px_rgba(68,46,31,.5)] transition-all hover:-translate-y-1 hover:shadow-[0_20px_40px_-24px_rgba(68,46,31,.58)] active:scale-[0.98] sm:p-4">
          <div className="relative h-28 w-28 flex-shrink-0 overflow-hidden rounded-2xl bg-[#f0ece5] border border-[#eee7de] sm:h-32 sm:w-32">
            {product.imageUrl ? <img src={product.imageUrl} alt={product.name} className="w-full h-full object-cover" /> : <div className="w-full h-full flex items-center justify-center text-blue-200"><Star size={32} className="text-[#d7c7b6]" /></div>}
          </div>
          <div className="min-w-0 flex-1 flex flex-col justify-between">
            <div>
              <div className="flex justify-between items-start"><div><h3 className="text-sm font-black leading-tight text-[#22252b] transition-colors group-hover:text-orange-600">{product.name}</h3><p className="mt-1 text-[9px] font-bold uppercase tracking-widest text-[#3e8069]">{product.group}</p></div><span className="text-sm font-black tracking-tight text-[#ee6c36]">{product.price.toLocaleString()}đ</span></div>
              <p className="text-[10px] text-gray-400 mt-1 line-clamp-2 leading-relaxed font-medium italic">{product.description || 'Hương vị thơm ngon tinh tế, chế biến từ nguyên liệu tươi sạch trong ngày.'}</p>
              <div className="flex gap-1 mt-2">{product.sizesJson && product.sizesJson !== '[]' && <span className="text-[8px] bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded font-black uppercase border border-blue-100">Kích cỡ</span>}{product.toppingsJson && product.toppingsJson !== '[]' && <span className="text-[8px] bg-orange-50 text-orange-600 px-1.5 py-0.5 rounded font-black uppercase border border-orange-100">Topping</span>}</div>
            </div>
            <div className="flex justify-end mt-2 items-center space-x-2">
              {cart.find(item => item.id === product.id) ? (
                <>
                  <button type="button" onClick={() => onRemoveProduct(product.id)} aria-label={`Xóa ${product.name} khỏi giỏ hàng`} className="min-h-10 min-w-10 touch-manipulation p-2 text-red-500 hover:bg-red-50 rounded-xl transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-500" title="Hủy món"><X size={18} /></button>
                  <div className="flex items-center bg-blue-50 rounded-xl p-1 border border-blue-100">
                    <button type="button" onClick={() => onDecreaseProduct(product)} aria-label={`Giảm số lượng ${product.name}`} className="min-h-10 min-w-10 touch-manipulation flex items-center justify-center text-blue-600 hover:bg-blue-100 rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><Minus size={14}/></button>
                    <span className="px-3 text-xs font-black text-blue-700">{cart.filter(item => item.id === product.id).reduce((sum, item) => sum + item.quantity, 0)}</span>
                    <button type="button" onClick={() => onAddProduct(product)} aria-label={`Tăng số lượng ${product.name}`} className="min-h-10 min-w-10 touch-manipulation flex items-center justify-center text-blue-600 hover:bg-blue-100 rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><Plus size={14}/></button>
                  </div>
                </>
              ) : <button type="button" onClick={() => onAddProduct(product)} aria-label={`Thêm ${product.name} vào giỏ hàng`} className="add-button min-h-11 min-w-11 touch-manipulation flex h-11 w-11 items-center justify-center rounded-2xl bg-orange-600 text-white shadow-lg shadow-orange-500/20 transition-all hover:rotate-6 hover:bg-orange-700 active:scale-90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-300 focus-visible:ring-offset-2"><Plus size={20} /></button>}
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
