import axios from 'axios'
import type { AxiosRequestConfig } from 'axios'
import type { ZodType } from 'zod'
import { ApiError } from './errors'
import { api } from './http'


type ProblemDetails = {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

const MENSAJE_GENERICO = 'No se pudo conectar con el servidor.'

export async function request<T>(
  config: AxiosRequestConfig,
  schema?: ZodType<T>,
): Promise<T> {
  try {
    const res = await api.request(config)

    if (schema) {
      const parsed = schema.safeParse(res.data)
      if (!parsed.success) {
        throw new ApiError(0, 'Respuesta con formato inesperado')
      }
      return parsed.data
    }

    return res.data as T
  } catch (err) {
    if (err instanceof ApiError) throw err
    if (axios.isAxiosError<ProblemDetails>(err)) {
      const problem = err.response?.data
      const mensaje =
        problem?.detail ??
        problem?.title ??
        (err.response ? 'Ocurrió un error inesperado.' : MENSAJE_GENERICO)
      throw new ApiError(
        err.response?.status ?? 0,
        mensaje,
        problem?.errors,
      )
    }
    throw err
  }
}
