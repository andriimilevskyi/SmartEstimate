import { Settings } from 'lucide-react';

import { LanguageSwitcher } from '@/features/change-language/ui/LanguageSwitcher';
import { useTranslation } from '@/shared/i18n/use-translation';
import { useUiStore } from '@/shared/model/ui-store';

const fieldClassName =
  'flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring';

export function SettingsPage() {
  const { t } = useTranslation();
  const dateFormat = useUiStore((state) => state.dateFormat);
  const currencyFormat = useUiStore((state) => state.currencyFormat);
  const setDateFormat = useUiStore((state) => state.setDateFormat);
  const setCurrencyFormat = useUiStore((state) => state.setCurrencyFormat);

  return (
    <section aria-labelledby="settings-title" className="space-y-6">
      <div className="space-y-2">
        <p className="text-sm font-medium text-primary">{t('settings.eyebrow')}</p>
        <h1 className="text-3xl font-semibold tracking-tight" id="settings-title">
          {t('settings.title')}
        </h1>
        <p className="max-w-2xl text-base leading-7 text-muted-foreground">
          {t('settings.description')}
        </p>
      </div>

      <section className="max-w-3xl rounded-xl border border-border bg-card p-5 shadow-sm">
        <div className="flex items-start gap-3">
          <div className="rounded-lg bg-primary/10 p-2 text-primary">
            <Settings aria-hidden="true" className="size-5" />
          </div>
          <div>
            <h2 className="font-semibold">{t('settings.interface.title')}</h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {t('settings.interface.description')}
            </p>
          </div>
        </div>

        <div className="mt-5 grid gap-4 sm:grid-cols-3">
          <div className="space-y-2 text-sm font-medium">
            <span>{t('settings.interface.language')}</span>
            <LanguageSwitcher />
          </div>

          <label className="space-y-2 text-sm font-medium">
            <span>{t('settings.interface.dateFormat')}</span>
            <select
              className={fieldClassName}
              onChange={(event) => setDateFormat(event.target.value)}
              value={dateFormat}
            >
              <option value="short">{t('settings.dateFormats.short')}</option>
              <option value="medium">{t('settings.dateFormats.medium')}</option>
              <option value="long">{t('settings.dateFormats.long')}</option>
            </select>
          </label>

          <label className="space-y-2 text-sm font-medium">
            <span>{t('settings.interface.currencyFormat')}</span>
            <select
              className={fieldClassName}
              onChange={(event) => setCurrencyFormat(event.target.value)}
              value={currencyFormat}
            >
              <option value="UAH">UAH</option>
              <option value="EUR">EUR</option>
              <option value="USD">USD</option>
            </select>
          </label>
        </div>
      </section>
    </section>
  );
}
