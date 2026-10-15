import React, { useState, useEffect } from 'react';
import { Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { LoginPage } from './features/auth';
import { installApiAuthInterceptor } from './apiClient';
import ManagementRoutes from './app/routes/ManagementRoutes';
import CashierRoutes from './app/routes/CashierRoutes';
import StaffRoutes from './app/routes/StaffRoutes';
import KitchenRoutes from './app/routes/KitchenRoutes';

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

  if (userRole === 'cashier') {
    return <CashierRoutes userName={userName} userPosition={userPosition} onLogout={handleLogout} />;
  }

  if (userRole === 'employee') {
    return <StaffRoutes userName={userName} userPosition={userPosition} onLogout={handleLogout} />;
  }

  if (userRole === 'kitchen') {
    return <KitchenRoutes userName={userName} onLogout={handleLogout} />;
  }

  // Admin and manager share the management auth/session flow. Their POS/KDS
  // routes are composed by ManagementRoutes under the operations shell.
  return (
    <ManagementRoutes
      userRole={userRole === 'admin' ? 'admin' : 'manager'}
      userName={userName}
      userPosition={userPosition}
      onLogout={handleLogout}
    />
  );
}

export default App;
