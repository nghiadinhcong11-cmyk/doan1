import React, { useState } from 'react';
import { HelpCircle, Book, MessageSquare, ChevronRight, Search, Play, CheckCircle2, ShieldCheck, Printer, QrCode } from 'lucide-react';

const SupportPage = () => {
  const [activeTab, setActiveTab] = useState<'manual' | 'faq'>('manual');
  const [searchTerm, setSearchTerm] = useState('');

  const faqs = [
    {
      q: "Làm thế nào để kết nối máy in hóa đơn?",
      a: "Bạn vào mục 'Thiết lập' -> 'Mẫu in'. Đảm bảo máy in đã được cài đặt Driver trên máy tính. Khi bấm 'In thử', trình duyệt sẽ hiện hộp thoại chọn máy in, bạn chọn đúng tên máy in K80 hoặc A5 của mình."
    },
    {
      q: "Nhân viên quên chấm công thì xử lý thế nào?",
      a: "Quản trị viên có thể vào mục 'Nhân viên' -> 'Chấm công', tìm đến ngày thiếu sót và nhấn nút 'Bổ sung công' để nhập tay giờ vào/ra cho nhân viên đó."
    },
    {
      q: "Tại sao mã QR thanh toán không hiển thị số tiền?",
      a: "Bạn cần kiểm tra lại trong 'Quản lý chi nhánh' xem đã nhập đúng Số tài khoản và Tên ngân hàng chưa. Ngoài ra, hãy đảm bảo mục 'QR động' trong phần 'Thiết lập hệ thống' đang được Bật."
    },
    {
      q: "Dữ liệu giữa máy tính và điện thoại khách có đồng bộ không?",
      a: "Có, hệ thống sử dụng công nghệ Cloud. Ngay khi khách đặt món trên điện thoại, máy tính thu ngân sẽ nhận được thông báo chuông và hiển thị đơn hàng ngay lập tức."
    }
  ];

  const manualSteps = [
    {
      title: "Bắt đầu bán hàng",
      icon: <Play size={20} className="text-blue-500" />,
      steps: [
        "Đăng nhập vào hệ thống với quyền Thu ngân.",
        "Chọn chi nhánh làm việc (nếu có nhiều cơ sở).",
        "Tại màn hình POS, chọn bàn khách ngồi hoặc chọn 'Mang về'.",
        "Chọn món từ thực đơn bên trái, món sẽ tự nhảy vào giỏ hàng bên phải.",
        "Nhấn 'Thanh toán' và chọn phương thức (Tiền mặt/Chuyển khoản)."
      ]
    },
    {
      title: "Quản lý thực đơn",
      icon: <Book size={20} className="text-orange-500" />,
      steps: [
        "Vào mục 'Thực đơn' trên thanh menu chính.",
        "Nhấn 'Thêm món mới' để nhập tên, giá và hình ảnh sản phẩm.",
        "Bạn có thể cập nhật trạng thái 'Hết hàng' để nhân viên không chọn được món đó tại POS.",
        "Sử dụng bộ lọc loại món để quản lý thực đơn dễ dàng hơn."
      ]
    },
    {
      title: "Thiết lập QR Điểm danh",
      icon: <QrCode size={20} className="text-green-500" />,
      steps: [
        "Vào mục 'Nhân viên' -> 'Chấm công'.",
        "Tại sidebar bên trái, nhấn 'Kích hoạt QR'.",
        "Chọn chi nhánh tương ứng và in mã QR này dán tại quầy.",
        "Nhân viên sử dụng tính năng 'Chấm công' trên điện thoại để quét mã này khi vào ca."
      ]
    }
  ];

  return (
    <div className="p-8 bg-[#f0f2f5] min-h-[calc(100vh-48px)] font-sans">
      <div className="max-w-5xl mx-auto">
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-center justify-between mb-8 gap-4">
          <div>
            <h1 className="text-3xl font-black text-gray-800 uppercase italic tracking-tighter flex items-center">
              <HelpCircle className="mr-3 text-blue-600" size={32} /> Trung tâm hỗ trợ
            </h1>
            <p className="text-gray-500 font-bold uppercase tracking-widest text-[10px] mt-1 italic">Hướng dẫn sử dụng & Giải đáp thắc mắc hệ thống DOAN POS</p>
          </div>
          <div className="relative">
            <Search className="absolute left-4 top-3.5 text-gray-400" size={18} />
            <input
              type="text"
              placeholder="Tìm kiếm vấn đề bạn gặp phải..."
              className="pl-12 pr-6 py-3.5 bg-white border-none rounded-2xl shadow-sm w-full md:w-96 outline-none focus:ring-2 focus:ring-blue-500/20 font-bold text-sm"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
        </div>

        {/* Tab Switcher */}
        <div className="flex space-x-2 mb-8 bg-gray-200/50 p-1.5 rounded-2xl w-fit">
          <button
            onClick={() => setActiveTab('manual')}
            className={`px-8 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest transition-all ${activeTab === 'manual' ? 'bg-white text-blue-600 shadow-md' : 'text-gray-500 hover:text-gray-700'}`}
          >
            Tài liệu hướng dẫn
          </button>
          <button
            onClick={() => setActiveTab('faq')}
            className={`px-8 py-2.5 rounded-xl text-xs font-black uppercase tracking-widest transition-all ${activeTab === 'faq' ? 'bg-white text-blue-600 shadow-md' : 'text-gray-500 hover:text-gray-700'}`}
          >
            Câu hỏi thường gặp
          </button>
        </div>

        {activeTab === 'manual' ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
             {manualSteps.map((section, idx) => (
               <div key={idx} className="bg-white rounded-[2.5rem] p-8 shadow-xl shadow-blue-500/5 border border-white flex flex-col h-full">
                  <div className="w-14 h-14 bg-gray-50 rounded-2xl flex items-center justify-center mb-6 shadow-inner border border-gray-100">
                     {section.icon}
                  </div>
                  <h3 className="text-lg font-black text-gray-800 uppercase tracking-tight mb-6">{section.title}</h3>
                  <div className="space-y-4 flex-1">
                     {section.steps.map((step, sIdx) => (
                       <div key={sIdx} className="flex items-start space-x-3 group">
                          <div className="w-5 h-5 rounded-full bg-blue-50 flex items-center justify-center text-[10px] font-black text-blue-500 shrink-0 mt-0.5 group-hover:bg-blue-500 group-hover:text-white transition-colors">
                             {sIdx + 1}
                          </div>
                          <p className="text-xs font-medium text-gray-500 leading-relaxed group-hover:text-gray-800 transition-colors">{step}</p>
                       </div>
                     ))}
                  </div>
                  <button className="mt-8 pt-4 border-t border-dashed border-gray-100 text-[10px] font-black text-blue-600 uppercase tracking-widest flex items-center hover:underline">
                    Xem chi tiết <ChevronRight size={14} className="ml-1" />
                  </button>
               </div>
             ))}
          </div>
        ) : (
          <div className="space-y-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
             {faqs.filter(f => f.q.toLowerCase().includes(searchTerm.toLowerCase())).map((faq, idx) => (
               <div key={idx} className="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 hover:border-blue-200 transition-all group">
                  <div className="flex items-center space-x-4 mb-3">
                     <div className="p-2 bg-blue-50 text-blue-600 rounded-xl group-hover:bg-blue-600 group-hover:text-white transition-all">
                        <MessageSquare size={18} />
                     </div>
                     <h4 className="font-black text-gray-800 uppercase tracking-tight text-sm">{faq.q}</h4>
                  </div>
                  <p className="text-xs text-gray-500 leading-relaxed font-medium italic ml-12">
                     {faq.a}
                  </p>
               </div>
             ))}
          </div>
        )}

        {/* Footer Support */}
        <div className="mt-12 bg-blue-600 rounded-[3rem] p-10 text-white relative overflow-hidden shadow-2xl">
           <div className="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full -mr-20 -mt-20 blur-3xl"></div>
           <div className="relative z-10 flex flex-col md:flex-row items-center justify-between gap-8">
              <div className="text-center md:text-left">
                 <h3 className="text-2xl font-black uppercase italic tracking-tighter mb-2">Bạn vẫn cần trợ giúp?</h3>
                 <p className="text-sm opacity-80 font-medium">Đội ngũ kỹ thuật của chúng tôi luôn sẵn sàng hỗ trợ bạn 24/7 qua Hotline hoặc Zalo.</p>
              </div>
              <div className="flex flex-col sm:flex-row gap-4">
                 <div className="bg-white/20 backdrop-blur-md px-8 py-4 rounded-3xl border border-white/20 text-center">
                    <p className="text-[10px] font-black uppercase tracking-widest opacity-70 mb-1">Hotline hỗ trợ</p>
                    <p className="text-xl font-black tracking-tighter italic">1900.88.99.00</p>
                 </div>
                 <button className="bg-white text-blue-600 px-10 py-4 rounded-3xl font-black uppercase tracking-widest text-xs shadow-xl hover:bg-gray-50 active:scale-95 transition-all">
                    Chát với tư vấn viên
                 </button>
              </div>
           </div>
        </div>
      </div>
    </div>
  );
};

export default SupportPage;

