import { API_URL } from '../../config';
import { PENDING_ORDER_STATUS } from '../orders/orderClassification';
import {
  assertSuccessfulOrderListResponse,
  selectPendingOrderNotifications,
  selectPendingWebOrders,
  type PendingOrderCandidate,
} from '../orders/orderListSelectors';

/** The status used by the existing POS flow for web/QR orders awaiting acceptance. */
export const PENDING_WEB_ORDER_STATUS = PENDING_ORDER_STATUS;
export const PENDING_ORDER_CHANGED_EVENT = 'pending-order-state-changed';

export interface PendingWebOrderNotification {
  id: string;
  title: string;
  message: string;
  type: 'order' | 'gift';
  link: string;
  isRead: false;
  isPendingOrder: true;
}

let pendingWebOrdersInFlight: {
  scopeKey: string;
  promise: Promise<PendingOrderCandidate[]>;
} | null = null;

/**
 * Coalesces simultaneous count/list reads only. The result is not cached after
 * completion, so logout or branch changes cannot reuse another scope's data.
 */
async function fetchPendingWebOrderSnapshot(signal?: AbortSignal, includeAllBranches = false): Promise<PendingOrderCandidate[]> {
  const selectedBranchId = includeAllBranches ? '' : (localStorage.getItem('selectedBranchId') || '');
  // The token is used only as an in-memory equality boundary so a logout/login
  // cannot join another user's active request. It is never persisted or logged.
  const scopeKey = `${includeAllBranches ? 'all' : selectedBranchId}|${localStorage.getItem('adminToken') || ''}`;
  if (pendingWebOrdersInFlight?.scopeKey === scopeKey) return pendingWebOrdersInFlight.promise;

  const params = new URLSearchParams({ status: 'Đang xử lý,Đổi quà' });
  if (selectedBranchId) params.set('branchId', selectedBranchId);

  const request = (async () => {
  const response = await fetch(`${API_URL}/api/Order/pending-summary?${params.toString()}`, { signal });
    assertSuccessfulOrderListResponse(response, 'Pending order snapshot');
    const orders: unknown = await response.json();
    return Array.isArray(orders) ? orders as PendingOrderCandidate[] : [];
  })();

  pendingWebOrdersInFlight = { scopeKey, promise: request };
  try {
    return await request;
  } finally {
    if (pendingWebOrdersInFlight?.promise === request) pendingWebOrdersInFlight = null;
  }
}

/**
 * Reads the server's order list and applies the same actionable-order definition
 * used by POS: pending status with no employee assigned yet.
 */
export async function fetchPendingWebOrderCount(signal?: AbortSignal, includeAllBranches = false): Promise<number> {
  return selectPendingWebOrders(await fetchPendingWebOrderSnapshot(signal, includeAllBranches)).length;
}

/**
 * Loads the current pending web orders for the selected branch. The result is
 * a replaceable view of server state, not an append-only event log; this keeps
 * merged orders and completed orders from producing stale or duplicate items.
 */
export async function fetchPendingWebOrderNotifications(signal?: AbortSignal, includeAllBranches = false): Promise<PendingWebOrderNotification[]> {
  return selectPendingOrderNotifications(await fetchPendingWebOrderSnapshot(signal, includeAllBranches))
    .map(order => {
      const isGift = order.status === 'Đổi quà';
      const amount = typeof order.totalAmount === 'number' ? order.totalAmount : 0;
      return {
        id: order.id!,
        title: isGift ? 'Yêu cầu đổi quà' : 'Đơn hàng mới',
        message: isGift
          ? `Khách ${order.customerName || order.customerPhone || 'vãng lai'} muốn đổi: ${order.details?.[0]?.productName || 'Quà tặng'}`
          : `Bàn ${order.tableName || 'vãng lai'} vừa đặt món: ${amount.toLocaleString()}đ`,
        type: isGift ? 'gift' : 'order',
        link: isGift ? '/customers' : `/pos?reviewOrderId=${order.id}`,
        isRead: false,
        isPendingOrder: true
      };
    });
}
