import React, { useState, useEffect } from 'react';
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import Navbar from './components/Navbar';
import CashierNavbar from './components/CashierNavbar';
import KitchenNavbar from './components/KitchenNavbar';
import StaffNavbar from './components/StaffNavbar';
import { Dashboard, InvoiceHistory, BusinessInsights } from './features/analytics';
import { ProductManagement, ToppingManagement, PromotionManagement } from './features/catalog';
import { TableManagement, ReservationManagement, CustomerManagement, ExpenseManagement } from './features/operations';
import { EmployeeManagement, AttendanceManagement, ShiftManagement, WorkSchedulePage, EmployeeProfile, EmployeeAttendance, EmployeeSchedule } from './features/hrm';
import { BranchManagement, SystemSettings, SupportPage, ReceiptSettingsPage } from './features/settings';
import { POSPage, TableStatusPage, PrintTemplates } from './features/pos';
import { KitchenPage, KitchenHistoryPage } from './features/kitchen';
import { InventoryPage } from './features/inventory';
import { LoginPage, ProfilePage } from './features/auth';
import ChatBot from './components/ChatBot';
import ForbiddenPage from './components/ForbiddenPage';
import { installApiAuthInterceptor } from './apiClient';

installApiAuthInterceptor();

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userRole, setUserRole] = useState<'admin' | 'manager' | 'employee' | 'cashier' | 'kitchen' | null>(null);
  const [userName, setUserName] = useState('');
  const [userPosition, setUserPosition] = useState('');
  const navigate = useNavigate();

  // Kiểm tra trạng thái đăng nhập từ localStorage khi khởi động
  useEffect(() => {
    const authStatus = localStorage.getItem('isLoggedIn');
    const savedRole = localStorage.getItem('userRole') as 'admin' | 'manager' | 'employee' | 'cashier' | 'kitchen' | null;
    const savedName = localStorage.getItem('userName') || '';
    const savedPosition = localStorage.getItem('userPosition') || '';

    if (authStatus === 'true' && savedRole) {
      setIsLoggedIn(true);
      setUserRole(savedRole);
      setUserName(savedName);
      setUserPosition(savedPosition);
    }
  }, []);

  const handleLogin = (role: 'admin' | 'manager' | 'employee' | 'cashier' | 'kitchen', fullName: string, branchId?: string, employeeId?: string, branchName?: string, position?: string, token?: string) => {
    // Replace all employee-system session state before accepting the server-issued role.
    ['isLoggedIn', 'userRole', 'userName', 'userPosition', 'adminToken', 'employeeId', 'selectedBranchId', 'selectedBranchName'].forEach(key => localStorage.removeItem(key));
    setIsLoggedIn(true);
    setUserRole(role);
    setUserName(fullName);
    setUserPosition(position || '');
    localStorage.setItem('isLoggedIn', 'true');
    localStorage.setItem('userRole', role);
    localStorage.setItem('userName', fullName);
    if (position) localStorage.setItem('userPosition', position);
    if (token) localStorage.setItem('adminToken', token);

    if (branchId) localStorage.setItem('selectedBranchId', branchId);
    if (branchName) localStorage.setItem('selectedBranchName', branchName);
    if (employeeId) localStorage.setItem('employeeId', employeeId);

    if (role === 'employee') {
      navigate('/staff/profile');
    } else if (role === 'cashier') {
      navigate('/pos');
    } else if (role === 'kitchen') {
      navigate('/kitchen');
    } else {
      navigate('/dashboard');
    }
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setUserRole(null);
    setUserName('');
    setUserPosition('');
    localStorage.removeItem('isLoggedIn');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userName');
    localStorage.removeItem('userPosition');
    localStorage.removeItem('adminToken');
    localStorage.removeItem('employeeId');
    localStorage.removeItem('selectedBranchId');
    localStorage.removeItem('selectedBranchName');
    localStorage.removeItem('pos_table_carts');
    localStorage.removeItem('pos_table_customers');
    localStorage.removeItem('pos_selected_table_id');
    navigate('/');
  };

  useEffect(() => {
    const onUnauthorized = () => handleLogout();
    const onForbidden = () => navigate('/forbidden');
    window.addEventListener('pos:unauthorized', onUnauthorized);
    window.addEventListener('pos:forbidden', onForbidden);
    return () => {
      window.removeEventListener('pos:unauthorized', onUnauthorized);
      window.removeEventListener('pos:forbidden', onForbidden);
    };
  }, [navigate]);

  // Nếu chưa đăng nhập, chỉ cho phép ở trang Login
  if (!isLoggedIn) {
    return (
      <Routes>
        <Route path="/" element={<LoginPage onLogin={handleLogin} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    );
  }

  // GIAO DIỆN THU NGÂN
  if (userRole === 'cashier') {
    return (
      <div className="min-h-screen bg-[#f0f2f5]">
        <CashierNavbar onLogout={handleLogout} userName={userName} userRole={userRole} userPosition={userPosition} />
        <main className="h-[calc(100dvh-48px)] min-h-0">
          <Routes>
            <Route path="/pos" element={<POSPage onLogout={handleLogout} userName={userName} userRole={userRole} userPosition={userPosition} />} />
            <Route path="/pos/invoices" element={<InvoiceHistory />} />
            <Route path="/pos/settings/receipt" element={<ReceiptSettingsPage />} />
            <Route path="/pos/attendance" element={<EmployeeAttendance />} />
            <Route path="/pos/schedule" element={<EmployeeSchedule />} />
            <Route path="/pos/shifts" element={<ShiftManagement />} />
            <Route path="/pos/reservations" element={<ReservationManagement />} />
            <Route path="/pos/profile" element={<EmployeeProfile userName={userName} onLogout={handleLogout} />} />
            <Route path="/forbidden" element={<ForbiddenPage />} />
            <Route path="*" element={<Navigate to="/pos" replace />} />
          </Routes>
        </main>
        <ChatBot />
      </div>
    );
  }

  if (userRole === 'employee') {
    return (
      <div className="min-h-screen bg-[#f0f2f5]">
        <StaffNavbar onLogout={handleLogout} userName={userName} />
        <main className="min-h-[calc(100dvh-48px)] overflow-auto">
          <Routes>
            <Route path="/staff" element={<Navigate to="/staff/profile" replace />} />
            <Route path="/staff/profile" element={<EmployeeProfile userName={userName} onLogout={handleLogout} />} />
            <Route path="/staff/attendance" element={<EmployeeAttendance />} />
            <Route path="/staff/schedule" element={<EmployeeSchedule />} />
            <Route path="/staff/inventory" element={<InventoryPage view="overview" operational />} />
            <Route path="/forbidden" element={<ForbiddenPage />} />
            <Route path="*" element={<Navigate to="/staff/profile" replace />} />
          </Routes>
        </main>
        <ChatBot />
      </div>
    );
  }

  // GIAO DIỆN NHÀ BẾP
  if (userRole === 'kitchen') {
    return (
      <div className="min-h-screen bg-[#f0f2f5]">
        <KitchenNavbar onLogout={handleLogout} userName={userName} />
        <main className="h-[calc(100dvh-48px)] min-h-0">
          <Routes>
            <Route path="/kitchen" element={<KitchenPage />} />
            <Route path="/kitchen/history" element={<KitchenHistoryPage />} />
            <Route path="/kitchen/tables" element={<TableStatusPage />} />
            <Route path="/kitchen/attendance" element={<EmployeeAttendance />} />
            <Route path="/kitchen/schedule" element={<EmployeeSchedule />} />
            <Route path="/kitchen/reservations" element={<ReservationManagement readOnly={true} />} />
            <Route path="/kitchen/profile" element={<EmployeeProfile userName={userName} onLogout={handleLogout} />} />
            <Route path="/forbidden" element={<ForbiddenPage />} />
            <Route path="*" element={<Navigate to="/kitchen" replace />} />
          </Routes>
        </main>
        <ChatBot />
      </div>
    );
  }

  // GIAO DIỆN QUẢN TRỊ
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <Navbar onLogout={handleLogout} userName={userName} userRole={userRole} />
      <main className="h-[calc(100vh-48px)] overflow-auto">
        <Routes>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/business-insights" element={<BusinessInsights />} />
          <Route path="/products" element={<ProductManagement />} />
          <Route path="/tables" element={<TableManagement />} />
          <Route path="/invoices" element={<InvoiceHistory />} />
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
          <Route path="/branches" element={userRole === 'admin' ? <BranchManagement /> : <Navigate to="/forbidden" replace />} />
          <Route path="/shifts" element={<ShiftManagement />} />
          <Route path="/reservations" element={<ReservationManagement />} />
          <Route path="/print-templates" element={<PrintTemplates />} />
          <Route path="/settings/receipt" element={<ReceiptSettingsPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/settings" element={userRole === 'admin' || userRole === 'manager' ? <SystemSettings /> : <Navigate to="/forbidden" replace />} />
          <Route path="/toppings" element={<ToppingManagement />} />
          <Route path="/kitchen" element={<KitchenPage />} />
          <Route path="/kitchen/history" element={<KitchenHistoryPage />} />
          <Route path="/support" element={<SupportPage />} />
          <Route path="/pos" element={<POSPage onLogout={() => navigate('/dashboard')} userName={userName} userRole={userRole} userPosition={userPosition} />} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </main>
      <ChatBot />
    </div>
  );
}

export default App;
