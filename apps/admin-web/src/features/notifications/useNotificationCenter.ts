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

const playIncomingNotificationSound = (notification: NotificationItem) => {
  if (!['ORDER_FOLLOW_UP', 'RESERVATION_CREATED', 'GIFT_REDEEM_REQUEST'].includes(notification.type)) return;

  const audio = new Audio('https://assets.mixkit.co/active_storage/sfx/2869/2869-preview.mp3');
  audio.volume = 0.85;
  void audio.play().catch(() => {
    // Browsers may block playback until the user has interacted with the page.
  });
};

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

export const useNotificationCenter = (role: NotificationRole, options?: { enabled?: boolean }) => {
  const enabled = options?.enabled ?? true;
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const [pendingNotifications, setPendingNotifications] = useState<PendingWebOrderNotification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [pendingCount, setPendingCount] = useState(0);
  const [loadingNotifications, setLoadingNotifications] = useState(true);
  const [loadingPending, setLoadingPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  // Keep the two feeds independent. Refreshing notifications must not make an
  // in-flight pending-order refresh look stale, and vice versa.
  const notificationGeneration = useRef(0);
  const pendingGeneration = useRef(0);

  const branchQuery = useCallback(() => {
    // Admin is global-scope and must receive notifications from every branch.
    // Branch-scoped roles are restricted server-side by their authenticated branch.
    return '';
  }, []);

  const refreshNotifications = useCallback(async () => {
    const generation = ++notificationGeneration.current;
    const scope = authScope();
    setLoadingNotifications(true);
    try {
      const query = `readState=unread&page=1&pageSize=20${branchQuery()}`;
      const snapshot = await fetchNotificationSnapshot(query, branchQuery().replace(/^&/, ''), scope);
      if (generation !== notificationGeneration.current || scope !== authScope()) return;
      setNotifications(snapshot.items);
      setUnreadCount(snapshot.count);
      setError(null);
    } catch (reason) {
      if (generation === notificationGeneration.current) setError(reason instanceof Error ? reason.message : 'Không thể tải thông báo.');
    } finally {
      if (generation === notificationGeneration.current) setLoadingNotifications(false);
    }
  }, [branchQuery]);

  const refreshPending = useCallback(async (forceRefresh = false) => {
    if (role === 'kitchen') {
      setPendingCount(0);
      setPendingNotifications([]);
      setLoadingPending(false);
      return;
    }
    const generation = ++pendingGeneration.current;
    setLoadingPending(true);
    try {
      const includeAllBranches = role === 'admin';
      const [count, list] = await Promise.all([
        fetchPendingWebOrderCount(undefined, includeAllBranches, forceRefresh),
        fetchPendingWebOrderNotifications(undefined, includeAllBranches, role === 'employee' ? '/staff/sales' : '/pos', forceRefresh)
      ]);
      if (generation !== pendingGeneration.current) return;
      setPendingCount(count);
      setPendingNotifications(list);
      window.dispatchEvent(new Event(PENDING_ORDER_CHANGED_EVENT));
    } catch (reason) {
      if (generation === pendingGeneration.current) setError(reason instanceof Error ? reason.message : 'Không thể tải đơn đang xử lý.');
    } finally {
      if (generation === pendingGeneration.current) setLoadingPending(false);
    }
  }, [role]);

  const markRead = useCallback(async (id: string, itemBranchId?: string | null) => {
    const branchId = role === 'admin' ? '' : (itemBranchId || '');
    const suffix = branchId ? `?branchId=${encodeURIComponent(branchId)}` : '';
    const response = await fetch(`${API_URL}/api/Notification/${id}/read${suffix}`, { method: 'PATCH' });
    if (!response.ok) throw new Error('Không thể đánh dấu thông báo đã đọc.');
    setNotifications(previous => previous.filter(item => item.id !== id));
    setUnreadCount(previous => Math.max(0, previous - 1));
  }, [role]);

  const markAllRead = useCallback(async () => {
    const branchId = '';
    const suffix = branchId ? `?branchId=${encodeURIComponent(branchId)}` : '';
    const response = await fetch(`${API_URL}/api/Notification/read-all${suffix}`, { method: 'POST' });
    if (!response.ok) throw new Error('Không thể đánh dấu tất cả thông báo.');
    setNotifications([]);
    setUnreadCount(0);
  }, [role]);

  useEffect(() => {
    if (!enabled) {
      setNotifications([]);
      setPendingNotifications([]);
      setUnreadCount(0);
      setPendingCount(0);
      setLoadingNotifications(false);
      setLoadingPending(false);
      setError(null);
      return;
    }
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
    const connectionUrl = `${API_URL}/kitchenHub`;
    const connection = new signalR.HubConnectionBuilder().withUrl(connectionUrl, { accessTokenFactory: () => localStorage.getItem('adminToken') || '' }).withAutomaticReconnect().build();
    const onNotification = (notification: NotificationItem) => {
      if (disposed) return;
      if (notification.type === 'RESERVATION_CREATED') {
        window.dispatchEvent(new CustomEvent('reservation-created', { detail: notification }));
      }
      let isNewNotification = false;
      setNotifications(previous => {
        if (previous.some(item => item.id === notification.id)) return previous;
        isNewNotification = true;
        setUnreadCount(count => count + 1);
        return [notification, ...previous].slice(0, 20);
      });
      if (isNewNotification) playIncomingNotificationSound(notification);
    };
    const onPendingChanged = (payload?: { branchId?: string | null }) => {
      if (disposed) return;
      void refreshPending(true);
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
    // React StrictMode mounts, cleans up, and mounts effects again in
    // development. Defer startup one turn so the first temporary instance is
    // disposed before it begins SignalR negotiation; otherwise SignalR logs
    // "connection was stopped during negotiation" even though the final
    // instance is healthy.
    let startPromise: Promise<void> | null = null;
    const startTimer = window.setTimeout(() => {
      if (disposed) return;
      startPromise = connection.start();
      void startPromise.catch(error => {
        if (!disposed) console.warn('Notification hub failed:', error);
      });
    }, 0);
    const onCustomRefresh = () => {
      if (disposed) return;
      void refreshNotifications();
      void refreshPending(true);
    };
    window.addEventListener('refresh-notifications', onCustomRefresh);
    document.addEventListener('visibilitychange', refreshVisible);
    return () => {
      disposed = true;
      window.clearTimeout(startTimer);
      notificationGeneration.current += 1;
      pendingGeneration.current += 1;
      window.clearInterval(notificationTimer);
      window.clearInterval(pendingTimer);
      connection.off('NotificationCreated', onNotification);
      connection.off('PendingOrderChanged', onPendingChanged);
      connection.off('ProductAvailabilityChanged', onProductAvailabilityChanged);
      connection.off('ToppingAvailabilityChanged', onToppingAvailabilityChanged);
      window.removeEventListener('refresh-notifications', onCustomRefresh);
      document.removeEventListener('visibilitychange', refreshVisible);
      const stopConnection = () => {
        if (connection.state !== signalR.HubConnectionState.Disconnected) {
          void connection.stop().catch(() => undefined);
        }
      };
      if (startPromise) void startPromise.catch(() => undefined).then(stopConnection);
      else stopConnection();
    };
  }, [enabled, refreshNotifications, refreshPending, role]);

  return { notifications, pendingNotifications, unreadCount, pendingCount, loadingNotifications, loadingPending, error, refreshNotifications, refreshPending, markRead, markAllRead };
};
