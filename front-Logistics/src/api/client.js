const TOKEN_KEY = 'tms_token'

export function getToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token)
  else localStorage.removeItem(TOKEN_KEY)
}

export function authHeaders() {
  const t = getToken()
  if (!t) return {}
  return { Authorization: `Bearer ${t}` }
}

/** Multipart form (без Content-Type — boundary виставить браузер). */
export async function apiForm(path, formData) {
  const res = await fetch(path, {
    method: 'POST',
    headers: authHeaders(),
    body: formData,
  })

  if (
    res.status === 401 &&
    getToken() &&
    !path.includes('/api/auth/login') &&
    !path.includes('/api/auth/register')
  ) {
    setToken(null)
    window.dispatchEvent(new CustomEvent('tms:unauthorized'))
  }

  const text = await res.text()
  let data = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = text
    }
  }

  if (!res.ok) {
    const msg =
      typeof data === 'object' && data !== null
        ? data.message || (Array.isArray(data.errors) ? data.errors.join('; ') : null)
        : null
    const err = new Error(msg || `HTTP ${res.status}`)
    err.status = res.status
    err.body = data
    throw err
  }

  return data
}

/**
 * @param {string} path — відносний, напр. /api/auth/login
 * @param {RequestInit & { json?: unknown }} [options]
 */
export async function api(path, options = {}) {
  const { json, ...rest } = options
  const headers = { ...authHeaders(), ...rest.headers }

  let body = rest.body
  if (json !== undefined) {
    headers['Content-Type'] = 'application/json'
    body = JSON.stringify(json)
  }

  const res = await fetch(path, { ...rest, headers, body })

  if (
    res.status === 401 &&
    getToken() &&
    !path.includes('/api/auth/login') &&
    !path.includes('/api/auth/register')
  ) {
    setToken(null)
    window.dispatchEvent(new CustomEvent('tms:unauthorized'))
  }

  const text = await res.text()
  let data = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = text
    }
  }

  if (!res.ok) {
    const msg =
      typeof data === 'object' && data !== null
        ? data.message || (Array.isArray(data.errors) ? data.errors.join('; ') : null)
        : null
    const err = new Error(msg || `HTTP ${res.status}`)
    err.status = res.status
    err.body = data
    throw err
  }

  return data
}
