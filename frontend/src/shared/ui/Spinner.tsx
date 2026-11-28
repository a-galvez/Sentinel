
import type { ReactElement } from 'react'

export function Spinner(): ReactElement {
  return (
    <div role="status" aria-label="Cargando" className="flex items-center gap-2 text-sm text-slate-500">
      <span className="h-5 w-5 animate-spin rounded-full border-2 border-slate-300 border-t-teal-700" />
      Cargando…
    </div>
  )
}
