import React, { useState, useEffect } from 'react';
import { ShoppingBag, ChevronRight, Star, Plus, Minus, Search, Loader2, CheckCircle2, Clock, MapPin, X, Gift, Percent } from 'lucide-react';
import { API_URL } from '../config';

interface Product {
  id: string;
  name: string;
  price: number;
  category: string;
  group?: string;
  imageUrl?: string;
  description?: string;
  sizesJson?: string;
  toppingsJson?: string;
  availabilityStatus?: string;
}

interface CartItem extends Product {
  quantity: number;
  selectedSize?: any;
  selectedToppings?: any[];
  totalItemPrice: number;
  note?: string; // Thêm trường ghi chú cho khách
}

const DigitalMenu = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<string[]>(['Tất cả']);
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [loading, setLoading] = useState(true);
  const [isSubmittingOrder, setIsSubmittingOrder] = useState(false);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [cartPulse, setCartPulse] = useState(false);
  const [isOrderSuccess, setIsOrderSuccess] = useState(false);
  const [tableInfo, setTableInfo] = useState<{id: string, name: string, branchId?: string, branchName?: string} | null>(null);
  const [branches, setBranches] = useState<any[]>([]);
  const [isBranchModalOpen, setIsBranchModalOpen] = useState(false);
  const [selectedBranch, setSelectedBranch] = useState<any>(null);

  // States cho Loyalty
  const [currentLoyaltyPoints, setCurrentLoyaltyPoints] = useState<number>(0);
  const [customerInfo, setCustomerInfo] = useState<any>(null);

  // States cho Khuyến mãi
  const [promotions, setPromotions] = useState<any[]>([]);
  const [isPromoModalOpen, setIsPromoModalOpen] = useState(false);

  // States cho tùy chọn món (Size & Topping)
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [currentCustomizingProduct, setCurrentCustomizingProduct] = useState<Product | null>(null);
  const [selectedSize, setSelectedSize] = useState<any>(null);
  const [selectedToppings, setSelectedToppings] = useState<any[]>([]);
  const [customizingQuantity, setCustomizingQuantity] = useState(1);

  // Lấy tableId từ URL ?tableId=...
  const urlParams = new URLSearchParams(window.location.search);
  const tableId = urlParams.get('tableId');

  useEffect(() => {
    fetchProducts();
    fetchBranches();
    fetchPromotions();
    loadCustomerInfo();
  }, []);

  const loadCustomerInfo = async () => {
    try {
      const saved = localStorage.getItem('customerInfo');
      if (saved) {
        const info = JSON.parse(saved);
        setCustomerInfo(info);
        setCurrentLoyaltyPoints(info.loyaltyPoints || 0);

        // Fetch latest from API
        if (info.phoneNumber) {
          const res = await fetch(`${API_URL}/api/Customer/${info.phoneNumber}`);
          if (res.ok) {
             const latest = await res.json();
             setCustomerInfo(latest);
             setCurrentLoyaltyPoints(latest.loyaltyPoints || 0);
             localStorage.setItem('customerInfo', JSON.stringify(latest));
          }
        }
      }
    } catch (e) { console.error(e); }
  };

  // Cập nhật thông tin bàn và xóa giỏ hàng nếu quét mã bàn mới
  useEffect(() => {
    if (tableId) {
      // Nếu tableId thay đổi so với ID đang lưu, ta xóa giỏ hàng để tránh đặt nhầm
      if (tableInfo && tableInfo.id !== tableId) {
        setCart([]);
        setIsOrderSuccess(false);
      }
      fetchTableInfo();
    }
  }, [tableId]);

  const fetchPromotions = async () => {
    try {
      const res = await fetch(`${API_URL}/api/Promotion`);
      if (res.ok) {
        const data = await res.json();
        setPromotions(data.filter((p: any) => p.isActive));
      }
    } catch (e) { console.error(e); }
  };

  useEffect(() => {
    if (cart.length === 0 && isBranchModalOpen) {
      setIsBranchModalOpen(false);
    }
  }, [cart, isBranchModalOpen]);

  const fetchProducts = async () => {
    try {
      setLoading(true);

      // Lấy đồng thời Sản phẩm và Topping
      const [prodRes, toppingRes] = await Promise.all([
        fetch(`${API_URL}/api/Product`),
        fetch(`${API_URL}/api/Topping`)
      ]);

      if (!prodRes.ok || !toppingRes.ok) {
        throw new Error(`Menu API failed: ${prodRes.status}/${toppingRes.status}`);
      }

      const productsData = await prodRes.json();
      const toppingsData = await toppingRes.json();

      if (!Array.isArray(productsData) || !Array.isArray(toppingsData)) {
        throw new Error('Menu API returned an invalid response.');
      }

      // Biến đổi Topping thành định dạng giống Sản phẩm để hiển thị chung
      const toppingProducts = toppingsData.map((t: any) => ({
        id: t.id,
        name: t.name,
        price: t.price,
        category: 'Topping', // Cố định danh mục Topping
        group: t.category, // Lấy Nhóm của Topping làm Group
        description: t.description,
        imageUrl: t.imageUrl,
        isStandaloneTopping: true // Đánh dấu đây là topping bán lẻ
      }));

      const sellableProducts = productsData.filter((product: any) =>
        product.isActive !== false && (!product.availabilityStatus || product.availabilityStatus === 'Available'));
      const allItems = [...sellableProducts, ...toppingProducts];
      setProducts(allItems);

      const cats = Array.from(new Set(allItems.map((p: any) => p.category).filter((c: any) => !!c))) as string[];
      setCategories(['Tất cả', ...cats]);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = window.setInterval(() => void fetchProducts(), 10000);
    return () => window.clearInterval(timer);
  }, []);

  const fetchBranches = async () => {
    try {
      const response = await fetch(`${API_URL}/api/Branch`);
      const data = await response.json();
      setBranches(data);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchTableInfo = async () => {
    if (!tableId) return;
    try {
      const response = await fetch(`${API_URL}/api/Table/${tableId}`);
      if (response.ok) {
        const currentTable = await response.json();
        setTableInfo({
          id: currentTable.id,
          name: currentTable.name,
          branchId: currentTable.branchId,
          branchName: currentTable.branchName
        });

        // Cố định chi nhánh theo bàn
        if (currentTable.branchId) {
          setSelectedBranch({ id: currentTable.branchId, name: currentTable.branchName });
        }
      } else {
         // Nếu tableId không tồn tại hoặc lỗi, reset thông tin
         setTableInfo(null);
      }
    } catch (err) {
      console.error(err);
    }
  };

  const addToCart = (product: Product) => {
    let productSizes = [];
    let productToppings = [];

    try {
      if (product.sizesJson) productSizes = JSON.parse(product.sizesJson);
      if (product.toppingsJson) productToppings = JSON.parse(product.toppingsJson);
    } catch (e) {
      console.error("Error parsing product options:", e);
    }

    // Nếu món có tùy chọn, luôn mở Modal để khách chọn (không tự động tăng số lượng món cũ)
    if (productSizes.length > 0 || productToppings.length > 0) {
       setCurrentCustomizingProduct(product);
       setSelectedSize(productSizes.length > 0 ? productSizes[0] : null);
       setSelectedToppings([]);
       setCustomizingQuantity(1);
       setIsOptionsModalOpen(true);
       return;
    }

    // Nếu món không có tùy chọn, mới thực hiện tăng số lượng nhanh
    const existingInCart = cart.find(item => item.id === product.id);
    if (existingInCart) {
      updateQuantityByItem(existingInCart, 1);
      return;
    }

    setCart(prev => [...prev, { ...product, quantity: 1, totalItemPrice: product.price }]);
  };

  const handleConfirmOptions = () => {
    if (!currentCustomizingProduct) return;

    const toppingPrice = selectedToppings.reduce((sum, t) => sum + t.price, 0);
    const sizePrice = selectedSize ? selectedSize.price : 0;
    const finalUnitPrice = currentCustomizingProduct.price + sizePrice + toppingPrice;

    setCart(prev => [
       ...prev,
       {
          ...currentCustomizingProduct,
          quantity: customizingQuantity,
          selectedSize,
          selectedToppings,
          totalItemPrice: finalUnitPrice
       }
    ]);

    setIsOptionsModalOpen(false);
    setCurrentCustomizingProduct(null);
  };

  const updateQuantityByItem = (item: CartItem, delta: number) => {
    setCart(prev => prev.map(cartItem => {
      // So sánh chính xác dựa trên ID và các tùy chọn (Size, Toppings)
      const isSameItem = cartItem.id === item.id &&
                        JSON.stringify(cartItem.selectedSize) === JSON.stringify(item.selectedSize) &&
                        JSON.stringify(cartItem.selectedToppings) === JSON.stringify(item.selectedToppings);

      if (isSameItem) {
        const newQty = cartItem.quantity + delta;
        return newQty > 0 ? { ...cartItem, quantity: newQty } : cartItem;
      }
      return cartItem;
    }).filter(cartItem => cartItem.quantity > 0));
  };

  const totalAmount = cart.reduce((sum, item) => sum + (item.totalItemPrice * item.quantity), 0);
  const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);

  useEffect(() => {
    if (totalItems === 0) return;
    setCartPulse(true);
    const timer = window.setTimeout(() => setCartPulse(false), 520);
    return () => window.clearTimeout(timer);
  }, [totalItems]);

  const removeFromCart = (idOrItem: string | CartItem) => {
    if (typeof idOrItem === 'string') {
      setCart(prev => prev.filter(item => item.id !== idOrItem));
    } else {
      setCart(prev => prev.filter(cartItem => {
        const isSameItem = cartItem.id === idOrItem.id &&
                          JSON.stringify(cartItem.selectedSize) === JSON.stringify(idOrItem.selectedSize) &&
                          JSON.stringify(cartItem.selectedToppings) === JSON.stringify(idOrItem.selectedToppings);
        return !isSameItem;
      }));
    }
  };

  const handleOrder = () => {
    if (cart.length === 0) return;
    setIsBranchModalOpen(true);
  };

  const updateNote = (item: CartItem, note: string) => {
    setCart(prev => prev.map(cartItem => {
      const isSameItem = cartItem.id === item.id &&
                        JSON.stringify(cartItem.selectedSize) === JSON.stringify(item.selectedSize) &&
                        JSON.stringify(cartItem.selectedToppings) === JSON.stringify(item.selectedToppings);
      return isSameItem ? { ...cartItem, note } : cartItem;
    }));
  };

  const confirmOrder = async (branch: any) => {
    if (isSubmittingOrder || !branch?.id || cart.length === 0) return;

    try {
      setIsSubmittingOrder(true);
      let customerInfo: any = {};
      try {
        customerInfo = JSON.parse(localStorage.getItem('customerInfo') || '{}');
      } catch {
        customerInfo = {};
      }
      const order = {
        tableName: tableInfo?.name || 'Khách vãng lai',
        totalAmount: totalAmount,
        paidAmount: 0,
        status: 'Đang xử lý',
        customerName: customerInfo.fullName || 'Khách tại bàn',
        customerPhone: customerInfo.phoneNumber,
        customerEmail: customerInfo.email,
        customerId: customerInfo.id,
        branchId: branch.id,
        branchName: branch.name,
        details: cart.map(item => ({
          productId: (item as any).isStandaloneTopping ? null : item.id,
          toppingId: (item as any).isStandaloneTopping ? item.id : null,
          productName: `${item.name}${item.selectedSize ? ` (${item.selectedSize.name})` : ''}`,
          quantity: item.quantity,
          unitPrice: item.totalItemPrice,
          options: [
            ...(item.selectedToppings?.map(t => t.name) || []),
            ...(item.note ? [`Ghi chú: ${item.note}`] : [])
          ].join(', ')
        }))
      };

      const response = await fetch(`${API_URL}/api/Order`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(order)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        const message = typeof errorData === 'string'
          ? errorData
          : errorData?.message || errorData?.title || 'Không thể gửi yêu cầu gọi món.';
        throw new Error(message);
      }

      // Cập nhật trạng thái bàn nếu có tableId. Order đã được lưu thành công;
      // lỗi phụ ở trạng thái bàn không được làm mất thông báo đặt món.
      if (tableId) {
        const tableResponse = await fetch(`${API_URL}/api/Table/${tableId}/status`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify('Có khách')
        });
        if (!tableResponse.ok) console.warn('Could not update table status after order creation.');
      }

      setCart([]);
      setIsBranchModalOpen(false);
      setIsOrderSuccess(true);
      setTimeout(() => setIsOrderSuccess(false), 5000);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Lỗi đặt món. Vui lòng thử lại!';
      alert(message);
    } finally {
      setIsSubmittingOrder(false);
    }
  };

  const handleRedeem = async (promo: any) => {
    const customerInfo = JSON.parse(localStorage.getItem('customerInfo') || '{}');
    if (!customerInfo.phoneNumber) {
      alert("Vui lòng đăng nhập để đổi quà!");
      return;
    }

    if (!window.confirm(`Bạn có chắc muốn dùng ${promo.requiredPoints} điểm để đổi "${promo.name}"?`)) return;

    try {
      const order = {
        tableName: tableInfo?.name || 'Khách tại bàn',
        totalAmount: 0,
        status: 'Đổi quà',
        customerName: customerInfo.fullName,
        customerPhone: customerInfo.phoneNumber,
        branchId: tableInfo?.branchId || selectedBranch?.id,
        branchName: tableInfo?.branchName || selectedBranch?.name,
        details: [{
          productName: `[ĐỔI QUÀ] ${promo.name}`,
          quantity: 1,
          unitPrice: 0,
          options: `Trừ ${promo.requiredPoints} điểm`
        }]
      };

      const res = await fetch(`${API_URL}/api/Order`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(order)
      });

      if (res.ok) {
        alert("Yêu cầu đổi quà đã được gửi! Vui lòng đợi nhân viên xác nhận.");
        setIsPromoModalOpen(false);
      }
    } catch (e) {
      console.error(e);
      alert("Lỗi khi đổi quà.");
    }
  };

  const filteredProducts = products.filter(p =>
    activeCategory === 'Tất cả' || p.category === activeCategory
  );

  return (
    <div className="min-h-screen bg-gray-50 font-sans text-gray-900 pb-24">
      {/* Header Image */}
      <div className="hero-sheen h-56 bg-blue-600 relative overflow-hidden">
        <div className="hero-orb hero-orb-one"></div><div className="hero-orb hero-orb-two"></div>
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent"></div>
        <img
          src="https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800"
          className="w-full h-full object-cover opacity-60"
          alt="Restaurant"
        />
        <div className="absolute bottom-6 left-6 text-white">
          <span className="bg-white/20 backdrop-blur-md px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border border-white/30 mb-2 inline-block italic">DOAN RESTAURANT</span>
          <h1 className="text-3xl font-black uppercase italic tracking-tighter drop-shadow-lg">
             {tableInfo ? `Bàn: ${tableInfo.name}` : 'Thực đơn điện tử'}
          </h1>
          <p className="text-xs opacity-90 flex items-center mt-1">
             <span className="w-2 h-2 bg-green-400 rounded-full mr-2 animate-pulse"></span> Đang mở cửa • Đặt món nhanh
          </p>
        </div>
        {promotions.length > 0 && (
          <button
            onClick={() => setIsPromoModalOpen(true)}
            className="absolute top-6 right-6 bg-orange-500 text-white p-3 rounded-2xl shadow-lg gift-wiggle border-2 border-white/20"
          >
            <Gift size={24} />
          </button>
        )}
      </div>

      {/* Categories */}
      <div className="flex space-x-3 p-4 overflow-x-auto no-scrollbar sticky top-0 bg-white/80 backdrop-blur-lg z-20 border-b shadow-sm">
        {categories.map((cat) => (
          <button
            key={cat}
            onClick={() => setActiveCategory(cat)}
            className={`whitespace-nowrap px-6 py-2.5 rounded-2xl text-xs font-black uppercase tracking-widest transition-all ${
              activeCategory === cat ? 'category-active bg-blue-600 text-white shadow-lg shadow-blue-500/30' : 'bg-gray-100 text-gray-400 hover:bg-gray-200 hover:-translate-y-0.5'
            }`}
          >
            {cat}
          </button>
        ))}
      </div>

      {/* Product List */}
      <div className="p-4 space-y-6">
        <div className="flex justify-between items-center">
           <h2 className="text-xl font-black italic uppercase tracking-tighter text-gray-800">
             {activeCategory === 'Tất cả' ? 'Gợi ý cho bạn' : activeCategory}
           </h2>
           <span className="text-[10px] font-bold text-gray-400 bg-gray-100 px-2 py-1 rounded uppercase tracking-widest">{filteredProducts.length} món</span>
        </div>

        {loading ? (
          <div className="py-20 flex flex-col items-center justify-center text-gray-400">
             <Loader2 className="animate-spin mb-2 text-blue-600" />
             <p className="text-xs font-bold uppercase tracking-widest">Đang tải thực đơn...</p>
          </div>
        ) : filteredProducts.map((p) => (
          <div key={p.id} style={{ animationDelay: `${Math.min(filteredProducts.indexOf(p), 8) * 55}ms` }} className="product-card bg-white p-4 rounded-3xl shadow-xl shadow-blue-500/5 flex space-x-4 border border-gray-100 group transition-all active:scale-[0.98] hover:-translate-y-1 hover:shadow-2xl">
            <div className="w-28 h-28 bg-gray-50 rounded-2xl flex-shrink-0 relative overflow-hidden border border-gray-100">
               {p.imageUrl ? (
                 <img src={p.imageUrl} alt={p.name} className="w-full h-full object-cover" />
               ) : (
                 <div className="w-full h-full flex items-center justify-center text-blue-200">
                    <Star size={32} />
                 </div>
               )}
            </div>
            <div className="flex-1 flex flex-col justify-between">
              <div>
                <div className="flex justify-between items-start">
                  <div>
                    <h3 className="font-black text-gray-800 text-sm leading-tight group-hover:text-blue-600 transition-colors">{p.name}</h3>
                    <p className="text-[9px] font-bold text-blue-500 uppercase tracking-widest mt-0.5">{p.group}</p>
                  </div>
                  <span className="text-blue-600 font-black text-sm tracking-tighter">{p.price.toLocaleString()}đ</span>
                </div>
                <p className="text-[10px] text-gray-400 mt-1 line-clamp-2 leading-relaxed font-medium italic">
                  {p.description || 'Hương vị thơm ngon tinh tế, chế biến từ nguyên liệu tươi sạch trong ngày.'}
                </p>

                {/* Indicators for options */}
                <div className="flex gap-1 mt-2">
                   {(p.sizesJson && p.sizesJson !== '[]') && (
                     <span className="text-[8px] bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded font-black uppercase border border-blue-100">Kích cỡ</span>
                   )}
                   {(p.toppingsJson && p.toppingsJson !== '[]') && (
                     <span className="text-[8px] bg-orange-50 text-orange-600 px-1.5 py-0.5 rounded font-black uppercase border border-orange-100">Topping</span>
                   )}
                </div>
              </div>

              <div className="flex justify-end mt-2 items-center space-x-2">
                 {cart.find(item => item.id === p.id) ? (
                    <>
                       <button
                         onClick={() => removeFromCart(p.id)}
                         className="p-2 text-red-500 hover:bg-red-50 rounded-xl transition-colors"
                         title="Hủy món"
                       >
                          <X size={18} />
                       </button>
                       <div className="flex items-center bg-blue-50 rounded-xl p-1 border border-blue-100">
                          <button
                            onClick={() => {
                               const item = cart.find(i => i.id === p.id);
                               if (item) updateQuantityByItem(item, -1);
                            }}
                            className="w-8 h-8 flex items-center justify-center text-blue-600 hover:bg-blue-100 rounded-lg"
                          >
                             <Minus size={14}/>
                          </button>
                          <span className="px-3 text-xs font-black text-blue-700">
                             {cart.filter(item => item.id === p.id).reduce((sum, i) => sum + i.quantity, 0)}
                          </span>
                          <button onClick={() => addToCart(p)} className="w-8 h-8 flex items-center justify-center text-blue-600 hover:bg-blue-100 rounded-lg"><Plus size={14}/></button>
                       </div>
                    </>
                 ) : (
                    <button
                      onClick={() => addToCart(p)}
                      className="add-button bg-blue-600 text-white w-10 h-10 rounded-2xl flex items-center justify-center shadow-lg shadow-blue-500/20 active:scale-90 transition-all hover:rotate-6 hover:bg-orange-500"
                    >
                      <Plus size={20} />
                    </button>
                 )}
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Floating Order Success Toast */}
      {isOrderSuccess && (
        <div className="fixed top-24 left-6 right-6 z-[100] bg-green-600 text-white p-4 rounded-2xl shadow-2xl flex items-center space-x-3 animate-in slide-in-from-top-10 duration-500">
           <div className="bg-white/20 p-2 rounded-full"><CheckCircle2 size={24}/></div>
           <div>
              <p className="font-black uppercase text-xs tracking-widest">Đặt món thành công!</p>
              <p className="text-[10px] opacity-80">Nhân viên đang chuẩn bị món cho bạn.</p>
           </div>
        </div>
      )}

      {/* Floating Cart Button */}
      {cart.length > 0 && (
        <div className={`fixed bottom-20 left-6 right-6 bg-blue-600 text-white p-5 rounded-[2.5rem] shadow-2xl flex justify-between items-center z-50 animate-in slide-in-from-bottom-10 border-b-4 border-blue-800 ${cartPulse ? 'cart-pulse' : ''}`}>
          <div className="flex items-center">
            <div className="bg-white/20 p-3 rounded-2xl mr-4 relative">
              <ShoppingBag size={24} />
              <span className="absolute -top-1 -right-1 bg-white text-blue-600 text-[10px] font-black w-5 h-5 flex items-center justify-center rounded-full border-2 border-blue-600">{totalItems}</span>
            </div>
            <div className="text-left">
              <div className="flex flex-col mb-0.5">
                <p className="text-[10px] opacity-70 font-black uppercase tracking-widest">Tổng cộng ({totalItems} món)</p>
                <div className="flex items-center space-x-2">
                   {totalAmount >= 10000 && (
                      <div className="flex items-center bg-yellow-400 text-blue-900 px-1.5 py-0.5 rounded-lg space-x-1">
                         <Star size={8} className="fill-blue-900" />
                         <span className="text-[8px] font-black">+{Math.floor(totalAmount / 10000)} ĐIỂM</span>
                      </div>
                   )}
                   {customerInfo && !customerInfo.isGuest && (
                      <div className="flex items-center bg-white/20 text-white px-1.5 py-0.5 rounded-lg space-x-1 border border-white/20">
                         <Star size={8} className="fill-white" />
                         <span className="text-[8px] font-black">{currentLoyaltyPoints.toLocaleString()} ĐIỂM CÓ SẴN</span>
                      </div>
                   )}
                </div>
              </div>
              <p className="font-black text-xl tracking-tighter italic">{totalAmount.toLocaleString()}đ</p>
            </div>
          </div>
          <button
            onClick={handleOrder}
            className="bg-white text-blue-600 px-8 py-3.5 rounded-3xl font-black text-xs uppercase tracking-widest shadow-lg active:scale-95 transition-all flex items-center"
          >
            ĐẶT MÓN NGAY
            <ChevronRight size={16} className="ml-1" />
          </button>
        </div>
      )}

      {/* Footer Info */}
      <div className="p-10 text-center opacity-30">
         <div className="inline-block p-4 border-2 border-gray-300 rounded-3xl mb-4">
            <X size={32} className="text-gray-400"/>
         </div>
         <p className="text-[10px] font-bold uppercase tracking-widest">Cám ơn quý khách đã tin dùng</p>
         <p className="text-[8px] font-medium mt-1 uppercase">DOAN Restaurant POS • Powered by DoanDev</p>
      </div>

      {/* PROMOTION MODAL */}
      {isPromoModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex items-center justify-center p-6 animate-in fade-in duration-300 backdrop-blur-sm">
           <div className="bg-white w-full max-w-sm rounded-[3rem] p-8 shadow-2xl relative animate-in zoom-in-95 duration-300 max-h-[80vh] flex flex-col">
              <button onClick={() => setIsPromoModalOpen(false)} className="absolute top-6 right-6 text-gray-300 hover:text-gray-600"><X size={20}/></button>

              <div className="text-center mb-6 flex-shrink-0">
                 <div className="w-20 h-20 bg-orange-50 rounded-[2rem] flex items-center justify-center text-orange-600 mx-auto mb-4 border-4 border-white shadow-xl">
                    <Gift size={32} />
                 </div>
                 <h3 className="text-xl font-black text-gray-800 uppercase italic tracking-tighter">Ưu đãi & Đổi quà</h3>
                 <p className="text-[10px] text-gray-400 font-bold uppercase tracking-widest mt-1">Dùng điểm tích lũy để nhận quà tặng</p>
              </div>

              <div className="flex-1 overflow-y-auto pr-2 no-scrollbar space-y-4 mb-6">
                 {promotions.map((p) => (
                    <div key={p.id} className="bg-gray-50 p-5 rounded-[2rem] border border-gray-100 group relative overflow-hidden">
                       <div className="absolute top-0 right-0 bg-orange-500 text-white px-3 py-1 text-[9px] font-black uppercase tracking-widest rounded-bl-xl shadow-md">
                          {p.requiredPoints} ĐIỂM
                       </div>
                       <div className="flex items-start space-x-3">
                          <div className={`p-2.5 rounded-xl ${p.promotionType === 'Gift' ? 'bg-orange-100 text-orange-600' : 'bg-blue-100 text-blue-600'}`}>
                             {p.promotionType === 'Gift' ? <Gift size={20}/> : <Percent size={20}/>}
                          </div>
                          <div>
                             <p className="text-sm font-black text-gray-800 leading-tight mb-1">{p.name}</p>
                             <p className="text-[10px] text-gray-400 font-medium italic line-clamp-2">{p.description}</p>
                          </div>
                       </div>
                       <button
                          onClick={() => {
                             const customerInfo = JSON.parse(localStorage.getItem('customerInfo') || '{}');
                             const points = customerInfo.loyaltyPoints || 0;
                             if (points < p.requiredPoints) {
                                alert(`Bạn cần thêm ${p.requiredPoints - points} điểm để đổi quà này!`);
                             } else {
                                handleRedeem(p);
                             }
                          }}
                          className="w-full mt-4 py-2.5 bg-white border border-orange-200 text-orange-600 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-orange-500 hover:text-white transition-all shadow-sm"
                       >
                          ĐỔI QUÀ NGAY
                       </button>
                    </div>
                 ))}
              </div>

              <button
                 onClick={() => setIsPromoModalOpen(false)}
                 className="w-full py-4 bg-gray-100 text-gray-500 rounded-2xl font-black uppercase tracking-widest text-xs hover:bg-gray-200 transition-all flex-shrink-0"
              >
                 Đóng
              </button>
           </div>
        </div>
      )}

      {/* BRANCH SELECTION MODAL */}
      {isBranchModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[200] flex items-end justify-center animate-in fade-in duration-300 backdrop-blur-sm">
           <div className="bg-white w-full max-w-lg rounded-t-[3rem] p-8 pb-12 shadow-2xl animate-in slide-in-from-bottom-20 duration-500">
              <div className="flex justify-between items-start mb-6">
                 <div>
                    <h3 className="text-2xl font-black text-gray-800 uppercase italic tracking-tighter">Xác nhận đơn hàng</h3>
                    <p className="text-xs font-bold text-blue-600 uppercase tracking-widest mt-1">Kiểm tra lại món & chọn cơ sở</p>
                 </div>
                 <button onClick={() => setIsBranchModalOpen(false)} className="bg-gray-100 p-3 rounded-full hover:bg-gray-200 transition-all text-gray-400"><X size={24}/></button>
              </div>

              <div className="mb-6">
                 <div className="flex justify-between items-center mb-3">
                    <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Danh sách món đã chọn</h4>
                    <span className="text-[9px] bg-blue-100 text-blue-600 px-2 py-0.5 rounded-full font-black">{totalItems} món</span>
                 </div>
                 <div className="space-y-3 max-h-[25vh] overflow-y-auto pr-2 no-scrollbar">
                    {cart.map((item, idx) => (
                       <div key={idx} className="flex justify-between items-center bg-gray-50/50 p-3 rounded-2xl border border-gray-100 group">
                          <div className="flex items-center space-x-3">
                             <div className="w-10 h-10 bg-white rounded-xl flex items-center justify-center text-blue-600 font-black text-xs border border-gray-100 shadow-sm">
                                {item.quantity}x
                             </div>
                             <div>
                                <p className="text-xs font-black text-gray-800 leading-tight">{item.name}</p>
                                <div className="flex flex-wrap gap-1 mt-0.5">
                                   {item.selectedSize && (
                                      <span className="text-[8px] bg-blue-50 text-blue-500 px-1.5 py-0.5 rounded font-black uppercase border border-blue-100">Size: {item.selectedSize.name}</span>
                                   )}
                                   {item.selectedToppings && item.selectedToppings.length > 0 && (
                                      <span className="text-[8px] bg-orange-50 text-orange-600 px-1.5 py-0.5 rounded font-bold italic border border-orange-100">
                                         + {item.selectedToppings.map(t => t.name).join(', ')}
                                      </span>
                                   )}
                                </div>
                                <input
                                  type="text"
                                  placeholder="Ghi chú (VD: Bỏ vào trà sữa...)"
                                  className="w-full mt-2 bg-white border border-dashed border-gray-200 rounded-lg px-2 py-1 text-[10px] italic font-medium text-orange-600 outline-none focus:border-orange-300"
                                  value={item.note || ''}
                                  onChange={(e) => updateNote(item, e.target.value)}
                                />
                             </div>
                          </div>
                          <div className="text-right flex flex-col items-end">
                             <p className="text-[10px] text-gray-400 font-bold">{item.totalItemPrice.toLocaleString()}đ</p>
                             <p className="text-xs font-black text-gray-700 tracking-tighter">{(item.totalItemPrice * item.quantity).toLocaleString()}đ</p>
                             <button
                                onClick={() => removeFromCart(item)}
                                className="text-[9px] font-bold text-red-400 hover:text-red-600 mt-1 uppercase"
                             >
                                Xóa
                             </button>
                          </div>
                       </div>
                    ))}
                 </div>
              </div>

              <div className="mb-4">
                 <h4 className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">
                    {tableInfo ? 'Chi nhánh phục vụ (Cố định theo bàn)' : 'Chọn chi nhánh phục vụ'}
                 </h4>
                 <div className="space-y-3 max-h-[25vh] overflow-y-auto pr-2 no-scrollbar">
                    {branches.filter(b => !tableInfo || b.id === tableInfo.branchId).map((b) => (
                       <button
                         key={b.id}
                         disabled={!!tableInfo}
                         onClick={() => setSelectedBranch(b)}
                         className={`w-full p-4 rounded-2xl border-2 flex items-center justify-between transition-all ${
                            selectedBranch?.id === b.id
                            ? 'border-blue-600 bg-blue-50 shadow-md ring-4 ring-blue-100'
                            : 'border-gray-50 bg-gray-50/50 hover:border-blue-200'
                         } ${tableInfo ? 'cursor-default' : 'cursor-pointer'}`}
                       >
                          <div className="flex items-center">
                             <div className={`w-12 h-12 rounded-xl mr-3 overflow-hidden border ${selectedBranch?.id === b.id ? 'border-blue-200 shadow-sm' : 'border-white'} flex items-center justify-center bg-white`}>
                                {b.imageUrl ? (
                                   <img src={b.imageUrl} className="w-full h-full object-cover" alt=""/>
                                ) : (
                                   <MapPin size={18} className={selectedBranch?.id === b.id ? 'text-blue-600' : 'text-gray-400'}/>
                                )}
                             </div>
                             <div className="text-left">
                                <p className={`text-xs font-black uppercase tracking-tight ${selectedBranch?.id === b.id ? 'text-blue-700' : 'text-gray-700'}`}>{b.name}</p>
                                <p className="text-[9px] text-gray-400 font-bold mt-0.5 line-clamp-1">{b.address || 'Địa chỉ đang cập nhật...'}</p>
                             </div>
                          </div>
                          {selectedBranch?.id === b.id && <CheckCircle2 size={18} className="text-blue-600" />}
                       </button>
                    ))}
                 </div>
              </div>

              <div className="mt-8 space-y-4">
                 <div className="bg-gray-50 p-4 rounded-2xl border border-dashed border-gray-200">
                    <div className="flex justify-between text-[11px] font-bold text-gray-400 uppercase tracking-widest">
                       <span>Đơn tại: {tableInfo?.name || 'Mang về'}</span>
                       <span>{totalItems} món</span>
                    </div>
                    <div className="flex justify-between items-center mt-2">
                       <span className="text-sm font-black text-gray-800 italic uppercase">Tổng thanh toán:</span>
                       <div className="text-right">
                          <span className="text-xl font-black text-blue-700">{totalAmount.toLocaleString()}đ</span>
                          <div className="flex flex-col items-end mt-1 space-y-1">
                             {totalAmount >= 10000 && (
                                <p className="text-[9px] font-black text-orange-500 uppercase flex items-center justify-end">
                                   <Star size={10} className="fill-orange-500 mr-1" />
                                   Tích lũy: +{Math.floor(totalAmount / 10000)} điểm
                                </p>
                             )}
                             {customerInfo && !customerInfo.isGuest && (
                                <p className="text-[9px] font-black text-blue-600 uppercase flex items-center justify-end bg-blue-50 px-2 py-0.5 rounded-full border border-blue-100">
                                   <Star size={10} className="fill-blue-600 mr-1" />
                                   Hiện có: {currentLoyaltyPoints.toLocaleString()} điểm
                                </p>
                             )}
                          </div>
                       </div>
                    </div>
                 </div>

                 <button
                   disabled={!selectedBranch || isSubmittingOrder}
                   onClick={() => confirmOrder(selectedBranch)}
                   className="w-full py-5 bg-blue-600 text-white rounded-[2rem] font-black text-sm uppercase tracking-[0.2em] shadow-xl shadow-blue-500/30 hover:bg-blue-700 transition-all active:scale-95 disabled:opacity-30 disabled:grayscale flex items-center justify-center"
                 >
                   {isSubmittingOrder ? 'ĐANG GỬI YÊU CẦU...' : 'GỬI YÊU CẦU GỌI MÓN'}
                   {isSubmittingOrder ? <Loader2 size={20} className="ml-2 animate-spin" /> : <ChevronRight size={20} className="ml-2" />}
                 </button>
              </div>
           </div>
        </div>
      )}

      {/* OPTIONS MODAL (SIZE & TOPPING) */}
      {isOptionsModalOpen && currentCustomizingProduct && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex items-end justify-center animate-in fade-in duration-300 backdrop-blur-sm">
           <div className="bg-white w-full max-w-lg rounded-t-[3rem] shadow-2xl animate-in slide-in-from-bottom-20 duration-500 max-h-[95vh] flex flex-col">
              {/* Sticky Header */}
              <div className="relative h-48 flex-shrink-0">
                 <img
                    src={currentCustomizingProduct.imageUrl || 'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800'}
                    className="w-full h-full object-cover rounded-t-[3rem]"
                    alt=""
                 />
                 <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent"></div>
                 <button
                    onClick={() => setIsOptionsModalOpen(false)}
                    className="absolute top-6 right-6 bg-white/20 backdrop-blur-md p-2 rounded-full text-white hover:bg-white/40 transition-all"
                 >
                    <X size={20}/>
                 </button>
                 <div className="absolute bottom-6 left-8 text-white">
                    <h3 className="text-2xl font-black uppercase italic tracking-tighter">{currentCustomizingProduct.name}</h3>
                    <p className="text-xs opacity-80 font-bold uppercase tracking-widest">{currentCustomizingProduct.category}</p>
                 </div>
              </div>

              {/* Scrollable Content */}
              <div className="flex-1 overflow-y-auto p-8 pt-6 no-scrollbar space-y-10">
                 {/* Size Selection */}
                 {currentCustomizingProduct.sizesJson && JSON.parse(currentCustomizingProduct.sizesJson).length > 0 && (
                    <div className="space-y-4">
                       <div className="flex justify-between items-end">
                          <label className="text-[11px] font-black text-gray-400 uppercase tracking-[0.2em]">Chọn kích cỡ</label>
                          <span className="text-[9px] bg-blue-100 text-blue-600 px-2 py-0.5 rounded font-black uppercase">Bắt buộc</span>
                       </div>
                       <div className="grid grid-cols-1 gap-3">
                          {JSON.parse(currentCustomizingProduct.sizesJson).map((s: any, i: number) => (
                             <button
                                key={i}
                                onClick={() => setSelectedSize(s)}
                                className={`p-4 rounded-[1.5rem] border-2 transition-all flex items-center justify-between group ${
                                   selectedSize?.name === s.name
                                   ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100'
                                   : 'border-gray-50 bg-gray-50/50 hover:border-blue-200'
                                }`}
                             >
                                <div className="flex items-center">
                                   <div className={`w-10 h-10 rounded-xl flex items-center justify-center mr-4 transition-colors ${selectedSize?.name === s.name ? 'bg-blue-600 text-white' : 'bg-white text-gray-400 shadow-sm'}`}>
                                      <span className="font-black text-sm uppercase">{s.name.charAt(0)}</span>
                                   </div>
                                   <div className="text-left">
                                      <p className={`text-sm font-black uppercase tracking-tight ${selectedSize?.name === s.name ? 'text-blue-700' : 'text-gray-700'}`}>{s.name}</p>
                                      <p className="text-[10px] text-gray-400 font-bold uppercase">Kích thước chuẩn</p>
                                   </div>
                                </div>
                                <div className="text-right">
                                   <p className={`text-sm font-black ${selectedSize?.name === s.name ? 'text-blue-700' : 'text-gray-400'}`}>+{s.price.toLocaleString()}đ</p>
                                </div>
                             </button>
                          ))}
                       </div>
                    </div>
                 )}

                 {/* Topping Selection */}
                 {currentCustomizingProduct.toppingsJson && JSON.parse(currentCustomizingProduct.toppingsJson).length > 0 && (
                    <div className="space-y-4 pb-4">
                       <div className="flex justify-between items-end">
                          <label className="text-[11px] font-black text-gray-400 uppercase tracking-[0.2em]">Thêm Topping</label>
                          <span className="text-[9px] bg-gray-100 text-gray-400 px-2 py-0.5 rounded font-black uppercase">Tùy chọn</span>
                       </div>
                       <div className="grid grid-cols-2 gap-3">
                          {JSON.parse(currentCustomizingProduct.toppingsJson).map((t: any, i: number) => {
                             const isSelected = selectedToppings.find(x => x.name === t.name);
                             return (
                                <button
                                   key={i}
                                   onClick={() => {
                                      if (isSelected) {
                                         setSelectedToppings(selectedToppings.filter(x => x.name !== t.name));
                                      } else {
                                         setSelectedToppings([...selectedToppings, t]);
                                      }
                                   }}
                                   className={`p-4 rounded-3xl border-2 transition-all flex flex-col items-center text-center ${
                                      isSelected
                                      ? 'border-orange-500 bg-orange-50 ring-4 ring-orange-100'
                                      : 'border-gray-50 bg-gray-50/50 hover:border-orange-200'
                                   }`}
                                >
                                   <p className={`text-[11px] font-black uppercase tracking-tight mb-1 ${isSelected ? 'text-orange-700' : 'text-gray-700'}`}>{t.name}</p>
                                   <p className={`text-[10px] font-bold ${isSelected ? 'text-orange-600' : 'text-gray-400'}`}>+{t.price.toLocaleString()}đ</p>
                                   {isSelected && <div className="mt-2 bg-orange-500 rounded-full p-0.5"><CheckCircle2 size={12} className="text-white" /></div>}
                                </button>
                             );
                          })}
                       </div>
                    </div>
                 )}
              </div>

              {/* Sticky Footer */}
              <div className="p-8 pt-6 border-t border-gray-100 bg-white/80 backdrop-blur-md rounded-t-[2.5rem] shadow-[0_-10px_40px_-15px_rgba(0,0,0,0.1)]">
                 <div className="flex items-center justify-between mb-6">
                    <div className="flex items-center bg-gray-100 rounded-2xl p-1.5 border border-gray-200">
                       <button
                        onClick={() => setCustomizingQuantity(Math.max(1, customizingQuantity - 1))}
                        className="w-10 h-10 flex items-center justify-center text-gray-400 hover:text-blue-600"
                       >
                          <Minus size={18}/>
                       </button>
                       <span className="px-5 font-black text-lg text-gray-800">{customizingQuantity.toString().padStart(2, '0')}</span>
                       <button
                        onClick={() => setCustomizingQuantity(customizingQuantity + 1)}
                        className="w-10 h-10 flex items-center justify-center text-blue-600"
                       >
                          <Plus size={18}/>
                       </button>
                    </div>
                    <div className="text-right">
                       <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Thành tiền</p>
                       <p className="text-3xl font-black text-blue-700 tracking-tighter">
                          {(currentCustomizingProduct.price + (selectedSize?.price || 0) + selectedToppings.reduce((s, t) => s + t.price, 0)).toLocaleString()}đ
                       </p>
                    </div>
                 </div>
                 <button
                    onClick={handleConfirmOptions}
                    className="w-full bg-blue-600 text-white py-5 rounded-[2rem] font-black text-sm uppercase tracking-[0.2em] shadow-2xl shadow-blue-500/40 hover:bg-blue-700 active:scale-95 transition-all flex items-center justify-center"
                 >
                    XÁC NHẬN THÊM VÀO GIỎ <ChevronRight size={20} className="ml-2" />
                 </button>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default DigitalMenu;
