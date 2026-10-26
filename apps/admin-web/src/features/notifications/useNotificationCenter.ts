import { useCallback, useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { API_URL } from '../../config';
import { fetchPendingWebOrderCount, fetchPendingWebOrderNotifications, PENDING_ORDER_CHANGED_EVENT, type PendingWebOrderNotification } from './pendingOrderQuery';

export interface NotificationItem {
  id: string;
  type: string;
  title: string;
  message: string;
  entityType?: string | null;
  entityId?: string | null;
  route?: string | null;
  branchId?: string | null;
  branchName?: string | null;
  createdAt: string;
  isRead: boolean;
}

type NotificationRole = 'admin' | 'manager' | 'cashier' | 'employee' | 'kitchen';

const branchScope = () => localStorage.getItem('selectedBranchId') || '';
const authScope = () => `${localStorage.getItem('adminToken') || ''}|${localStorage.getItem('userRole') || ''}|${branchScope()}`;

let notificationSnapshotInFlight: { scope: string; promise: Promise<{ items: NotificationItem[]; count: number }> } | null = null;

const fetchNotificationSnapshot = async (listQuery: string, countQuery: string, scope: string) => {
  if (notificationSnapshotInFlight?.scope === scope) return notificationSnapshotInFlight.promise;
  const promise = (async () => {
    const [listResponse, countResponse] = await Promise.all([
      fetch(`${API_URL}/api/Notification?${listQuery}`),
      fetch(`${API_URL}/api/Notification/count?${countQuery}`)
    ]);
    if (!listResponse.ok || !countResponse.ok) throw new Error('Notification request failed');
    const list = await listResponse.json() as { items?: NotificationItem[] };
    const count = await countResponse.json() as { count?: number };
    return { items: Array.isArray(list.items) ? list.items : [], count: typeof count.count === 'number' ? count.count : 0 };
  })();
  notificationSnapshotInFlight = { scope, promise };
  try {
    return await promise;
  } finally {
    if (notificationSnapshotInFlight?.promise === promise) notificationSnapshotInFlight = null;
  }
};

export const useNotificationCenter = (role: NotificationRole) => {
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [pendingNotifications, setPendingNotifications] = useState<PendingWebOrderNotification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [pendingCount, setPendingCount] = useState(0);
  const [loadingNotifications, setLoadingNotifications] = useState(true);
  const [loadingPending, setLoadingPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const requestGeneration = useRef(0);

  const branchQuery = useCallback(() => {
    if (role !== 'admin') return '';
    const branchId = branchScope();
    return branchId ? `&branchId=${encodeURIComponent(branchId)}` : '';
  }, []);

  const refreshNotifications = useCallback(async () => {
    const generation = ++requestGeneration.current;
    const scope = authScope();
    setLoadingNotifications(true);
    try {
      const query = `readState=unread&page=1&pageSize=20${branchQuery()}`;
      const snapshot = await fetchNotificationSnapshot(query, branchQuery().replace(/^&/, ''), scope);
      if (generation !== requestGeneration.current || scope !== authScope()) return;
      setNotifications(snapshot.items);
      setUnreadCount(snapshot.count);
      setError(null);
    } catch (reason) {
      if (generation === requestGeneration.current) setError(reason instanceof Error ? reason.message : 'Không thể tải thông báo.');
    } finally {
      if (generation === requestGeneration.current) setLoadingNotifications(false);
    }
  }, [branchQuery]);

  const refreshPending = useCallback(async () => {
    if (role === 'employee' || role === 'kitchen') {
      setPendingCount(0);
      setPendingNotifications([]);
      setLoadingPending(false);
      return;
    }
    const generation = requestGeneration.current;
    setLoadingPending(true);
    try {
      const [count, list] = await Promise.all([fetchPendingWebOrderCount(), fetchPendingWebOrderNotifications()]);
      if (generation !== requestGeneration.current) return;
      setPendingCount(count);
      setPendingNotifications(list);
      window.dispatchEvent(new Event(PENDING_ORDER_CHANGED_EVENT));
    } catch (reason) {
      if (generation === requestGeneration.current) setError(reason instanceof Error ? reason.message : 'Không thể tải đơn đang xử lý.');
    } finally {
      if (generation === requestGeneration.current) setLoadingPending(false);
    }
  }, [role]);

  const markRead = useCallback(async (id: string, itemBranchId?: string | null) => {
    const branchId = itemBranchId || (role === 'admin' ? branchScope() : '');
    const suffix = branchId ? `?branchId=${encodeURIComponent(branchId)}` : '';
    const response = await fetch(`${API_URL}/api/Notification/${id}/read${suffix}`, { method: 'PATCH' });
    if (!response.ok) throw new Error('Không thể đánh dấu thông báo đã đọc.');
    setNotifications(previous => previous.filter(item => item.id !== id));
    setUnreadCount(previous => Math.max(0, previous - 1));
  }, [role]);

  const markAllRead = useCallback(async () => {
    const branchId = role === 'admin' ? branchScope() : '';
    const suffix = branchId ? `?branchId=${encodeURIComponent(branchId)}` : '';
    const response = await fetch(`${API_URL}/api/Notification/read-all${suffix}`, { method: 'POST' });
    if (!response.ok) throw new Error('Không thể đánh dấu tất cả thông báo.');
    setNotifications([]);
    setUnreadCount(0);
  }, [role]);

  useEffect(() => {
    let disposed = false;
    const refreshVisible = () => {
      if (document.visibilityState === 'visible') {
        void refreshNotifications();
        void refreshPending();
      }
    };
    void refreshNotifications();
    void refreshPending();
    // SignalR delivers new notifications immediately; polling is only a reconciliation
    // fallback for missed events and is intentionally kept low-frequency to reduce API/DB traffic.
    const notificationTimer = window.setInterval(() => { if (!disposed && document.visibilityState === 'visible') void refreshNotifications(); }, 60000);
    const pendingTimer = window.setInterval(() => { if (!disposed && document.visibilityState === 'visible') void refreshPending(); }, 60000);
    const selectedBranch = role === 'admin' ? branchScope() : '';
    const connectionUrl = `${API_URL}/kitchenHub${selectedBranch ? `?branchId=${encodeURIComponent(selectedBranch)}` : ''}`;
    const connection = new signalR.HubConnectionBuilder().withUrl(connectionUrl, { accessTokenFactory: () => localStorage.getItem('adminToken') || '' }).withAutomaticReconnect().build();
    const onNotification = (notification: NotificationItem) => {
      if (disposed) return;
      const selectedBranch = branchScope();
      if (notification.branchId && selectedBranch && notification.branchId !== selectedBranch) return;
      setNotifications(previous => {
        if (previous.some(item => item.id === notification.id)) return previous;
        setUnreadCount(count => count + 1);
        return [notification, ...previous].slice(0, 20);
      });
    };
    const onPendingChanged = (payload?: { branchId?: string | null }) => {
      if (disposed) return;
      const selectedBranch = branchScope();
      if (payload?.branchId && selectedBranch && payload.branchId !== selectedBranch && role !== 'admin') return;
      void refreshPending();
    };
    const onProductAvailabilityChanged = (payload: { id: string; availabilityStatus: string; branchId: string }) => {
      if (!disposed) window.dispatchEvent(new CustomEvent('product-availability-changed', { detail: payload }));
    };
    const onToppingAvailabilityChanged = (payload: { id: string; availabilityStatus: string; branchId: string }) => {
      if (!disposed) window.dispatchEvent(new CustomEvent('topping-availability-changed', { detail: payload }));
    };
    connection.on('NotificationCreated', onNotification);
    connection.on('PendingOrderChanged', onPendingChanged);
    connection.on('ProductAvailabilityChanged', onProductAvailabilityChanged);
    connection.on('ToppingAvailabilityChanged', onToppingAvailabilityChanged);
    connection.onreconnected(() => refreshVisible());
    void connection.start().catch(() => undefined);
    const onCustomRefresh = () => { if (!disposed) void refreshNotifications(); };
    window.addEventListener('refresh-notifications', onCustomRefresh);
    document.addEventListener('visibilitychange', refreshVisible);
    return () => {
      disposed = true;
      requestGeneration.current += 1;
      window.clearInterval(notificationTimer);
      window.clearInterval(pendingTimer);
      connection.off('NotificationCreated', onNotification);
      connection.off('PendingOrderChanged', onPendingChanged);
      connection.off('ProductAvailabilityChanged', onProductAvailabilityChanged);
      connection.off('ToppingAvailabilityChanged', onToppingAvailabilityChanged);
      window.removeEventListener('refresh-notifications', onCustomRefresh);
      document.removeEventListener('visibilitychange', refreshVisible);
      void connection.stop().catch(() => undefined);
    };
  }, [refreshNotifications, refreshPending, role]);

  return { notifications, pendingNotifications, unreadCount, pendingCount, loadingNotifications, loadingPending, error, refreshNotifications, refreshPending, markRead, markAllRead };
};
