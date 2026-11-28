import type { ReactElement } from 'react'

export type DocumentoOrigen = 'registro' | 'licencia'

export function DocumentoPanel({ origen, id }: { origen: DocumentoOrigen; id: string }): ReactElement {
  return (
    <div className="rounded-md border border-dashed border-slate-300 p-4 text-sm text-slate-500">
      Documento {origen} (id {id})
    </div>
  )
}
