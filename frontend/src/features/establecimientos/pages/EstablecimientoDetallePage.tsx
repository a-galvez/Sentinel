import type { ReactElement } from 'react'

export function EstablecimientoDetallePage({ id }: { id: string }): ReactElement {
  return <h1 className="text-xl font-semibold">Detalle de establecimiento {id}</h1>
}
