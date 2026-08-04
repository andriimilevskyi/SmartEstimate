import { ArrowLeft, Building2, LoaderCircle, MapPin, Ruler } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';

import { useEstimateQuery } from '@/entities/estimate/api/estimate-queries';
import { DeleteEstimateButton } from '@/features/delete-estimate/ui/DeleteEstimateButton';
import { ApiClientError } from '@/shared/api/api-client';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';
import { EstimateEditor } from '@/widgets/estimate-editor/ui/EstimateEditor';

export function EstimateDetailsPage() {
  const { locale, t } = useTranslation();
  const navigate = useNavigate();
  const { estimateId } = useParams();
  const estimateQuery = useEstimateQuery(estimateId ?? '');

  if (!estimateId) {
    return null;
  }

  if (estimateQuery.isPending) {
    return (
      <div
        aria-live="polite"
        className="flex min-h-64 items-center justify-center gap-3 text-sm text-muted-foreground"
      >
        <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
        {t('estimates.states.loading')}
      </div>
    );
  }

  if (estimateQuery.isError || !estimateQuery.data) {
    const isNotFound =
      estimateQuery.error instanceof ApiClientError && estimateQuery.error.status === 404;

    return (
      <section
        aria-labelledby="estimate-error-title"
        className="grid min-h-[50vh] place-items-center text-center"
      >
        <div className="max-w-md space-y-4">
          <h1 className="text-2xl font-semibold" id="estimate-error-title">
            {isNotFound
              ? t('estimateDetails.notFoundTitle')
              : t('estimateDetails.unavailableTitle')}
          </h1>
          <p className="text-sm leading-6 text-muted-foreground">
            {isNotFound
              ? t('estimateDetails.notFoundDescription')
              : t('estimateDetails.unavailableDescription')}
          </p>
          <Button asChild>
            <Link to="/estimates">{t('estimateDetails.backToList')}</Link>
          </Button>
        </div>
      </section>
    );
  }

  const estimate = estimateQuery.data;
  const formattedArea =
    estimate.totalArea == null
      ? null
      : t('estimateDetails.objectArea', {
          area: new Intl.NumberFormat(locale, { maximumFractionDigits: 2 }).format(estimate.totalArea),
        });

  return (
    <section aria-labelledby="estimate-details-title" className="space-y-6">
      <Link
        className="inline-flex items-center gap-2 rounded-md text-sm font-medium text-muted-foreground hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        to="/estimates"
      >
        <ArrowLeft aria-hidden="true" className="size-4" />
        {t('estimateDetails.backToList')}
      </Link>

      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-start">
        <div className="space-y-2">
          <p className="text-sm font-medium text-primary">{t('estimateDetails.eyebrow')}</p>
          <h1 className="text-3xl font-semibold tracking-tight" id="estimate-details-title">
            {estimate.estimateNumber}
          </h1>
          <p className="text-sm text-muted-foreground">
            {t('estimateDetails.createdAt', { date: formatDate(estimate.createdAt, locale) })}
          </p>
          <div className="mt-3 flex flex-wrap gap-2 text-sm text-muted-foreground">
            <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
              <Building2 aria-hidden="true" className="size-4" />
              {t(`estimates.objectTypes.${estimate.objectType}`)}
            </span>
            {estimate.objectAddress ? (
              <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
                <MapPin aria-hidden="true" className="size-4" />
                {estimate.objectAddress}
              </span>
            ) : null}
            {formattedArea ? (
              <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
                <Ruler aria-hidden="true" className="size-4" />
                {formattedArea}
              </span>
            ) : null}
          </div>
        </div>
        <DeleteEstimateButton estimateId={estimate.id} onDeleted={() => navigate('/estimates')} />
      </div>

      {estimate.notes ? (
        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <h2 className="font-semibold">{t('estimateDetails.notes')}</h2>
          <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-muted-foreground">
            {estimate.notes}
          </p>
        </section>
      ) : null}

      <section aria-labelledby="estimate-editor-title" className="space-y-4">
        <div>
          <h2 className="text-xl font-semibold tracking-tight" id="estimate-editor-title">
            {t('estimateEditor.title')}
          </h2>
          <p className="mt-1 text-sm text-muted-foreground">{t('estimateEditor.description')}</p>
        </div>
        <EstimateEditor estimate={estimate} />
      </section>
    </section>
  );
}
