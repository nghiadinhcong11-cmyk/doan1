import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { QrCode, Camera, X, CheckCircle2, AlertCircle } from 'lucide-react';

const QRScan = () => {
  const [stream, setStream] = useState<MediaStream | null>(null);
  const [error, setError] = useState<string | null>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const requestRef = useRef<number>();
  const navigate = useNavigate();

  useEffect(() => {
    startCamera();
    return () => {
      if (stream) {
        stream.getTracks().forEach(track => track.stop());
      }
      if (requestRef.current) {
        cancelAnimationFrame(requestRef.current);
      }
    };
  }, []);

  const startCamera = async () => {
    try {
      const mediaStream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      });
      setStream(mediaStream);
      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream;
        videoRef.current.setAttribute("playsinline", "true");
        videoRef.current.play().catch(e => console.error("Video play error:", e));
        requestRef.current = requestAnimationFrame(scanQRCode);
      }
    } catch (err) {
      console.error("Camera access error:", err);
      setError("Không thể truy cập camera. Vui lòng cấp quyền!");
    }
  };

  const scanQRCode = () => {
    if (videoRef.current && videoRef.current.readyState === videoRef.current.HAVE_ENOUGH_DATA) {
      const canvas = canvasRef.current;
      if (canvas) {
        const context = canvas.getContext("2d", { willReadFrequently: true });
        if (context) {
          canvas.height = videoRef.current.videoHeight;
          canvas.width = videoRef.current.videoWidth;
          context.drawImage(videoRef.current, 0, 0, canvas.width, canvas.height);
          const imageData = context.getImageData(0, 0, canvas.width, canvas.height);

          // @ts-ignore
          if (window.jsQR) {
            // @ts-ignore
            const code = window.jsQR(imageData.data, imageData.width, imageData.height);
            if (code) {
              console.log("QR Data:", code.data);
              // Kiểm tra nếu là URL chứa tableId hoặc là ID bàn trực tiếp
              try {
                if (code.data.includes('qr=')) {
                  const url = new URL(code.data);
                  const qr = url.searchParams.get('qr');
                  if (qr && /^[A-Za-z0-9_-]{43}$/.test(qr)) navigate(`/?qr=${encodeURIComponent(qr)}`);
                } else if (/^[A-Za-z0-9_-]{43}$/.test(code.data)) {
                  // Giả sử mã QR là một ID GUID của bàn
                  navigate(`/?qr=${encodeURIComponent(code.data)}`);
                }
              } catch (e) {
                console.error("Invalid QR data format");
              }
            }
          }
        }
      }
    }
    requestRef.current = requestAnimationFrame(scanQRCode);
  };

  return (
    <div className="min-h-screen bg-black text-white flex flex-col items-center justify-center p-8 pb-32">
       <canvas ref={canvasRef} className="hidden" />

       <div className="text-center mb-10">
          <h1 className="text-2xl font-black uppercase italic tracking-tighter text-blue-500">Quét mã QR tại bàn</h1>
          <p className="text-gray-400 text-xs mt-2 font-bold uppercase tracking-widest">Căn chỉnh mã QR vào khung hình bên dưới</p>
       </div>

       {/* Camera Viewport */}
       <div className="relative w-full aspect-square max-w-xs rounded-[3rem] overflow-hidden border-4 border-blue-600/30 shadow-[0_0_50px_rgba(37,99,235,0.2)]">
          <video
            ref={videoRef}
            autoPlay
            playsInline
            className="w-full h-full object-cover"
          />

          <div className="absolute inset-0 bg-gradient-to-b from-transparent via-blue-500/20 to-transparent animate-scan-line"></div>

          {/* Corner Borders */}
          <div className="absolute top-8 left-8 w-12 h-12 border-t-4 border-l-4 border-blue-500 rounded-tl-2xl"></div>
          <div className="absolute top-8 right-8 w-12 h-12 border-t-4 border-r-4 border-blue-500 rounded-tr-2xl"></div>
          <div className="absolute bottom-8 left-8 w-12 h-12 border-b-4 border-l-4 border-blue-500 rounded-bl-2xl"></div>
          <div className="absolute bottom-8 right-8 w-12 h-12 border-b-4 border-r-4 border-blue-500 rounded-br-2xl"></div>

          {error && (
            <div className="absolute inset-0 bg-gray-900/90 flex flex-col items-center justify-center p-6 text-center">
               <AlertCircle size={48} className="text-red-500 mb-4" />
               <p className="font-bold text-sm uppercase">{error}</p>
               <button onClick={() => window.location.reload()} className="mt-4 px-6 py-2 bg-blue-600 rounded-xl text-xs font-black">THỬ LẠI</button>
            </div>
          )}
       </div>

       <div className="mt-12 flex flex-col items-center space-y-4">
          <div className="p-4 bg-white/5 rounded-2xl flex items-center border border-white/10">
             <QrCode size={20} className="text-blue-500 mr-3" />
             <p className="text-[10px] text-gray-400 font-medium leading-relaxed italic max-w-[200px]">
                Hệ thống sẽ tự động nhận diện bàn và chi nhánh sau khi quét thành công.
             </p>
          </div>
       </div>

       <style dangerouslySetInnerHTML={{ __html: `
          @keyframes scan-line {
            0% { transform: translateY(-150%); }
            100% { transform: translateY(150%); }
          }
          .animate-scan-line {
            animation: scan-line 2.5s linear infinite;
          }
       `}} />
    </div>
  );
};

export default QRScan;
