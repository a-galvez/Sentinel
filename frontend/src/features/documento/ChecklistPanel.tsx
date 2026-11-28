import type { ReactElement } from 'react'
import type { DocumentoOrigen } from './DocumentoPanel'

export function ChecklistPanel({ origen, id }: { origen: DocumentoOrigen; id: string }): ReactElement {
  return (
    <div className="rounded-md border border-dashed border-slate-300 p-4 text-sm text-slate-500">
      Checklist {origen} (id {id})
    </div>
  )
}
