import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react'
import { api, getToken, setToken } from '../api/client'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)
  const refreshGen = useRef(0)

  const refreshMe = useCallback(async () => {
    const id = ++refreshGen.current
    const tokenAtStart = getToken()
    if (!tokenAtStart) {
      if (id === refreshGen.current) {
        setUser(null)
        setLoading(false)
      }
      return
    }
    try {
      const me = await api('/api/auth/me')
      if (id !== refreshGen.current) return
      setUser(me)
    } catch {
      if (id !== refreshGen.current) return
      if (getToken() === tokenAtStart) {
        setUser(null)
        setToken(null)
      }
    } finally {
      if (id === refreshGen.current) setLoading(false)
    }
  }, [])

  useEffect(() => {
    refreshMe()
  }, [refreshMe])

  useEffect(() => {
    const onUnauth = () => setUser(null)
    window.addEventListener('tms:unauthorized', onUnauth)
    return () => window.removeEventListener('tms:unauthorized', onUnauth)
  }, [])

  const login = useCallback(async (email, password) => {
    const data = await api('/api/auth/login', {
      method: 'POST',
      json: { email, password },
    })
    setToken(data.accessToken)
    await refreshMe()
    return data
  }, [refreshMe])

  const register = useCallback(async (payload) => {
    const data = await api('/api/auth/register', {
      method: 'POST',
      json: payload,
    })
    setToken(data.accessToken)
    await refreshMe()
    return data
  }, [refreshMe])

  const logout = useCallback(() => {
    refreshGen.current++
    setToken(null)
    setUser(null)
    setLoading(false)
  }, [])

  const value = useMemo(
    () => ({
      user,
      loading,
      login,
      register,
      logout,
      refreshMe,
      roles: user?.roles ?? [],
      isRole: (r) => (user?.roles ?? []).includes(r),
    }),
    [user, loading, login, register, logout, refreshMe],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth outside AuthProvider')
  return ctx
}
