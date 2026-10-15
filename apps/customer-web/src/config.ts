/// <reference types="vite/client" />

// When the customer page is opened from a phone, localhost points to the
// phone itself. Use the host that served the page so LAN QR ordering can
// reach the API on the POS machine. VITE_API_URL remains the deploy override.
const runtimeApiHost = typeof window !== 'undefined' ? window.location.hostname : 'localhost';
// In Development, use the host that served the page so a phone opening the
// LAN QR URL does not try to resolve the POS machine's private hostname.
const defaultHost = runtimeApiHost;
// The Development API is HTTPS on port 5000 even when this Vite page is
// served over HTTP on port 5174. Keep VITE_API_URL as the explicit deployment
// override and never fall back to insecure HTTP for API traffic.
const runtimeApiProtocol = import.meta.env.DEV
  ? 'https:'
  : (typeof window !== 'undefined' ? window.location.protocol : 'https:');

export const API_URL = import.meta.env.VITE_API_URL
  || `${runtimeApiProtocol}//${defaultHost}:5000`;
