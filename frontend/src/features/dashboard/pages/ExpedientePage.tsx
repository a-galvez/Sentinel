
import type { ReactElement } from 'react'

export function ExpedientePage({ id }: { id: string }): ReactElement {
  return <h1 className="text-xl font-semibold">Expediente del producto {id}</h1>
}
