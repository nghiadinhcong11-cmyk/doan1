// Shared formatters for Currency and Date

export const formatCurrency = (amount: number | string | null | undefined): string => {
  if (amount === null || amount === undefined || isNaN(Number(amount))) return '0đ';
  return Number(amount).toLocaleString('vi-VN') + 'đ';
};

export const formatDate = (dateInput: string | Date | null | undefined): string => {
  if (!dateInput) return '';
  const d = new Date(dateInput);
  if (isNaN(d.getTime())) return String(dateInput);
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();
  const hours = String(d.getHours()).padStart(2, '0');
  const minutes = String(d.getMinutes()).padStart(2, '0');
  return `${hours}:${minutes} - ${day}/${month}/${year}`;
};
