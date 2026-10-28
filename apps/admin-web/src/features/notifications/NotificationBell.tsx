import React, { useEffect, useRef, useState } from 'react';
import { Bell, BellRing, Loader2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { API_URL } from '../../config';
import POSGiftConfirmationModal, { promotionIdFromOrder } from '../pos/components/POSGiftConfirmationModal';
import type { POSPendingOrderRequest } from '../pos/components/POSPendingRequests';
import { PENDING_ORDER_CHANGED_EVENT } from './pendingOrderQuery';
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
  showLabel?: boolean;
  label?: string;
  menuPlacement?: 'above' | 'right' | 'below';
  markRead: (id: string, branchId?: string | null) => Promise<void>;
  markAllRead: () => Promise<void>;
}

export default function NotificationBell({ notifications, pendingNotifications, unreadCount, pendingCount, loadingNotifications, loadingPending, error, historyPath, showPending = true, showLabel = false, label = 'Thông báo', menuPlacement = 'above', markRead: onMarkRead, markAllRead: onMarkAllRead }: Props) {
  const [open, setOpen] = useState(false);
  const [hovered, setHovered] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const [mutationError, setMutationError] = useState<string | null>(null);
  const [giftOrder, setGiftOrder] = useState<POSPendingOrderRequest | null>(null);
  const [giftSaving, setGiftSaving] = useState(false);
  const [handledGiftIds, setHandledGiftIds] = useState<Set<string>>(() => new Set());
  const visibleNotifications = notifications.filter(item => !(item.type === 'GIFT_REDEEM_REQUEST' && item.entityId && handledGiftIds.has(item.entityId)));
  const visiblePendingNotifications = pendingNotifications.filter(item => !(item.type === 'gift' && handledGiftIds.has(item.id)));
  const visibleUnreadCount = Math.max(0, unreadCount - (notifications.length - visibleNotifications.length));
  const visiblePendingCount = Math.max(0, pendingCount - (pendingNotifications.length - visiblePendingNotifications.length));
  useEffect(() => {
    if (!open) return;
    const closeWhenOutside = (event: PointerEvent) => {
      if (rootRef.current && !rootRef.current.contains(event.target as Node)) {
        setOpen(false);
        setHovered(false);
      }
    };
    document.addEventListener('pointerdown', closeWhenOutside);
    return () => document.removeEventListener('pointerdown', closeWhenOutside);
  }, [open]);
  const markRead = async (item: NotificationItem) => {
    try { setMutationError(null); await onMarkRead(item.id, item.branchId); } catch (reason) { setMutationError(reason instanceof Error ? reason.message : 'Không thể cập nhật thông báo.'); }
  };
  const markAll = async () => {
    try { setMutationError(null); await onMarkAllRead(); } catch (reason) { setMutationError(reason instanceof Error ? reason.message : 'Không thể cập nhật thông báo.'); }
  };
  const closeGift = () => { if (!giftSaving) setGiftOrder(null); };
  const markGiftEntityRead = async (orderId: string) => {
    await fetch(`${API_URL}/api/Notification/entity-read`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ entityId: orderId, entityType: 'Order' })
    });
  };
  const refreshGiftSources = () => {
    window.dispatchEvent(new CustomEvent('refresh-notifications'));
    window.dispatchEvent(new Event(PENDING_ORDER_CHANGED_EVENT));
  };
  const confirmGift = async () => {
    if (!giftOrder || giftSaving) return;
    const promotionId = promotionIdFromOrder(giftOrder);
    if (!giftOrder.customerId || !promotionId) {
      setMutationError('Yêu cầu đổi quà thiếu thông tin chương trình hoặc khách hàng.');
      return;
    }
    try {
      setGiftSaving(true);
      const response = await fetch(`${API_URL}/api/Order/${giftOrder.id}/redeem-gift`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ customerId: giftOrder.customerId, promotionId })
      });
      const body = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(body.message || 'Không thể xác nhận đổi quà.');
      await markGiftEntityRead(giftOrder.id);
      setHandledGiftIds(previous => new Set(previous).add(giftOrder.id));
      setGiftOrder(null);
      refreshGiftSources();
    } catch (reason) {
      setMutationError(reason instanceof Error ? reason.message : 'Không thể xác nhận đổi quà.');
    } finally {
      setGiftSaving(false);
    }
  };
  const rejectGift = async () => {
    if (!giftOrder || giftSaving) return;
    try {
      setGiftSaving(true);
      const response = await fetch(`${API_URL}/api/Order/${giftOrder.id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: 'Đã hủy' })
      });
      if (!response.ok) throw new Error('Không thể từ chối yêu cầu đổi quà.');
      await markGiftEntityRead(giftOrder.id);
      setHandledGiftIds(previous => new Set(previous).add(giftOrder.id));
      setGiftOrder(null);
      refreshGiftSources();
    } catch (reason) {
      setMutationError(reason instanceof Error ? reason.message : 'Không thể từ chối yêu cầu đổi quà.');
    } finally {
      setGiftSaving(false);
    }
  };
  const openGift = (item: PendingWebOrderNotification) => {
    setMutationError(null);
    setGiftOrder({
      id: item.id,
      customerId: item.customerId || undefined,
      customerName: item.customerName || undefined,
      status: item.status || 'Đổi quà',
      totalAmount: item.totalAmount || 0,
      details: item.details?.map(detail => ({
        productName: detail.productName || undefined,
        quantity: detail.quantity,
        unitPrice: detail.unitPrice,
        options: detail.options || undefined
      }))
    });
  };
  const openPending = (item: PendingWebOrderNotification, event: React.MouseEvent<HTMLAnchorElement>) => {
    if (item.type !== 'gift') return;
    event.preventDefault();
    setOpen(false);
    openGift(item);
  };
  const openUnread = (item: NotificationItem, event: React.MouseEvent<HTMLAnchorElement>) => {
    const pendingGift = item.type === 'GIFT_REDEEM_REQUEST' && item.entityId
      ? visiblePendingNotifications.find(candidate => candidate.id === item.entityId && candidate.type === 'gift')
      : undefined;
    if (!pendingGift) {
      void markRead(item);
      setOpen(false);
      return;
    }
    event.preventDefault();
    void markRead(item);
    setOpen(false);
    openGift(pendingGift);
  };
  return <div ref={rootRef} className="relative" onPointerEnter={event => { if (event.pointerType !== 'touch') setHovered(true); }} onPointerLeave={event => { if (event.pointerType !== 'touch') setHovered(false); }}>
    <button type="button" onClick={() => { setOpen(value => !value); setHovered(false); }} className={`relative flex items-center gap-2 rounded-xl p-2 transition-colors hover:bg-blue-600 lg:w-full ${showLabel ? 'lg:justify-start lg:px-3' : 'lg:justify-center'} ${open ? 'bg-blue-700' : ''}`} aria-expanded={open} aria-label={`${visibleUnreadCount} thông báo chưa đọc, ${visiblePendingCount} đơn cần xử lý`}>
      <Bell size={20} />
      {showLabel && <span className="hidden text-[12px] font-bold uppercase tracking-widest lg:inline">{label}</span>}
      {(visibleUnreadCount + (showPending ? visiblePendingCount : 0)) > 0 && <span className="absolute -right-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full border-2 border-[#0070f4] bg-red-500 px-0.5 text-[9px] font-black">{visibleUnreadCount + (showPending ? visiblePendingCount : 0) > 99 ? '99+' : visibleUnreadCount + (showPending ? visiblePendingCount : 0)}</span>}
    </button>
    {(open || hovered) && <div className={`notification-popover absolute z-[100] max-h-[calc(100vh-2rem)] w-[min(22rem,calc(100vw-1rem))] overflow-y-auto rounded-2xl border border-gray-100 bg-white text-gray-800 shadow-2xl ${menuPlacement === 'right' ? 'bottom-0 left-full ml-0' : menuPlacement === 'below' ? 'top-full right-0 mt-2' : 'bottom-full right-0 mb-2'}`}>
      <div className="flex items-center justify-between border-b bg-gray-50 p-4"><div className="flex items-center font-black text-sm"><BellRing className="mr-2 h-4 w-4 text-blue-600" /> Thông báo</div><Link to={historyPath} onClick={() => setOpen(false)} className="text-[10px] font-black uppercase text-blue-600">Xem tất cả</Link></div>
      {error && <div className="border-b border-amber-200 bg-amber-50 px-4 py-2 text-xs text-amber-800">{error}</div>}
      {mutationError && <div className="border-b border-red-200 bg-red-50 px-4 py-2 text-xs text-red-800">{mutationError}</div>}
      <div className="max-h-[28rem] overflow-y-auto">
        <div className="border-b"><div className="flex items-center justify-between px-4 py-2 text-[10px] font-black uppercase tracking-widest text-gray-400"><span>Chưa đọc</span>{visibleUnreadCount > 0 && <button type="button" onClick={() => void markAll()} className="text-blue-600">Đánh dấu tất cả đã đọc</button>}</div>{loadingNotifications && visibleNotifications.length === 0 ? <div className="p-8 text-center"><Loader2 className="mx-auto animate-spin text-blue-600" /></div> : visibleNotifications.length === 0 ? <p className="px-4 pb-5 text-center text-xs text-gray-400">Không có thông báo chưa đọc</p> : visibleNotifications.map(item => <Link key={item.id} to={targetPath(item)} onClick={event => openUnread(item, event)} className="block border-t px-4 py-3 hover:bg-blue-50"><div className="flex justify-between gap-2"><p className="text-xs font-black">{item.title}</p><time className="shrink-0 text-[9px] text-gray-400">{new Date(item.createdAt).toLocaleString('vi-VN')}</time></div><p className="mt-1 text-[11px] text-gray-500">{item.message}</p>{item.branchName && <p className="mt-1 text-[10px] font-bold text-blue-600">{item.branchName}</p>}</Link>)}</div>
        <div><div className="flex items-center justify-between px-4 py-2 text-[10px] font-black uppercase tracking-widest text-gray-400"><span>Đơn cần xử lý</span><span className="text-red-600">{visiblePendingCount}</span></div>{loadingPending && visiblePendingNotifications.length === 0 ? <div className="p-5 text-center"><Loader2 className="mx-auto animate-spin text-blue-600" /></div> : visiblePendingNotifications.length === 0 ? <p className="px-4 pb-5 text-center text-xs text-gray-400">Không có đơn đang chờ</p> : visiblePendingNotifications.map(item => <Link key={`pending-${item.id}`} to={item.link} onClick={event => openPending(item, event)} className="block border-t px-4 py-3 hover:bg-orange-50"><p className="text-xs font-black">{item.title}</p><p className="mt-1 text-[11px] text-gray-500">{item.message}</p></Link>)}</div>
      </div>
    </div>}
    <POSGiftConfirmationModal order={giftOrder} isSaving={giftSaving} onClose={closeGift} onConfirm={() => void confirmGift()} onReject={() => void rejectGift()} />
  </div>;
}
