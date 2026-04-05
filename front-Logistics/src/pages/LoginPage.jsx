import { useState } from 'react'
import { Link, Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function LoginPage() {
  const { login, user } = useAuth()
  const [email, setEmail] = useState('dispatcher@local.test')
  const [password, setPassword] = useState('ChangeMe!1')
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)
  const loc = useLocation()
  const from = loc.state?.from || '/'

  if (user) return <Navigate to={from} replace />

  async function onSubmit(e) {
    e.preventDefault()
    setError('')
    setPending(true)
    try {
      await login(email.trim(), password)
    } catch (err) {
      setError(err.message || 'Помилка входу')
    } finally {
      setPending(false)
    }
  }

  return (
    <div className="auth-card">
      <h1>Вхід</h1>
      <p className="muted small">
        Демо: диспетчер <code>dispatcher@local.test</code>, водій <code>driver@local.test</code>, склад{' '}
        <code>warehouse@local.test</code> — пароль усюди <code>ChangeMe!1</code>.
      </p>
      <form onSubmit={onSubmit} className="stack">
        <label>
          Email
          <input
            type="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </label>
        <label>
          Пароль
          <input
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>
        {error && <p className="error">{error}</p>}
        <button type="submit" className="btn primary" disabled={pending}>
          {pending ? '…' : 'Увійти'}
        </button>
      </form>
      <p className="muted small">
        Немає акаунта? <Link to="/register">Реєстрація замовника</Link>
      </p>
    </div>
  )
}
