/// <reference types="vite/client" />

// When the customer page is opened from a phone, localhost points to the
// phone itself. Use the host that served the page so LAN QR ordering can
// reach the API on the POS machine. VITE_API_URL remains the deploy override.
const runtimeApiHost = typeof window !== 'undefined' ? window.location.hostname : 'localhost';
const runtimeApiProtocol = typeof window !== 'undefined' ? window.location.protocol : 'http:';

export const API_URL = import.meta.env.VITE_API_URL
  || `${runtimeApiProtocol}//${runtimeApiHost}:5000`;
