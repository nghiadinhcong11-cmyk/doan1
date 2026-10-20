import React, { useState, useEffect } from 'react';
import { useRef } from 'react';
import { useLocation } from 'react-router-dom';
import { ShoppingBag, ChevronRight, Star, CheckCircle2, X, Gift, Percent, MapPin } from 'lucide-react';
import { API_URL } from '../config';
import Feedback, { FeedbackTone } from '../components/ui/Feedback';
import CustomerCategoryTabs from '../features/digital-menu/components/CustomerCategoryTabs';
import CustomerProductList from '../features/digital-menu/components/CustomerProductList';
import CustomerProductOptionsModal, { type ProductOption } from '../features/digital-menu/components/CustomerProductOptionsModal';
import CustomerOrderConfirmationModal from '../features/digital-menu/components/CustomerOrderConfirmationModal';
import type { CartItem, Product } from '../features/digital-menu/types';

const getQrDiagnosticPhase = (qrToken: string | null) => {
  if (!qrToken) return 'normal-link';
  return sessionStorage.getItem('guestQrSession') === 'true'
    ? 'after-qr-bootstrap'
    : 'before-or-during-qr-bootstrap';
};

const fetchWithDiagnostics = async (endpointPath: string, input: RequestInfo, init?: RequestInit, phase = 'unknown') => {
  try {
    const response = await fetch(input, init);
    if (import.meta.env.DEV) {
      console.info('[DigitalMenu] fetch response', {
        endpointPath,
        phase,
        status: response.status,
        contentType: response.headers.get('content-type') || null
      });
    }
    return response;
  } catch (error) {
    if (import.meta.env.DEV) {
      console.warn('[DigitalMenu] fetch failure', {
        endpointPath,
        phase,
        errorName: error instanceof Error ? error.name : 'UnknownError',
        errorMessage: error instanceof Error ? error.message : String(error)
      });
    }
    throw error;
  }
};

const parseJsonWithDiagnostics = async (response: Response, endpointPath: string, phase: string) => {
  try {
    const payload = await response.json();
    if (import.meta.env.DEV) {
      console.info('[DigitalMenu] JSON parsed', {
        endpointPath,
        phase,
        jsonParsed: true,
        responseShape: Array.isArray(payload) ? 'array' : payload === null ? 'null' : typeof payload
      });
    }
    return payload;
  } catch (error) {
    if (import.meta.env.DEV) {
      console.warn('[DigitalMenu] JSON parse failure', {
        endpointPath,
        phase,
        jsonParsed: false,
        errorName: error instanceof Error ? error.name : 'UnknownError',
        errorMessage: error instanceof Error ? error.message : String(error)
      });
    }
    throw error;
  }
};

type DiagnosticFetchState = {
  state: 'idle' | 'pending' | 'success' | 'http-error' | 'network-error' | 'json-error' | 'shape-error';
  status: number | null;
  errorName: string | null;
  errorMessage: string | null;
};

const initialDiagnosticFetchState: DiagnosticFetchState = {
  state: 'idle',
  status: null,
  errorName: null,
  errorMessage: null
};

const sanitizeDiagnosticError = (error: unknown) => {
  const name = error instanceof Error ? error.name : 'UnknownError';
  const message = error instanceof Error ? error.message : String(error);
  return {
    errorName: name.slice(0, 80),
    errorMessage: message
      .replace(/https?:\/\/[^\s]+/gi, '[url]')
      .replace(/([?&](?:qr|token|access_token)=)[^&\s]+/gi, '$1[redacted]')
      .slice(0, 240)
  };
};

