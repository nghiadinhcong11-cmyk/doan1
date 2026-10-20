import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { useRef } from 'react';
import StaffNavbar from '../../components/StaffNavbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import OperationsLayout from '../layouts/OperationsLayout';
import { EmployeeAttendance, EmployeeProfile, EmployeeSchedule } from '../../features/hrm';
import { InventoryPage } from '../../features/inventory';
import { POSPage, StaffOverviewPage } from '../../features/pos';
import NotificationHistoryPage from '../../features/notifications/NotificationHistoryPage';

type StaffRoutesProps = {
  userName: string;
  userPosition: string;
  onLogout: () => void;
};

export default function StaffRoutes({ userName, userPosition, onLogout }: StaffRoutesProps) {
  const location = useLocation();
  const opened = useRef(new Set<string>([location.pathname]));
  opened.current.add(location.pathname);
  const wasOpened = (path: string) => opened.current.has(path);
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <StaffNavbar onLogout={onLogout} userName={userName} />
      {wasOpened('/staff/profile') && <main className={location.pathname === '/staff/profile' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeProfile userName={userName} onLogout={onLogout} /></main>}
      {wasOpened('/staff/attendance') && <main className={location.pathname === '/staff/attendance' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeAttendance /></main>}
      {wasOpened('/staff/schedule') && <main className={location.pathname === '/staff/schedule' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><EmployeeSchedule /></main>}
      {wasOpened('/staff/inventory') && <main className={location.pathname === '/staff/inventory' ? 'h-[calc(100dvh-48px)] min-h-0 overflow-auto' : 'hidden'}><InventoryPage view="overview" operational /></main>}
      <Routes>
        <Route element={<OperationsLayout mainClassName="h-[calc(100dvh-48px)] min-h-0 overflow-auto" />}>
          <Route path="/staff" element={<StaffOverviewPage />} />
          <Route path="/staff/notifications" element={<NotificationHistoryPage role="employee" />} />
          <Route path="/staff/sales" element={<POSPage onLogout={onLogout} userName={userName} userRole="employee" userPosition={userPosition} />} />
          <Route path="/staff/tables" element={<Navigate to="/staff" replace />} />
          <Route path="/staff/profile" element={null} />
          <Route path="/staff/attendance" element={null} />
          <Route path="/staff/schedule" element={null} />
          <Route path="/staff/inventory" element={null} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="*" element={<Navigate to="/staff/profile" replace />} />
        </Route>
      </Routes>
    </div>
  );
}
