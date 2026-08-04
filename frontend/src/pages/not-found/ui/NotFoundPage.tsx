import { Link } from 'react-router-dom';

import { Button } from '@/shared/ui/button';
import { useTranslation } from '@/shared/i18n/use-translation';

export function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <section aria-labelledby="not-found-title" className="grid min-h-[60vh] place-items-center">
      <div className="max-w-md space-y-4 text-center">
        <p className="text-sm font-medium text-primary">{t('notFound.code')}</p>
        <h1 id="not-found-title" className="text-3xl font-semibold tracking-tight">
          {t('notFound.title')}
        </h1>
        <p className="text-muted-foreground">{t('notFound.description')}</p>
        <Button asChild>
          <Link to="/estimates">{t('notFound.action')}</Link>
        </Button>
      </div>
    </section>
  );
}
