import { RotateCcw } from 'lucide-react';

interface POSCloseShiftModalProps {
  open: boolean;
  activeShift: { startTime?: string; startingCash?: number } | null;
  endingCash: number;
  shiftNote: string;
  onEndingCashChange: (value: number) => void;
  onShiftNoteChange: (value: string) => void;
  onClose: () => void;
  onCloseShift: () => void;
}

export default function POSCloseShiftModal({ open, activeShift, endingCash, shiftNote, onEndingCashChange, onShiftNoteChange, onClose, onCloseShift }: POSCloseShiftModalProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 bg-black/80 z-[500] flex justify-center items-center p-4 backdrop-blur-md">
      <div className="bg-white w-full max-w-md rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="bg-orange-500 p-8 text-white text-center">
          <div className="w-20 h-20 bg-white/20 rounded-3xl flex items-center justify-center mx-auto mb-4"><RotateCcw size={40} /></div>
          <h3 className="font-black text-2xl uppercase italic tracking-tighter">Kết thúc ca làm việc</h3>
          <p className="text-xs font-bold opacity-80 uppercase tracking-widest mt-2">Tổng kết doanh thu và bàn giao két</p>
        </div>
        <div className="p-8 space-y-6">
          <div className="bg-orange-50 p-4 rounded-2xl border border-orange-100">
            <div className="flex justify-between items-center mb-1"><span className="text-[10px] font-bold text-orange-600 uppercase">Thời gian bắt đầu</span><span className="text-xs font-black text-gray-700">{new Date(activeShift?.startTime || '').toLocaleString('vi-VN')}</span></div>
            <div className="flex justify-between items-center"><span className="text-[10px] font-bold text-orange-600 uppercase">Tiền đầu ca</span><span className="text-xs font-black text-gray-700">{activeShift?.startingCash?.toLocaleString()}đ</span></div>
          </div>
          <div>
            <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">Số tiền mặt thực tế hiện tại (VNĐ)</label>
            <input type="number" value={endingCash} onChange={(event) => onEndingCashChange(Number(event.target.value))} className="w-full px-6 py-4 bg-gray-50 border-2 border-orange-100 rounded-2xl text-2xl font-black text-orange-700 focus:border-orange-500 outline-none transition-all" placeholder="0" />
          </div>
          <div>
            <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">Ghi chú bàn giao</label>
            <textarea className="w-full px-4 py-3 bg-gray-50 border-2 border-gray-100 rounded-2xl text-xs font-bold outline-none focus:border-blue-500" rows={2} value={shiftNote} onChange={(event) => onShiftNoteChange(event.target.value)} placeholder="Ví dụ: Thiếu 20k tiền lẻ, đã nạp thêm..." />
          </div>
          <div className="flex gap-3">
            <button onClick={onClose} className="flex-1 bg-gray-100 text-gray-500 py-4 rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-gray-200 transition-all">Hủy bỏ</button>
            <button onClick={onCloseShift} className="flex-2 bg-orange-600 text-white py-4 px-8 rounded-2xl font-black text-lg hover:bg-orange-700 transition-all shadow-lg shadow-orange-500/20 active:scale-95 uppercase italic tracking-tighter">Chốt ca</button>
          </div>
        </div>
      </div>
    </div>
  );
}
