import { describe, expect, it } from 'vitest';
import { buildBackendUrl, resolveBackendBaseUrl } from '@/lib/api/backendProxyInternal';

describe('backendProxyInternal', () => {
  it('resolveBackendBaseUrl trims and strips trailing slashes', () => {
    expect(
      resolveBackendBaseUrl({ HARVESTRY_BACKEND_URL: ' https://api.example.com/// ' })
    ).toBe('https://api.example.com');
  });

  it('resolveBackendBaseUrl throws when unset', () => {
    expect(() => resolveBackendBaseUrl({})).toThrow(/Backend base URL is not configured/);
  });

  it('buildBackendUrl merges incoming query and overrides', () => {
    const url = buildBackendUrl({
      backendBaseUrl: 'https://api.example.com',
      backendPath: '/api/v1/sites/abc/sales/orders',
      requestUrl: 'http://localhost:3000/api/v1/sites/abc/sales/orders?page=1&status=Draft',
      query: { page: 2, search: 'foo' },
    });

    expect(url).toContain('https://api.example.com/api/v1/sites/abc/sales/orders?');
    expect(url).toContain('page=2');
    expect(url).toContain('status=Draft');
    expect(url).toContain('search=foo');
  });
});

