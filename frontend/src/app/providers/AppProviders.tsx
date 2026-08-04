import type { PropsWithChildren } from 'react';
import { useEffect } from 'react';

import { QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';

import { queryClient } from '@/shared/config/query-client';
import { localeCultureMap } from '@/shared/i18n/types';
import { useTranslation } from '@/shared/i18n/use-translation';

function I18nDocumentSync() {
  const { locale } = useTranslation();

  useEffect(() => {
    document.documentElement.lang = localeCultureMap[locale];
  }, [locale]);

  return null;
}

export function AppProviders({ children }: PropsWithChildren) {
  return (
    <QueryClientProvider client={queryClient}>
      <I18nDocumentSync />
      {children}
      <Toaster closeButton position="top-right" richColors />
    </QueryClientProvider>
  );
}
