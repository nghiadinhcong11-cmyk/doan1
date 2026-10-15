export const PENDING_ORDER_STATUS = 'Đang xử lý';
export const GIFT_ORDER_STATUS = 'Đổi quà';
export const TAKEAWAY_TABLE_NAME = 'Mang về';
export const WALK_IN_TABLE_NAME = 'vãng lai';

export interface OrderClassificationInput {
  status?: string | null;
  createdBy?: string | null;
  tableName?: string | null;
  branchId?: string | null;
}

export type OrderLocation = 'takeaway' | 'walk-in' | 'physical-table' | 'named-table' | 'unknown';

export interface OrderClassification {
  isPendingUnaccepted: boolean;
  isAccepted: boolean;
  isGiftRequest: boolean;
  isAwaitingStaffAction: boolean;
  hasStaffOwner: boolean;
  location: OrderLocation;
}

const normalize = (value: string | null | undefined) => value?.trim() || '';

/**
 * Classifies the server-backed order state without treating a table label as
 * proof that an order is a web/virtual order. The backend marks an accepted
 * order through CreatedBy; TableName only describes where it is served.
 */
export function classifyOrder(
  order: OrderClassificationInput,
  physicalTableNames?: ReadonlySet<string>,
): OrderClassification {
  const status = normalize(order.status);
  const createdBy = normalize(order.createdBy);
  const tableName = normalize(order.tableName);
  const hasStaffOwner = createdBy.length > 0;
  const isPendingUnaccepted = status === PENDING_ORDER_STATUS && !hasStaffOwner;
  const isGiftRequest = status === GIFT_ORDER_STATUS;
  const isAwaitingStaffAction = isPendingUnaccepted || isGiftRequest;

  let location: OrderLocation = 'unknown';
  if (tableName === TAKEAWAY_TABLE_NAME) {
    location = 'takeaway';
  } else if (tableName.toLocaleLowerCase() === WALK_IN_TABLE_NAME) {
    location = 'walk-in';
  } else if (tableName && physicalTableNames?.has(tableName)) {
    location = 'physical-table';
  } else if (tableName) {
    // A legacy or unknown label is not evidence that the order is unaccepted.
    location = 'named-table';
  }

  return {
    isPendingUnaccepted,
    isAccepted: status === PENDING_ORDER_STATUS && hasStaffOwner,
    isGiftRequest,
    isAwaitingStaffAction,
    hasStaffOwner,
    location,
  };
}

export function isPendingWebOrder(order: OrderClassificationInput): boolean {
  return classifyOrder(order).isPendingUnaccepted;
}

export function isPendingOrderNotification(order: OrderClassificationInput): boolean {
  return classifyOrder(order).isAwaitingStaffAction;
}
