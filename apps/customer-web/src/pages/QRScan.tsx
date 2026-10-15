import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { QrCode, Camera, X, CheckCircle2, AlertCircle } from 'lucide-react';

const QRScan = () => {
  const [stream, setStream] = useState<MediaStream | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [canRetryCamera, setCanRetryCamera] = useState(false);
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const requestRef = useRef<number>();
  const streamRef = useRef<MediaStream | null>(null);
  const navigate = useNavigate();

  const stopCamera = useCallback(() => {
    if (streamRef.current) {
      streamRef.current.getTracks().forEach(track => track.stop());
      streamRef.current = null;
    }
    setStream(null);
    if (requestRef.current) {
      cancelAnimationFrame(requestRef.current);
      requestRef.current = undefined;
    }
  }, []);

  const startCamera = useCallback(async () => {
    stopCamera();
    setError(null);
    setCanRetryCamera(false);

    if (!window.isSecureContext) {
      setError('Camera chỉ hoạt động khi Customer Web được mở bằng HTTPS. Hãy dùng địa chỉ https:// của mạng phát triển.');
      return;
    }

    if (!navigator.mediaDevices || typeof navigator.mediaDevices.getUserMedia !== 'function') {
      setError('Trình duyệt hoặc thiết bị không hỗ trợ camera. Hãy thử trình duyệt hiện đại trên thiết bị có camera.');
      return;
    }

    try {
      const mediaStream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'environment' }
      });
      streamRef.current = mediaStream;
      setStream(mediaStream);
      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream;
        videoRef.current.setAttribute('playsinline', 'true');
        await videoRef.current.play();
        requestRef.current = requestAnimationFrame(scanQRCode);
      }
    } catch (err) {
      console.error('Camera access error:', err);
      const cameraError = err as DOMException;
      if (cameraError.name === 'NotAllowedError' || cameraError.name === 'SecurityError') {
        setError('Bạn đã từ chối quyền camera. Hãy cho phép camera trong cài đặt trình duyệt rồi thử lại.');
      } else if (cameraError.name === 'NotFoundError' || cameraError.name === 'DevicesNotFoundError') {
        setError('Không tìm thấy camera trên thiết bị này.');
      } else if (cameraError.name === 'NotReadableError' || cameraError.name === 'TrackStartError') {
        setError('Camera đang được ứng dụng khác sử dụng. Hãy đóng ứng dụng đó rồi thử lại.');
      } else {
        setError('Không thể khởi tạo camera. Hãy kiểm tra quyền truy cập và thử lại.');
      }
      setCanRetryCamera(true);
    }
  }, [stopCamera]);

  useEffect(() => {
    void startCamera();
    return () => {
      stopCamera();
    };
  }, [startCamera, stopCamera]);

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
    <div className="flex min-h-[100dvh] flex-col items-center justify-center overflow-x-hidden bg-[#22252b] p-6 pb-[calc(8rem+env(safe-area-inset-bottom))] text-white sm:p-8">
       <canvas ref={canvasRef} className="hidden" />

       <div className="text-center mb-10">
          <h1 className="text-2xl font-black tracking-tight text-orange-400">Quét mã QR tại bàn</h1>
          <p className="mt-2 text-xs font-bold uppercase tracking-widest text-gray-400">Căn chỉnh mã QR vào khung hình bên dưới</p>
       </div>

       {/* Camera Viewport */}
       <div className="relative aspect-square w-full max-w-xs overflow-hidden rounded-[2.5rem] border-4 border-orange-400/40 shadow-[0_0_50px_rgba(238,108,54,.22)]">
          <video
            ref={videoRef}
            autoPlay
            playsInline
            className="w-full h-full object-cover"
          />

          <div className="animate-scan-line absolute inset-0 bg-gradient-to-b from-transparent via-orange-400/20 to-transparent"></div>

          {/* Corner Borders */}
          <div className="absolute left-8 top-8 h-12 w-12 rounded-tl-2xl border-l-4 border-t-4 border-orange-400"></div>
          <div className="absolute right-8 top-8 h-12 w-12 rounded-tr-2xl border-r-4 border-t-4 border-orange-400"></div>
          <div className="absolute bottom-8 left-8 h-12 w-12 rounded-bl-2xl border-b-4 border-l-4 border-orange-400"></div>
          <div className="absolute bottom-8 right-8 h-12 w-12 rounded-br-2xl border-b-4 border-r-4 border-orange-400"></div>

          {error && (
            <div className="absolute inset-0 bg-gray-900/90 flex flex-col items-center justify-center p-6 text-center">
               <AlertCircle size={48} className="text-red-500 mb-4" />
               <p className="font-bold text-sm uppercase">{error}</p>
               {canRetryCamera && (
                 <button onClick={() => void startCamera()} className="mt-4 min-h-11 rounded-xl bg-orange-600 px-6 py-2 text-xs font-black">THỬ LẠI</button>
               )}
            </div>
          )}
       </div>

       <div className="mt-12 flex flex-col items-center space-y-4">
          <div className="p-4 bg-white/5 rounded-2xl flex items-center border border-white/10">
             <QrCode size={20} className="mr-3 text-orange-400" />
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
