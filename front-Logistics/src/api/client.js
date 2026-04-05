import { apiUrl } from '../config.js'

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
  const tokenUsed = getToken()
  const res = await fetch(apiUrl(path), {
    method: 'POST',
    headers: authHeaders(),
    body: formData,
  })

  // Не затирати новий токен, якщо 401 належить старому паралельному запиту (після логіну).
  if (
    res.status === 401 &&
    tokenUsed &&
    getToken() === tokenUsed &&
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
  const tokenUsed = getToken()
  const headers = { ...authHeaders(), ...rest.headers }

  let body = rest.body
  if (json !== undefined) {
    headers['Content-Type'] = 'application/json'
    body = JSON.stringify(json)
  }

  const res = await fetch(apiUrl(path), { ...rest, headers, body })

  if (
    res.status === 401 &&
    tokenUsed &&
    getToken() === tokenUsed &&
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
