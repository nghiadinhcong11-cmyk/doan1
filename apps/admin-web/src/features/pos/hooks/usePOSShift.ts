import { useEffect, useState } from 'react';
import { API_URL } from '../../../config';
import { notifyFeedback } from '../../../components/ui';
import type { POSUserRole } from '../types';

export interface POSShift {
  id: string;
  startTime?: string;
  startingCash?: number;
}

export interface POSShiftRecord {
  id: string;
  startTime: string;
  endTime?: string;
  totalRevenue: number;
  startingCash: number;
  endingCash: number;
  status: string;
}

interface UsePOSShiftOptions {
  userRole?: POSUserRole;
  userName?: string;
  onLogout: () => void;
}

export default function usePOSShift({ userRole, userName, onLogout }: UsePOSShiftOptions) {
  const [activeShift, setActiveShift] = useState<POSShift | null>(null);
  const [isShiftModalOpen, setIsShiftModalOpen] = useState(false);
  const [isCloseShiftModalOpen, setIsCloseShiftModalOpen] = useState(false);
  const [isShiftHistoryModalOpen, setIsShiftHistoryModalOpen] = useState(false);
  const [startingCash, setStartingCash] = useState<number>(0);
  const [endingCash, setEndingCash] = useState<number>(0);
  const [shiftNote, setShiftNote] = useState('');
  const [shiftHistory, setShiftHistory] = useState<POSShiftRecord[]>([]);
  const [loadingHistory, setLoadingHistory] = useState(false);

  const fetchShiftHistory = async () => {
    const empId = localStorage.getItem('employeeId');
    if (!empId) return;
    try {
      setLoadingHistory(true);
      const response = await fetch(`${API_URL}/api/Shift?employeeId=${empId}`);
      const data = await response.json();
      setShiftHistory(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoadingHistory(false);
    }
  };

  useEffect(() => {
    if (isShiftHistoryModalOpen) fetchShiftHistory();
  }, [isShiftHistoryModalOpen]);

  const checkActiveShift = async () => {
    // Chỉ cashier mới cần mở/đóng ca; quản lý và admin không bị chặn bởi ca.
    if (userRole !== 'cashier') {
      return;
    }

    const empId = localStorage.getItem('employeeId');
    if (!empId || empId === 'undefined' || empId === 'null') {
      return;
    }

    try {
      const response = await fetch(`${API_URL}/api/Shift/current/${empId}`);
      if (response.ok) {
        const data = await response.json();
        setActiveShift(data);
      } else if (response.status === 404) {
        setIsShiftModalOpen(true);
      }
    } catch (err) {
      console.warn('Lỗi kiểm tra ca làm việc:', err);
    }
  };

  const handleOpenShift = async () => {
    const empId = localStorage.getItem('employeeId');
    const branchId = localStorage.getItem('selectedBranchId');

    if (!empId || !branchId) {
      notifyFeedback('Thiếu thông tin nhân viên hoặc chi nhánh!');
      return;
    }

    try {
      const response = await fetch(`${API_URL}/api/Shift/open`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          employeeId: empId,
          employeeName: userName,
          branchId,
          branchName: localStorage.getItem('selectedBranchName'),
          startingCash
        })
      });

      if (response.ok) {
        const data = await response.json();
        setActiveShift(data);
        setIsShiftModalOpen(false);
        notifyFeedback('Mở ca thành công! Bắt đầu phiên bán hàng.', 'success');
      }
    } catch (err) {
      notifyFeedback('Lỗi kết nối khi mở ca.');
    }
  };

  const handleCloseShift = async () => {
    if (!activeShift) return;

    try {
      const response = await fetch(`${API_URL}/api/Shift/close/${activeShift.id}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          endingCash,
          note: shiftNote
        })
      });

      if (response.ok) {
        notifyFeedback('Kết thúc ca thành công! Phiên làm việc đã được đóng.', 'success');
        setIsCloseShiftModalOpen(false);
        setActiveShift(null);
        onLogout();
      }
    } catch (err) {
      notifyFeedback('Lỗi kết nối khi chốt ca.');
    }
  };

  return {
    activeShift,
    isShiftModalOpen,
    isCloseShiftModalOpen,
    isShiftHistoryModalOpen,
    startingCash,
    endingCash,
    shiftNote,
    shiftHistory,
    loadingHistory,
    setIsShiftModalOpen,
    setIsCloseShiftModalOpen,
    setIsShiftHistoryModalOpen,
    setStartingCash,
    setEndingCash,
    setShiftNote,
    checkActiveShift,
    handleOpenShift,
    handleCloseShift
  };
}
