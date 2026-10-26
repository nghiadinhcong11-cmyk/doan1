import React, { useEffect, useRef, useState } from 'react';
import { Pencil, Plus, RefreshCw, Trash2, X } from 'lucide-react';
import { API_URL } from '../../../config';
import { fetchBranchList } from '../../../api/branchQuery';
import { getCachedJson, invalidateCachedJson } from '../../../api/queryCache';
import { Button, Feedback, FormField, PageHeader, TableActions, TableCell, TableContainer, TableEmpty, TableHeader, TableLoading } from '../../../components/ui';

type Expense = { id: string; branchId: string; category: string; description: string; amount: number; expenseDate: string; paymentMethod?: string; note?: string };
type ExpensePage = { items: Expense[]; total: number; page: number; pageSize: number; totalAmount: number };
// GET /api/Branch intentionally returns active branches only and BranchPublicDto
// does not include isActive. Keep the optional field for compatibility with any
// richer branch response, but do not require it to render a valid option.
type Branch = { id: string; name: string; isActive?: boolean };
const CATEGORIES = ['Nguyên liệu', 'Bao bì', 'Điện nước', 'Vệ sinh', 'Gas', 'Vận hành', 'Khác'];
const emptyForm = { branchId: '', category: CATEGORIES[0], description: '', amount: '', expenseDate: new Date().toISOString().slice(0, 10), paymentMethod: 'Tiền mặt', note: '' };
const newIdempotencyKey = () => {
  if (typeof globalThis.crypto?.randomUUID === 'function') return globalThis.crypto.randomUUID();
  if (typeof globalThis.crypto?.getRandomValues === 'function') {
    const bytes = new Uint8Array(16);
    globalThis.crypto.getRandomValues(bytes);
    return `expense-${Array.from(bytes, value => value.toString(16).padStart(2, '0')).join('')}`;
  }
  return `expense-${Date.now()}-${Math.random().toString(36).slice(2)}`;
};

