import { Link, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export function Layout() {
  const { user, logout } = useAuth()

  return (
    <div className="layout">
      <header className="header">
        <Link to="/" className="logo">
          TMS
        </Link>
        <nav className="nav">
          {user && (
            <>
              <span className="muted">
                {user.fullName} · {(user.roles ?? []).join(', ')}
              </span>
              <button type="button" className="btn ghost" onClick={logout}>
                Вийти
              </button>
            </>
          )}
        </nav>
      </header>
      <main className="main">
        <Outlet />
      </main>
    </div>
  )
}
