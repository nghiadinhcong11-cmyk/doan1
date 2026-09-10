import React, { useState, useEffect, useMemo } from 'react';
import * as signalR from '@microsoft/signalr';
import { Search, Filter, FileText, Download, ChevronRight, Calendar, Loader2, X, Clock, MapPin, User, Printer } from 'lucide-react';
import { API_URL } from '../../../config';

interface OrderDetail {
  id: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  options?: string;
}

interface Order {
  id: string;
  invoiceCode: string;
  createdAt: string;
  paymentAt?: string;
  customerName: string;
  customerPhone?: string;
  customerEmail?: string;
  totalAmount: number;
  subTotal?: number | null;
  vatAmount?: number | null;
  vatPercent?: number | null;
  serviceFeeAmount?: number | null;
  serviceFeePercent?: number | null;
  discount: number;
  paidAmount: number;
  status: string;
  paymentMethod: string;
  tableName?: string;
  createdBy?: string;
  branchName?: string;
  branchId?: string;
  note?: string;
  details?: OrderDetail[];
}

const displayStatus = (order: Pick<Order, 'status' | 'paidAmount' | 'totalAmount'>) =>
  order.totalAmount > 0 && order.paidAmount >= order.totalAmount ? 'Hoàn thành' : order.status;

const InvoiceHistory = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState('all'); // all, Hoàn thành, Đang xử lý, Đã hủy

  const [fromDate, setFromDate] = useState(new Date().toISOString().split('T')[0]);
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [paymentMethod, setPaymentMethod] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [branches, setBranches] = useState<any[]>([]);
  const [receiptSettings, setReceiptSettings] = useState<any>({ showLogo: true, showAddress: true, showPhone: true, showStaff: true, showPaymentMethod: true, showOrderNote: true, showThankYou: true, thankYouText: 'Cảm ơn quý khách và hẹn gặp lại!' });
  const pageSize = 20;

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize), _t: String(Date.now()) });
      if (searchTerm.trim()) params.set('search', searchTerm.trim());
      if (fromDate) params.set('fromDate', fromDate);
      if (toDate) params.set('toDate', toDate);
      if (paymentMethod) params.set('paymentMethod', paymentMethod);
      if (filterStatus === 'Hoàn thành') params.set('paymentStatus', 'Completed');
      if (filterStatus === 'Đang xử lý') params.set('paymentStatus', 'Pending');
      const response = await fetch(`${API_URL}/api/Invoice?${params}`, { cache: 'no-store' });
      if (!response.ok) throw new Error(`Invoice API ${response.status}`);
      const result = await response.json();
      setOrders((result.items || []).map((order: Order) => ({ ...order, status: displayStatus(order) })));
      setTotalPages(Math.max(1, result.totalPages || 1));
    } catch (err) {
      console.error('Error fetching orders:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetch(`${API_URL}/api/Branch`).then(response => response.ok ? response.json() : []).then(data => setBranches(Array.isArray(data) ? data : [])).catch(() => undefined);
  }, []);

  useEffect(() => {
    fetchOrders();
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/kitchenHub`, { accessTokenFactory: () => localStorage.getItem('token') || '' })
      .withAutomaticReconnect()
      .build();
    const refresh = () => { void fetchOrders(); };
    connection.on('PaymentCompleted', refresh);
    void connection.start().catch(() => undefined);
    const poll = window.setInterval(() => { void fetchOrders(); }, 10000);
    return () => {
      window.clearInterval(poll);
      connection.off('PaymentCompleted', refresh);
      void connection.stop();
    };
  }, [fromDate, toDate, searchTerm, filterStatus, paymentMethod, page]);

  const selectedBranch = branches.find(branch => branch.id === selectedOrder?.branchId);

  useEffect(() => {
    if (!selectedOrder?.branchId) return;
    fetch(`${API_URL}/api/ReceiptSettings?branchId=${selectedOrder.branchId}`).then(response => response.ok ? response.json() : null).then(data => data && setReceiptSettings(data)).catch(() => undefined);
  }, [selectedOrder?.branchId]);

  const filteredOrders = useMemo(() => {
    return orders.filter(o => {
      const matchSearch = o.invoiceCode.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          (o.customerName && o.customerName.toLowerCase().includes(searchTerm.toLowerCase()));

      const status = displayStatus(o);
      let matchStatus = filterStatus === 'all' || status === filterStatus;

      // Nếu lọc "Đang xử lý", hiển thị luôn cả "Đổi quà"
      if (filterStatus === 'Đang xử lý') {
        matchStatus = status === 'Đang xử lý' || status === 'Đổi quà';
      }

      return matchSearch && matchStatus;
    });
  }, [orders, filterStatus]);

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f8f9fa] text-[13px] font-sans">
      {/* Sidebar Filters */}
      <div className="w-72 bg-white border-r p-6 hidden md:block overflow-y-auto shadow-sm">
        <h2 className="font-black text-xl mb-8 text-gray-800 uppercase italic tracking-tighter">Giao dịch</h2>

        <div className="space-y-8">
          <div>
            <label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 block">Tìm kiếm</label>
            <div className="relative group">
               <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-300 group-focus-within:text-blue-500 transition-colors" />
               <input
                 type="text"
                 placeholder="Mã hóa đơn, tên khách..."
                 className="w-full pl-10 pr-3 py-2.5 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:ring-2 focus:ring-blue-500/10 focus:border-blue-500 font-bold transition-all"
                 value={searchTerm}
                 onChange={(e) => setSearchTerm(e.target.value)}
               />
            </div>
          </div>

          <div>
            <label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 block">Thời gian</label>
            <div className="space-y-4">
              <div className="space-y-1">
                 <span className="text-[10px] font-bold text-gray-400 uppercase ml-1">Từ ngày</span>
                 <input
                    type="date"
                    className="w-full px-3 py-2 bg-gray-50 border border-gray-100 rounded-xl outline-none text-xs font-bold text-gray-700"
                    value={fromDate}
                    onChange={(e) => setFromDate(e.target.value)}
                 />
              </div>
              <div className="space-y-1">
                 <span className="text-[10px] font-bold text-gray-400 uppercase ml-1">Đến ngày</span>
                 <input
                    type="date"
                    className="w-full px-3 py-2 bg-gray-50 border border-gray-100 rounded-xl outline-none text-xs font-bold text-gray-700"
                    value={toDate}
                    onChange={(e) => setToDate(e.target.value)}
                 />
              </div>
            </div>
          </div>

          <div>
            <label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 block">Trạng thái</label>
            <div className="space-y-2">
              {[
                { id: 'all', label: 'Tất cả trạng thái' },
                { id: 'Hoàn thành', label: 'Hoàn thành' },
                { id: 'Đang xử lý', label: 'Đang xử lý' },
                { id: 'Đổi quà', label: 'Yêu cầu đổi quà' },
                { id: 'Đã hủy', label: 'Đã hủy' }
              ].map(s => (
                <label key={s.id} className="flex items-center cursor-pointer group">
                  <input
                    type="radio"
                    name="status"
                    className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                    checked={filterStatus === s.id}
                    onChange={() => setFilterStatus(s.id)}
                  />
                  <span className={`font-bold transition-colors ${filterStatus === s.id ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-500'}`}>{s.label}</span>
                </label>
              ))}
            </div>
          </div>

          <div>
            <label className="text-[10px] font-black text-gray-400 uppercase tracking-[0.2em] mb-3 block">Phương thức thanh toán</label>
            <select value={paymentMethod} onChange={e => { setPage(1); setPaymentMethod(e.target.value); }} className="w-full rounded-xl border border-gray-100 bg-gray-50 px-3 py-2.5 text-xs font-bold text-gray-700 outline-none focus:border-blue-500">
              <option value="">Tất cả phương thức</option>
              <option value="Tiền mặt">Tiền mặt</option>
              <option value="Chuyển khoản">Chuyển khoản</option>
            </select>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <div className="bg-white border-b p-4 flex justify-between items-center shadow-sm">
          <h1 className="text-xl font-semibold text-gray-800 tracking-tight">Lịch sử hóa đơn</h1>
          <div className="flex space-x-2">
            <button className="bg-[#0070f4] text-white px-6 py-2 rounded-xl text-xs font-black uppercase tracking-widest shadow-lg shadow-blue-500/30 hover:bg-blue-700 transition-all flex items-center active:scale-95">
               <Download size={14} className="mr-2" /> Xuất file EXCEL
            </button>
          </div>
        </div>

        {/* Table */}
          <div className="flex-1 overflow-auto p-6 bg-[#f8f9fa]">
          <div className="bg-white rounded-[2rem] shadow-xl shadow-blue-500/5 border border-white overflow-hidden">
            <table className="min-w-full divide-y divide-gray-100">
              <thead className="bg-gray-50/50">
                <tr>
                  <th className="px-6 py-5 text-left text-[10px] font-black text-gray-400 uppercase tracking-widest">Mã hóa đơn</th>
                  <th className="px-6 py-5 text-left text-[10px] font-black text-gray-400 uppercase tracking-widest">Thời gian</th>
                  <th className="px-6 py-5 text-left text-[10px] font-black text-gray-400 uppercase tracking-widest">Vị trí</th>
                  <th className="px-6 py-5 text-left text-[10px] font-black text-gray-400 uppercase tracking-widest">Nhân viên</th>
                  <th className="px-6 py-5 text-left text-[10px] font-black text-gray-400 uppercase tracking-widest">Chi nhánh</th>
                  <th className="px-6 py-5 text-right text-[10px] font-black text-gray-400 uppercase tracking-widest">Tiền hàng</th>
                  <th className="px-6 py-5 text-right text-[10px] font-black text-gray-400 uppercase tracking-widest">Giảm giá</th>
                  <th className="px-6 py-5 text-right text-[10px] font-black text-gray-400 uppercase tracking-widest">Thành tiền</th>
                  <th className="px-6 py-5 text-left text-[10px] font-black text-gray-400 uppercase tracking-widest">Thanh toán</th>
                  <th className="px-6 py-5 text-center text-[10px] font-black text-gray-400 uppercase tracking-widest">Trạng thái</th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-50">
                {loading ? (
                  <tr><td colSpan={10} className="py-20 text-center flex flex-col items-center justify-center">
                    <Loader2 className="animate-spin text-blue-600 mb-2" size={32} />
                    <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Đang truy xuất dữ liệu...</p>
                  </td></tr>
                ) : filteredOrders.length === 0 ? (
                  <tr><td colSpan={10} className="py-32 text-center">
                    <FileText className="mx-auto text-gray-100 mb-4" size={64} />
                    <p className="text-sm font-bold text-gray-400 uppercase tracking-widest">Không có dữ liệu phù hợp</p>
                  </td></tr>
                ) : filteredOrders.map((o) => (
                  <tr
                    key={o.id}
                    onClick={() => setSelectedOrder(o)}
                    className="hover:bg-blue-50/30 cursor-pointer group transition-colors"
                  >
                    <td className="px-6 py-5 whitespace-nowrap text-sm font-black text-blue-600 italic tracking-tighter">{o.invoiceCode}</td>
                    <td className="px-6 py-5 whitespace-nowrap">
                       <p className="text-xs font-bold text-gray-700">{new Date(o.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}</p>
                       <p className="text-[10px] text-gray-400 font-medium">{new Date(o.createdAt).toLocaleDateString('vi-VN')}</p>
                    </td>
                    <td className="px-6 py-5 whitespace-nowrap text-xs font-black text-gray-600 uppercase">{o.tableName || 'Mang về'}</td>
                    <td className="px-6 py-5 whitespace-nowrap">
                       <div className="flex items-center">
                          <User size={12} className="mr-1.5 text-gray-300" />
                          <span className="text-xs font-bold text-gray-600">{o.createdBy || '---'}</span>
                       </div>
                    </td>
                    <td className="px-6 py-5 whitespace-nowrap text-[11px] text-gray-400 font-bold italic uppercase tracking-tighter">{o.branchName || 'Tổng hệ thống'}</td>
                    <td className="px-6 py-5 whitespace-nowrap text-sm text-right font-bold text-gray-700">{(o.subTotal ?? (o.totalAmount + (o.discount || 0))).toLocaleString()}đ</td>
                    <td className="px-6 py-5 whitespace-nowrap text-sm text-right text-gray-500">{(o.discount || 0).toLocaleString()}đ</td>
                    <td className="px-6 py-5 whitespace-nowrap text-sm text-right font-black text-gray-800 tracking-tighter">{o.totalAmount.toLocaleString()}đ</td>
                    <td className="px-6 py-5 whitespace-nowrap text-xs font-semibold text-gray-600">{o.paymentMethod || 'Chưa thanh toán'}</td>
                    <td className="px-6 py-5 whitespace-nowrap text-center">
                      <span className={`px-3 py-1 text-[9px] rounded-full font-black uppercase tracking-tighter shadow-sm border ${
                        o.status === 'Hoàn thành' ? 'bg-green-50 text-green-700 border-green-100' :
                        o.status === 'Đã hủy' ? 'bg-red-50 text-red-700 border-red-100' :
                        'bg-orange-50 text-orange-700 border-orange-100'
                      }`}>
                        {o.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="flex items-center justify-between border-t bg-white px-6 py-3 text-xs font-semibold text-gray-500">
            <span>Trang {page} / {totalPages}</span>
            <div className="flex gap-2">
              <button disabled={page <= 1} onClick={() => setPage(p => p - 1)} className="rounded-lg border px-3 py-1.5 disabled:opacity-30">Trước</button>
              <button disabled={page >= totalPages} onClick={() => setPage(p => p + 1)} className="rounded-lg border px-3 py-1.5 disabled:opacity-30">Sau</button>
            </div>
          </div>
        </div>
      </div>

      {/* DETAIL MODAL */}
      {selectedOrder && (
        <div id="invoice-print" data-paper-width={receiptSettings.paperWidth || 80} className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-center p-4 backdrop-blur-sm">
           <div className="bg-white w-full max-w-2xl rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
              <div className="bg-[#1e293b] p-6 text-white flex justify-between items-center">
                 <div className="flex items-center space-x-3">
                    <div className="p-3 bg-white/10 rounded-2xl">
                       <FileText size={24} />
                    </div>
                    <div>
                       <h3 className="font-semibold text-xl tracking-tight">Chi tiết hóa đơn</h3>
                       <p className="text-[10px] font-bold opacity-60 uppercase tracking-widest">{selectedOrder.invoiceCode}</p>
                       <span className="mt-2 inline-block rounded-full bg-emerald-500/20 px-2.5 py-1 text-[10px] font-semibold text-emerald-100">{displayStatus(selectedOrder)}</span>
                    </div>
                 </div>
                 <button onClick={() => setSelectedOrder(null)} className="print-hide bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
              </div>

              <div className="p-8">
                 <div className="mb-6 border-b border-dashed border-gray-300 pb-5 text-center font-mono text-sm text-gray-800">
                    {receiptSettings.showLogo && <div className="mx-auto mb-2 flex h-8 w-8 items-center justify-center rounded-full bg-slate-800 text-xs font-bold text-white">R</div>}
                    <p className="font-bold">{selectedBranch?.name || selectedOrder.branchName || 'Nhà hàng'}</p>
                    {selectedBranch?.taxCode && <p className="text-xs">MST: {selectedBranch.taxCode}</p>}
                    {receiptSettings.showAddress && selectedBranch?.address && <p className="text-xs">{selectedBranch.address}</p>}
                    {receiptSettings.showPhone && selectedBranch?.phoneNumber && <p className="text-xs">ĐT: {selectedBranch.phoneNumber}</p>}
                    <p className="mt-2 font-bold">PHIẾU TÍNH TIỀN</p>
                 </div>
                 <div className="grid grid-cols-2 gap-8 mb-8">
                    <div className="space-y-4">
                       <div>
                          <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Khách hàng</p>
                          <p className="text-sm font-black text-gray-800 uppercase tracking-tight italic">{selectedOrder.customerName}</p>
                          {selectedOrder.customerEmail && (
                             <p className="text-[10px] text-blue-600 font-bold mt-0.5">{selectedOrder.customerEmail}</p>
                          )}
                       </div>
                       <div>
                          <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Thời gian lập hóa đơn</p>
                          <p className="text-sm font-bold text-gray-700">{new Date(selectedOrder.paymentAt || selectedOrder.createdAt).toLocaleString('vi-VN')}</p>
                       </div>
                       {receiptSettings.showPaymentMethod && <div>
                          <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Phương thức</p>
                          <span className="text-xs font-semibold text-blue-600">{selectedOrder.paymentMethod || 'Chưa thanh toán'}</span>
                       </div>}
                    </div>
                    <div className="space-y-4 text-right">
                       <div>
                          <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Vị trí / Bàn</p>
                          <p className="text-sm font-black text-blue-600 uppercase italic tracking-tighter">{selectedOrder.tableName || 'Mang về'}</p>
                       </div>
                       <div>
                          <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Người lập đơn</p>
                          {receiptSettings.showStaff ? <p className="text-sm font-bold text-gray-700">{selectedOrder.createdBy || '---'}</p> : <p className="text-sm text-gray-400">—</p>}
                       </div>
                       <div>
                          <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-1">Cơ sở</p>
                          <p className="text-xs font-bold text-gray-400 uppercase">{selectedOrder.branchName || 'Tổng hệ thống'}</p>
                          {selectedBranch?.address && <p className="mt-1 text-[11px] text-gray-500">{selectedBranch.address}</p>}
                          {selectedBranch?.phoneNumber && <p className="text-[11px] text-gray-500">ĐT: {selectedBranch.phoneNumber}</p>}
                       </div>
                    </div>
                 </div>

                 <div className="bg-gray-50 rounded-[2rem] p-6 border border-gray-100">
                    <table className="w-full">
                       <thead>
                          <tr className="border-b border-gray-200">
                             <th className="text-left py-3 text-[10px] font-black text-gray-400 uppercase">Món ăn</th>
                             <th className="text-center py-3 text-[10px] font-black text-gray-400 uppercase">SL</th>
                             <th className="text-right py-3 text-[10px] font-black text-gray-400 uppercase">Đơn giá</th>
                             <th className="text-right py-3 text-[10px] font-black text-gray-400 uppercase">Thành tiền</th>
                          </tr>
                       </thead>
                       <tbody>
                          {selectedOrder.details?.map((d, i) => (
                             <tr key={i} className="border-b border-gray-100 last:border-0">
                                <td className="py-4">
                                   <p className="text-xs font-bold text-gray-700 capitalize">{d.productName}</p>
                                   {d.options && <p className="text-[10px] text-orange-500 font-medium mt-0.5">Topping: {d.options}</p>}
                                </td>
                                <td className="py-4 text-center text-xs font-black text-blue-600">{d.quantity}</td>
                                <td className="py-4 text-right text-xs font-medium text-gray-400">{d.unitPrice.toLocaleString()}</td>
                                <td className="py-4 text-right text-xs font-black text-gray-800">{(d.quantity * d.unitPrice).toLocaleString()}</td>
                             </tr>
                          ))}
                       </tbody>
                    </table>

                    <div className="mt-6 pt-6 border-t-2 border-dashed border-gray-200 space-y-2">
                       <div className="flex justify-between items-center text-sm">
                          <span className="font-bold text-gray-500 uppercase text-[10px]">Tạm tính</span>
                           <span className="font-bold text-gray-700">{(selectedOrder.subTotal ?? selectedOrder.totalAmount).toLocaleString()}đ</span>
                        </div>
                        {selectedOrder.serviceFeeAmount != null && selectedOrder.serviceFeeAmount > 0 && (
                          <div className="flex justify-between items-center text-sm">
                            <span className="font-bold text-gray-500 uppercase text-[10px]">Phí phục vụ ({selectedOrder.serviceFeePercent || 0}%)</span>
                            <span className="font-bold text-gray-700">{selectedOrder.serviceFeeAmount.toLocaleString()}đ</span>
                          </div>
                        )}
                        {selectedOrder.vatAmount != null && selectedOrder.vatAmount > 0 && (
                          <div className="flex justify-between items-center text-sm">
                            <span className="font-bold text-gray-500 uppercase text-[10px]">VAT ({selectedOrder.vatPercent || 0}%)</span>
                            <span className="font-bold text-gray-700">{selectedOrder.vatAmount.toLocaleString()}đ</span>
                          </div>
                        )}
                       <div className="flex justify-between items-center">
                          <span className="font-bold text-gray-500 uppercase text-[10px]">Giảm giá</span>
                          <span className="font-bold text-red-500">-{selectedOrder.discount.toLocaleString()}đ</span>
                       </div>
                       <div className="flex justify-between items-center pt-2">
                          <span className="font-black text-blue-600 uppercase text-xs italic">Tổng cộng thanh toán</span>
                           <span className="text-2xl font-black text-blue-700 tracking-tighter">{selectedOrder.totalAmount.toLocaleString()}đ</span>
                       </div>
                       {receiptSettings.showOrderNote && selectedOrder.note && <p className="pt-3 text-left text-xs text-gray-500">Ghi chú: {selectedOrder.note}</p>}
                    </div>
                 </div>
                 {receiptSettings.showThankYou && <p className="mt-6 text-center font-mono text-xs text-gray-500">{receiptSettings.thankYouText}</p>}
              </div>

              <div className="print-hide p-8 bg-gray-100 border-t flex justify-between items-center">
                 <button onClick={() => window.print()} className="flex items-center text-blue-600 font-black text-[10px] uppercase tracking-widest hover:underline">
                    <Printer size={16} className="mr-2" /> In lại hóa đơn
                 </button>
                 <button onClick={() => setSelectedOrder(null)} className="px-10 py-3 bg-white border-2 border-gray-200 rounded-2xl font-black text-gray-400 text-[10px] uppercase tracking-[0.2em] hover:bg-gray-50 transition-all">Đóng cửa sổ</button>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default InvoiceHistory;

