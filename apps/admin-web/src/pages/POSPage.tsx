import React, { useState } from 'react';
import { Search, Grid, List, Utensils, ClipboardList, UserPlus, MoreVertical, Plus, Minus, Printer, History, Bell, RotateCcw, X, UtensilsCrossed, LayoutGrid } from 'lucide-react';

const POSPage = ({ setActiveTab: setAppTab }: { setActiveTab: (tab: string) => void }) => {
  // Logic điều hướng nội bộ trong POS (Phòng bàn <-> Thực đơn)
  const [posTab, setPosTab] = useState('menu');
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [cart, setCart] = useState([
    { id: 1, name: 'cà phê muối', quantity: 2, price: 25000, note: 'Sữa nóng' }
  ]);

  const categories = ['Tất cả', 'cà phê', 'Đồ ăn', 'Đồ uống khác'];

  const products = [
    { id: 1, name: 'cà phê muối', price: 25000, category: 'cà phê', image: null },
    { id: 2, name: 'Bạc xỉu', price: 22000, category: 'cà phê', image: null },
    { id: 3, name: 'Trà đào cam sả', price: 35000, category: 'Đồ uống khác', image: null },
    { id: 4, name: 'Bánh mì', price: 15000, category: 'Đồ ăn', image: null },
  ];

  const totalAmount = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);

  const addToCart = (product: any) => {
    const existing = cart.find(item => item.id === product.id);
    if (existing) {
      setCart(cart.map(item => item.id === product.id ? { ...item, quantity: item.quantity + 1 } : item));
    } else {
      setCart([...cart, { ...product, quantity: 1, note: '' }]);
    }
  };

  return (
    <div className="flex h-screen bg-[#f0f2f5] overflow-hidden text-gray-800 font-sans">
      {/* LEFT SIDE: Tables / Menu Selection */}
      <div className="w-[60%] flex flex-col border-r border-gray-300 bg-white">
        {/* Header Left (Blue) */}
        <div className="bg-[#0070f4] p-1 flex items-center space-x-1">
          <button
            onClick={() => setPosTab('tables')}
            className={`px-4 py-1.5 rounded-t text-sm font-bold flex items-center transition-colors ${posTab === 'tables' ? 'bg-white text-[#0070f4]' : 'text-white hover:bg-blue-600'}`}
          >
            <Grid size={16} className="mr-2" /> Phòng bàn
          </button>
          <button
            onClick={() => setPosTab('menu')}
            className={`px-4 py-1.5 rounded-t text-sm font-bold flex items-center transition-colors ${posTab === 'menu' ? 'bg-white text-[#0070f4]' : 'text-white hover:bg-blue-600'}`}
          >
            <ClipboardList size={16} className="mr-2" /> Thực đơn
          </button>

          <div className="flex-1 relative ml-4">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-blue-200" />
            </span>
            <input
              type="text"
              className="w-full bg-blue-600/50 border-none rounded py-1.5 pl-10 pr-4 text-sm text-white placeholder-blue-200 outline-none focus:ring-1 focus:ring-white"
              placeholder="Tìm món (F3)"
            />
          </div>
          <button className="p-2 text-white hover:bg-blue-600 rounded"><Plus size={20}/></button>
          <button className="p-2 text-white hover:bg-blue-600 rounded"><MoreVertical size={20}/></button>
        </div>

        {/* Category Tabs / Filters */}
        <div className="bg-white border-b px-2 py-2 flex items-center shadow-sm overflow-x-auto no-scrollbar">
          <div className="flex space-x-2">
            {categories.map(cat => (
              <button
                key={cat}
                onClick={() => setActiveCategory(cat)}
                className={`px-4 py-1 rounded-full text-xs font-bold transition-all whitespace-nowrap ${
                  activeCategory === cat
                  ? 'bg-blue-600 text-white shadow-md'
                  : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                }`}
              >
                {cat}
              </button>
            ))}
          </div>
        </div>

        {/* Main Content Area */}
        <div className="flex-1 overflow-auto p-4 bg-gray-50">
          {posTab === 'tables' ? (
            <div className="grid grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-4">
               <div className="bg-[#0070f4] text-white rounded-lg shadow-md aspect-square flex flex-col items-center justify-center cursor-pointer hover:scale-105 transition-all border-2 border-blue-400">
                  <div className="bg-white/20 p-2 rounded-lg mb-2">
                    <Utensils size={24} />
                  </div>
                  <span className="text-xs font-bold uppercase">Mang về</span>
               </div>
               {[1,2,3,4,5].map(i => (
                 <div key={i} className="bg-white text-gray-400 rounded-lg shadow-sm aspect-square flex flex-col items-center justify-center cursor-pointer hover:border-blue-400 border-2 border-transparent transition-all">
                    <span className="text-sm font-bold">Bàn {i}</span>
                    <span className="text-[10px]">Trống</span>
                 </div>
               ))}
            </div>
          ) : (
            <div className="grid grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
              {products.filter(p => activeCategory === 'Tất cả' || p.category === activeCategory).map(product => (
                <div
                  key={product.id}
                  onClick={() => addToCart(product)}
                  className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden cursor-pointer hover:shadow-md hover:border-blue-400 transition-all group"
                >
                  <div className="aspect-square bg-gray-50 flex items-center justify-center relative">
                    <UtensilsCrossed size={32} className="text-gray-200 group-hover:text-blue-100 transition-colors" />
                    <div className="absolute top-2 right-2 bg-blue-600 text-white text-[10px] font-bold px-1.5 py-0.5 rounded shadow-sm">
                      {product.price.toLocaleString()}
                    </div>
                  </div>
                  <div className="p-2 text-center border-t">
                    <p className="text-[11px] font-bold text-gray-700 truncate capitalize">{product.name}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer Left */}
        <div className="px-4 py-2 bg-white border-t flex items-center justify-between">
           <label className="flex items-center text-[11px] text-gray-500 cursor-pointer">
              <input type="checkbox" className="mr-2" defaultChecked /> Mở thực đơn khi chọn bàn
           </label>
           <div className="text-[10px] text-gray-400 italic">Hướng dẫn</div>
        </div>
      </div>

      {/* RIGHT SIDE: Order Details */}
      <div className="flex-1 flex flex-col bg-white shadow-2xl">
        {/* Order Tabs */}
        <div className="bg-[#1e293b] flex items-center p-1 space-x-1">
          <div className="bg-white text-gray-800 px-4 py-1.5 rounded-t text-xs font-bold flex items-center shadow-sm">
            1-3 <X size={12} className="ml-2 text-gray-400 hover:text-red-500 cursor-pointer" />
          </div>
          <div className="text-white opacity-60 px-4 py-1.5 rounded-t text-xs font-bold flex items-center hover:bg-gray-700 cursor-pointer">
            1-1
          </div>
          <button className="text-white p-1.5 hover:bg-gray-700 rounded transition-colors">
            <Plus size={16} />
          </button>
          <div className="flex-1"></div>
          <div className="flex items-center space-x-3 pr-2">
             <button
               onClick={() => setAppTab('dashboard')}
               className="bg-blue-600 hover:bg-blue-700 text-white px-3 py-1 rounded text-[11px] font-bold flex items-center transition-all mr-2 shadow-lg border border-blue-400"
               title="Quay về trang quản trị"
             >
               <LayoutGrid size={14} className="mr-1" /> Quản lý
             </button>
             <Bell size={18} className="text-white opacity-60 hover:opacity-100 cursor-pointer" />
             <Printer size={18} className="text-white opacity-60 hover:opacity-100 cursor-pointer" />
             <History size={18} className="text-white opacity-60 hover:opacity-100 cursor-pointer" />
          </div>
        </div>

        {/* Order Info Bar */}
        <div className="p-2 flex items-center border-b space-x-2">
          <div className="bg-blue-100 text-blue-700 px-3 py-1 rounded text-xs font-bold flex items-center whitespace-nowrap">
             <Utensils size={14} className="mr-1" /> Mang về
          </div>
          <div className="flex-1 relative">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-gray-400" />
            </span>
            <input
              type="text"
              className="w-full bg-gray-50 border border-gray-200 rounded py-1 pl-10 pr-8 text-sm outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Tìm khách hàng (F4)"
            />
            <UserPlus size={16} className="absolute right-3 top-2 text-blue-600 cursor-pointer" />
          </div>
          <button className="p-1.5 text-gray-400 hover:bg-gray-100 rounded"><List size={18}/></button>
          <button className="p-1.5 text-gray-400 hover:bg-gray-100 rounded"><RotateCcw size={18}/></button>
        </div>

        {/* Selected Items List */}
        <div className="flex-1 overflow-auto px-4">
           {cart.length > 0 ? cart.map((item, idx) => (
             <div key={idx} className="flex items-start py-3 border-b border-gray-100 group transition-all">
                <div className="flex-1">
                   <p className="text-sm font-bold text-gray-800 capitalize">{idx + 1}. {item.name}</p>
                   {item.note && <p className="text-[10px] text-orange-500 mt-0.5 flex items-center italic font-medium">✨ {item.note}</p>}
                </div>
                <div className="flex items-center space-x-4">
                   <div className="flex items-center border rounded-md overflow-hidden bg-gray-50">
                      <button className="px-1.5 py-1 hover:bg-gray-200 text-gray-400 transition-colors"><Minus size={12} /></button>
                      <input type="text" className="w-8 bg-transparent text-center text-xs font-bold outline-none" value={item.quantity} readOnly />
                      <button className="px-1.5 py-1 hover:bg-gray-200 text-blue-600 transition-colors"><Plus size={12} /></button>
                   </div>
                   <div className="text-right w-20">
                      <p className="text-[11px] text-gray-400 font-medium">{item.price.toLocaleString()}</p>
                      <p className="text-sm font-black text-gray-800">{(item.price * item.quantity).toLocaleString()}</p>
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

        {/* Footer Order Actions */}
        <div className="p-4 border-t bg-white shadow-[0_-10px_15px_-3px_rgba(0,0,0,0.1)]">
           <div className="flex justify-between items-center mb-4">
              <div className="flex items-center">
                 <span className="text-xs font-bold text-gray-500 mr-2">Đinh Công Nghĩa</span>
                 <MoreVertical size={14} className="text-gray-400 cursor-pointer" />
              </div>
              <div className="flex items-center space-x-2">
                 <span className="text-sm font-bold text-gray-500">Tổng tiền</span>
                 <div className="flex items-center">
                    <span className="bg-blue-600 text-white rounded-full w-4 h-4 flex items-center justify-center text-[10px] mr-2 shadow-sm font-bold">?</span>
                    <span className="text-2xl font-black text-blue-700 tracking-tighter">{totalAmount.toLocaleString()}</span>
                 </div>
              </div>
           </div>

           <div className="grid grid-cols-12 gap-3">
              <button className="col-span-4 bg-blue-100 text-blue-700 py-3 rounded-lg font-bold flex flex-col items-center justify-center hover:bg-blue-200 transition-all border border-blue-200">
                 <Bell size={20} className="mb-0.5" />
                 <span className="text-[11px]">Thông báo (F10)</span>
              </button>
              <button className="col-span-8 bg-[#0070f4] text-white py-3 rounded-lg font-bold flex items-center justify-center space-x-3 hover:bg-blue-700 transition-all shadow-lg active:scale-95 border-b-4 border-blue-800">
                 <span className="bg-white/20 p-2 rounded-full"><RotateCcw size={20} className="rotate-90"/></span>
                 <span className="text-xl uppercase tracking-widest font-black italic">Thanh toán (F9)</span>
              </button>
           </div>
        </div>
      </div>
    </div>
  );
};

export default POSPage;
