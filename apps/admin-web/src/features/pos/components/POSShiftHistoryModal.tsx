import { Loader2, X } from 'lucide-react';

interface ShiftRecord {
  id: string;
  startTime: string;
  endTime?: string;
  totalRevenue: number;
  startingCash: number;
  endingCash: number;
  status: string;
}

interface POSShiftHistoryModalProps {
  open: boolean;
  shifts: ShiftRecord[];
  loading: boolean;
  onClose: () => void;
}

export default function POSShiftHistoryModal({ open, shifts, loading, onClose }: POSShiftHistoryModalProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 bg-black/80 z-[600] flex justify-center items-center p-4 backdrop-blur-md">
      <div className="bg-white w-full max-w-2xl rounded-[2.5rem] shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="bg-gray-800 p-6 text-white flex justify-between items-center">
          <div><h3 className="font-black text-xl uppercase italic tracking-tighter">Lịch sử ca của bạn</h3><p className="text-[10px] font-bold opacity-80 uppercase tracking-widest">Danh sách các phiên làm việc gần đây</p></div>
          <button onClick={onClose} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-all"><X size={24}/></button>
        </div>
        <div className="p-6 max-h-[500px] overflow-y-auto no-scrollbar">
          {loading ? <div className="flex justify-center py-10"><Loader2 className="animate-spin text-blue-600" /></div> : shifts.length === 0 ? <div className="text-center py-10 text-gray-400 font-bold uppercase text-[10px]">Chưa có lịch sử ca</div> : (
            <div className="space-y-3">
              {shifts.map((shift) => (
                <div key={shift.id} className="bg-gray-50 p-4 rounded-2xl border border-gray-100 flex items-center justify-between">
                  <div><p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">{new Date(shift.startTime).toLocaleDateString('vi-VN')}</p><p className="text-xs font-black text-gray-700 mt-0.5">{new Date(shift.startTime).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'})} - {shift.endTime ? new Date(shift.endTime).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'}) : '...'}</p></div>
                  <div className="text-right"><p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Doanh thu</p><p className="text-sm font-black text-blue-600">{shift.totalRevenue.toLocaleString()}đ</p></div>
                  <div className="text-right"><p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Chênh lệch</p><p className={`text-xs font-black ${shift.endingCash - (shift.startingCash + shift.totalRevenue) === 0 ? 'text-green-500' : 'text-red-500'}`}>{shift.status === 'Closed' ? `${(shift.endingCash - (shift.startingCash + shift.totalRevenue)).toLocaleString()}đ` : 'Đang mở'}</p></div>
                </div>
              ))}
            </div>
          )}
        </div>
        <div className="p-6 bg-gray-50 border-t text-center"><p className="text-[10px] text-gray-400 font-medium italic">* Chỉ hiển thị các ca làm việc của bạn tại chi nhánh hiện tại.</p></div>
      </div>
    </div>
  );
}
