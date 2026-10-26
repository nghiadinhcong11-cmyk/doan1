import { useEffect, useState } from 'react';
import { Printer } from 'lucide-react';
import { useParams } from 'react-router-dom';
import { API_URL } from '../../config';
import { getCachedJson } from '../../api/queryCache';
import type { ReceiptBranch, ReceiptSettingsModel } from '../../components/ReceiptPreview';
import { inventoryApi, type StockReceipt } from './inventoryApi';

const defaults: ReceiptSettingsModel = {
  paperWidth: 80,
  showLogo: true,
  showAddress: true,
  showPhone: true,
  showSupplier: true,
  showWarehouse: true,
  showUnit: true,
  showUnitPrice: true,
  showPreparedBy: true,
  showApprovedBy: false,
  showSignatures: true,
  showOrderNote: true,
  thankYouText: ''
};

export default function StockReceiptPrintButton() {
  const { id } = useParams();
  const [receipt, setReceipt] = useState<StockReceipt | null>(null);
  const [settings, setSettings] = useState<ReceiptSettingsModel>(defaults);
  const [branch, setBranch] = useState<ReceiptBranch | null>(null);

  useEffect(() => {
    if (!id) return;
    const authHeaders = { Authorization: `Bearer ${localStorage.getItem('adminToken') || ''}` };
    void inventoryApi.getReceipt(id).then(loadedReceipt => Promise.all([
      loadedReceipt,
      getCachedJson<ReceiptSettingsModel>(`${API_URL}/api/ReceiptSettings?branchId=${loadedReceipt.branchId}&documentType=StockReceipt`, { headers: authHeaders }).catch(() => null),
      getCachedJson<ReceiptBranch[]>(`${API_URL}/api/Branch/management`, { headers: authHeaders }).catch(() => [])
    ])).then(([loadedReceipt, savedSettings, branches]) => {
      setReceipt(loadedReceipt);
      if (savedSettings) setSettings({ ...defaults, ...savedSettings });
      const current = Array.isArray(branches)
        ? branches.find((item: ReceiptBranch & { id?: string }) => item.id === loadedReceipt.branchId)
        : null;
      setBranch(current || null);
    }).catch(() => undefined);
  }, [id]);

  if (!receipt) return null;

  const paperWidth = settings.paperWidth === 58 ? 58 : 80;
  const total = receipt.totalAmount.toLocaleString('vi-VN');

  return <>
    <button type="button" onClick={() => window.print()} className="print-hide inline-flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-bold text-slate-700 hover:bg-slate-50">
      <Printer size={16} /> In phiếu
    </button>
    <div id="invoice-print" data-paper-width={paperWidth} className="hidden">
      <div className="receipt-paper bg-white p-4 font-mono text-black" style={{ width: paperWidth === 58 ? 230 : 300, fontSize: `${settings.fontSize || 11}px` }}>
        <div className="text-center">
          {settings.showLogo !== false && branch?.imageUrl && <img src={branch.imageUrl} alt="Logo chi nhánh" className="receipt-logo mx-auto mb-2 max-h-14 max-w-[80%] object-contain" />}
          <p className="font-bold uppercase">{branch?.name || 'DOAN RESTAURANT'}</p>
          {settings.showAddress !== false && <p>{branch?.address || 'Địa chỉ chi nhánh'}</p>}
          {settings.showPhone !== false && <p>ĐT: {branch?.phoneNumber || 'Chưa cập nhật'}</p>}
          <p className="mt-3 border-y border-dashed py-2 font-bold">PHIẾU NHẬP KHO</p>
          <p className="mt-1 font-bold">PN{receipt.id.slice(0, 8).toUpperCase()}</p>
        </div>
        <div className="my-3 space-y-1">
          <p>Ngày nhập: {receipt.purchaseDate ? new Date(receipt.purchaseDate).toLocaleDateString('vi-VN') : '—'}</p>
          {settings.showSupplier !== false && <p>Nhà cung cấp: {receipt.supplierName || '—'}</p>}
          {settings.showWarehouse !== false && <p>Kho: Kho của chi nhánh</p>}
        </div>
        <table className="my-3 w-full text-left">
          <thead className="border-b border-dashed"><tr><th className="py-1">Nguyên liệu</th>{settings.showUnit !== false && <th className="py-1 text-center">SL/ĐVT</th>}{settings.showUnitPrice !== false && <th className="py-1 text-right">Tiền</th>}</tr></thead>
          <tbody>{receipt.items.map(item => <tr key={item.id} className="border-b border-slate-100"><td className="py-1">{item.inventoryItemName}</td>{settings.showUnit !== false && <td className="py-1 text-center">{item.quantity.toLocaleString('vi-VN')} {item.unitCode}</td>}{settings.showUnitPrice !== false && <td className="py-1 text-right">{(item.quantity * (item.unitPrice || 0)).toLocaleString('vi-VN')}</td>}</tr>)}</tbody>
        </table>
        <p className="border-t border-dashed pt-2 text-right font-bold">Tổng tiền: {total} đ</p>
        {settings.showOrderNote !== false && receipt.note && <p className="mt-3 border-t border-dashed pt-2">Ghi chú: {receipt.note}</p>}
        {settings.showSignatures !== false && (settings.showPreparedBy !== false || settings.showApprovedBy) && <div className="mt-8 grid grid-cols-2 gap-3 text-center text-xs"><div>{settings.showPreparedBy !== false && <><p className="font-bold">Người lập phiếu</p><p className="mt-8">{receipt.createdBy}</p></>}</div><div>{settings.showApprovedBy && <><p className="font-bold">Người duyệt</p><p className="mt-8">{receipt.confirmedBy || '—'}</p></>}</div></div>}
      </div>
    </div>
  </>;
}
