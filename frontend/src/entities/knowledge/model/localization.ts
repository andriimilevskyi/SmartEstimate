import type { Locale } from '@/shared/i18n/types';

import type { LocalizedText } from '@/entities/knowledge/model/types';

export function getLocalizedText(value: LocalizedText, locale: Locale) {
  return value[locale] || value.en || value.uk;
}
