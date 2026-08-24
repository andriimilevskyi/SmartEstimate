import { BookOpenText, Building2, FileText, LoaderCircle, Users } from 'lucide-react';
import { Link } from 'react-router-dom';

import { useOverviewQuery } from '@/entities/overview/api/overview-queries';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate, formatMoney, formatNumber } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';

const metricItems = [
  { key: 'total', labelKey: 'overview.metrics.total' },
  { key: 'draft', labelKey: 'overview.metrics.draft' },
  { key: 'inProgress', labelKey: 'overview.metrics.inProgress' },
  { key: 'sent', labelKey: 'overview.metrics.sent' },
  { key: 'approved', labelKey: 'overview.metrics.approved' },
  { key: 'completed', labelKey: 'overview.metrics.completed' },
] as const;

const quickActions = [
  { icon: FileText, labelKey: 'overview.actions.createEstimate', to: '/estimates?create=estimate' },
  { icon: Building2, labelKey: 'overview.actions.createObject', to: '/objects?create=object' },
  { icon: Users, labelKey: 'overview.actions.addCustomer', to: '/customers?create=customer' },
  { icon: BookOpenText, labelKey: 'overview.actions.openKnowledgeStudio', to: '/knowledge-studio' },
] as const;

export function OverviewPage() {
  const { locale, t } = useTranslation();
  const overviewQuery = useOverviewQuery();

  return (
    <section aria-labelledby="overview-title" className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div className="space-y-2">
          <p className="text-sm font-medium text-primary">{t('overview.eyebrow')}</p>
          <h1 className="text-3xl font-semibold tracking-tight" id="overview-title">
            {t('overview.title')}
          </h1>
          <p className="max-w-2xl text-base leading-7 text-muted-foreground">
            {t('overview.description')}
          </p>
        </div>
      </div>

      {overviewQuery.isPending ? (
        <div className="flex min-h-64 items-center justify-center gap-3 rounded-xl border border-border bg-card text-sm text-muted-foreground">
          <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
          {t('overview.states.loading')}
        </div>
      ) : null}

      {overviewQuery.isError ? (
        <div className="grid min-h-64 place-items-center rounded-xl border border-border bg-card p-6 text-center">
          <div className="space-y-4">
            <p className="text-sm leading-6 text-muted-foreground">{t('overview.states.error')}</p>
            <Button onClick={() => void overviewQuery.refetch()} variant="outline">
              {t('actions.retry')}
            </Button>
          </div>
        </div>
      ) : null}

      {overviewQuery.data ? (
        <>
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-6">
            {metricItems.map((item) => (
              <article className="rounded-xl border border-border bg-card p-4 shadow-sm" key={item.key}>
                <p className="text-sm text-muted-foreground">{t(item.labelKey)}</p>
                <p className="mt-3 text-2xl font-semibold tabular-nums">
                  {formatNumber(overviewQuery.data.estimates[item.key], locale)}
                </p>
              </article>
            ))}
          </div>

          <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_minmax(18rem,0.38fr)]">
            <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
              <div className="flex items-center justify-between border-b border-border px-5 py-4">
                <h2 className="font-semibold">{t('overview.recentEstimates.title')}</h2>
                <Button asChild size="sm" variant="ghost">
                  <Link to="/estimates">{t('overview.openAll')}</Link>
                </Button>
              </div>
              {overviewQuery.data.recentEstimates.length === 0 ? (
                <div className="p-8 text-center text-sm text-muted-foreground">
                  {t('overview.recentEstimates.empty')}
                </div>
              ) : (
                <ul className="divide-y divide-border">
                  {overviewQuery.data.recentEstimates.map((estimate) => (
                    <li className="px-5 py-4" key={estimate.id}>
                      <Link className="block rounded-md hover:text-primary" to={`/estimates/${estimate.id}`}>
                        <div className="flex flex-wrap items-center justify-between gap-3">
                          <span className="font-medium">{estimate.estimateNumber}</span>
                          <span className="font-semibold tabular-nums">
                            {formatMoney(estimate.grandTotal, estimate.currency, locale)}
                          </span>
                        </div>
                        <div className="mt-1 flex flex-wrap gap-x-3 gap-y-1 text-sm text-muted-foreground">
                          <span>{estimate.object.customerName}</span>
                          <span>{estimate.object.name}</span>
                          <span>{t(`estimates.status.${estimate.status}`)}</span>
                          <span>{formatDate(estimate.updatedAt, locale)}</span>
                        </div>
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <aside className="space-y-6">
              <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
                <h2 className="font-semibold">{t('overview.quickActions.title')}</h2>
                <div className="mt-4 grid gap-2">
                  {quickActions.map((action) => {
                    const Icon = action.icon;

                    return (
                      <Button asChild className="justify-start" key={action.to} variant="outline">
                        <Link to={action.to}>
                          <Icon aria-hidden="true" className="size-4" />
                          {t(action.labelKey)}
                        </Link>
                      </Button>
                    );
                  })}
                </div>
              </section>

              <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
                <div className="flex items-center justify-between border-b border-border px-5 py-4">
                  <h2 className="font-semibold">{t('overview.recentObjects.title')}</h2>
                  <Button asChild size="sm" variant="ghost">
                    <Link to="/objects">{t('overview.openAll')}</Link>
                  </Button>
                </div>
                {overviewQuery.data.recentObjects.length === 0 ? (
                  <p className="p-5 text-sm text-muted-foreground">{t('overview.recentObjects.empty')}</p>
                ) : (
                  <ul className="divide-y divide-border">
                    {overviewQuery.data.recentObjects.map((estimateObject) => (
                      <li className="px-5 py-4" key={estimateObject.id}>
                        <Link className="block rounded-md hover:text-primary" to={`/objects/${estimateObject.id}`}>
                          <p className="font-medium">{estimateObject.name}</p>
                          <p className="mt-1 text-sm text-muted-foreground">
                            {estimateObject.customerName || t('overview.recentObjects.noCustomer')}
                          </p>
                        </Link>
                      </li>
                    ))}
                  </ul>
                )}
              </section>
            </aside>
          </div>
        </>
      ) : null}
    </section>
  );
}
