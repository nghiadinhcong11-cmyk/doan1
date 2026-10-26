import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import { useRef } from 'react';
import { Loader2 } from 'lucide-react';
import { API_URL } from '../../../config';
import type { ReceiptSettingsModel } from '../../../components/ReceiptPreview';
import { notifyFeedback } from '../../../components/ui';
import { PENDING_ORDER_CHANGED_EVENT } from '../../notifications/pendingOrderQuery';
import { fetchBranchList } from '../../../api/branchQuery';
import { getCachedJson } from '../../../api/queryCache';
import { classifyOrder } from '../../orders/orderClassification';
import POSCartItems from '../components/POSCartItems';
import POSCategoryTabs from '../components/POSCategoryTabs';
import POSOrderActions from '../components/POSOrderActions';
import POSProductGrid from '../components/POSProductGrid';
import POSNotificationToast from '../components/POSNotificationToast';
import POSAcceptOrderModal, { POSPendingOrder } from '../components/POSAcceptOrderModal';
import POSTransferTableModal from '../components/POSTransferTableModal';
import POSOpenShiftModal from '../components/POSOpenShiftModal';
import POSCloseShiftModal from '../components/POSCloseShiftModal';
import POSShiftHistoryModal from '../components/POSShiftHistoryModal';
import POSInvoiceHistoryModal, { POSInvoiceRecord, POSOrderLoyaltyInfo } from '../components/POSInvoiceHistoryModal';
import POSProductOptionsModal from '../components/POSProductOptionsModal';
import POSTableMap from '../components/POSTableMap';
import POSPendingRequests from '../components/POSPendingRequests';
import POSPaymentModal from '../components/POSPaymentModal';
import POSHeader from '../components/POSHeader';
import POSOrderHeader from '../components/POSOrderHeader';
import usePOSShift from '../hooks/usePOSShift';
import usePOSInvoiceHistory from '../hooks/usePOSInvoiceHistory';
import type { Branch, CartItem, POSUserRole, Product, Table } from '../types';
import { calculateCartPreviewTotal, filterProducts } from '../utils';

interface POSFinancialSnapshot {
  orderId: string;
  branchId: string;
  tableId: string;
  cartRevision: number;
  subTotal?: number | null;
  serviceFeeAmount?: number | null;
  serviceFeePercent?: number | null;
  vatAmount?: number | null;
  vatPercent?: number | null;
  discount?: number | null;
  totalAmount: number;
}

