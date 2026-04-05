import { useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function RegisterPage() {
  const { register, user } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fullName, setFullName] = useState('')
  const [phone, setPhone] = useState('')
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)

  if (user) return <Navigate to="/" replace />

  async function onSubmit(e) {
    e.preventDefault()
    setError('')
    setPending(true)
    try {
      await register({
        email: email.trim(),
        password,
        fullName: fullName.trim(),
        phone: phone.trim() || null,
      })
    } catch (err) {
      const body = err.body
      if (body?.errors) setError(body.errors.join('; '))
      else setError(err.message || 'Помилка')
    } finally {
      setPending(false)
    }
  }

  return (
    <div className="auth-card">
      <h1>Реєстрація</h1>
      <p className="muted small">Створюється лише роль «Замовник».</p>
      <form onSubmit={onSubmit} className="stack">
        <label>
          Повне ім’я
          <input value={fullName} onChange={(e) => setFullName(e.target.value)} required />
        </label>
        <label>
          Email
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>
          Телефон (необов’язково)
          <input value={phone} onChange={(e) => setPhone(e.target.value)} />
        </label>
        <label>
          Пароль (мін. 8 символів, цифра, велика літера)
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>
        {error && <p className="error">{error}</p>}
        <button type="submit" className="btn primary" disabled={pending}>
          {pending ? '…' : 'Зареєструватися'}
        </button>
      </form>
      <p className="muted small">
        Вже є акаунт? <Link to="/login">Увійти</Link>
      </p>
    </div>
  )
}
