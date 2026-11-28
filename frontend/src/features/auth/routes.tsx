
import type { AppRoute } from '@/app/router/types'
import { LoginPage } from './pages/LoginPage'

export const routes: AppRoute[] = [
  { path: '/login', element: () => <LoginPage /> },
]
