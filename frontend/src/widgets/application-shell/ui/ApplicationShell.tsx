import {
  BarChart3,
  Bot,
  BookOpenText,
  FileText,
  FolderKanban,
  LayoutDashboard,
  Package,
  PanelLeftClose,
  PanelLeftOpen,
  Settings,
  Tags,
  Users,
} from 'lucide-react';
import type { LucideIcon } from 'lucide-react';
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom';

import { LanguageSwitcher } from '@/features/change-language/ui/LanguageSwitcher';
import { useTranslation } from '@/shared/i18n/use-translation';
import { useUiStore } from '@/shared/model/ui-store';
import { Button } from '@/shared/ui/button';

interface NavigationItem {
  icon: LucideIcon;
  key: string;
  to: string;
}

const navigationItems: readonly NavigationItem[] = [
  { icon: LayoutDashboard, key: 'navigation.dashboard', to: '/overview' },
  { icon: Users, key: 'navigation.customers', to: '/customers' },
  { icon: FolderKanban, key: 'navigation.projects', to: '/objects' },
  { icon: FileText, key: 'navigation.estimates', to: '/estimates' },
  { icon: BookOpenText, key: 'navigation.knowledgeStudio', to: '/knowledge-studio' },
  { icon: Package, key: 'navigation.materials', to: '/materials' },
  { icon: Tags, key: 'navigation.pricing', to: '/pricing' },
  { icon: BarChart3, key: 'navigation.reports', to: '/reports' },
  { icon: Bot, key: 'navigation.aiAssistant', to: '/ai-assistant' },
  { icon: Settings, key: 'navigation.settings', to: '/settings' },
] as const;

const pageTitleKeyByPath = [
  { key: 'shell.overview', path: '/overview' },
  { key: 'shell.customers', path: '/customers' },
  { key: 'shell.objects', path: '/objects' },
  { key: 'shell.estimates', path: '/estimates' },
  { key: 'shell.knowledgeStudio', path: '/knowledge-studio' },
  { key: 'shell.materials', path: '/materials' },
  { key: 'shell.pricing', path: '/pricing' },
  { key: 'shell.reports', path: '/reports' },
  { key: 'shell.aiAssistant', path: '/ai-assistant' },
  { key: 'shell.settings', path: '/settings' },
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
          <Link
            className={`rounded-md font-semibold tracking-tight text-foreground hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring ${
              isSidebarCollapsed ? 'sr-only' : ''
            }`}
            to="/overview"
          >
            {t('shell.productName')}
          </Link>
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
            const Icon = item.icon;
            const itemContent = isSidebarCollapsed ? (
              <Icon aria-hidden="true" className="size-4" />
            ) : (
              <>
                <Icon aria-hidden="true" className="size-4" />
                <span>{t(item.key)}</span>
              </>
            );
            const className = `flex h-11 items-center rounded-md px-3 text-sm transition-colors ${
              isSidebarCollapsed ? 'justify-center px-0' : 'gap-3'
            }`;

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
          })}
        </nav>
      </aside>

      <div className={isSidebarCollapsed ? 'lg:pl-[72px]' : 'lg:pl-64'}>
        <header className="flex h-16 items-center justify-between gap-4 border-b border-border bg-card px-4 sm:px-6">
          <p className="text-sm font-medium text-muted-foreground">
            {t(
              pageTitleKeyByPath.find((item) => location.pathname.startsWith(item.path))?.key ??
                'shell.foundation',
            )}
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
