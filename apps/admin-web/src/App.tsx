import React, { useState, useEffect } from 'react';
import { Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import Navbar from './components/Navbar';
import Dashboard from './pages/Dashboard';
import ProductManagement from './pages/ProductManagement';
import TableManagement from './pages/TableManagement';
import InvoiceHistory from './pages/InvoiceHistory';
import ExpenseManagement from './pages/ExpenseManagement';
import EmployeeManagement from './pages/EmployeeManagement';
import UserManagement from './pages/UserManagement';
import BranchManagement from './pages/BranchManagement';
import PrintTemplates from './pages/PrintTemplates';
import ProfilePage from './pages/ProfilePage';
import SystemSettings from './pages/SystemSettings';
import POSPage from './pages/POSPage';
import LoginPage from './pages/LoginPage';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [userRole, setUserRole] = useState<'admin' | 'cashier' | null>(null);
  const [userName, setUserName] = useState('');
  const navigate = useNavigate();
  const location = useLocation();

  // Kiểm tra trạng thái đăng nhập từ localStorage khi khởi động
  useEffect(() => {
    const authStatus = localStorage.getItem('isLoggedIn');
    const savedRole = localStorage.getItem('userRole') as 'admin' | 'cashier' | null;
    const savedName = localStorage.getItem('userName') || '';

    if (authStatus === 'true' && savedRole) {
      setIsLoggedIn(true);
      setUserRole(savedRole);
      setUserName(savedName);
    }
  }, []);

  const handleLogin = (role: 'admin' | 'cashier', fullName: string, branchId?: string) => {
    setIsLoggedIn(true);
    setUserRole(role);
    setUserName(fullName);
    localStorage.setItem('isLoggedIn', 'true');
    localStorage.setItem('userRole', role);
    localStorage.setItem('userName', fullName);

    if (branchId) {
      localStorage.setItem('selectedBranchId', branchId);
    }

    if (role === 'cashier') {
      navigate('/pos');
    } else {
      navigate('/dashboard');
    }
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setUserRole(null);
    setUserName('');
    localStorage.removeItem('isLoggedIn');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userName');
    navigate('/');
  };

  // Nếu chưa đăng nhập, chỉ cho phép ở trang Login
  if (!isLoggedIn) {
    return (
      <Routes>
        <Route path="/" element={<LoginPage onLogin={handleLogin} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    );
  }

  // GIAO DIỆN THU NGÂN (Tách biệt hoàn toàn)
  if (userRole === 'cashier') {
    return (
      <div className="min-h-screen bg-[#f0f2f5]">
        <Routes>
          <Route path="/pos" element={<POSPage setActiveTab={() => {}} userName={userName} userRole={userRole} />} />
          <Route path="*" element={<Navigate to="/pos" replace />} />
        </Routes>
      </div>
    );
  }

  // GIAO DIỆN QUẢN TRỊ (Dành cho chủ quán)
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <Navbar
        onLogout={handleLogout}
        userName={userName}
      />

      <main>
        <Routes>
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/products" element={<ProductManagement />} />
          <Route path="/tables" element={<TableManagement />} />
          <Route path="/invoices" element={<InvoiceHistory />} />
          <Route path="/expenses" element={<ExpenseManagement />} />
          <Route path="/employees" element={<EmployeeManagement />} />
          <Route path="/users" element={<UserManagement />} />
          <Route path="/branches" element={<BranchManagement />} />
          <Route path="/print-templates" element={<PrintTemplates />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/settings" element={<SystemSettings />} />
          <Route path="/pos" element={<POSPage setActiveTab={() => navigate('/dashboard')} userName={userName} userRole={userRole} />} />
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default App;
