
import { useEffect, useState } from 'react'
import type { ReactElement } from 'react'
import { matchRoute } from './matchRoute'
import type { AppRoute } from './types'

function NotFound(): ReactElement {
  return (
    <section className="p-8 text-center">
      <h1 className="text-2xl font-semibold text-slate-800">404</h1>
      <p className="text-slate-500">La página que buscas no existe.</p>
    </section>
  )
}

function rutActual(): string {
  const hash = window.location.hash.replace(/^#/, '')
  return hash || '/'
}

export function Router({ routes }: { routes: AppRoute[] }) {
  const [path, setPath] = useState(rutActual)

  useEffect(() => {
    const onHash = () => setPath(rutActual())
    window.addEventListener('hashchange', onHash)
    return () => window.removeEventListener('hashchange', onHash)
  }, [])

  for (const ruta of routes) {
    const params = matchRoute(ruta.path, path)
    if (params) {
      // TODO hito 2: respetar ruta.protegida
      return ruta.element(params)
    }
  }

  return <NotFound />
}
