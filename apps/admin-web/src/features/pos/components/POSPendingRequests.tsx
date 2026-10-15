import { Calendar as CalendarIcon, ClipboardList, Clock } from 'lucide-react';

export interface POSPendingOrderRequest {
  id: string;
  tableName?: string;
  customerName?: string;
  status?: string;
}

export interface POSPendingReservationRequest {
  id: string;
  customerName: string;
  reservationTime: string;
  numberOfGuests: number;
}

interface POSPendingRequestsProps {
  orders: POSPendingOrderRequest[];
  reservations: POSPendingReservationRequest[];
  ordersError: string | null;
  onRejectOrder: (orderId: string, tableName?: string) => void;
  onProcessOrder: (order: POSPendingOrderRequest) => void;
  onViewReservation: () => void;
}

export default function POSPendingRequests({ orders, reservations, ordersError, onRejectOrder, onProcessOrder, onViewReservation }: POSPendingRequestsProps) {
  return (
    <div className="space-y-8 max-w-4xl mx-auto p-4">
      <div>
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-black text-gray-800 uppercase italic tracking-tighter flex items-center"><ClipboardList className="mr-2 text-blue-600" /> Đơn hàng chờ duyệt</h2>
          <span className="bg-blue-100 text-blue-600 px-3 py-1 rounded-full text-[10px] font-black">{orders.length}</span>
        </div>

        {ordersError && <div role="alert" className="mb-4 rounded-2xl border border-red-100 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700">{ordersError}</div>}

        {orders.length === 0 ? (
          <div className="py-10 text-center text-gray-300 italic text-[10px] font-bold uppercase tracking-widest border-2 border-dashed border-gray-100 rounded-3xl">Không có đơn hàng mới</div>
        ) : (
          <div className="space-y-4">
            {orders.map(order => (
              <div key={order.id} className="bg-white p-5 rounded-2xl shadow-sm border border-gray-100 flex items-center justify-between group animate-in slide-in-from-right-2">
                <div className="flex items-center space-x-4">
                  <div className="w-12 h-12 bg-orange-50 rounded-xl flex items-center justify-center text-orange-600 font-black text-lg">{order.tableName?.charAt(0) || 'K'}</div>
                  <div>
                    <h3 className="font-black text-gray-800 text-sm uppercase tracking-tight flex items-center">{order.tableName}{order.status === 'Đổi quà' && <span className="ml-2 bg-red-100 text-red-600 text-[8px] px-1.5 py-0.5 rounded-full font-black animate-pulse">ĐỔI QUÀ</span>}</h3>
                    <p className="text-[10px] text-gray-400 font-bold">{order.customerName}</p>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <button onClick={() => onRejectOrder(order.id, order.tableName)} className="bg-red-50 text-red-600 px-4 py-2 rounded-xl font-black text-[10px] uppercase hover:bg-red-600 hover:text-white transition-all">Từ chối</button>
                  <button onClick={() => onProcessOrder(order)} className="bg-blue-600 text-white px-5 py-2 rounded-xl font-black text-[10px] uppercase shadow-lg shadow-blue-500/20">Xử lý đơn</button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="pt-8 border-t border-gray-100">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-black text-gray-800 uppercase italic tracking-tighter flex items-center"><CalendarIcon className="mr-2 text-green-600" /> Lịch hẹn mới</h2>
          <span className="bg-green-100 text-green-600 px-3 py-1 rounded-full text-[10px] font-black">{reservations.length}</span>
        </div>

        {reservations.length === 0 ? (
          <div className="py-10 text-center text-gray-300 italic text-[10px] font-bold uppercase tracking-widest border-2 border-dashed border-gray-100 rounded-3xl">Không có lịch hẹn mới</div>
        ) : (
          <div className="space-y-4">
            {reservations.map(reservation => (
              <div key={reservation.id} className="bg-white p-5 rounded-2xl shadow-sm border border-green-50 flex items-center justify-between group animate-in slide-in-from-right-2">
                <div className="flex items-center space-x-4">
                  <div className="w-12 h-12 bg-green-50 rounded-xl flex items-center justify-center text-green-600 font-black text-lg">{reservation.customerName.charAt(0)}</div>
                  <div>
                    <h3 className="font-black text-gray-800 text-sm uppercase tracking-tight">{reservation.customerName}</h3>
                    <p className="text-[10px] text-gray-500 font-bold flex items-center mt-0.5"><Clock size={10} className="mr-1"/> {new Date(reservation.reservationTime).toLocaleTimeString([], {hour:'2-digit', minute:'2-digit'})} | {reservation.numberOfGuests} khách</p>
                  </div>
                </div>
                <button onClick={onViewReservation} className="bg-green-600 text-white px-5 py-2 rounded-xl font-black text-[10px] uppercase">Xem chi tiết</button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
