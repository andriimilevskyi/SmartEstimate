import { useTranslation as useI18nextTranslation } from 'react-i18next';

import { i18nNamespaces } from '@/shared/i18n/config';
import { normalizeLocale, type Locale } from '@/shared/i18n/types';

type TranslationOptions = Record<string, string | number | boolean | null | undefined>;

export function useTranslation() {
  const { i18n, t } = useI18nextTranslation(i18nNamespaces);
  const locale = normalizeLocale(i18n.resolvedLanguage ?? i18n.language);

  return {
    locale,
    changeLocale: (nextLocale: Locale) => i18n.changeLanguage(nextLocale),
    t: (key: string, options?: TranslationOptions) => t(key, options),
  };
}
