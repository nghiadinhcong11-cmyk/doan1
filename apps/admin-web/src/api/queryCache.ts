type CacheOptions = { force?: boolean };

const values = new Map<string, unknown>();
const inFlight = new Map<string, Promise<unknown>>();
const versions = new Map<string, number>();

const scopeKey = () => `${localStorage.getItem('adminToken') || ''}|${localStorage.getItem('userRole') || ''}`;
const cacheKey = (url: string) => `${scopeKey()}|${url}`;

/** Session-scoped GET cache. Mutations must invalidate the affected URL prefix. */
export async function getCachedJson<T>(url: string, init?: RequestInit, options?: CacheOptions): Promise<T> {
  const key = cacheKey(url);
  if (options?.force) {
    values.delete(key);
    inFlight.delete(key);
  }
  if (values.has(key)) return values.get(key) as T;
  const running = inFlight.get(key);
  if (running) return running as Promise<T>;
  const version = versions.get(key) || 0;

  const request = fetch(url, init).then(async response => {
    if (!response.ok) throw new Error(`Request failed (${response.status})`);
    const data = await response.json() as T;
    if ((versions.get(key) || 0) === version) values.set(key, data);
    return data;
  }).finally(() => {
    if (inFlight.get(key) === request) inFlight.delete(key);
  });

  inFlight.set(key, request);
  return request;
}

export function invalidateCachedJson(prefix?: string) {
  if (!prefix) {
    values.clear();
    inFlight.clear();
    versions.clear();
    return;
  }
  const keys = new Set([...values.keys(), ...inFlight.keys()]);
  for (const key of keys) {
    if (!key.includes(`|${prefix}`)) continue;
    values.delete(key);
    inFlight.delete(key);
    versions.set(key, (versions.get(key) || 0) + 1);
  }
}

if (typeof window !== 'undefined') {
  window.addEventListener('branches:changed', () => invalidateCachedJson('/api/Branch/management'));
  window.addEventListener('product-price-updated', () => invalidateCachedJson('/api/Product'));
  window.addEventListener('catalog-item-created', () => {
    invalidateCachedJson('/api/Product');
    invalidateCachedJson('/api/Topping');
  });
}
