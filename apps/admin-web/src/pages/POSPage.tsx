import React, { useState, useEffect } from 'react';
import { Search, Grid, List, Utensils, ClipboardList, UserPlus, MoreVertical, Plus, Minus, Printer, History, Bell, RotateCcw, X, UtensilsCrossed, LayoutGrid, Loader2, QrCode, Banknote, CheckCircle2, MapPin, ChevronDown, LogIn } from 'lucide-react';

interface Product {
  id: string;
  name: string;
  price: number;
  category: string;
  group?: string;
  imageUrl?: string;
}

interface Table {
  id: string;
  name: string;
  status: string;
  areaName: string;
  isActive: boolean;
}

interface Branch {
  id: string;
  name: string;
  address?: string;
  bankName?: string;
  accountNumber?: string;
  accountHolder?: string;
  isMain?: boolean;
}

interface CartItem extends Product {
  quantity: number;
  note: string;
}

const POSPage = ({ setActiveTab: setAppTab, userName, userRole }: { setActiveTab: (tab: string) => void, userName?: string, userRole?: 'admin' | 'cashier' | null }) => {
  const [posTab, setPosTab] = useState('menu');
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [products, setProducts] = useState<Product[]>([]);
  const [tables, setTables] = useState<Table[]>([]);
  const [categories, setCategories] = useState<string[]>(['Tất cả']);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);
  const [paymentMethod, setPaymentMethod] = useState<'Tiền mặt' | 'Chuyển khoản'>('Tiền mặt');
  const [branchInfo, setBranchInfo] = useState<Branch | null>(null);
  const [allBranches, setAllBranches] = useState<Branch[]>([]);

  // State quản lý giỏ hàng riêng cho từng bàn
  // Key là tableId, value là mảng CartItem
  const [selectedTableId, setSelectedTableId] = useState<string>('delivery'); // 'delivery' cho Mang về
  const [tableCarts, setTableCarts] = useState<Record<string, CartItem[]>>(() => {
    const saved = localStorage.getItem('pos_table_carts');
    if (saved) {
      try {
        return JSON.parse(saved);
      } catch (e) {
        return { 'delivery': [] };
      }
    }
    return { 'delivery': [] };
  });

  // Lưu giỏ hàng vào localStorage mỗi khi có thay đổi để không bị mất khi reload/thoát
  useEffect(() => {
    localStorage.setItem('pos_table_carts', JSON.stringify(tableCarts));
  }, [tableCarts]);

  const fetchProducts = async () => {
    try {
      const response = await fetch('http://localhost:5000/api/Product');
      const data: Product[] = await response.json();
      setProducts(data);

      const uniqueCats = Array.from(new Set(data.map(p => p.category).filter(c => !!c)));
      setCategories(['Tất cả', ...uniqueCats]);
    } catch (err) {
      console.error('Error fetching products:', err);
    }
  };

  const fetchTables = async () => {
    try {
      if (!branchInfo?.id) return;
      const response = await fetch(`http://localhost:5000/api/Table?isActive=true&branchId=${branchInfo.id}`);
      const data: Table[] = await response.json();
      setTables(data);
    } catch (err) {
      console.error('Error fetching tables:', err);
    }
  };

  const fetchBranchInfo = async () => {
    try {
      const response = await fetch('http://localhost:5000/api/Branch');
      const data: Branch[] = await response.json();
      setAllBranches(data);

      // Ưu tiên chi nhánh đã chọn trước đó, nếu không thì lấy chi nhánh chính
      const savedBranchId = localStorage.getItem('selectedBranchId');
      const selected = data.find(b => b.id === savedBranchId) || data.find(b => b.isMain) || data[0];

      setBranchInfo(selected);
      if (selected) {
        localStorage.setItem('selectedBranchId', selected.id);
      }
    } catch (err) {
      console.error('Error fetching branch info:', err);
    }
  };

  const handleSwitchBranch = (branch: Branch) => {
    if (currentCart.length > 0) {
      if (!window.confirm('Giỏ hàng hiện tại sẽ bị xóa khi đổi chi nhánh. Bạn có chắc chắn?')) {
        return;
      }
    }
    setBranchInfo(branch);
    localStorage.setItem('selectedBranchId', branch.id);
    setTableCarts({ 'delivery': [] }); // Reset carts
    // Re-fetch data for the new branch if needed
  };

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      await Promise.all([fetchProducts(), fetchBranchInfo()]);
      setLoading(false);
    };
    loadData();
  }, []);

  useEffect(() => {
    if (branchInfo?.id) {
      fetchTables();
    }
  }, [branchInfo?.id]);

  // Lấy giỏ hàng hiện tại dựa trên bàn đang chọn
  const currentCart = tableCarts[selectedTableId] || [];
  const totalAmount = currentCart.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  const selectedTable = tables.find(t => t.id === selectedTableId) || null;

  const updateTableStatus = async (tableId: string, status: string) => {
    if (tableId === 'delivery') return;
    try {
      await fetch(`http://localhost:5000/api/Table/${tableId}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(status)
      });
      fetchTables(); // Cập nhật lại danh sách bàn để hiện đúng màu sắc
    } catch (err) {
      console.error('Error updating table status:', err);
    }
  };

  const addToCart = (product: Product) => {
    const cart = tableCarts[selectedTableId] || [];
    const existing = cart.find(item => item.id === product.id);

    let newCart;
    if (existing) {
      newCart = cart.map(item => item.id === product.id ? { ...item, quantity: item.quantity + 1 } : item);
    } else {
      newCart = [...cart, { ...product, quantity: 1, note: '' }];
    }

    setTableCarts({ ...tableCarts, [selectedTableId]: newCart });

    // Nếu là bàn (không phải mang về) và đây là món đầu tiên, đổi trạng thái bàn
    if (selectedTableId !== 'delivery' && cart.length === 0) {
      updateTableStatus(selectedTableId, 'Có khách');
    }
  };

  const updateQuantity = (productId: string, delta: number) => {
    const cart = tableCarts[selectedTableId] || [];
    const newCart = cart.map(item => {
      if (item.id === productId) {
        const newQty = item.quantity + delta;
        return newQty > 0 ? { ...item, quantity: newQty } : item;
      }
      return item;
    }).filter(item => item.quantity > 0);

    setTableCarts({ ...tableCarts, [selectedTableId]: newCart });

    // Nếu giỏ hàng trống sau khi giảm, trả bàn về trạng thái Trống
    if (selectedTableId !== 'delivery' && newCart.length === 0) {
      updateTableStatus(selectedTableId, 'Trống');
    }
  };

  const removeFromCart = (productId: string) => {
    const cart = tableCarts[selectedTableId] || [];
    const newCart = cart.filter(item => item.id !== productId);
    setTableCarts({ ...tableCarts, [selectedTableId]: newCart });

    if (selectedTableId !== 'delivery' && newCart.length === 0) {
      updateTableStatus(selectedTableId, 'Trống');
    }
  };

  const handleSelectTable = (tableId: string) => {
    setSelectedTableId(tableId);
    setPosTab('menu');
  };

  const handlePayment = async () => {
    if (currentCart.length === 0) {
      alert('Vui lòng chọn món trước khi thanh toán!');
      return;
    }
    setIsPaymentModalOpen(true);
  };

  const confirmPayment = async () => {
    try {
      const order = {
        tableName: selectedTable ? selectedTable.name : 'Mang về',
        totalAmount: totalAmount,
        paidAmount: totalAmount,
        paymentMethod: paymentMethod,
        status: 'Hoàn thành',
        details: currentCart.map(item => ({
          productId: item.id,
          productName: item.name,
          quantity: item.quantity,
          unitPrice: item.price
        }))
      };

      const response = await fetch('http://localhost:5000/api/Order', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(order)
      });

      if (response.ok) {
        alert('Thanh toán thành công!');
        setIsPaymentModalOpen(false);

        // Xóa giỏ hàng của bàn này
        const newTableCarts = { ...tableCarts };
        delete newTableCarts[selectedTableId];
        setTableCarts(newTableCarts);

        // Trả bàn về trạng thái Trống
        if (selectedTableId !== 'delivery') {
          await updateTableStatus(selectedTableId, 'Trống');
        }

        setSelectedTableId('delivery'); // Reset về Mang về
      } else {
        const error = await response.json();
        alert(`Lỗi: ${error.message || 'Không thể lưu hóa đơn'}`);
      }
    } catch (err) {
      alert('Lỗi kết nối đến server.');
    }
  };

  const filteredProducts = products.filter(p => {
    const matchesCategory = activeCategory === 'Tất cả' || p.category === activeCategory;
    const matchesSearch = p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         (p.id && p.id.toLowerCase().includes(searchTerm.toLowerCase()));
    return matchesCategory && matchesSearch;
  });

  return (
    <div className="flex h-screen bg-[#f0f2f5] overflow-hidden text-gray-800 font-sans">
      {/* LEFT SIDE: Tables / Menu Selection */}
      <div className="w-[60%] flex flex-col border-r border-gray-300 bg-white">
        {/* Header Left (Blue) */}
        <div className="bg-[#0070f4] p-1 flex items-center space-x-1">
          {/* Branch Selector */}
          <div className={`relative mr-1 px-2 border-r border-blue-400 ${userRole === 'admin' ? 'group' : ''}`}>
             <div className={`flex items-center text-white p-1.5 rounded transition-all ${userRole === 'admin' ? 'cursor-pointer hover:bg-blue-600' : 'cursor-default'}`}>
                <MapPin size={16} className="mr-1.5 text-blue-200" />
                <div className="text-left">
                   <p className="text-[9px] font-bold text-blue-200 uppercase leading-none mb-0.5">Chi nhánh</p>
                   <p className="text-xs font-black truncate max-w-[120px]">{branchInfo?.name || 'Đang tải...'}</p>
                </div>
                {userRole === 'admin' && <ChevronDown size={14} className="ml-2 text-blue-200 opacity-60" />}
             </div>

             {/* Dropdown Menu - Only for Admin */}
             {userRole === 'admin' && (
                <div className="absolute top-full left-0 mt-1 w-64 bg-white rounded-lg shadow-2xl border border-gray-100 hidden group-hover:block z-[110] overflow-hidden animate-in fade-in slide-in-from-top-2 duration-200">
                    <div className="p-3 border-b bg-gray-50 flex items-center justify-between">
                    <p className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Hệ thống chi nhánh</p>
                    <span className="bg-blue-100 text-blue-600 text-[9px] px-1.5 py-0.5 rounded font-bold">{allBranches.length} cơ sở</span>
                    </div>
                    <div className="max-h-80 overflow-y-auto py-1">
                    {allBranches.map(b => (
                        <div
                            key={b.id}
                            onClick={() => handleSwitchBranch(b)}
                            className={`px-4 py-3 hover:bg-blue-50 cursor-pointer flex items-center justify-between border-b border-gray-50 last:border-0 transition-colors ${branchInfo?.id === b.id ? 'bg-blue-50/80' : ''}`}
                        >
                            <div className="flex-1 min-w-0 pr-4">
                                <div className="flex items-center">
                                <p className={`text-sm font-bold truncate ${branchInfo?.id === b.id ? 'text-blue-600' : 'text-gray-700'}`}>{b.name}</p>
                                {b.isMain && <span className="ml-2 bg-orange-100 text-orange-600 text-[8px] px-1 rounded-sm font-bold uppercase">Trụ sở</span>}
                                </div>
                                <p className="text-[10px] text-gray-400 truncate mt-0.5 flex items-center"><MapPin size={10} className="mr-1"/> {b.address || 'Chưa cập nhật địa chỉ'}</p>
                            </div>
                            {branchInfo?.id === b.id && <div className="bg-blue-600 rounded-full p-1"><CheckCircle2 size={12} className="text-white" /></div>}
                        </div>
                    ))}
                    </div>
                    <div className="p-2 bg-gray-50 text-center border-t">
                    <button onClick={() => setAppTab('branches')} className="text-[10px] text-blue-600 font-bold hover:underline">Quản lý danh sách chi nhánh</button>
                    </div>
                </div>
             )}
          </div>

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
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
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
          {loading ? (
            <div className="flex items-center justify-center h-full">
               <Loader2 size={32} className="animate-spin text-blue-600" />
            </div>
          ) : posTab === 'tables' ? (
            <div className="grid grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-4">
               <div
                 onClick={() => handleSelectTable('delivery')}
                 className={`rounded-lg shadow-md aspect-square flex flex-col items-center justify-center cursor-pointer hover:scale-105 transition-all border-2 ${selectedTableId === 'delivery' ? 'bg-[#0070f4] text-white border-blue-400' : 'bg-white text-gray-600 border-gray-100'}`}
               >
                  <div className={`${selectedTableId === 'delivery' ? 'bg-white/20' : 'bg-blue-50'} p-2 rounded-lg mb-2`}>
                    <Utensils size={24} className={selectedTableId === 'delivery' ? 'text-white' : 'text-blue-600'} />
                  </div>
                  <span className="text-xs font-bold uppercase text-center px-1">Mang về</span>
               </div>
               {tables.map(table => {
                 const hasItems = (tableCarts[table.id] || []).length > 0;
                 return (
                   <div
                     key={table.id}
                     onClick={() => handleSelectTable(table.id)}
                     className={`rounded-lg shadow-sm aspect-square flex flex-col items-center justify-center cursor-pointer transition-all border-2 ${
                       selectedTableId === table.id
                         ? 'bg-blue-50 border-blue-400 text-blue-700 ring-2 ring-blue-100'
                         : table.status !== 'Trống' || hasItems
                           ? 'bg-orange-50 border-orange-200 text-orange-700'
                           : 'bg-white border-transparent text-gray-600 hover:border-gray-300'
                     }`}
                   >
                      <span className="text-sm font-bold">{table.name}</span>
                      <span className={`text-[10px] ${table.status === 'Trống' && !hasItems ? 'text-green-500' : 'text-orange-500'}`}>
                        {hasItems ? 'Đang chọn' : table.status}
                      </span>
                   </div>
                 );
               })}
            </div>
          ) : (
            <div className="grid grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
              {filteredProducts.map(product => (
                <div
                  key={product.id}
                  onClick={() => addToCart(product)}
                  className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden cursor-pointer hover:shadow-md hover:border-blue-400 transition-all group"
                >
                  <div className="aspect-square bg-gray-50 flex items-center justify-center relative">
                    {product.imageUrl ? (
                      <img src={product.imageUrl} alt={product.name} className="w-full h-full object-cover" />
                    ) : (
                      <UtensilsCrossed size={32} className="text-gray-200 group-hover:text-blue-100 transition-colors" />
                    )}
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
           <div className="text-[10px] text-gray-400 italic font-medium">Bàn đang chọn: <span className="text-blue-600 font-bold">{selectedTable ? selectedTable.name : 'Mang về'}</span></div>
        </div>
      </div>

      {/* RIGHT SIDE: Order Details */}
      <div className="flex-1 flex flex-col bg-white shadow-2xl">
        {/* Order Tabs */}
        <div className="bg-[#1e293b] flex items-center p-1 space-x-1">
          <div className="bg-white text-gray-800 px-4 py-1.5 rounded-t text-xs font-bold flex items-center shadow-sm">
            {selectedTable ? selectedTable.name : 'Mang về'} <X size={12} className="ml-2 text-gray-400 hover:text-red-500 cursor-pointer" />
          </div>
          <div className="flex-1"></div>
          <div className="flex items-center space-x-3 pr-2">
             <button
               onClick={() => setAppTab('dashboard')}
               className="bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded text-[11px] font-bold flex items-center transition-all mr-2 shadow-lg border border-red-400"
               title="Đăng xuất khỏi phiên bán hàng"
             >
               <LogIn size={14} className="mr-1" /> Đăng xuất
             </button>
             <Bell size={18} className="text-white opacity-60 hover:opacity-100 cursor-pointer" />
             <Printer size={18} className="text-white opacity-60 hover:opacity-100 cursor-pointer" />
             <History size={18} className="text-white opacity-60 hover:opacity-100 cursor-pointer" />
          </div>
        </div>

        {/* Order Info Bar */}
        <div className="p-2 flex items-center border-b space-x-2">
          <div className="bg-blue-100 text-blue-700 px-3 py-1 rounded text-xs font-bold flex items-center whitespace-nowrap">
             <Utensils size={14} className="mr-1" /> {selectedTable ? selectedTable.name : 'Mang về'}
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
          <button onClick={() => setTableCarts({ ...tableCarts, [selectedTableId]: [] })} className="p-1.5 text-gray-400 hover:bg-gray-100 rounded"><RotateCcw size={18}/></button>
        </div>

        {/* Selected Items List */}
        <div className="flex-1 overflow-auto px-4">
           {currentCart.length > 0 ? currentCart.map((item, idx) => (
             <div key={item.id} className="flex items-start py-3 border-b border-gray-100 group transition-all">
                <div className="flex-1">
                   <div className="flex items-center">
                      <button onClick={() => removeFromCart(item.id)} className="mr-2 text-red-300 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity">
                         <X size={14} />
                      </button>
                      <p className="text-sm font-bold text-gray-800 capitalize">{idx + 1}. {item.name}</p>
                   </div>
                   <input
                     type="text"
                     className="text-[10px] text-orange-500 mt-1 bg-transparent border-b border-transparent hover:border-orange-200 outline-none w-full"
                     placeholder="Ghi chú món..."
                     value={item.note}
                     onChange={(e) => {
                        const newNote = e.target.value;
                        const newCart = currentCart.map(c => c.id === item.id ? { ...c, note: newNote } : c);
                        setTableCarts({ ...tableCarts, [selectedTableId]: newCart });
                     }}
                   />
                </div>
                <div className="flex items-center space-x-4">
                   <div className="flex items-center border rounded-md overflow-hidden bg-gray-50">
                      <button onClick={() => updateQuantity(item.id, -1)} className="px-1.5 py-1 hover:bg-gray-200 text-gray-400 transition-colors"><Minus size={12} /></button>
                      <input type="text" className="w-8 bg-transparent text-center text-xs font-bold outline-none" value={item.quantity} readOnly />
                      <button onClick={() => updateQuantity(item.id, 1)} className="px-1.5 py-1 hover:bg-gray-200 text-blue-600 transition-colors"><Plus size={12} /></button>
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
                 <span className="text-xs font-bold text-gray-500 mr-2">{userName || 'Nhân viên'}</span>
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
              <button
                onClick={handlePayment}
                className="col-span-8 bg-[#0070f4] text-white py-3 rounded-lg font-bold flex items-center justify-center space-x-3 hover:bg-blue-700 transition-all shadow-lg active:scale-95 border-b-4 border-blue-800"
              >
                 <span className="bg-white/20 p-2 rounded-full"><RotateCcw size={20} className="rotate-90"/></span>
                 <span className="text-xl uppercase tracking-widest font-black italic">Thanh toán (F9)</span>
              </button>
           </div>
        </div>
      </div>

      {/* PAYMENT MODAL */}
      {isPaymentModalOpen && (
        <div className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-center p-4 backdrop-blur-sm">
           <div className="bg-white w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col md:flex-row animate-in zoom-in-95 duration-200">
              {/* Left Side: Summary & Methods */}
              <div className="flex-1 p-6 border-r">
                 <div className="flex justify-between items-center mb-6">
                    <h3 className="font-black text-xl text-gray-800 uppercase italic tracking-tighter">Thanh toán hóa đơn</h3>
                    <button onClick={() => setIsPaymentModalOpen(false)} className="text-gray-400 hover:text-gray-600"><X size={24}/></button>
                 </div>

                 <div className="bg-blue-50 p-4 rounded-xl mb-6">
                    <div className="flex justify-between items-center mb-1">
                       <span className="text-gray-500 text-xs font-bold uppercase">Tổng tiền thanh toán</span>
                       <span className="bg-blue-600 text-white text-[10px] px-2 py-0.5 rounded-full font-bold">VNĐ</span>
                    </div>
                    <div className="text-3xl font-black text-blue-700">{totalAmount.toLocaleString()}</div>
                 </div>

                 <div className="space-y-3">
                    <p className="text-[11px] font-bold text-gray-400 uppercase mb-2">Phương thức thanh toán</p>
                    <button
                      onClick={() => setPaymentMethod('Tiền mặt')}
                      className={`w-full p-4 rounded-xl border-2 flex items-center justify-between transition-all ${paymentMethod === 'Tiền mặt' ? 'border-blue-600 bg-blue-50 shadow-md' : 'border-gray-100 hover:border-blue-200'}`}
                    >
                       <div className="flex items-center">
                          <div className={`p-2 rounded-lg mr-3 ${paymentMethod === 'Tiền mặt' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-400'}`}>
                             <Banknote size={24} />
                          </div>
                          <div className="text-left">
                             <p className="font-bold text-gray-800">Tiền mặt</p>
                             <p className="text-[10px] text-gray-400">Thanh toán trực tiếp tại quầy</p>
                          </div>
                       </div>
                       {paymentMethod === 'Tiền mặt' && <CheckCircle2 size={20} className="text-blue-600" />}
                    </button>

                    <button
                      onClick={() => setPaymentMethod('Chuyển khoản')}
                      className={`w-full p-4 rounded-xl border-2 flex items-center justify-between transition-all ${paymentMethod === 'Chuyển khoản' ? 'border-blue-600 bg-blue-50 shadow-md' : 'border-gray-100 hover:border-blue-200'}`}
                    >
                       <div className="flex items-center">
                          <div className={`p-2 rounded-lg mr-3 ${paymentMethod === 'Chuyển khoản' ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-400'}`}>
                             <QrCode size={24} />
                          </div>
                          <div className="text-left">
                             <p className="font-bold text-gray-800">Chuyển khoản / QR</p>
                             <p className="text-[10px] text-gray-400">Quét mã VietQR nhanh chóng</p>
                          </div>
                       </div>
                       {paymentMethod === 'Chuyển khoản' && <CheckCircle2 size={20} className="text-blue-600" />}
                    </button>
                 </div>

                 <button
                   onClick={confirmPayment}
                   className="w-full mt-8 bg-[#0070f4] text-white py-4 rounded-xl font-bold text-lg hover:bg-blue-700 transition-all shadow-lg active:scale-95 flex items-center justify-center"
                 >
                   Xác nhận thanh toán
                 </button>
              </div>

              {/* Right Side: QR Display */}
              <div className={`w-full md:w-72 bg-gray-50 p-6 flex flex-col items-center justify-center transition-all ${paymentMethod === 'Chuyển khoản' ? 'opacity-100' : 'opacity-30 grayscale pointer-events-none'}`}>
                 <p className="text-xs font-bold text-gray-400 uppercase mb-4">Mã QR Thanh toán</p>
                 <div className="bg-white p-3 rounded-2xl shadow-inner border-2 border-dashed border-gray-200 mb-4 relative">
                    {branchInfo?.bankName && branchInfo?.accountNumber ? (
                      <img
                        src={`https://img.vietqr.io/image/${branchInfo.bankName.replace(/\s/g, '')}-${branchInfo.accountNumber}-compact2.jpg?amount=${totalAmount}&addInfo=Thanh toan don hang&accountName=${encodeURIComponent(branchInfo.accountHolder || '')}`}
                        alt="VietQR"
                        className="w-48 h-48 object-contain"
                      />
                    ) : (
                      <div className="w-48 h-48 flex flex-col items-center justify-center text-gray-300 text-center p-4">
                         <QrCode size={48} className="mb-2 opacity-20" />
                         <p className="text-[10px]">Chưa thiết lập thông tin ngân hàng trong Quản lý chi nhánh</p>
                      </div>
                    )}
                 </div>
                 <div className="text-center">
                    <p className="font-bold text-sm text-gray-700">{branchInfo?.accountHolder || 'CHƯA CẬP NHẬT'}</p>
                    <p className="text-blue-600 font-black text-base">{branchInfo?.accountNumber || '---'}</p>
                    <p className="text-[10px] text-gray-400 font-bold uppercase">{branchInfo?.bankName || 'Ngân hàng'}</p>
                 </div>
                 <p className="mt-6 text-[10px] text-gray-400 text-center italic">
                    Khách hàng quét mã bằng ứng dụng Ngân hàng hoặc Ví điện tử để thanh toán.
                 </p>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default POSPage;
