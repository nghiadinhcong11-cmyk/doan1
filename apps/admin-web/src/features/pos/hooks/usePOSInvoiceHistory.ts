import { useEffect, useRef, useState, type MutableRefObject } from 'react';
import { API_URL } from '../../../config';

interface UsePOSInvoiceHistoryOptions {
  branchGenerationRef: MutableRefObject<number>;
  isCurrentBranchRequest: (branchId: string | null, generation: number) => boolean;
}

export default function usePOSInvoiceHistory({ branchGenerationRef, isCurrentBranchRequest }: UsePOSInvoiceHistoryOptions) {
  const [isInvoiceHistoryModalOpen, setIsInvoiceHistoryModalOpen] = useState(false);
  const [invoices, setInvoices] = useState<any[]>([]);
  const [loadingInvoices, setLoadingInvoices] = useState(false);
  const [invoiceSearch, setInvoiceSearch] = useState('');
  const [invoiceFromDate, setInvoiceFromDate] = useState(new Date().toISOString().split('T')[0]);
  const [invoiceToDate, setInvoiceToDate] = useState(new Date().toISOString().split('T')[0]);
  const [invoiceStatusFilter, setInvoiceStatusFilter] = useState('all');
  const invoiceAbortControllerRef = useRef<AbortController | null>(null);
  const invoiceRequestInFlight = useRef(false);
  const invoiceRequestTokenRef = useRef(0);

  const fetchInvoiceHistory = async (force = false) => {
    if (invoiceRequestInFlight.current && !force) return;
    if (force) invoiceAbortControllerRef.current?.abort();
    invoiceRequestInFlight.current = true;
    const requestToken = ++invoiceRequestTokenRef.current;
    const branchId = localStorage.getItem('selectedBranchId');
    const generation = branchGenerationRef.current;
    const controller = new AbortController();
    invoiceAbortControllerRef.current = controller;
    try {
      setLoadingInvoices(true);

      let statusParam = '';
      if (invoiceStatusFilter === 'Đang xử lý') {
        statusParam = '&status=Đang xử lý,Đổi quà';
      } else if (invoiceStatusFilter !== 'all') {
        statusParam = `&status=${invoiceStatusFilter}`;
      }

      // Bỏ lọc ngày khi xem các đơn vẫn đang xử lý để giữ nguyên kết quả hiện tại.
      const dateParams = invoiceStatusFilter === 'Đang xử lý'
        ? ''
        : `&fromDate=${invoiceFromDate}&toDate=${invoiceToDate}`;

      const response = await fetch(`${API_URL}/api/Order?search=${encodeURIComponent(invoiceSearch)}&branchId=${branchId || ''}${dateParams}${statusParam}&_t=${Date.now()}`, { cache: 'no-store', signal: controller.signal });
      const data = await response.json();
      if (!isCurrentBranchRequest(branchId, generation)) return;
      setInvoices(data);
    } catch (err) {
      if (!(err instanceof DOMException && err.name === 'AbortError')) {
        console.error('Lỗi lấy lịch sử hóa đơn:', err);
      }
    } finally {
      if (requestToken === invoiceRequestTokenRef.current) {
        setLoadingInvoices(false);
        invoiceRequestInFlight.current = false;
        if (invoiceAbortControllerRef.current === controller) invoiceAbortControllerRef.current = null;
      }
    }
  };

  useEffect(() => {
    if (isInvoiceHistoryModalOpen) void fetchInvoiceHistory(true);
  }, [isInvoiceHistoryModalOpen, invoiceSearch, invoiceFromDate, invoiceToDate, invoiceStatusFilter]);

  useEffect(() => () => {
    invoiceAbortControllerRef.current?.abort();
    invoiceAbortControllerRef.current = null;
    invoiceRequestTokenRef.current += 1;
    invoiceRequestInFlight.current = false;
  }, []);

  const resetForBranchChange = () => {
    invoiceAbortControllerRef.current?.abort();
    invoiceAbortControllerRef.current = null;
    invoiceRequestTokenRef.current += 1;
    invoiceRequestInFlight.current = false;
    setLoadingInvoices(false);
  };

  return {
    isInvoiceHistoryModalOpen,
    invoices,
    loadingInvoices,
    invoiceSearch,
    invoiceFromDate,
    invoiceToDate,
    invoiceStatusFilter,
    setIsInvoiceHistoryModalOpen,
    setInvoiceSearch,
    setInvoiceFromDate,
    setInvoiceToDate,
    setInvoiceStatusFilter,
    fetchInvoiceHistory,
    resetForBranchChange
  };
}
