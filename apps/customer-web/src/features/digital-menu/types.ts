export interface Product {
  id: string;
  name: string;
  price: number;
  category: string;
  isActive?: boolean;
  group?: string;
  imageUrl?: string;
  description?: string;
  sizesJson?: string;
  toppingsJson?: string;
  availabilityStatus?: string;
}

export interface CartItem extends Product {
  quantity: number;
  selectedSize?: any;
  selectedToppings?: any[];
  totalItemPrice: number;
  note?: string;
}
