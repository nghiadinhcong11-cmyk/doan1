import React, { useState, useEffect, useMemo } from 'react';
import { Plus, Search, User, Phone, MapPin, Mail, Edit2, Trash2, Loader2, Download, X, Star, History, Save, TrendingUp, Users } from 'lucide-react';
import { API_URL } from '../../../config';
import { notifyFeedback } from '../../../components/ui';

interface Customer {
  id?: string;
  fullName: string;
  phoneNumber: string;
  email?: string;
  address?: string;
  gender?: string;
  birthday?: string;
  customerGroup?: string; // Khách VIP, Khách quen, Khách lẻ
  totalSpending: number;
  totalOrders?: number;
  loyaltyPoints?: number;
  createdAt?: string;
  lastOrderDate?: string;
}

const CustomerManagement = () => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterGroup, setFilterGroup] = useState('all');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<Customer | null>(null);

  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);
  const [loyaltyHistory, setLoyaltyHistory] = useState<any[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);

  const [newCustomer, setNewCustomer] = useState<Customer>({
    fullName: '',
    phoneNumber: '',
    email: '',
    address: '',
    gender: 'Nam',
    birthday: '',
    totalSpending: 0
  });

  const fetchCustomers = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${API_URL}/api/Customer${searchTerm ? `?search=${searchTerm}` : ''}`);
      const data = await response.json();
      setCustomers(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCustomers();
  }, [searchTerm]);

  const filteredCustomers = useMemo(() => {
    return customers.filter(c =>
        (filterGroup === 'all' || c.customerGroup === filterGroup)
    );
  }, [customers, filterGroup]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await fetch(`${API_URL}/api/Customer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newCustomer)
      });

      if (response.ok) {
        setIsModalOpen(false);
        setNewCustomer({ fullName: '', phoneNumber: '', totalSpending: 0 });
        setEditingCustomer(null);
        fetchCustomers();
      }
    } catch (err) {
      notifyFeedback('Lỗi lưu thông tin khách hàng');
    }
  };

  const openEditModal = (c: Customer) => {
    setEditingCustomer(c);
    setNewCustomer({
      fullName: c.fullName,
      phoneNumber: c.phoneNumber,
      email: c.email || '',
      address: c.address || '',
      gender: c.gender || 'Nam',
      birthday: c.birthday ? new Date(c.birthday).toISOString().split('T')[0] : '',
      totalSpending: c.totalSpending
    });
    setIsModalOpen(true);
  };

  const fetchLoyaltyHistory = async (customerId: string) => {
    try {
      setHistoryLoading(true);
      const response = await fetch(`${API_URL}/api/Customer/${customerId}/loyalty-history`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('adminToken')}`
        }
      });
      const data = await response.json();
      setLoyaltyHistory(data);
    } catch (err) {
      console.error(err);
    } finally {
      setHistoryLoading(false);
    }
  };

  const openHistory = (c: Customer) => {
    if (!c.id) return;
    setSelectedCustomer(c);
    fetchLoyaltyHistory(c.id);
    setIsHistoryModalOpen(true);
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f8f9fa] text-[13px] font-sans">
      {/* Sidebar Filter */}
      <div className="w-72 bg-white border-r p-6 space-y-8 shadow-sm">
        <h2 className="font-black text-xl text-gray-800 uppercase italic tracking-tighter flex items-center">
           <Users className="mr-3 text-blue-600" size={24}/> Hội viên
        </h2>

        <div className="space-y-6">
          <div>
            <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest mb-4 ml-1">Phân hạng khách hàng</p>
            <div className="space-y-1">
              {['all', 'Khách VIP', 'Khách quen', 'Khách lẻ'].map(group => (
                <label key={group} className="flex items-center cursor-pointer py-2 px-3 rounded-xl transition-all group hover:bg-blue-50">
                  <input
                    type="radio"
                    name="group"
                    className="mr-3 h-4 w-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                    checked={filterGroup === group}
                    onChange={() => setFilterGroup(group)}
                  />
                  <span className={`text-xs font-bold transition-colors ${filterGroup === group ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-500'}`}>
                    {group === 'all' ? 'Tất cả khách hàng' : group}
                  </span>
                </label>
              ))}
            </div>
          </div>

          <div className="bg-blue-50 p-5 rounded-[2rem] border border-blue-100 shadow-inner">
             <p className="text-blue-700 font-black text-[10px] flex items-center mb-2 uppercase tracking-widest italic"><Star size={14} className="mr-2 fill-blue-700"/> Tích điểm tự động</p>
             <p className="text-[10px] text-blue-600/70 leading-relaxed italic font-medium">Hệ thống tự động cộng 1 điểm cho mỗi 10.000đ chi tiêu. Khách hàng có thể dùng điểm để đổi ưu đãi.</p>
          </div>
        </div>
      </div>

      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Actions Bar */}
        <div className="bg-white p-4 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-96">
            <Search className="absolute left-4 top-3 h-4 w-4 text-gray-300" />
            <input
              type="text"
              className="w-full pl-12 pr-4 py-2.5 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:ring-2 focus:ring-blue-500/10 focus:border-blue-500 font-black text-xs transition-all"
              placeholder="Nhập tên, số điện thoại khách..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex space-x-3">
            <button
              onClick={() => {
                setEditingCustomer(null);
                setNewCustomer({ fullName: '', phoneNumber: '', totalSpending: 0 });
                setIsModalOpen(true);
              }}
              className="bg-[#0070f4] text-white px-8 py-2.5 rounded-xl flex items-center font-black text-[10px] uppercase tracking-widest shadow-lg shadow-blue-500/30 hover:bg-blue-700 active:scale-95 transition-all"
            >
              <Plus size={18} className="mr-2" /> THÊM HỘI VIÊN
            </button>
            <button className="bg-white border-2 border-gray-100 px-6 py-2.5 rounded-xl flex items-center hover:bg-gray-50 text-[10px] font-black text-gray-400 uppercase tracking-widest transition-all">
              <Download size={14} className="mr-2" /> Xuất file
            </button>
          </div>
        </div>

        {/* Customer Table */}
        <div className="flex-1 overflow-auto p-8">
          <div className="bg-white rounded-[2.5rem] shadow-2xl shadow-blue-500/5 border border-white overflow-hidden">
            <table className="w-full text-left border-collapse">
              <thead className="bg-gray-50/50 border-b text-gray-400 font-black text-[10px] uppercase tracking-[0.2em]">
                <tr>
                  <th className="px-8 py-5">Định danh hội viên</th>
                  <th className="px-8 py-5">Liên hệ</th>
                  <th className="px-8 py-5 text-right">Tổng chi tiêu</th>
                  <th className="px-8 py-5 text-center">Điểm</th>
                  <th className="px-8 py-5 text-center">Giao dịch cuối</th>
                  <th className="px-8 py-5"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {loading ? (
                  <tr><td colSpan={6} className="py-20 text-center flex flex-col items-center justify-center">
                     <Loader2 className="animate-spin text-blue-600 mb-2" size={32} />
                     <p className="text-[10px] font-black uppercase tracking-widest text-gray-400">Đang truy xuất dữ liệu hội viên...</p>
                  </td></tr>
                ) : filteredCustomers.length === 0 ? (
                  <tr><td colSpan={6} className="py-32 text-center text-gray-300 italic font-bold uppercase tracking-widest text-[10px]">Chưa có dữ liệu hội viên phù hợp</td></tr>
                ) : filteredCustomers.map(c => (
                  <tr key={c.id} className="hover:bg-blue-50/30 transition-all group">
                    <td className="px-8 py-5">
                       <div className="flex items-center">
                          <div className="w-10 h-10 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center mr-4 font-black text-xs border border-blue-100 italic uppercase">
                             {(c.fullName || 'K').split(' ').pop()?.charAt(0)}
                          </div>
                          <div>
                             <p className="font-black text-gray-800 uppercase tracking-tight">{c.fullName || 'Khách lẻ'}</p>
                             <div className="flex items-center mt-0.5">
                                <TrendingUp size={10} className={`${c.customerGroup === 'Khách VIP' ? 'text-orange-500' : c.customerGroup === 'Khách quen' ? 'text-green-500' : 'text-blue-500'} mr-1`}/>
                                <p className={`text-[9px] font-black uppercase tracking-tighter italic ${c.customerGroup === 'Khách VIP' ? 'text-orange-600' : c.customerGroup === 'Khách quen' ? 'text-green-600' : 'text-blue-600'}`}>{c.customerGroup}</p>
                             </div>
                          </div>
                       </div>
                    </td>
                    <td className="px-8 py-5 font-black text-gray-400 italic tracking-tighter">{c.phoneNumber}</td>
                    <td className="px-8 py-5 text-right font-black text-gray-800 tracking-tighter">{c.totalSpending.toLocaleString()}đ</td>
                    <td className="px-8 py-5 text-center">
                       <span className="bg-orange-50 text-orange-600 px-3 py-1 rounded-full font-black text-[10px] border border-orange-100 shadow-sm">
                          {c.loyaltyPoints || 0} PTS
                       </span>
                    </td>
                    <td className="px-8 py-5 text-center text-[10px] font-bold text-gray-400">
                      {c.lastOrderDate ? new Date(c.lastOrderDate).toLocaleDateString('vi-VN') : '---'}
                    </td>
                    <td className="px-8 py-5 text-right">
                       <div className="flex justify-end opacity-0 group-hover:opacity-100 transition-all scale-90 group-hover:scale-100">
                          <button onClick={() => openHistory(c)} className="p-2 text-gray-400 hover:text-blue-600 transition-all" title="Lịch sử"><History size={18}/></button>
                          <button onClick={() => openEditModal(c)} className="p-2 text-gray-400 hover:text-blue-600 transition-all" title="Sửa"><Edit2 size={18}/></button>
                          <button className="p-2 text-gray-400 hover:text-blue-600 transition-all" title="Xóa"><Trash2 size={18}/></button>
                       </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* ADD/EDIT MODAL */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-md animate-in fade-in duration-200">
          <form onSubmit={handleSave} className="bg-white w-full max-w-md rounded-[3rem] overflow-hidden shadow-2xl animate-in zoom-in-95 duration-200 border border-white">
            <div className="bg-[#0070f4] p-8 text-white flex justify-between items-center">
              <div>
                 <h3 className="font-black text-xl uppercase italic tracking-tighter">Hồ sơ hội viên</h3>
                 <p className="text-[10px] font-bold opacity-70 uppercase tracking-widest mt-1">{editingCustomer ? 'Cập nhật thông tin khách hàng' : 'Đăng ký thành viên mới'}</p>
              </div>
              <button type="button" onClick={() => setIsModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-transform"><X size={24}/></button>
            </div>
            <div className="p-10 space-y-6 max-h-[70vh] overflow-y-auto no-scrollbar">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-2 col-span-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Họ và tên khách hàng *</label>
                    <div className="relative group">
                    <User className="absolute left-0 top-2.5 text-gray-300 group-focus-within:text-blue-500 transition-colors" size={18} />
                    <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 pl-8 py-2.5 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent"
                        placeholder="VD: NGUYỄN VĂN A"
                        value={newCustomer.fullName}
                        onChange={e => setNewCustomer({...newCustomer, fullName: e.target.value})}
                        required
                    />
                    </div>
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Số điện thoại *</label>
                    <div className="relative group">
                    <Phone className="absolute left-0 top-2.5 text-gray-300 group-focus-within:text-blue-500 transition-colors" size={18} />
                    <input
                        type="tel"
                        className="w-full border-b-2 border-gray-100 pl-8 py-2.5 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent"
                        placeholder="09xx xxx xxx"
                        value={newCustomer.phoneNumber}
                        onChange={e => setNewCustomer({...newCustomer, phoneNumber: e.target.value.replace(/\D/g, '')})}
                        required
                    />
                    </div>
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Email</label>
                    <div className="relative group">
                    <Mail className="absolute left-0 top-2.5 text-gray-300 group-focus-within:text-blue-500 transition-colors" size={18} />
                    <input
                        type="email"
                        className="w-full border-b-2 border-gray-100 pl-8 py-2.5 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent"
                        placeholder="email@example.com"
                        value={newCustomer.email}
                        onChange={e => setNewCustomer({...newCustomer, email: e.target.value})}
                    />
                    </div>
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Giới tính</label>
                    <select
                        className="w-full border-b-2 border-gray-100 py-2.5 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent appearance-none"
                        value={newCustomer.gender}
                        onChange={e => setNewCustomer({...newCustomer, gender: e.target.value})}
                    >
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                        <option value="Khác">Khác</option>
                    </select>
                </div>

                <div className="space-y-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Ngày sinh</label>
                    <input
                        type="date"
                        className="w-full border-b-2 border-gray-100 py-2.5 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent"
                        value={newCustomer.birthday}
                        onChange={e => setNewCustomer({...newCustomer, birthday: e.target.value})}
                    />
                </div>

                <div className="space-y-2 col-span-2">
                    <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Địa chỉ</label>
                    <div className="relative group">
                    <MapPin className="absolute left-0 top-2.5 text-gray-300 group-focus-within:text-blue-500 transition-colors" size={18} />
                    <input
                        type="text"
                        className="w-full border-b-2 border-gray-100 pl-8 py-2.5 outline-none font-black text-gray-700 focus:border-blue-500 transition-all bg-transparent"
                        placeholder="Địa chỉ..."
                        value={newCustomer.address}
                        onChange={e => setNewCustomer({...newCustomer, address: e.target.value})}
                    />
                    </div>
                </div>
              </div>

              <div className="bg-blue-50 p-6 rounded-[2rem] border-2 border-white shadow-inner">
                 <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-4 italic">Thông tin phân hạng</p>
                 <div className="flex justify-between items-center">
                    <span className="text-[11px] font-bold text-gray-500 uppercase tracking-widest">Tổng chi tiêu:</span>
                    <span className="font-black text-blue-600 tracking-tighter text-lg">{newCustomer.totalSpending.toLocaleString()}đ</span>
                 </div>
              </div>

              <button type="submit" className="w-full py-5 bg-[#0070f4] text-white rounded-[2rem] font-black uppercase tracking-[0.2em] text-[10px] shadow-2xl shadow-blue-500/30 hover:bg-blue-700 active:scale-95 mt-6">XÁC NHẬN LƯU HỒ SƠ</button>
            </div>
          </form>
        </div>
      )}

      {/* LOYALTY HISTORY MODAL */}
      {isHistoryModalOpen && selectedCustomer && (
        <div className="fixed inset-0 bg-black/80 z-[300] flex justify-center items-center p-4 backdrop-blur-md animate-in fade-in duration-200">
           <div className="bg-white w-full max-w-2xl rounded-[3rem] overflow-hidden shadow-2xl animate-in zoom-in-95 duration-200 border border-white flex flex-col h-[80vh]">
              <div className="bg-gray-800 p-8 text-white flex justify-between items-center">
                 <div>
                    <h3 className="font-black text-xl uppercase italic tracking-tighter">Lịch sử tích điểm</h3>
                    <p className="text-[10px] font-bold opacity-70 uppercase tracking-widest mt-1">Khách hàng: {selectedCustomer.fullName}</p>
                 </div>
                 <button onClick={() => setIsHistoryModalOpen(false)} className="bg-white/10 p-2 rounded-full hover:rotate-90 transition-transform"><X size={24}/></button>
              </div>

              <div className="p-8 flex-1 overflow-auto no-scrollbar">
                 {historyLoading ? (
                    <div className="flex justify-center py-20"><Loader2 className="animate-spin text-blue-600" size={32} /></div>
                 ) : loyaltyHistory.length === 0 ? (
                    <div className="text-center py-20 text-gray-300 italic font-bold uppercase tracking-widest text-[10px]">Chưa có giao dịch điểm nào</div>
                 ) : (
                    <div className="space-y-4">
                       {loyaltyHistory.map((t: any) => (
                          <div key={t.id} className="bg-gray-50 p-5 rounded-2xl border border-gray-100 flex items-center justify-between group">
                             <div className="flex items-center">
                                <div className={`w-10 h-10 rounded-xl flex items-center justify-center mr-4 font-black text-xs ${t.points > 0 ? 'bg-green-50 text-green-600' : 'bg-red-50 text-red-600'}`}>
                                   {t.points > 0 ? '+' : ''}{t.points}
                                </div>
                                <div>
                                   <p className="text-xs font-black text-gray-800 uppercase tracking-tight">{t.description}</p>
                                   <p className="text-[9px] text-gray-400 font-bold mt-0.5">{new Date(t.createdAt).toLocaleString('vi-VN')}</p>
                                </div>
                             </div>
                             <div className="text-right">
                                <p className="text-[9px] font-black text-gray-400 uppercase tracking-widest mb-0.5">Số dư sau</p>
                                <p className="text-sm font-black text-gray-800 tracking-tighter">{t.balanceAfter} PTS</p>
                             </div>
                          </div>
                       ))}
                    </div>
                 )}
              </div>

              <div className="p-8 bg-gray-50 border-t flex justify-between items-center">
                 <div className="text-left">
                    <p className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Điểm hiện tại</p>
                    <p className="text-2xl font-black text-orange-600 tracking-tighter">{selectedCustomer.loyaltyPoints || 0} PTS</p>
                 </div>
                 <button onClick={() => setIsHistoryModalOpen(false)} className="px-10 py-4 bg-white border-2 border-gray-200 rounded-[2rem] font-black text-gray-400 text-[10px] uppercase tracking-widest hover:bg-gray-100 transition-all">Đóng lịch sử</button>
              </div>
           </div>
        </div>
      )}
    </div>
  );
};

export default CustomerManagement;

