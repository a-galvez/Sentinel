
import axios from 'axios'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ApiError } from './errors'
import { api } from './http'
import { request } from './request'

afterEach(() => vi.restoreAllMocks())

describe('request', () => {
  it('convierte un error de ProblemDetails en ApiError', async () => {
    const error = new axios.AxiosError(
      'Request failed',
      'ERR_BAD_RESPONSE',
      undefined,
      undefined,
      {
        status: 400,
        statusText: 'Bad Request',
        headers: {},
        config: {} as never,
        data: {
          title: 'Datos inválidos',
          detail: 'Uno o más campos no son válidos.',
          errors: { nombre: ['Requerido'] },
        },
      },
    )
    vi.spyOn(api, 'request').mockRejectedValue(error)

    const rechazo = request({ url: '/x' })
    await expect(rechazo).rejects.toBeInstanceOf(ApiError)
    await expect(rechazo).rejects.toMatchObject({
      status: 400,
      message: 'Uno o más campos no son válidos.',
      errors: { nombre: ['Requerido'] },
    })
  })

  it('un 204 devuelve undefined', async () => {
    vi.spyOn(api, 'request').mockResolvedValue({ status: 204, data: undefined } as never)
    await expect(request({ url: '/x', method: 'DELETE' })).resolves.toBeUndefined()
  })
})
