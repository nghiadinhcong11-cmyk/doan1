import { NavLink, Outlet } from 'react-router-dom';
import { Building2, FileText, Printer, Settings } from 'lucide-react';

const settingsNavigation = [
  { path: '/settings', label: 'Cấu hình nhà hàng', icon: Settings, end: true },
  { path: '/settings/receipt', label: 'Hóa đơn & in bill', icon: FileText },
  { path: '/print-templates', label: 'Mẫu in', icon: Printer },
  { path: '/branches', label: 'Quản lý chi nhánh', icon: Building2 },
];

function SettingsNav({ mobile = false }: { mobile?: boolean }) {
  return (
    <nav aria-label="Điều hướng cấu hình" className={mobile ? 'flex gap-2 overflow-x-auto pb-1' : 'space-y-1'}>
      {settingsNavigation.map(({ path, label, icon: Icon, end }) => (
        <NavLink
          key={path}
          to={path}
          end={end}
          className={({ isActive }) => mobile
            ? `inline-flex shrink-0 items-center gap-2 rounded-lg px-3 py-2 text-sm font-semibold ${isActive ? 'bg-blue-600 text-white shadow-sm' : 'bg-white text-slate-600 ring-1 ring-slate-200'}`
            : `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-semibold transition-colors ${isActive ? 'bg-blue-600 text-white shadow-sm' : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'}`}
        >
          <Icon size={17} />
          {label}
        </NavLink>
      ))}
    </nav>
  );
}

/** Shared owner-only configuration shell; authorization remains at the route/API layers. */
export default function SettingsLayout() {
  return (
    <div className="min-h-full bg-slate-50 p-4 sm:p-6">
      <div className="mx-auto flex min-h-full max-w-[1600px] flex-col gap-4 lg:flex-row">
        <aside className="hidden w-64 shrink-0 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm lg:block">
          <h1 className="mb-4 flex items-center gap-2 px-2 text-base font-extrabold text-slate-900"><Settings size={18} className="text-blue-600" />Cấu hình</h1>
          <SettingsNav />
        </aside>
        <div className="lg:hidden"><SettingsNav mobile /></div>
        <section className="min-w-0 flex-1 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
          <Outlet />
        </section>
      </div>
    </div>
  );
}
