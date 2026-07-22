import React, { useState, useEffect } from 'react';
import { Plus, Search, Filter, MoreVertical, Edit2, Trash2, Loader2, X, MapPin, QrCode, Download, Upload, HelpCircle, LayoutGrid, CheckCircle2 } from 'lucide-react';

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

  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [filterArea, setFilterArea] = useState('');
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

  const fetchBranches = async () => {
    try {
      const response = await fetch('http://localhost:5000/api/Branch');
      const data = await response.json();
      setBranches(data);
    } catch (err) {
      console.error('Error fetching branches:', err);
    }
  };

  const fetchAreas = async () => {
    try {
      const response = await fetch('http://localhost:5000/api/Area');
      const data = await response.json();
      setAreas(data);
      if (data.length > 0 && !newTable.areaName) {
        setNewTable(prev => ({ ...prev, areaName: data[0].name }));
      }
    } catch (err) {
      console.error('Error fetching areas:', err);
    }
  };

  const fetchTables = async () => {
    try {
      setLoading(true);
      let url = 'http://localhost:5000/api/Table';
      const params = new URLSearchParams();
      if (searchTerm) params.append('search', searchTerm);
      if (filterArea) params.append('area', filterArea);
      if (filterBranch !== 'all') params.append('branchId', filterBranch);
      if (filterStatus !== 'all') params.append('isActive', (filterStatus === 'active').toString());

      if (params.toString()) {
        url += `?${params.toString()}`;
      }

      const response = await fetch(url);
      const data: Table[] = await response.json();
      setTables(data);
    } catch (err) {
      console.error('Error fetching tables:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAreas();
    fetchBranches();
  }, []);

  useEffect(() => {
    fetchTables();

    // Tự động làm mới mỗi 30 giây để cập nhật trạng thái từ POS
    const interval = setInterval(fetchTables, 30000);
    return () => clearInterval(interval);
  }, [filterArea, filterStatus, searchTerm, filterBranch]);

  const handleSaveTable = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const isEditing = !!editingTable;
      const url = isEditing
        ? `http://localhost:5000/api/Table/${editingTable.id}`
        : 'http://localhost:5000/api/Table';

      const response = await fetch(url, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(isEditing ? {
          ...editingTable,
          ...newTable,
          branchName: branches.find(b => b.id === newTable.branchId)?.name
        } : {
          ...newTable,
          branchName: branches.find(b => b.id === newTable.branchId)?.name
        })
      });

      if (response.ok) {
        setIsModalOpen(false);
        setEditingTable(null);
        setNewTable({ name: '', areaName: areas[0]?.name || '', seatCount: 4, description: '', status: 'Trống', isActive: true, branchId: '' });
        fetchTables();
      } else {
        alert('Lỗi khi lưu thông tin bàn.');
      }
    } catch (err) {
      console.error('Error saving table:', err);
      alert('Lỗi kết nối đến server.');
    }
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
      const response = await fetch(`http://localhost:5000/api/Table/${id}`, {
        method: 'DELETE'
      });
      if (response.ok) {
        fetchTables();
      }
    } catch (err) {
      alert('Lỗi khi xóa bàn');
    }
  };

  const handleAddArea = async () => {
    const areaName = prompt('Nhập tên khu vực mới:');
    if (areaName) {
      try {
        const response = await fetch('http://localhost:5000/api/Area', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ name: areaName, displayOrder: areas.length + 1 })
        });
        if (response.ok) {
          fetchAreas();
        }
      } catch (err) {
        alert('Lỗi khi thêm khu vực');
      }
    }
  };

  return (
    <div className="flex h-[calc(100vh-48px)] bg-[#f0f2f5] text-[13px]">
      {/* SIDEBAR FILTER */}
      <div className="w-64 bg-white border-r overflow-y-auto p-4 space-y-6">
        <h2 className="font-bold text-base">Phòng/Bàn</h2>

        {/* Chi nhánh */}
        <div className="space-y-2">
          <p className="font-bold text-gray-700">Chi nhánh</p>
          <div className="space-y-1 ml-1">
             <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="branchFilter"
                  className="mr-2"
                  checked={filterBranch === 'all'}
                  onChange={() => setFilterBranch('all')}
                />
                <span className={filterBranch === 'all' ? 'text-blue-600 font-bold' : ''}>Tất cả cơ sở</span>
             </label>
             {branches.map(branch => (
               <label key={branch.id} className="flex items-center cursor-pointer">
                  <input
                    type="radio"
                    name="branchFilter"
                    className="mr-2"
                    checked={filterBranch === branch.id}
                    onChange={() => setFilterBranch(branch.id)}
                  />
                  <span className={filterBranch === branch.id ? 'text-blue-600 font-bold' : ''}>{branch.name}</span>
               </label>
             ))}
          </div>
        </div>

        {/* Khu vực */}
        <div className="space-y-2">
          <div className="flex justify-between items-center">
            <p className="font-bold text-gray-700">Khu vực</p>
            <button onClick={handleAddArea} className="text-blue-600 text-[11px] hover:underline">Tạo mới</button>
          </div>
          <div className="space-y-1 ml-1">
             <label className="flex items-center cursor-pointer">
                <input
                  type="radio"
                  name="area"
                  className="mr-2"
                  checked={filterArea === ''}
                  onChange={() => setFilterArea('')}
                />
                <span className={filterArea === '' ? 'text-blue-600 font-bold' : ''}>Tất cả</span>
             </label>
             {areas.map(area => (
               <label key={area.id} className="flex items-center cursor-pointer">
                  <input
                    type="radio"
                    name="area"
                    className="mr-2"
                    checked={filterArea === area.name}
                    onChange={() => setFilterArea(area.name)}
                  />
                  <span className={filterArea === area.name ? 'text-blue-600 font-bold' : ''}>{area.name}</span>
               </label>
             ))}
          </div>
        </div>

        {/* Trạng thái */}
        <div className="space-y-2 pt-4 border-t">
          <p className="font-bold text-gray-700">Trạng thái</p>
          <div className="space-y-1.5 ml-1">
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" checked={filterStatus === 'active'} onChange={() => setFilterStatus('active')} />
              <span className={filterStatus === 'active' ? 'text-blue-600 font-bold' : ''}>Đang hoạt động</span>
            </label>
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" checked={filterStatus === 'inactive'} onChange={() => setFilterStatus('inactive')} />
              <span className={filterStatus === 'inactive' ? 'text-blue-600 font-bold' : ''}>Ngừng hoạt động</span>
            </label>
            <label className="flex items-center cursor-pointer">
              <input type="radio" name="status" className="mr-2" checked={filterStatus === 'all'} onChange={() => setFilterStatus('all')} />
              <span className={filterStatus === 'all' ? 'text-blue-600 font-bold' : ''}>Tất cả</span>
            </label>
          </div>
        </div>
      </div>

      {/* MAIN CONTENT */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Actions Bar */}
        <div className="bg-white p-3 flex justify-between items-center border-b">
          <div className="relative w-80">
            <Search className="absolute left-3 top-2 h-4 w-4 text-gray-400" />
            <input
              type="text"
              className="w-full pl-9 pr-3 py-1.5 bg-gray-100 border-none rounded-md outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Theo tên phòng bàn, số ghế"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <div className="flex space-x-2">
            <button
              onClick={() => { setEditingTable(null); setIsModalOpen(true); }}
              className="bg-[#0070f4] text-white px-4 py-1.5 rounded flex items-center font-bold hover:bg-blue-700"
            >
              <Plus size={16} className="mr-1" /> Thêm phòng/bàn
            </button>
            <button className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50">
              <Upload size={14} className="mr-1 text-gray-400" /> Import
            </button>
            <button className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50">
              <Download size={14} className="mr-1 text-gray-400" /> Xuất file
            </button>
            <button className="bg-white border border-gray-300 px-3 py-1.5 rounded flex items-center hover:bg-gray-50">
              <QrCode size={14} className="mr-1 text-gray-400" /> Tải tất cả mã QR
            </button>
          </div>
        </div>

        {/* Table List */}
        <div className="flex-1 overflow-auto bg-white">
          {loading ? (
            <div className="flex justify-center py-20"><Loader2 className="animate-spin text-blue-600" /> Đang tải...</div>
          ) : tables.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-32 text-gray-400">
               <div className="w-20 h-20 bg-blue-50 rounded-full flex items-center justify-center mb-4">
                  <LayoutGrid size={40} className="text-blue-200" />
               </div>
               <p className="text-base font-medium text-gray-600">Bắt đầu quản lý không gian của hàng với khu vực & phòng bàn</p>
               <p className="text-xs mb-6">Dễ thiết lập hồ sơ cửa hàng, trước tiên hãy tạo khu vực hoặc bạn có thể thêm phòng bàn ngay để quản lý và phục vụ khách hàng hiệu quả hơn.</p>
               <div className="flex space-x-4">
                  <button onClick={() => setIsModalOpen(true)} className="bg-white border border-gray-300 px-6 py-2 rounded font-bold text-gray-700 hover:bg-gray-50 transition-colors">Tạo phòng bàn</button>
                  <button onClick={handleAddArea} className="bg-[#0070f4] text-white px-6 py-2 rounded font-bold hover:bg-blue-700 transition-colors">Tạo Khu vực</button>
               </div>
            </div>
          ) : (
            <table className="w-full text-left border-collapse">
              <thead className="bg-[#f9fafb] border-b text-gray-500 font-bold sticky top-0 z-10">
                <tr>
                  <th className="px-4 py-2 w-10"><input type="checkbox" /></th>
                  <th className="px-4 py-2 uppercase text-[11px]">Tên phòng/bàn</th>
                  <th className="px-4 py-2 uppercase text-[11px]">Chi nhánh</th>
                  <th className="px-4 py-2 uppercase text-[11px]">Ghi chú</th>
                  <th className="px-4 py-2 w-48 uppercase text-[11px]">Khu vực</th>
                  <th className="px-4 py-2 w-32 uppercase text-[11px] text-center">Số ghế</th>
                  <th className="px-4 py-2 w-40 uppercase text-[11px] text-center">Tình trạng</th>
                  <th className="px-4 py-2 w-40 uppercase text-[11px] text-center">Hoạt động</th>
                  <th className="px-4 py-2 w-20 uppercase text-[11px] text-right"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {tables.map(t => (
                  <tr key={t.id} className="hover:bg-[#eef6ff] group transition-colors">
                    <td className="px-4 py-3"><input type="checkbox" /></td>
                    <td className="px-4 py-3 text-blue-600 font-medium">{t.name}</td>
                    <td className="px-4 py-3 text-gray-600 font-bold">{t.branchName || '---'}</td>
                    <td className="px-4 py-3 text-gray-400 italic">{t.description || '---'}</td>
                    <td className="px-4 py-3">{t.areaName}</td>
                    <td className="px-4 py-3 text-center">{t.seatCount}</td>
                    <td className="px-4 py-3 text-center">
                       <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-[10px] font-black uppercase ${t.status === 'Trống' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'}`}>
                          {t.status}
                       </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                       <span className={`inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium ${t.isActive ? 'bg-blue-50 text-blue-600' : 'bg-gray-100 text-gray-500'}`}>
                          {t.isActive ? 'Đang hoạt động' : 'Ngừng hoạt động'}
                       </span>
                    </td>
                    <td className="px-4 py-3 text-right">
                       <div className="flex justify-end opacity-0 group-hover:opacity-100 transition-opacity">
                          <button onClick={() => openEditModal(t)} className="p-1 text-blue-600 hover:bg-blue-50 rounded mr-1"><Edit2 size={14}/></button>
                          <button onClick={() => handleDeleteTable(t.id!)} className="p-1 text-red-600 hover:bg-red-50 rounded"><Trash2 size={14}/></button>
                       </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {/* FORM MODAL */}
      {isModalOpen && (
        <div className="fixed inset-0 bg-black/40 z-[100] flex justify-center items-start pt-20">
           <div className="bg-white w-full max-w-lg rounded-lg shadow-2xl overflow-hidden animate-in slide-in-from-bottom-4 duration-300">
              <div className="bg-[#0070f4] p-4 text-white flex justify-between items-center">
                 <h3 className="font-bold text-lg">{editingTable ? 'Cập nhật phòng/bàn' : 'Thêm phòng/bàn'}</h3>
                 <button onClick={() => setIsModalOpen(false)}><X size={24}/></button>
              </div>

              <form onSubmit={handleSaveTable}>
                <div className="p-6 space-y-4">
                   <div className="grid grid-cols-2 gap-4">
                      <div className="space-y-1">
                         <label className="text-[11px] text-gray-400 font-bold uppercase">Tên phòng/bàn</label>
                         <input
                           type="text"
                           className="w-full border-b py-1.5 outline-none focus:border-blue-500 font-medium"
                           placeholder="VD: Bàn 01"
                           value={newTable.name}
                           onChange={e => setNewTable({...newTable, name: e.target.value})}
                           required
                         />
                      </div>
                      <div className="space-y-1">
                         <label className="text-[11px] text-gray-400 font-bold uppercase">Số ghế</label>
                         <input
                           type="number"
                           className="w-full border-b py-1.5 outline-none focus:border-blue-500 font-medium"
                           value={newTable.seatCount}
                           onChange={e => setNewTable({...newTable, seatCount: parseInt(e.target.value) || 0})}
                         />
                      </div>
                   </div>

                   <div className="space-y-1">
                      <label className="text-[11px] text-gray-400 font-bold uppercase">Chi nhánh sở hữu</label>
                      <select
                        className="w-full border-b py-1.5 outline-none focus:border-blue-500 bg-transparent font-medium"
                        value={newTable.branchId}
                        onChange={e => setNewTable({...newTable, branchId: e.target.value})}
                        required
                      >
                         <option value="">Chọn chi nhánh</option>
                         {branches.map(b => (
                           <option key={b.id} value={b.id}>{b.name}</option>
                         ))}
                      </select>
                   </div>

                   <div className="space-y-1">
                      <label className="text-[11px] text-gray-400 font-bold uppercase">Khu vực</label>
                      <div className="flex items-center">
                         <select
                           className="w-full border-b py-1.5 outline-none focus:border-blue-500 bg-transparent font-medium"
                           value={newTable.areaName}
                           onChange={e => setNewTable({...newTable, areaName: e.target.value})}
                         >
                            <option value="">Chọn khu vực</option>
                            {areas.map(area => (
                              <option key={area.id} value={area.name}>{area.name}</option>
                            ))}
                         </select>
                         <button type="button" onClick={handleAddArea} className="ml-2 text-blue-600 p-1 hover:bg-blue-50 rounded"><Plus size={18}/></button>
                      </div>
                   </div>

                   <div className="space-y-1">
                      <label className="text-[11px] text-gray-400 font-bold uppercase">Ghi chú</label>
                      <textarea
                        className="w-full border border-gray-200 rounded p-2 outline-none focus:border-blue-500 h-20 text-[12px]"
                        placeholder="Mô tả hoặc vị trí bàn..."
                        value={newTable.description}
                        onChange={e => setNewTable({...newTable, description: e.target.value})}
                      />
                   </div>

                   <div className="flex items-center pt-2">
                      <label className="flex items-center cursor-pointer">
                         <input
                           type="checkbox"
                           className="mr-2 h-4 w-4 text-blue-600 border-gray-300 rounded"
                           checked={newTable.isActive}
                           onChange={e => setNewTable({...newTable, isActive: e.target.checked})}
                         />
                         <span className="font-medium text-gray-700">Đang hoạt động</span>
                      </label>
                   </div>
                </div>

                <div className="p-4 bg-gray-50 border-t flex justify-end space-x-3">
                   <button
                     type="button"
                     onClick={() => setIsModalOpen(false)}
                     className="px-6 py-2 border rounded font-bold text-gray-600 hover:bg-white transition-colors"
                   >
                     Bỏ qua
                   </button>
                   <button
                     type="submit"
                     className="px-6 py-2 bg-[#0070f4] text-white rounded font-bold shadow-lg hover:bg-blue-700 active:scale-95 transition-all"
                   >
                     {editingTable ? 'Cập nhật' : 'Lưu bàn'}
                   </button>
                </div>
              </form>
           </div>
        </div>
      )}
    </div>
  );
};

export default TableManagement;
