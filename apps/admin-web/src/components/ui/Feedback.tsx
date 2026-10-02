import React from 'react';
import { AlertCircle, AlertTriangle, CheckCircle2, Info, X } from 'lucide-react';

export type FeedbackTone = 'success' | 'error' | 'warning' | 'info';

export function notifyFeedback(message: string, tone: FeedbackTone = 'error') {
  window.dispatchEvent(new CustomEvent('ui:feedback', { detail: { message, tone } }));
}

const toneStyles: Record<FeedbackTone, { wrapper: string; icon: React.ElementType }> = {
  success: { wrapper: 'border-green-200 bg-green-50 text-green-700', icon: CheckCircle2 },
  error: { wrapper: 'border-red-200 bg-red-50 text-red-700', icon: AlertCircle },
  warning: { wrapper: 'border-amber-200 bg-amber-50 text-amber-700', icon: AlertTriangle },
  info: { wrapper: 'border-blue-200 bg-blue-50 text-blue-700', icon: Info }
};

interface FeedbackProps {
  tone: FeedbackTone;
  children: React.ReactNode;
  onDismiss?: () => void;
}

export function Feedback({ tone, children, onDismiss }: FeedbackProps) {
  const { wrapper, icon: Icon } = toneStyles[tone];
  return (
    <div role={tone === 'error' ? 'alert' : 'status'} className={`flex items-start gap-2 rounded-xl border px-3 py-2.5 text-sm font-medium ${wrapper}`}>
      <Icon aria-hidden="true" size={17} className="mt-0.5 shrink-0" />
      <span className="flex-1">{children}</span>
      {onDismiss && <button type="button" aria-label="Đóng thông báo" onClick={onDismiss} className="rounded-md p-0.5 hover:bg-black/5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-current"><X aria-hidden="true" size={16} /></button>}
    </div>
  );
}

export function FeedbackHost() {
  const [current, setCurrent] = React.useState<{ message: string; tone: FeedbackTone } | null>(null);

  React.useEffect(() => {
    const handleFeedback = (event: Event) => {
      const detail = (event as CustomEvent<{ message: string; tone: FeedbackTone }>).detail;
      setCurrent(detail);
      window.setTimeout(() => setCurrent(value => value === detail ? null : value), 4500);
    };
    window.addEventListener('ui:feedback', handleFeedback);
    return () => window.removeEventListener('ui:feedback', handleFeedback);
  }, []);

  if (!current) return null;
  return <div className="fixed right-4 top-16 z-[500] w-[min(360px,calc(100vw-2rem))]"><Feedback tone={current.tone} onDismiss={() => setCurrent(null)}>{current.message}</Feedback></div>;
}

export default Feedback;
