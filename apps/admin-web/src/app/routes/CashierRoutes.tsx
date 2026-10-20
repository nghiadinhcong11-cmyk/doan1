import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { useRef } from 'react';
import CashierNavbar from '../../components/CashierNavbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import OperationsLayout from '../layouts/OperationsLayout';
import { InvoiceHistory } from '../../features/analytics';
import { ReservationManagement } from '../../features/operations';
import { EmployeeAttendance, EmployeeProfile, EmployeeSchedule, ShiftManagement } from '../../features/hrm';
import { POSPage } from '../../features/pos';
import NotificationHistoryPage from '../../features/notifications/NotificationHistoryPage';

type CashierRoutesProps = {
  userName: string;
  userPosition: string;
  onLogout: () => void;
};

export default function CashierRoutes({ userName, userPosition, onLogout }: CashierRoutesProps) {
  const location = useLocation();
  const opened = useRef(new Set<string>([location.pathname]));
  opened.current.add(location.pathname);
  const wasOpened = (path: string) => opened.current.has(path);
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <CashierNavbar onLogout={onLogout} userName={userName} userRole="cashier" userPosition={userPosition} />
      {wasOpened('/pos/invoices') && <main className={location.pathname === '/pos/invoices' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><InvoiceHistory /></main>}
      {wasOpened('/pos/attendance') && <main className={location.pathname === '/pos/attendance' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeAttendance /></main>}
      {wasOpened('/pos/schedule') && <main className={location.pathname === '/pos/schedule' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeSchedule /></main>}
      {wasOpened('/pos/shifts') && <main className={location.pathname === '/pos/shifts' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><ShiftManagement /></main>}
      {wasOpened('/pos/reservations') && <main className={location.pathname === '/pos/reservations' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><ReservationManagement /></main>}
      {wasOpened('/pos/profile') && <main className={location.pathname === '/pos/profile' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeProfile userName={userName} onLogout={onLogout} /></main>}
      <Routes>
        <Route element={<OperationsLayout mainClassName="h-[calc(100dvh-48px)] min-h-0" />}>
          <Route path="/pos" element={<POSPage onLogout={onLogout} userName={userName} userRole="cashier" userPosition={userPosition} />} />
          <Route path="/pos/notifications" element={<NotificationHistoryPage role="cashier" />} />
          <Route path="/pos/invoices" element={null} />
          <Route path="/pos/settings/receipt" element={<Navigate to="/forbidden" replace />} />
          <Route path="/pos/attendance" element={null} />
          <Route path="/pos/schedule" element={null} />
          <Route path="/pos/shifts" element={null} />
          <Route path="/pos/reservations" element={null} />
          <Route path="/pos/profile" element={null} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="*" element={<Navigate to="/pos" replace />} />
        </Route>
      </Routes>
    </div>
  );
}
