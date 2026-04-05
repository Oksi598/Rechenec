/**
 * База API: порожньо в dev (запити йдуть на origin Vite, проксі на Kestrel).
 * Для прев’ю/прод окремого хоста: VITE_API_BASE=https://api.example.com
 */
const raw = (import.meta.env.VITE_API_BASE ?? '').trim().replace(/\/$/, '')

export function apiUrl(path) {
  if (!path) return raw
  if (path.startsWith('http://') || path.startsWith('https://')) return path
  return `${raw}${path}`
}

/** Порожній рядок = відносні URL (той самий origin, що й сторінка). */
export function getApiOrigin() {
  return raw
}
