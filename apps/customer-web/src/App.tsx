import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import DigitalMenu from './pages/DigitalMenu';
import CustomerLogin from './pages/CustomerLogin';
import CustomerProfile from './pages/CustomerProfile';
import ReservationPage from './pages/ReservationPage';
import QRScan from './pages/QRScan';
import BottomNav from './components/BottomNav';
import CustomerHeader from './components/CustomerHeader';
import ChatBot from './components/ChatBot';
import { API_URL } from './config';

let customerFetchInstalled = false;
if (!customerFetchInstalled && typeof window !== 'undefined') {
  customerFetchInstalled = true;
  const nativeFetch = window.fetch.bind(window);
  window.fetch = (input: RequestInfo | URL, init?: RequestInit) => {
    const url = input instanceof Request ? input.url : String(input);
    if (!url.startsWith(API_URL)) return nativeFetch(input, init);
    const headers = new Headers(init?.headers || (input instanceof Request ? input.headers : undefined));
    const method = (init?.method || (input instanceof Request ? input.method : 'GET')).toUpperCase();
    const path = new URL(url, window.location.origin).pathname;
    const isPublicMenuRead = method === 'GET' && [
      '/api/Product',
      '/api/Topping',
      '/api/Branch',
      '/api/Promotion'
    ].includes(path);
    const isGuestBootstrap = path === '/api/guest/bootstrap' || path === '/api/guest/direct-session';
    const useGuestSession = sessionStorage.getItem('guestQrSession') === 'true' ||
      sessionStorage.getItem('guestDirectSession') === 'true';
    const token = useGuestSession ? sessionStorage.getItem('guestToken') : localStorage.getItem('customerToken');
    if (!isPublicMenuRead && !isGuestBootstrap && token && !headers.has('Authorization')) {
      headers.set('Authorization', `Bearer ${token}`);
    }
    return nativeFetch(input, { ...init, headers }).then(response => {
      if (response.status === 401) {
        if (useGuestSession) {
          sessionStorage.removeItem('guestToken');
          sessionStorage.removeItem('guestQrSession');
          sessionStorage.removeItem('guestDirectSession');
          sessionStorage.removeItem('guestDirectBranchId');
        } else {
          localStorage.removeItem('customerToken');
          localStorage.removeItem('customerInfo');
        }
        window.dispatchEvent(new Event('customer:unauthorized'));
      }
      return response;
    });
  };
}

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isGuest, setIsGuest] = useState(() => {
    if (typeof window === 'undefined') return false;
    return sessionStorage.getItem('customerGuestMode') === 'true' ||
      sessionStorage.getItem('guestQrSession') === 'true' ||
      sessionStorage.getItem('guestDirectSession') === 'true' ||
      Boolean(sessionStorage.getItem('guestToken'));
  });
  const [isLoginRequested, setIsLoginRequested] = useState(false);
  const [customerInfo, setCustomerInfo] = useState<any>(null);

  useEffect(() => {
    const onUnauthorized = () => {
      setIsLoggedIn(false);
      setCustomerInfo(null);
    };
    window.addEventListener('customer:unauthorized', onUnauthorized);
    return () => window.removeEventListener('customer:unauthorized', onUnauthorized);
  }, []);

  useEffect(() => {
    const guestMode = sessionStorage.getItem('customerGuestMode') === 'true';
    if (guestMode) {
      localStorage.removeItem('customerToken');
      localStorage.removeItem('customerInfo');
      setIsGuest(true);
      setIsLoggedIn(false);
      setCustomerInfo(null);
      return;
    }
    const saved = localStorage.getItem('customerInfo');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        if (parsed && typeof parsed === 'object') {
          setCustomerInfo(parsed);
          setIsLoggedIn(true);
        } else {
          localStorage.removeItem('customerInfo');
        }
      } catch {
        // A corrupted display-only cache must not prevent the public QR menu from rendering.
        localStorage.removeItem('customerInfo');
      }
    }
  }, []);

  const handleLogin = (customer: any) => {
    setIsLoginRequested(false);
    sessionStorage.removeItem('customerGuestMode');
    sessionStorage.removeItem('guestToken');
    sessionStorage.removeItem('guestQrSession');
    sessionStorage.removeItem('guestDirectSession');
    sessionStorage.removeItem('guestDirectBranchId');
    localStorage.setItem('customerInfo', JSON.stringify(customer));
    if (customer.token) localStorage.setItem('customerToken', customer.token);
    setCustomerInfo(customer);
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    sessionStorage.removeItem('customerGuestMode');
    sessionStorage.removeItem('guestToken');
    sessionStorage.removeItem('guestQrSession');
    sessionStorage.removeItem('guestDirectSession');
    sessionStorage.removeItem('guestDirectBranchId');
    localStorage.removeItem('customerInfo');
    localStorage.removeItem('customerToken');
    setCustomerInfo(null);
    setIsLoggedIn(false);
  };

  const handleGuestEntry = () => {
    setIsLoginRequested(false);
    sessionStorage.setItem('customerGuestMode', 'true');
    localStorage.removeItem('customerToken');
    localStorage.removeItem('customerInfo');
    setCustomerInfo(null);
    setIsLoggedIn(false);
    setIsGuest(true);
  };

  const handleLoginRequest = () => setIsLoginRequested(true);

  return (
    <Router>
      <CustomerAppShell
        isLoggedIn={isLoggedIn}
        isGuest={isGuest}
        onLogin={handleLogin}
        onLogout={handleLogout}
        onGuestEntry={handleGuestEntry}
        onLoginRequest={handleLoginRequest}
        isLoginRequested={isLoginRequested}
      />
    </Router>
  );
}

interface CustomerAppShellProps {
  isLoggedIn: boolean;
  isGuest: boolean;
  onLogin: (customer: any) => void;
  onLogout: () => void;
  onGuestEntry: () => void;
  onLoginRequest: () => void;
  isLoginRequested: boolean;
}

const CustomerAppShell: React.FC<CustomerAppShellProps> = ({ isLoggedIn, isGuest, onLogin, onLogout, onGuestEntry, onLoginRequest, isLoginRequested }) => {
  const location = useLocation();
  const isQrBootstrap = new URLSearchParams(window.location.search).has('qr');

  if (location.pathname === '/scan') {
    return (
      <div className="min-h-screen bg-black">
        <QRScan />
        <BottomNav />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {!isLoggedIn && (isLoginRequested || (!isGuest && !isQrBootstrap)) ? (
        <>
          <CustomerLogin onLogin={onLogin} onGuestEntry={onGuestEntry} />
          <ChatBot />
        </>
      ) : (
        <>
          <CustomerHeader homeHref={isQrBootstrap ? `/${window.location.search}` : '/'} isGuest={!isLoggedIn} />
          <Routes>
            <Route path="/" element={<DigitalMenu />} />
            <Route path="/reservation" element={<ReservationPage />} />
            <Route path="/profile" element={<CustomerProfile isGuest={isGuest} onLoginRequest={onLoginRequest} onLogout={onLogout} />} />
            <Route path="/scan" element={<QRScan />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
          <BottomNav />
          {!isQrBootstrap && <ChatBot />}
        </>
      )}
    </div>
  );
};

export default App;
