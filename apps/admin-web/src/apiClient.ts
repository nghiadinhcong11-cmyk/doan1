import { API_URL } from './config';

let installed = false;

/** Adds the current JWT to API requests made by the existing fetch-based UI. */
export function installApiAuthInterceptor() {
  if (installed || typeof window === 'undefined') return;
  installed = true;

  const nativeFetch = window.fetch.bind(window);
  window.fetch = (input: RequestInfo | URL, init?: RequestInit) => {
    const url = input instanceof Request ? input.url : String(input);
    if (!url.startsWith(API_URL)) return nativeFetch(input, init);

    const headers = new Headers(init?.headers || (input instanceof Request ? input.headers : undefined));
    const requestPath = new URL(url, window.location.origin).pathname;
    const isAuthenticationRequest = requestPath.startsWith('/api/Auth/');
    const token = localStorage.getItem('adminToken');
    if (token && !isAuthenticationRequest && !headers.has('Authorization')) headers.set('Authorization', `Bearer ${token}`);

    return nativeFetch(input, { ...init, headers }).then(response => {
      if (response.status === 401) {
        localStorage.removeItem('adminToken');
        localStorage.removeItem('isLoggedIn');
        localStorage.removeItem('userRole');
        window.dispatchEvent(new Event('pos:unauthorized'));
      } else if (response.status === 403) {
        window.dispatchEvent(new Event('pos:forbidden'));
      }
      return response;
    });
  };
}
