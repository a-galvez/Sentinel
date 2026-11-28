
import { describe, expect, it } from 'vitest'
import { matchRoute } from './matchRoute'

describe('matchRoute', () => {
  it('coincidencia exacta', () => {
    expect(matchRoute('/productos', '/productos')).toEqual({})
  })

  it('captura :param', () => {
    expect(matchRoute('/productos/:id', '/productos/abc')).toEqual({ id: 'abc' })
  })

  it('no coincide con segmentos extra', () => {
    expect(matchRoute('/productos/:id', '/productos/3/expediente')).toBeNull()
  })
})
