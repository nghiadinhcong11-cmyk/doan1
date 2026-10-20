import React, { useState, useEffect, useRef } from 'react';
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
    const isGuestBootstrap = path === '/api/guest/bootstrap';
    // Customer registration and password login are anonymous authentication
    // flows. A QR guest session must not be attached to them, otherwise the
    // backend correctly treats the request as a guest mutation and returns
    // 403 before it can create a new registered account.
    const isCustomerAuthRoute = path === '/api/Customer' ||
      path.startsWith('/api/Customer/exists/') ||
      path === '/api/Auth/customer-token';
    const useGuestSession = sessionStorage.getItem('guestQrSession') === 'true';
    const token = useGuestSession ? sessionStorage.getItem('guestToken') : localStorage.getItem('customerToken');
    // Product remains a public endpoint for branch selection, but a QR guest
    // must send the signed session so the API can reject a mismatched branchId.
    const isGuestProductRead = useGuestSession && path === '/api/Product';
    if ((!isPublicMenuRead || isGuestProductRead) && !isGuestBootstrap && !isCustomerAuthRoute && token && !headers.has('Authorization')) {
      headers.set('Authorization', `Bearer ${token}`);
    }
    return nativeFetch(input, { ...init, headers }).then(response => {
      if (response.status === 401) {
        if (useGuestSession) {
          sessionStorage.removeItem('guestToken');
          sessionStorage.removeItem('guestQrSession');
          sessionStorage.removeItem('guestQrToken');
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
    const hasQrUrl = new URLSearchParams(window.location.search).has('qr');
    return hasQrUrl;
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
    // Guest access is QR-only. Clear legacy direct-link session markers so an
    // old browser session cannot reopen the menu without a QR URL.
    sessionStorage.removeItem('customerGuestMode');
    sessionStorage.removeItem('guestDirectSession');
    sessionStorage.removeItem('guestDirectBranchId');
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
    sessionStorage.removeItem('guestQrToken');
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
    sessionStorage.removeItem('guestQrToken');
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
  onLoginRequest: () => void;
  isLoginRequested: boolean;
}

const CustomerAppShell: React.FC<CustomerAppShellProps> = ({ isLoggedIn, isGuest, onLogin, onLogout, onLoginRequest, isLoginRequested }) => {
  const location = useLocation();
  const menuWasOpened = useRef(location.pathname === '/');
  const reservationWasOpened = useRef(location.pathname === '/reservation');
  const profileWasOpened = useRef(location.pathname === '/profile');
  if (location.pathname === '/') menuWasOpened.current = true;
  if (location.pathname === '/reservation') reservationWasOpened.current = true;
  if (location.pathname === '/profile') profileWasOpened.current = true;
  const locationQrToken = new URLSearchParams(location.search).get('qr');
  const currentQrToken = locationQrToken || sessionStorage.getItem('guestQrToken');
  // The URL itself is enough to keep the QR shell visible while bootstrap is
  // still in flight. Waiting for the session marker caused a login-screen
  // redirect when an already-authenticated customer scanned a QR code.
  const isQrBootstrap = Boolean(locationQrToken || (currentQrToken && sessionStorage.getItem('guestQrSession') === 'true'));
  const qrGuestView = !isLoggedIn && (isGuest || isQrBootstrap);
  const qrHomeHref = currentQrToken ? `/?qr=${encodeURIComponent(currentQrToken)}` : '/';

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
      {!isLoggedIn && (isLoginRequested || !qrGuestView) ? (
        <>
          <CustomerLogin onLogin={onLogin} />
        </>
      ) : (
        <>
          <CustomerHeader homeHref={isQrBootstrap ? qrHomeHref : '/'} isGuest={!isLoggedIn} />
          {menuWasOpened.current && (
            <div className={location.pathname === '/' ? undefined : 'hidden'}>
              <DigitalMenu />
            </div>
          )}
          {reservationWasOpened.current && (
            <div className={location.pathname === '/reservation' ? undefined : 'hidden'}>
              <ReservationPage />
            </div>
          )}
          {profileWasOpened.current && (
            <div className={location.pathname === '/profile' ? undefined : 'hidden'}>
              <CustomerProfile isGuest={qrGuestView} onLoginRequest={onLoginRequest} onLogout={onLogout} />
            </div>
          )}
          <Routes>
            <Route path="/" element={null} />
            <Route path="/reservation" element={null} />
            <Route path="/profile" element={null} />
            <Route path="/scan" element={<QRScan />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
          <BottomNav />
          {(isLoggedIn ||
            (isQrBootstrap && sessionStorage.getItem('guestQrSession') === 'true')) && <ChatBot />}
        </>
      )}
    </div>
  );
};

export default App;
