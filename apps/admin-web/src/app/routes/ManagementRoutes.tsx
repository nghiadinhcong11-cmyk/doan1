import React, { useState } from 'react';
import { Navigate, Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import Navbar from '../../components/Navbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import ChatBot from '../../components/ChatBot';
import ManagementLayout from '../layouts/ManagementLayout';
import OperationsLayout from '../layouts/OperationsLayout';
import SettingsLayout from '../layouts/SettingsLayout';
import { Dashboard, InvoiceHistory } from '../../features/analytics';
import { ProductManagement, ToppingManagement, PromotionManagement } from '../../features/catalog';
import { TableManagement, ReservationManagement, CustomerManagement, ExpenseManagement } from '../../features/operations';
import { EmployeeManagement, AttendanceManagement, ShiftManagement, WorkSchedulePage } from '../../features/hrm';
import { BranchManagement, SystemSettings, SupportPage, ReceiptSettingsPage } from '../../features/settings';
import { POSPage } from '../../features/pos';
import { KitchenPage, KitchenHistoryPage } from '../../features/kitchen';
import { InventoryPage, StockReceiptPrintButton } from '../../features/inventory';
import { ProfilePage } from '../../features/auth';
import NotificationHistoryPage from '../../features/notifications/NotificationHistoryPage';

type ManagementRoutesProps = {
  userRole: 'admin' | 'manager';
  userName: string;
  userPosition: string;
  onLogout: () => void;
};

/** Route map for admin/manager sessions. Route areas only select content layout. */
export default function ManagementRoutes({ userRole, userName, userPosition, onLogout }: ManagementRoutesProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  // Mount only the active page. Keeping every previously visited page mounted
  // leaves its effects/listeners alive and makes each navigation accumulate API
  // requests against Supabase.
  const wasOpened = (path: string) => location.pathname === path;

  return (
    <div className="h-screen overflow-hidden bg-[#f0f2f5]">
      <Navbar onLogout={onLogout} userName={userName} userRole={userRole} sidebarCollapsed={sidebarCollapsed} onSidebarToggle={() => setSidebarCollapsed(value => !value)} />
      <div className={`admin-management-content ${sidebarCollapsed ? 'lg:pl-20' : 'lg:pl-72'}`}>
      {wasOpened('/dashboard') && (
        <main className={location.pathname === '/dashboard' ? 'h-[calc(100vh-48px)] min-w-0 overflow-auto lg:h-screen' : 'hidden'}>
          <Dashboard />
        </main>
      )}
      {wasOpened('/products') && (
        <main className={location.pathname === '/products' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <ProductManagement />
        </main>
      )}
      {wasOpened('/profile') && (
        <main className={location.pathname === '/profile' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <ProfilePage />
        </main>
      )}
      {wasOpened('/expenses') && (
        <main className={location.pathname === '/expenses' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <ExpenseManagement />
        </main>
      )}
      {wasOpened('/inventory') && (
        <main className={location.pathname === '/inventory' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <InventoryPage view="overview" />
        </main>
      )}
      {wasOpened('/inventory/items') && (
        <main className={location.pathname === '/inventory/items' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <InventoryPage view="items" />
        </main>
      )}
      {wasOpened('/inventory/receipts') && (
        <main className={location.pathname === '/inventory/receipts' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <InventoryPage view="receipts" />
        </main>
      )}
      {wasOpened('/inventory/issues') && (
        <main className={location.pathname === '/inventory/issues' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <InventoryPage view="issues" />
        </main>
      )}
      {wasOpened('/inventory/history') && (
        <main className={location.pathname === '/inventory/history' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <InventoryPage view="history" />
        </main>
      )}
      {wasOpened('/employees') && (
        <main className={location.pathname === '/employees' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <EmployeeManagement />
        </main>
      )}
      {wasOpened('/attendance') && (
        <main className={location.pathname === '/attendance' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <AttendanceManagement />
        </main>
      )}
      {wasOpened('/schedule') && (
        <main className={location.pathname === '/schedule' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <WorkSchedulePage />
        </main>
      )}
      {wasOpened('/customers') && (
        <main className={location.pathname === '/customers' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <CustomerManagement />
        </main>
      )}
      {wasOpened('/promotions') && (
        <main className={location.pathname === '/promotions' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <PromotionManagement />
        </main>
      )}
      {wasOpened('/toppings') && (
        <main className={location.pathname === '/toppings' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <ToppingManagement />
        </main>
      )}
      {wasOpened('/support') && (
        <main className={location.pathname === '/support' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <SupportPage />
        </main>
      )}
      {userRole === 'admin' && ['/settings', '/settings/receipt', '/print-templates', '/branches'].some(path => wasOpened(path)) && (
        <SettingsLayout>
          {wasOpened('/settings') && (
            <div className={location.pathname === '/settings' ? 'h-full overflow-auto' : 'hidden'}>
              <SystemSettings />
            </div>
          )}
          {wasOpened('/settings/receipt') && (
            <div className={location.pathname === '/settings/receipt' ? 'h-full overflow-auto' : 'hidden'}>
              <ReceiptSettingsPage />
            </div>
          )}
          {wasOpened('/print-templates') && (
            <div className={location.pathname === '/print-templates' ? 'h-full overflow-auto' : 'hidden'}>
              <ReceiptSettingsPage />
            </div>
          )}
          {wasOpened('/branches') && (
            <div className={location.pathname === '/branches' ? 'h-full overflow-hidden' : 'hidden'}>
              <BranchManagement />
            </div>
          )}
        </SettingsLayout>
      )}
      {wasOpened('/tables') && (
        <main className={location.pathname === '/tables' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <TableManagement />
        </main>
      )}
      {wasOpened('/shifts') && (
        <main className={location.pathname === '/shifts' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <ShiftManagement />
        </main>
      )}
      {wasOpened('/reservations') && (
        <main className={location.pathname === '/reservations' ? 'h-[calc(100vh-48px)] overflow-auto' : 'hidden'}>
          <ReservationManagement />
        </main>
      )}
      <Routes>
        <Route element={<ManagementLayout />}>
          <Route path="/dashboard" element={null} />
          <Route path="/notifications" element={<NotificationHistoryPage role={userRole} />} />
          <Route path="/products" element={null} />
          <Route path="/expenses" element={null} />
          <Route path="/inventory" element={null} />
          <Route path="/inventory/items" element={null} />
          <Route path="/inventory/receipts" element={null} />
          <Route path="/inventory/receipts/:id" element={<><StockReceiptPrintButton /><InventoryPage view="receipts" /></>} />
          <Route path="/inventory/issues" element={null} />
          <Route path="/inventory/issues/:id" element={<InventoryPage view="issues" />} />
          <Route path="/inventory/history" element={null} />
          <Route path="/employees" element={null} />
          <Route path="/attendance" element={null} />
          <Route path="/schedule" element={null} />
          <Route path="/customers" element={null} />
          <Route path="/promotions" element={null} />
          <Route path="/profile" element={null} />
          <Route path="/toppings" element={null} />
          <Route path="/support" element={null} />
          <Route path="/settings" element={userRole === 'admin' ? null : <Navigate to="/forbidden" replace />} />
          <Route path="/settings/receipt" element={userRole === 'admin' ? null : <Navigate to="/forbidden" replace />} />
          <Route path="/print-templates" element={userRole === 'admin' ? null : <Navigate to="/forbidden" replace />} />
          <Route path="/branches" element={userRole === 'admin' ? null : <Navigate to="/forbidden" replace />} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Route>

        <Route element={<OperationsLayout mainClassName="h-[calc(100vh-48px)] overflow-auto lg:h-screen" />}>
          <Route path="/pos" element={<POSPage onLogout={onLogout} userName={userName} userRole={userRole} userPosition={userPosition} />} />
          <Route path="/tables" element={null} />
          <Route path="/invoices" element={<InvoiceHistory />} />
          <Route path="/shifts" element={null} />
          <Route path="/reservations" element={null} />
          <Route path="/kitchen" element={<KitchenPage userRole={userRole} />} />
          <Route path="/kitchen/history" element={<KitchenHistoryPage />} />
        </Route>
      </Routes>
      </div>
      {userRole === 'admin' && <ChatBot />}
    </div>
  );
}
