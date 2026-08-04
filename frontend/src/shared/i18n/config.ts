import i18n from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { initReactI18next } from 'react-i18next';

import commonDe from '@/shared/i18n/locales/de/common.json';
import estimateDe from '@/shared/i18n/locales/de/estimate.json';
import knowledgeDe from '@/shared/i18n/locales/de/knowledge.json';
import navigationDe from '@/shared/i18n/locales/de/navigation.json';
import validationDe from '@/shared/i18n/locales/de/validation.json';
import commonEn from '@/shared/i18n/locales/en/common.json';
import estimateEn from '@/shared/i18n/locales/en/estimate.json';
import knowledgeEn from '@/shared/i18n/locales/en/knowledge.json';
import navigationEn from '@/shared/i18n/locales/en/navigation.json';
import validationEn from '@/shared/i18n/locales/en/validation.json';
import commonUk from '@/shared/i18n/locales/uk/common.json';
import estimateUk from '@/shared/i18n/locales/uk/estimate.json';
import knowledgeUk from '@/shared/i18n/locales/uk/knowledge.json';
import navigationUk from '@/shared/i18n/locales/uk/navigation.json';
import validationUk from '@/shared/i18n/locales/uk/validation.json';
import { defaultLocale, locales } from '@/shared/i18n/types';

export const i18nNamespaces = ['common', 'navigation', 'estimate', 'knowledge', 'validation'];
const fallbackNamespaces = i18nNamespaces.filter((namespace) => namespace !== 'common');

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    detection: {
      caches: ['localStorage'],
      lookupLocalStorage: 'smartestimate.locale',
      order: ['localStorage', 'navigator', 'htmlTag'],
    },
    fallbackLng: [defaultLocale, 'en'],
    fallbackNS: fallbackNamespaces,
    interpolation: {
      escapeValue: false,
    },
    load: 'languageOnly',
    ns: i18nNamespaces,
    defaultNS: 'common',
    resources: {
      de: {
        common: commonDe,
        estimate: estimateDe,
        knowledge: knowledgeDe,
        navigation: navigationDe,
        validation: validationDe,
      },
      en: {
        common: commonEn,
        estimate: estimateEn,
        knowledge: knowledgeEn,
        navigation: navigationEn,
        validation: validationEn,
      },
      uk: {
        common: commonUk,
        estimate: estimateUk,
        knowledge: knowledgeUk,
        navigation: navigationUk,
        validation: validationUk,
      },
    },
    supportedLngs: [...locales],
  });

export { i18n };
