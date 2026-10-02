import { API_URL } from '../../config';

/** The status used by the existing POS flow for web/QR orders awaiting acceptance. */
export const PENDING_WEB_ORDER_STATUS = 'Đang xử lý';
export const PENDING_ORDER_CHANGED_EVENT = 'pending-order-state-changed';

interface PendingOrderCandidate {
  status?: string | null;
  createdBy?: string | null;
}

/**
 * Reads the server's order list and applies the same actionable-order definition
 * used by POS: pending status with no employee assigned yet.
 */
export async function fetchPendingWebOrderCount(signal?: AbortSignal): Promise<number> {
  const params = new URLSearchParams({ status: PENDING_WEB_ORDER_STATUS });
  const selectedBranchId = localStorage.getItem('selectedBranchId');
  if (selectedBranchId) params.set('branchId', selectedBranchId);

  const response = await fetch(`${API_URL}/api/Order?${params.toString()}`, { signal });
  if (!response.ok) {
    throw new Error(`Pending order query failed: ${response.status}`);
  }

  const orders: unknown = await response.json();
  if (!Array.isArray(orders)) return 0;

  return orders.filter((candidate): candidate is PendingOrderCandidate => {
    if (!candidate || typeof candidate !== 'object') return false;
    const order = candidate as PendingOrderCandidate;
    return order.status === PENDING_WEB_ORDER_STATUS && !String(order.createdBy ?? '').trim();
  }).length;
}
