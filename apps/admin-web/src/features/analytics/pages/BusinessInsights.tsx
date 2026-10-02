import React, { useState, useEffect } from 'react';
import { BellRing, ChevronRight, AlertCircle, AlertTriangle, Info, CheckCircle, Clock, ExternalLink, Loader2, RefreshCw } from 'lucide-react';
import { API_URL } from '../../../config';

interface BusinessInsight {
  id: string;
  type: string;
  severity: 'Info' | 'Warning' | 'Critical';
  title: string;
  summary: string;
  status: 'Unread' | 'Read' | 'Resolved';
  detectedAt: string;
  createdAt: string;
}

interface BusinessInsightDetail extends BusinessInsight {
  aiExplanation: string | null;
  aiRecommendation: string | null;
  evidence: any;
  periodStart: string;
  periodEnd: string;
  comparisonPeriodStart: string | null;
  comparisonPeriodEnd: string | null;
}

const BusinessInsights: React.FC = () => {
  const [insights, setInsights] = useState<BusinessInsight[]>([]);
  const [selectedInsight, setSelectedInsight] = useState<BusinessInsightDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingDetail, setLoadingDetail] = useState(false);
  const [filter, setFilter] = useState({ status: '', severity: '' });

  const fetchInsights = async () => {
    try {
      setLoading(true);
      const query = new URLSearchParams();
      if (filter.status) query.append('status', filter.status);
      if (filter.severity) query.append('severity', filter.severity);

      const res = await fetch(`${API_URL}/api/BusinessInsight?${query.toString()}`);
      if (res.ok) {
        const data = await res.json();
        setInsights(data.items);
      }
    } catch (err) {
      console.error("Lỗi lấy danh sách cảnh báo:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchInsights();

    // Realtime update when a new insight is created via SignalR (triggered from Navbar)
    const handleCreated = () => {
      fetchInsights();
    };
    window.addEventListener('business-insight-created', handleCreated);
    return () => window.removeEventListener('business-insight-created', handleCreated);
  }, [filter]);

  const fetchDetail = async (id: string) => {
    try {
      setLoadingDetail(true);
      const res = await fetch(`${API_URL}/api/BusinessInsight/${id}`);
      if (res.ok) {
        const data = await res.json();
        setSelectedInsight(data);

        // Auto mark as read
        if (data.status === 'Unread') {
          handleMarkRead(id);
        }
      }
    } catch (err) {
      console.error("Lỗi lấy chi tiết cảnh báo:", err);
    } finally {
      setLoadingDetail(false);
    }
  };

  const handleMarkRead = async (id: string) => {
    try {
      const res = await fetch(`${API_URL}/api/BusinessInsight/${id}/read`, { method: 'POST' });
      if (res.ok) {
        setInsights(prev => prev.map(i => i.id === id ? { ...i, status: 'Read' } : i));
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleResolve = async (id: string) => {
    try {
      const res = await fetch(`${API_URL}/api/BusinessInsight/${id}/resolve`, { method: 'POST' });
      if (res.ok) {
        setInsights(prev => prev.map(i => i.id === id ? { ...i, status: 'Resolved' } : i));
        if (selectedInsight?.id === id) {
          setSelectedInsight(prev => prev ? { ...prev, status: 'Resolved' } : null);
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  const getSeverityIcon = (severity: string) => {
    switch (severity) {
      case 'Critical': return <AlertCircle className="text-red-500" size={18} />;
      case 'Warning': return <AlertTriangle className="text-orange-500" size={18} />;
      default: return <Info className="text-blue-500" size={18} />;
    }
  };

  const getSeverityBadge = (severity: string) => {
    switch (severity) {
      case 'Critical': return <span className="px-2 py-0.5 bg-red-100 text-red-700 text-[10px] font-bold rounded uppercase">Nghiêm trọng</span>;
      case 'Warning': return <span className="px-2 py-0.5 bg-orange-100 text-orange-700 text-[10px] font-bold rounded uppercase">Cảnh báo</span>;
      default: return <span className="px-2 py-0.5 bg-blue-100 text-blue-700 text-[10px] font-bold rounded uppercase">Thông tin</span>;
    }
  };

  return (
    <div className="p-4 md:p-6 max-w-7xl mx-auto">
      <div className="flex flex-col md:flex-row md:items-center justify-between mb-6 gap-4">
        <div>
          <h1 className="text-2xl font-black text-gray-800 flex items-center gap-2 uppercase tracking-tighter italic">
            <BellRing className="text-blue-600" />
            Cảnh báo kinh doanh thông minh
          </h1>
          <p className="text-gray-500 text-sm font-medium uppercase italic">Phát hiện bất thường và nhận định từ AI</p>
        </div>

        <div className="flex items-center gap-2">
          <select
            value={filter.status}
            onChange={(e) => setFilter(f => ({ ...f, status: e.target.value }))}
            className="bg-white border border-gray-200 rounded-lg px-3 py-2 text-xs font-bold uppercase text-gray-600 outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">Tất cả trạng thái</option>
            <option value="Unread">Chưa đọc</option>
            <option value="Read">Đã đọc</option>
            <option value="Resolved">Đã xử lý</option>
          </select>

          <button
            onClick={fetchInsights}
            className="p-2 bg-gray-100 hover:bg-gray-200 rounded-lg text-gray-600 transition-colors"
            title="Làm mới"
          >
            <RefreshCw size={18} />
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 h-[calc(100vh-200px)]">
        {/* List */}
        <div className="lg:col-span-5 bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden flex flex-col">
          <div className="p-4 border-b bg-gray-50/50 flex items-center justify-between">
            <span className="text-[10px] font-black text-gray-400 uppercase tracking-widest">Danh sách cảnh báo</span>
            <span className="text-[10px] font-black text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full uppercase">{insights.length} Mục</span>
          </div>

          <div className="flex-1 overflow-y-auto">
            {loading ? (
              <div className="flex flex-col items-center justify-center h-full text-gray-400">
                <Loader2 className="animate-spin mb-2" size={32} />
                <p className="text-xs font-bold uppercase italic">Đang tải dữ liệu...</p>
              </div>
            ) : insights.length === 0 ? (
              <div className="flex flex-col items-center justify-center h-full text-gray-400 p-8 text-center">
                <CheckCircle size={48} className="mb-4 text-green-500/30" />
                <p className="text-sm font-black uppercase tracking-tighter italic">Tuyệt vời! Không có cảnh báo bất thường nào.</p>
              </div>
            ) : (
              insights.map(insight => (
                <button
                  key={insight.id}
                  onClick={() => fetchDetail(insight.id)}
                  className={`w-full text-left p-4 border-b border-gray-50 hover:bg-blue-50/30 transition-all group relative ${selectedInsight?.id === insight.id ? 'bg-blue-50/50 border-l-4 border-l-blue-600' : ''}`}
                >
                  <div className="flex justify-between items-start mb-1">
                    <div className="flex items-center gap-2">
                      {getSeverityIcon(insight.severity)}
                      <h3 className={`text-sm font-black uppercase tracking-tight ${insight.status === 'Unread' ? 'text-gray-900' : 'text-gray-500'}`}>
                        {insight.title}
                      </h3>
                    </div>
                    {insight.status === 'Unread' && (
                      <span className="w-2 h-2 bg-blue-600 rounded-full animate-pulse shadow-md shadow-blue-500/50"></span>
                    )}
                  </div>
                  <p className={`text-xs mb-2 line-clamp-2 ${insight.status === 'Unread' ? 'text-gray-600 font-medium' : 'text-gray-400 font-normal'}`}>
                    {insight.summary}
                  </p>
                  <div className="flex items-center justify-between text-[10px] font-bold uppercase text-gray-400">
                    <div className="flex items-center gap-1 italic">
                      <Clock size={10} />
                      {new Date(insight.detectedAt).toLocaleDateString('vi-VN')} {new Date(insight.detectedAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                    </div>
                    {insight.status === 'Resolved' && (
                      <span className="text-green-600 flex items-center gap-1"><CheckCircle size={10} /> Đã xử lý</span>
                    )}
                  </div>
                </button>
              ))
            )}
          </div>
        </div>

        {/* Detail */}
        <div className="lg:col-span-7 bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden flex flex-col">
          {loadingDetail ? (
            <div className="flex flex-col items-center justify-center h-full text-gray-400">
              <Loader2 className="animate-spin mb-2" size={32} />
              <p className="text-xs font-bold uppercase italic text-blue-600">AI đang chuẩn bị nhận định...</p>
            </div>
          ) : !selectedInsight ? (
            <div className="flex flex-col items-center justify-center h-full text-gray-400 p-8 text-center bg-gray-50/30">
              <ExternalLink size={48} className="mb-4 opacity-10" />
              <p className="text-sm font-bold uppercase tracking-widest italic">Chọn một cảnh báo để xem chi tiết nhận định từ AI</p>
            </div>
          ) : (
            <div className="flex-1 overflow-y-auto p-6 animate-in fade-in slide-in-from-right-4 duration-300">
              <div className="flex items-center justify-between mb-6">
                {getSeverityBadge(selectedInsight.severity)}
                <div className="flex items-center gap-2">
                   {selectedInsight.status !== 'Resolved' && (
                     <button
                       onClick={() => handleResolve(selectedInsight.id)}
                       className="px-4 py-2 bg-green-600 hover:bg-green-700 text-white text-xs font-black uppercase tracking-widest rounded-xl transition-all shadow-lg shadow-green-500/20"
                     >
                       Đánh dấu đã xử lý
                     </button>
                   )}
                   {selectedInsight.status === 'Resolved' && (
                     <span className="px-4 py-2 bg-gray-100 text-gray-400 text-xs font-black uppercase tracking-widest rounded-xl flex items-center gap-2">
                       <CheckCircle size={14} /> Đã xử lý thành công
                     </span>
                   )}
                </div>
              </div>

              <h2 className="text-xl font-black text-gray-800 uppercase tracking-tighter mb-2 italic">{selectedInsight.title}</h2>
              <p className="text-gray-600 font-medium border-l-4 border-gray-200 pl-4 py-1 mb-8">{selectedInsight.summary}</p>

              <div className="space-y-8">
                {/* AI Explanation */}
                <section>
                  <div className="flex items-center gap-2 mb-3">
                    <div className="p-1.5 bg-blue-100 text-blue-600 rounded-lg"><Info size={16} /></div>
                    <h3 className="text-sm font-black text-gray-700 uppercase tracking-widest">Phân tích chuyên sâu từ AI</h3>
                  </div>
                  <div className="bg-blue-50/50 rounded-2xl p-5 text-sm text-gray-700 leading-relaxed border border-blue-100/50 italic whitespace-pre-wrap">
                    {selectedInsight.aiExplanation || "Đang cập nhật nhận định từ AI..."}
                  </div>
                </section>

                {/* AI Recommendation */}
                <section>
                  <div className="flex items-center gap-2 mb-3">
                    <div className="p-1.5 bg-green-100 text-green-600 rounded-lg"><Clock size={16} /></div>
                    <h3 className="text-sm font-black text-gray-700 uppercase tracking-widest">Hành động khuyến nghị</h3>
                  </div>
                  <div className="bg-green-50/30 rounded-2xl p-5 text-sm text-gray-700 leading-relaxed border border-green-100/50 whitespace-pre-wrap">
                    {selectedInsight.aiRecommendation || "Đang chuẩn bị khuyến nghị phù hợp..."}
                  </div>
                </section>

                {/* Evidence Snapshot */}
                <section>
                  <div className="flex items-center gap-2 mb-3">
                    <div className="p-1.5 bg-gray-100 text-gray-600 rounded-lg"><ExternalLink size={16} /></div>
                    <h3 className="text-sm font-black text-gray-700 uppercase tracking-widest">Bằng chứng dữ liệu (Snapshot)</h3>
                  </div>
                  <div className="bg-gray-50 rounded-2xl p-5 border border-gray-100">
                    <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                      {selectedInsight.evidence?.Current && (
                        <>
                          <div className="bg-white p-3 rounded-xl border border-gray-100 shadow-sm">
                            <p className="text-[10px] font-bold text-gray-400 uppercase leading-none mb-1">Doanh thu</p>
                            <p className="text-sm font-black text-gray-800">{selectedInsight.evidence.Current.Revenue.toLocaleString()}đ</p>
                          </div>
                          <div className="bg-white p-3 rounded-xl border border-gray-100 shadow-sm">
                            <p className="text-[10px] font-bold text-gray-400 uppercase leading-none mb-1">Số đơn hàng</p>
                            <p className="text-sm font-black text-gray-800">{selectedInsight.evidence.Current.OrderCount}</p>
                          </div>
                          <div className="bg-white p-3 rounded-xl border border-gray-100 shadow-sm">
                            <p className="text-[10px] font-bold text-gray-400 uppercase leading-none mb-1">AOV</p>
                            <p className="text-sm font-black text-gray-800">{selectedInsight.evidence.Current.AverageOrderValue.toLocaleString()}đ</p>
                          </div>
                          <div className="bg-white p-3 rounded-xl border border-gray-100 shadow-sm">
                            <p className="text-[10px] font-bold text-gray-400 uppercase leading-none mb-1">Lợi nhuận ước tính</p>
                            <p className="text-sm font-black text-blue-600">{selectedInsight.evidence.Current.NetProfit.toLocaleString()}đ</p>
                          </div>
                          <div className="bg-white p-3 rounded-xl border border-gray-100 shadow-sm">
                            <p className="text-[10px] font-bold text-gray-400 uppercase leading-none mb-1">Tăng trưởng DT</p>
                            <p className={`text-sm font-black ${selectedInsight.evidence.Growth?.RevenueGrowthPercent < 0 ? 'text-red-500' : 'text-green-500'}`}>
                              {selectedInsight.evidence.Growth?.RevenueGrowthPercent}%
                            </p>
                          </div>
                        </>
                      )}
                    </div>
                  </div>
                </section>

                <p className="text-[10px] text-gray-400 font-bold uppercase italic text-center py-4 border-t border-dashed">
                  Cảnh báo được tạo vào {new Date(selectedInsight.createdAt).toLocaleString('vi-VN')} • Dữ liệu tài chính mang tính chất tham khảo
                </p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default BusinessInsights;
