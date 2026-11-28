
import type { ReactElement } from 'react'

export type AppRoute = {
  path: string
  element: (params: Record<string, string>) => ReactElement
  protegida?: boolean
}

export function navigate(path: string): void {
  window.location.hash = '#' + path
}
