import React from 'react';
import { AlertCircle, AlertTriangle, CheckCircle2, Info, X } from 'lucide-react';

export type FeedbackTone = 'success' | 'error' | 'warning' | 'info';
const styles: Record<FeedbackTone, { wrapper: string; icon: React.ElementType }> = {
  success: { wrapper: 'border-green-200 bg-green-50 text-green-700', icon: CheckCircle2 },
  error: { wrapper: 'border-red-200 bg-red-50 text-red-700', icon: AlertCircle },
  warning: { wrapper: 'border-amber-200 bg-amber-50 text-amber-700', icon: AlertTriangle },
  info: { wrapper: 'border-blue-200 bg-blue-50 text-blue-700', icon: Info }
};

export default function Feedback({ tone, children, onDismiss }: { tone: FeedbackTone; children: React.ReactNode; onDismiss?: () => void }) {
  const { wrapper, icon: Icon } = styles[tone];
  return (
    <div role={tone === 'error' ? 'alert' : 'status'} className={`flex items-start gap-2 rounded-2xl border px-4 py-3 text-xs font-bold ${wrapper}`}>
      <Icon aria-hidden="true" size={16} className="mt-0.5 shrink-0" />
      <span className="flex-1">{children}</span>
      {onDismiss && <button type="button" aria-label="Đóng thông báo" onClick={onDismiss} className="rounded-md p-0.5 hover:bg-black/5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-current"><X aria-hidden="true" size={15} /></button>}
    </div>
  );
}
