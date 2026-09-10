// Shared Domain Types between Admin Web and Customer Web

export interface Product {
  id?: string;
  code?: string;
  name: string;
  category: string;
  price: number;
  costPrice?: number;
  group?: string;
  type?: string;
  isActive?: boolean;
  imageUrl?: string;
  sizesJson?: string;
  toppingsJson?: string;
  description?: string;
}

export interface Table {
  id: string;
  name: string;
  status: string;
  areaName?: string;
  isActive?: boolean;
}

export interface Branch {
  id: string;
  name: string;
  address?: string;
  bankName?: string;
  accountNumber?: string;
  accountHolder?: string;
  isMain?: boolean;
  imageUrl?: string;
}

export interface OrderDetail {
  id?: string;
  productId: string;
  productName: string;
  quantity: number;
  sentQuantity: number;
  unitPrice: number;
  options?: string;
  toppingId?: string;
  note?: string;
}

export interface Order {
  id?: string;
  invoiceCode?: string;
  tableName: string;
  branchId?: string;
  branchName?: string;
  status: 'Đang xử lý' | 'Hoàn thành' | 'Đã hủy';
  totalAmount: number;
  paidAmount?: number;
  discount?: number;
  paymentMethod?: string;
  customerName?: string;
  customerPhone?: string;
  customerEmail?: string;
  createdBy?: string;
  note?: string;
  createdAt?: string;
  details?: OrderDetail[];
}

export interface Customer {
  id?: string;
  fullName: string;
  phoneNumber: string;
  email?: string;
  address?: string;
  point?: number;
}

export interface Reservation {
  id?: string;
  customerName: string;
  customerPhone: string;
  reservationTime: string;
  guestCount: number;
  tableName?: string;
  status: string;
  note?: string;
  branchId?: string;
}
