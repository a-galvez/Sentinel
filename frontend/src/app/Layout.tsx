
import type { ReactElement, ReactNode } from 'react'

const enlaces = [
  { href: '#/', texto: 'Inicio' },
  { href: '#/productos', texto: 'Productos' },
  { href: '#/establecimientos', texto: 'Establecimientos' },
]

export function Layout({ children }: { children: ReactNode }): ReactElement {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-800">
      <nav className="flex gap-4 border-b border-slate-200 bg-white px-6 py-3">
        {enlaces.map((e) => (
          <a key={e.href} href={e.href} className="text-sm font-medium hover:text-teal-700">
            {e.texto}
          </a>
        ))}
      </nav>
      <main className="mx-auto max-w-5xl p-6">{children}</main>
    </div>
  )
}
