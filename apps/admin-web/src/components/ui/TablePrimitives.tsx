import React from 'react';
import { Loader2 } from 'lucide-react';

export const TableContainer = ({ children }: { children: React.ReactNode }) => (
  <div className="overflow-x-auto rounded-2xl border border-gray-200 bg-white">
    <table className="min-w-full text-sm">{children}</table>
  </div>
);

export const TableHeader = ({ children }: { children: React.ReactNode }) => (
  <thead className="bg-gray-50 text-left text-xs font-bold text-gray-500">{children}</thead>
);

export const TableCell = ({ children, className = '' }: { children: React.ReactNode; className?: string }) => (
  <td className={`border-t border-gray-100 px-4 py-3 ${className}`}>{children}</td>
);

export const TableActions = ({ children }: { children: React.ReactNode }) => (
  <td className="border-t border-gray-100 px-4 py-3 text-right whitespace-nowrap">{children}</td>
);

export const TableLoading = ({ colSpan }: { colSpan: number }) => (
  <tr><td colSpan={colSpan} className="px-4 py-10 text-center text-gray-500"><Loader2 className="mx-auto animate-spin" size={20} /></td></tr>
);

export const TableEmpty = ({ colSpan, children }: { colSpan: number; children: React.ReactNode }) => (
  <tr><td colSpan={colSpan} className="px-4 py-10 text-center text-gray-500">{children}</td></tr>
);
