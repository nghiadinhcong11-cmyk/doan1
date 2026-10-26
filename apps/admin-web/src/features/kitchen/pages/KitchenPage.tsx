import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { CheckCircle2, Clock, Loader2, PlayCircle, RefreshCw, Signal, Utensils, WifiOff, X } from 'lucide-react';
import { API_URL } from '../../../config';
import { getCachedJson, invalidateCachedJson } from '../../../api/queryCache';
import { Feedback, StatusBadge } from '../../../components/ui';
import { fetchBranchList } from '../../../api/branchQuery';

interface OrderRequestItem { productName: string; quantity: number; options?: string; }
interface KitchenProduct { id: string; name: string; code?: string; availabilityStatus?: string; isActive: boolean; }
type RequestStatus = 'Pending' | 'Preparing' | 'Ready' | 'Completed' | 'Cancelled';
interface OrderRequest { id: string; orderId: string; requestNumber: number; status: RequestStatus; createdAt: string; tableName: string; items: OrderRequestItem[]; }
interface KitchenBranch { id: string; name: string; }
const ACTIVE_STATUSES: RequestStatus[] = ['Pending', 'Preparing', 'Ready'];

const KitchenPage = ({ userRole }: { userRole?: 'admin' | 'manager' }) => {
  const [requests, setRequests] = useState<OrderRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [connected, setConnected] = useState(false);
  const [busyIds, setBusyIds] = useState<Set<string>>(new Set());
  const [error, setError] = useState('');
  const [products, setProducts] = useState<KitchenProduct[]>([]);
  const [branches, setBranches] = useState<KitchenBranch[]>([]);
  const [selectedBranchId, setSelectedBranchId] = useState(() => localStorage.getItem('selectedBranchId') || '');
  const [branchReady, setBranchReady] = useState(() => userRole !== 'admin' || Boolean(localStorage.getItem('selectedBranchId')));
  const lastSyncRef = useRef(0);
  const connectedRef = useRef(false);
  const activeRequestsInFlightRef = useRef(false);
  const activeRequestsRefreshQueuedRef = useRef(false);

  useEffect(() => {
    if (userRole !== 'admin') return;
    void fetchBranchList().then(data => {
      const branchList = data as KitchenBranch[];
      setBranches(branchList);
      const savedId = localStorage.getItem('selectedBranchId');
      const selected = branchList.find(branch => branch.id === savedId) || branchList[0];
      if (selected) {
        localStorage.setItem('selectedBranchId', selected.id);
        setSelectedBranchId(selected.id);
        setBranchReady(true);
      }
    }).catch(() => {
      setBranches([]);
      // Keep a previously selected branch usable if the branch-list request
      // temporarily fails; never fall back to an all-branch kitchen query.
      setBranchReady(Boolean(localStorage.getItem('selectedBranchId')));
    });
  }, [userRole]);

  const handleBranchChange = (branchId: string) => {
    setSelectedBranchId(branchId);
    localStorage.setItem('selectedBranchId', branchId);
    setRequests([]);
    setProducts([]);
    setLoading(true);
    window.dispatchEvent(new Event('branch:changed'));
  };

  const fetchActiveRequests = useCallback(async (silent = false, isOwnerActive: () => boolean = () => true) => {
    if (activeRequestsInFlightRef.current) return;
    activeRequestsInFlightRef.current = true;
    try {
      if (!silent) setRefreshing(true);
      const branchId = selectedBranchId || localStorage.getItem('selectedBranchId');
      if (!branchId) return;
      const query = branchId ? `?branchId=${encodeURIComponent(branchId)}` : '';
      const response = await fetch(`${API_URL}/api/Order/kitchen/active-requests${query}`);
      if (!response.ok) throw new Error('Không thể tải danh sách bếp.');
      const data: OrderRequest[] = await response.json();
      if (!isOwnerActive()) return;
      setRequests(data); setError(''); lastSyncRef.current = Date.now();
    } catch (err) {
      if (isOwnerActive()) {
        console.error('Kitchen sync failed:', err);
        setError('Chưa đồng bộ được dữ liệu. Hãy thử làm mới lại.');
      }
    }
    finally {
      if (isOwnerActive()) {
        setLoading(false);
        setRefreshing(false);
      }
      activeRequestsInFlightRef.current = false;
    }
  }, []);

  const requestActiveRequestsRefresh = useCallback((silent = true, allowTrailing = false, isOwnerActive: () => boolean = () => true) => {
    if (activeRequestsInFlightRef.current) {
      if (allowTrailing) activeRequestsRefreshQueuedRef.current = true;
      return;
    }
    void fetchActiveRequests(silent, isOwnerActive).finally(() => {
      if (activeRequestsRefreshQueuedRef.current && isOwnerActive() && document.visibilityState === 'visible') {
        activeRequestsRefreshQueuedRef.current = false;
        requestActiveRequestsRefresh(silent, true, isOwnerActive);
      } else {
        activeRequestsRefreshQueuedRef.current = false;
      }
    });
  }, [fetchActiveRequests]);

  const fetchProducts = useCallback(async (isOwnerActive: () => boolean = () => true) => {
    try {
      const branchId = selectedBranchId || localStorage.getItem('selectedBranchId');
      if (!branchId) return;
      const query = new URLSearchParams({ isActive: 'true' });
      if (branchId) query.set('branchId', branchId);
      const data = await getCachedJson<KitchenProduct[]>(`${API_URL}/api/Product?${query.toString()}`);
      if (isOwnerActive()) setProducts(data);
    } catch (err) {
      if (isOwnerActive()) console.error('Product availability sync failed:', err);
    }
  }, []);

  const updateAvailability = async (product: KitchenProduct, status: string) => {
    try {
      const branchId = localStorage.getItem('selectedBranchId');
      const query = branchId ? `?branchId=${encodeURIComponent(branchId)}` : '';
      const response = await fetch(`${API_URL}/api/Product/${product.id}/availability${query}`, {
        method: 'PATCH', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ status })
      });
      if (!response.ok) throw new Error('Không thể cập nhật trạng thái món.');
      invalidateCachedJson('/api/Product');
      setProducts(prev => prev.map(item => item.id === product.id ? { ...item, availabilityStatus: status } : item));
    } catch (err: any) { setError(err.message || 'Không thể cập nhật trạng thái món.'); }
  };

  useEffect(() => {
    if (!branchReady) return;
    let disposed = false;
    const branchQuery = selectedBranchId ? `?branchId=${encodeURIComponent(selectedBranchId)}` : '';
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/kitchenHub${branchQuery}`, { accessTokenFactory: () => localStorage.getItem('adminToken') || '' })
      .withAutomaticReconnect([0, 2000, 5000, 10000]).build();
    const onNew = (payload: OrderRequest) => {
      if (disposed) return;
      if (!payload?.id || !ACTIVE_STATUSES.includes(payload.status)) return;
      const payloadBranchId = (payload as OrderRequest & { branchId?: string }).branchId;
      if (selectedBranchId && payloadBranchId && payloadBranchId !== selectedBranchId) return;
      setRequests(prev => prev.some(item => item.id === payload.id) ? prev.map(item => item.id === payload.id ? { ...item, ...payload } : item) : [...prev, payload]);
      void new Audio('/kitchen-order.mp3').play().catch(() => undefined);
    };
    const onStatus = ({ id, status }: { id: string; status: RequestStatus }) => {
      if (disposed) return;
      setRequests(prev => ACTIVE_STATUSES.includes(status) ? prev.map(item => item.id === id ? { ...item, status } : item) : prev.filter(item => item.id !== id));
    };
    const onAvailabilityChanged = ({ id, availabilityStatus, branchId }: { id: string; availabilityStatus: string; branchId: string }) => {
      if (disposed) return;
      const selectedBranchId = localStorage.getItem('selectedBranchId');
      if (!selectedBranchId || branchId !== selectedBranchId) return;
      setProducts(prev => prev.map(product => product.id === id ? { ...product, availabilityStatus } : product));
    };
    connection.on('NewOrderRequest', onNew); connection.on('RequestStatusUpdated', onStatus); connection.on('ProductAvailabilityChanged', onAvailabilityChanged);
    connection.onreconnecting(() => { connectedRef.current = false; if (!disposed) setConnected(false); });
    connection.onreconnected(() => { connectedRef.current = true; if (!disposed) { setConnected(true); requestActiveRequestsRefresh(true, true, () => !disposed); void fetchProducts(() => !disposed); } });
    connection.onclose(() => { connectedRef.current = false; if (!disposed) setConnected(false); });
    void fetchActiveRequests(false, () => !disposed);
    void fetchProducts(() => !disposed);
    void connection.start().then(() => {
      connectedRef.current = true;
      if (!disposed) setConnected(true);
    }).catch(err => {
      connectedRef.current = false;
      if (!disposed) {
        console.error('Kitchen hub failed:', err);
        setError('Realtime đang chờ khôi phục. Dữ liệu vẫn tự đồng bộ.');
      }
    });
    // Bù các sự kiện SignalR bị lỡ trong lúc mất mạng/reconnect.
    const refreshIfVisible = () => {
      if (!disposed && document.visibilityState === 'visible') requestActiveRequestsRefresh(true, true, () => !disposed);
    };
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') refreshIfVisible();
    };
    let pollTimer: number | undefined;
    const schedulePoll = () => {
      pollTimer = window.setTimeout(() => {
        if (disposed) return;
        if (document.visibilityState === 'visible') requestActiveRequestsRefresh(true, false, () => !disposed);
        schedulePoll();
      }, connectedRef.current ? 30000 : 10000);
    };
    schedulePoll();
    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => {
      disposed = true;
      if (pollTimer !== undefined) window.clearTimeout(pollTimer);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
      activeRequestsRefreshQueuedRef.current = false;
      connection.off('NewOrderRequest', onNew);
      connection.off('RequestStatusUpdated', onStatus);
      connection.off('ProductAvailabilityChanged', onAvailabilityChanged);
      void connection.stop().catch(err => console.warn('Kitchen hub cleanup failed.', err));
    };
  }, [branchReady, selectedBranchId, fetchActiveRequests, fetchProducts, requestActiveRequestsRefresh]);

  const updateStatus = async (request: OrderRequest, status: RequestStatus) => {
    if (busyIds.has(request.id)) return;
    setBusyIds(prev => new Set(prev).add(request.id)); setError('');
    setRequests(prev => status === 'Completed' || status === 'Cancelled' ? prev.filter(item => item.id !== request.id) : prev.map(item => item.id === request.id ? { ...item, status } : item));
    try {
      const response = await fetch(`${API_URL}/api/Order/kitchen/requests/${request.id}/status`, { method: 'PATCH', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(status) });
      if (!response.ok) { const body = await response.json().catch(() => ({})); throw new Error(body.message || 'Máy chủ chưa nhận thao tác.'); }
    } catch (err: any) { setError(err.message || 'Không thể cập nhật trạng thái đơn.'); await fetchActiveRequests(true); }
    finally { setBusyIds(prev => { const next = new Set(prev); next.delete(request.id); return next; }); }
  };

  const stats = useMemo(() => ({ pending: requests.filter(r => r.status === 'Pending').length, preparing: requests.filter(r => r.status === 'Preparing').length, ready: requests.filter(r => r.status === 'Ready').length }), [requests]);
  const meta = (status: RequestStatus): [string, string] => ({ Pending: ['Mới', 'bg-rose-50 text-rose-600 border-rose-200'], Preparing: ['Đang chế biến', 'bg-amber-50 text-amber-600 border-amber-200'], Ready: ['Sẵn sàng', 'bg-emerald-50 text-emerald-600 border-emerald-200'] } as Partial<Record<RequestStatus, [string, string]>>)[status] || ['Đã xong', 'bg-slate-50 text-slate-500 border-slate-200'];
  const statusTone = (status: RequestStatus) => ({ Pending: 'pending', Preparing: 'pending', Ready: 'completed', Completed: 'completed', Cancelled: 'cancelled' } as const)[status];
  if (loading && requests.length === 0) return <div className="flex h-screen items-center justify-center bg-slate-100"><Loader2 className="animate-spin text-orange-500" size={42} /></div>;

  return <div className="min-h-screen bg-[#f5f7fb] p-4 font-sans text-slate-800 md:p-7">
    <header className="mb-6 flex flex-wrap items-center justify-between gap-4"><div><div className="mb-2 flex items-center gap-2 text-xs font-bold uppercase tracking-[.2em] text-orange-500"><Utensils size={15} /> Kitchen command center</div><h1 className="text-3xl font-black tracking-tight text-slate-900">Điều phối bếp</h1><p className="mt-1 text-sm text-slate-500">Theo dõi món mới theo thời gian thực, không bỏ lỡ đơn.</p></div><div className="flex flex-wrap items-center justify-end gap-3">{userRole === 'admin' && <label className="flex items-center gap-2 rounded-2xl border border-slate-200 bg-white px-3 py-2.5 text-xs font-bold text-slate-600"><span>Chi nhánh</span><select value={selectedBranchId} onChange={event => handleBranchChange(event.target.value)} className="max-w-[170px] bg-transparent outline-none"><option value="">Chọn chi nhánh</option>{branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>}<span className={`flex items-center gap-2 rounded-2xl border px-4 py-2.5 text-xs font-bold ${connected ? 'border-emerald-200 bg-emerald-50 text-emerald-600' : 'border-amber-200 bg-amber-50 text-amber-600'}`} role="status">{connected ? <Signal aria-hidden="true" size={14} /> : <WifiOff aria-hidden="true" size={14} />} {connected ? 'Đang kết nối' : 'Đang đồng bộ'}</span><button type="button" onClick={() => void fetchActiveRequests()} disabled={refreshing} className="touch-manipulation flex min-h-11 items-center gap-2 rounded-2xl bg-blue-600 px-4 py-2.5 text-xs font-bold text-white shadow-lg transition hover:bg-blue-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:ring-offset-2 disabled:opacity-60" aria-label="Làm mới danh sách bếp"><RefreshCw aria-hidden="true" size={14} className={refreshing ? 'animate-spin' : ''} /> Làm mới</button></div></header>
    {error && <div className="mb-5"><Feedback tone="error" onDismiss={() => setError('')}>{error}</Feedback></div>}
    <section className="mb-7 rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><div className="mb-4 flex items-center justify-between"><div><h2 className="text-lg font-black text-slate-900">Trạng thái món</h2><p className="text-xs text-slate-400">Cập nhật món đang bán để khách không đặt nhầm.</p></div><button onClick={() => void fetchProducts()} className="text-xs font-bold text-blue-600">Làm mới</button></div><div className="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-3">{products.map(product => { const status = product.availabilityStatus || 'Available'; return <div key={product.id} className="flex items-center justify-between gap-3 rounded-2xl border border-slate-100 bg-slate-50 p-3"><div className="min-w-0"><p className="truncate text-sm font-bold">{product.name}</p><p className={`text-[10px] font-black uppercase ${status === 'Available' ? 'text-emerald-600' : status === 'OutOfStock' ? 'text-rose-600' : 'text-amber-600'}`}>{status === 'Available' ? 'Đang bán' : status === 'OutOfStock' ? 'Hết món' : 'Tạm hết'}</p></div><select value={status} onChange={e => void updateAvailability(product, e.target.value)} className="rounded-2xl border border-slate-200 bg-white px-2 py-2 text-[10px] font-bold"><option value="Available">Đang bán</option><option value="TemporarilyUnavailable">Tạm hết</option><option value="OutOfStock">Hết món</option></select></div>; })}</div></section>
    <section className="mb-7 grid grid-cols-3 gap-3 md:max-w-xl">{[['Đơn mới', stats.pending, 'text-rose-600'], ['Đang làm', stats.preparing, 'text-amber-600'], ['Sẵn sàng', stats.ready, 'text-emerald-600']].map(([label, count, color]) => <div key={label as string} className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm"><p className="text-[10px] font-bold uppercase tracking-wider text-slate-400">{label}</p><p className={`mt-1 text-2xl font-black ${color}`}>{count}</p></div>)}</section>
    {requests.length === 0 ? <div className="rounded-2xl border border-dashed border-slate-300 bg-white px-6 py-20 text-center"><Utensils size={52} className="mx-auto mb-4 text-slate-200" /><p className="font-bold text-slate-500">Bếp đang trống</p><p className="mt-1 text-sm text-slate-400">Đơn mới sẽ tự động xuất hiện ở đây.</p></div> : <div className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">{requests.map(request => { const [label, color] = meta(request.status); const busy = busyIds.has(request.id); return <article key={request.id} className="flex flex-col overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm transition hover:-translate-y-0.5 hover:shadow-xl"><div className={`border-b p-5 ${color}`}><div className="flex items-start justify-between gap-3"><div className="min-w-0"><h2 className="truncate text-xl font-black" title={request.tableName}>{request.tableName}</h2><p className="mt-1 text-[10px] font-bold uppercase tracking-widest opacity-70">Đợt gọi #{request.requestNumber}</p></div><StatusBadge tone={statusTone(request.status)}>{label}</StatusBadge></div><div className="mt-4 flex items-center gap-1 text-xs font-semibold opacity-75"><Clock aria-hidden="true" size={13} /> {new Date(request.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</div></div><div className="flex-1 space-y-4 p-5">{request.items.map((item, index) => <div key={`${request.id}-${index}`} className="flex items-start justify-between gap-3"><div className="min-w-0"><p className="break-words font-bold leading-tight text-slate-800">{item.productName}</p>{item.options && <p className="mt-1 break-words rounded-2xl bg-orange-50 px-2 py-1 text-xs font-medium italic text-orange-700">{item.options}</p>}</div><span className="shrink-0 rounded-2xl bg-slate-100 px-2.5 py-1 font-black text-orange-600" aria-label={`Số lượng ${item.quantity}`}>×{item.quantity}</span></div>)}</div><div className="flex gap-2 border-t border-slate-100 bg-slate-50 p-4">{request.status === 'Pending' && <button type="button" disabled={busy} onClick={() => void updateStatus(request, 'Preparing')} className="touch-manipulation flex min-h-11 flex-1 items-center justify-center gap-2 rounded-xl bg-orange-500 py-3 text-xs font-black text-white shadow-lg transition hover:bg-orange-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-500 focus-visible:ring-offset-2 disabled:cursor-wait disabled:opacity-60" aria-label={`Bắt đầu chế biến đợt gọi ${request.requestNumber}`}>{busy ? <Loader2 aria-hidden="true" className="animate-spin" size={16} /> : <PlayCircle aria-hidden="true" size={16} />} {busy ? 'Đang gửi...' : 'Bắt đầu chế biến'}</button>}{request.status === 'Preparing' && <button type="button" disabled={busy} onClick={() => void updateStatus(request, 'Ready')} className="touch-manipulation flex min-h-11 flex-1 items-center justify-center gap-2 rounded-xl bg-emerald-500 py-3 text-xs font-black text-white shadow-lg transition hover:bg-emerald-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-2 disabled:cursor-wait disabled:opacity-60" aria-label={`Báo sẵn sàng đợt gọi ${request.requestNumber}`}>{busy ? <Loader2 aria-hidden="true" className="animate-spin" size={16} /> : <CheckCircle2 aria-hidden="true" size={16} />} {busy ? 'Đang gửi...' : 'Báo sẵn sàng'}</button>}{request.status === 'Ready' && <button type="button" disabled={busy} onClick={() => void updateStatus(request, 'Completed')} className="touch-manipulation flex min-h-11 flex-1 items-center justify-center gap-2 rounded-xl bg-blue-600 py-3 text-xs font-black text-white shadow-lg transition hover:bg-blue-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 disabled:cursor-wait disabled:opacity-60" aria-label={`Xác nhận đã trả món đợt gọi ${request.requestNumber}`}>{busy ? <Loader2 aria-hidden="true" className="animate-spin" size={16} /> : <CheckCircle2 aria-hidden="true" size={16} />} {busy ? 'Đang gửi...' : 'Đã trả món'}</button>}<button type="button" disabled={busy} onClick={() => { if (window.confirm('Hủy yêu cầu này?')) void updateStatus(request, 'Cancelled'); }} aria-label={`Hủy đợt gọi ${request.requestNumber}`} title="Hủy yêu cầu" className="touch-manipulation min-h-11 min-w-11 rounded-xl border border-slate-200 bg-white px-3 text-rose-500 transition hover:bg-rose-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-rose-500 focus-visible:ring-offset-2 disabled:opacity-50"><X aria-hidden="true" size={17} /></button></div></article>; })}</div>}
    <p className="mt-6 text-center text-[11px] text-slate-400">Realtime qua SignalR · Đồng bộ dự phòng mỗi {connected ? 30 : 10} giây · Đồng bộ cuối: {lastSyncRef.current ? new Date(lastSyncRef.current).toLocaleTimeString('vi-VN') : '—'}</p>
  </div>;
};
export default KitchenPage;
