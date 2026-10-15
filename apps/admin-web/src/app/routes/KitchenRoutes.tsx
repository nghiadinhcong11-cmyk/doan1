import { Navigate, Route, Routes } from 'react-router-dom';
import KitchenNavbar from '../../components/KitchenNavbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import ChatBot from '../../components/ChatBot';
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
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <KitchenNavbar onLogout={onLogout} userName={userName} />
      <Routes>
        <Route element={<OperationsLayout mainClassName="h-[calc(100dvh-48px)] min-h-0" />}>
          <Route path="/kitchen" element={<KitchenPage />} />
          <Route path="/kitchen/notifications" element={<NotificationHistoryPage role="kitchen" />} />
          <Route path="/kitchen/history" element={<KitchenHistoryPage />} />
          <Route path="/kitchen/tables" element={<TableStatusPage />} />
          <Route path="/kitchen/attendance" element={<EmployeeAttendance />} />
          <Route path="/kitchen/schedule" element={<EmployeeSchedule />} />
          <Route path="/kitchen/reservations" element={<ReservationManagement readOnly={true} />} />
          <Route path="/kitchen/profile" element={<EmployeeProfile userName={userName} onLogout={onLogout} />} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="*" element={<Navigate to="/kitchen" replace />} />
        </Route>
      </Routes>
      <ChatBot />
    </div>
  );
}
