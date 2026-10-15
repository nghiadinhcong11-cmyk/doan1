import { Navigate, Route, Routes } from 'react-router-dom';
import StaffNavbar from '../../components/StaffNavbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import ChatBot from '../../components/ChatBot';
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
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <StaffNavbar onLogout={onLogout} userName={userName} />
      <Routes>
        <Route element={<OperationsLayout mainClassName="h-[calc(100dvh-48px)] min-h-0 overflow-auto" />}>
          <Route path="/staff" element={<StaffOverviewPage />} />
          <Route path="/staff/notifications" element={<NotificationHistoryPage role="employee" />} />
          <Route path="/staff/sales" element={<POSPage onLogout={onLogout} userName={userName} userRole="employee" userPosition={userPosition} />} />
          <Route path="/staff/tables" element={<Navigate to="/staff" replace />} />
          <Route path="/staff/profile" element={<EmployeeProfile userName={userName} onLogout={onLogout} />} />
          <Route path="/staff/attendance" element={<EmployeeAttendance />} />
          <Route path="/staff/schedule" element={<EmployeeSchedule />} />
          <Route path="/staff/inventory" element={<InventoryPage view="overview" operational />} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="*" element={<Navigate to="/staff/profile" replace />} />
        </Route>
      </Routes>
      <ChatBot />
    </div>
  );
}
