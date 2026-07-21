import React, { useState, useEffect } from 'react';
import Navbar from './components/Navbar';
import Dashboard from './pages/Dashboard';
import ProductManagement from './pages/ProductManagement';
import TableManagement from './pages/TableManagement';
import InvoiceHistory from './pages/InvoiceHistory';
import ExpenseManagement from './pages/ExpenseManagement';
import EmployeeManagement from './pages/EmployeeManagement';
import ProfilePage from './pages/ProfilePage';
import SystemSettings from './pages/SystemSettings';
import POSPage from './pages/POSPage';
import LoginPage from './pages/LoginPage';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [activeTab, setActiveTab] = useState('dashboard');

  // Kiểm tra trạng thái đăng nhập từ localStorage khi khởi động
  useEffect(() => {
    const authStatus = localStorage.getItem('isLoggedIn');
    if (authStatus === 'true') {
      setIsLoggedIn(true);
    }
  }, []);

  const handleLogin = () => {
    setIsLoggedIn(true);
    localStorage.setItem('isLoggedIn', 'true');
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    localStorage.removeItem('isLoggedIn');
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
      case 'profile':
        return <ProfilePage />;
      case 'settings':
        return <SystemSettings />;
      case 'pos':
        return <POSPage setActiveTab={setActiveTab} />;
      default:
        return <Dashboard />;
    }
  };

  // Nếu chưa đăng nhập, chỉ hiển thị trang Login
  if (!isLoggedIn) {
    return <LoginPage onLogin={handleLogin} />;
  }

  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      {/* Chỉ hiện Navbar nếu không phải đang ở trang POS */}
      {activeTab !== 'pos' && (
        <Navbar
          activeTab={activeTab}
          setActiveTab={setActiveTab}
          onLogout={handleLogout}
        />
      )}

      <main>
        {renderContent()}
      </main>
    </div>
  );
}

export default App;
