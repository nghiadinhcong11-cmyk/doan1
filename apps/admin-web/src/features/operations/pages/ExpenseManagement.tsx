import React, { useEffect, useMemo, useState } from 'react';
import { Plus, Pencil, Trash2, RefreshCw, X } from 'lucide-react';
import { API_URL } from '../../../config';

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
      if (branchId) params.set('branchId', branchId); if (category) params.set('category', category);
      if (fromDate) params.set('fromDate', fromDate); if (toDate) params.set('toDate', `${toDate}T23:59:59`);
      const response = await fetch(`${API_URL}/api/Expense?${params}`);
      if (!response.ok) throw new Error((await response.json()).message || 'Không thể tải dữ liệu chi phí');
      const data = await response.json(); setExpenses(data.items || data || []);
    } catch (e) { setError(e instanceof Error ? e.message : 'Lỗi kết nối máy chủ'); }
    finally { setLoading(false); }
  };
  useEffect(() => { fetch(`${API_URL}/api/Branch`).then(r => r.ok ? r.json() : []).then(setBranches).catch(() => undefined); }, []);
  useEffect(() => { load(); }, [branchId, category, fromDate, toDate]);

  const total = useMemo(() => expenses.reduce((sum, e) => sum + Number(e.amount), 0), [expenses]);
  const openCreate = () => { setEditing(null); setForm({ ...emptyForm, branchId: branchId || ownBranch }); setIsFormOpen(true); };
  const openEdit = (expense: Expense) => { setEditing(expense); setForm({ branchId: expense.branchId, category: expense.category, description: expense.description, amount: String(expense.amount), expenseDate: expense.expenseDate.slice(0, 10), paymentMethod: expense.paymentMethod || 'Tiền mặt', note: expense.note || '' }); setIsFormOpen(true); };
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
    } catch (e) { setError(e instanceof Error ? e.message : 'Lỗi lưu dữ liệu'); }
    finally { setSaving(false); }
  };
  const remove = async (id: string) => {
    if (!confirm('Bạn có chắc muốn xóa phiếu chi này?')) return;
    const response = await fetch(`${API_URL}/api/Expense/${id}`, { method: 'DELETE' });
    if (!response.ok) setError('Không thể xóa phiếu chi'); else load();
  };

  return <div className="p-6 space-y-5 bg-gray-50 min-h-[calc(100vh-48px)]">
    <div className="flex flex-wrap justify-between items-center gap-3"><div><h1 className="text-2xl font-bold text-gray-800">Chi phí / phiếu mua hàng</h1><p className="text-sm text-gray-500">Ghi nhận chi phí thực tế, không quản lý tồn kho vật lý.</p></div><button onClick={openCreate} className="bg-blue-600 text-white px-4 py-2 rounded-lg flex items-center gap-2"><Plus size={17}/> Lập phiếu chi</button></div>
    <div className="bg-white p-4 rounded-xl border flex flex-wrap gap-3 items-end"><label className="text-sm">Chi nhánh<select value={branchId} onChange={e => setBranchId(e.target.value)} className="block mt-1 border rounded px-3 py-2"><option value="">Tất cả chi nhánh</option>{branches.filter(b => b.isActive).map(b => <option key={b.id} value={b.id}>{b.name}</option>)}</select></label><label className="text-sm">Loại chi<select value={category} onChange={e => setCategory(e.target.value)} className="block mt-1 border rounded px-3 py-2"><option value="">Tất cả</option>{CATEGORIES.map(c => <option key={c}>{c}</option>)}</select></label><label className="text-sm">Từ ngày<input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} className="block mt-1 border rounded px-3 py-2"/></label><label className="text-sm">Đến ngày<input type="date" value={toDate} onChange={e => setToDate(e.target.value)} className="block mt-1 border rounded px-3 py-2"/></label><button onClick={load} className="border rounded px-3 py-2 flex gap-2 items-center"><RefreshCw size={16}/> Làm mới</button></div>
    {error && <div role="alert" className="bg-red-50 border border-red-200 text-red-700 p-3 rounded-lg">{error}</div>}
    {success && <div role="status" className="bg-green-50 border border-green-200 text-green-700 p-3 rounded-lg">{success}</div>}
    <div className="bg-white rounded-xl border overflow-hidden"><div className="p-4 border-b font-semibold">Tổng chi: <span className="text-red-600">{total.toLocaleString('vi-VN')} đ</span></div>{loading ? <div className="p-8 text-center">Đang tải...</div> : expenses.length === 0 ? <div className="p-8 text-center text-gray-500">Chưa có phiếu chi phù hợp.</div> : <div className="overflow-x-auto"><table className="min-w-full text-sm"><thead className="bg-gray-50"><tr>{['Ngày', 'Nội dung', 'Loại', 'Chi nhánh', 'Phương thức', 'Số tiền', ''].map(h => <th key={h} className="p-3 text-left font-semibold">{h}</th>)}</tr></thead><tbody>{expenses.map(e => <tr key={e.id} className="border-t"><td className="p-3">{new Date(e.expenseDate).toLocaleDateString('vi-VN')}</td><td className="p-3 font-medium">{e.description}</td><td className="p-3">{e.category}</td><td className="p-3">{branches.find(b => b.id === e.branchId)?.name || e.branchId}</td><td className="p-3">{e.paymentMethod || '-'}</td><td className="p-3 text-right text-red-600 font-semibold">{Number(e.amount).toLocaleString('vi-VN')} đ</td><td className="p-3 whitespace-nowrap"><button onClick={() => openEdit(e)} className="text-blue-600 mr-3"><Pencil size={16}/></button><button onClick={() => remove(e.id)} className="text-red-600"><Trash2 size={16}/></button></td></tr>)}</tbody></table></div>}</div>
    {isFormOpen && <div className="fixed inset-0 bg-black/40 flex items-center justify-center p-4 z-50"><form onSubmit={save} className="bg-white rounded-xl p-6 w-full max-w-lg space-y-4"><div className="flex justify-between"><h2 className="text-xl font-bold">{editing ? 'Sửa phiếu chi' : 'Lập phiếu chi'}</h2><button type="button" onClick={() => { setEditing(null); setIsFormOpen(false); setForm(emptyForm); }}><X/></button></div><div className="grid grid-cols-2 gap-3"><label className="text-sm">Chi nhánh<select required value={form.branchId} onChange={e => setForm({ ...form, branchId: e.target.value })} className="block w-full border rounded p-2 mt-1" disabled={role !== 'admin'}><option value="">-- Chọn --</option>{branches.filter(b => b.isActive).map(b => <option key={b.id} value={b.id}>{b.name}</option>)}</select></label><label className="text-sm">Ngày chi<input required type="date" value={form.expenseDate} onChange={e => setForm({ ...form, expenseDate: e.target.value })} className="block w-full border rounded p-2 mt-1"/></label></div><label className="text-sm">Nội dung<input required value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} className="block w-full border rounded p-2 mt-1"/></label><div className="grid grid-cols-2 gap-3"><label className="text-sm">Loại chi<select required value={form.category} onChange={e => setForm({ ...form, category: e.target.value })} className="block w-full border rounded p-2 mt-1">{CATEGORIES.map(c => <option key={c}>{c}</option>)}</select></label><label className="text-sm">Số tiền<input required min="1" type="number" value={form.amount} onChange={e => setForm({ ...form, amount: e.target.value })} className="block w-full border rounded p-2 mt-1"/></label></div><div className="grid grid-cols-2 gap-3"><label className="text-sm">Thanh toán<input value={form.paymentMethod} onChange={e => setForm({ ...form, paymentMethod: e.target.value })} className="block w-full border rounded p-2 mt-1"/></label><label className="text-sm">Ghi chú<input value={form.note} onChange={e => setForm({ ...form, note: e.target.value })} className="block w-full border rounded p-2 mt-1"/></label></div><button disabled={saving} className="w-full bg-blue-600 disabled:bg-blue-300 text-white rounded p-2">{saving ? 'Đang lưu...' : 'Lưu phiếu chi'}</button></form></div>}
  </div>;
};
export default ExpenseManagement;
