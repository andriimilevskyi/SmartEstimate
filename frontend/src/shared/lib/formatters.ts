import { localeCultureMap, type Locale } from '@/shared/i18n/types';

export function formatDate(value: string, locale: Locale) {
  return new Intl.DateTimeFormat(localeCultureMap[locale], {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value));
}

export function formatMoney(value: number, currency: string, locale: Locale) {
  return new Intl.NumberFormat(localeCultureMap[locale], {
    currency,
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency',
  }).format(value);
}

export function formatNumber(value: number, locale: Locale) {
  return new Intl.NumberFormat(localeCultureMap[locale], {
    maximumFractionDigits: 2,
  }).format(value);
}
