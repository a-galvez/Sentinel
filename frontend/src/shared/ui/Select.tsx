import { useId } from 'react'
import type { ReactElement, ReactNode, Ref, SelectHTMLAttributes } from 'react'

type Props = SelectHTMLAttributes<HTMLSelectElement> & {
  label: string
  error?: string
  children: ReactNode
  ref?: Ref<HTMLSelectElement>
}

export function Select({
  label,
  error,
  id,
  children,
  className = '',
  ref,
  ...props
}: Props): ReactElement {
  const autoId = useId()
  const selectId = id ?? autoId

  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={selectId} className="text-sm font-medium text-slate-700">
        {label}
      </label>
      <select
        ref={ref}
        id={selectId}
        className={`rounded-md border px-3 py-2 text-sm outline-none focus:border-teal-600 ${error ? 'border-red-500' : 'border-slate-300'} ${className}`}
        {...props}
      >
        {children}
      </select>
      {error && <span className="text-xs text-red-600">{error}</span>}
    </div>
  )
}
