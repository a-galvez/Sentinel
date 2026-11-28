import { useCallback, useEffect, useState } from 'react'
import type { ZodType } from 'zod'
import type { ApiError } from './errors'
import { request } from './request'

type Estado<T> = {
  url: string | null
  tick: number
  data: T | null
  error: ApiError | null
  listo: boolean
}

function inicial<T>(): Estado<T> {
  return { url: null, tick: 0, data: null, error: null, listo: false }
}

export function useFetch<T>(url: string | null, schema?: ZodType<T>) {
  const [tick, setTick] = useState(0)
  const [estado, setEstado] = useState<Estado<T>>(inicial)

  useEffect(() => {
    if (!url) return

    let vivo = true

    request<T>({ url, method: 'GET' }, schema)
      .then((data) => {
        if (vivo) setEstado({ url, tick, data, error: null, listo: true })
      })
      .catch((error: ApiError) => {
        if (vivo) setEstado((e) => ({ ...e, url, tick, error, listo: true }))
      })

    return () => {
      vivo = false
    }
  }, [url, schema, tick])

  const reload = useCallback(() => setTick((n) => n + 1), [])

  const actual = estado.url === url && estado.tick === tick

  return {
    data: estado.data,
    loading: url !== null && !(actual && estado.listo),
    error: actual ? estado.error : null,
    reload,
  }
}
