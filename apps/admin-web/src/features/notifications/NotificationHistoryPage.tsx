import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { Loader2, RefreshCw } from 'lucide-react';
import { API_URL } from '../../config';
import { fetchBranchList, type BranchSummary } from '../../api/branchQuery';
import type { NotificationItem } from './useNotificationCenter';

type HistoryRole = 'admin' | 'manager' | 'cashier' | 'employee' | 'kitchen';
type ReadFilter = 'all' | 'unread' | 'read';

interface Props { role: HistoryRole; }
interface PageResponse { items?: NotificationItem[]; total?: number; page?: number; pageSize?: number; }

const targetPath = (item: NotificationItem) => {
  if (!item.route) return '#';
  if (item.entityType?.toLowerCase() === 'businessinsight' && item.entityId) return `${item.route}${item.route.includes('?') ? '&' : '?'}insightId=${encodeURIComponent(item.entityId)}`;
  return item.route;
};

export default function NotificationHistoryPage({ role }: Props) {
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [readState, setReadState] = useState<ReadFilter>('all');
  const [type, setType] = useState('');
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [branchId, setBranchId] = useState(role === 'admin' ? localStorage.getItem('selectedBranchId') || '' : '');
  const [branches, setBranches] = useState<BranchSummary[]>([]);
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [mutationError, setMutationError] = useState<string | null>(null);
  const generation = useRef(0);
  const canChooseBranch = role === 'admin';
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  useEffect(() => {
    if (!canChooseBranch) return;
    void fetchBranchList().then(setBranches).catch(() => setBranches([]));
  }, [canChooseBranch]);

  const query = useMemo(() => {
    const params = new URLSearchParams({ readState, page: String(page), pageSize: String(pageSize) });
    if (type) params.set('type', type);
    if (from) params.set('from', new Date(`${from}T00:00:00`).toISOString());
    if (to) params.set('to', new Date(`${to}T23:59:59.999`).toISOString());
    if (role === 'admin' && branchId) params.set('branchId', branchId);
    return params.toString();
  }, [readState, page, type, from, to, branchId, role]);

  const load = useCallback(async (signal?: AbortSignal) => {
    const current = ++generation.current;
    setLoading(true);
    try {
      const response = await fetch(`${API_URL}/api/Notification?${query}`, { signal });
      if (!response.ok) throw new Error('Không thể tải lịch sử thông báo.');
      const data = await response.json() as PageResponse;
      if (current !== generation.current) return;
      setItems(Array.isArray(data.items) ? data.items : []);
      setTotal(typeof data.total === 'number' ? data.total : 0);
      setError(null);
    } catch (reason) {
      if ((reason as Error)?.name !== 'AbortError') setError(reason instanceof Error ? reason.message : 'Không thể tải lịch sử thông báo.');
    } finally {
      if (current === generation.current) setLoading(false);
    }
  }, [query]);

  useEffect(() => {
    const controller = new AbortController();
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  const markRead = async (item: NotificationItem) => {
    try {
      setMutationError(null);
      const suffix = item.branchId ? `?branchId=${encodeURIComponent(item.branchId)}` : '';
      const response = await fetch(`${API_URL}/api/Notification/${item.id}/read${suffix}`, { method: 'PATCH' });
      if (!response.ok) throw new Error('Không thể đánh dấu thông báo đã đọc.');
      await load();
    } catch (reason) { setMutationError(reason instanceof Error ? reason.message : 'Không thể cập nhật thông báo.'); }
  };

  const markAllRead = async () => {
    try {
      setMutationError(null);
      const suffix = role === 'admin' && branchId ? `?branchId=${encodeURIComponent(branchId)}` : '';
      const response = await fetch(`${API_URL}/api/Notification/read-all${suffix}`, { method: 'POST' });
      if (!response.ok) throw new Error('Không thể đánh dấu tất cả thông báo.');
      await load();
    } catch (reason) { setMutationError(reason instanceof Error ? reason.message : 'Không thể cập nhật thông báo.'); }
  };

  const resetPage = (setter: (value: string) => void, value: string) => { setter(value); setPage(1); };
  return <main className="mx-auto max-w-5xl p-4 sm:p-6">
    <div className="mb-5 flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-black text-gray-900">Lịch sử thông báo</h1><p className="text-sm text-gray-500">Thông báo theo tài khoản và phạm vi được phép.</p></div><button type="button" onClick={() => void load()} className="inline-flex items-center gap-2 rounded-lg border px-3 py-2 text-sm font-bold"><RefreshCw size={15} /> Làm mới</button></div>
    <div className="mb-4 flex flex-wrap gap-2">{(['all', 'unread', 'read'] as ReadFilter[]).map(value => <button key={value} type="button" onClick={() => { setReadState(value); setPage(1); }} className={`rounded-full px-4 py-2 text-xs font-black ${readState === value ? 'bg-blue-600 text-white' : 'bg-white text-gray-600 ring-1 ring-gray-200'}`}>{value === 'all' ? 'Tất cả' : value === 'unread' ? 'Chưa đọc' : 'Đã đọc'}</button>)}<button type="button" onClick={() => void markAllRead()} className="ml-auto rounded-full px-4 py-2 text-xs font-black text-blue-700 ring-1 ring-blue-200">Đánh dấu tất cả đã đọc</button></div>
    <div className="mb-5 grid grid-cols-1 gap-3 rounded-xl bg-white p-4 shadow-sm sm:grid-cols-2 lg:grid-cols-4"><label className="text-xs font-bold text-gray-500">Loại<select value={type} onChange={event => resetPage(setType, event.target.value)} className="mt-1 w-full rounded border p-2 text-sm"><option value="">Tất cả loại</option><option value="ORDER_CREATED">Đơn mới</option><option value="BusinessInsight">Insight</option><option value="OUT_OF_STOCK">Hết món</option></select></label><label className="text-xs font-bold text-gray-500">Từ ngày<input type="date" value={from} onChange={event => resetPage(setFrom, event.target.value)} className="mt-1 w-full rounded border p-2 text-sm" /></label><label className="text-xs font-bold text-gray-500">Đến ngày<input type="date" value={to} onChange={event => resetPage(setTo, event.target.value)} className="mt-1 w-full rounded border p-2 text-sm" /></label>{canChooseBranch ? <label className="text-xs font-bold text-gray-500">Chi nhánh<select value={branchId} onChange={event => resetPage(setBranchId, event.target.value)} className="mt-1 w-full rounded border p-2 text-sm"><option value="">Phạm vi global</option>{branches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label> : <div className="flex items-end text-xs text-gray-500">Chi nhánh theo tài khoản hiện tại</div>}</div>
    {error && <div className="mb-3 rounded-lg bg-amber-50 p-3 text-sm text-amber-800">{error}</div>}{mutationError && <div className="mb-3 rounded-lg bg-red-50 p-3 text-sm text-red-800">{mutationError}</div>}
    <section className="overflow-hidden rounded-xl bg-white shadow-sm">{loading ? <div className="p-12 text-center"><Loader2 className="mx-auto animate-spin text-blue-600" /></div> : items.length === 0 ? <div className="p-12 text-center text-sm text-gray-500">Không có thông báo phù hợp.</div> : <div>{items.map(item => <article key={item.id} className="border-b p-4 last:border-b-0"><div className="flex flex-wrap items-start justify-between gap-2"><div><h2 className="font-black text-gray-900">{item.title}</h2><p className="mt-1 text-sm text-gray-600">{item.message}</p></div><span className={`rounded-full px-2 py-1 text-[10px] font-black ${item.isRead ? 'bg-gray-100 text-gray-500' : 'bg-blue-100 text-blue-700'}`}>{item.isRead ? 'Đã đọc' : 'Chưa đọc'}</span></div><div className="mt-3 flex flex-wrap items-center gap-3 text-xs text-gray-500"><time>{new Date(item.createdAt).toLocaleString('vi-VN')}</time>{item.branchName && <span className="font-bold text-blue-600">{item.branchName}</span>}{!item.isRead && <button type="button" onClick={() => void markRead(item)} className="font-bold text-blue-600">Đánh dấu đã đọc</button>}{item.route ? <Link to={targetPath(item)} className="font-bold text-blue-600">Mở đối tượng</Link> : <span className="italic">Đối tượng không còn đường dẫn</span>}</div></article>)}</div>}</section>
    <div className="mt-4 flex items-center justify-between text-sm"><button type="button" disabled={page <= 1} onClick={() => setPage(value => value - 1)} className="rounded border px-3 py-2 disabled:opacity-40">Trước</button><span>Trang {page}/{totalPages} · {total} thông báo</span><button type="button" disabled={page >= totalPages} onClick={() => setPage(value => value + 1)} className="rounded border px-3 py-2 disabled:opacity-40">Sau</button></div>
  </main>;
}
