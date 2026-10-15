import React, { useEffect, useState } from 'react';
import { Printer, Save, Trash2, Upload } from 'lucide-react';
import { API_URL } from '../../../config';
import ReceiptPreview, { ReceiptBranch, ReceiptSettingsModel } from '../../../components/ReceiptPreview';

const defaults: ReceiptSettingsModel = { paperWidth: 80, showLogo: true, showAddress: true, showPhone: true, showStaff: true, showPaymentMethod: true, showOrderNote: true, showThankYou: true, thankYouText: 'Cảm ơn quý khách và hẹn gặp lại!', fontFamily: 'font-mono', fontSize: 11 };

const ReceiptSettingsPage = () => {
  const [settings, setSettings] = useState<ReceiptSettingsModel>(defaults);
  const [branch, setBranch] = useState<ReceiptBranch | null>(null);
  const [saved, setSaved] = useState(false);
  const [logoMessage, setLogoMessage] = useState<string | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const branchId = localStorage.getItem('selectedBranchId') || '';
  const canEdit = localStorage.getItem('userRole') === 'admin';

  useEffect(() => {
    setLoadError(null);
    void Promise.all([
      fetch(`${API_URL}/api/ReceiptSettings?branchId=${branchId}`).then(r => { if (!r.ok) throw new Error('Không thể tải cấu hình in bill.'); return r.json(); }),
      // This admin-only page uses the management read model so receipt preview can
      // include branch tax information without widening the public Branch DTO.
      fetch(`${API_URL}/api/Branch/management`).then(r => { if (!r.ok) throw new Error('Không thể tải thông tin chi nhánh.'); return r.json(); })
    ]).then(([data, branches]) => {
      setSettings({ ...defaults, ...data });
      setBranch(branches.find((item: ReceiptBranch & { id?: string }) => item.id === branchId) || null);
    }).catch(error => setLoadError(error instanceof Error ? error.message : 'Không thể tải cấu hình in bill.'));
  }, [branchId]);

  const update = <K extends keyof ReceiptSettingsModel>(key: K, value: ReceiptSettingsModel[K]) => setSettings(prev => ({ ...prev, [key]: value }));
  const save = async () => {
    try {
      const response = await fetch(`${API_URL}/api/ReceiptSettings?branchId=${branchId}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...settings, branchId }) });
      if (!response.ok) throw new Error('Không thể lưu cấu hình in bill.');
      setSaved(true); window.setTimeout(() => setSaved(false), 2500);
    } catch (error) {
      setLoadError(error instanceof Error ? error.message : 'Không thể lưu cấu hình in bill.');
    }
  };
  const handleLogoUpload = async (file?: File) => {
    if (!file || !branchId) return;
    if (!['image/png', 'image/jpeg', 'image/webp'].includes(file.type)) { setLogoMessage('Logo chỉ hỗ trợ PNG, JPG hoặc WebP.'); return; }
    if (file.size > 2 * 1024 * 1024) { setLogoMessage('Logo không được vượt quá 2 MB.'); return; }
    const formData = new FormData();
    formData.append('logo', file);
    const response = await fetch(`${API_URL}/api/ReceiptSettings/logo?branchId=${branchId}`, { method: 'POST', body: formData });
    const data = await response.json().catch(() => null);
    if (!response.ok) { setLogoMessage(data?.message || `Không thể tải logo (${response.status}).`); return; }
    setBranch(prev => prev ? { ...prev, imageUrl: data.logoUrl } : prev);
    setLogoMessage('Đã cập nhật logo.');
  };
  const handleLogoDelete = async () => {
    if (!branchId) return;
    const response = await fetch(`${API_URL}/api/ReceiptSettings/logo?branchId=${branchId}`, { method: 'DELETE' });
    if (!response.ok) { setLogoMessage(`Không thể xóa logo (${response.status}).`); return; }
    setBranch(prev => prev ? { ...prev, imageUrl: undefined } : prev);
    setLogoMessage('Đã xóa logo.');
  };
  const Toggle = ({ label, name }: { label: string; name: keyof ReceiptSettingsModel }) => <label className="flex items-center justify-between border-b border-slate-100 py-3 text-sm"><span>{label}</span><input disabled={!canEdit} type="checkbox" checked={Boolean(settings[name])} onChange={e => update(name, e.target.checked)} className="h-4 w-4 accent-blue-600" /></label>;
  const previewOrder = { invoiceCode: 'HD000027 · BÀN 22', createdAt: new Date().toISOString(), status: 'Hoàn thành', totalAmount: 198000, subTotal: 198000, paidAmount: 0, paymentMethod: 'Tiền mặt', tableName: 'Bàn 22', createdBy: 'Nhân viên', details: [{ productName: 'Vũ heo nướng', quantity: 1, unitPrice: 79000 }, { productName: 'Ốc hương sốt trứng', quantity: 1, unitPrice: 89000 }, { productName: 'Nước suối', quantity: 2, unitPrice: 15000 }] };

  return <div className="min-h-full bg-slate-50 p-4 text-slate-800 md:p-7">
    {loadError && <p role="alert" className="mb-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">{loadError}</p>}
    <div className="mb-6 flex items-center justify-between"><div><h1 className="text-2xl font-semibold tracking-tight">Cấu hình in bill</h1><p className="mt-1 text-sm text-slate-500">Thiết lập phiếu tính tiền cho chi nhánh đang chọn.</p></div>{canEdit && <button onClick={() => void save()} className="flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-bold text-white shadow hover:bg-blue-700"><Save size={16}/> {saved ? 'Đã lưu' : 'Lưu cấu hình'}</button>}</div>
    <div className="grid gap-6 lg:grid-cols-[minmax(0,420px)_minmax(280px,1fr)]">
      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="mb-4 text-base font-bold">Thiết lập hiển thị</h2>
        <label className="mb-4 block text-sm font-semibold">Khổ giấy<select disabled={!canEdit} value={settings.paperWidth} onChange={e => update('paperWidth', Number(e.target.value) as 58 | 80)} className="mt-2 w-full rounded-lg border border-slate-200 px-3 py-2"><option value={58}>58 mm</option><option value={80}>80 mm</option></select></label>
        <Toggle label="Hiển thị logo" name="showLogo"/><Toggle label="Hiển thị địa chỉ" name="showAddress"/><Toggle label="Hiển thị số điện thoại" name="showPhone"/><Toggle label="Hiển thị nhân viên" name="showStaff"/><Toggle label="Hiển thị phương thức thanh toán" name="showPaymentMethod"/><Toggle label="Hiển thị ghi chú đơn hàng" name="showOrderNote"/><Toggle label="Hiển thị lời cảm ơn" name="showThankYou"/>
        <label className="mt-4 block text-sm font-semibold">Lời cảm ơn<textarea disabled={!canEdit} value={settings.thankYouText || ''} onChange={e => update('thankYouText', e.target.value)} rows={2} className="mt-2 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm"/></label>
        <div className="mt-6 border-t border-slate-100 pt-5">
          <h2 className="text-base font-bold">Logo nhà hàng</h2><p className="mt-1 text-xs text-slate-500">PNG, JPG hoặc WebP; tối đa 2 MB. Logo áp dụng theo chi nhánh đang chọn.</p>
          {branch?.imageUrl && <img src={branch.imageUrl} alt="Logo hiện tại" className="mt-3 h-20 w-full rounded-lg border border-slate-200 object-contain p-2" onError={() => setLogoMessage('Không thể tải logo hiện tại.')} />}
          {canEdit && <div className="mt-3 flex flex-wrap gap-2"><label className="inline-flex cursor-pointer items-center gap-2 rounded-lg bg-blue-600 px-3 py-2 text-sm font-bold text-white hover:bg-blue-700"><Upload size={15}/>{branch?.imageUrl ? 'Thay logo' : 'Tải logo'}<input type="file" accept="image/png,image/jpeg,image/webp" className="hidden" onChange={e => void handleLogoUpload(e.target.files?.[0])}/></label>{branch?.imageUrl && <button type="button" onClick={() => void handleLogoDelete()} className="inline-flex items-center gap-2 rounded-lg border border-red-200 px-3 py-2 text-sm font-bold text-red-700 hover:bg-red-50"><Trash2 size={15}/> Xóa logo</button>}</div>}
          {logoMessage && <p className="mt-2 text-sm text-slate-600" role="status">{logoMessage}</p>}
        </div>
      </section>
      <section><div className="mb-3 flex items-center gap-2 text-sm font-bold">Xem trước bill ({settings.paperWidth} mm)</div><div id="invoice-print" data-paper-width={settings.paperWidth} className="mx-auto w-fit"><ReceiptPreview settings={settings} branch={branch} preview order={previewOrder} /></div><button onClick={() => window.print()} className="print-hide mx-auto mt-4 flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-bold"><Printer size={15}/> In thử</button></section>
    </div>
  </div>;
};

export default ReceiptSettingsPage;
