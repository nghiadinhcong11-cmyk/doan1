import { API_URL } from '../config';

export interface BranchSummary {
  id: string;
  name: string;
  isMain?: boolean;
  [key: string]: unknown;
}

let inFlightRequest: Promise<BranchSummary[]> | null = null;
let inFlightScope = '';
let cachedBranches: BranchSummary[] | null = null;

const requestScope = () => `${localStorage.getItem('adminToken') || ''}|${localStorage.getItem('userRole') || ''}`;
let cachedScope = '';

/** Clear after branch mutations or logout so the next read is authoritative. */
export function invalidateBranchListCache() {
  cachedBranches = null;
  cachedScope = '';
}

/**
 * Coalesces simultaneous branch-list reads from the shared navbar and the page.
 * There is intentionally no persistent cache: a later navigation gets a fresh request.
 */
export function fetchBranchList(options?: { force?: boolean }): Promise<BranchSummary[]> {
  const scope = requestScope();
  if (options?.force) invalidateBranchListCache();
  if (cachedBranches && cachedScope === scope) {
    return Promise.resolve(cachedBranches);
  }
  if (inFlightRequest && inFlightScope === scope) return inFlightRequest;

  const request = fetch(`${API_URL}/api/Branch`)
    .then(response => {
      if (!response.ok) throw new Error('Branch request failed');
      return response.json();
    })
    .then(data => {
      if (!Array.isArray(data)) throw new Error('Branch response was not a list');
      cachedBranches = data as BranchSummary[];
      cachedScope = scope;
      return data as BranchSummary[];
    })
    .finally(() => {
      if (inFlightRequest === request) {
        inFlightRequest = null;
        inFlightScope = '';
      }
    });

  inFlightRequest = request;
  inFlightScope = scope;
  return request;
}

if (typeof window !== 'undefined') {
  window.addEventListener('branches:changed', invalidateBranchListCache);
}
