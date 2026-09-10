import React, { useState, useRef, useEffect } from 'react';
import { Camera, Clock, CheckCircle2, AlertCircle, LogIn, LogOut, Loader2, MapPin, QrCode } from 'lucide-react';
import { API_URL } from '../../../config';
import jsQR from 'jsqr';

const EmployeeAttendance = () => {
  const [stream, setStream] = useState<MediaStream | null>(null);
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<'idle' | 'success' | 'error' | 'scanning'>('idle');
  const [lastAction, setLastAction] = useState<{ type: string, time: string } | null>(null);
  const [todayLogs, setTodayLogs] = useState<any[]>([]);
  const [qrVerified, setQrVerified] = useState(false);
  const [qrPayload, setQrPayload] = useState<string | null>(null);
  const [qrError, setQrError] = useState<string | null>(null);

  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const requestRef = useRef<number>();

  useEffect(() => {
    const empId = localStorage.getItem('employeeId');
    const userRole = localStorage.getItem('userRole');

    if (userRole === 'admin') {
       setQrError("Tài khoản Quản trị không thể thực hiện chấm công. Vui lòng dùng tài khoản Nhân viên.");
    } else if (!empId) {
       setQrError("Phiên làm việc hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại!");
    }

    startCamera();
    fetchTodayLogs();

    return () => {
      // Dọn dẹp animation frame
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
      stopCamera();
    };
  }, []);

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      const img = new Image();
      img.onload = () => {
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        if (context) {
          canvas.width = img.width;
          canvas.height = img.height;
          context.drawImage(img, 0, 0);
          const imageData = context.getImageData(0, 0, canvas.width, canvas.height);

          const code = jsQR(imageData.data, imageData.width, imageData.height, { inversionAttempts: 'attemptBoth' });
          if (code) processQrCode(code.data);
          else { setQrError("Không tìm thấy mã QR trong ảnh này!"); setQrVerified(false); }
        }
      };
      img.src = event.target?.result as string;
    };
    reader.readAsDataURL(file);
  };

  const processQrCode = (qrData: string) => {
    try {
      const branchId = localStorage.getItem('selectedBranchId');
      if (!branchId) {
        setQrError("Lỗi hệ thống: Không tìm thấy ID chi nhánh trên máy này.");
        return;
      }

      const scannedData = JSON.parse(qrData);

      if (scannedData.type === 'ATTENDANCE_POINT' &&
          scannedData.branchId?.toLowerCase() === branchId.toLowerCase()) {
        setQrVerified(true);
        setQrPayload(qrData);
        setQrError(null);
        setStatus('idle');
        if (navigator.vibrate) navigator.vibrate([100, 50, 100]);
      } else {
        setQrVerified(false);
        setQrError(`Mã không khớp chi nhánh! (Yêu cầu: ${localStorage.getItem('selectedBranchName') || 'Cơ sở hiện tại'})`);
      }
    } catch (e) {
      setQrVerified(false);
      setQrError("Mã QR không hợp lệ hoặc không thuộc hệ thống nhà hàng.");
    }
  };

  const startCamera = async () => {
    try {
      stopCamera();
      if (!navigator.mediaDevices?.getUserMedia) throw new Error('unsupported');
      let mediaStream: MediaStream;
      try {
        mediaStream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' } } });
      } catch {
        mediaStream = await navigator.mediaDevices.getUserMedia({ video: true });
      }
      setStream(mediaStream);

      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream;
        videoRef.current.setAttribute("playsinline", "true");

        // Chờ video sẵn sàng rồi mới play để tránh AbortError
        videoRef.current.onloadedmetadata = () => {
          videoRef.current?.play().then(() => {
            requestRef.current = requestAnimationFrame(scanQRCode);
          }).catch(e => {
            if (e.name !== 'AbortError') {
              console.error("Lỗi phát video:", e);
            }
          });
        };
      }
    } catch (err) {
      console.error("Không thể truy cập camera:", err);
      setQrError("Không thể truy cập camera. Vui lòng cấp quyền Camera cho trình duyệt và thử lại.");
      setStatus('error');
    }
  };

  const stopCamera = () => {
    if (requestRef.current) cancelAnimationFrame(requestRef.current);
    requestRef.current = undefined;
    setStream(currentStream => {
      currentStream?.getTracks().forEach(track => track.stop());
      return null;
    });
    if (videoRef.current) videoRef.current.srcObject = null;
  };

  const scanQRCode = () => {
    if (videoRef.current && videoRef.current.readyState === videoRef.current.HAVE_ENOUGH_DATA) {
      const canvas = canvasRef.current;
      if (canvas) {
        const context = canvas.getContext("2d", { willReadFrequently: true });
        if (context) {
          canvas.height = videoRef.current.videoHeight;
          canvas.width = videoRef.current.videoWidth;

          // Vẽ ảnh từ video lên canvas
          context.drawImage(videoRef.current, 0, 0, canvas.width, canvas.height);

          const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
          const code = jsQR(imageData.data, imageData.width, imageData.height, { inversionAttempts: 'attemptBoth' });
          const value = code?.data;
          if (value) {
            console.log("Đã tìm thấy mã:", value);

            // Hiệu ứng nháy sáng khi bắt được mã
            if (videoRef.current) {
                videoRef.current.style.filter = "brightness(1.5)";
                setTimeout(() => { if(videoRef.current) videoRef.current.style.filter = "none"; }, 100);
            }

            processQrCode(value);
          }
        }
      }
    }
    requestRef.current = requestAnimationFrame(scanQRCode);
  };

  const fetchTodayLogs = async () => {
    try {
      const empId = localStorage.getItem('employeeId');
      if (!empId) return;

      const response = await fetch(`${API_URL}/api/Attendance?employeeId=${empId}&date=${new Date().toISOString()}`);
      const data = await response.json();
      setTodayLogs(data);
    } catch (err) {
      console.error("Lỗi lấy lịch sử chấm công:", err);
    }
  };

  const handleAction = async (type: 'check-in' | 'check-out') => {
    if (!qrVerified) {
      setQrError("Vui lòng quét mã QR Điểm danh tại quầy trước!");
      return;
    }

    const empId = localStorage.getItem('employeeId');
    const empName = localStorage.getItem('userName');
    const branchId = localStorage.getItem('selectedBranchId');

    // Kiểm tra dữ liệu hợp lệ trước khi gửi
    if (!empId || empId === 'undefined' || empId === 'null') {
      alert("Lỗi: Không tìm thấy ID nhân viên. Vui lòng đăng nhập lại!");
      return;
    }
    if (!branchId || branchId === 'undefined' || branchId === 'null') {
      alert("Lỗi: Không tìm thấy ID chi nhánh. Vui lòng đăng nhập lại!");
      return;
    }

    setLoading(true);
    try {
      if (type === 'check-in') {
        const response = await fetch(`${API_URL}/api/Attendance/check-in`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            qrPayload
          })
        });

        if (!response.ok) {
          const errData = await response.json();
          throw new Error(errData.message || "Lỗi hệ thống khi check-in");
        }

        setStatus('success');
        setLastAction({ type: 'VÀO CA', time: new Date().toLocaleTimeString('vi-VN') });
        setQrVerified(false);
        setQrPayload(null);
      } else {
        const lastIn = todayLogs.find(l => !l.checkOutTime);
        if (!lastIn) {
          alert("Bạn chưa check-in hoặc đã check-out rồi!");
          setLoading(false);
          return;
        }
        const response = await fetch(`${API_URL}/api/Attendance/${lastIn.id}/check-out`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ qrPayload })
        });

        if (!response.ok) throw new Error("Lỗi hệ thống khi check-out");

        setStatus('success');
        setLastAction({ type: 'RA CA', time: new Date().toLocaleTimeString('vi-VN') });
        setQrVerified(false);
        setQrPayload(null);
      }

      await fetchTodayLogs();
      setTimeout(() => setStatus('idle'), 3000);
    } catch (err: any) {
      alert(err.message || "Lỗi kết nối máy chủ");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#f0f2f5] p-6 flex flex-col items-center">
      <canvas ref={canvasRef} className="hidden" />
      <div className="max-w-md w-full space-y-6">
        <div className="text-center">
          <h1 className="text-2xl font-black text-gray-800 uppercase tracking-tighter">Chấm công nhân sự</h1>
          <p className="text-gray-500 text-sm">Quét mã QR tại quầy và nhận diện mặt để {todayLogs.some(l=>!l.checkOutTime) ? 'check-out' : 'check-in'}</p>
        </div>

        {/* Camera Feed */}
        <div className="relative aspect-square bg-black rounded-3xl overflow-hidden shadow-2xl border-4 border-white">
          <video
            ref={videoRef}
            autoPlay
            playsInline
            className={`w-full h-full object-cover scale-x-[-1] ${status === 'success' ? 'opacity-20' : 'opacity-100'}`}
          />

          <div className="absolute inset-0 border-[40px] border-black/20 pointer-events-none">
             <div className={`w-full h-full border-2 rounded-xl relative transition-colors ${qrVerified ? 'border-green-500 shadow-[0_0_20px_rgba(34,197,94,0.5)]' : 'border-blue-500/50'}`}>
                <div className={`absolute top-0 left-0 w-8 h-8 border-t-4 border-l-4 ${qrVerified ? 'border-green-500' : 'border-blue-500'}`}></div>
                <div className={`absolute top-0 right-0 w-8 h-8 border-t-4 border-r-4 ${qrVerified ? 'border-green-500' : 'border-blue-500'}`}></div>
                <div className={`absolute bottom-0 left-0 w-8 h-8 border-b-4 border-l-4 ${qrVerified ? 'border-green-500' : 'border-blue-500'}`}></div>
                <div className={`absolute bottom-0 right-0 w-8 h-8 border-b-4 border-r-4 ${qrVerified ? 'border-green-500' : 'border-blue-500'}`}></div>

                {!qrVerified && (
                   <div className="absolute top-0 left-0 w-full h-1 bg-blue-500 shadow-[0_0_15px_blue] animate-scan"></div>
                )}
             </div>
          </div>

          {qrVerified && status === 'idle' && (
             <div className="absolute top-4 left-1/2 -translate-x-1/2 bg-green-600 text-white px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest flex items-center shadow-lg animate-bounce">
                <QrCode size={14} className="mr-2"/> Đã xác thực QR
             </div>
          )}

          {qrError && !qrVerified && (
             <div className="absolute bottom-4 left-4 right-4 bg-red-600/90 text-white p-3 rounded-2xl text-[10px] font-bold text-center animate-in fade-in slide-in-from-bottom-2">
                <AlertCircle size={16} className="mx-auto mb-1"/> {qrError}
             </div>
          )}

          {status === 'error' && (
            <div className="absolute inset-0 bg-gray-900 flex flex-col items-center justify-center text-white p-6 text-center">
               <AlertCircle size={48} className="text-red-500 mb-4" />
               <p className="font-bold uppercase tracking-tighter">Lỗi Truy Cập Camera</p>
               <p className="text-[10px] text-gray-400 mt-2 italic">Vui lòng cấp quyền truy cập camera và sử dụng kết nối bảo mật (HTTPS/localhost) để thực hiện quét mã QR.</p>
               <button onClick={startCamera} className="mt-4 px-4 py-2 bg-blue-600 rounded-xl text-[10px] font-bold uppercase tracking-widest">Thử lại</button>
            </div>
          )}

          {status === 'success' && (
            <div className="absolute inset-0 bg-green-600/90 flex flex-col items-center justify-center text-white animate-in zoom-in-95 duration-300">
               <CheckCircle2 size={80} className="mb-4" />
               <p className="text-2xl font-black uppercase tracking-widest">{lastAction?.type} THÀNH CÔNG</p>
               <p className="text-lg opacity-80">{lastAction?.time}</p>
            </div>
          )}
        </div>

        <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100 space-y-6">
           <div className="grid grid-cols-2 gap-4">
              <button
                disabled={loading || status === 'success'}
                onClick={() => handleAction('check-in')}
                className={`flex flex-col items-center justify-center py-4 rounded-2xl font-bold transition-all shadow-lg active:scale-95 disabled:opacity-30 ${qrVerified ? 'bg-green-600 text-white hover:bg-green-700 shadow-green-500/20' : 'bg-green-600/40 text-white/70'}`}
              >
                 <LogIn size={24} className="mb-1" />
                 <span>VÀO CA</span>
              </button>
              <button
                disabled={loading || status === 'success'}
                onClick={() => handleAction('check-out')}
                className={`flex flex-col items-center justify-center py-4 rounded-2xl font-bold transition-all shadow-lg active:scale-95 disabled:opacity-30 ${qrVerified ? 'bg-[#0070f4] text-white hover:bg-blue-700 shadow-blue-500/20' : 'bg-blue-600/40 text-white/70'}`}
              >
                 <LogOut size={24} className="mb-1" />
                 <span>RA CA</span>
              </button>
           </div>

           {!qrVerified && (
              <div className="bg-blue-50 p-4 rounded-xl border border-blue-100 space-y-3">
                 <div className="flex items-center">
                    <QrCode size={20} className="text-blue-500 mr-3 shrink-0" />
                    <p className="text-[11px] text-blue-700 font-medium leading-relaxed italic">
                        Vui lòng đưa mã QR Điểm danh vào Camera hoặc tải ảnh lên để kích hoạt nút chấm công.
                    </p>
                 </div>

                 <div className="pt-2 border-t border-blue-200/50">
                    <input
                      type="file"
                      ref={fileInputRef}
                      className="hidden"
                      accept="image/*"
                      onChange={handleFileUpload}
                    />
                    <button
                      onClick={() => fileInputRef.current?.click()}
                      className="w-full py-2 bg-white text-blue-600 border border-blue-300 rounded-lg text-[10px] font-black uppercase tracking-widest hover:bg-blue-50 transition-all flex items-center justify-center"
                    >
                       <Camera size={14} className="mr-2" /> Tải ảnh QR từ thư viện
                    </button>
                 </div>
              </div>
           )}
        </div>

        <div className="text-center">
           <p className="text-gray-400 text-[10px] font-bold uppercase tracking-widest flex items-center justify-center italic">
              <Clock size={12} className="mr-1" /> Lịch sử hôm nay
           </p>
           <div className="mt-2 text-[11px] font-medium text-gray-500 space-y-1">
              {todayLogs.length === 0 ? "Chưa có dữ liệu hôm nay" : todayLogs.map((log, i) => (
                <div key={i}>
                   {new Date(log.checkInTime).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'})} -
                   {log.checkOutTime ? new Date(log.checkOutTime).toLocaleTimeString('vi-VN', {hour:'2-digit', minute:'2-digit'}) : " Đang làm"}
                   ({log.status})
                </div>
              ))}
           </div>
        </div>
      </div>

      <style dangerouslySetInnerHTML={{ __html: `
        @keyframes scan { from { top: 0; } to { top: 100%; } }
        .animate-scan { animation: scan 2s linear infinite; }
      `}} />
    </div>
  );
};

export default EmployeeAttendance;

