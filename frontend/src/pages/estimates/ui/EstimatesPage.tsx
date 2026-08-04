import { FileText, LoaderCircle, Plus } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { useEstimatesQuery } from '@/entities/estimate/api/estimate-queries';
import type { Estimate } from '@/entities/estimate/model/types';
import { CreateEstimateForm } from '@/features/create-estimate/ui/CreateEstimateForm';
import { DeleteEstimateButton } from '@/features/delete-estimate/ui/DeleteEstimateButton';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate, formatMoney } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';

export function EstimatesPage() {
  const { locale, t } = useTranslation();
  const navigate = useNavigate();
  const [isCreateFormOpen, setIsCreateFormOpen] = useState(false);
  const estimatesQuery = useEstimatesQuery();

  const openCreatedEstimate = (estimate: Estimate) => {
    setIsCreateFormOpen(false);
    navigate(`/estimates/${estimate.id}`);
  };

  return (
    <section aria-labelledby="estimates-title" className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div className="space-y-2">
          <p className="text-sm font-medium text-primary">{t('estimates.eyebrow')}</p>
          <h1 className="text-3xl font-semibold tracking-tight" id="estimates-title">
            {t('estimates.title')}
          </h1>
          <p className="max-w-2xl text-base leading-7 text-muted-foreground">
            {t('estimates.description')}
          </p>
        </div>
        <Button onClick={() => setIsCreateFormOpen(true)}>
          <Plus aria-hidden="true" className="size-4" />
          {t('estimates.create.action')}
        </Button>
      </div>

      {isCreateFormOpen ? (
        <CreateEstimateForm
          onCancel={() => setIsCreateFormOpen(false)}
          onCreated={openCreatedEstimate}
        />
      ) : null}

      <section
        aria-labelledby="estimate-list-title"
        className="overflow-hidden rounded-xl border border-border bg-card shadow-sm"
      >
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h2 className="font-semibold" id="estimate-list-title">
            {t('estimates.list.title')}
          </h2>
          {estimatesQuery.data ? (
            <span className="text-sm text-muted-foreground">
              {t('estimates.list.count', { count: estimatesQuery.data.totalCount })}
            </span>
          ) : null}
        </div>

        {estimatesQuery.isPending ? (
          <div
            aria-live="polite"
            className="flex min-h-56 items-center justify-center gap-3 px-5 text-sm text-muted-foreground"
          >
            <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
            {t('estimates.states.loading')}
          </div>
        ) : null}

        {estimatesQuery.isError ? (
          <div className="flex min-h-56 flex-col items-center justify-center gap-4 px-5 text-center">
            <p className="max-w-md text-sm leading-6 text-muted-foreground">
              {t('estimates.states.error')}
            </p>
            <Button onClick={() => void estimatesQuery.refetch()} variant="outline">
              {t('estimates.states.retry')}
            </Button>
          </div>
        ) : null}

        {!estimatesQuery.isPending &&
        !estimatesQuery.isError &&
        estimatesQuery.data?.items.length === 0 ? (
          <div className="flex min-h-56 flex-col items-center justify-center gap-4 px-5 text-center">
            <div className="rounded-full bg-muted p-3 text-muted-foreground">
              <FileText aria-hidden="true" className="size-6" />
            </div>
            <div className="space-y-1">
              <p className="font-medium">{t('estimates.states.emptyTitle')}</p>
              <p className="text-sm text-muted-foreground">
                {t('estimates.states.emptyDescription')}
              </p>
            </div>
            <Button onClick={() => setIsCreateFormOpen(true)} variant="outline">
              <Plus aria-hidden="true" className="size-4" />
              {t('estimates.create.action')}
            </Button>
          </div>
        ) : null}

        {!estimatesQuery.isPending &&
        !estimatesQuery.isError &&
        estimatesQuery.data?.items.length ? (
          <ul className="divide-y divide-border">
            {estimatesQuery.data.items.map((estimate) => {
              const formattedArea =
                estimate.totalArea == null
                  ? null
                  : t('estimateDetails.objectArea', {
                      area: new Intl.NumberFormat(locale, {
                        maximumFractionDigits: 2,
                      }).format(estimate.totalArea),
                    });

              return (
                <li className="flex items-center gap-3 px-5 py-4" key={estimate.id}>
                  <Link
                    className="min-w-0 flex-1 rounded-md text-left transition-colors hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                    to={`/estimates/${estimate.id}`}
                  >
                    <div className="flex flex-wrap items-center justify-between gap-x-6 gap-y-1">
                      <span className="font-medium">{estimate.estimateNumber}</span>
                      <span className="font-semibold tabular-nums">
                        {formatMoney(estimate.grandTotal, estimate.currency, locale)}
                      </span>
                    </div>
                    <div className="mt-1 flex flex-wrap gap-x-3 gap-y-1 text-sm text-muted-foreground">
                      <span>{t(`estimates.objectTypes.${estimate.objectType}`)}</span>
                      {estimate.objectAddress ? <span>{estimate.objectAddress}</span> : null}
                      {formattedArea ? <span>{formattedArea}</span> : null}
                      <span>{formatDate(estimate.createdAt, locale)}</span>
                    </div>
                  </Link>
                  <DeleteEstimateButton estimateId={estimate.id} />
                </li>
              );
            })}
          </ul>
        ) : null}
      </section>
    </section>
  );
}
