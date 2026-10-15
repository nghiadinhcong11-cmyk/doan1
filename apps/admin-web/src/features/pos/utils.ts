import type { CartItem, Product } from './types';

export const kitchenStatusLabel = (status?: string) => {
  switch (status) {
    case 'Pending': return 'Bếp đã nhận';
    case 'Preparing': return 'Đang chế biến';
    case 'Ready': return 'Đã ra món';
    case 'Completed': return 'Đã hoàn tất';
    case 'Cancelled': return 'Đã hủy';
    default: return '';
  }
};

export const kitchenStatusClass = (status?: string) => {
  switch (status) {
    case 'Ready':
    case 'Completed': return 'bg-green-100 text-green-700';
    case 'Preparing': return 'bg-orange-100 text-orange-700';
    case 'Cancelled': return 'bg-red-100 text-red-700';
    default: return 'bg-slate-100 text-slate-600';
  }
};

export const calculateCartPreviewTotal = (cart: CartItem[]) =>
  cart.reduce((sum, item) => sum + (item.totalItemPrice * item.quantity), 0);

export const filterProducts = (products: Product[], activeCategory: string, searchTerm: string) => {
  const normalizedSearchTerm = searchTerm.toLowerCase();
  return products.filter(product => {
    const matchesCategory = activeCategory === 'Tất cả' || product.category === activeCategory;
    const matchesSearch = product.name.toLowerCase().includes(normalizedSearchTerm) ||
      (product.id && product.id.toLowerCase().includes(normalizedSearchTerm));
    return matchesCategory && matchesSearch;
  });
};
