export type BackendProxyQuery = Record<
  string,
  string | number | boolean | undefined | null
>;

export function resolveBackendBaseUrl(env: {
  HARVESTRY_BACKEND_URL?: string;
  NEXT_PUBLIC_API_URL?: string;
}): string {
  const candidate = (env.HARVESTRY_BACKEND_URL ?? env.NEXT_PUBLIC_API_URL ?? '').trim();
  if (!candidate) {
    throw new Error(
      'Backend base URL is not configured. Set HARVESTRY_BACKEND_URL (server) or NEXT_PUBLIC_API_URL (dev).'
    );
  }
  return candidate.replace(/\/+$/, '');
}

export function buildBackendUrl(args: {
  backendBaseUrl: string;
  backendPath: string;
  requestUrl: string;
  query?: BackendProxyQuery;
}): string {
  const normalizedPath = args.backendPath.startsWith('/')
    ? args.backendPath
    : `/${args.backendPath}`;

  const url = new URL(`${args.backendBaseUrl}${normalizedPath}`);

  const incoming = new URL(args.requestUrl);
  incoming.searchParams.forEach((value, key) => {
    url.searchParams.set(key, value);
  });

  if (args.query) {
    Object.entries(args.query).forEach(([k, v]) => {
      if (v === undefined || v === null) return;
      url.searchParams.set(k, String(v));
    });
  }

  return url.toString();
}

export function buildForwardHeaders(requestHeaders: Headers): Headers {
  const headers = new Headers();

  const auth = requestHeaders.get('authorization');
  if (auth) headers.set('authorization', auth);

  const siteId = requestHeaders.get('x-site-id');
  if (siteId) headers.set('x-site-id', siteId);

  const userId = requestHeaders.get('x-user-id');
  if (userId) headers.set('x-user-id', userId);

  // Forward user role for backend development authentication
  const userRole = requestHeaders.get('x-user-role');
  if (userRole) headers.set('x-user-role', userRole);

  const contentType = requestHeaders.get('content-type');
  if (contentType) headers.set('content-type', contentType);

  return headers;
}

