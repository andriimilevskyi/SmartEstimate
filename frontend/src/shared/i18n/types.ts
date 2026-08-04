export const locales = ['uk', 'en', 'de'] as const;

export type Locale = (typeof locales)[number];

export const defaultLocale: Locale = 'uk';

export const localeCultureMap: Record<Locale, string> = {
  de: 'de-DE',
  en: 'en-US',
  uk: 'uk-UA',
};

export function normalizeLocale(value: string | undefined | null): Locale {
  const language = value?.split('-')[0]?.toLowerCase();

  return locales.find((locale) => locale === language) ?? defaultLocale;
}
