
import type { AppRoute } from '@/app/router/types'
import { DashboardPage } from './pages/DashboardPage'
import { ExpedientePage } from './pages/ExpedientePage'

export const routes: AppRoute[] = [
  { path: '/', element: () => <DashboardPage /> },
  { path: '/productos/:id/expediente', element: ({ id }) => <ExpedientePage id={id} /> },
]
