import i18n from 'i18next';

import { environment } from '@/shared/config/environment';
import { localeCultureMap, normalizeLocale } from '@/shared/i18n/types';

interface ApiErrorPayload {
  code?: string;
  message?: string;
}

interface ApiEnvelope<TData> {
  data?: TData;
  error?: ApiErrorPayload | null;
  success: boolean;
}

export class ApiClientError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly code?: string,
  ) {
    super(message);
    this.name = 'ApiClientError';
  }
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null;

const getErrorMessage = (payload: unknown, fallback: string) => {
  if (!isRecord(payload)) {
    return fallback;
  }

  if (typeof payload.detail === 'string') {
    return payload.detail;
  }

  if (typeof payload.title === 'string') {
    return payload.title;
  }

  if (isRecord(payload.error) && typeof payload.error.message === 'string') {
    return payload.error.message;
  }

  return fallback;
};

const getErrorCode = (payload: unknown) => {
  if (!isRecord(payload)) {
    return undefined;
  }

  if (typeof payload.code === 'string') {
    return payload.code;
  }

  if (isRecord(payload.error) && typeof payload.error.code === 'string') {
    return payload.error.code;
  }

  return undefined;
};

const unwrapEnvelope = <TData>(payload: unknown): TData => {
  if (!isRecord(payload) || !('success' in payload)) {
    return payload as TData;
  }

  const envelope = payload as unknown as ApiEnvelope<TData>;

  if (!envelope.success) {
    throw new ApiClientError(
      envelope.error?.message ?? i18n.t('errors.requestFailed'),
      400,
      envelope.error?.code,
    );
  }

  return envelope.data as TData;
};

export async function apiRequest<TData>(path: string, init: RequestInit = {}): Promise<TData> {
  const headers = new Headers(init.headers);
  const locale = normalizeLocale(i18n.resolvedLanguage ?? i18n.language);

  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (!headers.has('Accept-Language')) {
    headers.set('Accept-Language', localeCultureMap[locale]);
  }

  const response = await fetch(`${environment.apiBaseUrl}${path}`, {
    ...init,
    headers,
  });

  if (response.status === 204) {
    return undefined as TData;
  }

  const payload: unknown = await response.json().catch(() => undefined);

  if (!response.ok) {
    throw new ApiClientError(
      getErrorMessage(payload, i18n.t('errors.requestFailed')),
      response.status,
      getErrorCode(payload),
    );
  }

  return unwrapEnvelope<TData>(payload);
}

export async function apiBlobRequest(path: string, init: RequestInit = {}): Promise<Blob> {
  const headers = new Headers(init.headers);
  const locale = normalizeLocale(i18n.resolvedLanguage ?? i18n.language);

  if (!headers.has('Accept-Language')) {
    headers.set('Accept-Language', localeCultureMap[locale]);
  }

  const response = await fetch(`${environment.apiBaseUrl}${path}`, {
    ...init,
    headers,
  });

  if (!response.ok) {
    const payload: unknown = await response.json().catch(() => undefined);

    throw new ApiClientError(
      getErrorMessage(payload, i18n.t('errors.requestFailed')),
      response.status,
      getErrorCode(payload),
    );
  }

  return response.blob();
}

export function getApiErrorMessage(
  error: unknown,
  t: (
    key: string,
    options?: Record<string, string | number | boolean | null | undefined>,
  ) => string,
  fallbackKey = 'errors.requestFailed',
) {
  if (error instanceof ApiClientError && error.code) {
    const codeKey = `errors.api.${error.code}`;
    const translated = t(codeKey);

    if (translated !== codeKey) {
      return translated;
    }
  }

  return t(fallbackKey);
}
