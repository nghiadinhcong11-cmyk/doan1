import { CheckCircle2, ChevronDown, ClipboardList, Grid, MapPin, Store } from 'lucide-react';
import POSMenuSearchControls from './POSMenuSearchControls';
import type { Branch, POSUserRole } from '../types';

interface POSHeaderProps {
  userRole?: POSUserRole;
  branchInfo: Branch | null;
  allBranches: Branch[];
  posTab: string;
  searchTerm: string;
  loading: boolean;
  refreshError: string | null;
  onSwitchBranch: (branch: Branch) => void;
  onNavigateBranches: () => void;
  onTabChange: (tab: 'tables' | 'menu') => void;
  onSearchTermChange: (value: string) => void;
  onRefresh: () => void;
  onFocusSearch: () => void;
}

export default function POSHeader({
  userRole,
  branchInfo,
  allBranches,
  posTab,
  searchTerm,
  loading,
  refreshError,
  onSwitchBranch,
  onNavigateBranches,
  onTabChange,
  onSearchTermChange,
  onRefresh,
  onFocusSearch
}: POSHeaderProps) {
  return (
    <div className="bg-[#0070f4] p-1 flex flex-wrap items-center gap-1">
      <div className={`relative mr-1 px-2 border-r border-blue-400 ${userRole === 'admin' ? 'group' : ''}`}>
        <div className={`flex items-center text-white p-1.5 rounded transition-all ${userRole === 'admin' ? 'cursor-pointer hover:bg-blue-600' : 'cursor-default'}`}>
          <MapPin size={16} className="mr-1.5 text-blue-200" />
          <div className="text-left">
            <p className="text-[9px] font-bold text-blue-200 uppercase leading-none mb-0.5">Chi nhánh</p>
            <p className="text-xs font-black truncate max-w-[120px]">{branchInfo?.name || 'Đang tải...'}</p>
          </div>
          {userRole === 'admin' && <ChevronDown size={14} className="ml-2 text-blue-200 opacity-60" />}
        </div>

        {userRole === 'admin' && (
          <div className="absolute top-full left-0 mt-1 w-64 bg-white rounded-lg shadow-2xl border border-gray-100 hidden group-hover:block z-[110] overflow-hidden animate-in fade-in slide-in-from-top-2 duration-200">
            <div className="p-3 border-b bg-gray-50 flex items-center justify-between">
              <p className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Hệ thống chi nhánh</p>
              <span className="bg-blue-100 text-blue-600 text-[9px] px-1.5 py-0.5 rounded font-bold">{allBranches.length} cơ sở</span>
            </div>
            <div className="max-h-80 overflow-y-auto py-1">
              {Array.isArray(allBranches) && allBranches.map(branch => (
                <div
                  key={branch.id}
                  onClick={() => onSwitchBranch(branch)}
                  className={`px-4 py-3 hover:bg-blue-50 cursor-pointer flex items-center justify-between border-b border-gray-50 last:border-0 transition-colors ${branchInfo?.id === branch.id ? 'bg-blue-50/80' : ''}`}
                >
                  <div className="flex-1 min-w-0 pr-4">
                    <div className="flex items-center space-x-3">
                      <div className="w-8 h-8 rounded-lg overflow-hidden shrink-0 bg-gray-100 border border-gray-200">
                        {branch.imageUrl ? <img src={branch.imageUrl} className="w-full h-full object-cover" alt="" /> : <Store size={14} className="m-auto mt-1.5 text-gray-400" />}
                      </div>
                      <p className={`text-sm font-bold truncate ${branchInfo?.id === branch.id ? 'text-blue-600' : 'text-gray-700'}`}>{branch.name}</p>
                      {branch.isMain && <span className="ml-2 bg-orange-100 text-orange-600 text-[8px] px-1 rounded-sm font-bold uppercase">Trụ sở</span>}
                    </div>
                    <p className="text-[10px] text-gray-400 truncate mt-0.5 flex items-center ml-11"><MapPin size={10} className="mr-1" /> {branch.address || 'Chưa cập nhật địa chỉ'}</p>
                  </div>
                  {branchInfo?.id === branch.id && <div className="bg-blue-600 rounded-full p-1"><CheckCircle2 size={12} className="text-white" /></div>}
                </div>
              ))}
            </div>
            <div className="p-2 bg-gray-50 text-center border-t">
              <button onClick={onNavigateBranches} className="text-[10px] text-blue-600 font-bold hover:underline">Quản lý danh sách chi nhánh</button>
            </div>
          </div>
        )}
      </div>

      <button onClick={() => onTabChange('tables')} className={`shrink-0 px-2 sm:px-4 py-1.5 rounded-t text-sm font-bold flex items-center transition-colors ${posTab === 'tables' ? 'bg-white text-[#0070f4]' : 'text-white hover:bg-blue-600'}`}>
        <Grid size={16} className="mr-1 sm:mr-2" /> <span className="hidden sm:inline">Phòng bàn</span><span className="sm:hidden">Bàn</span>
      </button>
      <button onClick={() => onTabChange('menu')} className={`shrink-0 px-2 sm:px-4 py-1.5 rounded-t text-sm font-bold flex items-center transition-colors ${posTab === 'menu' ? 'bg-white text-[#0070f4]' : 'text-white hover:bg-blue-600'}`}>
        <ClipboardList size={16} className="mr-1 sm:mr-2" /> <span className="hidden sm:inline">Thực đơn</span><span className="sm:hidden">Món</span>
      </button>

      <POSMenuSearchControls
        searchTerm={searchTerm}
        loading={loading}
        onSearchTermChange={onSearchTermChange}
        onRefresh={onRefresh}
        onFocusSearch={onFocusSearch}
      />
      {refreshError && <span role="status" className="order-last w-full px-2 pb-1 text-[10px] font-bold text-red-100 sm:order-none sm:w-auto">{refreshError}</span>}
    </div>
  );
}
