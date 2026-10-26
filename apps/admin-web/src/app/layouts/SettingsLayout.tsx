import { NavLink, Outlet } from 'react-router-dom';
import { Building2, FileText, Settings, ShieldCheck, WalletCards } from 'lucide-react';
import type { ReactNode } from 'react';

type SettingsItem = {
  path?: string;
  label: string;
  icon: typeof Settings;
  end?: boolean;
  comingSoon?: boolean;
};

const settingsNavigation: Array<{ label: string; items: SettingsItem[] }> = [
  { label: 'Nhà hàng & tài chính', items: [{ path: '/settings', label: 'Cấu hình nhà hàng, thuế & phí', icon: WalletCards, end: true }] },
  { label: 'Hóa đơn & in ấn', items: [
    { path: '/settings/receipt', label: 'Cấu hình in ấn', icon: FileText }
  ] },
  { label: 'Chi nhánh', items: [{ path: '/branches', label: 'Quản lý chi nhánh', icon: Building2 }] },
  { label: 'Sắp ra mắt', items: [
    { label: 'Bảo mật & hệ thống', icon: ShieldCheck, comingSoon: true },
    { label: 'Chốt sổ & khóa dữ liệu', icon: Settings, comingSoon: true }
  ] }
];

function SettingsNav() {
  return (
    <nav aria-label="Điều hướng cấu hình" className="space-y-1">
      {settingsNavigation.map(group => (
        <div key={group.label} className="mb-4">
          <p className="px-3 pb-1 text-[10px] font-black uppercase tracking-widest text-slate-400">{group.label}</p>
          {group.items.map(({ path, label, icon: Icon, end, comingSoon }) => comingSoon ? (
            <div key={label} className="flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold text-slate-400">
              <Icon size={17} />
              <span className="flex-1">{label}</span>
              <span className="text-[9px] font-black uppercase tracking-wide">Sắp ra mắt</span>
            </div>
          ) : (
            <NavLink
              key={`${path}-${label}`}
              to={path!}
              end={end}
              className={({ isActive }) => `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold transition-colors ${isActive ? 'bg-blue-600 text-white shadow-sm' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'}`}
            >
              <Icon size={17} />
              {label}
            </NavLink>
          ))}
        </div>
      ))}
    </nav>
  );
}

/** Shared owner-only configuration shell; authorization remains at the route/API layers. */
export default function SettingsLayout({ children }: { children?: ReactNode }) {
  return (
    <div className="h-[calc(100vh-48px)] min-h-0 overflow-hidden bg-slate-50 p-4 sm:p-6 lg:h-screen">
      <div className="mx-auto flex h-full min-h-0 max-w-[1600px] gap-4">
        <main className="min-h-0 min-w-0 flex-1 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
          {children ?? <Outlet />}
        </main>
        <aside className="hidden w-64 shrink-0 overflow-y-auto rounded-2xl border border-slate-200 bg-white p-4 shadow-sm xl:block">
          <h1 className="mb-4 flex items-center gap-2 px-2 text-base font-extrabold text-slate-900"><Settings size={18} className="text-blue-600" />Cấu hình</h1>
          <SettingsNav />
        </aside>
      </div>
    </div>
  );
}
