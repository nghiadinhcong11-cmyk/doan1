interface CustomerCategoryTabsProps {
  categories: string[];
  activeCategory: string;
  onCategoryChange: (category: string) => void;
}

export default function CustomerCategoryTabs({ categories, activeCategory, onCategoryChange }: CustomerCategoryTabsProps) {
  return (
    <div className="sticky top-16 z-20 flex space-x-3 overflow-x-auto border-b border-[#ebe5dc] bg-[#fffdf9]/90 p-4 shadow-sm backdrop-blur-lg no-scrollbar">
      {categories.map(category => (
        <button
          type="button"
          key={category}
          onClick={() => onCategoryChange(category)}
          className={`min-h-11 touch-manipulation whitespace-nowrap rounded-2xl px-5 py-2.5 text-xs font-black uppercase tracking-widest transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-orange-300 focus-visible:ring-offset-2 ${
            activeCategory === category ? 'category-active bg-orange-600 text-white shadow-lg shadow-orange-500/30' : 'bg-[#f0ece5] text-[#85796d] hover:bg-[#e7dfd5] hover:-translate-y-0.5'
          }`}
        >
          {category}
        </button>
      ))}
    </div>
  );
}
