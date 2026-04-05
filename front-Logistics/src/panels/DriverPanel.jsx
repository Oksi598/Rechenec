import { useEffect, useMemo, useState } from 'react'
import { api, apiForm, getToken } from '../api/client'
import { createTrackingConnection, startTracking, stopTracking } from '../realtime/tracking'

export function DriverPanel() {
  const [orders, setOrders] = useState([])
  const [error, setError] = useState('')
  const [routeId, setRouteId] = useState('')
  const [vehicleFilter, setVehicleFilter] = useState('')
  const [locations, setLocations] = useState([])
  const [connState, setConnState] = useState('offline')
  const [proof, setProof] = useState({ orderId: '', signature: '', file: null })

  async function load() {
    setError('')
    try {
      const list = await api('/api/orders/driver')
      setOrders(list)
      const ids = [...new Set(list.map((o) => o.routeId).filter(Boolean))]
      if (ids.length && !routeId) setRouteId(ids[0])
    } catch (e) {
      setError(e.message)
    }
  }

  useEffect(() => {
    load()
  }, [])

  const connection = useMemo(() => {
    if (!routeId) return null
    return createTrackingConnection('', routeId, getToken())
  }, [routeId])

  useEffect(() => {
    if (!connection) return

    const onLoc = (payload) => {
      setLocations((prev) => [{ ...payload, receivedAt: new Date().toISOString() }, ...prev].slice(0, 30))
      setConnState('connected')
    }

    connection.onreconnecting(() => setConnState('reconnecting'))
    connection.onreconnected(() => setConnState('connected'))
    connection.onclose(() => setConnState('disconnected'))

    startTracking(connection, onLoc).catch((err) => {
      setError(String(err))
      setConnState('error')
    })

    return () => {
      stopTracking(connection, onLoc).catch(() => {})
    }
  }, [connection])

  async function sendProof(e) {
    e.preventDefault()
    if (!proof.file || !proof.orderId) return
    setError('')
    try {
      const fd = new FormData()
      fd.append('orderId', proof.orderId)
      fd.append('clientProofId', `${proof.orderId}-${Date.now()}`)
      fd.append('signature', proof.signature)
      fd.append('photo', proof.file, proof.file.name)
      await apiForm('/api/tms/delivery-proofs/sync', fd)
      setProof({ orderId: '', signature: '', file: null })
    } catch (err) {
      setError(err.message)
    }
  }

  const routeOptions = [...new Set(orders.map((o) => o.routeId).filter(Boolean))]

  return (
    <div className="panels">
      <section>
        <h2>Мої замовлення на маршрутах</h2>
        <button type="button" className="btn ghost small" onClick={load}>
          Оновити
        </button>
        <div className="stack mt">
          {orders.length === 0 ? (
            <p className="muted">Поки немає призначених замовлень.</p>
          ) : (
            orders.map((o) => (
              <article key={o.id} className="card">
                <div className="row-between">
                  <strong>{o.productDescription}</strong>
                  <span className="badge">{o.status}</span>
                </div>
                <p className="small muted">
                  {o.routeStartTime && new Date(o.routeStartTime).toLocaleString()} —{' '}
                  {o.routeEndTime && new Date(o.routeEndTime).toLocaleString()}
                </p>
                <p className="small">
                  {o.weight} кг · {o.volume} м³
                </p>
              </article>
            ))
          )}
        </div>
      </section>

      <section>
        <h2>Підтвердження доставки</h2>
        <form onSubmit={sendProof} className="stack">
          <label>
            Замовлення
            <select
              value={proof.orderId}
              onChange={(e) => setProof((p) => ({ ...p, orderId: e.target.value }))}
              required
            >
              <option value="">—</option>
              {orders.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.productDescription?.slice(0, 40) || o.id.slice(0, 8)}
                </option>
              ))}
            </select>
          </label>
          <label>
            Підпис
            <input
              value={proof.signature}
              onChange={(e) => setProof((p) => ({ ...p, signature: e.target.value }))}
              required
            />
          </label>
          <label>
            Фото
            <input
              type="file"
              accept="image/*"
              onChange={(e) => setProof((p) => ({ ...p, file: e.target.files?.[0] ?? null }))}
              required
            />
          </label>
          <button type="submit" className="btn primary">
            Надіслати
          </button>
        </form>
      </section>

      <section>
        <h2>Трекінг маршруту (SignalR)</h2>
        <div className="grid2">
          <label>
            Маршрут (routeId)
            <select value={routeId} onChange={(e) => setRouteId(e.target.value)}>
              <option value="">—</option>
              {routeOptions.map((id) => (
                <option key={id} value={id}>
                  {id.slice(0, 8)}…
                </option>
              ))}
            </select>
          </label>
          <label>
            Фільтр vehicleId
            <input value={vehicleFilter} onChange={(e) => setVehicleFilter(e.target.value)} placeholder="опційно" />
          </label>
        </div>
        <p className="muted small">
          Стан: {connState} · хаб <code>/hubs/tracking</code>
        </p>
        <ul className="log">
          {locations
            .filter((x) => !vehicleFilter || x.vehicleId === vehicleFilter)
            .map((item) => (
              <li key={`${item.vehicleId}-${item.recordedAt}`}>
                {item.vehicleId} · {Number(item.lat).toFixed(5)}, {Number(item.lng).toFixed(5)} ·{' '}
                {item.speed ?? 0} км/год
              </li>
            ))}
        </ul>
      </section>

      {error && <p className="error">{error}</p>}
    </div>
  )
}
