import type { Table } from '../types';

interface POSTransferTableModalProps {
  open: boolean;
  tables: Table[];
  currentTableId: string;
  isSaving: boolean;
  onClose: () => void;
  onTransfer: (targetTableId: string) => void;
}

export default function POSTransferTableModal({ open, tables, currentTableId, isSaving, onClose, onTransfer }: POSTransferTableModalProps) {
  if (!open) return null;
  const availableTables = tables.filter(table => table.id !== currentTableId && table.status === 'Trống');

  return (
    <div className="fixed inset-0 z-[260] flex items-center justify-center bg-black/40 p-4">
      <div className="w-full max-w-md rounded-2xl bg-white p-5 shadow-2xl">
        <div className="mb-4 flex items-center justify-between">
          <div>
            <h2 className="text-lg font-black text-slate-900">Chuyển toàn bộ đơn sang bàn khác</h2>
            <p className="mt-1 text-xs text-slate-500">Chỉ chọn được bàn đang trống trong cùng chi nhánh.</p>
          </div>
          <button type="button" onClick={onClose} disabled={isSaving} className="rounded-lg px-3 py-2 text-sm font-bold text-slate-500 hover:bg-slate-100">Đóng</button>
        </div>
        <div className="max-h-72 space-y-2 overflow-y-auto">
          {availableTables.map(table => (
            <button key={table.id} type="button" onClick={() => onTransfer(table.id)} disabled={isSaving} className="flex w-full items-center justify-between rounded-xl border border-slate-200 px-4 py-3 text-left transition hover:border-blue-500 hover:bg-blue-50 disabled:cursor-not-allowed disabled:opacity-50">
              <span className="font-bold text-slate-800">{table.areaName ? `${table.areaName} · ` : ''}{table.name}</span>
              <span className="text-xs font-bold text-emerald-600">Trống</span>
            </button>
          ))}
          {availableTables.length === 0 && <p className="rounded-xl bg-slate-50 p-4 text-center text-sm text-slate-500">Không có bàn trống để chuyển.</p>}
        </div>
      </div>
    </div>
  );
}
