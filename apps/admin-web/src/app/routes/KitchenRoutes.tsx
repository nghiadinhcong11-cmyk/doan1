import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { useRef } from 'react';
import KitchenNavbar from '../../components/KitchenNavbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import OperationsLayout from '../layouts/OperationsLayout';
import { ReservationManagement } from '../../features/operations';
import { EmployeeAttendance, EmployeeProfile, EmployeeSchedule } from '../../features/hrm';
import { KitchenHistoryPage, KitchenPage } from '../../features/kitchen';
import { TableStatusPage } from '../../features/pos';
import NotificationHistoryPage from '../../features/notifications/NotificationHistoryPage';

type KitchenRoutesProps = {
  userName: string;
  onLogout: () => void;
};

export default function KitchenRoutes({ userName, onLogout }: KitchenRoutesProps) {
  const location = useLocation();
  const opened = useRef(new Set<string>([location.pathname]));
  opened.current.add(location.pathname);
  const wasOpened = (path: string) => opened.current.has(path);
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <KitchenNavbar onLogout={onLogout} userName={userName} />
      {wasOpened('/kitchen') && <main className={location.pathname === '/kitchen' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><KitchenPage /></main>}
      {wasOpened('/kitchen/notifications') && <main className={location.pathname === '/kitchen/notifications' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><NotificationHistoryPage role="kitchen" /></main>}
      {wasOpened('/kitchen/history') && <main className={location.pathname === '/kitchen/history' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><KitchenHistoryPage /></main>}
      {wasOpened('/kitchen/tables') && <main className={location.pathname === '/kitchen/tables' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><TableStatusPage /></main>}
      {wasOpened('/kitchen/attendance') && <main className={location.pathname === '/kitchen/attendance' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeAttendance /></main>}
      {wasOpened('/kitchen/schedule') && <main className={location.pathname === '/kitchen/schedule' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeSchedule /></main>}
      {wasOpened('/kitchen/reservations') && <main className={location.pathname === '/kitchen/reservations' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><ReservationManagement readOnly={true} /></main>}
      {wasOpened('/kitchen/profile') && <main className={location.pathname === '/kitchen/profile' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeProfile userName={userName} onLogout={onLogout} /></main>}
      <Routes>
        <Route element={<OperationsLayout mainClassName="h-[calc(100dvh-48px)] min-h-0" />}>
          <Route path="/kitchen" element={null} />
          <Route path="/kitchen/notifications" element={null} />
          <Route path="/kitchen/history" element={null} />
          <Route path="/kitchen/tables" element={null} />
          <Route path="/kitchen/attendance" element={null} />
          <Route path="/kitchen/schedule" element={null} />
          <Route path="/kitchen/reservations" element={null} />
          <Route path="/kitchen/profile" element={null} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="*" element={<Navigate to="/kitchen" replace />} />
        </Route>
      </Routes>
    </div>
  );
}
