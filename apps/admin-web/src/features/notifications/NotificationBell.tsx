import React, { useState } from 'react';
import { Bell, BellRing, Loader2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import type { PendingWebOrderNotification } from './pendingOrderQuery';
import type { NotificationItem } from './useNotificationCenter';

const targetPath = (item: NotificationItem) => {
  if (!item.route) return '#';
  if (!item.entityId) return item.route;
  const separator = item.route.includes('?') ? '&' : '?';
  if (item.entityType?.toLowerCase() === 'businessinsight') return `${item.route}${separator}insightId=${encodeURIComponent(item.entityId)}`;
  return item.route;
};

interface Props {
  notifications: NotificationItem[];
  pendingNotifications: PendingWebOrderNotification[];
  unreadCount: number;
  pendingCount: number;
  loadingNotifications: boolean;
  loadingPending: boolean;
  error: string | null;
  historyPath: string;
  showPending?: boolean;
  markRead: (id: string, branchId?: string | null) => Promise<void>;
  markAllRead: () => Promise<void>;
}

export default function NotificationBell({ notifications, pendingNotifications, unreadCount, pendingCount, loadingNotifications, loadingPending, error, historyPath, showPending = true, markRead: onMarkRead, markAllRead: onMarkAllRead }: Props) {
  const [open, setOpen] = useState(false);
  const [mutationError, setMutationError] = useState<string | null>(null);
  const markRead = async (item: NotificationItem) => {
    try { setMutationError(null); await onMarkRead(item.id, item.branchId); } catch (reason) { setMutationError(reason instanceof Error ? reason.message : 'Không thể cập nhật thông báo.'); }
  };
  const markAll = async () => {
    try { setMutationError(null); await onMarkAllRead(); } catch (reason) { setMutationError(reason instanceof Error ? reason.message : 'Không thể cập nhật thông báo.'); }
  };
  return <div className="relative">
    <button type="button" onClick={() => setOpen(value => !value)} className={`relative rounded-xl p-2 transition-colors hover:bg-blue-600 ${open ? 'bg-blue-700' : ''}`} aria-label={`${unreadCount} thông báo chưa đọc, ${pendingCount} đơn cần xử lý`}>
      <Bell size={20} />
      {unreadCount > 0 && <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full border-2 border-[#0070f4] bg-red-500 px-0.5 text-[9px] font-black">{unreadCount > 99 ? '99+' : unreadCount}</span>}
    </button>
    {open && <div className="absolute right-0 z-50 mt-2 w-[min(22rem,calc(100vw-1rem))] overflow-hidden rounded-2xl border border-gray-100 bg-white text-gray-800 shadow-2xl">
      <div className="flex items-center justify-between border-b bg-gray-50 p-4"><div className="flex items-center font-black text-sm"><BellRing className="mr-2 h-4 w-4 text-blue-600" /> Thông báo</div><Link to={historyPath} onClick={() => setOpen(false)} className="text-[10px] font-black uppercase text-blue-600">Xem tất cả</Link></div>
      {error && <div className="border-b border-amber-200 bg-amber-50 px-4 py-2 text-xs text-amber-800">{error}</div>}
      {mutationError && <div className="border-b border-red-200 bg-red-50 px-4 py-2 text-xs text-red-800">{mutationError}</div>}
      <div className="max-h-[28rem] overflow-y-auto">
        <div className="border-b"><div className="flex items-center justify-between px-4 py-2 text-[10px] font-black uppercase tracking-widest text-gray-400"><span>Chưa đọc</span>{unreadCount > 0 && <button type="button" onClick={() => void markAll()} className="text-blue-600">Đánh dấu tất cả đã đọc</button>}</div>{loadingNotifications && notifications.length === 0 ? <div className="p-8 text-center"><Loader2 className="mx-auto animate-spin text-blue-600" /></div> : notifications.length === 0 ? <p className="px-4 pb-5 text-center text-xs text-gray-400">Không có thông báo chưa đọc</p> : notifications.map(item => <Link key={item.id} to={targetPath(item)} onClick={() => { void markRead(item); setOpen(false); }} className="block border-t px-4 py-3 hover:bg-blue-50"><div className="flex justify-between gap-2"><p className="text-xs font-black">{item.title}</p><time className="shrink-0 text-[9px] text-gray-400">{new Date(item.createdAt).toLocaleString('vi-VN')}</time></div><p className="mt-1 text-[11px] text-gray-500">{item.message}</p>{item.branchName && <p className="mt-1 text-[10px] font-bold text-blue-600">{item.branchName}</p>}</Link>)}</div>
        <div><div className="flex items-center justify-between px-4 py-2 text-[10px] font-black uppercase tracking-widest text-gray-400"><span>Đơn cần xử lý</span><span className="text-red-600">{pendingCount}</span></div>{loadingPending && pendingNotifications.length === 0 ? <div className="p-5 text-center"><Loader2 className="mx-auto animate-spin text-blue-600" /></div> : pendingNotifications.length === 0 ? <p className="px-4 pb-5 text-center text-xs text-gray-400">Không có đơn đang chờ</p> : pendingNotifications.map(item => <Link key={`pending-${item.id}`} to={item.link} onClick={() => setOpen(false)} className="block border-t px-4 py-3 hover:bg-orange-50"><p className="text-xs font-black">{item.title}</p><p className="mt-1 text-[11px] text-gray-500">{item.message}</p></Link>)}</div>
      </div>
    </div>}
  </div>;
}