const POSPage = ({ onLogout, userName, userRole, userPosition }: { onLogout: () => void, userName?: string, userRole?: POSUserRole, userPosition?: string }) => {
  const {
    activeShift,
    isShiftModalOpen,
    isCloseShiftModalOpen,
    isShiftHistoryModalOpen,
    startingCash,
    endingCash,
    shiftNote,
    shiftHistory,
    loadingHistory,
    setIsShiftModalOpen,
    setIsCloseShiftModalOpen,
    setIsShiftHistoryModalOpen,
    setStartingCash,
    setEndingCash,
    setShiftNote,
    checkActiveShift,
    handleOpenShift,
    handleCloseShift
  } = usePOSShift({ userRole, userName, onLogout });

  const [posTab, setPosTab] = useState('menu');
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [products, setProducts] = useState<Product[]>([]);
  const [tables, setTables] = useState<Table[]>([]);
  const [categories, setCategories] = useState<string[]>(['Tất cả']);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [isPaymentModalOpen, setIsPaymentModalOpen] = useState(false);
  const [isTransferTableModalOpen, setIsTransferTableModalOpen] = useState(false);
  const [isPreparingPayment, setIsPreparingPayment] = useState(false);
  const [paymentPreparationError, setPaymentPreparationError] = useState<string | null>(null);
  const [kitchenMessage, setKitchenMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [paymentMethod, setPaymentMethod] = useState<'Tiền mặt' | 'Chuyển khoản'>('Tiền mặt');
  const [branchPaymentInfoError, setBranchPaymentInfoError] = useState<string | null>(null);
  const [qrImageError, setQrImageError] = useState(false);
  const [customerPhone, setCustomerPhone] = useState('');
  const [customerName, setCustomerName] = useState('Khách lẻ');
  const [customerEmail, setCustomerEmail] = useState('');
  const [isCheckingPhone, setIsCheckingPhone] = useState(false);
  const [branchInfo, setBranchInfo] = useState<Branch | null>(null);
  const [allBranches, setAllBranches] = useState<Branch[]>([]);
  const activeBranchIdRef = useRef<string | null>(localStorage.getItem('selectedBranchId'));
  const branchGenerationRef = useRef(0);
  const tablesAbortControllerRef = useRef<AbortController | null>(null);
  const pendingOrdersAbortControllerRef = useRef<AbortController | null>(null);
  const paymentInfoAbortControllerRef = useRef<AbortController | null>(null);
  const isPaymentModalOpenRef = useRef(false);
  const isSavingRef = useRef(false);
  const saveRequestInFlightRef = useRef(false);
  const operationalRefreshInFlightRef = useRef(false);
  const operationalRefreshQueuedRef = useRef(false);
  const operationalRefreshOwnerActiveRef = useRef(true);
  const [pendingOrders, setPendingOrders] = useState<any[]>([]);
  const [pendingOrdersError, setPendingOrdersError] = useState<string | null>(null);
  const [pendingReservations, setPendingReservations] = useState<any[]>([]);
  const [lastOrderCount, setLastOrderCount] = useState(0);
  const [lastResCount, setLastResCount] = useState(0);
  const [showNotification, setShowNotification] = useState(false);
  const [notifType, setNotifType] = useState<'order' | 'res'>('order');
  const [orderToAccept, setOrderToAccept] = useState<any>(null);
  const [isSignalRConnected, setIsSignalRConnected] = useState(false);
  const [refreshError, setRefreshError] = useState<string | null>(null);

  const [selectedHistoryOrder, setSelectedHistoryOrder] = useState<any>(null);
  const [receiptSettings, setReceiptSettings] = useState<ReceiptSettingsModel>({ showLogo: true, showAddress: true, showPhone: true, showStaff: true, showPaymentMethod: true, showOrderNote: true, showThankYou: true, thankYouText: 'Cảm ơn quý khách và hẹn gặp lại!', paperWidth: 80, fontSize: 11 });
  const [orderLoyaltyInfo, setOrderLoyaltyInfo] = useState<{ pointsEarned: number, pointsRedeemed: number, balanceAfter: number } | null>(null);

  const handlePrintInvoice = (invoice?: POSInvoiceRecord) => {
    if (invoice) {
      setSelectedHistoryOrder(invoice);
      setTimeout(() => window.print(), 0);
      return;
    }
    window.print();
  };

  const isCurrentBranchRequest = (branchId: string | null, generation: number) =>
    generation === branchGenerationRef.current && branchId === activeBranchIdRef.current;

  const {
    isInvoiceHistoryModalOpen,
    invoices,
    loadingInvoices,
    invoiceSearch,
    invoiceFromDate,
    invoiceToDate,
    invoiceStatusFilter,
    setIsInvoiceHistoryModalOpen,
    setInvoiceSearch,
    setInvoiceFromDate,
    setInvoiceToDate,
    setInvoiceStatusFilter,
    fetchInvoiceHistory,
    resetForBranchChange: resetInvoiceHistoryForBranchChange
  } = usePOSInvoiceHistory({ branchGenerationRef, isCurrentBranchRequest });

  useEffect(() => {
    if (!selectedHistoryOrder?.branchId) return;
    getCachedJson(`${API_URL}/api/ReceiptSettings?branchId=${selectedHistoryOrder.branchId}&documentType=Invoice`, {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('adminToken') || ''}` }
    })
      .then(data => data && setReceiptSettings(prev => ({ ...prev, ...data })))
      .catch(() => undefined);
  }, [selectedHistoryOrder?.branchId]);

  useEffect(() => {
    const fetchLoyalty = async () => {
       if (!selectedHistoryOrder?.id) {
          setOrderLoyaltyInfo(null);
          return;
       }
       try {
          const response = await fetch(`${API_URL}/api/Order/${selectedHistoryOrder.id}/loyalty`, {
             headers: { 'Authorization': `Bearer ${localStorage.getItem('adminToken')}` }
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
  const [financialSnapshots, setFinancialSnapshots] = useState<Record<string, POSFinancialSnapshot>>({});
  const cartRevisionsRef = useRef<Record<string, number>>({});

  const [tableCustomers, setTableCustomers] = useState<Record<string, { id?: string, phone: string, name: string, email?: string, loyaltyPoints?: number, lookupStatus?: 'found' | 'not-found' | 'error', lookupError?: string, memberConsent?: boolean }>>(() => {
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

  const fetchProducts = async (force = false, requestedBranchId = localStorage.getItem('selectedBranchId'), requestGeneration = branchGenerationRef.current) => {
    try {
      const posQuery = new URLSearchParams({ sellableOnly: 'true' });
      if (requestedBranchId) posQuery.set('branchId', requestedBranchId);
      const branchQuery = `?${posQuery.toString()}`;
      const [productsData, toppingsData] = await Promise.all([
        getCachedJson<Product[]>(`${API_URL}/api/Product${branchQuery}`, undefined, { force }),
        getCachedJson<any[]>(`${API_URL}/api/Topping${requestedBranchId ? `?branchId=${encodeURIComponent(requestedBranchId)}&sellableOnly=true` : ''}`, undefined, { force })
      ]);

      if (!isCurrentBranchRequest(requestedBranchId, requestGeneration)) return;

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
      setRefreshError(null);
    } catch (err) {
      console.error('Error fetching products:', err);
      if (requestGeneration === branchGenerationRef.current) setRefreshError('Không thể làm mới danh sách món.');
    }
  };

  const fetchTables = async (branchIdOverride?: string) => {
    const branchId = branchIdOverride || branchInfo?.id || null;
    const generation = branchGenerationRef.current;
    if (!branchId) return;
    const controller = new AbortController();
    tablesAbortControllerRef.current?.abort();
    tablesAbortControllerRef.current = controller;
    try {
      const response = await fetch(`${API_URL}/api/Table?isActive=true&branchId=${branchId}`, { signal: controller.signal });
      const data: Table[] = await response.json();
      if (!isCurrentBranchRequest(branchId, generation)) return;
      const physicalTables = data.filter(table => table.name !== 'Mang về');
      setTables(physicalTables);
      return physicalTables;
    } catch (err) {
      if (!(err instanceof DOMException && err.name === 'AbortError')) {
        console.error('Error fetching tables:', err);
      }
    } finally {
      if (tablesAbortControllerRef.current === controller) tablesAbortControllerRef.current = null;
    }
  };

  const fetchBranchInfo = async () => {
    const requestGeneration = branchGenerationRef.current;
    try {
      const data = await fetchBranchList();

      if (requestGeneration !== branchGenerationRef.current) return;

      if (Array.isArray(data)) {
        setAllBranches(data as Branch[]);
        const savedBranchId = localStorage.getItem('selectedBranchId');
        const selected = data.find(b => b.id === savedBranchId) || data.find(b => b.isMain) || data[0];

        if (selected) {
          localStorage.setItem('selectedBranchId', selected.id);
          activeBranchIdRef.current = selected.id;
          await loadBranchPaymentInfo(selected);
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
    branchGenerationRef.current += 1;
    activeBranchIdRef.current = branch.id;
    const requestGeneration = branchGenerationRef.current;
    tablesAbortControllerRef.current?.abort();
    pendingOrdersAbortControllerRef.current?.abort();
    paymentInfoAbortControllerRef.current?.abort();
    resetInvoiceHistoryForBranchChange();
    setBranchInfo(branch);
    localStorage.setItem('selectedBranchId', branch.id);
    setPendingOrders([]);
    setPendingReservations([]);
    setPendingOrdersError(null);
    setLastOrderCount(0);
    setLastResCount(0);
    setActiveCategory('Tất cả');
    setSearchTerm('');
    setSelectedTableId('delivery');
    setTableCarts({ 'delivery': [] }); // Reset carts
    setActiveOrderIds({});
    setFinancialSnapshots({});
    cartRevisionsRef.current = {};
    setIsPaymentModalOpen(false);
    setIsPreparingPayment(false);
    setPaymentPreparationError(null);
    setBranchPaymentInfoError(null);
    setQrImageError(false);
    void fetchProducts(true, branch.id, requestGeneration);
    void (async () => {
      const nextTables = await fetchTables(branch.id);
      await fetchPendingOrders(nextTables || []);
    })();
    void loadBranchPaymentInfo(branch);
    if (isInvoiceHistoryModalOpen) void fetchInvoiceHistory();
  };

  const fetchPendingOrders = async (tableSnapshot = tables) => {
    const branchId = localStorage.getItem('selectedBranchId');
    const generation = branchGenerationRef.current;
    const controller = new AbortController();
    pendingOrdersAbortControllerRef.current?.abort();
    pendingOrdersAbortControllerRef.current = controller;
    try {
      // Lấy đơn hàng mới & yêu cầu đổi quà
      // LƯU Ý: Chỉ lấy đơn Web (vãng lai/Mang về) chưa được gán vào bàn cụ thể trong POS
      const orderRes = await fetch(`${API_URL}/api/Order/pending-summary${branchId ? `?branchId=${branchId}` : ''}`, { signal: controller.signal });
      if (!orderRes.ok) {
        setPendingOrdersError('Không thể tải danh sách đơn đang chờ xử lý.');
        throw new Error(`Failed to fetch orders (${orderRes.status})`);
      }
      setPendingOrdersError(null);
      const data = await orderRes.json();
      if (!isCurrentBranchRequest(branchId, generation)) return;

      if (Array.isArray(data)) {
        // Pending/unaccepted is determined by the server-backed ownership
        // fields. A takeaway or legacy table label alone must not make an
        // already accepted order appear as a new web order.
        const physicalTableNames = new Set(tableSnapshot.map(t => t.name));
        const webOrders = data.filter((o: any) =>
          classifyOrder(o, physicalTableNames).isAwaitingStaffAction
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
      const resRes = await fetch(`${API_URL}/api/Reservation?status=Pending${branchId ? `&branchId=${branchId}` : ''}`, { signal: controller.signal });
      if (!resRes.ok) throw new Error('Failed to fetch reservations');
      const reservations = await resRes.json();
      if (!isCurrentBranchRequest(branchId, generation)) return;

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
      if (!(err instanceof DOMException && err.name === 'AbortError')) {
        console.error("Lỗi lấy dữ liệu thông báo:", err);
      }
    } finally {
      if (pendingOrdersAbortControllerRef.current === controller) pendingOrdersAbortControllerRef.current = null;
    }
  };

  const playNotifSound = () => {
    const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
    audio.play().catch(e => console.log("Audio play failed:", e));
  };

  const refreshOperationalData = async (allowTrailing = false) => {
    if (!operationalRefreshOwnerActiveRef.current) return;
    if (operationalRefreshInFlightRef.current) {
      if (allowTrailing) operationalRefreshQueuedRef.current = true;
      return;
    }
    operationalRefreshInFlightRef.current = true;
    try {
      // Không làm mới bàn trong lúc thanh toán hoặc lưu đơn để tránh ảnh hưởng dữ liệu đang nhập.
      if (!isPaymentModalOpenRef.current && !isSavingRef.current) await fetchTables();
    } catch (e) {
      console.warn("Mất kết nối với máy chủ, đang thử lại...");
    } finally {
      operationalRefreshInFlightRef.current = false;
      if (operationalRefreshQueuedRef.current && operationalRefreshOwnerActiveRef.current && document.visibilityState === 'visible') {
        operationalRefreshQueuedRef.current = false;
        void refreshOperationalData(true);
      } else {
        operationalRefreshQueuedRef.current = false;
      }
    }
  };

  useEffect(() => {
    operationalRefreshOwnerActiveRef.current = true;
    const loadData = async () => {
      setLoading(true);
      await Promise.all([fetchProducts(), fetchBranchInfo(), fetchPendingOrders(), checkActiveShift()]);
      setLoading(false);
    };
    void loadData();

    let disposed = false;
    const refreshIfVisible = (allowTrailing = false) => {
      if (!disposed && document.visibilityState === 'visible') void refreshOperationalData(allowTrailing);
    };
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') refreshIfVisible(true);
    };
    const handlePriceUpdated = () => { void fetchProducts(); };
    const handlePendingOrderChanged = () => {
      if (document.visibilityState === 'visible') void fetchPendingOrders();
    };
    window.addEventListener('product-price-updated', handlePriceUpdated);
    window.addEventListener(PENDING_ORDER_CHANGED_EVENT, handlePendingOrderChanged);
    // SignalR handles immediate kitchen updates; this polling remains a low-frequency
    // reconciliation for pending web orders/reservations and missed events.
    const interval = window.setInterval(() => refreshIfVisible(false), 30000);
    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => {
      disposed = true;
      operationalRefreshOwnerActiveRef.current = false;
      window.clearInterval(interval);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
      window.removeEventListener('product-price-updated', handlePriceUpdated);
      window.removeEventListener(PENDING_ORDER_CHANGED_EVENT, handlePendingOrderChanged);
      operationalRefreshQueuedRef.current = false;
    };
  }, []);

  useEffect(() => {
    if (branchInfo?.id) {
      fetchTables();
    }
  }, [branchInfo?.id]);

  const currentCart = tableCarts[selectedTableId] || [];
  const previewTotal = calculateCartPreviewTotal(currentCart);
  const currentCartRevision = cartRevisionsRef.current[selectedTableId] || 0;
  const currentSnapshot = financialSnapshots[selectedTableId];
  const hasAuthoritativeSnapshot = Boolean(
    currentSnapshot &&
    currentSnapshot.branchId === (branchInfo?.id || '') &&
    currentSnapshot.tableId === selectedTableId &&
    currentSnapshot.cartRevision === currentCartRevision &&
    (!activeOrderIds[selectedTableId] || activeOrderIds[selectedTableId] === currentSnapshot.orderId)
  );
  const totalAmount = hasAuthoritativeSnapshot && currentSnapshot
    ? currentSnapshot.totalAmount
    : previewTotal;

  const getCartRevision = (tableId: string) => cartRevisionsRef.current[tableId] || 0;

  const parseFinancialSnapshot = (
    order: any,
    tableId: string,
    branchId: string,
    cartRevision: number
  ): POSFinancialSnapshot | null => {
    const parsedTotal = Number(order?.totalAmount);
    if (!order?.id || !order?.branchId || !branchId || String(order.branchId) !== branchId || !Number.isFinite(parsedTotal)) return null;
    const optionalNumber = (value: unknown) => {
      if (value == null) return null;
      const parsed = Number(value);
      return Number.isFinite(parsed) ? parsed : null;
    };

    return {
      orderId: String(order.id),
      branchId,
      tableId,
      cartRevision,
      subTotal: optionalNumber(order.subTotal),
      serviceFeeAmount: optionalNumber(order.serviceFeeAmount),
      serviceFeePercent: optionalNumber(order.serviceFeePercent),
      vatAmount: optionalNumber(order.vatAmount),
      vatPercent: optionalNumber(order.vatPercent),
      discount: optionalNumber(order.discount),
      totalAmount: parsedTotal
    };
  };

  const setFinancialSnapshotIfCurrent = (
    order: any,
    tableId: string,
    branchId: string,
    cartRevision: number,
    requestGeneration = branchGenerationRef.current
  ) => {
    const snapshot = parseFinancialSnapshot(order, tableId, branchId, cartRevision);
    if (!snapshot || requestGeneration !== branchGenerationRef.current || activeBranchIdRef.current !== branchId || getCartRevision(tableId) !== cartRevision) return null;

    setFinancialSnapshots(previous => ({ ...previous, [tableId]: snapshot }));
    setActiveOrderIds(previous => ({ ...previous, [tableId]: snapshot.orderId }));
    return snapshot;
  };

  const invalidateAuthoritativeTotal = (tableId = selectedTableId) => {
    cartRevisionsRef.current[tableId] = getCartRevision(tableId) + 1;
    setFinancialSnapshots(previous => {
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
            invalidateAuthoritativeTotal(table.id);
            await updateTableStatus(table.id, 'Trống');
          }
        }
        // Phát sự kiện để Navbar cập nhật lại số lượng chuông thông báo ngay lập tức
        window.dispatchEvent(new CustomEvent('refresh-notifications'));
        window.dispatchEvent(new Event(PENDING_ORDER_CHANGED_EVENT));
        await fetchPendingOrders();
      } else {
        const errorData = await response.json().catch(() => ({}));
        notifyFeedback(`Lỗi khi hủy đơn hàng: ${errorData.message || response.statusText}`);
      }
    } catch (err) {
      console.error('Lỗi khi hủy đơn hàng:', err);
      notifyFeedback('Không thể kết nối đến máy chủ để hủy đơn hàng.');
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

  const loadKitchenStatuses = async (orderId: string, tableId: string, requestGeneration = branchGenerationRef.current, isOwnerActive: () => boolean = () => true) => {
    try {
      const response = await fetch(`${API_URL}/api/Order/${orderId}/kitchen-status`);
      if (!response.ok) return;
      const requests = await response.json();
      if (!isOwnerActive() || requestGeneration !== branchGenerationRef.current) return;
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

  const handleSelectTable = async (tableId: string, forceFetch = false, skipSave = false) => {
    // TRƯỚC KHI ĐỔI BÀN: Lưu đơn của bàn hiện tại nếu có món
    if (!skipSave && selectedTableId && tableCarts[selectedTableId]?.length > 0) {
       handleSaveOrder(true);
    }

    setSelectedTableId(tableId);
    setPosTab('menu');

    const table = tables.find(t => t.id === tableId);
    if (!table || tableId === 'delivery') return;

    // Nếu bàn đang có khách hoặc ép buộc nạp (từ thông báo), hãy tải đơn hàng từ server về để đồng bộ
    if (table.status === 'Có khách' || forceFetch) {
      const fetchOrderDetails = async (retryCount = 0) => {
        const requestGeneration = branchGenerationRef.current;
        const requestRevision = getCartRevision(tableId);
        try {
          const branchId = branchInfo?.id || localStorage.getItem('selectedBranchId');
          if (!isCurrentBranchRequest(branchId || null, requestGeneration)) return false;
          // Thêm timestamp để tránh bị cache và nạp lại món tin cậy hơn
          const response = await fetch(`${API_URL}/api/Order?status=Đang xử lý&branchId=${branchId || ''}&tableName=${encodeURIComponent(table.name)}&_t=${Date.now()}`);

          if (response.ok) {
            const orders = await response.json();
            if (!isCurrentBranchRequest(branchId || null, requestGeneration)) return false;
            if (getCartRevision(tableId) !== requestRevision) return false;
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
                setFinancialSnapshotIfCurrent(activeOrder, tableId, branchId || '', requestRevision, requestGeneration);
               setTableCustomers(prev => ({
                  ...prev,
                  [tableId]: {
                     phone: activeOrder.customerPhone || '',
                     name: activeOrder.customerName || 'Khách lẻ',
                     email: activeOrder.customerEmail || ''
                  }
               }));
               void loadKitchenStatuses(activeOrder.id, tableId, requestGeneration);

               // Nếu đang forceFetch mà thấy có đơn, cập nhật luôn trạng thái bàn locally
               if (table.status === 'Trống') {
                  updateTableStatus(tableId, 'Có khách');
               }
               return true;
            }
          }
        } catch (err) {
          if (err instanceof DOMException && err.name === 'AbortError') return false;
          console.warn(`Thử nạp lại món lần ${retryCount + 1}...`);
          if (retryCount < 5 && requestGeneration === branchGenerationRef.current) {
            setTimeout(() => fetchOrderDetails(retryCount + 1), 1500);
          }
        }
        return false;
      };

      fetchOrderDetails();
    } else {
      // Nếu bàn trống, đảm bảo giỏ hàng local cũng trống
      setTableCarts(prev => ({ ...prev, [tableId]: [] }));
      invalidateAuthoritativeTotal(tableId);
      setTableCustomers(prev => ({ ...prev, [tableId]: { phone: '', name: 'Khách lẻ' } }));
    }
  };

  useEffect(() => {
    let disposed = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/kitchenHub`, {
        accessTokenFactory: () => localStorage.getItem('adminToken') || ''
      })
      .withAutomaticReconnect()
      .build();

    const onRequestStatusUpdated = async () => {
      if (disposed) return;
      const tableId = selectedTableIdRef.current;
      const requestGeneration = branchGenerationRef.current;
      if (!tableId || tableId === 'delivery') return;

      const table = tablesRef.current.find(t => t.id === tableId);
      if (!table) return;

      try {
        const branchId = branchInfoRef.current?.id || localStorage.getItem('selectedBranchId');
        const response = await fetch(`${API_URL}/api/Order?status=Đang xử lý&branchId=${branchId || ''}&tableName=${encodeURIComponent(table.name)}&_t=${Date.now()}`);
        if (!response.ok) return;
        const orders = await response.json();
        if (disposed) return;
        if (!isCurrentBranchRequest(branchId || null, requestGeneration)) return;
        const activeOrder = orders[0];
        if (activeOrder?.id) await loadKitchenStatuses(activeOrder.id, tableId, requestGeneration, () => !disposed);
      } catch (err) {
        console.warn('Không thể đồng bộ trạng thái món từ bếp.', err);
      }
    };

    connection.on('RequestStatusUpdated', onRequestStatusUpdated);
    connection.onreconnecting(() => {
      if (!disposed) setIsSignalRConnected(false);
    });
    connection.onreconnected(() => {
      if (disposed) return;
      setIsSignalRConnected(true);
      void onRequestStatusUpdated();
    });
    connection.onclose(() => {
      if (!disposed) setIsSignalRConnected(false);
    });

    void connection.start()
      .then(() => {
        if (!disposed) setIsSignalRConnected(true);
      })
      .catch(err => {
        if (!disposed) console.warn('Kitchen status connection failed.', err);
      });

    return () => {
      disposed = true;
      connection.off('RequestStatusUpdated', onRequestStatusUpdated);
      void connection.stop().catch(err => console.warn('Kitchen status connection cleanup failed.', err));
    };
  }, []);

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
          notifyFeedback(`Món này đã gửi bếp ${minQty} phần, không thể giảm thêm. Vui lòng dùng chức năng Hủy món nếu cần.`, 'warning');
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
          notifyFeedback("Món ăn đã được gửi xuống bếp, không thể xóa trực tiếp. Vui lòng dùng chức năng Hủy món.", 'warning');
          return prev;
        }
        cart.splice(index, 1);
      }
      return { ...prev, [selectedTableId]: cart };
    });
    invalidateAuthoritativeTotal();
  };

  const handleCartNoteChange = (index: number, note: string) => {
    const cart = [...(tableCarts[selectedTableId] || [])];
    if (cart[index]) cart[index] = { ...cart[index], note };
    setTableCarts({ ...tableCarts, [selectedTableId]: cart });
    invalidateAuthoritativeTotal();
  };

  const updateCustomerForTable = (field: 'phone' | 'name' | 'email', value: string) => {
    setTableCustomers(prev => ({
      ...prev,
      [selectedTableId]: {
        ...(prev[selectedTableId] || { phone: '', name: 'Khách lẻ' }),
        [field]: value,
        ...(field === 'phone' ? {
          id: undefined,
          loyaltyPoints: undefined,
          lookupStatus: undefined,
          lookupError: undefined,
          memberConsent: false
        } : {})
      }
    }));
  };

  const handlePaymentPhoneChange = (value: string) => updateCustomerForTable('phone', value.replace(/\D/g, ''));
  const handlePaymentCustomerNameChange = (value: string) => updateCustomerForTable('name', value);
  const handlePaymentMemberConsentChange = (checked: boolean) => setTableCustomers(prev => ({
    ...prev,
    [selectedTableId]: { ...prev[selectedTableId], memberConsent: checked }
  }));

  const loadBranchPaymentInfo = async (branch: Branch) => {
    const generation = branchGenerationRef.current;
    const controller = new AbortController();
    paymentInfoAbortControllerRef.current?.abort();
    paymentInfoAbortControllerRef.current = controller;
    try {
      const response = await fetch(`${API_URL}/api/Branch/${branch.id}/payment-info`, { signal: controller.signal });
      if (!isCurrentBranchRequest(branch.id, generation)) return;
      if (!response.ok) {
        setBranchInfo(branch);
        setBranchPaymentInfoError(response.status === 404
          ? 'Chi nhánh chưa cấu hình thông tin VietQR.'
          : `Không thể tải thông tin VietQR (HTTP ${response.status}).`);
        return;
      }

      const paymentInfo = await response.json();
      if (isCurrentBranchRequest(branch.id, generation)) {
        setBranchInfo({ ...branch, ...paymentInfo });
        setBranchPaymentInfoError(null);
        setQrImageError(false);
      }
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') return;
      if (!isCurrentBranchRequest(branch.id, generation)) return;
      setBranchInfo(branch);
      setBranchPaymentInfoError('Không thể kết nối để tải thông tin VietQR.');
    } finally {
      if (paymentInfoAbortControllerRef.current === controller) paymentInfoAbortControllerRef.current = null;
    }
  };

  const currentCustomer = tableCustomers[selectedTableId] || { phone: '', name: 'Khách lẻ' };

  const customerLookupRequestRef = React.useRef(0);

  const checkCustomer = async (phone: string, tableId: string, requestId: number) => {
    if (phone.length < 10) return;
    try {
      setIsCheckingPhone(true);
      const token = localStorage.getItem('adminToken');
      const response = await fetch(`${API_URL}/api/Customer/${phone}`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (requestId !== customerLookupRequestRef.current) return;
      if (response.ok) {
        const data = await response.json();
        setTableCustomers(prev => ({
          ...prev,
          [tableId]: {
            id: data.id,
            phone: phone,
            name: data.fullName,
            email: data.email || '',
            loyaltyPoints: data.loyaltyPoints || 0,
            lookupStatus: 'found',
            memberConsent: false
          }
        }));
      } else if (response.status === 404) {
        setTableCustomers(prev => ({
          ...prev,
          [tableId]: {
            ...(prev[tableId] || { phone, name: 'Khách lẻ' }),
            id: undefined,
            phone,
            loyaltyPoints: undefined,
            lookupStatus: 'not-found',
            lookupError: undefined,
            memberConsent: false
          }
        }));
      } else {
        const errorMessage = response.status === 401 || response.status === 403
          ? 'Bạn không có quyền tra cứu thành viên.'
          : `Không thể tra cứu thành viên (HTTP ${response.status}). Bạn có thể thử lại hoặc xóa SĐT để thanh toán khách vãng lai.`;
        setTableCustomers(prev => ({
          ...prev,
          [tableId]: {
            ...(prev[tableId] || { phone, name: 'Khách lẻ' }),
            id: undefined,
            phone,
            lookupStatus: 'error',
            lookupError: errorMessage,
            memberConsent: false
          }
        }));
      }
    } catch (err) {
      if (requestId === customerLookupRequestRef.current) {
        const lookupError = err instanceof SyntaxError
          ? 'Phản hồi tra cứu thành viên không hợp lệ. Bạn có thể thử lại hoặc xóa SĐT để thanh toán khách vãng lai.'
          : 'Không thể kết nối để tra cứu thành viên. Bạn có thể thử lại hoặc xóa SĐT để thanh toán khách vãng lai.';
        setTableCustomers(prev => ({
          ...prev,
          [tableId]: {
            ...(prev[tableId] || { phone, name: 'Khách lẻ' }),
            id: undefined,
            phone,
            lookupStatus: 'error',
            lookupError,
            memberConsent: false
          }
        }));
      }
    } finally {
      if (requestId === customerLookupRequestRef.current) setIsCheckingPhone(false);
    }
  };

  const handleRetryCustomerLookup = () => {
    const requestId = ++customerLookupRequestRef.current;
    void checkCustomer(currentCustomer.phone, selectedTableId, requestId);
  };

  useEffect(() => {
    const requestedTableId = searchParams.get('tableId');
    if (!requestedTableId || !tables.some(table => table.id === requestedTableId)) return;
    void handleSelectTable(requestedTableId, true);
    const nextParams = new URLSearchParams(searchParams);
    nextParams.delete('tableId');
    setSearchParams(nextParams, { replace: true });
  }, [tables, searchParams]);

  useEffect(() => {
    const requestId = ++customerLookupRequestRef.current;
    const tableId = selectedTableId;
    const phone = currentCustomer.phone.trim();

    if (!phone || phone.length < 10) {
      setIsCheckingPhone(false);
      return;
    }

    const timer = window.setTimeout(() => {
      void checkCustomer(phone, tableId, requestId);
    }, 300);

    return () => window.clearTimeout(timer);
  }, [selectedTableId, currentCustomer.phone]);

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
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        },
        body: JSON.stringify({ customerId: customer.id, points: pointsToRedeem })
      });

      if (response.ok) {
        const data = await response.json();
        notifyFeedback(data.message, 'success');
        setPointsToRedeem(0);
        // Refresh order data
        await handleSelectTable(selectedTableId, true);
        // Also refresh customer to get new loyalty balance
        const requestId = ++customerLookupRequestRef.current;
        void checkCustomer(customer.phone, selectedTableId, requestId);
      } else {
        const error = await response.json();
        notifyFeedback(error.message || 'Lỗi khi đổi điểm');
      }
    } catch (err) {
      console.error(err);
      notifyFeedback('Lỗi kết nối khi đổi điểm');
    } finally {
      setIsSaving(false);
    }
  };

  const handlePaymentPointsChange = (value: number) => {
    setPointsToRedeem(Math.min(value, currentCustomer.loyaltyPoints || 0));
  };
  const handlePaymentMethodChange = (method: 'Tiền mặt' | 'Chuyển khoản') => setPaymentMethod(method);
  const handleClosePaymentModal = () => {
    setIsPaymentModalOpen(false);
    setPaymentPreparationError(null);
  };
  const handleQrImageLoad = () => setQrImageError(false);
  const handleQrImageError = () => setQrImageError(true);

  const handleAcceptWebOrder = async (targetTableId: string) => {
    if (!orderToAccept || isSaving) return;

    try {
      setIsSaving(true);
      const acceptedId = orderToAccept.id;
      const acceptanceGeneration = branchGenerationRef.current;
      const acceptanceRevision = getCartRevision(targetTableId);
      const acceptResponse = await fetch(`${API_URL}/api/Order/${acceptedId}/accept`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ tableId: targetTableId === 'delivery' ? null : targetTableId })
      });

      if (acceptResponse.status === 409) {
        const conflict = await acceptResponse.json().catch(() => ({}));
        notifyFeedback(conflict.message || 'Đơn này đã được nhân viên khác tiếp nhận hoặc không còn chờ xử lý.', 'warning');
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
      const acceptedBranchId = String(acceptedOrder.branchId || branchInfo?.id || activeBranchIdRef.current || '');
      const acceptedSnapshot = setFinancialSnapshotIfCurrent(acceptedOrder, targetTableId, acceptedBranchId, acceptanceRevision, acceptanceGeneration);
      const acceptedHasBreakdown = ['subTotal', 'serviceFeeAmount', 'serviceFeePercent', 'vatAmount', 'vatPercent', 'discount']
        .every(field => Object.prototype.hasOwnProperty.call(acceptedOrder, field));
      if ((!acceptedSnapshot || !acceptedHasBreakdown) && acceptedOrder.id) {
        try {
          await fetchAndApplyFinancialSnapshot(String(acceptedOrder.id), targetTableId, acceptedBranchId, acceptanceRevision, acceptanceGeneration);
        } catch {
          setFinancialSnapshots(previous => {
            const next = { ...previous };
            delete next[targetTableId];
            return next;
          });
        }
      }
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
      window.dispatchEvent(new Event(PENDING_ORDER_CHANGED_EVENT));
      notifyFeedback(`Đã chấp nhận đơn hàng và chuyển vào ${tableName}`, 'success');
    } catch (err: any) {
      console.error("Lỗi khi chấp nhận đơn:", err);
      notifyFeedback(`Lỗi hệ thống: ${err.message}`);
    } finally {
      setIsSaving(false);
      setOrderToAccept(null);
    }
  };

  const preparePayment = async () => {
    if (currentCart.length === 0) {
      notifyFeedback('Vui lòng chọn món trước khi thanh toán!', 'warning');
      return false;
    }
    if (isPreparingPayment) return false;
    if (isSaving || saveRequestInFlightRef.current) {
      setPaymentPreparationError('Đơn hàng đang được đồng bộ. Vui lòng thử lại sau khi đồng bộ hoàn tất.');
      return false;
    }

    const requestTableId = selectedTableId;
    const requestBranchId = branchInfo?.id || activeBranchIdRef.current || '';
    const requestRevision = getCartRevision(requestTableId);
    const requestGeneration = branchGenerationRef.current;
    const previousSnapshot = financialSnapshots[requestTableId];
    const previousWasCurrent = Boolean(
      previousSnapshot &&
      previousSnapshot.branchId === requestBranchId &&
      previousSnapshot.tableId === requestTableId &&
      previousSnapshot.cartRevision === requestRevision
    );

    setIsPreparingPayment(true);
    setPaymentPreparationError(null);
    try {
      const orderId = await handleSaveOrder(true);
      if (!orderId) throw new Error('Không thể đồng bộ đơn hàng trước khi thanh toán.');
      const snapshot = await fetchAndApplyFinancialSnapshot(orderId, requestTableId, requestBranchId, requestRevision, requestGeneration);

      if (previousWasCurrent && previousSnapshot && previousSnapshot.totalAmount !== snapshot.totalAmount) {
        throw new Error('Tổng tiền đã thay đổi. Vui lòng kiểm tra lại breakdown rồi bấm Chuẩn bị thanh toán lần nữa.');
      }
      return true;
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Không thể chuẩn bị tổng tiền thanh toán.';
      setPaymentPreparationError(message);
      return false;
    } finally {
      setIsPreparingPayment(false);
    }
  };

  const handlePayment = async () => {
    if (userRole === 'employee') {
      notifyFeedback('Nhân viên phục vụ không có quyền thanh toán.', 'warning');
      return;
    }
    if (currentCart.length === 0) {
      notifyFeedback('Vui lòng chọn món trước khi thanh toán!', 'warning');
      return;
    }
    setIsPaymentModalOpen(true);
    await preparePayment();
  };

  const [isSaving, setIsSaving] = useState(false);
  isPaymentModalOpenRef.current = isPaymentModalOpen;
  isSavingRef.current = isSaving;
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
      setRefreshError(null);
      setLoading(true);
      await Promise.all([
        fetchProducts(true),
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

  const handleSaveOrder = async (isAuto = false): Promise<string | null> => {
    if (currentCart.length === 0 || isSaving || saveRequestInFlightRef.current) return null;
    saveRequestInFlightRef.current = true;

    const requestTableId = selectedTableId;
    const requestBranchId = branchInfo?.id || activeBranchIdRef.current || '';
    const requestRevision = getCartRevision(requestTableId);
    const requestGeneration = branchGenerationRef.current;
    const requestOrderId = activeOrderIds[requestTableId];
    const requestCart = currentCart;
    const requestTable = selectedTable;
    const requestCustomer = currentCustomer;

    try {
      setIsSaving(true);
      const order = {
        ...(requestOrderId ? { id: requestOrderId } : {}),
        tableName: requestTable ? requestTable.name : 'Mang về',
        totalAmount: totalAmount,
        paidAmount: 0,
        status: 'Đang xử lý',
        customerName: requestCustomer.name,
        customerPhone: requestCustomer.phone || null,
        customerEmail: requestCustomer.email || null,
        createdBy: userName,
        branchId: requestBranchId || null,
        branchName: branchInfo?.name,
        details: requestCart.map(item => ({
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
         setFinancialSnapshotIfCurrent(savedOrder, requestTableId, requestBranchId, requestRevision, requestGeneration);
        if (!isAuto) {
          notifyFeedback('Đã đồng bộ đơn hàng lên hệ thống!', 'success');
        }

        // Tự động cập nhật trạng thái bàn nếu cần
        if (requestTableId !== 'delivery' && requestGeneration === branchGenerationRef.current && getCartRevision(requestTableId) === requestRevision) {
          const table = tables.find(t => t.id === requestTableId);
          if (table && table.status !== 'Có khách') {
            await updateTableStatus(requestTableId, 'Có khách');
          }
        }
        return savedOrder?.id ? String(savedOrder.id) : null;
      }
      if (!isAuto) notifyFeedback('Lỗi khi gửi đơn vào hệ thống.');
      return null;
    } catch (err) {
      if (!isAuto) notifyFeedback('Lỗi khi gửi đơn vào hệ thống.');
      return null;
    } finally {
      saveRequestInFlightRef.current = false;
      setIsSaving(false);
    }
  };

  const fetchAndApplyFinancialSnapshot = async (
    orderId: string,
    tableId: string,
    branchId: string,
    cartRevision: number,
    requestGeneration: number
  ) => {
    const response = await fetch(`${API_URL}/api/Order/${orderId}`, { cache: 'no-store' });
    if (!response.ok) throw new Error(`Không thể tải lại tổng tiền (${response.status}).`);
    const order = await response.json();
    const snapshot = setFinancialSnapshotIfCurrent(order, tableId, branchId, cartRevision, requestGeneration);
    if (!snapshot) throw new Error('Tổng tiền không còn thuộc giỏ hàng hoặc chi nhánh hiện tại.');
    return snapshot;
  };

  const confirmPayment = async () => {
    if (!hasAuthoritativeSnapshot || !currentSnapshot) {
       notifyFeedback('Tổng tiền chưa được xác nhận từ máy chủ. Vui lòng bấm Chuẩn bị thanh toán lại.', 'warning');
       return;
    }

    if (currentSnapshot.branchId !== (branchInfo?.id || '') || currentSnapshot.tableId !== selectedTableId || currentSnapshot.cartRevision !== currentCartRevision) {
       notifyFeedback('Giỏ hàng hoặc chi nhánh đã thay đổi. Vui lòng tải lại tổng tiền trước khi thanh toán.', 'warning');
       return;
    }

    if (isSaving) {
       notifyFeedback('Hệ thống đang lưu dữ liệu, vui lòng đợi giây lát.', 'warning');
       return;
    }

    if (currentCustomer.phone && currentCustomer.phone.length < 10) {
       notifyFeedback('Số điện thoại chưa đủ hợp lệ. Hãy nhập đủ hoặc xóa SĐT để thanh toán khách vãng lai.', 'warning');
       return;
    }

    if (currentCustomer.phone && currentCustomer.lookupStatus === 'error') {
       notifyFeedback(currentCustomer.lookupError || 'Tra cứu thành viên thất bại. Hãy thử lại hoặc xóa SĐT.', 'warning');
       return;
    }

    if (currentCustomer.phone && !currentCustomer.id && currentCustomer.lookupStatus !== 'not-found') {
      notifyFeedback('Vui lòng chờ tra cứu thành viên hoàn tất.', 'warning');
      return;
    }

    try {
      setIsSaving(true);
      const paymentSnapshot = currentSnapshot;
      const paymentTableId = selectedTableId;
      const paymentBranchId = branchInfo?.id || '';
      const paymentRevision = currentCartRevision;
      const paymentGeneration = branchGenerationRef.current;

      const paymentResponse = await fetch(`${API_URL}/api/Order/${paymentSnapshot.orderId}/payment`, {
           method: 'POST', headers: { 'Content-Type': 'application/json' },
           body: JSON.stringify({
             amount: paymentSnapshot.totalAmount,
             paymentMethod,
             customerPhone: currentCustomer.phone || null,
             customerName: currentCustomer.name || null,
             createMember: currentCustomer.lookupStatus === 'not-found' && Boolean(currentCustomer.memberConsent)
           })
      });

      if (!paymentResponse.ok) {
        const error = await paymentResponse.json().catch(() => ({}));
        try {
          await handleSelectTable(paymentTableId, true, true);
        } catch {
          setFinancialSnapshots(previous => {
            const next = { ...previous };
            delete next[paymentTableId];
            return next;
          });
        }
        setPaymentPreparationError(error.message || 'Tổng tiền hoặc trạng thái đơn đã thay đổi. Vui lòng tải lại và xác nhận lại trước khi thanh toán.');
        notifyFeedback(`Thanh toán chưa thực hiện: ${error.message || 'Tổng tiền hoặc trạng thái đơn đã thay đổi.'}`, 'warning');
        return;
      }

      setIsPaymentModalOpen(false);
      if (paymentTableId !== 'delivery' && paymentGeneration === branchGenerationRef.current && activeBranchIdRef.current === paymentBranchId) {
        await updateTableStatus(paymentTableId, 'Trống');
      }

      if (paymentGeneration === branchGenerationRef.current && activeBranchIdRef.current === paymentBranchId && getCartRevision(paymentTableId) === paymentRevision) {
        setTableCarts(prev => ({ ...prev, [paymentTableId]: [] }));
        setActiveOrderIds(prev => { const next = { ...prev }; delete next[paymentTableId]; return next; });
        setFinancialSnapshots(prev => { const next = { ...prev }; delete next[paymentTableId]; return next; });
        cartRevisionsRef.current[paymentTableId] = getCartRevision(paymentTableId) + 1;

        setTableCustomers(prev => ({ ...prev, [paymentTableId]: { phone: '', name: 'Khách lẻ' } }));
        setSelectedTableId('delivery');
      }

      notifyFeedback('Thanh toán thành công!', 'success');
    } catch (err) {
      setPaymentPreparationError('Lỗi kết nối đến server. Vui lòng thử lại; chưa ghi nhận thanh toán.');
      notifyFeedback('Lỗi kết nối đến server. Chưa ghi nhận thanh toán.', 'warning');
    } finally {
      setIsSaving(false);
    }
  };

  const filteredProducts = filterProducts(products, activeCategory, searchTerm);
  const handleFocusProductSearch = () => document.getElementById('pos-product-search')?.focus();
  const handleCloseCurrentOrder = () => setSelectedTableId('delivery');
  const handlePrintCurrentOrder = () => window.print();
  const handleOpenInvoiceHistory = () => setIsInvoiceHistoryModalOpen(true);
  const handleOpenCloseShiftModal = () => setIsCloseShiftModalOpen(true);
  const handleOpenShiftHistory = () => setIsShiftHistoryModalOpen(true);
  const handleClearUnsavedItems = () => {
    if (currentCart.some(item => item.sentQuantity > 0)) {
      notifyFeedback('Món đã gửi bếp không thể xóa bằng thao tác này.', 'warning');
      return;
    }
    if (currentCart.length > 0 && window.confirm('Xóa toàn bộ món chưa gửi trong đơn?')) {
      setTableCarts(prev => ({ ...prev, [selectedTableId]: [] }));
      invalidateAuthoritativeTotal();
    }
  };
  const handleTransferOrder = async (targetTableId: string) => {
    if (selectedTableId === 'delivery' || !currentCart.length || isSaving) return;
    const sourceTableId = selectedTableId;
    const orderId = activeOrderIds[sourceTableId];

    if (!orderId) {
      if (!window.confirm('Đơn chưa được lưu trên máy chủ. Chỉ chuyển giỏ hàng hiện tại sang bàn mới?')) return;
      const movedCart = tableCarts[sourceTableId] || [];
      setTableCarts(previous => ({ ...previous, [sourceTableId]: [], [targetTableId]: movedCart }));
      setTableCustomers(previous => ({ ...previous, [targetTableId]: previous[sourceTableId] || { phone: '', name: 'Khách lẻ' } }));
      setSelectedTableId(targetTableId);
      setIsTransferTableModalOpen(false);
      notifyFeedback('Đã chuyển giỏ hàng sang bàn mới.', 'success');
      return;
    }

    try {
      setIsSaving(true);
      const response = await fetch(`${API_URL}/api/Order/${orderId}/transfer-table`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ targetTableId })
      });
      const payload = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(payload.message || 'Không thể chuyển đơn sang bàn mới.');

      const movedCart = tableCarts[sourceTableId] || [];
      const movedCustomer = tableCustomers[sourceTableId];
      setTableCarts(previous => ({ ...previous, [sourceTableId]: [], [targetTableId]: movedCart }));
      setTableCustomers(previous => ({ ...previous, [targetTableId]: movedCustomer || { phone: '', name: 'Khách lẻ' } }));
      setActiveOrderIds(previous => {
        const next = { ...previous, [targetTableId]: orderId };
        delete next[sourceTableId];
        return next;
      });
      setFinancialSnapshots(previous => {
        const snapshot = previous[sourceTableId];
        const next = { ...previous };
        delete next[sourceTableId];
        if (snapshot) next[targetTableId] = { ...snapshot, tableId: targetTableId };
        return next;
      });
      setSelectedTableId(targetTableId);
      setIsTransferTableModalOpen(false);
      await Promise.all([fetchTables(), handleSelectTable(targetTableId, true)]);
      notifyFeedback(payload.message || 'Đã chuyển toàn bộ đơn sang bàn mới.', 'success');
    } catch (error) {
      notifyFeedback(error instanceof Error ? error.message : 'Không thể chuyển đơn sang bàn mới.', 'warning');
    } finally {
      setIsSaving(false);
    }
  };
  const handleCancelCurrentOrder = async () => {
    // Tìm đơn hàng hiện tại của bàn trên server bằng tham số tableName thay vì search
    try {
      const branchId = branchInfo?.id || localStorage.getItem('selectedBranchId');
      const response = await fetch(`${API_URL}/api/Order?status=Đang xử lý&branchId=${branchId || ''}&tableName=${encodeURIComponent(selectedTable?.name || '')}`);
      const orders = await response.json();

      // Tìm đơn hàng khớp chính xác bàn
      const activeOrder = Array.isArray(orders) ? orders.find((order: any) => order.tableName === selectedTable?.name) : null;

      if (activeOrder) {
        await handleDeleteOrder(activeOrder.id, selectedTable?.name);
      } else if (window.confirm('Không tìm thấy đơn hàng trên máy chủ. Xóa giỏ hàng local?')) {
        setTableCarts(prev => ({ ...prev, [selectedTableId]: [] }));
        invalidateAuthoritativeTotal();
        await updateTableStatus(selectedTableId, 'Trống');
      }
    } catch (error) {
      console.error('Lỗi khi truy tìm đơn để xóa:', error);
    }
  };
  const paymentQrImageUrl = branchInfo?.bankName && branchInfo?.accountNumber
    && hasAuthoritativeSnapshot && currentSnapshot
    ? `https://img.vietqr.io/image/${branchInfo.bankName.replace(/\s/g, '')}-${branchInfo.accountNumber}-compact2.jpg?amount=${currentSnapshot.totalAmount}&addInfo=Thanh toan don hang&accountName=${encodeURIComponent(branchInfo.accountHolder || '')}`
    : null;
  const estimatedEarnedPoints = hasAuthoritativeSnapshot ? Math.floor(totalAmount / 10000) : 0;

  const handleViewPendingReservation = () => navigate(userRole === 'cashier' ? '/pos/reservations' : '/reservations');

  return (
    <div className="flex h-full min-h-0 w-full min-w-0 max-w-full flex-col overflow-hidden bg-[#f8f9fa] text-gray-800 font-sans md:flex-row">
      {/* LEFT SIDE: Tables / Menu Selection */}
      <div className="w-full min-w-0 h-[52%] min-h-0 flex flex-col border-b border-gray-300 bg-white md:h-full md:w-[60%] md:border-b-0 md:border-r">
        {/* Header Left (Blue) */}
        <POSHeader
          userRole={userRole}
          branchInfo={branchInfo}
          allBranches={allBranches}
          posTab={posTab}
          searchTerm={searchTerm}
          loading={loading}
          refreshError={refreshError}
          onSwitchBranch={handleSwitchBranch}
          onNavigateBranches={() => navigate('/branches')}
          onTabChange={setPosTab}
          onSearchTermChange={setSearchTerm}
          onRefresh={handleManualRefresh}
          onFocusSearch={handleFocusProductSearch}
        />
        {/* Category Tabs / Filters */}
        <POSCategoryTabs
          categories={categories}
          activeCategory={activeCategory}
          onCategoryChange={setActiveCategory}
        />

        {/* Main Content Area */}
        <div className="flex-1 min-h-0 overflow-auto p-2 sm:p-4 bg-gray-50">
          {loading ? (
            <div className="flex items-center justify-center h-full">
               <Loader2 size={32} className="animate-spin text-blue-600" />
            </div>
          ) : posTab === 'notifications' ? (
            <POSPendingRequests
              orders={pendingOrders}
              reservations={pendingReservations}
              ordersError={pendingOrdersError}
              onRejectOrder={handleDeleteOrder}
              onProcessOrder={setOrderToAccept}
              onViewReservation={handleViewPendingReservation}
            />
          ) : posTab === 'tables' ? (
            <POSTableMap
              tables={tables}
              tableCarts={tableCarts}
              selectedTableId={selectedTableId}
              onSelectTable={handleSelectTable}
            />
          ) : (
            <POSProductGrid products={filteredProducts} onAddProduct={addToCart} />
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
      <div className="w-full min-w-0 h-[48%] min-h-0 flex flex-col bg-white shadow-2xl md:h-full md:flex-1">
        <POSOrderHeader
          selectedTable={selectedTable}
          selectedTableId={selectedTableId}
          currentCart={currentCart}
          currentCustomerPhone={currentCustomer.phone}
          customerPhoneInputRef={customerPhoneInputRef}
          activeShift={Boolean(activeShift)}
          userRole={userRole}
          isSignalRConnected={isSignalRConnected}
          isSaving={isSaving}
          onCloseCurrentOrder={handleCloseCurrentOrder}
          onPrint={handlePrintCurrentOrder}
          onOpenInvoiceHistory={handleOpenInvoiceHistory}
          onCloseShift={handleOpenCloseShiftModal}
          onOpenShiftHistory={handleOpenShiftHistory}
          onRefreshTable={() => handleSelectTable(selectedTableId)}
          onOpenTransferTable={() => setIsTransferTableModalOpen(true)}
          onCancelCurrentOrder={handleCancelCurrentOrder}
          onCustomerPhoneChange={handlePaymentPhoneChange}
          onFocusCustomerPhone={() => customerPhoneInputRef.current?.focus()}
          onClearUnsavedItems={handleClearUnsavedItems}
        />
        {/* Selected Items List */}
        <POSCartItems
          items={currentCart}
          onRemove={removeFromCart}
          onUpdateQuantity={updateQuantity}
          onNoteChange={handleCartNoteChange}
        />

        {/* Footer Order Actions */}
        <POSOrderActions
          userName={userName}
          userRole={userRole}
          totalAmount={totalAmount}
          previewTotal={previewTotal}
          hasAuthoritativeSnapshot={hasAuthoritativeSnapshot}
          financialSnapshot={hasAuthoritativeSnapshot ? currentSnapshot : null}
          currentCartLength={currentCart.length}
          isSendingToKitchen={isSendingToKitchen}
          isSaving={isSaving}
          kitchenMessage={kitchenMessage}
          onSendToKitchen={handleSendToKitchen}
          onPayment={handlePayment}
        />
      </div>

      {/* PAYMENT MODAL */}
      {isPaymentModalOpen && (
        <POSPaymentModal
          totalAmount={totalAmount}
          previewTotal={previewTotal}
          hasAuthoritativeSnapshot={hasAuthoritativeSnapshot}
          financialSnapshot={hasAuthoritativeSnapshot ? currentSnapshot : null}
          isPreparingPayment={isPreparingPayment}
          preparationError={paymentPreparationError}
          estimatedEarnedPoints={estimatedEarnedPoints}
          currentCartLength={currentCart.length}
          currentCustomer={currentCustomer}
          isCheckingPhone={isCheckingPhone}
          isSaving={isSaving}
          pointsToRedeem={pointsToRedeem}
          paymentMethod={paymentMethod}
          branchInfo={branchInfo}
          branchPaymentInfoError={branchPaymentInfoError}
          qrImageError={qrImageError}
          qrImageUrl={paymentQrImageUrl}
          onClose={handleClosePaymentModal}
          onPhoneChange={handlePaymentPhoneChange}
          onNameChange={handlePaymentCustomerNameChange}
          onRedeemPoints={handleRedeemPoints}
          onPointsChange={handlePaymentPointsChange}
          onMemberConsentChange={handlePaymentMemberConsentChange}
          onRetryCustomerLookup={handleRetryCustomerLookup}
          onPaymentMethodChange={handlePaymentMethodChange}
          onConfirmPayment={confirmPayment}
          onRetryPreparation={preparePayment}
          onQrImageLoad={handleQrImageLoad}
          onQrImageError={handleQrImageError}
        />
      )}
      <POSNotificationToast visible={showNotification} type={notifType} onView={() => { setPosTab('notifications'); setShowNotification(false); }} onClose={() => setShowNotification(false)} />
      <POSAcceptOrderModal order={orderToAccept as POSPendingOrder | null} tables={tables} isSaving={isSaving} onClose={() => setOrderToAccept(null)} onAccept={handleAcceptWebOrder} onReject={handleDeleteOrder} />
      <POSTransferTableModal open={isTransferTableModalOpen} tables={tables} currentTableId={selectedTableId} isSaving={isSaving} onClose={() => setIsTransferTableModalOpen(false)} onTransfer={handleTransferOrder} />

      <POSOpenShiftModal open={isShiftModalOpen} startingCash={startingCash} onStartingCashChange={setStartingCash} onOpenShift={handleOpenShift} onLogout={onLogout} />
      <POSCloseShiftModal open={isCloseShiftModalOpen} activeShift={activeShift} endingCash={endingCash} shiftNote={shiftNote} onEndingCashChange={setEndingCash} onShiftNoteChange={setShiftNote} onClose={() => setIsCloseShiftModalOpen(false)} onCloseShift={handleCloseShift} />
      <POSShiftHistoryModal open={isShiftHistoryModalOpen} shifts={shiftHistory} loading={loadingHistory} onClose={() => setIsShiftHistoryModalOpen(false)} />

      <POSInvoiceHistoryModal
        open={isInvoiceHistoryModalOpen}
        invoices={invoices as POSInvoiceRecord[]}
        loading={loadingInvoices}
        search={invoiceSearch}
        fromDate={invoiceFromDate}
        toDate={invoiceToDate}
        statusFilter={invoiceStatusFilter}
        selectedOrder={selectedHistoryOrder as POSInvoiceRecord | null}
        receiptSettings={receiptSettings}
        allBranches={allBranches}
        loyaltyInfo={orderLoyaltyInfo as POSOrderLoyaltyInfo | null}
        onSearchChange={setInvoiceSearch}
        onFromDateChange={setInvoiceFromDate}
        onToDateChange={setInvoiceToDate}
        onStatusFilterChange={setInvoiceStatusFilter}
        onRefresh={fetchInvoiceHistory}
        onPrint={handlePrintInvoice}
        onClose={() => setIsInvoiceHistoryModalOpen(false)}
        onSelectOrder={setSelectedHistoryOrder}
      />
      <POSProductOptionsModal
        open={isOptionsModalOpen}
        product={currentCustomizingProduct}
        selectedSize={selectedSize}
        selectedToppings={selectedToppings}
        onSelectSize={setSelectedSize}
        onToggleTopping={(topping) => setSelectedToppings(current => current.some(item => item.name === topping.name) ? current.filter(item => item.name !== topping.name) : [...current, topping])}
        onClose={() => setIsOptionsModalOpen(false)}
        onConfirm={handleConfirmOptions}
      />
    </div>
  );
};

export default POSPage;
