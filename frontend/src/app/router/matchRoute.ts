
function segmentos(path: string): string[] {
  return path.split('/').filter(Boolean)
}

export function matchRoute(
  pattern: string,
  path: string,
): Record<string, string> | null {
  const p = segmentos(pattern)
  const s = segmentos(path)
  if (p.length !== s.length) return null

  const params: Record<string, string> = {}
  for (let i = 0; i < p.length; i++) {
    if (p[i].startsWith(':')) {
      params[p[i].slice(1)] = s[i]
    } else if (p[i] !== s[i]) {
      return null
    }
  }
  return params
}
