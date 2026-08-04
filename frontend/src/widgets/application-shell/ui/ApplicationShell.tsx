import { FileText, Menu, PanelLeftClose, PanelLeftOpen } from 'lucide-react';
import { NavLink, Outlet, useLocation } from 'react-router-dom';

import { LanguageSwitcher } from '@/features/change-language/ui/LanguageSwitcher';
import { useTranslation } from '@/shared/i18n/use-translation';
import { useUiStore } from '@/shared/model/ui-store';
import { Button } from '@/shared/ui/button';

interface NavigationItem {
  key: string;
  to?: string;
}

const navigationItems: readonly NavigationItem[] = [
  { key: 'navigation.dashboard' },
  { key: 'navigation.customers' },
  { key: 'navigation.projects' },
  { key: 'navigation.estimates', to: '/estimates' },
  { key: 'navigation.knowledgeStudio', to: '/knowledge-studio' },
  { key: 'navigation.materials' },
  { key: 'navigation.pricing' },
  { key: 'navigation.reports' },
  { key: 'navigation.aiAssistant' },
  { key: 'navigation.settings' },
] as const;

export function ApplicationShell() {
  const { t } = useTranslation();
  const location = useLocation();
  const isSidebarCollapsed = useUiStore((state) => state.isSidebarCollapsed);
  const toggleSidebar = useUiStore((state) => state.toggleSidebar);

  return (
    <div className="min-h-screen bg-muted/40 text-foreground">
      <a className="skip-link" href="#workspace">
        {t('shell.skipToContent')}
      </a>
      <aside
        aria-label={t('shell.primaryNavigation')}
        className={`fixed inset-y-0 left-0 z-20 hidden border-r border-border bg-card transition-[width] duration-200 lg:flex lg:flex-col ${
          isSidebarCollapsed ? 'w-[72px]' : 'w-64'
        }`}
      >
        <div className="flex h-16 items-center justify-between border-b border-border px-3">
          <span
            className={`font-semibold tracking-tight text-foreground ${
              isSidebarCollapsed ? 'sr-only' : ''
            }`}
          >
            {t('shell.productName')}
          </span>
          <Button
            aria-label={isSidebarCollapsed ? t('shell.expandSidebar') : t('shell.collapseSidebar')}
            className="shrink-0"
            onClick={toggleSidebar}
            size="icon"
            variant="ghost"
          >
            {isSidebarCollapsed ? (
              <PanelLeftOpen aria-hidden="true" />
            ) : (
              <PanelLeftClose aria-hidden="true" />
            )}
          </Button>
        </div>
        <nav className="flex-1 space-y-1 p-3">
          {navigationItems.map((item) => {
            const itemContent = isSidebarCollapsed ? (
              item.to ? (
                <FileText aria-hidden="true" className="size-4" />
              ) : (
                <Menu aria-hidden="true" className="size-4" />
              )
            ) : (
              t(item.key)
            );
            const className = `flex h-11 items-center rounded-md px-3 text-sm transition-colors ${
              isSidebarCollapsed ? 'justify-center px-0' : ''
            }`;

            if (item.to) {
              return (
                <NavLink
                  className={({ isActive }) =>
                    `${className} ${isActive ? 'bg-accent font-medium text-accent-foreground' : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'}`
                  }
                  key={item.key}
                  title={isSidebarCollapsed ? t(item.key) : undefined}
                  to={item.to}
                >
                  {itemContent}
                </NavLink>
              );
            }

            return (
              <span
                aria-disabled="true"
                className={`${className} cursor-not-allowed text-muted-foreground/60`}
                key={item.key}
                title={isSidebarCollapsed ? t(item.key) : undefined}
              >
                {itemContent}
              </span>
            );
          })}
        </nav>
      </aside>

      <div className={isSidebarCollapsed ? 'lg:pl-[72px]' : 'lg:pl-64'}>
        <header className="flex h-16 items-center justify-between gap-4 border-b border-border bg-card px-4 sm:px-6">
          <p className="text-sm font-medium text-muted-foreground">
            {location.pathname.startsWith('/estimates')
              ? t('shell.estimates')
              : location.pathname.startsWith('/knowledge-studio')
                ? t('shell.knowledgeStudio')
                : t('shell.foundation')}
          </p>
          <LanguageSwitcher />
        </header>
        <main className="container py-8 sm:py-10" id="workspace" tabIndex={-1}>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
