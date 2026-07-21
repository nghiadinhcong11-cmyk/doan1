import React from 'react';
import { ShoppingBag, ChevronRight, Star } from 'lucide-react';

const DigitalMenu = () => {
  const categories = ['Phổ biến', 'Cà phê', 'Trà trái cây', 'Đồ ăn nhẹ'];
  const products = [
    { id: 1, name: 'Cà phê muối', price: '25.000', rate: '4.8', desc: 'Hương vị đậm đà kết hợp lớp kem mặn đặc trưng.' },
    { id: 2, name: 'Trà đào cam sả', price: '35.000', rate: '4.9', desc: 'Thức uống thanh mát cho ngày hè năng động.' },
  ];

  return (
    <div className="min-h-screen bg-white font-sans text-gray-900 pb-24">
      {/* Header Image */}
      <div className="h-48 bg-blue-600 relative overflow-hidden">
        <div className="absolute inset-0 bg-black/20"></div>
        <div className="absolute bottom-4 left-4 text-white">
          <h1 className="text-2xl font-black uppercase italic tracking-tighter">Kabo Coffee</h1>
          <p className="text-xs opacity-80 flex items-center">
             <span className="w-2 h-2 bg-green-400 rounded-full mr-2"></span> Đang mở cửa • 07:00 - 22:00
          </p>
        </div>
      </div>

      {/* Categories */}
      <div className="flex space-x-4 p-4 overflow-x-auto no-scrollbar sticky top-0 bg-white z-10 border-b">
        {categories.map((cat, i) => (
          <button
            key={i}
            className={`whitespace-nowrap px-4 py-2 rounded-full text-sm font-bold transition-all ${
              i === 0 ? 'bg-blue-600 text-white shadow-lg' : 'bg-gray-100 text-gray-500'
            }`}
          >
            {cat}
          </button>
        ))}
      </div>

      {/* Product List */}
      <div className="p-4 space-y-8">
        <h2 className="text-xl font-bold">Món nổi bật 🔥</h2>
        {products.map((p) => (
          <div key={p.id} className="flex space-x-4 group">
            <div className="w-24 h-24 bg-gray-100 rounded-2xl flex-shrink-0 relative overflow-hidden">
               <div className="absolute top-1 left-1 bg-white/90 px-1 rounded flex items-center text-[10px] font-bold">
                 <Star size={10} className="text-yellow-500 mr-0.5" fill="currentColor"/> {p.rate}
               </div>
            </div>
            <div className="flex-1 border-b pb-4">
              <div className="flex justify-between items-start">
                <h3 className="font-bold text-gray-800">{p.name}</h3>
                <span className="text-blue-600 font-black">{p.price}</span>
              </div>
              <p className="text-xs text-gray-400 mt-1 line-clamp-2 leading-relaxed">{p.desc}</p>
              <button className="mt-3 flex items-center text-xs font-bold text-blue-600 group-active:translate-x-1 transition-transform">
                Thêm vào giỏ <ChevronRight size={14} className="ml-1" />
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Floating Cart Button */}
      <div className="fixed bottom-6 left-4 right-4 bg-blue-600 text-white p-4 rounded-2xl shadow-2xl flex justify-between items-center animate-bounce-subtle">
        <div className="flex items-center">
          <div className="bg-white/20 p-2 rounded-lg mr-3">
            <ShoppingBag size={20} />
          </div>
          <div>
            <p className="text-xs opacity-80 font-medium">2 món đã chọn</p>
            <p className="font-bold text-lg">60.000 đ</p>
          </div>
        </div>
        <button className="bg-white text-blue-600 px-6 py-2 rounded-xl font-bold text-sm shadow-sm active:scale-95 transition-transform">
          Xem giỏ hàng
        </button>
      </div>
    </div>
  );
};

export default DigitalMenu;
