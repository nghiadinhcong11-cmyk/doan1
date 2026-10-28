import React, { useEffect, useState } from 'react';
import { Printer, RefreshCw, Save, Trash2, Upload } from 'lucide-react';
import { API_URL } from '../../../config';
import { getCachedJson, invalidateCachedJson } from '../../../api/queryCache';
import ReceiptPreview, { ReceiptBranch, ReceiptSettingsModel } from '../../../components/ReceiptPreview';

const defaults: ReceiptSettingsModel = { paperWidth: 80, showLogo: true, showAddress: true, showPhone: true, showStaff: true, showPaymentMethod: true, showOrderNote: true, showThankYou: true, showSupplier: true, showWarehouse: true, showUnit: true, showUnitPrice: true, showPreparedBy: true, showApprovedBy: false, showSignatures: true, thankYouText: 'Cảm ơn quý khách và hẹn gặp lại!', fontFamily: 'font-mono', fontSize: 11 };

const ReceiptSettingsPage = () => {
  const [documentType, setDocumentType] = useState<'invoice' | 'gift-redemption' | 'stock-receipt'>('invoice');
  const [scope, setScope] = useState<'global' | 'branch'>('branch');
  const [settings, setSettings] = useState<ReceiptSettingsModel>(defaults);
  const [branch, setBranch] = useState<ReceiptBranch | null>(null);
  const [saved, setSaved] = useState(false);
  const [logoMessage, setLogoMessage] = useState<string | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const branchId = localStorage.getItem('selectedBranchId') || '';
  const canEdit = localStorage.getItem('userRole') === 'admin';
  const authHeaders = { 'Authorization': `Bearer ${localStorage.getItem('adminToken') || ''}` };
  const apiDocumentType = documentType === 'invoice' ? 'Invoice' : documentType === 'gift-redemption' ? 'GiftRedemption' : 'StockReceipt';

  const load = async (force = false) => {
    setLoadError(null);
    try {
      const [data, branches] = await Promise.all([
        getCachedJson<ReceiptSettingsModel>(`${API_URL}/api/ReceiptSettings?documentType=${apiDocumentType}${scope === 'branch' && branchId ? `&branchId=${branchId}` : ''}`, { headers: authHeaders }, { force }),
      // This admin-only page uses the management read model so receipt preview can
      // include branch tax information without widening the public Branch DTO.
        getCachedJson<ReceiptBranch[]>(`${API_URL}/api/Branch/management`, { headers: authHeaders }, { force })
      ]);
      setSettings({ ...defaults, ...data });
      setBranch(branches.find((item: ReceiptBranch & { id?: string }) => item.id === branchId) || null);
    } catch (error) {
      setLoadError(error instanceof Error ? error.message : 'Không thể tải cấu hình in bill.');
    }
  };

  useEffect(() => { void load(); }, [branchId, documentType, scope]);

  const update = <K extends keyof ReceiptSettingsModel>(key: K, value: ReceiptSettingsModel[K]) => setSettings(prev => ({ ...prev, [key]: value }));
  const save = async () => {
    try {
      const query = `documentType=${apiDocumentType}${scope === 'branch' && branchId ? `&branchId=${branchId}` : ''}`;
      const response = await fetch(`${API_URL}/api/ReceiptSettings?${query}`, { method: 'PUT', headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${localStorage.getItem('adminToken') || ''}` }, body: JSON.stringify({ ...settings, branchId: scope === 'branch' ? branchId : null, documentType: apiDocumentType }) });
      if (!response.ok) throw new Error('Không thể lưu cấu hình in bill.');
      invalidateCachedJson('/api/ReceiptSettings');
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
    const response = await fetch(`${API_URL}/api/ReceiptSettings/logo?branchId=${branchId}`, { method: 'POST', headers: authHeaders, body: formData });
    const data = await response.json().catch(() => null);
    if (!response.ok) { setLogoMessage(data?.message || `Không thể tải logo (${response.status}).`); return; }
    setBranch(prev => prev ? { ...prev, imageUrl: data.logoUrl } : prev);
    invalidateCachedJson('/api/Branch/management');
    setLogoMessage('Đã cập nhật logo.');
  };
  const handleLogoDelete = async () => {
    if (!branchId) return;
    const response = await fetch(`${API_URL}/api/ReceiptSettings/logo?branchId=${branchId}`, { method: 'DELETE', headers: authHeaders });
    if (!response.ok) { setLogoMessage(`Không thể xóa logo (${response.status}).`); return; }
    setBranch(prev => prev ? { ...prev, imageUrl: undefined } : prev);
    invalidateCachedJson('/api/Branch/management');
    setLogoMessage('Đã xóa logo.');
  };
  const Toggle = ({ label, name }: { label: string; name: keyof ReceiptSettingsModel }) => <label className="flex items-center justify-between border-b border-slate-100 py-3 text-sm"><span>{label}</span><input disabled={!canEdit} type="checkbox" checked={Boolean(settings[name])} onChange={e => update(name, e.target.checked)} className="h-4 w-4 accent-blue-600" /></label>;
  const displayToggles = documentType === 'invoice' || documentType === 'gift-redemption'
    ? [
        ['Hiển thị logo', 'showLogo'], ['Hiển thị địa chỉ', 'showAddress'], ['Hiển thị số điện thoại', 'showPhone'],
        ['Hiển thị nhân viên', 'showStaff'], ['Hiển thị phương thức thanh toán', 'showPaymentMethod'],
        ['Hiển thị ghi chú đơn hàng', 'showOrderNote'], ['Hiển thị lời cảm ơn', 'showThankYou']
      ] as Array<[string, keyof ReceiptSettingsModel]>
    : [
        ['Hiển thị logo', 'showLogo'], ['Hiển thị địa chỉ', 'showAddress'], ['Hiển thị số điện thoại', 'showPhone'],
        ['Hiển thị nhà cung cấp', 'showSupplier'], ['Hiển thị kho nhập', 'showWarehouse'], ['Hiển thị đơn vị tính', 'showUnit'],
        ['Hiển thị đơn giá nhập', 'showUnitPrice'], ['Hiển thị người lập phiếu', 'showPreparedBy'],
        ['Hiển thị người duyệt', 'showApprovedBy'], ['Hiển thị khu vực chữ ký', 'showSignatures']
      ] as Array<[string, keyof ReceiptSettingsModel]>;
  const previewOrder = { invoiceCode: 'HD000027 · BÀN 22', createdAt: new Date().toISOString(), status: 'Hoàn thành', totalAmount: 198000, subTotal: 198000, paidAmount: 0, paymentMethod: 'Tiền mặt', tableName: 'Bàn 22', createdBy: 'Nhân viên', details: [{ productName: 'Vũ heo nướng', quantity: 1, unitPrice: 79000 }, { productName: 'Ốc hương sốt trứng', quantity: 1, unitPrice: 89000 }, { productName: 'Nước suối', quantity: 2, unitPrice: 15000 }] };

  const stockReceiptPreview = (
    <div className="mx-auto w-[280px] max-w-full bg-white p-4 text-slate-900 shadow-sm ring-1 ring-slate-200">
      <div className="mb-5 text-center font-mono text-[11px]">
        {settings.showLogo && branch?.imageUrl && <img src={branch.imageUrl} alt="Logo chi nhánh" className="mx-auto mb-2 h-12 w-32 object-contain" />}
        <p className="font-bold uppercase">{branch?.name || 'DOAN RESTAURANT'}</p>
        {settings.showAddress && <p>{branch?.address || 'Địa chỉ chi nhánh'}</p>}
        {settings.showPhone && <p>{branch?.phoneNumber || 'Số điện thoại'}</p>}
      </div>
      <h3 className="border-y py-2 text-center font-bold uppercase">Phiếu nhập kho</h3>
      <div className="mt-4 grid grid-cols-2 gap-2 text-xs">
        <p>Mã phiếu: PN000027</p><p className="text-right">Ngày: 23/07/2026</p>
        {settings.showSupplier !== false && <p>Nhà cung cấp: Nhà cung cấp mẫu</p>}
        {settings.showWarehouse !== false && <p className="text-right">Kho: Kho chính</p>}
      </div>
      <table className="mt-5 w-full text-xs"><thead className="border-b"><tr><th className="py-2 text-left">Nguyên liệu</th>{settings.showUnit !== false && <th className="py-2 text-center">SL/ĐVT</th>}{settings.showUnitPrice !== false && <th className="py-2 text-right">Thành tiền</th>}</tr></thead><tbody><tr className="border-b border-slate-100"><td className="py-2">Cà phê hạt</td>{settings.showUnit !== false && <td className="py-2 text-center">10 kg</td>}{settings.showUnitPrice !== false && <td className="py-2 text-right">1,200,000</td>}</tr><tr className="border-b border-slate-100"><td className="py-2">Sữa tươi</td>{settings.showUnit !== false && <td className="py-2 text-center">20 hộp</td>}{settings.showUnitPrice !== false && <td className="py-2 text-right">600,000</td>}</tr></tbody></table>
      <p className="mt-4 text-right font-bold">Tổng tiền: 1,800,000 đ</p>
      {(settings.showPreparedBy !== false || settings.showApprovedBy || settings.showSignatures !== false) && <div className="mt-8 grid grid-cols-2 gap-4 text-center text-xs"><div>{settings.showPreparedBy !== false && <><p className="font-bold">Người lập phiếu</p><p className="mt-8">Quản trị viên</p></>}</div><div>{settings.showApprovedBy && <><p className="font-bold">Người duyệt</p><p className="mt-8">Quản lý</p></>}</div></div>}
      {settings.showThankYou && <p className="mt-8 text-center text-xs italic opacity-60">{settings.thankYouText}</p>}
    </div>
  );

  return <div className="min-h-full bg-slate-50 p-4 text-slate-800 md:p-7">
    {loadError && <p role="alert" className="mb-4 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">{loadError}</p>}
    <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between"><div><h1 className="text-2xl font-semibold tracking-tight">Cấu hình in ấn</h1><p className="mt-1 text-sm text-slate-500">Thiết lập theo loại phiếu và phạm vi áp dụng.</p></div><div className="flex flex-wrap items-center justify-end gap-3"><label className="text-sm font-semibold">Phạm vi<select disabled={!canEdit} value={scope} onChange={e => setScope(e.target.value as 'global' | 'branch')} className="ml-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm"><option value="branch">Chi nhánh đang chọn</option><option value="global">Toàn hệ thống</option></select></label><label className="text-sm font-semibold">Loại phiếu<select value={documentType} onChange={e => setDocumentType(e.target.value as 'invoice' | 'gift-redemption' | 'stock-receipt')} className="ml-2 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm"><option value="invoice">Hóa đơn bán hàng</option><option value="gift-redemption">Phiếu đổi quà</option><option value="stock-receipt">Phiếu nhập kho</option></select></label><button type="button" onClick={() => void load(true)} className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2.5 text-sm font-bold text-slate-600 hover:bg-slate-50"><RefreshCw size={16}/> Làm mới</button>{canEdit && <button onClick={() => void save()} className="flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-bold text-white shadow hover:bg-blue-700"><Save size={16}/> {saved ? 'Đã lưu' : 'Lưu cấu hình'}</button>}</div></div>
    <div className="grid gap-6 lg:grid-cols-[minmax(0,420px)_minmax(280px,1fr)]">
      <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="mb-4 text-base font-bold">Thiết lập hiển thị</h2>
        <label className="mb-4 block text-sm font-semibold">Khổ giấy<select disabled={!canEdit} value={settings.paperWidth} onChange={e => update('paperWidth', Number(e.target.value) as 58 | 80)} className="mt-2 w-full rounded-lg border border-slate-200 px-3 py-2"><option value={58}>58 mm</option><option value={80}>80 mm</option></select></label>
        {displayToggles.map(([label, name]) => <Toggle key={name} label={label} name={name} />)}
        <label className="mt-4 block text-sm font-semibold">Lời cảm ơn<textarea disabled={!canEdit} value={settings.thankYouText || ''} onChange={e => update('thankYouText', e.target.value)} rows={2} className="mt-2 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm"/></label>
        <div className="mt-6 border-t border-slate-100 pt-5">
          <h2 className="text-base font-bold">Logo nhà hàng</h2><p className="mt-1 text-xs text-slate-500">PNG, JPG hoặc WebP; tối đa 2 MB. Logo áp dụng theo chi nhánh đang chọn.</p>
          {branch?.imageUrl && <img src={branch.imageUrl} alt="Logo hiện tại" className="mt-3 h-20 w-full rounded-lg border border-slate-200 object-contain p-2" onError={() => setLogoMessage('Không thể tải logo hiện tại.')} />}
          {canEdit && <div className="mt-3 flex flex-wrap gap-2"><label className="inline-flex cursor-pointer items-center gap-2 rounded-lg bg-blue-600 px-3 py-2 text-sm font-bold text-white hover:bg-blue-700"><Upload size={15}/>{branch?.imageUrl ? 'Thay logo' : 'Tải logo'}<input type="file" accept="image/png,image/jpeg,image/webp" className="hidden" onChange={e => void handleLogoUpload(e.target.files?.[0])}/></label>{branch?.imageUrl && <button type="button" onClick={() => void handleLogoDelete()} className="inline-flex items-center gap-2 rounded-lg border border-red-200 px-3 py-2 text-sm font-bold text-red-700 hover:bg-red-50"><Trash2 size={15}/> Xóa logo</button>}</div>}
          {logoMessage && <p className="mt-2 text-sm text-slate-600" role="status">{logoMessage}</p>}
        </div>
      </section>
      <section><div className="mb-3 flex items-center gap-2 text-sm font-bold">Xem trước {documentType === 'invoice' ? 'hóa đơn' : documentType === 'gift-redemption' ? 'phiếu đổi quà' : 'phiếu nhập kho'} ({settings.paperWidth} mm)</div><div id="invoice-print" data-paper-width={settings.paperWidth} className="mx-auto w-fit">{documentType === 'invoice' ? <ReceiptPreview settings={settings} branch={branch} preview order={previewOrder} /> : documentType === 'gift-redemption' ? <ReceiptPreview settings={settings} branch={branch} documentType="GiftRedemption" preview order={{ ...previewOrder, totalAmount: 0, paidAmount: 0, status: 'Hoàn thành', paymentMethod: 'Đổi điểm', customerName: 'Khách hàng thành viên', details: [{ productName: 'Quà mẫu', quantity: 1, unitPrice: 0, options: 'Trừ 500 điểm' }] }} /> : stockReceiptPreview}</div><button onClick={() => window.print()} className="print-hide mx-auto mt-4 flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-bold"><Printer size={15}/> In thử</button></section>
    </div>
  </div>;
};

export default ReceiptSettingsPage;
