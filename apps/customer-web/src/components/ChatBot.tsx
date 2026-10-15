import React, { useState, useEffect, useRef } from 'react';
import { MessageCircle, X, Send, Bot, User, Sparkles, ChevronRight } from 'lucide-react';
import { API_URL } from '../config';
import { useLocation } from 'react-router-dom';

const getUsableCustomerToken = (): string | null => {
  const token = localStorage.getItem('customerToken');
  if (!token) return null;

  try {
    const payloadSegment = token.split('.')[1];
    if (!payloadSegment) throw new Error('Malformed token');
    const payload = JSON.parse(atob(payloadSegment.replace(/-/g, '+').replace(/_/g, '/')));
    if (typeof payload.exp === 'number' && payload.exp <= Math.floor(Date.now() / 1000)) {
      throw new Error('Expired token');
    }
    return token;
  } catch {
    // Never send a malformed or expired bearer token to the public guest route.
    localStorage.removeItem('customerToken');
    localStorage.removeItem('customerInfo');
    return null;
  }
};

const ChatBot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState<any[]>([
    { id: 1, text: "Xin chào! Tôi là trợ lý DOAN. Bạn muốn xem thực đơn, giá món hay hỏi nhanh điều gì hôm nay?", isBot: true }
  ]);
  const [input, setInput] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const scrollRef = useRef<HTMLDivElement>(null);
  const location = useLocation();

  // Draggable states
  const [position, setPosition] = useState({ x: window.innerWidth - 80, y: window.innerHeight - 150 });
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

  const getSuggestions = () => {
    return ["Thực đơn hôm nay có gì?", "Tôi muốn đặt bàn", "Giờ mở cửa của nhà hàng?", "Địa chỉ nhà hàng ở đâu?"];
  };

  const handleSend = async (text: string) => {
    if (!text.trim()) return;

    const userMsg = { id: Date.now(), text, isBot: false };
    setMessages(prev => [...prev, userMsg]);
    setInput('');
    setIsTyping(true);

    try {
      // Lấy 6 tin nhắn gần nhất làm lịch sử đàm thoại cho Mock AI
      const history = messages.slice(-6).map(m => ({
        role: m.isBot ? "model" : "user",
        content: m.text
      }));

      const token = getUsableCustomerToken();
      const headers: Record<string, string> = {
        'Content-Type': 'application/json'
      };
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const endpoint = token ? `${API_URL}/api/Ai/chat` : `${API_URL}/api/Ai/guest-chat`;
      const response = await fetch(endpoint, {
        method: 'POST',
        headers,
        body: JSON.stringify({ message: text, history })
      });

      if (!response.ok) {
        throw new Error(`Server trả về lỗi: ${response.status}`);
      }
      const data = await response.json();
      setMessages(prev => [...prev, { id: Date.now() + 1, text: data.message, isBot: true }]);
    } catch (err: any) {
      console.error('ChatBot Error:', err);
      setMessages(prev => [...prev, {
        id: Date.now() + 1,
        text: `Lỗi kết nối: ${err.message}. Hãy đảm bảo Backend đang chạy tại ${API_URL}`,
        isBot: true
      }]);
    } finally {
      setIsTyping(false);
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
        aria-label={isOpen ? 'Đóng trợ lý DOAN' : 'Mở trợ lý DOAN'}
        onMouseDown={onMouseDown}
        onTouchStart={onTouchStart}
        onClick={handleToggle}
        className={`flex h-14 w-14 items-center justify-center rounded-full shadow-2xl transition-all duration-300 ${isOpen ? 'rotate-90 bg-[#22252b]' : 'bg-orange-600 hover:scale-110 active:scale-95'} ${!isOpen && !isDragging ? 'animate-bounce' : ''}`}
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
          <div className="flex items-center space-x-3 bg-[#3e8069] p-5 text-white">
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

          {/* Suggestions & Input */}
          <div className="p-4 bg-white border-t border-gray-100 space-y-4">
            <div className="flex flex-wrap gap-2">
               {getSuggestions().map((s, i) => (
                 <button
                  key={i}
                  onClick={() => handleSend(s)}
                  className="rounded-full border border-orange-100 bg-orange-50 px-3 py-2 text-[10px] font-bold text-orange-600 transition-colors hover:bg-orange-100"
                 >
                   {s}
                 </button>
               ))}
            </div>

            <form
              onSubmit={(e) => { e.preventDefault(); handleSend(input); }}
                className="relative"
            >
              <input
                type="text"
                value={input}
                onChange={(e) => setInput(e.target.value)}
                placeholder="Nhập câu hỏi của bạn..."
                aria-label="Nội dung câu hỏi"
                className="w-full rounded-2xl border border-[#ebe5dc] bg-[#f8f6f1] py-3.5 pl-4 pr-12 text-xs font-bold outline-none focus:ring-2 focus:ring-orange-300"
              />
              <button
                type="submit"
                aria-label="Gửi câu hỏi"
                className="absolute right-2 top-2 flex h-9 w-9 items-center justify-center rounded-xl bg-orange-600 text-white shadow-lg transition-all hover:bg-orange-700 active:scale-95"
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
