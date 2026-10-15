import { Plus, RotateCcw, Search } from 'lucide-react';

interface POSMenuSearchControlsProps {
  searchTerm: string;
  loading: boolean;
  onSearchTermChange: (value: string) => void;
  onRefresh: () => void;
  onFocusSearch: () => void;
}

const POSMenuSearchControls = ({
  searchTerm,
  loading,
  onSearchTermChange,
  onRefresh,
  onFocusSearch
}: POSMenuSearchControlsProps) => (
  <>
    <div className="order-last sm:order-none w-full sm:flex-1 relative sm:ml-4">
      <span className="absolute inset-y-0 left-0 pl-3 flex items-center">
        <Search className="h-4 w-4 text-blue-200" />
      </span>
      <input
        type="text"
        className="w-full bg-blue-700/50 border-none rounded py-1.5 pl-10 pr-4 text-sm text-white placeholder-blue-200 outline-none focus:ring-1 focus:ring-white"
        placeholder="Tìm món (F3)"
        value={searchTerm}
        onChange={(e) => onSearchTermChange(e.target.value)}
      />
    </div>
    <button
      onClick={onRefresh}
      disabled={loading}
      className="p-2 text-white hover:bg-blue-600 rounded transition-all"
      title="Làm mới dữ liệu (F5)"
    >
      <RotateCcw size={20} className={loading ? 'animate-spin' : ''} />
    </button>
    <button onClick={onFocusSearch} className="p-2 text-white hover:bg-blue-600 rounded" title="Tìm món mới" aria-label="Tìm món mới"><Plus size={20}/></button>
  </>
);

export default POSMenuSearchControls;
