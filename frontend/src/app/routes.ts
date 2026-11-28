
import type { AppRoute } from './router/types'
import { routes as auth } from '@/features/auth/routes'
import { routes as dashboard } from '@/features/dashboard/routes'
import { routes as documento } from '@/features/documento/routes'
import { routes as establecimientos } from '@/features/establecimientos/routes'
import { routes as productos } from '@/features/productos/routes'

export const routes: AppRoute[] = [
  ...auth,
  ...dashboard,
  ...productos,
  ...establecimientos,
  ...documento,
]
