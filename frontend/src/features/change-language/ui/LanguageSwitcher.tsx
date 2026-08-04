import { Languages } from 'lucide-react';

import { locales, type Locale } from '@/shared/i18n/types';
import { useTranslation } from '@/shared/i18n/use-translation';

const selectClassName =
  'h-9 rounded-md border border-input bg-background px-2 text-sm shadow-sm outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring';

export function LanguageSwitcher() {
  const { changeLocale, locale, t } = useTranslation();

  return (
    <label className="inline-flex items-center gap-2 text-sm text-muted-foreground">
      <Languages aria-hidden="true" className="size-4" />
      <span className="sr-only">{t('language.label')}</span>
      <select
        aria-label={t('language.label')}
        className={selectClassName}
        onChange={(event) => void changeLocale(event.target.value as Locale)}
        value={locale}
      >
        {locales.map((availableLocale) => (
          <option key={availableLocale} value={availableLocale}>
            {t(`language.options.${availableLocale}`)}
          </option>
        ))}
      </select>
    </label>
  );
}
