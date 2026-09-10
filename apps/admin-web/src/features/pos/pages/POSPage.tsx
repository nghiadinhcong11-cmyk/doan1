import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import { useRef } from 'react';
import {
  Search, Grid, List, Utensils, ClipboardList, UserPlus, MoreVertical,
  Plus, Minus, Printer, History, Bell, RotateCcw, X, UtensilsCrossed,
  LayoutGrid, Loader2, QrCode, Banknote, CheckCircle2, MapPin,
  ChevronDown, LogIn, Trash2, ChevronRight, Clock, Store, Calendar as CalendarIcon,
  Star
} from 'lucide-react';
import { API_URL } from '../../../config';

interface Product {
  id: string;
  name: string;
  price: number;
  category: string;
  group?: string;
  imageUrl?: string;
  sizesJson?: string;
  toppingsJson?: string;
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
  imageUrl?: string;
}

interface CartItem extends Product {
  quantity: number;
  sentQuantity: number;
  note: string;
  selectedSize?: any;
  selectedToppings?: any[];
  totalItemPrice: number;
  kitchenStatus?: string;
  optionsText?: string;
}

const kitchenStatusLabel = (status?: string) => {
  switch (status) {
    case 'Pending': return 'Bếp đã nhận';
    case 'Preparing': return 'Đang chế biến';
    case 'Ready': return 'Đã ra món';
    case 'Completed': return 'Đã hoàn tất';
    case 'Cancelled': return 'Đã hủy';
    default: return '';
  }
};

const kitchenStatusClass = (status?: string) => {
  switch (status) {
    case 'Ready':
    case 'Completed': return 'bg-green-100 text-green-700';
    case 'Preparing': return 'bg-orange-100 text-orange-700';
    case 'Cancelled': return 'bg-red-100 text-red-700';
    default: return 'bg-slate-100 text-slate-600';
  }
};

