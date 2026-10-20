import React, { useCallback, useEffect, useRef, useState } from 'react';
import { ClipboardList, ChevronDown, ChevronRight, Loader2, RefreshCw, Utensils } from 'lucide-react';
import { Link } from 'react-router-dom';
import { API_URL } from '../../../config';

interface StaffTable { id: string; name: string; areaId?: string; areaName?: string; status?: string; }
interface StaffArea { id: string; name: string; displayOrder?: number; branchId?: string; }
interface StaffOverviewOrderCounts { pendingCount: number; activeCount: number; completedTodayCount: number; }

const StaffOverviewPage = () => {
  const branchName = localStorage.getItem('selectedBranchName') || 'Chi nhánh được phân công';
  const branchId = localStorage.getItem('selectedBranchId') || '';
  const [tables, setTables] = useState<StaffTable[]>([]);
  const [areas, setAreas] = useState<StaffArea[]>([]);
  const [collapsedAreas, setCollapsedAreas] = useState<Record<string, boolean>>({});
  const [orderCounts, setOrderCounts] = useState<StaffOverviewOrderCounts>({ pendingCount: 0, activeCount: 0, completedTodayCount: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const mountedRef = useRef(false);
  const refreshInFlightRef = useRef(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  const refresh = useCallback(async () => {
    if (!branchId) { setError('Tài khoản chưa có chi nhánh được phân công.'); setLoading(false); return; }
    if (refreshInFlightRef.current) return;

    refreshInFlightRef.current = true;
    const controller = new AbortController();
    abortControllerRef.current = controller;
    setError(null);
    try {
      const [tableResponse, areaResponse, orderCountsResponse] = await Promise.all([
        fetch(`${API_URL}/api/Table?isActive=true&branchId=${encodeURIComponent(branchId)}`, { signal: controller.signal }),
        fetch(`${API_URL}/api/Area?branchId=${encodeURIComponent(branchId)}`, { signal: controller.signal }),
        fetch(`${API_URL}/api/Order/staff-overview-counts?branchId=${encodeURIComponent(branchId)}`, { signal: controller.signal })
      ]);
      if (![tableResponse, areaResponse, orderCountsResponse].every(response => response.ok)) throw new Error('Không thể tải dữ liệu phục vụ.');
      const [tablesData, areasData, countsData] = await Promise.all([
        tableResponse.json(),
        areaResponse.json(),
        orderCountsResponse.json()
      ]);
      if (!mountedRef.current) return;
      setTables(tablesData);
      setAreas(areasData);
      setOrderCounts(countsData);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      if (!mountedRef.current) return;
      setError(e instanceof Error ? e.message : 'Không thể tải dữ liệu phục vụ.');
    } finally {
      if (abortControllerRef.current === controller) abortControllerRef.current = null;
      refreshInFlightRef.current = false;
      if (mountedRef.current) setLoading(false);
    }
  }, [branchId]);

  useEffect(() => {
    mountedRef.current = true;
    void refresh();
    const refreshIfVisible = () => {
      if (document.visibilityState === 'visible') void refresh();
    };
    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') void refresh();
    };
    const timer = window.setInterval(refreshIfVisible, 30000);
    document.addEventListener('visibilitychange', handleVisibilityChange);
    return () => {
      mountedRef.current = false;
      window.clearInterval(timer);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
      abortControllerRef.current?.abort();
      abortControllerRef.current = null;
      refreshInFlightRef.current = false;
    };
  }, [refresh]);

  const occupied = tables.filter(table => table.status === 'Có khách').length;
  const empty = tables.filter(table => table.status === 'Trống').length;
  const groupedAreas = areas.map(area => ({
    ...area,
    tables: tables.filter(table => table.areaId === area.id)
  }));
  const unassignedTables = tables.filter(table => !table.areaId || !areas.some(area => area.id === table.areaId));
  const displayAreas = unassignedTables.length > 0
    ? [...groupedAreas, { id: '__unassigned__', name: 'Chưa phân khu vực', tables: unassignedTables }]
    : groupedAreas;
  const cards = [
    ['Tổng số bàn', tables.length, 'text-blue-700 bg-blue-50'],
    ['Bàn trống', empty, 'text-emerald-700 bg-emerald-50'],
    ['Đang có khách', occupied, 'text-orange-700 bg-orange-50'],
    ['Đơn chờ xử lý', orderCounts.pendingCount, 'text-amber-700 bg-amber-50'],
    ['Đơn đang phục vụ', orderCounts.activeCount, 'text-indigo-700 bg-indigo-50'],
    ['Hoàn thành hôm nay', orderCounts.completedTodayCount, 'text-green-700 bg-green-50']
  ];

  if (loading) return <div className="flex min-h-full items-center justify-center bg-slate-50"><Loader2 className="animate-spin text-blue-600" size={32} /></div>;
  return <div className="min-h-full bg-slate-50 p-4 text-slate-800 md:p-7">
    <div className="mb-6 flex flex-wrap items-center justify-between gap-3"><div><h1 className="text-2xl font-black tracking-tight">Tổng quan phục vụ</h1><p className="mt-1 text-sm text-slate-500">{branchName} · Số liệu cập nhật tự động mỗi 30 giây</p></div><button onClick={() => void refresh()} className="inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-bold hover:bg-slate-50"><RefreshCw size={15}/> Làm mới</button></div>
    {error && <div role="alert" className="mb-5 rounded-xl border border-red-200 bg-red-50 p-3 text-sm font-semibold text-red-700">{error}</div>}
    <div className="grid grid-cols-2 gap-3 md:grid-cols-3 xl:grid-cols-6">{cards.map(([label, value, style]) => <div key={label} className={`rounded-2xl p-4 ${style}`}><p className="text-xs font-bold uppercase tracking-wide opacity-80">{label}</p><p className="mt-2 text-3xl font-black">{value}</p></div>)}</div>
    <div className="mt-6"><Link to="/staff/sales" className="flex items-center gap-4 rounded-2xl bg-blue-600 p-5 text-white shadow-lg shadow-blue-500/20 hover:bg-blue-700"><ClipboardList size={28}/><span><b className="block text-lg">Bán hàng / Gọi món</b><small>Chọn bàn, thêm món và gửi bếp</small></span></Link></div>
    <div className="mt-6 rounded-2xl bg-white p-5 shadow-sm ring-1 ring-slate-200"><h2 className="mb-4 flex items-center gap-2 text-lg font-black"><Utensils className="text-orange-500" size={20}/> Trạng thái bàn</h2>{displayAreas.length === 0 ? <p className="text-sm text-slate-500">Không có khu vực hoặc bàn hoạt động.</p> : <div className="space-y-4">{displayAreas.map(area => { const occupiedInArea = area.tables.filter(table => table.status === 'Có khách').length; const emptyInArea = area.tables.filter(table => table.status === 'Trống').length; const collapsed = Boolean(collapsedAreas[area.id]); return <section key={area.id} className="rounded-2xl border border-slate-200"><button type="button" onClick={() => setCollapsedAreas(previous => ({ ...previous, [area.id]: !collapsed }))} className="flex w-full items-center justify-between gap-3 p-4 text-left"><span className="flex min-w-0 items-center gap-2">{collapsed ? <ChevronRight size={18}/> : <ChevronDown size={18}/>}<span className="truncate font-black">{area.name}</span></span><span className="shrink-0 text-[11px] font-bold text-slate-500">{area.tables.length} bàn · Trống {emptyInArea} · Có khách {occupiedInArea}</span></button>{!collapsed && <div className="grid grid-cols-2 gap-3 border-t border-slate-100 p-4 sm:grid-cols-3 lg:grid-cols-6">{area.tables.map(table => <Link key={table.id} to={`/staff/sales?tableId=${encodeURIComponent(table.id)}`} className={`min-h-20 rounded-xl border p-3 transition hover:-translate-y-0.5 hover:shadow-sm ${table.status === 'Có khách' ? 'border-orange-200 bg-orange-50' : table.status === 'Đã đặt' ? 'border-blue-200 bg-blue-50' : 'border-slate-200 bg-slate-50'}`}><p className="truncate font-bold">{table.name}</p><p className="mt-1 text-xs font-semibold text-slate-500">{table.status || 'Chưa xác định'}</p></Link>)}{area.tables.length === 0 && <p className="col-span-full text-sm text-slate-500">Khu vực chưa có bàn.</p>}</div>}</section>; })}</div>}</div>
  </div>;
};

export default StaffOverviewPage;
