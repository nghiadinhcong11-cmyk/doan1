import { isPendingOrderNotification, isPendingWebOrder } from './orderClassification.ts';

export interface PendingOrderCandidate {
  id?: string;
  status?: string | null;
  createdBy?: string | null;
  tableName?: string | null;
  customerName?: string | null;
  customerPhone?: string | null;
  totalAmount?: number | null;
  createdAt?: string | null;
  details?: Array<{ productName?: string | null }>;
}

export function selectPendingWebOrders(orders: unknown): PendingOrderCandidate[] {
  if (!Array.isArray(orders)) return [];

  return orders.filter((candidate): candidate is PendingOrderCandidate => {
    if (!candidate || typeof candidate !== 'object') return false;
    return isPendingWebOrder(candidate as PendingOrderCandidate);
  });
}

export function selectPendingOrderNotifications(orders: unknown): PendingOrderCandidate[] {
  if (!Array.isArray(orders)) return [];

  const seen = new Set<string>();
  return orders
    .filter((candidate): candidate is PendingOrderCandidate => {
      if (!candidate || typeof candidate !== 'object') return false;
      const order = candidate as PendingOrderCandidate;
      return Boolean(order.id) && isPendingOrderNotification(order);
    })
    .filter(order => {
      if (!order.id || seen.has(order.id)) return false;
      seen.add(order.id);
      return true;
    });
}

/** Keeps HTTP failures observable; callers must not turn them into empty lists. */
export function assertSuccessfulOrderListResponse(
  response: { ok: boolean; status: number },
  operation = 'Pending order query',
): void {
  if (!response.ok) throw new Error(`${operation} failed: ${response.status}`);
}
