import { NextResponse } from 'next/server';
import {
  buildBackendUrl,
  buildForwardHeaders,
  resolveBackendBaseUrl,
  type BackendProxyQuery,
} from './backendProxyInternal';

export type BackendProxyRequest = {
  request: Request;
  backendPath: string;
  /**
   * Optional query overrides (merged with request.query).
   * Values are stringified.
   */
  query?: BackendProxyQuery;
};

export class BackendProxy {
  private readonly _backendBaseUrl: string;

  public constructor() {
    this._backendBaseUrl = resolveBackendBaseUrl({
      HARVESTRY_BACKEND_URL: process.env.HARVESTRY_BACKEND_URL,
      NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
    });
  }

  public async proxyJson({ request, backendPath, query }: BackendProxyRequest) {
    const url = buildBackendUrl({
      backendBaseUrl: this._backendBaseUrl,
      backendPath,
      requestUrl: request.url,
      query,
    });

    const headers = buildForwardHeaders(request.headers);

    const response = await fetch(url, {
      method: request.method,
      headers,
      body: this.getBodyIfPresent(request),
      cache: 'no-store',
    });

    const contentType = response.headers.get('content-type') ?? '';
    const isJson = contentType.includes('application/json');

    if (response.ok) {
      if (response.status === 204) {
        return new NextResponse(null, { status: 204 });
      }

      const data = isJson ? await response.json() : await response.text();
      return NextResponse.json(data, { status: response.status });
    }

    // Normalize errors for the UI.
    let errorBody: unknown = undefined;
    try {
      errorBody = isJson ? await response.json() : await response.text();
    } catch {
      errorBody = undefined;
    }

    const message =
      typeof errorBody === 'object' && errorBody !== null && 'error' in errorBody
        ? String((errorBody as { error: unknown }).error)
        : `Request failed: ${response.status} ${response.statusText}`;

    return NextResponse.json(
      {
        error: message,
        status: response.status,
        body: errorBody,
      },
      { status: response.status }
    );
  }

  private getBodyIfPresent(request: Request): BodyInit | null {
    const method = request.method.toUpperCase();
    if (method === 'GET' || method === 'HEAD') return null;
    return request.body;
  }
}

