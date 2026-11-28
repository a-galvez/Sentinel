
import type { AppRoute } from '@/app/router/types'
import { ProductoDetallePage } from './pages/ProductoDetallePage'
import { ProductosPage } from './pages/ProductosPage'

export const routes: AppRoute[] = [
  { path: '/productos', element: () => <ProductosPage /> },
  { path: '/productos/:id', element: ({ id }) => <ProductoDetallePage id={id} /> },
]
