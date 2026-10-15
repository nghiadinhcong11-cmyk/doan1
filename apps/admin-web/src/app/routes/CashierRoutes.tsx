import { Navigate, Route, Routes } from 'react-router-dom';
import CashierNavbar from '../../components/CashierNavbar';
import ForbiddenPage from '../../components/ForbiddenPage';
import ChatBot from '../../components/ChatBot';
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
  return (
    <div className="min-h-screen bg-[#f0f2f5]">
      <CashierNavbar onLogout={onLogout} userName={userName} userRole="cashier" userPosition={userPosition} />
      <Routes>
        <Route element={<OperationsLayout mainClassName="h-[calc(100dvh-48px)] min-h-0" />}>
          <Route path="/pos" element={<POSPage onLogout={onLogout} userName={userName} userRole="cashier" userPosition={userPosition} />} />
          <Route path="/pos/notifications" element={<NotificationHistoryPage role="cashier" />} />
          <Route path="/pos/invoices" element={<InvoiceHistory />} />
          <Route path="/pos/settings/receipt" element={<Navigate to="/forbidden" replace />} />
          <Route path="/pos/attendance" element={<EmployeeAttendance />} />
          <Route path="/pos/schedule" element={<EmployeeSchedule />} />
          <Route path="/pos/shifts" element={<ShiftManagement />} />
          <Route path="/pos/reservations" element={<ReservationManagement />} />
          <Route path="/pos/profile" element={<EmployeeProfile userName={userName} onLogout={onLogout} />} />
          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="*" element={<Navigate to="/pos" replace />} />
        </Route>
      </Routes>
      <ChatBot />
    </div>
  );
}
