export type POSUserRole = 'admin' | 'manager' | 'employee' | 'cashier' | null;

export interface Product {
  id: string;
  name: string;
  price: number;
  category: string;
  group?: string;
  imageUrl?: string;
  sizesJson?: string;
  toppingsJson?: string;
}

export interface Table {
  id: string;
  name: string;
  status: string;
  areaName: string;
  isActive: boolean;
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

export interface CartItem extends Product {
  quantity: number;
  sentQuantity: number;
  note: string;
  selectedSize?: any;
  selectedToppings?: any[];
  totalItemPrice: number;
  kitchenStatus?: string;
  optionsText?: string;
}
