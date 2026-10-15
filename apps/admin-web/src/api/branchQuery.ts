import { API_URL } from '../config';

export interface BranchSummary {
  id: string;
  name: string;
  isMain?: boolean;
  [key: string]: unknown;
}

let inFlightRequest: Promise<BranchSummary[]> | null = null;

/**
 * Coalesces simultaneous branch-list reads from the shared navbar and the page.
 * There is intentionally no persistent cache: a later navigation gets a fresh request.
 */
export function fetchBranchList(): Promise<BranchSummary[]> {
  if (inFlightRequest) return inFlightRequest;

  const request = fetch(`${API_URL}/api/Branch`)
    .then(response => {
      if (!response.ok) throw new Error('Branch request failed');
      return response.json();
    })
    .then(data => {
      if (!Array.isArray(data)) throw new Error('Branch response was not a list');
      return data as BranchSummary[];
    })
    .finally(() => {
      if (inFlightRequest === request) inFlightRequest = null;
    });

  inFlightRequest = request;
  return request;
}
