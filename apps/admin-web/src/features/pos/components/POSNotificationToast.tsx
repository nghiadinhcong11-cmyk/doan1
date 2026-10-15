import { Calendar as CalendarIcon, ClipboardList, X } from 'lucide-react';

interface POSNotificationToastProps {
  visible: boolean;
  type: 'order' | 'res';
  onView: () => void;
  onClose: () => void;
}

export default function POSNotificationToast({ visible, type, onView, onClose }: POSNotificationToastProps) {
  if (!visible) return null;

  return (
    <div className="fixed top-6 right-6 z-[200] bg-[#0070f4] text-white p-6 rounded-[2rem] shadow-2xl flex items-center space-x-6 animate-in slide-in-from-right-10 duration-500 max-w-sm border-b-4 border-blue-800">
      <div className={`p-4 rounded-3xl relative ${type === 'order' ? 'bg-orange-400' : 'bg-green-500'}`}>
        {type === 'order' ? <ClipboardList size={32} className="animate-bounce" /> : <CalendarIcon size={32} className="animate-bounce" />}
        <span className="absolute top-2 right-2 w-3 h-3 bg-white rounded-full border-2 border-blue-400"></span>
      </div>
      <div>
        <p className="font-black uppercase text-sm tracking-widest italic">{type === 'order' ? 'Đơn hàng mới!' : 'Lịch hẹn mới!'}</p>
        <p className="text-xs opacity-80 mt-1 font-medium italic">
          {type === 'order' ? 'Một khách vừa đặt món từ Web.' : 'Một khách vừa đặt bàn trước.'}
        </p>
        <button onClick={onView} className="mt-4 bg-white text-blue-600 px-6 py-2 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-blue-50 transition-all shadow-sm">
          XEM NGAY (F10)
        </button>
      </div>
      <button onClick={onClose} className="absolute top-4 right-4 text-white/50 hover:text-white transition-colors">
        <X size={18} />
      </button>
    </div>
  );
}
