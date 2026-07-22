import React, { useState, useEffect } from 'react';
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
  const [activeTab, setActiveTab] = useState('dashboard');

  // Kiểm tra trạng thái đăng nhập từ localStorage khi khởi động
  useEffect(() => {
    const authStatus = localStorage.getItem('isLoggedIn');
    const savedRole = localStorage.getItem('userRole') as 'admin' | 'cashier' | null;
    const savedName = localStorage.getItem('userName') || '';

    if (authStatus === 'true' && savedRole) {
      setIsLoggedIn(true);
      setUserRole(savedRole);
      setUserName(savedName);
      // Nếu là thu ngân, luôn để activeTab là pos
      if (savedRole === 'cashier') {
        setActiveTab('pos');
      }
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
      setActiveTab('pos');
    } else {
      setActiveTab('dashboard');
    }
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    setUserRole(null);
    setUserName('');
    localStorage.removeItem('isLoggedIn');
    localStorage.removeItem('userRole');
    localStorage.removeItem('userName');
    setActiveTab('dashboard'); // Reset về trang chủ sau khi logout
  };

  const renderContent = () => {
    switch (activeTab) {
      case 'dashboard':
        return <Dashboard />;
      case 'products':
        return <ProductManagement />;
      case 'tables':
        return <TableManagement />;
      case 'invoices':
        return <InvoiceHistory />;
      case 'expenses':
        return <ExpenseManagement />;
      case 'employees':
        return <EmployeeManagement />;
      case 'users':
        return <UserManagement />;
      case 'branches':
        return <BranchManagement />;
      case 'print-templates':
        return <PrintTemplates />;
      case 'profile':
        return <ProfilePage />;
      case 'settings':
        return <SystemSettings setActiveTab={setActiveTab} />;
      case 'pos':
        return <POSPage setActiveTab={setActiveTab} userName={userName} userRole={userRole} />;
      default:
        return <Dashboard />;
    }
  };

  // Nếu chưa đăng nhập, chỉ hiển thị trang Login
  if (!isLoggedIn) {
    return <LoginPage onLogin={handleLogin} />;
  }

  // GIAO DIỆN THU NGÂN (Tách biệt hoàn toàn)
  if (userRole === 'cashier') {
    return (
      <div className="min-h-screen bg-[#f0f2f5]">
        <POSPage setActiveTab={handleLogout} userName={userName} userRole={userRole} />
      </div>
    );
  }

  // GIAO DIỆN QUẢN TRỊ (Dành cho chủ quán)
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <Navbar
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        onLogout={handleLogout}
        userName={userName}
      />

      <main>
        {renderContent()}
      </main>
    </div>
  );
}

export default App;
