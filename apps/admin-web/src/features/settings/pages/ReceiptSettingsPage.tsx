import React, { useEffect, useState } from 'react';
import { Eye, Printer, Save } from 'lucide-react';
import { API_URL } from '../../../config';

type Settings = { paperWidth: 58 | 80; showLogo: boolean; showAddress: boolean; showPhone: boolean; showStaff: boolean; showPaymentMethod: boolean; showOrderNote: boolean; showThankYou: boolean; thankYouText: string };
const defaults: Settings = { paperWidth: 80, showLogo: true, showAddress: true, showPhone: true, showStaff: true, showPaymentMethod: true, showOrderNote: true, showThankYou: true, thankYouText: 'Cảm ơn quý khách và hẹn gặp lại!' };
const money = (value: number) => value.toLocaleString('vi-VN');

const ReceiptSettingsPage = () => {
  const [settings, setSettings] = useState<Settings>(defaults);
  const [branch, setBranch] = useState<any>(null);
  const [saved, setSaved] = useState(false);
  const branchId = localStorage.getItem('selectedBranchId') || '';
  const canEdit = localStorage.getItem('userRole') === 'admin' || localStorage.getItem('userPosition') === 'Quản lý';

  useEffect(() => {
    void Promise.all([
      fetch(`${API_URL}/api/ReceiptSettings?branchId=${branchId}`).then(r => r.ok ? r.json() : defaults),
      fetch(`${API_URL}/api/Branch`).then(r => r.ok ? r.json() : [])
    ]).then(([data, branches]) => { setSettings({ ...defaults, ...data }); setBranch(branches.find((item: any) => item.id === branchId) || branches[0]); }).catch(() => undefined);
  }, [branchId]);

  const update = (key: keyof Settings, value: any) => setSettings(prev => ({ ...prev, [key]: value }));
  const save = async () => {
    const response = await fetch(`${API_URL}/api/ReceiptSettings?branchId=${branchId}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ ...settings, branchId }) });
    if (response.ok) { setSaved(true); window.setTimeout(() => setSaved(false), 2500); }
  };
  const Toggle = ({ label, name }: { label: string; name: keyof Settings }) => <label className="flex items-center justify-between border-b border-slate-100 py-3 text-sm"><span>{label}</span><input disabled={!canEdit} type="checkbox" checked={Boolean(settings[name])} onChange={e => update(name, e.target.checked)} className="h-4 w-4 accent-blue-600" /></label>;

  return <div className="min-h-full bg-slate-50 p-4 text-slate-800 md:p-7"><div className="mb-6 flex items-center justify-between"><div><h1 className="text-2xl font-semibold tracking-tight">Cấu hình in bill</h1><p className="mt-1 text-sm text-slate-500">Thiết lập phiếu tính tiền cho chi nhánh đang chọn.</p></div>{canEdit && <button onClick={() => void save()} className="flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-sm font-bold text-white shadow hover:bg-blue-700"><Save size={16}/> {saved ? 'Đã lưu' : 'Lưu cấu hình'}</button>}</div><div className="grid gap-6 lg:grid-cols-[minmax(0,420px)_minmax(280px,1fr)]"><section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm"><h2 className="mb-4 text-base font-bold">Thiết lập hiển thị</h2><label className="mb-4 block text-sm font-semibold">Khổ giấy<select disabled={!canEdit} value={settings.paperWidth} onChange={e => update('paperWidth', Number(e.target.value))} className="mt-2 w-full rounded-lg border border-slate-200 px-3 py-2"><option value={58}>58 mm</option><option value={80}>80 mm</option></select></label><Toggle label="Hiển thị logo" name="showLogo"/><Toggle label="Hiển thị địa chỉ" name="showAddress"/><Toggle label="Hiển thị số điện thoại" name="showPhone"/><Toggle label="Hiển thị nhân viên" name="showStaff"/><Toggle label="Hiển thị phương thức thanh toán" name="showPaymentMethod"/><Toggle label="Hiển thị ghi chú đơn hàng" name="showOrderNote"/><Toggle label="Hiển thị lời cảm ơn" name="showThankYou"/><label className="mt-4 block text-sm font-semibold">Lời cảm ơn<textarea disabled={!canEdit} value={settings.thankYouText} onChange={e => update('thankYouText', e.target.value)} rows={2} className="mt-2 w-full rounded-lg border border-slate-200 px-3 py-2 text-sm"/></label></section><section><div className="mb-3 flex items-center gap-2 text-sm font-bold"><Eye size={17}/> Bill preview ({settings.paperWidth} mm)</div><div id="invoice-print" data-paper-width={settings.paperWidth} className="receipt-paper mx-auto bg-white p-4 font-mono text-[11px] text-black shadow-lg" style={{ width: settings.paperWidth === 58 ? 230 : 300 }}><div className="text-center">{settings.showLogo && <div className="mx-auto mb-2 flex h-9 w-9 items-center justify-center rounded-full bg-slate-900 text-sm font-bold text-white">R</div>}<p className="text-sm font-bold">{branch?.name || 'Tên nhà hàng'}</p>{settings.showAddress && <p>{branch?.address || 'Địa chỉ nhà hàng'}</p>}{settings.showPhone && <p>ĐT: {branch?.phoneNumber || 'Chưa cập nhật'}</p>}<p className="mt-3 border-y border-dashed py-2 font-bold">PHIẾU TÍNH TIỀN</p><p className="mt-1 font-bold">HD000027 · BÀN 22</p></div><div className="my-3 space-y-1">{[['Vũ heo nướng', 1, 79000], ['Ốc hương sốt trứng', 1, 89000], ['Nước suối', 2, 15000]].map(([name, quantity, price]) => <div className="flex justify-between gap-2" key={String(name)}><span className="min-w-0 flex-1">{name} ×{quantity}</span><span>{money(Number(price) * Number(quantity))}</span></div>)}</div><div className="border-t border-dashed pt-2 text-right"><p className="flex justify-between"><span>Tổng tiền hàng</span><span>{money(198000)}</span></p><p className="flex justify-between"><span>Chiết khấu</span><span>0</span></p><p className="mt-1 flex justify-between border-t pt-1 text-sm font-bold"><span>TỔNG CỘNG</span><span>{money(198000)}</span></p></div>{settings.showPaymentMethod && <p className="mt-3 text-center">Thanh toán: Tiền mặt</p>}{settings.showStaff && <p className="text-center">Thu ngân: Nhân viên</p>}{settings.showOrderNote && <p className="mt-2 border-t border-dashed pt-2">Ghi chú: —</p>}{settings.showThankYou && <p className="mt-5 text-center">{settings.thankYouText}</p>}</div><button onClick={() => window.print()} className="print-hide mx-auto mt-4 flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-bold"><Printer size={15}/> In thử</button></section></div></div>;
};
export default ReceiptSettingsPage;
