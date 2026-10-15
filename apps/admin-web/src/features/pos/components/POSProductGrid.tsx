import { UtensilsCrossed } from 'lucide-react';
import type { Product } from '../types';

interface POSProductGridProps {
  products: Product[];
  onAddProduct: (product: Product) => void;
}

const POSProductGrid = ({ products, onAddProduct }: POSProductGridProps) => (
  <div className="grid grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
    {products.map((product, idx) => (
      <button
        type="button"
        key={`${product.id}-${idx}`}
        onClick={() => onAddProduct(product)}
        aria-label={`Thêm ${product.name} vào đơn`}
        className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden cursor-pointer hover:shadow-md hover:border-blue-400 transition-all group"
      >
        <div className="aspect-square bg-gray-50 flex items-center justify-center relative">
          {product.imageUrl && product.imageUrl !== 'string' ? (
            <img src={product.imageUrl} alt={product.name} className="w-full h-full object-cover" />
          ) : (
            <UtensilsCrossed size={32} className="text-gray-200 group-hover:text-blue-100 transition-colors" />
          )}
          <div className="absolute top-2 right-2 bg-blue-600 text-white text-[10px] font-bold px-1.5 py-0.5 rounded shadow-sm">
            {product.price.toLocaleString()}
          </div>
        </div>
        <div className="p-2 text-center border-t">
          <p className="text-[11px] font-bold text-gray-700 truncate capitalize">{product.name}</p>
        </div>
      </button>
    ))}
  </div>
);

export default POSProductGrid;
