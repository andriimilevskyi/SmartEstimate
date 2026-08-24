import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';

import { ComingSoonPage } from '@/pages/coming-soon/ui/ComingSoonPage';
import { CustomerDetailsPage } from '@/pages/customers/ui/CustomerDetailsPage';
import { CustomersPage } from '@/pages/customers/ui/CustomersPage';
import { EstimateDetailsPage } from '@/pages/estimate-details/ui/EstimateDetailsPage';
import { EstimatesPage } from '@/pages/estimates/ui/EstimatesPage';
import { FoundationPage } from '@/pages/foundation/ui/FoundationPage';
import { KnowledgeStudioPage } from '@/pages/knowledge-studio/ui/KnowledgeStudioPage';
import { MaterialsPage } from '@/pages/materials/ui/MaterialsPage';
import { NotFoundPage } from '@/pages/not-found/ui/NotFoundPage';
import { ObjectsPage } from '@/pages/objects/ui/ObjectsPage';
import { ObjectDetailsPage } from '@/pages/object-details/ui/ObjectDetailsPage';
import { OverviewPage } from '@/pages/overview/ui/OverviewPage';
import { PricingPage } from '@/pages/pricing/ui/PricingPage';
import { SettingsPage } from '@/pages/settings/ui/SettingsPage';
import { ApplicationShell } from '@/widgets/application-shell/ui/ApplicationShell';

const router = createBrowserRouter([
  {
    element: <ApplicationShell />,
    children: [
      {
        index: true,
        element: <Navigate replace to="/overview" />,
      },
      {
        path: 'overview',
        element: <OverviewPage />,
      },
      {
        path: 'customers',
        element: <CustomersPage />,
      },
      {
        path: 'customers/:customerId',
        element: <CustomerDetailsPage />,
      },
      {
        path: 'objects',
        element: <ObjectsPage />,
      },
      {
        path: 'estimates',
        element: <EstimatesPage />,
      },
      {
        path: 'estimates/:estimateId',
        element: <EstimateDetailsPage />,
      },
      {
        path: 'foundation',
        element: <FoundationPage />,
      },
      {
        path: 'knowledge-studio',
        element: <KnowledgeStudioPage />,
      },
      {
        path: 'objects/:objectId',
        element: <ObjectDetailsPage />,
      },
      {
        path: 'materials',
        element: <MaterialsPage />,
      },
      {
        path: 'pricing',
        element: <PricingPage />,
      },
      {
        path: 'reports',
        element: (
          <ComingSoonPage
            descriptionKey="placeholders.reports.description"
            titleKey="placeholders.reports.title"
          />
        ),
      },
      {
        path: 'ai-assistant',
        element: (
          <ComingSoonPage
            descriptionKey="placeholders.aiAssistant.description"
            titleKey="placeholders.aiAssistant.title"
          />
        ),
      },
      {
        path: 'settings',
        element: <SettingsPage />,
      },
      {
        path: '*',
        element: <NotFoundPage />,
      },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
