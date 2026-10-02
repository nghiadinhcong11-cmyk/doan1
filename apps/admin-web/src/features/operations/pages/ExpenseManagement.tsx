import React, { useEffect, useMemo, useState } from 'react';
import { Pencil, Plus, RefreshCw, Trash2, X } from 'lucide-react';
import { API_URL } from '../../../config';
import { Button, Feedback, FormField, PageHeader, TableActions, TableCell, TableContainer, TableEmpty, TableHeader, TableLoading } from '../../../components/ui';

type Expense = { id: string; branchId: string; category: string; description: string; amount: number; expenseDate: string; paymentMethod?: string; note?: string };
type Branch = { id: string; name: string; isActive: boolean };
const CATEGORIES = ['Nguyên liệu', 'Bao bì', 'Điện nước', 'Vệ sinh', 'Gas', 'Vận hành', 'Khác'];
const emptyForm = { branchId: '', category: CATEGORIES[0], description: '', amount: '', expenseDate: new Date().toISOString().slice(0, 10), paymentMethod: 'Tiền mặt', note: '' };

const ExpenseManagement = () => {
  const role = localStorage.getItem('userRole') || '';
  const ownBranch = localStorage.getItem('selectedBranchId') || '';
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [category, setCategory] = useState('');
  const [branchId, setBranchId] = useState(role === 'admin' ? '' : ownBranch);
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [saving, setSaving] = useState(false);
  const [editing, setEditing] = useState<Expense | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [form, setForm] = useState(emptyForm);

  const load = async () => {
    setLoading(true); setError('');
    try {
      const params = new URLSearchParams({ page: '1', pageSize: '200' });
      if (branchId) params.set('branchId', branchId);
      if (category) params.set('category', category);
      if (fromDate) params.set('fromDate', fromDate);
      if (toDate) params.set('toDate', `${toDate}T23:59:59`);
      const response = await fetch(`${API_URL}/api/Expense?${params}`);
      if (!response.ok) throw new Error((await response.json()).message || 'Không thể tải dữ liệu chi phí');
      const data = await response.json();
      setExpenses(data.items || data || []);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Lỗi kết nối máy chủ');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetch(`${API_URL}/api/Branch`).then(r => r.ok ? r.json() : []).then(setBranches).catch(() => undefined); }, []);
  useEffect(() => { void load(); }, [branchId, category, fromDate, toDate]);

  const total = useMemo(() => expenses.reduce((sum, expense) => sum + Number(expense.amount), 0), [expenses]);
  const openCreate = () => { setEditing(null); setForm({ ...emptyForm, branchId: branchId || ownBranch }); setIsFormOpen(true); };
  const openEdit = (expense: Expense) => {
    setEditing(expense);
    setForm({ branchId: expense.branchId, category: expense.category, description: expense.description, amount: String(expense.amount), expenseDate: expense.expenseDate.slice(0, 10), paymentMethod: expense.paymentMethod || 'Tiền mặt', note: expense.note || '' });
    setIsFormOpen(true);
  };

  const save = async (event: React.FormEvent) => {
    event.preventDefault(); setError(''); setSuccess(''); setSaving(true);
    const payload = { ...form, amount: Number(form.amount), expenseDate: new Date(`${form.expenseDate}T00:00:00`).toISOString() };
    try {
      const response = await fetch(`${API_URL}/api/Expense${editing ? `/${editing.id}` : ''}`, { method: editing ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
      if (!response.ok) {
        let message = 'Không thể lưu phiếu chi';
        try { const body = await response.json(); message = body.message || body.title || message; } catch { /* Response may not contain JSON. */ }
        throw new Error(message);
      }
      const wasEditing = Boolean(editing);
      setEditing(null); setIsFormOpen(false); await load();
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
    if (!response.ok) setError('Không thể xóa phiếu chi'); else void load();
  };

  return (
    <div className="min-h-[calc(100vh-48px)] space-y-5 bg-gray-50 p-4 sm:p-6">
      <PageHeader title="Chi phí / phiếu mua hàng" description="Ghi nhận chi phí thực tế, không quản lý tồn kho vật lý." action={<Button onClick={openCreate}><Plus aria-hidden="true" size={17} /> Lập phiếu chi</Button>} />
      <div className="flex flex-wrap items-end gap-3 rounded-2xl border border-gray-200 bg-white p-4">
        <label className="text-sm font-medium text-gray-600">Chi nhánh<select value={branchId} onChange={e => setBranchId(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100" disabled={role !== 'admin'}><option value="">Tất cả chi nhánh</option>{branches.filter(b => b.isActive).map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label>
        <label className="text-sm font-medium text-gray-600">Loại chi<select value={category} onChange={e => setCategory(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100"><option value="">Tất cả</option>{CATEGORIES.map(item => <option key={item}>{item}</option>)}</select></label>
        <label className="text-sm font-medium text-gray-600">Từ ngày<input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100" /></label>
        <label className="text-sm font-medium text-gray-600">Đến ngày<input type="date" value={toDate} onChange={e => setToDate(e.target.value)} className="mt-1 block rounded-xl border border-gray-200 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100" /></label>
        <Button variant="outline" onClick={() => void load()}><RefreshCw aria-hidden="true" size={16} /> Làm mới</Button>
      </div>
      {error && <Feedback tone="error" onDismiss={() => setError('')}>{error}</Feedback>}
      {success && <Feedback tone="success" onDismiss={() => setSuccess('')}>{success}</Feedback>}
      <section className="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm">
        <p className="mb-4 font-semibold text-gray-700">Tổng chi: <span className="text-red-600">{total.toLocaleString('vi-VN')} đ</span></p>
        <TableContainer>
          <TableHeader><tr>{['Ngày', 'Nội dung', 'Loại', 'Chi nhánh', 'Phương thức', 'Số tiền', ''].map(header => <th key={header} className="px-4 py-3">{header}</th>)}</tr></TableHeader>
          <tbody>{loading ? <TableLoading colSpan={7} /> : expenses.length === 0 ? <TableEmpty colSpan={7}>Chưa có phiếu chi phù hợp.</TableEmpty> : expenses.map(expense => <tr key={expense.id} className="hover:bg-gray-50">
            <TableCell>{new Date(expense.expenseDate).toLocaleDateString('vi-VN')}</TableCell><TableCell className="font-medium">{expense.description}</TableCell><TableCell>{expense.category}</TableCell><TableCell>{branches.find(branch => branch.id === expense.branchId)?.name || expense.branchId}</TableCell><TableCell>{expense.paymentMethod || '-'}</TableCell><TableCell className="text-right font-semibold text-red-600">{Number(expense.amount).toLocaleString('vi-VN')} đ</TableCell>
            <TableActions><button type="button" aria-label="Sửa phiếu chi" onClick={() => openEdit(expense)} className="mr-3 rounded-md p-1.5 text-blue-600 hover:bg-blue-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><Pencil aria-hidden="true" size={16} /></button><button type="button" aria-label="Xóa phiếu chi" onClick={() => void remove(expense.id)} className="rounded-md p-1.5 text-red-600 hover:bg-red-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-red-500"><Trash2 aria-hidden="true" size={16} /></button></TableActions>
          </tr>)}</tbody>
        </TableContainer>
      </section>
      {isFormOpen && <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="expense-dialog-title"><form onSubmit={save} className="max-h-[90vh] w-full max-w-lg space-y-4 overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl">
        <div className="flex items-center justify-between"><h2 id="expense-dialog-title" className="text-xl font-bold text-gray-800">{editing ? 'Sửa phiếu chi' : 'Lập phiếu chi'}</h2><button type="button" aria-label="Đóng biểu mẫu" onClick={() => { setEditing(null); setIsFormOpen(false); setForm(emptyForm); }} className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"><X aria-hidden="true" /></button></div>
        <div className="grid gap-3 sm:grid-cols-2"><label className="text-sm font-medium text-gray-600">Chi nhánh<select required value={form.branchId} onChange={e => setForm({ ...form, branchId: e.target.value })} className="mt-1 block w-full rounded-xl border border-gray-200 p-2.5" disabled={role !== 'admin'}><option value="">-- Chọn --</option>{branches.filter(branch => branch.isActive).map(branch => <option key={branch.id} value={branch.id}>{branch.name}</option>)}</select></label><label className="text-sm font-medium text-gray-600">Ngày chi<input required type="date" value={form.expenseDate} onChange={e => setForm({ ...form, expenseDate: e.target.value })} className="mt-1 block w-full rounded-xl border border-gray-200 p-2.5" /></label></div>
        <FormField label="Nội dung" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} required />
        <div className="grid gap-3 sm:grid-cols-2"><label className="text-sm font-medium text-gray-600">Loại chi<select required value={form.category} onChange={e => setForm({ ...form, category: e.target.value })} className="mt-1 block w-full rounded-xl border border-gray-200 p-2.5">{CATEGORIES.map(item => <option key={item}>{item}</option>)}</select></label><FormField label="Số tiền" type="number" min="1" value={form.amount} onChange={e => setForm({ ...form, amount: e.target.value })} required /></div>
        <div className="grid gap-3 sm:grid-cols-2"><FormField label="Thanh toán" value={form.paymentMethod} onChange={e => setForm({ ...form, paymentMethod: e.target.value })} /><FormField label="Ghi chú" value={form.note} onChange={e => setForm({ ...form, note: e.target.value })} /></div>
        <div className="flex justify-end gap-3 pt-2"><Button type="button" variant="secondary" onClick={() => setIsFormOpen(false)}>Hủy</Button><Button type="submit" loading={saving}>Lưu phiếu chi</Button></div>
      </form></div>}
    </div>
  );
};

export default ExpenseManagement;
