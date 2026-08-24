import { ArrowRight, Construction } from 'lucide-react';
import { Link } from 'react-router-dom';

import { useTranslation } from '@/shared/i18n/use-translation';
import { Button } from '@/shared/ui/button';

interface ComingSoonPageProps {
  actionTo?: string;
  descriptionKey: string;
  titleKey: string;
}

export function ComingSoonPage({ actionTo, descriptionKey, titleKey }: ComingSoonPageProps) {
  const { t } = useTranslation();

  return (
    <section aria-labelledby="coming-soon-title" className="mx-auto max-w-3xl">
      <div className="rounded-xl border border-border bg-card p-8 shadow-sm">
        <div className="flex flex-col gap-5 sm:flex-row sm:items-start">
          <div className="rounded-lg bg-primary/10 p-3 text-primary">
            <Construction aria-hidden="true" className="size-6" />
          </div>
          <div className="min-w-0 flex-1 space-y-3">
            <p className="text-sm font-medium text-primary">{t('comingSoon.eyebrow')}</p>
            <h1 className="text-2xl font-semibold tracking-tight" id="coming-soon-title">
              {t(titleKey)}
            </h1>
            <p className="text-sm leading-6 text-muted-foreground">{t(descriptionKey)}</p>
            {actionTo ? (
              <Button asChild variant="outline">
                <Link to={actionTo}>
                  {t('comingSoon.openKnowledgeStudio')}
                  <ArrowRight aria-hidden="true" className="size-4" />
                </Link>
              </Button>
            ) : null}
          </div>
        </div>
      </div>
    </section>
  );
}
