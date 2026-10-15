const runtimeHost = typeof window !== 'undefined' ? window.location.hostname : 'localhost';
// Derive Development links from the host serving Admin Web. This keeps QR
// URLs reachable when Admin is opened through the current LAN address.
const defaultHost = runtimeHost;
const runtimeCustomerProtocol = import.meta.env.DEV
  ? 'https:'
  : (typeof window !== 'undefined' ? window.location.protocol : 'https:');
const runtimeApiProtocol = import.meta.env.DEV ? 'https:' : runtimeCustomerProtocol;
// Explicit Vite variables remain authoritative for deployments and deliberate
// hostname overrides.
export const API_URL = import.meta.env.VITE_API_URL
  || `${runtimeApiProtocol}//${defaultHost}:5000`;

export const CUSTOMER_WEB_URL = import.meta.env.VITE_CUSTOMER_WEB_URL
  || `${runtimeCustomerProtocol}//${defaultHost}:5174`;