const DebugMenuPanel = ({
  qrPresent,
  bootstrapState,
  productState,
  toppingState,
  productCountBeforeFilter,
  productCountAfterFilter
}: {
  qrPresent: boolean;
  bootstrapState: 'idle' | 'pending' | 'success' | 'error';
  productState: DiagnosticFetchState;
  toppingState: DiagnosticFetchState;
  productCountBeforeFilter: number | null;
  productCountAfterFilter: number | null;
}) => {
  const api = new URL(API_URL, window.location.origin);
  const currentPort = window.location.port || (window.location.protocol === 'https:' ? '443' : '80');
  const apiPort = api.port || (api.protocol === 'https:' ? '443' : '80');

  return (
    <aside className="mx-4 mt-4 rounded-2xl border-2 border-purple-300 bg-purple-50 p-4 text-[11px] font-mono text-purple-950" role="status">
      <p className="mb-2 font-black">QR MENU DIAGNOSTIC v1</p>
      <p>bundle: diagnostic-panel-enabled</p>
      <p>frontend: {window.location.hostname}:{currentPort}</p>
      <p>api: {api.hostname}:{apiPort}</p>
      <p>qrParameterPresent: {String(qrPresent)}</p>
      <p>bootstrap: {bootstrapState}</p>
      <p>product: {productState.state} / HTTP {productState.status ?? 'n/a'}</p>
      <p>productError: {productState.errorName || 'none'} / {productState.errorMessage || 'none'}</p>
      <p>topping: {toppingState.state} / HTTP {toppingState.status ?? 'n/a'}</p>
      <p>toppingError: {toppingState.errorName || 'none'} / {toppingState.errorMessage || 'none'}</p>
      <p>productCountBeforeFilter: {productCountBeforeFilter ?? 'n/a'}</p>
      <p>productCountAfterFilter: {productCountAfterFilter ?? 'n/a'}</p>
    </aside>
  );
};

