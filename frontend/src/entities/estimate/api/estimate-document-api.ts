import type { EstimateDocumentTemplate } from '@/entities/estimate/model/types';
import { apiBlobRequest, apiRequest } from '@/shared/api/api-client';
import { environment } from '@/shared/config/environment';
import type { Locale } from '@/shared/i18n/types';

const estimatesPath = '/v1/estimates';

export function getEstimateDocumentTemplates(locale: Locale, signal?: AbortSignal) {
  const searchParams = new URLSearchParams({ locale });

  return apiRequest<EstimateDocumentTemplate[]>(
    `${estimatesPath}/document-templates?${searchParams}`,
    {
      signal,
    },
  );
}

export function getEstimatePdfDocument(
  estimateId: string,
  templateCode: string,
  locale: Locale,
  signal?: AbortSignal,
) {
  const searchParams = new URLSearchParams({ locale, template: templateCode });

  return apiBlobRequest(`${estimatesPath}/${estimateId}/documents/pdf?${searchParams}`, { signal });
}

export function getEstimatePdfDocumentUrl(
  estimateId: string,
  templateCode: string,
  locale: Locale,
) {
  const searchParams = new URLSearchParams({
    disposition: 'inline',
    locale,
    template: templateCode,
  });

  return `${window.location.origin}${environment.apiBaseUrl}${estimatesPath}/${estimateId}/documents/pdf?${searchParams}`;
}
