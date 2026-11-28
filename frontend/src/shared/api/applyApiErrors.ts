
import type { FieldValues, UseFormSetError } from 'react-hook-form'
import { ApiError } from './errors'

export function applyApiErrors<T extends FieldValues>(
  err: ApiError,
  setError: UseFormSetError<T>,
): void {
  if (!err.errors || Object.keys(err.errors).length === 0) {
    setError('root', { message: err.message })
    return
  }

  for (const [campo, mensajes] of Object.entries(err.errors)) {
    const nombre = (campo.charAt(0).toLowerCase() + campo.slice(1)) as Parameters<
      UseFormSetError<T>
    >[0]
    setError(nombre, { message: mensajes.join('. ') })
  }
}
