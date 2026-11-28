
import type { ReactElement } from 'react'
import type { EstadoVencimiento } from '@/shared/lib/estados'

const config: Record<EstadoVencimiento, { texto: string; clase: string }> = {
  NoVerificado: { texto: 'No verificado', clase: 'bg-slate-100 text-slate-700' },
  EnTramite: { texto: 'En trámite', clase: 'bg-blue-100 text-blue-800' },
  Vigente: { texto: 'Vigente', clase: 'bg-green-100 text-green-800' },
  PorVencer: { texto: 'Por vencer', clase: 'bg-amber-100 text-amber-800' },
  Vencido: { texto: 'Vencido', clase: 'bg-red-100 text-red-800' },
}

export function EstadoBadge({ estado }: { estado: EstadoVencimiento }): ReactElement {
  const { texto, clase } = config[estado]
  return (
    <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium ${clase}`}>
      {texto}
    </span>
  )
}
