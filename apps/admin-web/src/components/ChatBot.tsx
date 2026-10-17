import React, { useState, useEffect, useRef } from 'react';
import { MessageCircle, X, Send, Bot } from 'lucide-react';
import { API_URL } from '../config';

const ADMIN_WELCOME = 'Xin chào! Tôi là trợ lý ảo DOAN POS. Tôi có thể giúp gì cho bạn?';

const readTokenContext = (token: string | null): string => {
  if (!token) return 'anonymous';
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    return `${payload.sub ?? ''}|${payload.role ?? ''}|${payload.branchId ?? ''}`;
  } catch {
    return 'invalid-token';
  }
};

const getAdminContextKey = (): string =>
  `${readTokenContext(localStorage.getItem('adminToken'))}|${localStorage.getItem('userRole') ?? ''}|${localStorage.getItem('selectedBranchId') ?? ''}`;

const ChatBot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<any[]>([
    { id: 1, text: "Xin chào! Tôi là trợ lý ảo DOAN POS. Tôi có thể giúp gì cho bạn?", isBot: true }
  ]);
  const [input, setInput] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [confirmingMessageId, setConfirmingMessageId] = useState<number | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);
  const inFlightRef = useRef(false);
  const requestIdRef = useRef(0);
  const contextKeyRef = useRef(getAdminContextKey());
  const [contextKey, setContextKey] = useState(contextKeyRef.current);

  useEffect(() => {
    const syncContext = () => {
      const next = getAdminContextKey();
      setContextKey(previous => previous === next ? previous : next);
    };
    window.addEventListener('storage', syncContext);
    const timer = window.setInterval(syncContext, 500);
    return () => {
      window.removeEventListener('storage', syncContext);
      window.clearInterval(timer);
    };
  }, []);

  useEffect(() => {
    if (contextKeyRef.current === contextKey) return;
    contextKeyRef.current = contextKey;
    requestIdRef.current += 1;
    inFlightRef.current = false;
    setMessages([{ id: Date.now(), text: ADMIN_WELCOME, isBot: true }]);
    setInput('');
    setIsOpen(false);
    setIsTyping(false);
    setConfirmingMessageId(null);
  }, [contextKey]);

  // Draggable states
  const [position, setPosition] = useState({ x: window.innerWidth - 80, y: window.innerHeight - 80 });
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [hasMoved, setHasMoved] = useState(false);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, isTyping]);

  // Update position on window resize
  useEffect(() => {
    const handleResize = () => {
      setPosition(prev => ({
        x: Math.min(prev.x, window.innerWidth - 60),
        y: Math.min(prev.y, window.innerHeight - 60)
      }));
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  const onMouseDown = (e: React.MouseEvent) => {
    setIsDragging(true);
    setHasMoved(false);
    setDragStart({
      x: e.clientX - position.x,
      y: e.clientY - position.y
    });
  };

  const onTouchStart = (e: React.TouchEvent) => {
    const touch = e.touches[0];
    setIsDragging(true);
    setHasMoved(false);
    setDragStart({
      x: touch.clientX - position.x,
      y: touch.clientY - position.y
    });
  };

  useEffect(() => {
    const onMouseMove = (e: MouseEvent) => {
      if (!isDragging) return;

      setHasMoved(true);
      const newX = Math.max(10, Math.min(e.clientX - dragStart.x, window.innerWidth - 60));
      const newY = Math.max(10, Math.min(e.clientY - dragStart.y, window.innerHeight - 60));

      setPosition({ x: newX, y: newY });
    };

    const onTouchMove = (e: TouchEvent) => {
      if (!isDragging) return;

      // Ngăn chặn cuộn trang khi đang kéo bot trên mobile
      if (e.cancelable) e.preventDefault();

      setHasMoved(true);
      const touch = e.touches[0];
      const newX = Math.max(10, Math.min(touch.clientX - dragStart.x, window.innerWidth - 60));
      const newY = Math.max(10, Math.min(touch.clientY - dragStart.y, window.innerHeight - 60));

      setPosition({ x: newX, y: newY });
    };

    const onEnd = () => {
      setIsDragging(false);
    };

    if (isDragging) {
      window.addEventListener('mousemove', onMouseMove);
      window.addEventListener('mouseup', onEnd);
      window.addEventListener('touchmove', onTouchMove, { passive: false });
      window.addEventListener('touchend', onEnd);
    }

    return () => {
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onEnd);
      window.removeEventListener('touchmove', onTouchMove);
      window.removeEventListener('touchend', onEnd);
    };
  }, [isDragging, dragStart]);

  const handleToggle = () => {
    if (!hasMoved) {
      setIsOpen(!isOpen);
    }
  };

  const handleSend = async (text: string) => {
    if (!text.trim() || inFlightRef.current) return;

    inFlightRef.current = true;
    const requestId = ++requestIdRef.current;
    const requestContextKey = contextKey;

    const userMsg = { id: Date.now(), text, isBot: false };
    setMessages(prev => [...prev, userMsg]);
    setInput('');
    setIsTyping(true);

    try {
      const token = localStorage.getItem('adminToken');
      const headers: Record<string, string> = {
        'Content-Type': 'application/json'
      };
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const history = messages.slice(-6).map(m => ({ role: m.isBot ? 'assistant' : 'user', content: m.text }));

      const response = await fetch(`${API_URL}/api/Ai/chat`, {
        method: 'POST',
        headers,
        body: JSON.stringify({ message: text, history })
      });

      if (!response.ok) {
        throw new Error(`Server trả về lỗi: ${response.status}`);
      }
      const data = await response.json();
      if (requestId !== requestIdRef.current || requestContextKey !== getAdminContextKey()) return;
      if (data.success === false) {
        setMessages(prev => [...prev, { id: Date.now() + 1, text: data.error ?? 'Yêu cầu AI chưa thực hiện được.', isBot: true }]);
        return;
      }
      setMessages(prev => [...prev, {
        id: Date.now() + 1,
        text: data.message,
        isBot: true,
        confirmation: data.confirmation
      }]);
    } catch (err: any) {
      if (requestId !== requestIdRef.current || requestContextKey !== getAdminContextKey()) return;
      console.error('ChatBot Error:', err);
      setMessages(prev => [...prev, {
        id: Date.now() + 1,
        text: `Lỗi kết nối: ${err.message}. Hãy đảm bảo Backend đang chạy tại ${API_URL}`,
        isBot: true
      }]);
    } finally {
      if (requestId === requestIdRef.current) {
        inFlightRef.current = false;
        setIsTyping(false);
      }
    }
  };

  const handlePriceConfirmation = async (messageId: number, token: string) => {
    if (!token || confirmingMessageId !== null) return;

    setConfirmingMessageId(messageId);
    try {
      const adminToken = localStorage.getItem('adminToken');
      const response = await fetch(`${API_URL}/api/Ai/product-price-confirmation`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(adminToken ? { Authorization: `Bearer ${adminToken}` } : {})
        },
        body: JSON.stringify({ token })
      });
      const data = await response.json();
      setMessages(prev => [
        ...prev.map(message => message.id === messageId
          ? { ...message, confirmation: undefined }
          : message),
        {
          id: Date.now() + 1,
          text: response.ok ? data.message : (data.message ?? 'Không thể xác nhận cập nhật giá.'),
          isBot: true
        }
      ]);
    } catch (err: any) {
      setMessages(prev => [...prev, {
        id: Date.now() + 1,
        text: `Lỗi xác nhận: ${err.message}`,
        isBot: true
      }]);
    } finally {
      setConfirmingMessageId(null);
    }
  };

  const isNearRight = position.x > window.innerWidth / 2;
  const isNearBottom = position.y > window.innerHeight / 2;

  return (
    <div
      className="fixed z-[999] font-sans transition-shadow"
      style={{ left: position.x, top: position.y, cursor: isDragging ? 'grabbing' : 'grab' }}
    >
      {/* Nút bong bóng Chat */}
      <button
        onMouseDown={onMouseDown}
        onTouchStart={onTouchStart}
        onClick={handleToggle}
        aria-label={isOpen ? 'Đóng trợ lý AI' : 'Mở trợ lý AI'}
        className={`w-14 h-14 rounded-full shadow-2xl flex items-center justify-center transition-all duration-300 ${isOpen ? 'bg-gray-800 rotate-90' : 'bg-blue-600 hover:scale-110 active:scale-95'} ${!isOpen && !isDragging ? 'animate-bounce' : ''}`}
      >
        {isOpen ? <X className="text-white" /> : <MessageCircle className="text-white" size={28} />}
        {!isOpen && (
          <span className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full border-2 border-white"></span>
        )}
      </button>

      {/* Cửa sổ Chat */}
      {isOpen && (
        <div
          className={`absolute w-[calc(100vw-40px)] sm:w-[350px] h-[500px] max-h-[70vh] sm:max-h-[500px] bg-white rounded-[2rem] shadow-2xl border border-gray-100 flex flex-col overflow-hidden animate-in slide-in-from-bottom-10 duration-300`}
          style={{
            bottom: isNearBottom ? '70px' : 'auto',
            top: isNearBottom ? 'auto' : '70px',
            right: isNearRight ? '0px' : 'auto',
            left: isNearRight ? 'auto' : '0px'
          }}
        >
          {/* Header */}
          <div className="bg-blue-600 p-6 text-white flex items-center space-x-3">
            <div className="w-10 h-10 bg-white/20 rounded-2xl flex items-center justify-center">
              <Bot size={24} />
            </div>
            <div>
              <p className="font-black text-sm uppercase tracking-tighter italic">DOAN Assistant</p>
              <p className="text-[10px] opacity-80 font-bold flex items-center">
                <span className="w-1.5 h-1.5 bg-green-400 rounded-full mr-1.5 animate-pulse"></span> Đang trực tuyến
              </p>
            </div>
          </div>

          {/* Messages */}
          <div ref={scrollRef} className="flex-1 overflow-y-auto p-6 space-y-4 bg-gray-50/50">
            {messages.map(msg => (
              <div key={msg.id} className={`flex ${msg.isBot ? 'justify-start' : 'justify-end'}`}>
                <div className={`max-w-[80%] p-4 rounded-2xl text-xs font-medium leading-relaxed ${
                  msg.isBot
                  ? 'bg-white text-gray-700 shadow-sm rounded-tl-none border border-gray-100'
                  : 'bg-blue-600 text-white rounded-tr-none shadow-blue-500/20 shadow-lg'
                }`}>
                  {msg.text}
                  {msg.confirmation && (
                    <button
                      type="button"
                      disabled={confirmingMessageId !== null}
                      onClick={() => handlePriceConfirmation(msg.id, msg.confirmation.token)}
                      className="mt-3 w-full rounded-xl bg-emerald-600 px-3 py-2 text-[10px] font-bold text-white disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {confirmingMessageId === msg.id ? 'Đang xác nhận...' : 'Xác nhận cập nhật giá'}
                    </button>
                  )}
                </div>
              </div>
            ))}
            {isTyping && (
              <div className="flex justify-start">
                <div className="bg-white p-3 rounded-2xl shadow-sm flex space-x-1">
                   <div className="w-1.5 h-1.5 bg-gray-300 rounded-full animate-bounce"></div>
                   <div className="w-1.5 h-1.5 bg-gray-300 rounded-full animate-bounce delay-75"></div>
                   <div className="w-1.5 h-1.5 bg-gray-300 rounded-full animate-bounce delay-150"></div>
                </div>
              </div>
            )}
          </div>

          {/* Input */}
          <div className="p-4 bg-white border-t border-gray-100">
            <form
              onSubmit={(e) => { e.preventDefault(); handleSend(input); }}
              className="relative"
            >
              <input
                type="text"
                value={input}
                onChange={(e) => setInput(e.target.value)}
                placeholder="Nhập câu hỏi của bạn..."
                className="w-full pl-4 pr-12 py-3.5 bg-gray-100 border-none rounded-2xl text-xs font-bold focus:ring-2 focus:ring-blue-500/20 outline-none"
              />
              <button
                type="submit"
                className="absolute right-2 top-2 w-9 h-9 bg-blue-600 text-white rounded-xl flex items-center justify-center shadow-lg hover:bg-blue-700 transition-all active:scale-95"
              >
                <Send size={16} />
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ChatBot;
