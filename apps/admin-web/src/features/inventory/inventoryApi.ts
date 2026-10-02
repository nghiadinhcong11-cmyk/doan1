import { API_URL } from '../../config';

export const INVENTORY_UNITS = ['kg', 'g', 'litre', 'ml', 'piece', 'box', 'bottle', 'pack'] as const;
export type InventoryUnit = typeof INVENTORY_UNITS[number];
export type StockStatus = string | number;
export type TransactionType = string | number;

export interface InventoryItem {
  id: string;
  name: string;
  unitCode: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}

export interface BranchInventory {
  branchId: string;
  inventoryItemId: string;
  name: string;
  unitCode: string;
  currentQuantity: number;
  minimumStock: number;
  isLowStock: boolean;
  isOutOfStock: boolean;
  updatedAtUtc: string;
}

export interface InventoryQuantityByUnit {
  unitCode: string;
  quantity: number;
}

export interface InventoryLowStockItem {
  inventoryItemId: string;
  name: string;
  unitCode: string;
  currentQuantity: number;
  minimumStock: number;
  isOutOfStock: boolean;
}

export interface InventoryOverview {
  branchId: string;
  periodStartUtc: string;
  periodEndUtc: string;
  activeItemCount: number;
  lowStockItemCount: number;
  outOfStockItemCount: number;
  receivedThisMonthByUnit: InventoryQuantityByUnit[];
  issuedThisMonthByUnit: InventoryQuantityByUnit[];
  importCostThisMonth: number;
  lowStockItems: InventoryLowStockItem[];
}

export interface StockDocumentItem {
  id: string;
  inventoryItemId: string;
  inventoryItemName: string;
  unitCode: string;
  quantity: number;
  unitPrice: number | null;
}

export interface StockReceipt {
  id: string;
  branchId: string;
  supplierName?: string | null;
  purchaseDate?: string | null;
  totalAmount: number;
  status: StockStatus;
  note?: string | null;
  createdBy: string;
  createdAtUtc: string;
  confirmedBy?: string | null;
  confirmedAtUtc?: string | null;
  items: StockDocumentItem[];
}

export interface StockIssue {
  id: string;
  branchId: string;
  reason: string;
  note?: string | null;
  status: StockStatus;
  createdBy: string;
  createdAtUtc: string;
  confirmedBy?: string | null;
  confirmedAtUtc?: string | null;
  items: StockDocumentItem[];
}

export interface StockTransaction {
  id: string;
  branchId: string;
  inventoryItemId: string;
  inventoryItemName: string;
  unitCode: string;
  type: TransactionType;
  quantity: number;
  beforeQuantity: number;
  afterQuantity: number;
  referenceType: string;
  referenceId: string;
  createdBy: string;
  createdAtUtc: string;
  note?: string | null;
}

export interface InventoryPage<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface StockLineInput {
  inventoryItemId: string;
  quantity: number;
  unitPrice?: number;
}

export interface ApiError {
  message: string;
  status: number;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...(init?.headers || {}) }
  });
  if (!response.ok) {
    let message = 'Không thể hoàn tất thao tác kho.';
    try {
      const body: unknown = await response.json();
      if (typeof body === 'object' && body !== null && 'message' in body && typeof body.message === 'string') message = body.message;
    } catch { /* The API may return an empty error response. */ }
    const error: ApiError = { message, status: response.status };
    throw error;
  }
  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

const query = (params: Record<string, string | number | boolean | undefined>) => {
  const search = new URLSearchParams();
  Object.entries(params).forEach(([key, value]) => { if (value !== undefined && value !== '') search.set(key, String(value)); });
  const value = search.toString();
  return value ? `?${value}` : '';
};

export const inventoryApi = {
  getItems: (activeOnly?: boolean) => request<InventoryItem[]>(`/api/inventory/items${query({ activeOnly })}`),
  createItem: (body: { name: string; unitCode: string }) => request<InventoryItem>('/api/inventory/items', { method: 'POST', body: JSON.stringify(body) }),
  updateItem: (id: string, body: { name: string; unitCode: string; isActive: boolean }) => request<InventoryItem>(`/api/inventory/items/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  getBranchInventory: (branchId: string) => request<BranchInventory[]>(`/api/inventory/branches/${branchId}`),
  getOverview: (branchId: string) => request<InventoryOverview>(`/api/inventory/overview/${branchId}`),
  updateMinimumStock: (branchId: string, itemId: string, minimumStock: number) => request<BranchInventory>(`/api/inventory/branches/${branchId}/items/${itemId}/minimum-stock`, { method: 'PUT', body: JSON.stringify({ minimumStock }) }),
  getReceipts: (params: { branchId?: string; status?: StockStatus; page?: number }) => request<InventoryPage<StockReceipt>>(`/api/inventory/receipts${query({ ...params, pageSize: 20 })}`),
  getReceipt: (id: string) => request<StockReceipt>(`/api/inventory/receipts/${id}`),
  createReceipt: (body: { branchId: string; supplierName?: string; purchaseDate?: string; note?: string; idempotencyKey: string; items: StockLineInput[] }) => request<StockReceipt>('/api/inventory/receipts', { method: 'POST', body: JSON.stringify(body) }),
  updateReceipt: (id: string, body: { supplierName?: string; purchaseDate?: string; note?: string; items: StockLineInput[] }) => request<StockReceipt>(`/api/inventory/receipts/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  confirmReceipt: (id: string) => request<StockReceipt>(`/api/inventory/receipts/${id}/confirm`, { method: 'POST' }),
  cancelReceipt: (id: string) => request<StockReceipt>(`/api/inventory/receipts/${id}/cancel`, { method: 'POST' }),
  getIssues: (params: { branchId?: string; status?: StockStatus; page?: number }) => request<InventoryPage<StockIssue>>(`/api/inventory/issues${query({ ...params, pageSize: 20 })}`),
  getIssue: (id: string) => request<StockIssue>(`/api/inventory/issues/${id}`),
  createIssue: (body: { branchId: string; reason: string; note?: string; idempotencyKey: string; items: StockLineInput[] }) => request<StockIssue>('/api/inventory/issues', { method: 'POST', body: JSON.stringify(body) }),
  updateIssue: (id: string, body: { reason: string; note?: string; items: StockLineInput[] }) => request<StockIssue>(`/api/inventory/issues/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  confirmIssue: (id: string) => request<StockIssue>(`/api/inventory/issues/${id}/confirm`, { method: 'POST' }),
  cancelIssue: (id: string) => request<StockIssue>(`/api/inventory/issues/${id}/cancel`, { method: 'POST' }),
  getTransactions: (params: { branchId?: string; inventoryItemId?: string; type?: TransactionType; fromDate?: string; toDate?: string; page?: number }) => request<InventoryPage<StockTransaction>>(`/api/inventory/transactions${query({ ...params, pageSize: 20 })}`)
};

export function formatApiError(error: unknown): string {
  if (typeof error === 'object' && error !== null && 'message' in error && typeof error.message === 'string') return error.message;
  return 'Không thể hoàn tất thao tác kho.';
}