const POSPage = ({ onLogout, userName, userRole, userPosition }: { onLogout: () => void, userName?: string, userRole?: 'admin' | 'manager' | 'cashier' | null, userPosition?: string }) => {
  const [posTab, setPosTab] = useState('menu');
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [products, setProducts] = useState<Product[]>([]);
  const [tables, setTables] = useState<Table[]>([]);
  const [categories, setCategories] = useState<string[]>(['Tất cả']);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);
  const [kitchenMessage, setKitchenMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [paymentMethod, setPaymentMethod] = useState<'Tiền mặt' | 'Chuyển khoản'>('Tiền mặt');
  const [customerPhone, setCustomerPhone] = useState('');
  const [customerName, setCustomerName] = useState('Khách lẻ');
  const [customerEmail, setCustomerEmail] = useState('');
  const [isCheckingPhone, setIsCheckingPhone] = useState(false);
  const [branchInfo, setBranchInfo] = useState<Branch | null>(null);
  const [allBranches, setAllBranches] = useState<Branch[]>([]);
  const [pendingOrders, setPendingOrders] = useState<any[]>([]);
  const [pendingReservations, setPendingReservations] = useState<any[]>([]);
  const [lastOrderCount, setLastOrderCount] = useState(0);
  const [lastResCount, setLastResCount] = useState(0);
  const [showNotification, setShowNotification] = useState(false);
  const [notifType, setNotifType] = useState<'order' | 'res'>('order');
  const [orderToAccept, setOrderToAccept] = useState<any>(null);

  // Quản lý ca làm việc (Shift)
  const [activeShift, setActiveShift] = useState<any>(null);
  const [isShiftModalOpen, setIsShiftModalOpen] = useState(false);
  const [isCloseShiftModalOpen, setIsCloseShiftModalOpen] = useState(false);
  const [isShiftHistoryModalOpen, setIsShiftHistoryModalOpen] = useState(false);
  const [startingCash, setStartingCash] = useState<number>(0);
  const [endingCash, setEndingCash] = useState<number>(0);
  const [shiftNote, setShiftNote] = useState('');
  const [isCheckingShift, setIsCheckingShift] = useState(true);
  const [shiftHistory, setShiftHistory] = useState<any[]>([]);
  const [loadingHistory, setLoadingHistory] = useState(false);

  // Lịch sử hóa đơn
  const [isInvoiceHistoryModalOpen, setIsInvoiceHistoryModalOpen] = useState(false);
  const [invoices, setInvoices] = useState<any[]>([]);
  const [loadingInvoices, setLoadingInvoices] = useState(false);
  const [invoiceSearch, setInvoiceSearch] = useState('');
  const [invoiceFromDate, setInvoiceFromDate] = useState(new Date().toISOString().split('T')[0]);
  const [invoiceToDate, setInvoiceToDate] = useState(new Date().toISOString().split('T')[0]);
  const [invoiceStatusFilter, setInvoiceStatusFilter] = useState('all');
  const [selectedHistoryOrder, setSelectedHistoryOrder] = useState<any>(null);
  const [orderLoyaltyInfo, setOrderLoyaltyInfo] = useState<{ pointsEarned: number, pointsRedeemed: number, balanceAfter: number } | null>(null);

  useEffect(() => {
    const fetchLoyalty = async () => {
       if (!selectedHistoryOrder?.id) {
          setOrderLoyaltyInfo(null);
          return;
       }
       try {
          const response = await fetch(`${API_URL}/api/Order/${selectedHistoryOrder.id}/loyalty`, {
             headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }
          });
          if (response.ok) {
             const data = await response.json();
             setOrderLoyaltyInfo(data);
          } else {
             setOrderLoyaltyInfo(null);
          }
       } catch (err) {
          setOrderLoyaltyInfo(null);
       }
    };
    fetchLoyalty();
  }, [selectedHistoryOrder]);

  // States cho tùy chọn món (Size & Topping)
  const [isOptionsModalOpen, setIsOptionsModalOpen] = useState(false);
  const [currentCustomizingProduct, setCurrentCustomizingProduct] = useState<Product | null>(null);
  const [selectedSize, setSelectedSize] = useState<any>(null);
  const [selectedToppings, setSelectedToppings] = useState<any[]>([]);
  const customerPhoneInputRef = React.useRef<HTMLInputElement>(null);

  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  // Xử lý chọn bàn/đơn từ thông báo Navbar
  useEffect(() => {
    const handleAction = (orderId: string) => {
        if (!orderId) return;
        setPosTab('notifications');
        const targetOrder = pendingOrders.find(o => o.id === orderId);
        if (targetOrder) {
            setOrderToAccept(targetOrder);
        }
    };

    // Kiểm tra URL params khi load trang
    const reviewOrderId = searchParams.get('reviewOrderId');
    if (reviewOrderId && pendingOrders.length > 0) {
        handleAction(reviewOrderId);
        // Xóa param sau khi xử lý
        const newParams = new URLSearchParams(searchParams);
        newParams.delete('reviewOrderId');
        setSearchParams(newParams, { replace: true });
    }

    // Lắng nghe sự kiện click trực tiếp (khi đã ở sẵn trang POS)
    const handleEvent = (e: any) => {
        handleAction(e.detail.orderId);
    };

    window.addEventListener('pos-open-notification', handleEvent);
    return () => window.removeEventListener('pos-open-notification', handleEvent);
  }, [searchParams, pendingOrders]);

  // ... (giữ nguyên các hàm cũ)

  const fetchShiftHistory = async () => {
    const empId = localStorage.getItem('employeeId');
    if (!empId) return;
    try {
      setLoadingHistory(true);
      const response = await fetch(`${API_URL}/api/Shift?employeeId=${empId}`);
      const data = await response.json();
      setShiftHistory(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingHistory(false);
    }
  };

  useEffect(() => {
    if (isShiftHistoryModalOpen) fetchShiftHistory();
  }, [isShiftHistoryModalOpen]);

  const fetchInvoiceHistory = async () => {
    try {
      setLoadingInvoices(true);
      const branchId = localStorage.getItem('selectedBranchId');

      let statusParam = '';
      if (invoiceStatusFilter === 'Đang xử lý') {
        statusParam = '&status=Đang xử lý,Đổi quà';
      } else if (invoiceStatusFilter !== 'all') {
        statusParam = `&status=${invoiceStatusFilter}`;
      }

      // Nếu lọc "Đang xử lý", chúng ta có thể bỏ qua lọc ngày để thấy các đơn cũ chưa xong
      const dateParams = invoiceStatusFilter === 'Đang xử lý'
        ? ''
        : `&fromDate=${invoiceFromDate}&toDate=${invoiceToDate}`;

      const response = await fetch(`${API_URL}/api/Order?search=${encodeURIComponent(invoiceSearch)}&branchId=${branchId || ''}${dateParams}${statusParam}&_t=${Date.now()}`, { cache: 'no-store' });
      const data = await response.json();
      setInvoices(data);
    } catch (err) {
      console.error('Lỗi lấy lịch sử hóa đơn:', err);
    } finally {
      setLoadingInvoices(false);
    }
  };

  useEffect(() => {
    if (isInvoiceHistoryModalOpen) fetchInvoiceHistory();
  }, [isInvoiceHistoryModalOpen, invoiceSearch, invoiceFromDate, invoiceToDate, invoiceStatusFilter]);

  // State quản lý giỏ hàng riêng cho từng bàn
  const [selectedTableId, setSelectedTableId] = useState<string>(() => {
    return localStorage.getItem('pos_selected_table_id') || 'delivery';
  });

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

  // Giữ ID đơn thật của từng bàn/mang về để không tạo đơn trùng khi tự lưu hoặc thanh toán.
  const [activeOrderIds, setActiveOrderIds] = useState<Record<string, string>>(() => {
    const saved = localStorage.getItem('pos_active_order_ids');
    try { return saved ? JSON.parse(saved) : {}; } catch { return {}; }
  });
  const [authoritativeTotals, setAuthoritativeTotals] = useState<Record<string, number>>({});

  const [tableCustomers, setTableCustomers] = useState<Record<string, { id?: string, phone: string, name: string, email?: string, loyaltyPoints?: number }>>(() => {
    const saved = localStorage.getItem('pos_table_customers');
    if (saved) {
      try {
        return JSON.parse(saved);
      } catch (e) {
        return { 'delivery': { phone: '', name: 'Khách lẻ' } };
      }
    }
    return { 'delivery': { phone: '', name: 'Khách lẻ' } };
  });

  const [pointsToRedeem, setPointsToRedeem] = useState<number>(0);

  useEffect(() => {
    localStorage.setItem('pos_table_carts', JSON.stringify(tableCarts));
  }, [tableCarts]);

  useEffect(() => {
    localStorage.setItem('pos_table_customers', JSON.stringify(tableCustomers));
  }, [tableCustomers]);

  useEffect(() => {
    localStorage.setItem('pos_selected_table_id', selectedTableId);
  }, [selectedTableId]);

  useEffect(() => {
    localStorage.setItem('pos_active_order_ids', JSON.stringify(activeOrderIds));
  }, [activeOrderIds]);

  const checkActiveShift = async () => {
    // Chỉ "Thu ngân" mới cần mở/đóng ca (Quản lý và Admin có thể xem menu mà không cần mở ca)
    if (userRole !== 'cashier' || userPosition !== 'Thu ngân') {
      setIsCheckingShift(false);
      return;
    }

    const empId = localStorage.getItem('employeeId');
    if (!empId || empId === 'undefined' || empId === 'null') {
      setIsCheckingShift(false);
      return;
    }

    try {
      setIsCheckingShift(true);
      const response = await fetch(`${API_URL}/api/Shift/current/${empId}`);
      if (response.ok) {
        const data = await response.json();
        setActiveShift(data);
      } else if (response.status === 404) {
        // Nếu trả về 404 nghĩa là chưa mở ca, hiển thị modal mở ca
        setIsShiftModalOpen(true);
      }
    } catch (err) {
      console.warn('Lỗi kiểm tra ca làm việc:', err);
    } finally {
      setIsCheckingShift(false);
    }
  };

  const handleOpenShift = async () => {
    const empId = localStorage.getItem('employeeId');
    const branchId = localStorage.getItem('selectedBranchId');

    if (!empId || !branchId) {
      alert('Thiếu thông tin nhân viên hoặc chi nhánh!');
      return;
    }

    try {
      const response = await fetch(`${API_URL}/api/Shift/open`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          employeeId: empId,
          employeeName: userName,
          branchId: branchId,
          branchName: localStorage.getItem('selectedBranchName'),
          startingCash: startingCash
        })
      });

      if (response.ok) {
        const data = await response.json();
        setActiveShift(data);
        setIsShiftModalOpen(false);
        alert('Mở ca thành công! Bắt đầu phiên bán hàng.');
      }
    } catch (err) {
      alert('Lỗi kết nối khi mở ca.');
    }
  };

  const handleCloseShift = async () => {
    if (!activeShift) return;

    try {
      const response = await fetch(`${API_URL}/api/Shift/close/${activeShift.id}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          endingCash: endingCash,
          note: shiftNote
        })
      });

      if (response.ok) {
        alert('Kết thúc ca thành công! Phiên làm việc đã được đóng.');
        setIsCloseShiftModalOpen(false);
        setActiveShift(null);
        onLogout(); // Đăng xuất sau khi chốt ca
      }
    } catch (err) {
      alert('Lỗi kết nối khi chốt ca.');
    }
  };

  const fetchProducts = async () => {
    try {
      const [prodRes, toppingRes] = await Promise.all([
        fetch(`${API_URL}/api/Product`),
        fetch(`${API_URL}/api/Topping`)
      ]);

      if (!prodRes.ok || !toppingRes.ok) {
        throw new Error('Failed to fetch products or toppings');
      }

      const productsData = await prodRes.json();
      const toppingsData = await toppingRes.json();

      if (!Array.isArray(productsData) || !Array.isArray(toppingsData)) {
        console.error('Data returned from products/toppings API is not an array');
        return;
      }

      const toppingProducts = toppingsData.map((t: any) => ({
        id: t.id,
        name: t.name,
        price: t.price,
        category: 'Topping',
        group: t.category,
        imageUrl: t.imageUrl,
        isStandaloneTopping: true
      }));

      const allItems = [...productsData, ...toppingProducts];
      setProducts(allItems);

      const uniqueCats = Array.from(new Set(allItems.map(p => p.category).filter(c => !!c)));
      setCategories(['Tất cả', ...uniqueCats]);
    } catch (err) {
      console.error('Error fetching products:', err);
    }
  };

  const fetchTables = async () => {
    try {
      if (!branchInfo?.id) return;
      const response = await fetch(`${API_URL}/api/Table?isActive=true&branchId=${branchInfo.id}`);
      const data: Table[] = await response.json();
      const physicalTables = data.filter(table => table.name !== 'Mang về');
      setTables(physicalTables);
      return physicalTables;
    } catch (err) {
      console.error('Error fetching tables:', err);
    }
  };

  const fetchBranchInfo = async () => {
    try {
      const response = await fetch(`${API_URL}/api/Branch`);
      if (!response.ok) {
        throw new Error('Failed to fetch branches');
      }
      const data = await response.json();

      if (Array.isArray(data)) {
        setAllBranches(data);
        const savedBranchId = localStorage.getItem('selectedBranchId');
        const selected = data.find(b => b.id === savedBranchId) || data.find(b => b.isMain) || data[0];

        setBranchInfo(selected);
        if (selected) {
          localStorage.setItem('selectedBranchId', selected.id);
        }
      } else {
        console.error('Data returned from branch API is not an array:', data);
        setAllBranches([]);
      }
    } catch (err) {
      console.error('Error fetching branch info:', err);
      setAllBranches([]);
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
  };

  const fetchPendingOrders = async () => {
    try {
      const branchId = localStorage.getItem('selectedBranchId');

      // Lấy đơn hàng mới & yêu cầu đổi quà
      // LƯU Ý: Chỉ lấy đơn Web (vãng lai/Mang về) chưa được gán vào bàn cụ thể trong POS
      const orderRes = await fetch(`${API_URL}/api/Order?status=Đang xử lý,Đổi quà${branchId ? `&branchId=${branchId}` : ''}`);
      if (!orderRes.ok) throw new Error('Failed to fetch orders');
      const data = await orderRes.json();

      if (Array.isArray(data)) {
        // Lọc: Chỉ lấy những đơn có tên bàn là "Mang về", "vãng lai" hoặc không nằm trong danh sách bàn hiện tại của POS
        // để tránh hiện lại những đơn đã được POS chấp nhận và đang phục vụ.
        const tableNames = tables.map(t => t.name);
        const webOrders = data.filter((o: any) =>
            o.status === 'Đổi quà' ||
            o.tableName === 'Mang về' ||
            o.tableName === 'vãng lai' ||
            !tableNames.includes(o.tableName) ||
            !o.createdBy // Đơn từ Web khách thường không có createdBy là nhân viên
        );

        setPendingOrders(webOrders);

        if (webOrders.length > lastOrderCount && lastOrderCount !== 0) {
          setNotifType('order');
          setShowNotification(true);
          playNotifSound();
        }
        setLastOrderCount(webOrders.length);
      }

      // Lấy lịch hẹn mới
      const resRes = await fetch(`${API_URL}/api/Reservation?status=Pending${branchId ? `&branchId=${branchId}` : ''}`);
      if (!resRes.ok) throw new Error('Failed to fetch reservations');
      const reservations = await resRes.json();

      if (Array.isArray(reservations)) {
        setPendingReservations(reservations);

        if (reservations.length > lastResCount && lastResCount !== 0) {
          setNotifType('res');
          setShowNotification(true);
          playNotifSound();
        }
        setLastResCount(reservations.length);
      }
    } catch (err) {
      console.error("Lỗi lấy dữ liệu thông báo:", err);
    }
  };

  const playNotifSound = () => {
    const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
    audio.play().catch(e => console.log("Audio play failed:", e));
  };

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      await Promise.all([fetchProducts(), fetchBranchInfo(), fetchPendingOrders(), checkActiveShift()]);
      setLoading(false);
    };
    loadData();

    const interval = setInterval(async () => {
        try {
          await fetchPendingOrders();
          // Chỉ cập nhật bàn nếu không đang trong quá trình thanh toán hoặc lưu
          if (!isPaymentModalOpen && !isSaving) {
            await fetchTables();
          }
        } catch (e) {
          console.warn("Mất kết nối với máy chủ, đang thử lại...");
        }
    }, 5000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    if (branchInfo?.id) {
      fetchTables();
    }
  }, [branchInfo?.id]);

  const currentCart = tableCarts[selectedTableId] || [];
  const previewTotal = currentCart.reduce((sum, item) => sum + (item.totalItemPrice * item.quantity), 0);
  const totalAmount = authoritativeTotals[selectedTableId] ?? previewTotal;
  const invalidateAuthoritativeTotal = (tableId = selectedTableId) => {
    setAuthoritativeTotals(previous => {
      if (!(tableId in previous)) return previous;
      const next = { ...previous };
      delete next[tableId];
      return next;
    });
  };

  // Tự động đồng bộ giỏ hàng của bàn đang mở nếu thấy dữ liệu local đang trống
  useEffect(() => {
    if (selectedTableId === 'delivery') return;
    const table = tables.find(t => t.id === selectedTableId);

    // Chỉ tự động nạp lại nếu bàn đang có khách VÀ giỏ hàng local thực sự trống
    // Để tránh việc nạp chồng dữ liệu khi nhân viên đang thao tác.
    if (table && table.status === 'Có khách' && !isSaving && !isPaymentModalOpen) {
       const localCart = tableCarts[selectedTableId] || [];
       if (localCart.length === 0) {
          handleSelectTable(selectedTableId);
       }
    }
  }, [pendingOrders, selectedTableId]);

  const handleDeleteOrder = async (orderId: string, tableName?: string) => {
    if (!window.confirm(`Bạn có chắc muốn HỦY đơn hàng này${tableName ? ` tại ${tableName}` : ''}? Đơn sẽ được lưu vào lịch sử với trạng thái "Đã hủy".`)) return;

    try {
      const response = await fetch(`${API_URL}/api/Order/${orderId}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          status: 'Đã hủy',
          createdBy: userName,
          branchName: branchInfo?.name,
          branchId: branchInfo?.id
        })
      });

      if (response.ok) {
        setActiveOrderIds(prev => { const next = { ...prev }; delete next[selectedTableId]; return next; });
        // Nếu đang mở bàn này, xóa giỏ hàng local
        if (tableName) {
          const table = tables.find(t => t.name === tableName);
          if (table) {
            setTableCarts(prev => ({ ...prev, [table.id]: [] }));
            await updateTableStatus(table.id, 'Trống');
          }
        }
        // Phát sự kiện để Navbar cập nhật lại số lượng chuông thông báo ngay lập tức
        window.dispatchEvent(new CustomEvent('refresh-notifications'));
        await fetchPendingOrders();
      } else {
        const errorData = await response.json().catch(() => ({}));
        alert(`Lỗi khi hủy đơn hàng: ${errorData.message || response.statusText}`);
      }
    } catch (err) {
      console.error('Lỗi khi hủy đơn hàng:', err);
      alert('Không thể kết nối đến máy chủ để hủy đơn hàng.');
    }
  };
  const selectedTable = tables.find(t => t.id === selectedTableId) || null;
  const selectedTableIdRef = useRef(selectedTableId);
  const tablesRef = useRef(tables);
  const branchInfoRef = useRef(branchInfo);

  useEffect(() => {
    selectedTableIdRef.current = selectedTableId;
    tablesRef.current = tables;
    branchInfoRef.current = branchInfo;
  }, [selectedTableId, tables, branchInfo]);

  const loadKitchenStatuses = async (orderId: string, tableId: string) => {
    try {
      const response = await fetch(`${API_URL}/api/Order/${orderId}/kitchen-status`);
      if (!response.ok) return;
      const requests = await response.json();
      const latestByItem = new Map<string, string>();
      requests.forEach((request: any) => {
        request.items?.forEach((item: any) => {
          latestByItem.set(`${item.productName}|${item.options || ''}`, request.status);
        });
      });

      setTableCarts(prev => ({
        ...prev,
        [tableId]: (prev[tableId] || []).map(item => ({
          ...item,
          kitchenStatus: latestByItem.get(`${item.name}|${item.optionsText || ''}`)
        }))
      }));
    } catch (err) {
      console.warn('Không thể tải trạng thái món từ bếp.', err);
    }
  };

  // Bàn đã bị tắt/xóa không còn là lựa chọn hợp lệ. Chuyển về quầy Mang về
  // (mã ảo của POS, không phải một bàn trong sơ đồ) để nhân viên vẫn lập đơn.
  useEffect(() => {
    if (selectedTableId !== 'delivery' && !tables.some(table => table.id === selectedTableId)) {
      setSelectedTableId('delivery');
    }
  }, [tables, selectedTableId]);

  const updateTableStatus = async (tableId: string, status: string) => {
    if (tableId === 'delivery') return;
    try {
      await fetch(`${API_URL}/api/Table/${tableId}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(status)
      });
      await fetchTables();
    } catch (err) {
      console.error('Error updating table status:', err);
    }
  };

  const handleSelectTable = async (tableId: string, forceFetch = false) => {
    // TRƯỚC KHI ĐỔI BÀN: Lưu đơn của bàn hiện tại nếu có món
    if (selectedTableId && tableCarts[selectedTableId]?.length > 0) {
       handleSaveOrder(true);
    }

    setSelectedTableId(tableId);
    setPosTab('menu');

    const table = tables.find(t => t.id === tableId);
    if (!table || tableId === 'delivery') return;

    // Nếu bàn đang có khách hoặc ép buộc nạp (từ thông báo), hãy tải đơn hàng từ server về để đồng bộ
    if (table.status === 'Có khách' || forceFetch) {
      const fetchOrderDetails = async (retryCount = 0) => {
        try {
          const branchId = branchInfo?.id || localStorage.getItem('selectedBranchId');
          // Thêm timestamp để tránh bị cache và nạp lại món tin cậy hơn
          const response = await fetch(`${API_URL}/api/Order?status=Đang xử lý&branchId=${branchId || ''}&tableName=${encodeURIComponent(table.name)}&_t=${Date.now()}`);

          if (response.ok) {
            const orders = await response.json();
            const activeOrder = orders[0];

            if (activeOrder && activeOrder.details) {
               const serverCart: CartItem[] = activeOrder.details.map((d: any) => ({
                  id: d.productId || d.toppingId || d.id,
                  name: d.productName,
                  quantity: d.quantity,
                  sentQuantity: d.sentQuantity || 0,
                  totalItemPrice: d.unitPrice,
                  selectedToppings: d.options && !d.options.includes('Ghi chú:') ? d.options.split(', ').map((name: string) => ({ name })) : [],
                  note: d.options?.includes('Ghi chú:') ? d.options.split('Ghi chú: ')[1] : '',
                  optionsText: d.options || '',
                  isStandaloneTopping: !!d.toppingId,
                  selectedSize: d.productName.includes('(') ? { name: d.productName.split('(')[1].split(')')[0] } : null
               }));

                setTableCarts(prev => ({ ...prev, [tableId]: serverCart }));
                setAuthoritativeTotals(prev => ({ ...prev, [tableId]: Number(activeOrder.totalAmount) }));
               setTableCustomers(prev => ({
                  ...prev,
                  [tableId]: {
                     phone: activeOrder.customerPhone || '',
                     name: activeOrder.customerName || 'Khách lẻ',
                     email: activeOrder.customerEmail || ''
                  }
               }));
               void loadKitchenStatuses(activeOrder.id, tableId);

               // Nếu đang forceFetch mà thấy có đơn, cập nhật luôn trạng thái bàn locally
               if (table.status === 'Trống') {
                  updateTableStatus(tableId, 'Có khách');
               }
               return true;
            }
          }
        } catch (err) {
          console.warn(`Thử nạp lại món lần ${retryCount + 1}...`);
          if (retryCount < 5) {
            setTimeout(() => fetchOrderDetails(retryCount + 1), 1500);
          }
        }
        return false;
      };

      fetchOrderDetails();
    } else {
      // Nếu bàn trống, đảm bảo giỏ hàng local cũng trống
      setTableCarts(prev => ({ ...prev, [tableId]: [] }));
      setTableCustomers(prev => ({ ...prev, [tableId]: { phone: '', name: 'Khách lẻ' } }));
    }
  };

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/kitchenHub`, {
        accessTokenFactory: () => localStorage.getItem('token') || ''
      })
      .withAutomaticReconnect()
      .build();

    const onRequestStatusUpdated = async () => {
      const tableId = selectedTableIdRef.current;
      if (!tableId || tableId === 'delivery') return;

      const table = tablesRef.current.find(t => t.id === tableId);
      if (!table) return;

      try {
        const branchId = branchInfoRef.current?.id || localStorage.getItem('selectedBranchId');
        const response = await fetch(`${API_URL}/api/Order?status=Đang xử lý&branchId=${branchId || ''}&tableName=${encodeURIComponent(table.name)}&_t=${Date.now()}`);
        if (!response.ok) return;
        const orders = await response.json();
        const activeOrder = orders[0];
        if (activeOrder?.id) await loadKitchenStatuses(activeOrder.id, tableId);
      } catch (err) {
        console.warn('Không thể đồng bộ trạng thái món từ bếp.', err);
      }
    };

    connection.on('RequestStatusUpdated', onRequestStatusUpdated);
    const onPaymentCompleted = () => { void fetchInvoiceHistory(); };
    connection.on('PaymentCompleted', onPaymentCompleted);
    connection.start().catch(err => console.warn('Kitchen status connection failed.', err));

    return () => {
      connection.off('RequestStatusUpdated', onRequestStatusUpdated);
      connection.off('PaymentCompleted', onPaymentCompleted);
      void connection.stop();
    };
  }, []);

  useEffect(() => {
    if (!isInvoiceHistoryModalOpen) return;
    const poll = window.setInterval(() => { void fetchInvoiceHistory(); }, 10000);
    return () => window.clearInterval(poll);
  }, [isInvoiceHistoryModalOpen, invoiceSearch, invoiceFromDate, invoiceToDate, invoiceStatusFilter]);

  const addToCart = (product: Product) => {
    let productSizes = [];
    let productToppings = [];

    try {
      if (product.sizesJson) productSizes = JSON.parse(product.sizesJson);
      if (product.toppingsJson) productToppings = JSON.parse(product.toppingsJson);
    } catch (e) {
      console.error("Error parsing product options:", e);
    }

    if (productSizes.length > 0 || productToppings.length > 0) {
       setCurrentCustomizingProduct(product);
       setSelectedSize(productSizes.length > 0 ? productSizes[0] : null);
       setSelectedToppings([]);
       setIsOptionsModalOpen(true);
       return;
    }

    const cart = tableCarts[selectedTableId] || [];
    const existingIndex = cart.findIndex(item => item.id === product.id && !item.selectedSize && (!item.selectedToppings || item.selectedToppings.length === 0));

    setTableCarts(prev => {
      const currentCart = prev[selectedTableId] || [];
      let newCart;

      if (existingIndex >= 0) {
        newCart = currentCart.map((item, idx) =>
          idx === existingIndex ? { ...item, quantity: item.quantity + 1 } : item
        );
      } else {
        newCart = [...currentCart, { ...product, quantity: 1, sentQuantity: 0, note: '', totalItemPrice: product.price }];
      }

      return { ...prev, [selectedTableId]: newCart };
    });
    invalidateAuthoritativeTotal();

    if (selectedTableId !== 'delivery' && cart.length === 0) {
      updateTableStatus(selectedTableId, 'Có khách');
    }
  };

  const handleConfirmOptions = () => {
    if (!currentCustomizingProduct) return;

    const toppingPrice = selectedToppings.reduce((sum, t) => sum + t.price, 0);
    const sizePrice = selectedSize ? selectedSize.price : 0;
    const finalUnitPrice = currentCustomizingProduct.price + sizePrice + toppingPrice;

    setTableCarts(prev => {
      const currentCart = prev[selectedTableId] || [];
      const newCart = [
        ...currentCart,
        {
           ...currentCustomizingProduct,
           quantity: 1,
           sentQuantity: 0,
           note: '',
           selectedSize,
           selectedToppings,
           totalItemPrice: finalUnitPrice
        }
      ];
      return { ...prev, [selectedTableId]: newCart };
    });
    invalidateAuthoritativeTotal();

    if (selectedTableId !== 'delivery' && (tableCarts[selectedTableId] || []).length === 0) {
       updateTableStatus(selectedTableId, 'Có khách');
    }

    setIsOptionsModalOpen(false);
    setCurrentCustomizingProduct(null);
  };

  const updateQuantity = (productId: string, delta: number, index: number) => {
    setTableCarts(prev => {
      const cart = [...(prev[selectedTableId] || [])];
      if (cart[index] && cart[index].id === productId) {
        const newQty = cart[index].quantity + delta;
        // Không cho phép giảm xuống thấp hơn số lượng đã gửi bếp
        const minQty = cart[index].sentQuantity || 0;

        if (newQty >= minQty && newQty > 0) {
          cart[index] = { ...cart[index], quantity: newQty };
        } else if (newQty === 0 && minQty === 0) {
          cart.splice(index, 1);
        } else if (newQty < minQty) {
          alert(`Món này đã gửi bếp ${minQty} phần, không thể giảm thêm. Vui lòng dùng chức năng Hủy món nếu cần.`);
          return prev;
        }
      }
      return { ...prev, [selectedTableId]: cart };
    });
    invalidateAuthoritativeTotal();

    if (selectedTableId !== 'delivery' && (tableCarts[selectedTableId] || []).length === 1 && delta < 0) {
      // Cẩn thận: Chỉ cập nhật trạng thái bàn nếu sau khi xóa giỏ hàng thực sự trống
    }
  };

  const removeFromCart = (productId: string, index: number) => {
    setTableCarts(prev => {
      const cart = [...(prev[selectedTableId] || [])];
      if (cart[index] && cart[index].id === productId) {
        if ((cart[index].sentQuantity || 0) > 0) {
          alert("Món ăn đã được gửi xuống bếp, không thể xóa trực tiếp. Vui lòng dùng chức năng Hủy món.");
          return prev;
        }
        cart.splice(index, 1);
      }
      return { ...prev, [selectedTableId]: cart };
    });
    invalidateAuthoritativeTotal();
  };

  const updateCustomerForTable = (field: 'phone' | 'name' | 'email', value: string) => {
    setTableCustomers(prev => ({
      ...prev,
      [selectedTableId]: {
        ...(prev[selectedTableId] || { phone: '', name: 'Khách lẻ' }),
        [field]: value
      }
    }));
  };

  const currentCustomer = tableCustomers[selectedTableId] || { phone: '', name: 'Khách lẻ' };

  const checkCustomer = async (phone: string) => {
    if (phone.length < 10) return;
    try {
      setIsCheckingPhone(true);
      const token = localStorage.getItem('token');
      const response = await fetch(`${API_URL}/api/Customer/${phone}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (response.ok) {
        const data = await response.json();
        setTableCustomers(prev => ({
          ...prev,
          [selectedTableId]: {
            id: data.id,
            phone: phone,
            name: data.fullName,
            email: data.email || '',
            loyaltyPoints: data.loyaltyPoints || 0
          }
        }));
      }
    } catch (err) {
      console.error(err);
    } finally {
      setIsCheckingPhone(false);
    }
  };

  const handleRedeemPoints = async () => {
    const customer = tableCustomers[selectedTableId];
    const orderId = activeOrderIds[selectedTableId];
    if (!customer?.id || !orderId || pointsToRedeem <= 0) return;

    try {
      setIsSaving(true);
      const response = await fetch(`${API_URL}/api/Order/${orderId}/redeem`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: JSON.stringify({ customerId: customer.id, points: pointsToRedeem })
      });

      if (response.ok) {
        const data = await response.json();
        alert(data.message);
        setPointsToRedeem(0);
        // Refresh order data
        handleSelectTable(selectedTableId, true);
        // Also refresh customer to get new loyalty balance
        checkCustomer(customer.phone);
      } else {
        const error = await response.json();
        alert(error.message || 'Lỗi khi đổi điểm');
      }
    } catch (err) {
      console.error(err);
      alert('Lỗi kết nối khi đổi điểm');
    } finally {
      setIsSaving(false);
    }
  };

  const handleAcceptWebOrder = async (targetTableId: string) => {
    if (!orderToAccept || isSaving) return;

    try {
      setIsSaving(true);
      const acceptedId = orderToAccept.id;
      const acceptResponse = await fetch(`${API_URL}/api/Order/${acceptedId}/accept`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tableId: targetTableId === 'delivery' ? null : targetTableId })
      });

      if (acceptResponse.status === 409) {
        const conflict = await acceptResponse.json().catch(() => ({}));
        alert(conflict.message || 'Đơn này đã được nhân viên khác tiếp nhận hoặc không còn chờ xử lý.');
        await Promise.all([fetchPendingOrders(), fetchTables()]);
        return;
      }
      if (!acceptResponse.ok) {
        const error = await acceptResponse.json().catch(() => ({}));
        throw new Error(error.message || 'Không thể tiếp nhận đơn hàng.');
      }

      const response = await acceptResponse.json();
      const acceptedOrder = response.order || response;
      const details = acceptedOrder.details || orderToAccept.details;
      const acceptedItems: CartItem[] = details.map((d: any) => ({
        id: d.productId || d.toppingId || d.id,
        name: d.productName,
        price: d.unitPrice,
        quantity: d.quantity,
        sentQuantity: d.sentQuantity || 0,
        note: d.options?.includes('Ghi chú:') ? d.options.split('Ghi chú: ')[1] : '',
        totalItemPrice: d.unitPrice,
        optionsText: d.options,
        isStandaloneTopping: !!d.toppingId
      }));
      const tableName = targetTableId === 'delivery' ? 'Mang về' : tables.find(t => t.id === targetTableId)?.name || 'Bàn';

      setTableCarts(prev => ({ ...prev, [targetTableId]: acceptedItems }));
      setActiveOrderIds(prev => ({ ...prev, [targetTableId]: acceptedOrder.id }));
      if (orderToAccept.customerPhone) {
        setTableCustomers(prev => ({
          ...prev,
          [targetTableId]: {
            phone: orderToAccept.customerPhone,
            name: orderToAccept.customerName || 'Khách lẻ',
            email: orderToAccept.customerEmail || ''
          }
        }));
      }

      setSelectedTableId(targetTableId);
      setPosTab('menu');
      setPendingOrders(prev => prev.filter(o => o.id !== acceptedId));

      await Promise.all([fetchTables(), fetchPendingOrders()]);

      window.dispatchEvent(new CustomEvent('refresh-notifications'));
      alert(`Đã chấp nhận đơn hàng và chuyển vào ${tableName}`);
    } catch (err: any) {
      console.error("Lỗi khi chấp nhận đơn:", err);
      alert(`Lỗi hệ thống: ${err.message}`);
    } finally {
      setIsSaving(false);
      setOrderToAccept(null);
    }
  };

  const handlePayment = async () => {
    if (currentCart.length === 0) {
      alert('Vui lòng chọn món trước khi thanh toán!');
      return;
    }
    setIsPaymentModalOpen(true);
  };

  const [isSaving, setIsSaving] = useState(false);
  const [isSendingToKitchen, setIsSendingToKitchen] = useState(false);

  // Tự động đồng bộ đơn hàng lên Server khi giỏ hàng thay đổi (Real-time Sync)
  useEffect(() => {
    if (currentCart.length === 0 || isSaving) return;

    const timer = setTimeout(() => {
      handleSaveOrder(true);
    }, 1500); // Tăng lên 1.5 giây để giảm tải server

    return () => clearTimeout(timer);
  }, [currentCart, selectedTableId]);

  const handleSendToKitchen = async () => {
    setKitchenMessage(null);
    const hasNewItems = currentCart.some(item => item.quantity > item.sentQuantity);
    if (!hasNewItems) {
      setKitchenMessage({ type: 'error', text: 'Không có món mới cần gửi bếp.' });
      return;
    }

    try {
      setIsSendingToKitchen(true);
      // 1. Đảm bảo đơn hàng đã được lưu trước khi gửi bếp
      const saved = await handleSaveOrder(true);
      if (!saved) {
        setKitchenMessage({ type: 'error', text: 'Không thể lưu đơn hàng trước khi gửi bếp.' });
        return;
      }

      // 2. Tìm orderId hiện tại trên server cho bàn này
      const branchId = branchInfo?.id || localStorage.getItem('selectedBranchId');
      const orderTableName = selectedTable?.name || 'Mang về';
      const orderRes = await fetch(`${API_URL}/api/Order?status=Đang xử lý&branchId=${branchId || ''}&tableName=${encodeURIComponent(orderTableName)}&_t=${Date.now()}`, { cache: 'no-store' });
      if (!orderRes.ok) {
        const errorText = await orderRes.text();
        throw new Error(errorText || `Không thể tìm đơn hàng (${orderRes.status})`);
      }
      const orders = await orderRes.json();
      const activeOrder = orders[0];

      if (!activeOrder) {
        setKitchenMessage({ type: 'error', text: 'Không tìm thấy đơn hàng đang xử lý.' });
        return;
      }

      // 3. Gọi API gửi bếp
      const response = await fetch(`${API_URL}/api/Order/${activeOrder.id}/send-to-kitchen`, {
        method: 'POST'
      });

      if (response.ok) {
        setKitchenMessage({ type: 'success', text: 'Đã gửi món xuống bếp thành công.' });
        // 4. Cập nhật local ngay cả với Mang về (không có tableId vật lý để reload).
        setTableCarts(prev => ({
          ...prev,
          [selectedTableId]: (prev[selectedTableId] || []).map(item => ({ ...item, sentQuantity: item.quantity }))
        }));
        if (selectedTableId !== 'delivery') handleSelectTable(selectedTableId);
      } else {
        // API returns plain text for InvalidOperationException (e.g. no new
        // items on a second send), not JSON. Do not turn that into a fake
        // network error by unconditionally calling response.json().
        const errorText = await response.text();
        let message = errorText;
        try {
          const error = JSON.parse(errorText);
          message = error.message || error.title || errorText;
        } catch {
          // Keep the plain-text API message.
        }
        setKitchenMessage({ type: 'error', text: message || `Không thể gửi bếp (${response.status})` });
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : '';
      setKitchenMessage({ type: 'error', text: message ? `Lỗi khi gửi bếp: ${message}` : 'Lỗi kết nối khi gửi bếp.' });
    } finally {
      setIsSendingToKitchen(false);
    }
  };

  const handleManualRefresh = async () => {
    try {
      setLoading(true);
      await Promise.all([
        fetchProducts(),
        fetchBranchInfo(),
        fetchPendingOrders(),
        checkActiveShift(),
        fetchTables()
      ]);
    } catch (err) {
      console.error("Manual refresh failed:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleSaveOrder = async (isAuto = false): Promise<boolean> => {
    if (currentCart.length === 0 || isSaving) return false;

    try {
      setIsSaving(true);
      const order = {
        ...(activeOrderIds[selectedTableId] ? { id: activeOrderIds[selectedTableId] } : {}),
        tableName: selectedTable ? selectedTable.name : 'Mang về',
        totalAmount: totalAmount,
        paidAmount: 0,
        status: 'Đang xử lý',
        customerName: currentCustomer.name,
        customerPhone: currentCustomer.phone || null,
        createdBy: userName,
        branchId: branchInfo?.id && branchInfo.id !== '' ? branchInfo.id : null,
        branchName: branchInfo?.name,
        details: currentCart.map(item => ({
          productId: (item as any).isStandaloneTopping ? null : item.id,
          toppingId: (item as any).isStandaloneTopping ? item.id : null,
          productName: `${item.name}${item.selectedSize ? ` (${item.selectedSize.name})` : ''}`,
          quantity: item.quantity,
          sentQuantity: item.sentQuantity || 0, // Gửi kèm số lượng đã gửi bếp
          unitPrice: item.totalItemPrice,
          options: item.selectedToppings?.map(t => t.name).join(', ') || item.note || ''
        }))
      };

      const response = await fetch(`${API_URL}/api/Order`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(order)
      });

       if (response.ok) {
         const savedOrder = await response.json();
         if (savedOrder?.id) setActiveOrderIds(prev => ({ ...prev, [selectedTableId]: savedOrder.id }));
         if (Number.isFinite(Number(savedOrder?.totalAmount))) {
           setAuthoritativeTotals(prev => ({ ...prev, [selectedTableId]: Number(savedOrder.totalAmount) }));
         }
        if (!isAuto) {
          alert('Đã đồng bộ đơn hàng lên hệ thống!');
        }

        // Tự động cập nhật trạng thái bàn nếu cần
        if (selectedTableId !== 'delivery') {
          const table = tables.find(t => t.id === selectedTableId);
          if (table && table.status !== 'Có khách') {
            await updateTableStatus(selectedTableId, 'Có khách');
          }
        }
        return true;
      }
      if (!isAuto) alert('Lỗi khi gửi đơn vào hệ thống.');
      return false;
    } catch (err) {
      if (!isAuto) alert('Lỗi khi gửi đơn vào hệ thống.');
      return false;
    } finally {
      setIsSaving(false);
    }
  };

  const confirmPayment = async () => {
    if (isSaving) {
       alert('Hệ thống đang lưu dữ liệu, vui lòng đợi giây lát...');
       return;
    }

    try {
      setIsSaving(true);
      const existingOrderId = activeOrderIds[selectedTableId];
      const order = {
        ...(existingOrderId ? { id: existingOrderId } : {}),
        tableName: selectedTable ? selectedTable.name : 'Mang về',
        totalAmount: totalAmount,
        paidAmount: 0,
        paymentMethod: null,
        customerName: currentCustomer.phone ? currentCustomer.name : 'Khách lẻ',
        customerPhone: currentCustomer.phone || null,
        customerEmail: currentCustomer.email || null,
        status: 'Đang xử lý',
        createdBy: userName || 'Hệ thống',
        branchId: branchInfo?.id && branchInfo.id !== '' ? branchInfo.id : null,
        branchName: branchInfo?.name,
        details: currentCart.map(item => ({
          productId: (item as any).isStandaloneTopping ? null : item.id,
          toppingId: (item as any).isStandaloneTopping ? item.id : null,
          productName: `${item.name}${item.selectedSize ? ` (${item.selectedSize.name})` : ''}`,
          quantity: item.quantity,
          sentQuantity: item.sentQuantity || 0, // Gửi kèm số lượng đã gửi bếp
          unitPrice: item.totalItemPrice,
          options: item.selectedToppings?.map(t => t.name).join(', ') || item.note || ''
        }))
      };

        // Persist first so the server recalculates from its product prices and the
        // branch's settings. The cart total remains only a pre-submit preview.
        const response = await fetch(`${API_URL}/api/Order`, {
           method: 'POST',
           headers: { 'Content-Type': 'application/json' },
           body: JSON.stringify(order)
        });

       if (response.ok) {
         const savedOrder = await response.json();
         const authoritativeTotal = Number(savedOrder.totalAmount);
         if (!Number.isFinite(authoritativeTotal) || authoritativeTotal < 0) {
           throw new Error('Máy chủ trả về tổng tiền không hợp lệ.');
         }
         setActiveOrderIds(prev => ({ ...prev, [selectedTableId]: savedOrder.id }));
         setAuthoritativeTotals(prev => ({ ...prev, [selectedTableId]: authoritativeTotal }));

         if (currentCustomer.phone && currentCustomer.phone.length >= 10) {
           await fetch(`${API_URL}/api/Customer`, {
             method: 'POST',
             headers: { 'Content-Type': 'application/json' },
             body: JSON.stringify({
               fullName: currentCustomer.name,
               phoneNumber: currentCustomer.phone,
               totalSpending: authoritativeTotal
             })
           });
         }

         const paymentResponse = await fetch(`${API_URL}/api/Order/${savedOrder.id}/payment`, {
           method: 'POST', headers: { 'Content-Type': 'application/json' },
           body: JSON.stringify({ amount: authoritativeTotal, paymentMethod })
         });

        if (!paymentResponse.ok) {
          const error = await paymentResponse.json().catch(() => ({}));
          alert(`Lỗi thanh toán: ${error.message || 'Không thể thanh toán hóa đơn'}`);
          return;
        }

        setIsPaymentModalOpen(false);
        if (selectedTableId !== 'delivery') {
          await updateTableStatus(selectedTableId, 'Trống');
        }

        setTableCarts(prev => {
           const updated = { ...prev };
           updated[selectedTableId] = [];
           return updated;
        });
        setActiveOrderIds(prev => { const next = { ...prev }; delete next[selectedTableId]; return next; });
        invalidateAuthoritativeTotal();

        setTableCustomers(prev => {
           const updated = { ...prev };
           updated[selectedTableId] = { phone: '', name: 'Khách lẻ' };
           return updated;
        });

        alert('Thanh toán thành công!');
        setIsPaymentModalOpen(false);
        setSelectedTableId('delivery');
      } else {
        const error = await response.json();
        alert(`Lỗi: ${error.message || 'Không thể lưu hóa đơn'}`);
      }
    } catch (err) {
      alert('Lỗi kết nối đến server.');
    } finally {
      setIsSaving(false);
    }
  };

  const filteredProducts = products.filter(p => {
    const matchesCategory = activeCategory === 'Tất cả' || p.category === activeCategory;
    const matchesSearch = p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         (p.id && p.id.toLowerCase().includes(searchTerm.toLowerCase()));
    return matchesCategory && matchesSearch;
  });

  return (
    <div className="flex h-full min-h-0 flex-col md:flex-row bg-[#f8f9fa] overflow-hidden text-gray-800 font-sans">
      {/* LEFT SIDE: Tables / Menu Selection */}
      <div className="w-full h-[52%] min-h-0 flex flex-col border-b border-gray-300 bg-white md:h-full md:w-[60%] md:border-b-0 md:border-r">
        {/* Header Left (Blue) */}
        <div className="bg-[#0070f4] p-1 flex flex-wrap items-center gap-1">
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
                    {Array.isArray(allBranches) && allBranches.map(b => (
                        <div
                            key={b.id}
                            onClick={() => handleSwitchBranch(b)}
                            className={`px-4 py-3 hover:bg-blue-50 cursor-pointer flex items-center justify-between border-b border-gray-50 last:border-0 transition-colors ${branchInfo?.id === b.id ? 'bg-blue-50/80' : ''}`}
                        >
                            <div className="flex-1 min-w-0 pr-4">
                                <div className="flex items-center space-x-3">
                                   <div className="w-8 h-8 rounded-lg overflow-hidden shrink-0 bg-gray-100 border border-gray-200">
                                      {b.imageUrl ? <img src={b.imageUrl} className="w-full h-full object-cover" alt=""/> : <Store size={14} className="m-auto mt-1.5 text-gray-400"/>}
                                   </div>
                                   <p className={`text-sm font-bold truncate ${branchInfo?.id === b.id ? 'text-blue-600' : 'text-gray-700'}`}>{b.name}</p>
                                   {b.isMain && <span className="ml-2 bg-orange-100 text-orange-600 text-[8px] px-1 rounded-sm font-bold uppercase">Trụ sở</span>}
                                </div>
                                <p className="text-[10px] text-gray-400 truncate mt-0.5 flex items-center ml-11"><MapPin size={10} className="mr-1"/> {b.address || 'Chưa cập nhật địa chỉ'}</p>
                            </div>
                            {branchInfo?.id === b.id && <div className="bg-blue-600 rounded-full p-1"><CheckCircle2 size={12} className="text-white" /></div>}
                        </div>
                    ))}
                    </div>
                    <div className="p-2 bg-gray-50 text-center border-t">
                    <button onClick={() => navigate('/branches')} className="text-[10px] text-blue-600 font-bold hover:underline">Quản lý danh sách chi nhánh</button>
                    </div>
                </div>
             )}
          </div>

          <button
            onClick={() => setPosTab('tables')}
            className={`shrink-0 px-2 sm:px-4 py-1.5 rounded-t text-sm font-bold flex items-center transition-colors ${posTab === 'tables' ? 'bg-white text-[#0070f4]' : 'text-white hover:bg-blue-600'}`}
          >
            <Grid size={16} className="mr-1 sm:mr-2" /> <span className="hidden sm:inline">Phòng bàn</span><span className="sm:hidden">Bàn</span>
          </button>
          <button
            onClick={() => setPosTab('menu')}
            className={`shrink-0 px-2 sm:px-4 py-1.5 rounded-t text-sm font-bold flex items-center transition-colors ${posTab === 'menu' ? 'bg-white text-[#0070f4]' : 'text-white hover:bg-blue-600'}`}
          >
            <ClipboardList size={16} className="mr-1 sm:mr-2" /> <span className="hidden sm:inline">Thực đơn</span><span className="sm:hidden">Món</span>
          </button>

          <div className="order-last sm:order-none w-full sm:flex-1 relative sm:ml-4">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-blue-200" />
            </span>
            <input
              type="text"
              className="w-full bg-blue-700/50 border-none rounded py-1.5 pl-10 pr-4 text-sm text-white placeholder-blue-200 outline-none focus:ring-1 focus:ring-white"
              placeholder="Tìm món (F3)"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <button
            onClick={handleManualRefresh}
            disabled={loading}
            className="p-2 text-white hover:bg-blue-600 rounded transition-all"
            title="Làm mới dữ liệu (F5)"
          >
            <RotateCcw size={20} className={loading ? 'animate-spin' : ''} />
          </button>
          <button onClick={() => document.getElementById('pos-product-search')?.focus()} className="p-2 text-white hover:bg-blue-600 rounded" title="Tìm món mới" aria-label="Tìm món mới"><Plus size={20}/></button>
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
        <div className="flex-1 min-h-0 overflow-auto p-2 sm:p-4 bg-gray-50">
          {loading ? (
            <div className="flex items-center justify-center h-full">
               <Loader2 size={32} className="animate-spin text-blue-600" />
            </div>
          ) : posTab === 'notifications' ? (
            <div className="space-y-8 max-w-4xl mx-auto p-4">
               {/* Đơn hàng Section */}
              <div>
                  <div className="flex justify-between items-center mb-6">
                     <h2 className="text-xl font-black text-gray-800 uppercase italic tracking-tighter flex items-center">
                        <ClipboardList className="mr-2 text-blue-600" /> Đơn hàng chờ duyệt
                     </h2>
                     <span className="bg-blue-100 text-blue-600 px-3 py-1 rounded-full text-[10px] font-black">{pendingOrders.length}</span>
                  </div>

                  {pendingOrders.length === 0 ? (
                    <div className="py-10 text-center text-gray-300 italic text-[10px] font-bold uppercase tracking-widest border-2 border-dashed border-gray-100 rounded-3xl">Không có đơn hàng mới</div>
                  ) : (
                    <div className="space-y-4">
                       {pendingOrders.map(order => (
                          <div key={order.id} className="bg-white p-5 rounded-2xl shadow-sm border border-gray-100 flex items-center justify-between group animate-in slide-in-from-right-2">
                             {/* ... (phần nội dung cũ của order) ... */}
                             <div className="flex items-center space-x-4">
                                <div className="w-12 h-12 bg-orange-50 rounded-xl flex items-center justify-center text-orange-600 font-black text-lg">
                                   {order.tableName?.charAt(0) || 'K'}
                                </div>
                                <div>
                                   <h3 className="font-black text-gray-800 text-sm uppercase tracking-tight flex items-center">
                                      {order.tableName}
                                      {order.status === 'Đổi quà' && <span className="ml-2 bg-red-100 text-red-600 text-[8px] px-1.5 py-0.5 rounded-full font-black animate-pulse">ĐỔI QUÀ</span>}
                                   </h3>
                                   <p className="text-[10px] text-gray-400 font-bold">{order.customerName}</p>
                                </div>
                             </div>
                             <div className="flex items-center space-x-2">
                                <button
                                  onClick={() => handleDeleteOrder(order.id, order.tableName)}
                                  className="bg-red-50 text-red-600 px-4 py-2 rounded-xl font-black text-[10px] uppercase hover:bg-red-600 hover:text-white transition-all"
                                >
                                   Từ chối
                                </button>
                                <button
                                  onClick={() => setOrderToAccept(order)}
                                  className="bg-blue-600 text-white px-5 py-2 rounded-xl font-black text-[10px] uppercase shadow-lg shadow-blue-500/20"
                                >
                                   Xử lý đơn
                                </button>
                             </div>
                          </div>
                       ))}
                    </div>
                  )}
               </div>

               {/* Lịch hẹn Section */}
               <div className="pt-8 border-t border-gray-100">
                  <div className="flex justify-between items-center mb-6">
                     <h2 className="text-xl font-black text-gray-800 uppercase italic tracking-tighter flex items-center">
                        <CalendarIcon className="mr-2 text-green-600" /> Lịch hẹn mới
                     </h2>
                     <span className="bg-green-100 text-green-600 px-3 py-1 rounded-full text-[10px] font-black">{pendingReservations.length}</span>
                  </div>

                  {pendingReservations.length === 0 ? (
                    <div className="py-10 text-center text-gray-300 italic text-[10px] font-bold uppercase tracking-widest border-2 border-dashed border-gray-100 rounded-3xl">Không có lịch hẹn mới</div>
                  ) : (
                    <div className="space-y-4">
                       {pendingReservations.map(res => (
                          <div key={res.id} className="bg-white p-5 rounded-2xl shadow-sm border border-green-50 flex items-center justify-between group animate-in slide-in-from-right-2">
                             <div className="flex items-center space-x-4">
                                <div className="w-12 h-12 bg-green-50 rounded-xl flex items-center justify-center text-green-600 font-black text-lg">
                                   {res.customerName.charAt(0)}
                                </div>
                                <div>
                                   <h3 className="font-black text-gray-800 text-sm uppercase tracking-tight">{res.customerName}</h3>
                                   <p className="text-[10px] text-gray-500 font-bold flex items-center mt-0.5">
                                      <Clock size={10} className="mr-1"/> {new Date(res.reservationTime).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'})} | {res.numberOfGuests} khách
                                   </p>
                                </div>
                             </div>
                             <button
                               onClick={() => navigate(userRole === 'cashier' ? '/pos/reservations' : '/reservations')}
                               className="bg-green-600 text-white px-5 py-2 rounded-xl font-black text-[10px] uppercase"
                             >
                               Xem chi tiết
                             </button>
                          </div>
                       ))}
                    </div>
                  )}
               </div>
            </div>
          ) : posTab === 'tables' ? (
            <div className="space-y-10">
               {/* Table Areas */}
               {Array.from(new Set(tables.map(t => t.areaName))).map(area => (
                 <div key={area} className="space-y-4">
                    <h3 className="text-[10px] font-black text-[#0070f4] uppercase tracking-[0.2em] flex items-center">
                      <div className="h-px bg-blue-100 flex-1 mr-4"></div>
                      Khu vực: {area}
                      <div className="h-px bg-blue-100 flex-1 ml-4"></div>
                    </h3>
                    <div className="grid grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-4">
                       {tables.filter(t => t.areaName === area).map((table, idx) => {
                          const hasItems = (tableCarts[table.id] || []).length > 0;
                          const itemCount = (tableCarts[table.id] || []).reduce((sum, item) => sum + item.quantity, 0);
                          return (
                            <div
                              key={`${table.id}-${idx}`}
                              onClick={() => handleSelectTable(table.id)}
                              className={`rounded-2xl shadow-sm aspect-square flex flex-col items-center justify-center cursor-pointer transition-all border-2 relative ${
                                selectedTableId === table.id
                                  ? 'bg-blue-50 border-[#0070f4] text-[#0070f4] ring-4 ring-blue-100 scale-105 z-10 shadow-xl'
                                  : table.status !== 'Trống' || hasItems
                                    ? 'bg-orange-50 border-orange-200 text-orange-700'
                                    : 'bg-white border-gray-100 text-gray-400 hover:border-blue-200 hover:text-[#0070f4]'
                              }`}
                            >
                               <span className="text-[9px] font-bold uppercase tracking-wide text-gray-400">{table.areaName || 'Khu vực chưa đặt tên'}</span>
                               <span className="text-xs font-black uppercase tracking-widest mt-0.5">{table.name}</span>
                               <span className={`text-[9px] font-bold mt-1 uppercase ${table.status === 'Trống' && !hasItems ? 'text-green-500' : 'text-orange-500'}`}>
                                 {hasItems || table.status === 'Có khách' ? 'Đang xử lý' : table.status}
                               </span>
                               {(hasItems || table.status === 'Có khách') && (
                                  <span className="absolute -top-2 -right-1 bg-blue-600 text-white text-[10px] font-black px-2 py-0.5 rounded-full border-2 border-white shadow-lg">
                                     {itemCount > 0 ? itemCount : '!'}
                                  </span>
                                )}
                                {table.status === 'Có khách' && (
                                   <div className="absolute -bottom-2 bg-[#0070f4] text-white text-[7px] font-black px-2 py-0.5 rounded-full uppercase tracking-tighter shadow-sm">Đang dùng</div>
                                )}
                            </div>
                          );
                       })}
                    </div>
                 </div>
               ))}
            </div>
          ) : (
            <div className="grid grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
              {filteredProducts.map((product, idx) => (
                <div
                  key={`${product.id}-${idx}`}
                  onClick={() => addToCart(product)}
                  className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden cursor-pointer hover:shadow-md hover:border-blue-400 transition-all group"
                >
                  <div className="aspect-square bg-gray-50 flex items-center justify-center relative">
                    {product.imageUrl && product.imageUrl !== 'string' ? (
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
                  <div className="text-[10px] text-gray-400 italic font-medium">Bàn đang chọn: <span className="text-blue-600 font-bold">{selectedTable ? `${selectedTable.areaName || 'Khu vực chưa đặt tên'} · ${selectedTable.name}` : 'Mang về'}</span></div>
        </div>
      </div>

      {/* RIGHT SIDE: Order Details */}
      <div className="w-full h-[48%] min-h-0 flex flex-col bg-white shadow-2xl md:h-full md:flex-1">
        {/* Order Tabs */}
        <div className="bg-[#1e293b] flex items-center p-1 space-x-1">
          <div className="bg-white text-gray-800 px-4 py-1.5 rounded-t text-xs font-bold flex items-center shadow-sm">
            {selectedTable ? `${selectedTable.areaName || 'Khu vực chưa đặt tên'} · ${selectedTable.name}` : 'Mang về'} <button onClick={() => setSelectedTableId('delivery')} title="Đóng đơn đang chọn" aria-label="Đóng đơn đang chọn" className="ml-2 text-gray-400 hover:text-red-500"><X size={12} /></button>
          </div>
          <div className="flex-1"></div>
          <div className="flex items-center space-x-3 pr-2">
             <button
               onClick={onLogout}
               className="bg-blue-500 hover:bg-blue-600 text-white px-3 py-1 rounded text-[11px] font-bold flex items-center transition-all mr-2 shadow-lg border border-blue-400"
               title="Đăng xuất khỏi phiên bán hàng"
             >
               <LogIn size={14} className="mr-1" /> Đăng xuất
             </button>
             <button onClick={() => setPosTab('notifications')} title="Thông báo" aria-label="Thông báo" className="text-white opacity-60 hover:opacity-100"><Bell size={18} /></button>
             <button onClick={() => window.print()} title="In đơn hiện tại" aria-label="In đơn hiện tại" className="text-white opacity-60 hover:opacity-100"><Printer size={18} /></button>
             <button onClick={() => setIsInvoiceHistoryModalOpen(true)} title="Lịch sử hóa đơn" aria-label="Lịch sử hóa đơn" className="text-white opacity-60 hover:opacity-100"><History size={18} /></button>
          </div>
        </div>

        {/* Order Info Bar */}
        <div className="p-2 flex items-center border-b space-x-2">
          {activeShift && userPosition === 'Thu ngân' && (
            <div className="flex space-x-1">
               <button
                onClick={() => setIsCloseShiftModalOpen(true)}
                className="bg-orange-100 text-orange-600 px-3 py-1 rounded text-[10px] font-black uppercase hover:bg-orange-200 transition-all border border-orange-200 flex items-center"
              >
                <RotateCcw size={12} className="mr-1" /> Chốt ca
              </button>
              <button
                onClick={() => setIsShiftHistoryModalOpen(true)}
                className="bg-gray-100 text-gray-500 px-3 py-1 rounded text-[10px] font-black uppercase hover:bg-gray-200 transition-all border border-gray-200 flex items-center"
              >
                <History size={12} className="mr-1" /> Lịch sử ca
              </button>
            </div>
          )}
          <div className="bg-blue-100 text-blue-700 px-3 py-1 rounded text-xs font-bold flex items-center whitespace-nowrap">
             <Utensils size={14} className="mr-1" /> {selectedTable ? `${selectedTable.areaName || 'Khu vực chưa đặt tên'} · ${selectedTable.name}` : 'Mang về'}
          </div>

          {selectedTable?.status === 'Có khách' && (
             <button
               onClick={() => handleSelectTable(selectedTableId)}
               className="p-1.5 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors border border-blue-100 shadow-sm"
               title="Đồng bộ lại món từ máy chủ"
             >
                <RotateCcw size={14} className={isSaving ? 'animate-spin' : ''} />
             </button>
          )}

          {selectedTableId !== 'delivery' && currentCart.length > 0 && (
             <button
               onClick={async () => {
                 // Tìm đơn hàng hiện tại của bàn trên server bằng tham số tableName thay vì search
                 try {
                   const branchId = branchInfo?.id || localStorage.getItem('selectedBranchId');
                   const res = await fetch(`${API_URL}/api/Order?status=Đang xử lý&branchId=${branchId || ''}&tableName=${encodeURIComponent(selectedTable?.name || '')}`);
                   const orders = await res.json();

                   // Tìm đơn hàng khớp chính xác bàn
                   const activeOrder = Array.isArray(orders) ? orders.find((o: any) => o.tableName === selectedTable?.name) : null;

                   if (activeOrder) {
                     await handleDeleteOrder(activeOrder.id, selectedTable?.name);
                   } else {
                     // Nếu không có trên server, chỉ xóa local
                     if (window.confirm('Không tìm thấy đơn hàng trên máy chủ. Xóa giỏ hàng local?')) {
                        setTableCarts(prev => ({ ...prev, [selectedTableId]: [] }));
                        await updateTableStatus(selectedTableId, 'Trống');
                     }
                   }
                 } catch (e) {
                   console.error('Lỗi khi truy tìm đơn để xóa:', e);
                 }
               }}
               className="p-1.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors border border-red-100 shadow-sm"
               title="Hủy/Xóa đơn bàn này"
             >
                <Trash2 size={16} />
             </button>
          )}

          <div className="flex-1 relative">
            <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
              <Search className="h-4 w-4 text-gray-400" />
            </span>
            <input
              id="pos-product-search"
              type="text"
              className="w-full bg-gray-50 border border-gray-200 rounded py-1 pl-10 pr-8 text-sm outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Tìm khách hàng (F4)"
              ref={customerPhoneInputRef}
              aria-label="Tìm khách hàng theo số điện thoại"
              value={currentCustomer.phone}
              onChange={(e) => {
                const value = e.target.value.replace(/\D/g, '');
                updateCustomerForTable('phone', value);
                if (value.length >= 10) checkCustomer(value);
              }}
            />
            <button onClick={() => customerPhoneInputRef.current?.focus()} title="Nhập thông tin khách hàng" aria-label="Nhập thông tin khách hàng" className="absolute right-2 top-1.5 rounded p-1 text-blue-600 hover:bg-blue-50"><UserPlus size={16} /></button>
          </div>
          <button onClick={() => {
            if (currentCart.some(item => item.sentQuantity > 0)) {
              alert('Món đã gửi bếp không thể xóa bằng thao tác này.');
              return;
            }
            if (currentCart.length > 0 && window.confirm('Xóa toàn bộ món chưa gửi trong đơn?')) setTableCarts(prev => ({ ...prev, [selectedTableId]: [] }));
          }} title="Xóa món chưa gửi" aria-label="Xóa món chưa gửi" className="p-1.5 text-gray-400 hover:bg-gray-100 rounded"><RotateCcw size={18}/></button>
        </div>

        {/* Selected Items List */}
        <div className="flex-1 overflow-auto px-4">
           {currentCart.length > 0 ? currentCart.map((item, idx) => (
             <div key={`${item.id}-${idx}`} className="flex items-start py-3 border-b border-gray-100 group transition-all">
                <div className="flex-1">
                   <div className="flex items-center">
                      <button onClick={() => removeFromCart(item.id, idx)} className="mr-2 text-red-300 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity">
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
                     onChange={(e) => {
                        const newNote = e.target.value;
                        const cart = [...(tableCarts[selectedTableId] || [])];
                        if (cart[idx]) cart[idx] = { ...cart[idx], note: newNote };
                        setTableCarts({ ...tableCarts, [selectedTableId]: cart });
                     }}
                   />
                </div>
                <div className="flex items-center space-x-4">
                   <div className="flex items-center border rounded-md overflow-hidden bg-gray-50">
                      <button onClick={() => updateQuantity(item.id, -1, idx)} className="px-1.5 py-1 hover:bg-gray-200 text-gray-400 transition-colors"><Minus size={12} /></button>
                      <input type="text" className="w-8 bg-transparent text-center text-xs font-bold outline-none" value={item.quantity} readOnly />
                      <button onClick={() => updateQuantity(item.id, 1, idx)} className="px-1.5 py-1 hover:bg-gray-200 text-blue-600 transition-colors"><Plus size={12} /></button>
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
              <button
                onClick={handleSendToKitchen}
                disabled={isSendingToKitchen || currentCart.length === 0}
                className={`col-span-4 py-4 rounded-xl font-bold flex flex-col items-center justify-center transition-all shadow-lg active:scale-[0.98] border-b-4 ${
                  'bg-orange-500 text-white hover:bg-orange-600 border-orange-700 shadow-orange-500/20'
                }`}
              >
                 {isSendingToKitchen ? <Loader2 size={20} className="animate-spin" /> : <Utensils size={20} />}
                 <span className="text-[10px] uppercase mt-1 font-black">Gửi bếp</span>
              </button>

              <button
                onClick={handlePayment}
                disabled={isSaving || currentCart.length === 0}
                className="col-span-8 bg-[#0070f4] text-white py-4 rounded-xl font-bold flex items-center justify-center space-x-3 hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/30 active:scale-[0.98] border-b-4 border-blue-800 disabled:cursor-not-allowed disabled:opacity-50"
              >
                 <span className="bg-white/20 p-2 rounded-full"><RotateCcw size={24} className="rotate-90"/></span>
                 <span className="text-xl uppercase tracking-tighter font-black italic">Thanh toán</span>
              </button>
           </div>
           {kitchenMessage && (
             <div
               role="status"
               className={`mt-3 rounded-lg px-3 py-2 text-center text-xs font-bold ${kitchenMessage.type === 'success' ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'}`}
             >
               {kitchenMessage.text}
             </div>
           )}
        </div>
      </div>

      {/* PAYMENT MODAL */}
      {isPaymentModalOpen && (
        <div className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-start md:items-center p-2 sm:p-4 backdrop-blur-sm">
           <div className="bg-white w-full max-w-2xl max-h-[calc(100dvh-1rem)] md:max-h-[calc(100dvh-2rem)] rounded-2xl shadow-2xl overflow-y-auto flex flex-col md:flex-row animate-in zoom-in-95 duration-200">
              {/* Left Side: Summary & Methods */}
              <div className="flex-1 p-4 sm:p-6 border-r">
                 <div className="sticky top-0 z-10 -mx-4 sm:-mx-6 px-4 sm:px-6 py-3 mb-3 bg-white/95 backdrop-blur flex justify-between items-center border-b border-gray-100">
                    <h3 className="font-black text-xl text-gray-800 uppercase italic tracking-tighter">Thanh toán hóa đơn</h3>
                    <button type="button" onClick={() => setIsPaymentModalOpen(false)} aria-label="Đóng thanh toán" className="shrink-0 ml-3 p-2 rounded-full bg-gray-100 text-gray-600 hover:bg-red-100 hover:text-red-600"><X size={24}/></button>
                 </div>

                 <div className="bg-blue-50 p-4 rounded-xl mb-6">
                    <div className="flex justify-between items-center mb-1">
                       <span className="text-gray-500 text-xs font-bold uppercase">Tổng tiền thanh toán</span>
                       <span className="bg-blue-600 text-white text-[10px] px-2 py-0.5 rounded-full font-bold">VNĐ</span>
                    </div>
                    <div className="text-3xl font-black text-blue-700">{totalAmount.toLocaleString()}</div>
                 </div>

                 {/* Customer Point System */}
                 <div className="bg-white border border-orange-100 rounded-xl p-4 mb-6 shadow-sm">
                    <div className="flex items-center justify-between mb-3">
                       <div className="flex items-center">
                          <span className="bg-orange-500 text-white text-[9px] font-black px-2 py-0.5 rounded mr-2 uppercase">Member</span>
                          <h4 className="text-xs font-bold text-gray-700 uppercase tracking-wider">Tích điểm khách hàng</h4>
                       </div>
                       {currentCustomer.loyaltyPoints !== undefined && (
                          <div className="text-[10px] font-black text-orange-600 bg-orange-50 px-2 py-0.5 rounded-full">
                             ĐIỂM HIỆN CÓ: {currentCustomer.loyaltyPoints}
                          </div>
                       )}
                    </div>
                    <div className="space-y-3">
                       <div className="relative">
                          <input
                            type="text"
                            placeholder="Nhập số điện thoại khách..."
                            className="w-full pl-3 pr-10 py-2.5 bg-gray-50 border border-gray-100 rounded-lg text-sm outline-none focus:ring-2 focus:ring-orange-500/20 focus:border-orange-500 font-bold"
                            value={currentCustomer.phone}
                            onChange={(e) => {
                              const val = e.target.value.replace(/\D/g, '');
                              updateCustomerForTable('phone', val);
                              if (val.length >= 10) checkCustomer(val);
                            }}
                          />
                          {isCheckingPhone && <Loader2 size={16} className="absolute right-3 top-3 animate-spin text-orange-500" />}
                       </div>
                       {currentCustomer.phone && (
                         <div className="animate-in slide-in-from-top-2 duration-300 space-y-3">
                            <input
                              type="text"
                              placeholder="Họ tên khách hàng..."
                              className="w-full px-3 py-2 bg-white border border-gray-100 rounded-lg text-xs outline-none focus:border-orange-300 font-medium"
                              value={currentCustomer.name}
                              onChange={(e) => updateCustomerForTable('name', e.target.value)}
                            />

                            {currentCustomer.loyaltyPoints !== undefined && currentCustomer.loyaltyPoints > 0 && (
                               <div className="pt-2 border-t border-orange-50">
                                  <label className="text-[9px] font-black text-gray-400 uppercase tracking-widest block mb-2">Đổi điểm giảm giá</label>
                                  <div className="flex space-x-2">
                                     <input
                                       type="number"
                                       className="flex-1 px-3 py-2 bg-gray-50 border border-gray-100 rounded-lg text-xs font-black text-orange-600 outline-none focus:border-orange-500"
                                       placeholder="Số điểm đổi..."
                                       value={pointsToRedeem || ''}
                                       max={currentCustomer.loyaltyPoints}
                                       min={0}
                                       onChange={(e) => setPointsToRedeem(Math.min(Number(e.target.value), currentCustomer.loyaltyPoints || 0))}
                                     />
                                     <button
                                       type="button"
                                       onClick={handleRedeemPoints}
                                       disabled={isSaving || pointsToRedeem <= 0}
                                       className="bg-orange-600 text-white px-4 py-2 rounded-lg text-[10px] font-black uppercase hover:bg-orange-700 transition-all disabled:opacity-50"
                                     >
                                        Áp dụng
                                     </button>
                                  </div>
                               </div>
                            )}

                            <p className="text-[10px] text-orange-600 italic font-medium">
                               * Hệ thống sẽ tích <b>+{Math.floor(totalAmount / 10000)} điểm</b> mới khi hoàn tất thanh toán (1 điểm / 10.000đ).
                            </p>
                         </div>
                       )}
                    </div>
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
                   disabled={isSaving || currentCart.length === 0}
                   className="w-full mt-8 bg-[#0070f4] text-white py-4 rounded-xl font-bold text-lg hover:bg-blue-700 transition-all shadow-lg active:scale-95 flex items-center justify-center disabled:cursor-not-allowed disabled:opacity-60"
                 >
                   {isSaving ? <><Loader2 size={20} className="mr-2 animate-spin" />Đang thanh toán...</> : 'Xác nhận thanh toán'}
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

      {/* NOTIFICATION TOAST */}
      {showNotification && (
        <div className="fixed top-6 right-6 z-[200] bg-[#0070f4] text-white p-6 rounded-[2rem] shadow-2xl flex items-center space-x-6 animate-in slide-in-from-right-10 duration-500 max-w-sm border-b-4 border-blue-800">
           <div className={`p-4 rounded-3xl relative ${notifType === 'order' ? 'bg-orange-400' : 'bg-green-500'}`}>
              {notifType === 'order' ? <ClipboardList size={32} className="animate-bounce" /> : <CalendarIcon size={32} className="animate-bounce" />}
              <span className="absolute top-2 right-2 w-3 h-3 bg-white rounded-full border-2 border-blue-400"></span>
           </div>
           <div>
              <p className="font-black uppercase text-sm tracking-widest italic">{notifType === 'order' ? 'Đơn hàng mới!' : 'Lịch hẹn mới!'}</p>
              <p className="text-xs opacity-80 mt-1 font-medium italic">
                 {notifType === 'order' ? 'Một khách vừa đặt món từ Web.' : 'Một khách vừa đặt bàn trước.'}
              </p>
              <button
                onClick={() => { setPosTab('notifications'); setShowNotification(false); }}
                className="mt-4 bg-white text-blue-600 px-6 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-blue-50 transition-all shadow-sm"
              >
                 XEM NGAY (F10)
              </button>
           </div>
           <button onClick={() => setShowNotification(false)} className="absolute top-4 right-4 text-white/50 hover:text-white transition-colors">
              <X size={18} />
           </button>
        </div>
      )}

      {/* ACCEPT ORDER MODAL (Form chuyển vào bàn) */}
      {orderToAccept && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-md">
           <div className="bg-white w-full max-w-2xl rounded-[3rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
                 <div>
                    <h3 className="font-black text-xl uppercase italic tracking-tighter">Xác nhận nhận đơn</h3>
                    <p className="text-[10px] font-bold opacity-80 uppercase tracking-widest">Chọn bàn để bắt đầu phục vụ món</p>
                 </div>
                 <button onClick={() => setOrderToAccept(null)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <div className="p-8 flex flex-col md:flex-row gap-8">
                 {/* Order Info Summary */}
                 <div className="w-full md:w-64 space-y-4">
                    <div className="bg-gray-50 p-5 rounded-3xl border border-gray-100">
                       <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-3">Thông tin từ khách</p>
                       <h4 className="text-lg font-black text-blue-600 uppercase tracking-tight mb-1">{orderToAccept.tableName}</h4>
                       <p className="text-xs font-bold text-gray-700">{orderToAccept.customerName}</p>
                       <div className="mt-4 pt-4 border-t border-dashed border-gray-200">
                          {orderToAccept.details.map((d: any, i: number) => (
                             <div key={i} className="flex justify-between items-center text-[11px] mb-1">
                                <div className="flex-1">
                                   <p className="font-bold text-gray-700">{d.quantity}x {d.productName}</p>
                                   <p className="text-[9px] text-gray-400 italic">{d.options}</p>
                                </div>
                                <div className="text-right">
                                   <p className="text-[9px] text-gray-400">{d.unitPrice.toLocaleString()}đ</p>
                                   <p className="font-black text-blue-700">{(d.quantity * d.unitPrice).toLocaleString()}đ</p>
                                </div>
                             </div>
                          ))}
                          <div className="mt-4 pt-2 border-t font-black text-right text-lg text-blue-800">
                             Tổng: {orderToAccept.totalAmount.toLocaleString()}đ
                          </div>
                       </div>
                    </div>
                 </div>

                 {/* Table Selection Grid */}
                 <div className="flex-1">
                    <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-4">Gán vào bàn thực tế:</p>
                    <div className="grid grid-cols-3 sm:grid-cols-4 gap-3 max-h-[300px] overflow-auto pr-2 no-scrollbar">
                       <button
                         onClick={() => handleAcceptWebOrder('delivery')}
                         disabled={isSaving}
                         className={`p-3 rounded-2xl border-2 flex flex-col items-center justify-center transition-all disabled:cursor-not-allowed disabled:opacity-50 ${orderToAccept.tableName === 'Mang về' ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100' : 'border-gray-100 hover:border-blue-200'}`}
                       >
                          <Utensils size={18} className="text-blue-500 mb-1" />
                          <span className="text-[10px] font-black uppercase">Mang về</span>
                       </button>
                       {tables.map((table, idx) => (
                          <button
                            key={`${table.id}-${idx}`}
                            onClick={() => handleAcceptWebOrder(table.id)}
                            disabled={isSaving || (table.status === 'Có khách' && table.name !== orderToAccept.tableName)}
                            className={`p-3 rounded-2xl border-2 flex flex-col items-center justify-center transition-all disabled:cursor-not-allowed disabled:opacity-50 ${
                               table.name === orderToAccept.tableName
                               ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100'
                               : table.status === 'Có khách'
                               ? 'bg-gray-50 border-gray-200 opacity-50 cursor-not-allowed'
                               : 'border-gray-100 hover:border-blue-200'
                            }`}
                          >
                            <span className="text-[9px] font-bold uppercase tracking-wide text-gray-400 text-center">{table.areaName || 'Khu vực chưa đặt tên'}</span>
                            <span className="text-xs font-black uppercase mt-0.5">{table.name}</span>
                             <span className="text-[8px] font-bold text-gray-400 mt-1 uppercase">{table.status}</span>
                          </button>
                       ))}
                    </div>
                 </div>
              </div>

              <div className="p-6 bg-gray-50 border-t flex items-center justify-between">
                 <button
                    onClick={() => {
                       handleDeleteOrder(orderToAccept.id, orderToAccept.tableName);
                       setOrderToAccept(null);
                    }}
                    className="bg-red-50 text-red-600 px-6 py-3 rounded-2xl font-black text-xs uppercase hover:bg-red-600 hover:text-white transition-all flex items-center"
                 >
                    <Trash2 size={16} className="mr-2" /> Từ chối / Hủy đơn
                 </button>
                 <p className="text-[10px] text-gray-400 italic font-medium ml-4 text-right">* Chọn một bàn trống hoặc "Mang về" để đồng bộ giỏ hàng.</p>
              </div>
           </div>
        </div>
      )}

      {/* OPEN SHIFT MODAL */}
      {isShiftModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[500] flex justify-center items-center p-4 backdrop-blur-md">
           <div className="bg-white w-full max-w-md rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-[#0070f4] p-8 text-white text-center">
                 <div className="w-20 h-20 bg-white/20 rounded-3xl flex items-center justify-center mx-auto mb-4">
                    <Banknote size={40} />
                 </div>
                 <h3 className="font-black text-2xl uppercase italic tracking-tighter">Bắt đầu ca làm việc</h3>
                 <p className="text-xs font-bold opacity-80 uppercase tracking-widest mt-2">Vui lòng khai báo tiền đầu ca</p>
              </div>
              <div className="p-8 space-y-6">
                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">Số tiền mặt trong két (VNĐ)</label>
                    <input
                      type="number"
                      value={startingCash}
                      onChange={(e) => setStartingCash(Number(e.target.value))}
                      className="w-full px-6 py-4 bg-gray-50 border-2 border-gray-100 rounded-2xl text-2xl font-black text-blue-700 focus:border-blue-500 outline-none transition-all"
                      placeholder="0"
                    />
                 </div>
                 <button
                   onClick={handleOpenShift}
                   className="w-full bg-[#0070f4] text-white py-4 rounded-2xl font-black text-lg hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/20 active:scale-95 uppercase italic tracking-tighter"
                 >
                   Mở ca ngay
                 </button>
                 <button
                   onClick={onLogout}
                   className="w-full text-gray-400 font-bold text-xs uppercase tracking-widest hover:text-gray-600 transition-all"
                 >
                   Quay lại đăng nhập
                 </button>
              </div>
           </div>
        </div>
      )}

      {/* CLOSE SHIFT MODAL */}
      {isCloseShiftModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[500] flex justify-center items-center p-4 backdrop-blur-md">
           <div className="bg-white w-full max-w-md rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-orange-500 p-8 text-white text-center">
                 <div className="w-20 h-20 bg-white/20 rounded-3xl flex items-center justify-center mx-auto mb-4">
                    <RotateCcw size={40} />
                 </div>
                 <h3 className="font-black text-2xl uppercase italic tracking-tighter">Kết thúc ca làm việc</h3>
                 <p className="text-xs font-bold opacity-80 uppercase tracking-widest mt-2">Tổng kết doanh thu và bàn giao két</p>
              </div>
              <div className="p-8 space-y-6">
                 <div className="bg-orange-50 p-4 rounded-2xl border border-orange-100">
                    <div className="flex justify-between items-center mb-1">
                       <span className="text-[10px] font-bold text-orange-600 uppercase">Thời gian bắt đầu</span>
                       <span className="text-xs font-black text-gray-700">{new Date(activeShift?.startTime).toLocaleString('vi-VN')}</span>
                    </div>
                    <div className="flex justify-between items-center">
                       <span className="text-[10px] font-bold text-orange-600 uppercase">Tiền đầu ca</span>
                       <span className="text-xs font-black text-gray-700">{activeShift?.startingCash?.toLocaleString()}đ</span>
                    </div>
                 </div>
                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">Số tiền mặt thực tế hiện tại (VNĐ)</label>
                    <input
                      type="number"
                      value={endingCash}
                      onChange={(e) => setEndingCash(Number(e.target.value))}
                      className="w-full px-6 py-4 bg-gray-50 border-2 border-orange-100 rounded-2xl text-2xl font-black text-orange-700 focus:border-orange-500 outline-none transition-all"
                      placeholder="0"
                    />
                 </div>
                 <div>
                    <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">Ghi chú bàn giao</label>
                    <textarea
                      className="w-full px-4 py-3 bg-gray-50 border-2 border-gray-100 rounded-2xl text-xs font-bold outline-none focus:border-blue-500"
                      rows={2}
                      value={shiftNote}
                      onChange={(e) => setShiftNote(e.target.value)}
                      placeholder="Ví dụ: Thiếu 20k tiền lẻ, đã nạp thêm..."
                    />
                 </div>
                 <div className="flex gap-3">
                    <button
                       onClick={() => setIsCloseShiftModalOpen(false)}
                       className="flex-1 bg-gray-100 text-gray-500 py-4 rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-gray-200 transition-all"
                    >
                       Hủy bỏ
                    </button>
                    <button
                       onClick={handleCloseShift}
                       className="flex-2 bg-orange-600 text-white py-4 px-8 rounded-2xl font-black text-lg hover:bg-orange-700 transition-all shadow-lg shadow-orange-500/20 active:scale-95 uppercase italic tracking-tighter"
                    >
                       Chốt ca
                    </button>
                 </div>
              </div>
           </div>
        </div>
      )}

      {/* SHIFT HISTORY MODAL */}
      {isShiftHistoryModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[600] flex justify-center items-center p-4 backdrop-blur-md">
           <div className="bg-white w-full max-w-2xl rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-gray-800 p-6 text-white flex justify-between items-center">
                 <div>
                    <h3 className="font-black text-xl uppercase italic tracking-tighter">Lịch sử ca của bạn</h3>
                    <p className="text-[10px] font-bold opacity-80 uppercase tracking-widest">Danh sách các phiên làm việc gần đây</p>
                 </div>
                 <button onClick={() => setIsShiftHistoryModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>
              <div className="p-6 max-h-[500px] overflow-y-auto no-scrollbar">
                 {loadingHistory ? (
                   <div className="flex justify-center py-10"><Loader2 className="animate-spin text-blue-600" /></div>
                 ) : shiftHistory.length === 0 ? (
                   <div className="text-center py-10 text-gray-400 font-bold uppercase text-[10px]">Chưa có lịch sử ca</div>
                 ) : (
                   <div className="space-y-3">
                      {shiftHistory.map((s: any) => (
                        <div key={s.id} className="bg-gray-50 p-4 rounded-2xl border border-gray-100 flex items-center justify-between">
                           <div>
                              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{new Date(s.startTime).toLocaleDateString('vi-VN')}</p>
                              <p className="text-xs font-black text-gray-700 mt-0.5">
                                 {new Date(s.startTime).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})} - {s.endTime ? new Date(s.endTime).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'}) : '...'}
                              </p>
                           </div>
                           <div className="text-right">
                              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Doanh thu</p>
                              <p className="text-sm font-black text-blue-600">{s.totalRevenue.toLocaleString()}đ</p>
                           </div>
                           <div className="text-right">
                              <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Chênh lệch</p>
                              <p className={`text-xs font-black ${s.endingCash - (s.startingCash + s.totalRevenue) === 0 ? 'text-green-500' : 'text-red-500'}`}>
                                 {s.status === 'Closed' ? `${(s.endingCash - (s.startingCash + s.totalRevenue)).toLocaleString()}đ` : 'Đang mở'}
                              </p>
                           </div>
                        </div>
                      ))}
                   </div>
                 )}
              </div>
              <div className="p-6 bg-gray-50 border-t text-center">
                 <p className="text-[10px] text-gray-400 font-medium italic">* Chỉ hiển thị các ca làm việc của bạn tại chi nhánh hiện tại.</p>
              </div>
           </div>
        </div>
      )}

      {/* INVOICE HISTORY MODAL */}
      {isInvoiceHistoryModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[600] flex justify-center items-center p-4 backdrop-blur-md">
           <div className="bg-white w-full max-w-4xl h-[80vh] rounded-[2.5rem] shadow-2xl overflow-hidden flex flex-col animate-in zoom-in-95 duration-200">
              <div className="bg-[#1e293b] p-6 text-white flex justify-between items-center">
                 <div className="flex items-center space-x-3">
                    <div className="p-3 bg-white/10 rounded-2xl">
                       <History size={24} />
                    </div>
                    <div>
                       <h3 className="font-black text-xl uppercase italic tracking-tighter">Lịch sử hóa đơn</h3>
                       <p className="text-[10px] font-bold opacity-60 uppercase tracking-widest">Tra cứu các giao dịch đã thực hiện</p>
                    </div>
                 </div>
                 <button onClick={() => setIsInvoiceHistoryModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <div className="p-6 bg-gray-50 border-b flex flex-col md:flex-row md:items-center gap-4">
                 <div className="relative flex-1">
                    <Search className="absolute left-4 top-3 text-gray-400" size={18} />
                    <input
                       type="text"
                       placeholder="Mã hóa đơn hoặc tên khách..."
                       className="w-full pl-12 pr-4 py-3 bg-white border border-gray-200 rounded-2xl text-sm font-bold outline-none focus:ring-2 focus:ring-blue-500/20"
                       value={invoiceSearch}
                       onChange={(e) => setInvoiceSearch(e.target.value)}
                    />
                 </div>

                 <div className="flex bg-white p-1 rounded-2xl border border-gray-200">
                    {[
                      { id: 'all', label: 'Tất cả' },
                      { id: 'Đang xử lý', label: 'Xử lý' },
                      { id: 'Đổi quà', label: 'Đổi quà' },
                      { id: 'Hoàn thành', label: 'Xong' },
                      { id: 'Đã hủy', label: 'Đã hủy' }
                    ].map((s) => (
                       <button
                          key={s.id}
                          onClick={() => setInvoiceStatusFilter(s.id)}
                          className={`px-4 py-2 rounded-xl text-[10px] font-black uppercase transition-all ${invoiceStatusFilter === s.id ? 'bg-blue-600 text-white shadow-md' : 'text-gray-400'}`}
                       >
                          {s.label}
                       </button>
                    ))}
                 </div>

                 <div className="flex items-center space-x-3 bg-white p-1 rounded-2xl border border-gray-200">
                    <div className="flex items-center px-3 border-r">
                       <span className="text-[9px] font-black text-gray-400 uppercase mr-2">Từ:</span>
                       <input
                          type="date"
                          className="text-[11px] font-bold text-gray-700 outline-none border-none bg-transparent"
                          value={invoiceFromDate}
                          onChange={(e) => setInvoiceFromDate(e.target.value)}
                       />
                    </div>
                    <div className="flex items-center px-3">
                       <span className="text-[9px] font-black text-gray-400 uppercase mr-2">Đến:</span>
                       <input
                          type="date"
                          className="text-[11px] font-bold text-gray-700 outline-none border-none bg-transparent"
                          value={invoiceToDate}
                          onChange={(e) => setInvoiceToDate(e.target.value)}
                       />
                    </div>
                 </div>
                 <button onClick={fetchInvoiceHistory} className="bg-white p-3 rounded-2xl border border-gray-200 hover:bg-gray-100 transition-all">
                    <RotateCcw size={20} className="text-gray-400" />
                 </button>
              </div>

              <div className="flex-1 overflow-auto p-6">
                 {loadingInvoices ? (
                    <div className="flex justify-center py-20"><Loader2 className="animate-spin text-blue-600" /></div>
                 ) : invoices.length === 0 ? (
                    <div className="text-center py-20 text-gray-400 font-bold uppercase text-xs tracking-widest">Không tìm thấy hóa đơn nào</div>
                 ) : (
                    <div className="space-y-4">
                       {invoices.map((inv: any) => (
                          <div
                             key={inv.id}
                             onClick={() => setSelectedHistoryOrder(inv)}
                             className="bg-white p-5 rounded-[2rem] border border-gray-100 shadow-sm hover:shadow-md cursor-pointer transition-all flex flex-col md:flex-row md:items-center justify-between gap-4 group"
                          >
                             <div className="flex items-center space-x-4">
                                <div className="p-3 bg-blue-50 rounded-2xl text-blue-600 group-hover:bg-blue-600 group-hover:text-white transition-colors">
                                   <ClipboardList size={24} />
                                </div>
                                <div>
                                   <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-0.5">
                                      {inv.invoiceCode}
                                      <span className={`ml-2 px-1.5 py-0.5 rounded text-[8px] font-black uppercase ${
                                         inv.status === 'Đã hủy' ? 'bg-red-100 text-red-600' :
                                         (inv.status === 'Hoàn thành' || (inv.totalAmount > 0 && inv.paidAmount >= inv.totalAmount)) ? 'bg-green-100 text-green-600' :
                                         'bg-blue-100 text-blue-600'
                                      }`}>
                                         {(inv.totalAmount > 0 && inv.paidAmount >= inv.totalAmount) ? 'Hoàn thành' : inv.status}
                                      </span>
                                   </p>
                                   <h4 className="font-black text-gray-800 text-sm uppercase tracking-tight">{inv.tableName || 'Mang về'}</h4>
                                   <p className="text-[11px] text-gray-500 font-medium">Khách: <span className="text-blue-600 font-bold">{inv.customerName}</span></p>
                                </div>
                             </div>

                             <div className="grid grid-cols-2 md:grid-cols-3 gap-8 flex-1 md:max-w-md text-right md:text-left">
                                <div>
                                   <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Thời gian</p>
                                   <p className="text-xs font-bold text-gray-700">{new Date(inv.paymentAt || inv.createdAt).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})}</p>
                                </div>
                                <div>
                                   <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Phương thức</p>
                                   <span className={`text-[10px] font-black px-2 py-0.5 rounded-full uppercase ${inv.paymentMethod === 'Tiền mặt' ? 'bg-orange-100 text-orange-600' : 'bg-purple-100 text-purple-600'}`}>
                                      {inv.paymentMethod || 'Chưa thanh toán'}
                                   </span>
                                </div>
                                <div>
                                   <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Tổng tiền</p>
                                   <p className="text-sm font-black text-blue-700 tracking-tighter">{inv.totalAmount.toLocaleString()}đ</p>
                                </div>
                             </div>

                             <div className="flex items-center space-x-2">
                                <button onClick={() => { setSelectedHistoryOrder(inv); setTimeout(() => window.print(), 0); }} title="In hóa đơn" aria-label="In hóa đơn" className="p-2.5 bg-gray-50 text-gray-400 hover:text-blue-600 rounded-xl transition-all">
                                   <Printer size={18} />
                                </button>
                                <div className="p-2.5 bg-blue-600 text-white rounded-xl shadow-lg shadow-blue-500/20 group-hover:scale-110 transition-all">
                                   <ChevronRight size={18} />
                                </div>
                             </div>
                          </div>
                       ))}
                    </div>
                 )}
              </div>

              {/* DETAILED VIEW OVERLAY */}
              {selectedHistoryOrder && (
                 <div className="absolute inset-0 bg-white z-[650] flex flex-col animate-in slide-in-from-right duration-300">
                    <div className="bg-[#1e293b] p-6 text-white flex justify-between items-center print-hide">
                       <button onClick={() => setSelectedHistoryOrder(null)} className="flex items-center text-xs font-bold uppercase tracking-widest opacity-60 hover:opacity-100">
                          <X size={18} className="mr-2" /> Quay lại danh sách
                       </button>
                       <div className="text-right">
                          <h3 className="font-semibold text-lg tracking-tight">Chi tiết hóa đơn</h3>
                          <p className="mt-1 text-[10px] font-medium text-white/60">{selectedHistoryOrder.invoiceCode}</p>
                       </div>
                       <button onClick={() => window.print()} title="In hóa đơn" aria-label="In hóa đơn" className="bg-white/10 p-2 rounded-xl"><Printer size={20}/></button>
                    </div>

                    <div id="invoice-print" className="flex-1 overflow-auto p-8 flex flex-col items-center">
                       <div className="bg-white w-full max-w-2xl print:max-w-none">
                          <div className="grid grid-cols-2 gap-12 mb-10 pb-10 border-b border-gray-100">
                             <div className="space-y-4">
                                <div>
                                   <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Thông tin khách hàng</p>
                                   <p className="text-lg font-black text-gray-800 uppercase italic">{selectedHistoryOrder.customerName}</p>
                                   {selectedHistoryOrder.customerPhone && (
                                      <p className="text-xs font-bold text-blue-600 mt-1">{selectedHistoryOrder.customerPhone}</p>
                                   )}
                                </div>
                                <div className="flex space-x-8">
                                   <div>
                                      <p className="text-[10px] font-semibold text-gray-400 uppercase tracking-widest mb-1">Lập hóa đơn</p>
                                      <p className="text-sm font-bold text-gray-700">{new Date(selectedHistoryOrder.paymentAt || selectedHistoryOrder.createdAt).toLocaleString('vi-VN')}</p>
                                   </div>
                                   <div>
                                      <p className="text-[10px] font-semibold text-gray-400 uppercase tracking-widest mb-1">Thanh toán</p>
                                      <span className="text-xs font-semibold text-blue-600">{selectedHistoryOrder.paymentMethod || 'Chưa thanh toán'}</span>
                                   </div>
                                </div>
                             </div>
                             <div className="text-right space-y-4">
                                <div>
                                   <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Bàn / Vị trí</p>
                                   <p className="text-lg font-black text-blue-600 uppercase italic">{selectedHistoryOrder.tableName || 'Mang về'}</p>
                                </div>
                                <div>
                                   <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Nhân viên thực hiện</p>
                                   <p className="text-sm font-bold text-gray-700">{selectedHistoryOrder.createdBy || '---'}</p>
                                </div>
                             </div>
                          </div>

                          {orderLoyaltyInfo && (
                             <div className="bg-orange-50/50 rounded-[2rem] p-6 mb-8 border border-orange-100/50 flex justify-between items-center shadow-sm print:rounded-none print:border-dashed">
                                <div className="flex items-center">
                                   <div className="p-3 bg-orange-100 rounded-2xl text-orange-600 mr-4 print:hidden">
                                      <Star size={20} className="fill-orange-600" />
                                   </div>
                                   <div>
                                      <p className="text-[10px] font-black text-orange-600/60 uppercase tracking-widest">Hội viên tích điểm</p>
                                      <p className="text-sm font-black text-gray-800 uppercase tracking-tight">Thành viên thân thiết</p>
                                   </div>
                                </div>
                                <div className="flex space-x-12">
                                   {orderLoyaltyInfo.pointsRedeemed > 0 && (
                                      <div className="text-right">
                                         <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Điểm đã tiêu</p>
                                         <p className="text-sm font-black text-red-600">-{orderLoyaltyInfo.pointsRedeemed} PTS</p>
                                      </div>
                                   )}
                                   {orderLoyaltyInfo.pointsEarned > 0 && (
                                      <div className="text-right">
                                         <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Điểm tích mới</p>
                                         <p className="text-sm font-black text-green-600">+{orderLoyaltyInfo.pointsEarned} PTS</p>
                                      </div>
                                   )}
                                   <div className="text-right">
                                      <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-1">Số dư hiện tại</p>
                                      <p className="text-sm font-black text-blue-700">{orderLoyaltyInfo.balanceAfter} PTS</p>
                                   </div>
                                </div>
                             </div>
                          )}

                          <div className="bg-gray-50 rounded-[3rem] p-8 border border-gray-100 shadow-inner print:bg-white print:rounded-none print:border-none print:shadow-none">
                             <table className="w-full">
                                <thead>
                                   <tr className="border-b-2 border-gray-200">
                                      <th className="text-left pb-4 text-[11px] font-black text-gray-400 uppercase">Mặt hàng</th>
                                      <th className="text-center pb-4 text-[11px] font-black text-gray-400 uppercase">SL</th>
                                      <th className="text-right pb-4 text-[11px] font-black text-gray-400 uppercase">Đơn giá</th>
                                      <th className="text-right pb-4 text-[11px] font-black text-gray-400 uppercase">Thành tiền</th>
                                   </tr>
                                </thead>
                                <tbody className="divide-y divide-gray-100">
                                   {selectedHistoryOrder.details?.map((d: any, i: number) => (
                                      <tr key={i}>
                                         <td className="py-5 text-sm font-black text-gray-800 capitalize">{d.productName}</td>
                                         <td className="py-5 text-center text-sm font-black text-blue-600">{d.quantity}</td>
                                         <td className="py-5 text-right text-xs font-bold text-gray-400">{d.unitPrice.toLocaleString()}đ</td>
                                         <td className="py-5 text-right text-sm font-black text-gray-800">{(d.quantity * d.unitPrice).toLocaleString()}đ</td>
                                      </tr>
                                   ))}
                                </tbody>
                             </table>

                             <div className="mt-8 pt-8 border-t-2 border-dashed border-gray-200 space-y-3">
                                <div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]">
                                   <span>Tạm tính</span>
                                   <span className="text-gray-700">{(selectedHistoryOrder.subTotal ?? (selectedHistoryOrder.totalAmount + (selectedHistoryOrder.discount || 0))).toLocaleString()}đ</span>
                                </div>
                                {selectedHistoryOrder.serviceFeeAmount != null && selectedHistoryOrder.serviceFeeAmount > 0 && (
                                  <div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]">
                                    <span>Phí phục vụ ({selectedHistoryOrder.serviceFeePercent || 0}%)</span>
                                    <span className="text-gray-700">{selectedHistoryOrder.serviceFeeAmount.toLocaleString()}đ</span>
                                  </div>
                                )}
                                {selectedHistoryOrder.vatAmount != null && selectedHistoryOrder.vatAmount > 0 && (
                                  <div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]">
                                    <span>VAT ({selectedHistoryOrder.vatPercent || 0}%)</span>
                                    <span className="text-gray-700">{selectedHistoryOrder.vatAmount.toLocaleString()}đ</span>
                                  </div>
                                )}
                                <div className="flex justify-between items-center text-gray-500 font-bold uppercase text-[10px]">
                                   <span>Giảm giá khuyến mãi</span>
                                   <span className="text-red-500">-{(selectedHistoryOrder.discount || 0).toLocaleString()}đ</span>
                                </div>
                                <div className="flex justify-between items-center pt-4">
                                   <span className="text-blue-600 font-black text-sm uppercase italic tracking-widest">Khách đã thanh toán</span>
                                   <span className="text-3xl font-black text-blue-700 tracking-tighter">{(selectedHistoryOrder.paidAmount || 0).toLocaleString()}đ</span>
                                </div>
                             </div>
                          </div>
                       </div>
                    </div>

                    <div className="p-8 bg-gray-100 border-t flex justify-end print-hide">
                       <button onClick={() => setSelectedHistoryOrder(null)} className="px-12 py-4 bg-white border-2 border-gray-200 rounded-[2rem] font-black text-gray-400 text-[10px] uppercase tracking-[0.2em] hover:bg-gray-50 active:scale-95 transition-all">Đóng chi tiết</button>
                    </div>
                 </div>
              )}

              <div className="p-6 bg-gray-50 border-t text-center">
                 <p className="text-[10px] text-gray-400 font-medium italic">* Chỉ hiển thị hóa đơn thuộc chi nhánh bạn đang làm việc.</p>
              </div>
           </div>
        </div>
      )}

      {/* OPTIONS MODAL (SIZE & TOPPING) */}
      {isOptionsModalOpen && currentCustomizingProduct && (
        <div className="fixed inset-0 bg-black/80 z-[500] flex items-center justify-center p-4 backdrop-blur-md">
           <div className="bg-white w-full max-w-lg rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
                 <div>
                    <h3 className="font-black text-xl uppercase italic tracking-tighter">{currentCustomizingProduct.name}</h3>
                    <p className="text-[10px] font-bold opacity-80 uppercase tracking-widest mt-1">Tùy chọn kích cỡ & Topping</p>
                 </div>
                 <button onClick={() => setIsOptionsModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <div className="p-8 space-y-8 max-h-[60vh] overflow-y-auto no-scrollbar">
                 {/* Size Selection */}
                 {currentCustomizingProduct.sizesJson && JSON.parse(currentCustomizingProduct.sizesJson).length > 0 && (
                    <div className="space-y-4">
                       <label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] block ml-1">Chọn kích cỡ (Size)</label>
                       <div className="grid grid-cols-3 gap-3">
                          {JSON.parse(currentCustomizingProduct.sizesJson).map((s: any, i: number) => (
                             <button
                                key={i}
                                onClick={() => setSelectedSize(s)}
                                className={`py-4 px-2 rounded-2xl border-2 transition-all flex flex-col items-center ${
                                   selectedSize?.name === s.name
                                   ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100'
                                   : 'border-gray-100 bg-gray-50/50 hover:border-blue-200'
                                }`}
                             >
                                <span className={`text-sm font-black uppercase ${selectedSize?.name === s.name ? 'text-blue-600' : 'text-gray-500'}`}>{s.name}</span>
                                <span className="text-[10px] font-bold text-gray-400 mt-1">+{s.price.toLocaleString()}đ</span>
                             </button>
                          ))}
                       </div>
                    </div>
                 )}

                 {/* Topping Selection */}
                 {currentCustomizingProduct.toppingsJson && JSON.parse(currentCustomizingProduct.toppingsJson).length > 0 && (
                    <div className="space-y-4">
                       <label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] block ml-1">Thêm Topping (Tùy chọn)</label>
                       <div className="grid grid-cols-2 gap-3">
                          {JSON.parse(currentCustomizingProduct.toppingsJson).map((t: any, i: number) => (
                             <button
                                key={i}
                                onClick={() => {
                                   const isSelected = selectedToppings.find(x => x.name === t.name);
                                   if (isSelected) {
                                      setSelectedToppings(selectedToppings.filter(x => x.name !== t.name));
                                   } else {
                                      setSelectedToppings([...selectedToppings, t]);
                                   }
                                }}
                                className={`py-4 px-5 rounded-2xl border-2 transition-all flex items-center justify-between ${
                                   selectedToppings.find(x => x.name === t.name)
                                   ? 'border-blue-600 bg-blue-50 ring-4 ring-blue-100'
                                   : 'border-gray-100 bg-gray-50/50 hover:border-blue-200'
                                }`}
                             >
                                <div className="text-left">
                                   <p className={`text-xs font-black uppercase tracking-tight ${selectedToppings.find(x => x.name === t.name) ? 'text-blue-700' : 'text-gray-700'}`}>{t.name}</p>
                                   <p className="text-[10px] font-bold text-gray-400 mt-0.5">+{t.price.toLocaleString()}đ</p>
                                </div>
                                {selectedToppings.find(x => x.name === t.name) && <CheckCircle2 size={18} className="text-blue-600" />}
                             </button>
                          ))}
                       </div>
                    </div>
                 )}
              </div>

              <div className="p-8 bg-gray-50 border-t flex items-center justify-between">
                 <div>
                    <p className="text-[10px] font-bold text-gray-400 uppercase tracking-widest mb-1">Đơn giá món</p>
                    <p className="text-3xl font-black text-blue-700 tracking-tighter">
                       {(currentCustomizingProduct.price + (selectedSize?.price || 0) + selectedToppings.reduce((s, t) => s + t.price, 0)).toLocaleString()}đ
                    </p>
                 </div>
                 <div className="flex space-x-3">
                    <button onClick={() => setIsOptionsModalOpen(false)} className="px-8 py-4 bg-white border-2 border-gray-200 rounded-2xl font-black text-gray-400 text-xs uppercase tracking-widest hover:bg-gray-100 transition-all">Bỏ qua</button>
                    <button
                       onClick={handleConfirmOptions}
                       className="px-12 py-4 bg-blue-600 text-white rounded-2xl font-black text-xs uppercase tracking-widest shadow-xl shadow-blue-500/20 hover:bg-blue-700 transition-all active:scale-95"
                    >
                       XÁC NHẬN CHỌN
                    </button>
                 </div>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default POSPage;