const DigitalMenu = () => {
  const location = useLocation();
  type QrBootstrapState = 'idle' | 'loading' | 'success' | 'error';
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<string[]>(['Tất cả']);
  const [activeCategory, setActiveCategory] = useState('Tất cả');
  const [loading, setLoading] = useState(true);
  const [isSubmittingOrder, setIsSubmittingOrder] = useState(false);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [cartPulse, setCartPulse] = useState(false);
  const [isOrderSuccess, setIsOrderSuccess] = useState(false);
  const [feedbackMessage, setFeedbackMessage] = useState('');
  const [feedbackTone, setFeedbackTone] = useState<FeedbackTone>('error');
  const [tableInfo, setTableInfo] = useState<{id: string, name: string, branchId?: string, branchName?: string} | null>(null);
  const [branches, setBranches] = useState<any[]>([]);
  const [isBranchModalOpen, setIsBranchModalOpen] = useState(false);
  const [selectedBranch, setSelectedBranch] = useState<any>(null);
  const [qrBootstrapState, setQrBootstrapState] = useState<QrBootstrapState>('idle');
  const [productFetchState, setProductFetchState] = useState<DiagnosticFetchState>(initialDiagnosticFetchState);
  const [toppingFetchState, setToppingFetchState] = useState<DiagnosticFetchState>(initialDiagnosticFetchState);
  const [productCountBeforeFilter, setProductCountBeforeFilter] = useState<number | null>(null);
  const [productCountAfterFilter, setProductCountAfterFilter] = useState<number | null>(null);
  const requestInFlight = useRef(false);
  const initialLoadStarted = useRef(false);
  const qrBootstrapStartedFor = useRef<string | null>(null);
  const directGuestSessionRequest = useRef<Promise<void> | null>(null);

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
  const urlParams = new URLSearchParams(location.search);
  const qrToken = urlParams.get('qr');
  const debugMenu = urlParams.get('debugMenu') === '1';

  useEffect(() => {
    if (initialLoadStarted.current) return;
    initialLoadStarted.current = true;

    if (!qrToken) {
      void fetchProducts();
      /* if (false) {
        // sellableOnly requires an authoritative branch. Do not issue a
        // predictable 400 request before the customer selects one.
        setLoading(false);
        setFeedbackTone('warning');
        setFeedbackMessage('Vui lòng chọn chi nhánh để xem thực đơn.');
      } */
    }
    if (!qrToken) fetchBranches();
    fetchPromotions();
    if (!qrToken) {
      loadCustomerInfo();
    }
  }, []);

  useEffect(() => {
    const handlePriceUpdated = () => void fetchProducts();
    window.addEventListener('product-price-updated', handlePriceUpdated);
    return () => window.removeEventListener('product-price-updated', handlePriceUpdated);
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
    if (qrToken) {
      // Nếu tableId thay đổi so với ID đang lưu, ta xóa giỏ hàng để tránh đặt nhầm
      if (tableInfo) {
        setCart([]);
        setIsOrderSuccess(false);
      }
      fetchTableInfo();
    }
  }, [qrToken]);

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

  const fetchProducts = async (branchId?: string) => {
    if (requestInFlight.current) return;
    if (false && !branchId) {
      setLoading(false);
      setFeedbackTone('warning');
      setFeedbackMessage('Vui lòng chọn chi nhánh để xem thực đơn.');
      return;
    }
    requestInFlight.current = true;
    try {
      setLoading(true);
      setProductFetchState({ ...initialDiagnosticFetchState, state: 'pending' });
      setToppingFetchState({ ...initialDiagnosticFetchState, state: 'pending' });
      setProductCountBeforeFilter(null);
      setProductCountAfterFilter(null);
      const phase = getQrDiagnosticPhase(qrToken);

      // Load products independently so a topping endpoint failure does not hide the main menu.
      // Customer/Guest menu is the shared catalog; branch context is needed
      // for ordering, not for viewing products and prices.
      const menuQuery = new URLSearchParams({ isActive: 'true' });
      const branchQuery = `?${menuQuery.toString()}`;
      const [productResult, toppingResult] = await Promise.allSettled([
        fetchWithDiagnostics('/api/Product', `${API_URL}/api/Product${branchQuery}`, undefined, phase),
        fetchWithDiagnostics('/api/Topping', `${API_URL}/api/Topping`, undefined, phase)
      ]);

      const productStatus = productResult.status === 'fulfilled' ? productResult.value.status : null;
      const toppingStatus = toppingResult.status === 'fulfilled' ? toppingResult.value.status : null;
      if (productResult.status === 'rejected') {
        setProductFetchState({ state: 'network-error', status: null, ...sanitizeDiagnosticError(productResult.reason) });
      } else if (!productResult.value.ok) {
        setProductFetchState({ state: 'http-error', status: productResult.value.status, errorName: 'HttpError', errorMessage: `HTTP ${productResult.value.status}` });
      }
      if (toppingResult.status === 'rejected') {
        setToppingFetchState({ state: 'network-error', status: null, ...sanitizeDiagnosticError(toppingResult.reason) });
      } else if (!toppingResult.value.ok) {
        setToppingFetchState({ state: 'http-error', status: toppingResult.value.status, errorName: 'HttpError', errorMessage: `HTTP ${toppingResult.value.status}` });
      }
      if (import.meta.env.DEV) {
        console.info('[DigitalMenu] menu API diagnostics', {
          productStatus,
          toppingStatus,
          productRequestSucceeded: productResult.status === 'fulfilled',
          toppingRequestSucceeded: toppingResult.status === 'fulfilled'
        });
      }

      if (productResult.status === 'rejected' || !productResult.value.ok) {
        throw new Error(`Product API failed: ${productStatus ?? 'network'}`);
      }

      let productsData: any;
      try {
        productsData = await parseJsonWithDiagnostics(productResult.value, '/api/Product', phase);
      } catch (error) {
        setProductFetchState({ state: 'json-error', status: productResult.value.status, ...sanitizeDiagnosticError(error) });
        throw error;
      }
      const productCountBeforeFilter = Array.isArray(productsData) ? productsData.length : null;
      setProductCountBeforeFilter(productCountBeforeFilter);
      if (!Array.isArray(productsData)) {
        setProductFetchState({ state: 'shape-error', status: productResult.value.status, errorName: 'ResponseShapeError', errorMessage: 'Product response is not an array.' });
        throw new Error('Product API returned an invalid response.');
      }
      setProductFetchState({ state: 'success', status: productResult.value.status, errorName: null, errorMessage: null });

      let toppingsData: any[] = [];
      let toppingLoadFailed = false;
      if (toppingResult.status === 'fulfilled' && toppingResult.value.ok) {
        let parsedToppings: any;
        try {
          parsedToppings = await parseJsonWithDiagnostics(toppingResult.value, '/api/Topping', phase);
        } catch (error) {
          setToppingFetchState({ state: 'json-error', status: toppingResult.value.status, ...sanitizeDiagnosticError(error) });
          throw error;
        }
        if (Array.isArray(parsedToppings)) {
          toppingsData = parsedToppings;
          setToppingFetchState({ state: 'success', status: toppingResult.value.status, errorName: null, errorMessage: null });
        } else {
          toppingLoadFailed = true;
          setToppingFetchState({ state: 'shape-error', status: toppingResult.value.status, errorName: 'ResponseShapeError', errorMessage: 'Topping response is not an array.' });
        }
      } else {
        toppingLoadFailed = true;
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

      const sellableProducts = productsData.filter((product: any) => product.isActive !== false);
      const allItems = [...sellableProducts, ...toppingProducts];
      setProducts(allItems);
      setProductCountAfterFilter(sellableProducts.length);

      if (import.meta.env.DEV) {
        console.info('[DigitalMenu] product counts', {
          productCountBeforeFilter,
          productCountAfterFilter: sellableProducts.length,
          toppingCount: toppingProducts.length,
          totalMenuItemCount: allItems.length
        });
      }

      if (toppingLoadFailed) {
        setFeedbackTone('warning');
        setFeedbackMessage('Đã tải món chính; danh sách topping tạm thời không khả dụng.');
      } else if (allItems.length === 0) {
        setFeedbackTone('warning');
        setFeedbackMessage('Thực đơn hiện chưa có món đang bán.');
      }

      const cats = Array.from(new Set(allItems.map((p: any) => p.category).filter((c: any) => !!c))) as string[];
      setCategories(['Tất cả', ...cats]);
    } catch (err) {
      console.error(err);
      if (import.meta.env.DEV) {
      console.warn('[DigitalMenu] menu load failure', {
          phase: getQrDiagnosticPhase(qrToken),
          errorName: err instanceof Error ? err.name : 'UnknownError',
          errorMessage: err instanceof Error ? err.message : String(err)
        });
      }
      setFeedbackTone('error');
      setFeedbackMessage('Không thể tải thực đơn. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
      requestInFlight.current = false;
    }
  };

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
    if (!qrToken) return;
    if (qrBootstrapStartedFor.current === qrToken) return;
    qrBootstrapStartedFor.current = qrToken;
    setQrBootstrapState('loading');
    try {
      const phase = getQrDiagnosticPhase(qrToken);
      const response = await fetchWithDiagnostics('/api/guest/bootstrap', `${API_URL}/api/guest/bootstrap`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ qrToken })
      }, phase);
      if (response.ok) {
        const bootstrap = await parseJsonWithDiagnostics(response, '/api/guest/bootstrap', phase);
        if (!bootstrap?.token || !bootstrap?.table?.name || !bootstrap?.branch?.id) {
          throw new Error('QR bootstrap returned incomplete context.');
        }
        sessionStorage.removeItem('guestDirectSession');
        sessionStorage.removeItem('guestDirectBranchId');
        sessionStorage.setItem('guestToken', bootstrap.token);
        sessionStorage.setItem('guestQrSession', 'true');
        sessionStorage.setItem('guestQrToken', qrToken);
        setTableInfo({
          id: bootstrap.table.id || '',
          name: bootstrap.table.name,
          branchId: bootstrap.branch.id,
          branchName: bootstrap.branch.name
        });
        setFeedbackMessage('');

        // Cố định chi nhánh theo bàn
        if (bootstrap.branch.id) {
          setSelectedBranch({ id: bootstrap.branch.id, name: bootstrap.branch.name });
          void fetchProducts(bootstrap.branch.id);
        }
        setQrBootstrapState('success');
      } else {
         throw new Error(`QR bootstrap failed with status ${response.status}.`);
      }
    } catch (err) {
      console.error(err);
      qrBootstrapStartedFor.current = null;
      sessionStorage.removeItem('guestToken');
      sessionStorage.removeItem('guestQrSession');
      sessionStorage.removeItem('guestQrToken');
      setQrBootstrapState('error');
      setTableInfo(null);
      setSelectedBranch(null);
    }
  };

  const requestDirectGuestSession = async (branch: { id: string }) => {
    if (sessionStorage.getItem('guestDirectSession') === 'true' &&
      sessionStorage.getItem('guestDirectBranchId') === branch.id &&
      sessionStorage.getItem('guestToken')) {
      return;
    }

    const response = await fetchWithDiagnostics('/api/guest/direct-session', `${API_URL}/api/guest/direct-session`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ branchId: branch.id })
    }, 'direct-guest-session');

    const payload = await parseJsonWithDiagnostics(response, '/api/guest/direct-session', 'direct-guest-session');
    if (!response.ok || !payload?.token || !payload?.branch?.id) {
      throw new Error(payload?.message || 'Không thể xác định chi nhánh phục vụ.');
    }

    sessionStorage.removeItem('guestQrSession');
    sessionStorage.setItem('guestToken', payload.token);
    sessionStorage.setItem('guestDirectSession', 'true');
    sessionStorage.setItem('guestDirectBranchId', payload.branch.id);
  };

  const ensureDirectGuestSession = async (branch: { id: string }) => {
    if (directGuestSessionRequest.current) {
      return directGuestSessionRequest.current;
    }
    const request = requestDirectGuestSession(branch);
    directGuestSessionRequest.current = request;
    try {
      await request;
    } finally {
      if (directGuestSessionRequest.current === request) {
        directGuestSessionRequest.current = null;
      }
    }
  };

  const handleBranchSelect = async (branch: any) => {
    if (!branch?.id) return;

    setSelectedBranch(branch);
    setFeedbackMessage('');
    try {
      // A direct guest receives the same server-issued branch context used by
      // QR guests before the menu and AI requests are made. Registered
      // customers keep their existing account token and branch selection.
      if (!qrToken && localStorage.getItem('customerToken')) {
        // This is only a UI preference. The API re-validates the branch before
        // placing it in the authenticated AI context.
        localStorage.setItem('selectedBranchId', branch.id);
      }
      await fetchProducts(branch.id);
    } catch (error) {
      setFeedbackTone('error');
      setFeedbackMessage(error instanceof Error ? error.message : 'Không thể xác định chi nhánh phục vụ.');
      setLoading(false);
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

  const handleToggleTopping = (topping: ProductOption) => {
    if (selectedToppings.some(item => item.name === topping.name)) {
      setSelectedToppings(selectedToppings.filter(item => item.name !== topping.name));
    } else {
      setSelectedToppings([...selectedToppings, topping]);
    }
  };

  const handleDecreaseCustomizingQuantity = () => {
    setCustomizingQuantity(Math.max(1, customizingQuantity - 1));
  };

  const handleIncreaseCustomizingQuantity = () => {
    setCustomizingQuantity(customizingQuantity + 1);
  };

  const parseProductOptions = (optionsJson?: string): ProductOption[] => {
    return optionsJson ? JSON.parse(optionsJson) : [];
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

  const handleDecreaseProduct = (product: Product) => {
    const item = cart.find(cartItem => cartItem.id === product.id);
    if (item) updateQuantityByItem(item, -1);
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

    const hasQrSession = Boolean(qrToken) &&
      qrBootstrapState === 'success' &&
      Boolean(tableInfo?.branchId) &&
      sessionStorage.getItem('guestQrSession') === 'true' &&
      Boolean(sessionStorage.getItem('guestToken'));
    if (qrToken && !hasQrSession) {
      setFeedbackMessage('Phiên QR chưa sẵn sàng. Vui lòng quét lại mã QR và thử lại.');
      return;
    }

    try {
      setIsSubmittingOrder(true);
      const hasRegisteredCustomer = Boolean(localStorage.getItem('customerToken'));
      if (!qrToken && !hasRegisteredCustomer) {
        await ensureDirectGuestSession(branch);
      }

      let customerInfo: any = {};
      try {
        customerInfo = JSON.parse(localStorage.getItem('customerInfo') || '{}');
      } catch {
        customerInfo = {};
      }
      const order = {
        // Normal-link customers submit the selected public branch as context.
        // QR orders deliberately omit it; the API overwrites context from the
        // authenticated guest table session instead of trusting this client.
        ...(!qrToken ? { branchId: branch.id, branchName: branch.name } : {}),
        totalAmount: totalAmount,
        paidAmount: 0,
        status: 'Đang xử lý',
        customerName: customerInfo.fullName || 'Khách tại bàn',
        customerPhone: customerInfo.phoneNumber,
        customerEmail: customerInfo.email,
        customerId: customerInfo.id,
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

      setCart([]);
      setIsBranchModalOpen(false);
      setIsOrderSuccess(true);
      setFeedbackMessage('');
      setTimeout(() => setIsOrderSuccess(false), 5000);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Lỗi đặt món. Vui lòng thử lại!';
      setFeedbackMessage(message);
    } finally {
      setIsSubmittingOrder(false);
    }
  };

  const handleRedeem = async (promo: any) => {
    const customerInfo = JSON.parse(localStorage.getItem('customerInfo') || '{}');
    if (!customerInfo.phoneNumber) {
      setFeedbackTone('warning');
      setFeedbackMessage('Vui lòng đăng nhập để đổi quà!');
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
        setFeedbackTone('success');
        setFeedbackMessage('Yêu cầu đổi quà đã được gửi! Vui lòng đợi nhân viên xác nhận.');
        setIsPromoModalOpen(false);
      }
    } catch (e) {
      console.error(e);
      setFeedbackTone('error');
      setFeedbackMessage('Lỗi khi đổi quà.');
    }
  };

  const filteredProducts = products.filter(p =>
    activeCategory === 'Tất cả' || p.category === activeCategory
  );

  return (
    <div className="customer-surface min-h-screen font-sans pb-24">
      {/* Header Image */}
      <div className="hero-sheen relative h-64 overflow-hidden bg-[#3e8069] sm:h-72">
        <div className="hero-orb hero-orb-one"></div><div className="hero-orb hero-orb-two"></div>
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 to-transparent"></div>
        <img
          src="https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=800"
          className="w-full h-full object-cover opacity-60"
          alt="Restaurant"
        />
        <div className="absolute bottom-6 left-6 text-white">
          <span className="mb-3 inline-block rounded-full border border-white/30 bg-white/15 px-3 py-1 text-[10px] font-black uppercase tracking-widest backdrop-blur-md">DOAN RESTAURANT</span>
          <h1 className="max-w-[18rem] text-3xl font-black tracking-tight drop-shadow-lg sm:text-4xl">
             {tableInfo ? `Bàn: ${tableInfo.name}` : 'Thực đơn điện tử'}
          </h1>
          {tableInfo?.branchName && <p className="mt-1 text-[10px] font-bold uppercase tracking-widest opacity-80">{tableInfo.branchName}</p>}
          <p className="mt-2 flex items-center text-xs font-medium opacity-90">
             <span className="mr-2 h-2 w-2 animate-pulse rounded-full bg-[#f7c76b]"></span> Món ngon mỗi ngày • Phục vụ tận tâm
          </p>
        </div>
        {promotions.length > 0 && (
          <button
            type="button"
            onClick={() => setIsPromoModalOpen(true)}
            aria-label="Xem ưu đãi và đổi quà"
            className="absolute top-6 right-6 bg-orange-500 text-white p-3 rounded-2xl shadow-lg gift-wiggle border-2 border-white/20"
          >
            <Gift size={24} />
          </button>
        )}
      </div>

      {debugMenu && (
        <DebugMenuPanel
          qrPresent={Boolean(qrToken)}
          bootstrapState={!qrToken ? 'idle' : qrBootstrapState === 'loading' ? 'pending' : qrBootstrapState}
          productState={productFetchState}
          toppingState={toppingFetchState}
          productCountBeforeFilter={productCountBeforeFilter}
          productCountAfterFilter={productCountAfterFilter}
        />
      )}

      {feedbackMessage && (
        <div className="mx-4 mt-4" role="status">
          <Feedback tone={feedbackTone} onDismiss={() => setFeedbackMessage('')}>{feedbackMessage}</Feedback>
        </div>
      )}

      {false && !qrToken && !selectedBranch && branches.length > 0 && (
        <section className="mx-4 mt-4 rounded-3xl border border-blue-100 bg-blue-50 p-5" aria-label="Chọn chi nhánh xem thực đơn">
          <div className="mb-3 flex items-center gap-2 text-blue-800">
            <MapPin size={18} aria-hidden="true" />
            <h2 className="text-sm font-black">Chọn chi nhánh để xem thực đơn</h2>
          </div>
          <div className="grid gap-2 sm:grid-cols-2">
            {branches.map(branch => (
              <button
                key={branch.id}
                type="button"
                onClick={() => void handleBranchSelect(branch)}
                className="min-h-11 rounded-2xl border border-blue-200 bg-white px-4 py-3 text-left text-xs font-black text-slate-700 transition hover:border-blue-500 hover:text-blue-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
              >
                {branch.name}
              </button>
            ))}
          </div>
        </section>
      )}

      {qrToken && qrBootstrapState === 'loading' && (
        <div className="mx-4 mt-4">
          <Feedback tone="info">Đang xác định bàn và chi nhánh từ mã QR...</Feedback>
        </div>
      )}
      {qrToken && qrBootstrapState === 'success' && tableInfo && (
        <div className="mx-4 mt-4">
          <Feedback tone="success">Đã xác định {tableInfo.name} tại {tableInfo.branchName || 'chi nhánh phục vụ'}.</Feedback>
        </div>
      )}
      {qrToken && qrBootstrapState === 'error' && (
        <div className="mx-4 mt-4">
          <Feedback tone="error">Không thể xác định bàn từ mã QR. Vui lòng thử lại bằng mã QR hợp lệ.</Feedback>
        </div>
      )}

      <CustomerCategoryTabs
        categories={categories}
        activeCategory={activeCategory}
        onCategoryChange={setActiveCategory}
      />

      <CustomerProductList
        products={filteredProducts}
        cart={cart}
        activeCategory={activeCategory}
        loading={loading}
        onRefresh={() => void fetchProducts()}
        onAddProduct={addToCart}
        onRemoveProduct={removeFromCart}
        onDecreaseProduct={handleDecreaseProduct}
      />

      {/* Floating Order Success Toast */}
      {isOrderSuccess && (
        <div className="fixed top-24 left-6 right-6 z-[100] bg-green-600 text-white p-4 rounded-2xl shadow-2xl flex items-center space-x-3 animate-in slide-in-from-top-10 duration-500">
           <div className="bg-white/20 p-2 rounded-full"><CheckCircle2 size={24}/></div>
           <div role="status">
              <p className="font-black uppercase text-xs tracking-widest">Đặt món thành công!</p>
              <p className="text-[10px] opacity-80">Đơn của bạn đã được ghi nhận.</p>
           </div>
        </div>
      )}

      {/* Floating Cart Button */}
      {cart.length > 0 && (
        <div className={`fixed bottom-[5.2rem] left-3 right-3 z-50 flex items-center justify-between gap-3 rounded-[1.6rem] border-b-4 border-orange-800 bg-orange-600 p-3 text-white shadow-2xl sm:left-6 sm:right-6 sm:p-5 safe-bottom ${cartPulse ? 'cart-pulse' : ''}`}>
          <div className="min-w-0 flex items-center">
            <div className="bg-white/20 p-2 sm:p-3 rounded-2xl mr-3 sm:mr-4 relative shrink-0">
              <ShoppingBag size={24} />
              <span className="absolute -right-1 -top-1 flex h-5 w-5 items-center justify-center rounded-full border-2 border-orange-600 bg-white text-[10px] font-black text-orange-600">{totalItems}</span>
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
              <p className="font-black text-lg sm:text-xl tracking-tighter italic">{totalAmount.toLocaleString()}đ</p>
            </div>
          </div>
          <button
            type="button"
            onClick={handleOrder}
            aria-label="Mở giỏ hàng và đặt món"
            className="flex min-h-11 shrink-0 items-center rounded-2xl bg-white px-4 py-3.5 text-xs font-black uppercase tracking-widest text-orange-600 shadow-lg transition-all active:scale-95 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white focus-visible:ring-offset-2 focus-visible:ring-offset-orange-600 sm:px-8"
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
           <div className="bg-white w-full max-w-sm rounded-[3rem] p-4 sm:p-8 shadow-2xl relative animate-in zoom-in-95 duration-300 max-h-[90dvh] flex flex-col">
              <button type="button" onClick={() => setIsPromoModalOpen(false)} aria-label="Đóng ưu đãi" className="absolute top-6 right-6 min-h-11 min-w-11 touch-manipulation text-gray-300 hover:text-gray-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><X size={20}/></button>

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
                          type="button"
                          onClick={() => {
                             const customerInfo = JSON.parse(localStorage.getItem('customerInfo') || '{}');
                             const points = customerInfo.loyaltyPoints || 0;
                             if (points < p.requiredPoints) {
                                setFeedbackTone('warning');
                                setFeedbackMessage(`Bạn cần thêm ${p.requiredPoints - points} điểm để đổi quà này!`);
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
                 type="button"
                 onClick={() => setIsPromoModalOpen(false)}
                 className="w-full py-4 bg-gray-100 text-gray-500 rounded-2xl font-black uppercase tracking-widest text-xs hover:bg-gray-200 transition-all flex-shrink-0"
              >
                 Đóng
              </button>
           </div>
        </div>
      )}

      {isBranchModalOpen && (
        <CustomerOrderConfirmationModal
          cart={cart}
          totalItems={totalItems}
          totalAmount={totalAmount}
          branches={branches}
          selectedBranch={selectedBranch}
          tableInfo={tableInfo}
          customerInfo={customerInfo}
          currentLoyaltyPoints={currentLoyaltyPoints}
          isSubmittingOrder={isSubmittingOrder}
          onClose={() => setIsBranchModalOpen(false)}
          onBranchSelect={handleBranchSelect}
          onNoteChange={updateNote}
          onRemoveItem={removeFromCart}
          onConfirm={confirmOrder}
        />
      )}

      {isOptionsModalOpen && currentCustomizingProduct && (
        <CustomerProductOptionsModal
          product={currentCustomizingProduct}
          selectedSize={selectedSize}
          selectedToppings={selectedToppings}
          customizingQuantity={customizingQuantity}
          totalAmount={currentCustomizingProduct.price + (selectedSize?.price || 0) + selectedToppings.reduce((sum, topping) => sum + topping.price, 0)}
          sizes={parseProductOptions(currentCustomizingProduct.sizesJson)}
          toppings={parseProductOptions(currentCustomizingProduct.toppingsJson)}
          onClose={() => setIsOptionsModalOpen(false)}
          onSizeChange={setSelectedSize}
          onToggleTopping={handleToggleTopping}
          onDecreaseQuantity={handleDecreaseCustomizingQuantity}
          onIncreaseQuantity={handleIncreaseCustomizingQuantity}
          onConfirm={handleConfirmOptions}
        />
      )}
    </div>
  );
};

export default DigitalMenu;