const ExpenseManagement = () => {
  const role = localStorage.getItem('userRole') || '';
  const ownBranch = localStorage.getItem('selectedBranchId') || '';
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [total, setTotal] = useState(0);
  const [pageSize, setPageSize] = useState(50);
  const [page, setPage] = useState(1);
  const [totalAmount, setTotalAmount] = useState(0);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [branchLoadError, setBranchLoadError] = useState('');
  const [branchLoaded, setBranchLoaded] = useState(false);
  const [category, setCategory] = useState('');
  const [branchId, setBranchId] = useState(role === 'admin' ? '' : ownBranch);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [saving, setSaving] = useState(false);
  const [editing, setEditing] = useState<Expense | null>(null);
  const [createIdempotencyKey, setCreateIdempotencyKey] = useState<string | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const requestGeneration = useRef(0);
  const abortController = useRef<AbortController | null>(null);
  const previousQuery = useRef({ branchId, category, fromDate, toDate });

  const load = async (requestedPage = page, force = false) => {
    const generation = ++requestGeneration.current;
    abortController.current?.abort();
    const controller = new AbortController();
    abortController.current = controller;
    setLoading(true); setError('');
    try {
      const params = new URLSearchParams({ page: String(requestedPage), pageSize: String(pageSize) });
      if (branchId) params.set('branchId', branchId);
      if (category) params.set('category', category);
      if (fromDate) params.set('fromDate', fromDate);
      if (toDate) params.set('toDate', `${toDate}T23:59:59`);
      const data = await getCachedJson<ExpensePage>(`${API_URL}/api/Expense?${params}`, { signal: controller.signal }, { force });
      if (generation !== requestGeneration.current) return;
      setExpenses(data.items || []);
      setTotal(data.total || 0);
      setPageSize(data.pageSize || pageSize);
      setTotalAmount(data.totalAmount || 0);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      if (generation !== requestGeneration.current) return;
      setError(e instanceof Error ? e.message : 'Lỗi kết nối máy chủ');
    } finally {
      if (generation === requestGeneration.current) setLoading(false);
    }
  };

  useEffect(() => {
    let disposed = false;
    fetchBranchList()
      .then(data => {
        if (!disposed) {
          setBranches(data as Branch[]);
          setBranchLoadError('');
        }
      })
      .catch(() => {
        if (!disposed) {
          setBranches([]);
          setBranchLoadError('Không thể tải danh sách chi nhánh.');
        }
      })
      .finally(() => {
        if (!disposed) setBranchLoaded(true);
      });
    return () => { disposed = true; };
  }, []);

  // The public branch endpoint already filters inactive branches server-side.
  // Do not filter on isActive because BranchPublicDto deliberately omits it.
  const availableBranches = branches.filter(branch => branch.isActive !== false);
  useEffect(() => {
    const queryChanged = previousQuery.current.branchId !== branchId
      || previousQuery.current.category !== category
      || previousQuery.current.fromDate !== fromDate
      || previousQuery.current.toDate !== toDate;
    previousQuery.current = { branchId, category, fromDate, toDate };

    if (queryChanged && page !== 1) {
      setPage(1);
      return () => abortController.current?.abort();
    }

    void load(queryChanged ? 1 : page);
    return () => abortController.current?.abort();
  }, [branchId, category, fromDate, toDate, page]);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const openCreate = () => { setEditing(null); setCreateIdempotencyKey(newIdempotencyKey()); setForm({ ...emptyForm, branchId: branchId || ownBranch }); setIsFormOpen(true); };
  const openEdit = (expense: Expense) => {
    setEditing(expense);
    setCreateIdempotencyKey(null);
    setForm({ branchId: expense.branchId, category: expense.category, description: expense.description, amount: String(expense.amount), expenseDate: expense.expenseDate.slice(0, 10), paymentMethod: expense.paymentMethod || 'Tiền mặt', note: expense.note || '' });
    setIsFormOpen(true);
  };

  const save = async (event: React.FormEvent) => {
    event.preventDefault(); setError(''); setSuccess(''); setSaving(true);
    const payload = { ...form, amount: Number(form.amount), expenseDate: new Date(`${form.expenseDate}T00:00:00`).toISOString() };
    try {
      const idempotencyKey = editing ? null : (createIdempotencyKey || newIdempotencyKey());
      if (!editing && !createIdempotencyKey) setCreateIdempotencyKey(idempotencyKey);
      const headers: Record<string, string> = { 'Content-Type': 'application/json' };
      if (idempotencyKey) headers['Idempotency-Key'] = idempotencyKey;
      const response = await fetch(`${API_URL}/api/Expense${editing ? `/${editing.id}` : ''}`, { method: editing ? 'PUT' : 'POST', headers, body: JSON.stringify(payload) });
      if (!response.ok) {
        let message = 'Không thể lưu phiếu chi';
        try { const body = await response.json(); message = body.message || body.title || message; } catch { /* Response may not contain JSON. */ }
        throw new Error(message);
      }
      const wasEditing = Boolean(editing);
      invalidateCachedJson('/api/Expense');
      setEditing(null); setCreateIdempotencyKey(null); setIsFormOpen(false); await load(page, true);
      setSuccess(wasEditing ? 'Cập nhật phiếu chi thành công.' : 'Lưu phiếu chi thành công.');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Lỗi lưu dữ liệu');
    } finally {
      setSaving(false);
    }
  };

  const remove = async (id: string) => {
    if (!window.confirm('Bạn có chắc muốn xóa phiếu chi này?')) return;
    const response = await fetch(`${API_URL}/api/Expense/${id}`, { method: 'DELETE' });
    if (!response.ok) setError('Không thể xóa phiếu chi'); else { invalidateCachedJson('/api/Expense'); void load(page, true); }
  };

  return (
    <div className="min-h-[calc(100vh-48px)] space-y-5 bg-gray-50 p-4 sm:p-6">
      <PageHeader title="Chi phí / phiếu mua hàng" description="Ghi nhận chi phí thực tế, không quản lý tồn kho vật lý." action={<Button className="min-h-0 rounded px-4 py-1.5 text-sm" onClick={openCreate}><Plus aria-hidden="true" size={16} /> Lập phiếu chi</Button>} />
      {branchLoadError && <Feedback tone="error">{branchLoadError}</Feedback>}
      {branchLoaded && !branchLoadError && availableBranches.length === 0 && <Feedback tone="warning">Chưa có chi nhánh hoạt động để chọn.</Feedback>}
      <div className="flex flex-wrap items-end gap-3 rounded-2xl border border-gray-200 bg-white p-4">
        <label className="text-sm font-medium text-gray-600">Chi nhánh<select value={branchId} onChange={e => setBranchId(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100" disabled={role !== 'admin'}><option value="">Tất cả chi nhánh</option>{availableBranches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>
        <label className="text-sm font-medium text-gray-600">Loại chi<select value={category} onChange={e => setCategory(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100"><option value="">Tất cả</option>{CATEGORIES.map(item => <option key={item}>{item}</option>)}</select></label>
        <label className="text-sm font-medium text-gray-600">Từ ngày<input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100" /></label>
        <label className="text-sm font-medium text-gray-600">Đến ngày<input type="date" value={toDate} onChange={e => setToDate(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100" /></label>
        <Button variant="outline" onClick={() => void load(page, true)}><RefreshCw aria-hidden="true" size={16} /> Làm mới</Button>
      </div>
      {error && <Feedback tone="error" onDismiss={() => setError('')}>{error}</Feedback>}
      {success && <Feedback tone="success" onDismiss={() => setSuccess('')}>{success}</Feedback>}
      <section className="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm">
        <p className="mb-4 font-semibold text-gray-700">Tổng chi theo bộ lọc: <span className="text-red-600">{totalAmount.toLocaleString('vi-VN')} đ</span> <span className="text-sm font-normal text-gray-500">({total} phiếu)</span></p>
        <TableContainer>
          <TableHeader><tr>{['Ngày', 'Nội dung', 'Loại', 'Chi nhánh', 'Phương thức', 'Số tiền', ''].map(header => <th key={header} className="px-4 py-3">{header}</th>)}</tr></TableHeader>
          <tbody>{loading ? <TableLoading colSpan={7} /> : expenses.length === 0 ? <TableEmpty colSpan={7}>Chưa có phiếu chi phù hợp.</TableEmpty> : expenses.map(expense => <tr key={expense.id} className="hover:bg-gray-50">
            <TableCell>{new Date(expense.expenseDate).toLocaleDateString('vi-VN')}</TableCell><TableCell className="font-medium">{expense.description}</TableCell><TableCell>{expense.category}</TableCell><TableCell>{branches.find(branch => branch.id === expense.branchId)?.name || expense.branchId}</TableCell><TableCell>{expense.paymentMethod || '-'}</TableCell><TableCell className="text-right font-semibold text-red-600">{Number(expense.amount).toLocaleString('vi-VN')} đ</TableCell>
            <TableActions><button type="button" aria-label="Sửa phiếu chi" onClick={() => openEdit(expense)} className="mr-3 rounded-md p-1.5 text-blue-600 hover:bg-blue-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><Pencil aria-hidden="true" size={16} /></button><button type="button" aria-label="Xóa phiếu chi" onClick={() => void remove(expense.id)} className="rounded-md p-1.5 text-red-600 hover:bg-red-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-500"><Trash2 aria-hidden="true" size={16} /></button></TableActions>
          </tr>)}</tbody>
        </TableContainer>
        <div className="mt-4 flex items-center justify-between gap-3 text-sm text-gray-600">
          <span>Trang {page}/{totalPages}</span>
          <div className="flex gap-2"><Button variant="outline" disabled={loading || page <= 1} onClick={() => setPage(value => value - 1)}>Trang trước</Button><Button variant="outline" disabled={loading || page >= totalPages} onClick={() => setPage(value => value + 1)}>Trang sau</Button></div>
        </div>
      </section>
      {isFormOpen && <div className="fixed inset-0 z-[100] flex items-start justify-center overflow-y-auto bg-black/60 px-4 pb-10 pt-10 backdrop-blur-sm" role="dialog" aria-modal="true" aria-labelledby="expense-dialog-title"><form onSubmit={save} className="max-h-[90vh] w-full max-w-3xl space-y-6 overflow-y-auto rounded-[2.5rem] bg-white p-10 shadow-2xl">
        <div className="flex items-center justify-between"><h2 id="expense-dialog-title" className="text-xl font-black uppercase italic tracking-tight text-gray-800">{editing ? 'Sửa phiếu chi' : 'Lập phiếu chi'}</h2><button type="button" aria-label="Đóng biểu mẫu" onClick={() => { setEditing(null); setCreateIdempotencyKey(null); setIsFormOpen(false); setForm(emptyForm); }} className="rounded-full p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><X aria-hidden="true" /></button></div>
        <div className="grid gap-4 sm:grid-cols-2"><label className="text-xs font-bold text-gray-600">Chi nhánh<select required value={form.branchId} onChange={e => setForm({ ...form, branchId: e.target.value })} className="mt-1 block w-full rounded-2xl border border-gray-200 bg-gray-50 px-5 py-3 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100" disabled={role !== 'admin'}><option value="">-- Chọn --</option>{availableBranches.map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label><label className="text-xs font-bold text-gray-600">Ngày chi<input required type="date" value={form.expenseDate} onChange={e => setForm({ ...form, expenseDate: e.target.value })} className="mt-1 block w-full rounded-2xl border border-gray-200 bg-gray-50 px-5 py-3 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100" /></label></div>
        {branchLoaded && !branchLoadError && !availableBranches.some(branch => branch.id === form.branchId) && <Feedback tone="warning">Chưa có chi nhánh hoạt động hợp lệ cho phiếu này.</Feedback>}
        <FormField label="Nội dung" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} required />
        <div className="grid gap-4 sm:grid-cols-2"><label className="text-xs font-bold text-gray-600">Loại chi<select required value={form.category} onChange={e => setForm({ ...form, category: e.target.value })} className="mt-1 block w-full rounded-2xl border border-gray-200 bg-gray-50 px-5 py-3 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100">{CATEGORIES.map(item => <option key={item}>{item}</option>)}</select></label><FormField label="Số tiền" type="number" min="1" value={form.amount} onChange={e => setForm({ ...form, amount: e.target.value })} required /></div>
        <div className="grid gap-3 sm:grid-cols-2"><FormField label="Thanh toán" value={form.paymentMethod} onChange={e => setForm({ ...form, paymentMethod: e.target.value })} /><FormField label="Ghi chú" value={form.note} onChange={e => setForm({ ...form, note: e.target.value })} /></div>
       <div className="flex justify-end gap-3 pt-4"><Button className="rounded-full" type="button" variant="secondary" onClick={() => { setEditing(null); setCreateIdempotencyKey(null); setIsFormOpen(false); }}>Hủy</Button><Button className="rounded-full" type="submit" loading={saving} disabled={!form.branchId || !availableBranches.some(branch => branch.id === form.branchId)}>Lưu phiếu chi</Button></div>
      </form></div>}
    </div>
  );
};

export default ExpenseManagement;
