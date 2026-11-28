
import type { ReactElement } from 'react'

export function ProductoDetallePage({ id }: { id: string }): ReactElement {
  return <h1 className="text-xl font-semibold">Detalle de producto {id}</h1>
}
