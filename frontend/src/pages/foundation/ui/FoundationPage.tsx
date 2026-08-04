import { useTranslation } from '@/shared/i18n/use-translation';

export function FoundationPage() {
  const { t } = useTranslation();

  return (
    <section aria-labelledby="foundation-title" className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm font-medium text-primary">{t('foundation.eyebrow')}</p>
        <h1 id="foundation-title" className="text-3xl font-semibold tracking-tight text-foreground">
          {t('foundation.title')}
        </h1>
        <p className="max-w-2xl text-base leading-7 text-muted-foreground">
          {t('foundation.description')}
        </p>
      </div>

      <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
        <h2 className="text-base font-semibold text-card-foreground">
          {t('foundation.statusTitle')}
        </h2>
        <p className="mt-2 text-sm leading-6 text-muted-foreground">
          {t('foundation.statusDescription')}
        </p>
      </div>
    </section>
  );
}
