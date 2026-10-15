interface POSCategoryTabsProps {
  categories: string[];
  activeCategory: string;
  onCategoryChange: (category: string) => void;
}

const POSCategoryTabs = ({ categories, activeCategory, onCategoryChange }: POSCategoryTabsProps) => (
  <div className="bg-white border-b px-2 py-2 flex items-center shadow-sm overflow-x-auto no-scrollbar">
    <div className="flex space-x-2">
      {categories.map(category => (
        <button
          key={category}
          onClick={() => onCategoryChange(category)}
          className={`px-4 py-1 rounded-full text-xs font-bold transition-all whitespace-nowrap ${
            activeCategory === category
              ? 'bg-blue-600 text-white shadow-md'
              : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
          }`}
        >
          {category}
        </button>
      ))}
    </div>
  </div>
);

export default POSCategoryTabs;
