import { Banknote } from 'lucide-react';

interface POSOpenShiftModalProps {
  open: boolean;
  startingCash: number;
  onStartingCashChange: (value: number) => void;
  onOpenShift: () => void;
  onLogout: () => void;
}

export default function POSOpenShiftModal({ open, startingCash, onStartingCashChange, onOpenShift, onLogout }: POSOpenShiftModalProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 bg-black/80 z-[500] flex justify-center items-center p-4 backdrop-blur-md">
      <div className="bg-white w-full max-w-md rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="bg-[#0070f4] p-8 text-white text-center">
          <div className="w-20 h-20 bg-white/20 rounded-3xl flex items-center justify-center mx-auto mb-4"><Banknote size={40} /></div>
          <h3 className="font-black text-2xl uppercase italic tracking-tighter">Bắt đầu ca làm việc</h3>
          <p className="text-xs font-bold opacity-80 uppercase tracking-widest mt-2">Vui lòng khai báo tiền đầu ca</p>
        </div>
        <div className="p-8 space-y-6">
          <div>
            <label className="block text-[10px] font-black text-gray-400 uppercase tracking-widest mb-2">Số tiền mặt trong két (VNĐ)</label>
            <input type="number" value={startingCash} onChange={(event) => onStartingCashChange(Number(event.target.value))} className="w-full px-6 py-4 bg-gray-50 border-2 border-gray-100 rounded-2xl text-2xl font-black text-blue-700 focus:border-blue-500 outline-none transition-all" placeholder="0" />
          </div>
          <button onClick={onOpenShift} className="w-full bg-[#0070f4] text-white py-4 rounded-2xl font-black text-lg hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/20 active:scale-95 uppercase italic tracking-tighter">Mở ca ngay</button>
          <button onClick={onLogout} className="w-full text-gray-400 font-bold text-xs uppercase tracking-widest hover:text-gray-600 transition-all">Quay lại đăng nhập</button>
        </div>
      </div>
    </div>
  );
}
