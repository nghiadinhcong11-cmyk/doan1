import React from 'react';

export type StatusTone = 'active' | 'inactive' | 'pending' | 'confirmed' | 'cancelled' | 'completed' | 'paid' | 'unpaid' | 'open' | 'closed';

const styles: Record<StatusTone, string> = {
  active: 'bg-green-50 text-green-700 border-green-100',
  inactive: 'bg-gray-100 text-gray-600 border-gray-200',
  pending: 'bg-amber-50 text-amber-700 border-amber-100',
  confirmed: 'bg-blue-50 text-blue-700 border-blue-100',
  cancelled: 'bg-red-50 text-red-700 border-red-100',
  completed: 'bg-emerald-50 text-emerald-700 border-emerald-100',
  paid: 'bg-emerald-50 text-emerald-700 border-emerald-100',
  unpaid: 'bg-orange-50 text-orange-700 border-orange-100',
  open: 'bg-blue-50 text-blue-700 border-blue-100',
  closed: 'bg-gray-100 text-gray-600 border-gray-200'
};

const StatusBadge = ({ tone, children }: { tone: StatusTone; children: React.ReactNode }) => (
  <span className={`inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-bold ${styles[tone]}`}>{children}</span>
);

export default StatusBadge;
