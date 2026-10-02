const runtimeCustomerHost = typeof window !== 'undefined' ? window.location.hostname : 'localhost';
const runtimeCustomerProtocol = typeof window !== 'undefined' ? window.location.protocol : 'http:';
const runtimeApiProtocol = import.meta.env.DEV ? 'https:' : runtimeCustomerProtocol;
// Khi mở frontend trên điện thoại, localhost là điện thoại chứ không phải máy chạy API.
// Dùng cùng hostname với frontend để hoạt động trong LAN; VITE_API_URL vẫn được ưu tiên khi deploy.
export const API_URL = import.meta.env.VITE_API_URL
  || `${runtimeApiProtocol}//${runtimeCustomerHost}:5000`;

export const CUSTOMER_WEB_URL = import.meta.env.VITE_CUSTOMER_WEB_URL
  || `${runtimeCustomerProtocol}//${runtimeCustomerHost}:5174`;
