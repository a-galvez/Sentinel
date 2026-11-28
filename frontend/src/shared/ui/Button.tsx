
import type { ButtonHTMLAttributes, ReactElement } from 'react'

type Variante = 'primary' | 'secondary' | 'danger'

const estilos: Record<Variante, string> = {
  primary: 'bg-teal-700 text-white hover:bg-teal-800',
  secondary: 'bg-white text-slate-700 border border-slate-300 hover:bg-slate-100',
  danger: 'bg-red-600 text-white hover:bg-red-700',
}

type Props = ButtonHTMLAttributes<HTMLButtonElement> & { variant?: Variante }

export function Button({ variant = 'primary', className = '', ...props }: Props): ReactElement {
  return (
    <button
      className={`inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium disabled:opacity-50 ${estilos[variant]} ${className}`}
      {...props}
    />
  )
}
