
import type { AppRoute } from '@/app/router/types'
import { EstablecimientoDetallePage } from './pages/EstablecimientoDetallePage'
import { EstablecimientosPage } from './pages/EstablecimientosPage'

export const routes: AppRoute[] = [
  { path: '/establecimientos', element: () => <EstablecimientosPage /> },
  { path: '/establecimientos/:id', element: ({ id }) => <EstablecimientoDetallePage id={id} /> },
]
