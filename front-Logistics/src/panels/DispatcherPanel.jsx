import { useEffect, useState } from 'react'
import { api } from '../api/client'

export function DispatcherPanel() {
  const [orders, setOrders] = useState([])
  const [routes, setRoutes] = useState([])
  const [vehicles, setVehicles] = useState([])
  const [drivers, setDrivers] = useState([])
  const [error, setError] = useState('')
  const [msg, setMsg] = useState('')
  const [staff, setStaff] = useState({
    email: '',
    password: '',
    fullName: '',
    phone: '',
    role: 'Driver',
  })
  const [assign, setAssign] = useState({
    routeId: '',
    vehicleId: '',
    driverId: '',
    rowVersion: '',
  })

  async function loadAll() {
    setError('')
    try {
      const [o, r, v, d] = await Promise.all([
        api('/api/orders/all'),
        api('/api/dispatch/routes'),
        api('/api/dispatch/vehicles'),
        api('/api/dispatch/drivers'),
      ])
      setOrders(o)
      setVehicles(v)
      setDrivers(d)
      setRoutes(r)
      setAssign((a) => {
        if (!r.length) return a
        const pick = r.find((x) => x.id === a.routeId) ?? r[0]
        return {
          ...a,
          routeId: pick.id,
          rowVersion: pick.rowVersionBase64,
          vehicleId: pick.vehicleId,
          driverId: pick.driverId,
        }
      })
    } catch (e) {
      setError(e.message)
    }
  }

  useEffect(() => {
    loadAll()
  }, [])

  async function createStaff(e) {
    e.preventDefault()
    setMsg('')
    setError('')
    try {
      await api('/api/staff/users', {
        method: 'POST',
        json: {
          email: staff.email.trim(),
          password: staff.password,
          fullName: staff.fullName.trim(),
          phone: staff.phone.trim() || null,
          role: staff.role,
        },
      })
      setMsg('Користувача створено.')
      setStaff((s) => ({ ...s, email: '', password: '', fullName: '', phone: '' }))
      await loadAll()
    } catch (err) {
      setError(err.message)
    }
  }

  async function optimize(routeId) {
    setMsg('')
    setError('')
    try {
      await api(`/api/tms/routes/${routeId}/optimize-load`, { method: 'POST' })
      setMsg('Оптимізацію навантаження виконано.')
      await loadAll()
    } catch (err) {
      setError(err.message)
    }
  }

  async function submitAssignment(e) {
    e.preventDefault()
    setMsg('')
    setError('')
    try {
      const res = await api(`/api/tms/routes/${assign.routeId}/assignment`, {
        method: 'PUT',
        json: {
          vehicleId: assign.vehicleId,
          driverId: assign.driverId,
          rowVersion: assign.rowVersion,
        },
      })
      setMsg('Призначення оновлено.')
      setAssign((a) => ({ ...a, rowVersion: res.rowVersion }))
      await loadAll()
    } catch (err) {
      setError(err.message)
    }
  }

  function onRouteChange(routeId) {
    const r = routes.find((x) => x.id === routeId)
    setAssign((a) => ({
      ...a,
      routeId,
      rowVersion: r?.rowVersionBase64 ?? '',
      vehicleId: r?.vehicleId ?? a.vehicleId,
      driverId: r?.driverId ?? a.driverId,
    }))
  }

  return (
    <div className="panels wide">
      <section>
        <h2>Новий співробітник</h2>
        <p className="muted small">Ролі: водій або склад.</p>
        <form onSubmit={createStaff} className="stack">
          <div className="grid2">
            <label>
              Email
              <input
                value={staff.email}
                onChange={(e) => setStaff((s) => ({ ...s, email: e.target.value }))}
                required
              />
            </label>
            <label>
              Пароль
              <input
                type="password"
                value={staff.password}
                onChange={(e) => setStaff((s) => ({ ...s, password: e.target.value }))}
                required
              />
            </label>
          </div>
          <label>
            Ім’я
            <input
              value={staff.fullName}
              onChange={(e) => setStaff((s) => ({ ...s, fullName: e.target.value }))}
              required
            />
          </label>
          <label>
            Телефон
            <input value={staff.phone} onChange={(e) => setStaff((s) => ({ ...s, phone: e.target.value }))} />
          </label>
          <label>
            Роль
            <select value={staff.role} onChange={(e) => setStaff((s) => ({ ...s, role: e.target.value }))}>
              <option value="Driver">Driver</option>
              <option value="Warehouse">Warehouse</option>
            </select>
          </label>
          <button type="submit" className="btn primary">
            Створити
          </button>
        </form>
      </section>

      <section>
        <h2>Маршрути</h2>
        <button type="button" className="btn ghost small" onClick={loadAll}>
          Оновити дані
        </button>
        <div className="table-wrap mt">
          <table className="table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Статус</th>
                <th>Початок</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {routes.map((r) => (
                <tr key={r.id}>
                  <td className="mono small">{r.id.slice(0, 8)}…</td>
                  <td>{r.status}</td>
                  <td className="small">{new Date(r.startTime).toLocaleString()}</td>
                  <td>
                    <button type="button" className="btn ghost small" onClick={() => optimize(r.id)}>
                      Оптимізувати навантаження
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {routes.length === 0 && <p className="muted">Немає маршрутів у БД.</p>}
        </div>
      </section>

      <section>
        <h2>Призначення ТЗ і водія</h2>
        <p className="muted small">
          Поточний <code>rowVersion</code> підставляється з обраного маршруту; після успішного PUT оновлюється
          автоматично.
        </p>
        <form onSubmit={submitAssignment} className="stack">
          <label>
            Маршрут
            <select value={assign.routeId} onChange={(e) => onRouteChange(e.target.value)} required>
              {routes.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.id.slice(0, 8)}… · {r.status}
                </option>
              ))}
            </select>
          </label>
          <label>
            Транспорт
            <select
              value={assign.vehicleId}
              onChange={(e) => setAssign((a) => ({ ...a, vehicleId: e.target.value }))}
              required
            >
              {vehicles.map((v) => (
                <option key={v.id} value={v.id}>
                  {v.plateNumber} · {v.vehicleType}
                </option>
              ))}
            </select>
          </label>
          <label>
            Водій
            <select
              value={assign.driverId}
              onChange={(e) => setAssign((a) => ({ ...a, driverId: e.target.value }))}
              required
            >
              {drivers.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.fullName} ({d.email})
                </option>
              ))}
            </select>
          </label>
          <label>
            RowVersion (base64)
            <input
              className="mono"
              value={assign.rowVersion}
              onChange={(e) => setAssign((a) => ({ ...a, rowVersion: e.target.value }))}
              required
            />
          </label>
          <button type="submit" className="btn primary">
            Зберегти призначення
          </button>
        </form>
      </section>

      <section>
        <h2>Усі замовлення</h2>
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Статус</th>
                <th>Що</th>
                <th>Ціна</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o) => (
                <tr key={o.id}>
                  <td>
                    <span className="badge">{o.status}</span>
                  </td>
                  <td>{o.productDescription || '—'}</td>
                  <td>{typeof o.priceEstimate === 'number' ? o.priceEstimate.toFixed(2) : o.priceEstimate}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {orders.length === 0 && <p className="muted">Немає замовлень.</p>}
        </div>
      </section>

      {msg && <p className="ok">{msg}</p>}
      {error && <p className="error">{error}</p>}
    </div>
  )
}
