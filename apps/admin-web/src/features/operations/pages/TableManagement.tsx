import React, { useState, useEffect, useMemo } from 'react';
import { Plus, Search, Filter, MoreVertical, Edit2, Trash2, Loader2, X, MapPin, QrCode, Download, Upload, HelpCircle, LayoutGrid, CheckCircle2, Eye, Printer, AlertTriangle, Settings2, Store, Utensils } from 'lucide-react';
import { API_URL, CUSTOMER_WEB_URL } from '../../../config';
import { notifyFeedback } from '../../../components/ui';

interface Table {
  id?: string;
  name: string;
  areaName: string;
  seatCount: number;
  description: string;
  status: string;
  isActive: boolean;
  qrCodeUrl?: string;
  branchId?: string;
  branchName?: string;
}

interface Area {
  id: string;
  name: string;
  displayOrder: number;
}

interface Branch {
  id: string;
  name: string;
}

const TableManagement = () => {
  const [tables, setTables] = useState<Table[]>([]);
  const [areas, setAreas] = useState<Area[]>([]);
  const [branches, setBranches] = useState<Branch[]>([]);
  const [loading, setLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingTable, setEditingTable] = useState<Table | null>(null);
  const [viewingQr, setViewingQr] = useState<Table | null>(null);
  const [viewingQrToken, setViewingQrToken] = useState<string | null>(null);
  const [isManageAreasMode, setIsManageAreasMode] = useState(false);

  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterArea, setFilterArea] = useState('all');
  const [filterBranch, setFilterBranch] = useState('all');
  const [filterStatus, setFilterStatus] = useState('all'); // all, active, inactive

  // Form State
  const [newTable, setNewTable] = useState<Table>({
    name: '',
    areaName: '',
    seatCount: 4,
    description: '',
    status: 'Trống',
    isActive: true,
    branchId: ''
  });

  const fetchData = async () => {
    try {
      setLoading(true);
      const [resTables, resAreas, resBranches] = await Promise.all([
        fetch(`${API_URL}/api/Table`),
        fetch(`${API_URL}/api/Area`),
        fetch(`${API_URL}/api/Branch`)
      ]);

      const dataTables = await resTables.json();
      const dataAreas = await resAreas.json();
      const dataBranches = await resBranches.json();

      setTables(dataTables);
      setAreas(dataAreas);
      setBranches(dataBranches);

      if (dataAreas.length > 0 && !newTable.areaName) {
         setNewTable(prev => ({ ...prev, areaName: dataAreas[0].name }));
      }
    } catch (err) {
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    if (!viewingQr?.id) {
      setViewingQrToken(null);
      return;
    }

    fetch(`${API_URL}/api/Table/${viewingQr.id}/qr-token`)
      .then(response => response.ok ? response.json() : Promise.reject())
      .then(data => setViewingQrToken(data.qrToken))
      .catch(() => {
        setViewingQrToken(null);
        notifyFeedback('Không thể tải mã QR của bàn.', 'error');
      });
  }, [viewingQr?.id]);

  const filteredTables = useMemo(() => {
    return tables.filter(t => {
      const matchSearch = t.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          (t.description && t.description.toLowerCase().includes(searchTerm.toLowerCase()));

      const matchBranch = filterBranch === 'all' || t.branchId === filterBranch;
      const matchArea = filterArea === 'all' || t.areaName === filterArea;
      const matchStatus = filterStatus === 'all' ||
                          (filterStatus === 'active' && t.isActive) ||
                          (filterStatus === 'inactive' && !t.isActive);

      return matchSearch && matchBranch && matchArea && matchStatus;
    });
  }, [tables, searchTerm, filterBranch, filterArea, filterStatus]);

  const handleSaveTable = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const isEditing = !!editingTable;
      const url = isEditing
        ? `${API_URL}/api/Table/${editingTable.id}`
        : `${API_URL}/api/Table`;

      const payload = {
        ...(isEditing ? editingTable : {}),
        ...newTable,
        branchName: branches.find(b => b.id === newTable.branchId)?.name
      };

      const response = await fetch(url, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        setIsModalOpen(false);
        setEditingTable(null);
        resetForm();
        fetchData();
      } else if (response.status !== 403) {
        notifyFeedback('Lỗi khi lưu thông tin bàn.');
      }
    } catch (err) {
      notifyFeedback('Lỗi kết nối đến server.');
    }
  };

  const resetForm = () => {
    setNewTable({
      name: '',
      areaName: areas[0]?.name || '',
      seatCount: 4,
      description: '',
      status: 'Trống',
      isActive: true,
      branchId: branches.length > 0 ? branches[0].id : ''
    });
  };

  const openEditModal = (table: Table) => {
    setEditingTable(table);
    setNewTable({
      name: table.name,
      areaName: table.areaName,
      seatCount: table.seatCount,
      description: table.description || '',
      status: table.status,
      isActive: table.isActive,
      branchId: table.branchId || ''
    });
    setIsModalOpen(true);
  };

  const handleDeleteTable = async (id: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn xóa bàn này không?')) return;
    try {
      const response = await fetch(`${API_URL}/api/Table/${id}`, { method: 'DELETE' });
      if (response.ok) fetchData();
    } catch (err) { notifyFeedback('Lỗi khi xóa bàn'); }
  };

  const handleAddArea = async () => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    const areaName = prompt('Nhập tên khu vực mới:');
    if (areaName) {
      try {
        const response = await fetch(`${API_URL}/api/Area`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ name: areaName, displayOrder: areas.length + 1 })
        });
        if (response.ok) fetchData();
      } catch (err) { notifyFeedback('Lỗi khi thêm khu vực'); }
    }
  };

  const handleEditArea = async (area: Area) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    const newName = prompt('Nhập tên mới cho khu vực:', area.name);
    if (newName && newName !== area.name) {
      try {
        const response = await fetch(`${API_URL}/api/Area/${area.id}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ ...area, name: newName })
        });
        if (response.ok) fetchData();
        else if (response.status !== 403) notifyFeedback('Lỗi khi cập nhật khu vực');
      } catch (err) { notifyFeedback('Lỗi kết nối server'); }
    }
  };

  const handleDeleteArea = async (id: string) => {
    if (localStorage.getItem('userRole') !== 'admin') {
      notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
      return;
    }
    if (!window.confirm('Xóa khu vực này? Tất cả bàn thuộc khu vực này phải được xóa hoặc chuyển đi trước.')) return;
    try {
      const response = await fetch(`${API_URL}/api/Area/${id}`, { method: 'DELETE' });
      if (response.ok) {
        fetchData();
      } else if (response.status !== 403) {
        const error = await response.json();
        notifyFeedback(error.message || 'Lỗi khi xóa khu vực');
      }
    } catch (err) { notifyFeedback('Lỗi kết nối server'); }
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f8f9fa] text-[13px] font-sans">
      {/* SIDEBAR FILTER */}
      <div className="w-72 bg-white border-r overflow-y-auto p-6 space-y-8 shadow-sm">
        <h2 className="font-black text-xl text-gray-800 uppercase tracking-tighter italic flex items-center">
           <Store className="mr-3 text-blue-600" size={24}/> Phòng/Bàn
        </h2>

        {/* Chi nhánh Filter */}
        <div className="space-y-3">
          <p className="font-black text-gray-400 text-[10px] uppercase tracking-widest ml-1">Lọc theo chi nhánh</p>
          <div className="space-y-1">
             <label className="flex items-center cursor-pointer py-2 px-3 rounded-xl transition-all group hover:bg-blue-50">
                <input
                  type="radio"
                  name="branch"
                  className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                  checked={filterBranch === 'all'}
                  onChange={() => setFilterBranch('all')}
                />
                <span className={`text-xs font-bold transition-colors ${filterBranch === 'all' ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-600'}`}>Tất cả cơ sở</span>
             </label>
             {branches.map(b => (
               <label key={b.id} className="flex items-center cursor-pointer py-2 px-3 rounded-xl transition-all group hover:bg-blue-50">
                  <input
                    type="radio"
                    name="branch"
                    className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                    checked={filterBranch === b.id}
                    onChange={() => setFilterBranch(b.id)}
                  />
                  <span className={`text-xs font-bold transition-colors ${filterBranch === b.id ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-600'}`}>{b.name}</span>
               </label>
             ))}
          </div>
        </div>

        {/* Khu vực Filter */}
        <div className="space-y-3 pt-6 border-t">
          <div className="flex justify-between items-center px-1">
            <p className="font-black text-gray-400 text-[10px] uppercase tracking-widest">Khu vực / Tầng</p>
            <div className="flex space-x-1">
              <button
                onClick={() => setIsManageAreasMode(!isManageAreasMode)}
                className={`p-1.5 rounded-lg hover:bg-gray-100 transition-colors ${isManageAreasMode ? 'text-blue-600 bg-blue-50' : 'text-gray-400'}`}
                title="Quản lý khu vực"
              >
                <Settings2 size={14}/>
              </button>
              <button onClick={handleAddArea} className="p-1.5 rounded-lg text-blue-600 hover:bg-blue-50 transition-colors" title="Thêm khu vực">
                <Plus size={14}/>
              </button>
            </div>
          </div>
          <div className="space-y-1">
             <label className="flex items-center cursor-pointer py-2 px-3 rounded-xl transition-all group hover:bg-blue-50">
                <input
                  type="radio"
                  name="area"
                  className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                  checked={filterArea === 'all'}
                  onChange={() => setFilterArea('all')}
                />
                <span className={`text-xs font-bold transition-colors ${filterArea === 'all' ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-600'}`}>Tất cả khu vực</span>
             </label>
             {areas.map(a => (
               <div key={a.id} className="flex items-center justify-between group/area py-0.5 px-3 rounded-xl hover:bg-blue-50 transition-all">
                 <label className="flex items-center cursor-pointer flex-1 group">
                    <input
                      type="radio"
                      name="area"
                      className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                      checked={filterArea === a.name}
                      onChange={() => setFilterArea(a.name)}
                    />
                    <span className={`text-xs font-bold transition-colors ${filterArea === a.name ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-600'}`}>{a.name}</span>
                 </label>
                 {isManageAreasMode && (
                   <div className="flex space-x-1 animate-in slide-in-from-right-2 duration-200">
                     <button onClick={() => handleEditArea(a)} className="p-1 text-gray-400 hover:text-blue-600"><Edit2 size={12}/></button>
                     <button onClick={() => handleDeleteArea(a.id)} className="p-1 text-gray-400 hover:text-blue-600"><Trash2 size={12}/></button>
                   </div>
                 )}
               </div>
             ))}
          </div>
        </div>

        {/* Trạng thái Filter */}
        <div className="space-y-3 pt-6 border-t">
          <p className="font-black text-gray-400 text-[10px] uppercase tracking-widest ml-1">Trạng thái kinh doanh</p>
          <div className="space-y-1">
            {[
              { id: 'all', label: 'Tất cả' },
              { id: 'active', label: 'Đang sử dụng' },
              { id: 'inactive', label: 'Đã khóa / Ngưng' }
            ].map(s => (
              <label key={s.id} className="flex items-center cursor-pointer py-2 px-3 rounded-xl group hover:bg-blue-50">
                <input
                  type="radio"
                  name="status"
                  className="mr-3 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                  checked={filterStatus === s.id}
                  onChange={() => setFilterStatus(s.id)}
                />
                <span className={`text-xs font-bold transition-colors ${filterStatus === s.id ? 'text-blue-600' : 'text-gray-500 group-hover:text-blue-500'}`}>{s.label}</span>
              </label>
            ))}
          </div>
        </div>
      </div>

      {/* MAIN CONTENT */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Actions Bar */}
        <div className="bg-white p-4 flex justify-between items-center border-b shadow-sm">
          <div className="relative w-96">
            <Search className="absolute left-4 top-3 h-4 w-4 text-gray-300" />
            <input
              type="text"
              className="w-full pl-12 pr-4 py-2.5 bg-gray-50 border border-gray-100 rounded-xl outline-none focus:ring-2 focus:ring-blue-500/10 focus:border-blue-500 font-bold text-xs transition-all"
              placeholder="Tìm nhanh theo tên bàn hoặc ghi chú..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex space-x-3">
            <button
              onClick={() => {
                const role = localStorage.getItem('userRole');
                if (role !== 'admin' && role !== 'manager') {
                  notifyFeedback("Bạn không có quyền thực hiện chức năng này.");
                  return;
                }
                resetForm(); setEditingTable(null); setIsModalOpen(true);
              }}
              className="bg-[#0070f4] text-white px-8 py-2.5 rounded-xl flex items-center font-black text-[10px] uppercase tracking-widest hover:bg-blue-700 transition-all shadow-lg shadow-blue-500/30 active:scale-95"
            >
              <Plus size={16} className="mr-2" /> THÊM PHÒNG/BÀN
            </button>
          </div>
        </div>

        {/* Table Grid */}
        <div className="flex-1 overflow-auto p-8 bg-[#f8f9fa]">
          {loading ? (
            <div className="flex flex-col items-center justify-center py-20 text-blue-600">
               <Loader2 className="animate-spin mb-2" size={32} />
               <p className="font-black text-[10px] uppercase tracking-widest">Đang cập nhật sơ đồ...</p>
            </div>
          ) : filteredTables.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-32 bg-white rounded-[3rem] border-2 border-dashed border-gray-100">
               <AlertTriangle size={64} className="mb-4 text-gray-100" />
               <p className="text-sm font-black text-gray-400 uppercase tracking-widest">Không tìm thấy bàn nào</p>
               <button onClick={() => { setSearchTerm(''); setFilterArea('all'); setFilterBranch('all'); setFilterStatus('all'); }} className="mt-4 text-blue-600 text-[10px] font-black uppercase hover:underline">Xóa tất cả bộ lọc</button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-8">
                {filteredTables.map(t => (
                  <div key={t.id} className={`bg-white rounded-[2.5rem] shadow-xl shadow-blue-500/5 border-2 p-8 hover:border-blue-500 hover:-translate-y-1 transition-all group relative overflow-hidden ${!t.isActive ? 'bg-gray-100/50 border-gray-200 grayscale-[0.5]' : 'border-white'}`}>
                     <div className="flex justify-between items-start mb-8">
                        <div className={`p-4 rounded-[1.5rem] shadow-sm ${t.status === 'Trống' ? 'bg-green-50 text-green-600 border border-green-100' : 'bg-blue-50 text-blue-600 border border-blue-100'}`}>
                           <Utensils size={32} className={t.status !== 'Trống' ? 'animate-pulse' : ''} />
                        </div>
                        <div className="flex bg-gray-50 p-1.5 rounded-2xl opacity-0 group-hover:opacity-100 transition-all scale-90 group-hover:scale-100">
                           <button onClick={() => setViewingQr(t)} className="p-2 text-gray-400 hover:text-blue-600 transition-all" title="Xem QR"><QrCode size={18}/></button>
                           <button onClick={() => openEditModal(t)} className="p-2 text-gray-400 hover:text-blue-600 transition-all" title="Chỉnh sửa"><Edit2 size={18}/></button>
                           <button onClick={() => handleDeleteTable(t.id!)} className="p-2 text-gray-400 hover:text-blue-600 transition-all" title="Xóa"><Trash2 size={18}/></button>
                        </div>
                     </div>

                     <div className="space-y-1">
                        <h3 className="font-black text-2xl text-gray-800 tracking-tighter uppercase italic leading-none">{t.name}</h3>
                        <p className="text-[10px] font-black text-blue-500 uppercase tracking-widest flex items-center pt-2">
                           <MapPin size={10} className="mr-1.5"/> {t.areaName} <span className="mx-2 opacity-20">/</span> {t.branchName || 'Tất cả'}
                        </p>

                        <div className="pt-6 flex items-center justify-between border-t border-gray-50 mt-6">
                           <div className="flex flex-col">
                              <span className="text-[9px] font-black text-gray-400 uppercase leading-none mb-1">Quy mô</span>
                              <span className="text-sm font-black text-gray-700 tracking-tight">{t.seatCount} CHỖ NGỒI</span>
                           </div>
                           <span className={`px-4 py-1.5 rounded-full text-[9px] font-black uppercase tracking-widest shadow-lg border ${
                              t.status === 'Trống' ? 'bg-green-500 text-white border-green-400 shadow-green-500/20' : 'bg-blue-600 text-white border-blue-500 shadow-blue-500/20'
                           }`}>
                              {t.status}
                           </span>
                        </div>
                     </div>

                     {!t.isActive && (
                       <div className="absolute top-3 right-3 bg-gray-800 text-white text-[8px] font-black px-3 py-1 rounded-full uppercase tracking-widest shadow-lg italic">Tạm ngưng</div>
                     )}
                  </div>
                ))}
            </div>
          )}
        </div>
      </div>

      {/* QR MODAL */}
      {viewingQr && (
        <div className="fixed inset-0 bg-black/80 z-[200] flex justify-center items-center p-4 backdrop-blur-md animate-in fade-in duration-300">
           <div className="bg-white rounded-[3rem] p-12 max-w-sm w-full text-center shadow-2xl relative overflow-hidden">
              <div className="absolute top-0 left-0 w-full h-3 bg-[#0070f4]"></div>
              <button onClick={() => setViewingQr(null)} className="absolute top-6 right-6 text-gray-300 hover:text-gray-800 hover:rotate-90 transition-all"><X size={32}/></button>

              <span className="bg-blue-50 text-[#0070f4] text-[10px] font-black px-6 py-2 rounded-full uppercase tracking-[0.2em] border border-blue-100 italic">Smart Ordering QR</span>
              <h3 className="text-4xl font-black text-gray-800 uppercase tracking-tighter mt-6 mb-1 italic">{viewingQr.name}</h3>
              <p className="text-xs text-gray-400 mb-10 uppercase font-black tracking-[0.2em] opacity-60">{viewingQr.areaName} — {viewingQr.branchName}</p>

              <div className="bg-gray-50 p-10 rounded-[3rem] border-4 border-white shadow-2xl mb-10 group relative overflow-hidden">
                 {viewingQrToken && <img src={`https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=${encodeURIComponent(`${CUSTOMER_WEB_URL}?qr=${viewingQrToken}`)}`} alt="QR" className="w-full aspect-square" />}
                 <div className="absolute inset-0 bg-blue-600/5 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                    <QrCode size={48} className="text-blue-100 opacity-20" />
                 </div>
              </div>

              <div className="flex space-x-4">
                 <button className="flex-1 py-5 bg-gray-100 text-gray-600 rounded-3xl font-black text-[10px] uppercase tracking-widest hover:bg-gray-200 transition-all flex items-center justify-center active:scale-95">
                    <Download size={18} className="mr-2" /> TẢI VỀ
                 </button>
                 <button className="flex-[2] py-5 bg-[#0070f4] text-white rounded-3xl font-black text-[10px] uppercase tracking-widest shadow-xl shadow-blue-500/30 hover:bg-blue-700 transition-all flex items-center justify-center active:scale-95">
                    <Printer size={18} className="mr-2" /> IN NHÃN QR
                 </button>
              </div>
           </div>
        </div>
      )}

      {/* FORM MODAL */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/60 z-[100] flex justify-center items-start pt-20 backdrop-blur-sm px-4">
           <div className="bg-white w-full max-w-lg rounded-[3rem] shadow-2xl overflow-hidden animate-in slide-in-from-bottom-4 duration-300">
              <div className="bg-[#0070f4] p-6 text-white flex justify-between items-center">
                 <h3 className="font-black text-xl uppercase italic tracking-tighter flex items-center"><LayoutGrid size={24} className="mr-3"/> {editingTable ? 'Cập nhật phòng/bàn' : 'Thêm phòng/bàn mới'}</h3>
                 <button onClick={() => setIsModalOpen(false)} className="hover:rotate-90 transition-transform"><X size={24}/></button>
              </div>

              <form onSubmit={handleSaveTable}>
                <div className="p-10 space-y-8">
                   <div className="grid grid-cols-2 gap-8">
                      <div className="space-y-1">
                         <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Tên phòng/bàn *</label>
                         <input type="text" className="w-full border-b-2 border-gray-100 py-2.5 outline-none focus:border-blue-500 font-black text-gray-700 transition-all bg-transparent" placeholder="VD: Bàn 01" value={newTable.name} onChange={e => setNewTable({...newTable, name: e.target.value})} required />
                      </div>
                      <div className="space-y-1">
                         <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Số ghế ngồi</label>
                         <input type="number" className="w-full border-b-2 border-gray-100 py-2.5 outline-none focus:border-blue-500 font-black text-gray-700 transition-all bg-transparent" value={newTable.seatCount} onChange={e => setNewTable({...newTable, seatCount: parseInt(e.target.value) || 0})} />
                      </div>
                   </div>
                   <div className="space-y-1">
                      <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Cơ sở (Chi nhánh) *</label>
                      <select className="w-full border-b-2 border-gray-100 py-2.5 outline-none focus:border-blue-500 bg-transparent font-black text-gray-700 transition-all" value={newTable.branchId} onChange={e => setNewTable({...newTable, branchId: e.target.value})} required>
                         <option value="">-- Chọn một cơ sở --</option>
                         {branches.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
                      </select>
                   </div>
                   <div className="space-y-1">
                      <label className="text-[10px] font-black text-gray-400 uppercase tracking-widest ml-1">Khu vực / Tầng</label>
                      <div className="flex items-center">
                         <select className="w-full border-b-2 border-gray-100 py-2.5 outline-none focus:border-blue-500 bg-transparent font-black text-gray-700 flex-1 transition-all" value={newTable.areaName} onChange={e => setNewTable({...newTable, areaName: e.target.value})}>
                            <option value="">-- Chọn khu vực --</option>
                            {areas.map(area => <option key={area.id} value={area.name}>{area.name}</option>)}
                         </select>
                         <button type="button" onClick={handleAddArea} className="ml-4 text-blue-600 p-2 bg-blue-50 rounded-xl hover:bg-blue-100 transition-all shadow-sm"><Plus size={20}/></button>
                      </div>
                   </div>
                   <div className="flex items-center pt-4 justify-between bg-gray-50 p-6 rounded-[2rem] border border-gray-100 shadow-inner">
                      <div className="flex items-center">
                         <input
                           type="checkbox"
                           id="active-check"
                           className="hidden"
                           checked={newTable.isActive}
                           onChange={e => setNewTable({...newTable, isActive: e.target.checked})}
                         />
                         <label htmlFor="active-check" className="flex items-center cursor-pointer">
                            <div className={`w-10 h-6 rounded-full relative transition-all mr-3 ${newTable.isActive ? 'bg-blue-600' : 'bg-gray-300'}`}>
                               <div className={`absolute top-1 w-4 h-4 bg-white rounded-full transition-all ${newTable.isActive ? 'left-5' : 'left-1'}`}></div>
                            </div>
                            <span className="font-black text-gray-700 text-[10px] uppercase tracking-widest">Kích hoạt hoạt động</span>
                         </label>
                      </div>
                   </div>
                </div>
                <div className="p-8 bg-gray-100 border-t flex justify-end space-x-4">
                   <button type="button" onClick={() => setIsModalOpen(false)} className="px-10 py-4 bg-white border-2 border-gray-200 rounded-2xl font-black text-gray-400 text-[10px] uppercase tracking-[0.2em] transition-all hover:bg-gray-50 active:scale-95">HỦY BỎ</button>
                   <button type="submit" className="px-12 py-4 bg-[#0070f4] text-white rounded-2xl font-black shadow-xl shadow-blue-500/30 hover:bg-blue-700 transition-all text-[10px] uppercase tracking-[0.2em] active:scale-95">XÁC NHẬN LƯU</button>
                </div>
              </form>
           </div>
        </div>
      )}
    </div>
  );
};

export default TableManagement;

