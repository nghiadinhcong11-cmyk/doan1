import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import DigitalMenu from './pages/DigitalMenu';
import CustomerLogin from './pages/CustomerLogin';
import CustomerProfile from './pages/CustomerProfile';
import ReservationPage from './pages/ReservationPage';
import QRScan from './pages/QRScan';
import BottomNav from './components/BottomNav';
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
    const useGuestSession = sessionStorage.getItem('guestQrSession') === 'true';
    const token = useGuestSession ? localStorage.getItem('guestToken') : localStorage.getItem('customerToken');
    if (token && !headers.has('Authorization')) headers.set('Authorization', `Bearer ${token}`);
    return nativeFetch(input, { ...init, headers }).then(response => {
      if (response.status === 401) {
        if (useGuestSession) {
          localStorage.removeItem('guestToken');
          sessionStorage.removeItem('guestQrSession');
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
    const saved = localStorage.getItem('customerInfo');
    if (saved) {
      setCustomerInfo(JSON.parse(saved));
      setIsLoggedIn(true);
    }
  }, []);

  const handleLogin = (customer: any) => {
    localStorage.setItem('customerInfo', JSON.stringify(customer));
    if (customer.token) localStorage.setItem('customerToken', customer.token);
    setCustomerInfo(customer);
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('customerInfo');
    localStorage.removeItem('customerToken');
    setCustomerInfo(null);
    setIsLoggedIn(false);
  };

  const isQrBootstrap = new URLSearchParams(window.location.search).has('qr');
  const isScanPage = window.location.pathname === '/scan';

  if (isScanPage) {
    return <Router><QRScan /></Router>;
  }

  return (
    <Router>
      <div className="min-h-screen bg-gray-50">
        {!isLoggedIn && !isQrBootstrap ? (
          <CustomerLogin onLogin={handleLogin} />
        ) : (
          <>
            <Routes>
              <Route path="/" element={<DigitalMenu />} />
              <Route path="/reservation" element={<ReservationPage />} />
              <Route path="/profile" element={<CustomerProfile onLogout={handleLogout} />} />
              <Route path="/scan" element={<QRScan />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
            <BottomNav />
            {!isQrBootstrap && <ChatBot />}
          </>
        )}
      </div>
    </Router>
  );
}

export default App;
