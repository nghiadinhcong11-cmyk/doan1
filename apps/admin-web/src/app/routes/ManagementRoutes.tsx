import { Navigate, Route, Routes, useNavigate } from 'react-router-dom';
import Navbar from '../../components/Navbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import ChatBot from '../../components/ChatBot';
import ManagementLayout from '../layouts/ManagementLayout';
import OperationsLayout from '../layouts/OperationsLayout';
import SettingsLayout from '../layouts/SettingsLayout';
import { Dashboard, InvoiceHistory, BusinessInsights } from '../../features/analytics';
import { ProductManagement, ToppingManagement, PromotionManagement } from '../../features/catalog';
import { TableManagement, ReservationManagement, CustomerManagement, ExpenseManagement } from '../../features/operations';
import { EmployeeManagement, AttendanceManagement, ShiftManagement, WorkSchedulePage } from '../../features/hrm';
import { BranchManagement, SystemSettings, SupportPage, ReceiptSettingsPage } from '../../features/settings';
import { POSPage, PrintTemplates } from '../../features/pos';
import { KitchenPage, KitchenHistoryPage } from '../../features/kitchen';
import { InventoryPage } from '../../features/inventory';
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

  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <Navbar onLogout={onLogout} userName={userName} userRole={userRole} />
      <Routes>
        <Route element={<ManagementLayout />}>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/business-insights" element={<BusinessInsights />} />
          <Route path="/notifications" element={<NotificationHistoryPage role={userRole} />} />
          <Route path="/products" element={<ProductManagement />} />
          <Route path="/expenses" element={<ExpenseManagement />} />
          <Route path="/inventory" element={<InventoryPage view="overview" />} />
          <Route path="/inventory/items" element={<InventoryPage view="items" />} />
          <Route path="/inventory/receipts" element={<InventoryPage view="receipts" />} />
          <Route path="/inventory/receipts/:id" element={<InventoryPage view="receipts" />} />
          <Route path="/inventory/issues" element={<InventoryPage view="issues" />} />
          <Route path="/inventory/issues/:id" element={<InventoryPage view="issues" />} />
          <Route path="/inventory/history" element={<InventoryPage view="history" />} />
          <Route path="/employees" element={<EmployeeManagement />} />
          <Route path="/attendance" element={<AttendanceManagement />} />
          <Route path="/schedule" element={<WorkSchedulePage />} />
          <Route path="/customers" element={<CustomerManagement />} />
          <Route path="/promotions" element={<PromotionManagement />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/toppings" element={<ToppingManagement />} />
          <Route path="/support" element={<SupportPage />} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Route>

        <Route element={userRole === 'admin' ? <SettingsLayout /> : <Navigate to="/forbidden" replace />}>
          <Route path="/settings" element={<SystemSettings />} />
          <Route path="/settings/receipt" element={<ReceiptSettingsPage />} />
          <Route path="/print-templates" element={<PrintTemplates />} />
          <Route path="/branches" element={<BranchManagement />} />
        </Route>

        <Route element={<OperationsLayout mainClassName="h-[calc(100vh-48px)] overflow-auto" />}>
          <Route path="/pos" element={<POSPage onLogout={() => navigate('/dashboard')} userName={userName} userRole={userRole} userPosition={userPosition} />} />
          <Route path="/tables" element={<TableManagement />} />
          <Route path="/invoices" element={<InvoiceHistory />} />
          <Route path="/shifts" element={<ShiftManagement />} />
          <Route path="/reservations" element={<ReservationManagement />} />
          <Route path="/kitchen" element={<KitchenPage />} />
          <Route path="/kitchen/history" element={<KitchenHistoryPage />} />
        </Route>
      </Routes>
      <ChatBot />
    </div>
  );
}
