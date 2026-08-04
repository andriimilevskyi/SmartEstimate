import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';

import { EstimateDetailsPage } from '@/pages/estimate-details/ui/EstimateDetailsPage';
import { EstimatesPage } from '@/pages/estimates/ui/EstimatesPage';
import { FoundationPage } from '@/pages/foundation/ui/FoundationPage';
import { KnowledgeStudioPage } from '@/pages/knowledge-studio/ui/KnowledgeStudioPage';
import { NotFoundPage } from '@/pages/not-found/ui/NotFoundPage';
import { ApplicationShell } from '@/widgets/application-shell/ui/ApplicationShell';

const router = createBrowserRouter([
  {
    element: <ApplicationShell />,
    children: [
      {
        index: true,
        element: <Navigate replace to="/estimates" />,
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
        path: '*',
        element: <NotFoundPage />,
      },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
