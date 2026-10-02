import { Loader2 } from 'lucide-react';

const Spinner = ({ label = 'Đang xử lý...' }: { label?: string }) => (
  <span className="inline-flex items-center gap-2" role="status">
    <Loader2 aria-hidden="true" className="animate-spin" size={16} />
    <span className="sr-only">{label}</span>
  </span>
);

export default Spinner;
