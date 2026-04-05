import { useEffect, useState } from 'react'
import { api } from '../api/client'

function fmtOrder(o) {
  return (
    <article key={o.id} className="card">
      <div className="row-between">
        <strong>{o.productDescription || '—'}</strong>
        <span className="badge">{o.status}</span>
      </div>
      <p className="muted small">
        {typeof o.priceEstimate === 'number' ? o.priceEstimate.toFixed(2) : o.priceEstimate} грн ·{' '}
        {o.isUrgent ? 'терміново' : 'звичайно'}
      </p>
      <p className="small">{o.deliveryAddress}</p>
    </article>
  )
}

export function CustomerPanel() {
  const [depots, setDepots] = useState([])
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({
    pickupDepotId: '',
    deliveryLatitude: 50.45,
    deliveryLongitude: 30.52,
    deliveryAddress: '',
    productDescription: '',
    weight: 10,
    volume: 0.5,
    isUrgent: false,
    requiredDeliveryBefore: '',
  })

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      try {
        const d = await api('/api/depots')
        if (cancelled) return
        setDepots(d)
        if (d.length) {
          setForm((f) => ({ ...f, pickupDepotId: f.pickupDepotId || d[0].id }))
        }
        const mine = await api('/api/orders/mine')
        if (!cancelled) setOrders(mine)
      } catch (e) {
        if (!cancelled) setError(e.message)
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [])

  async function refreshOrders() {
    try {
      const mine = await api('/api/orders/mine')
      setOrders(mine)
    } catch (e) {
      setError(e.message)
    }
  }

  async function onCreate(e) {
    e.preventDefault()
    setError('')
    try {
      const body = {
        pickupDepotId: form.pickupDepotId,
        deliveryLatitude: Number(form.deliveryLatitude),
        deliveryLongitude: Number(form.deliveryLongitude),
        deliveryAddress: form.deliveryAddress,
        productDescription: form.productDescription,
        weight: Number(form.weight),
        volume: Number(form.volume),
        isUrgent: form.isUrgent,
        requiredDeliveryBefore: form.requiredDeliveryBefore
          ? new Date(form.requiredDeliveryBefore).toISOString()
          : null,
      }
      await api('/api/orders', { method: 'POST', json: body })
      setForm((f) => ({
        ...f,
        productDescription: '',
        deliveryAddress: '',
      }))
      await refreshOrders()
    } catch (err) {
      setError(err.message)
    }
  }

  if (loading && orders.length === 0 && depots.length === 0) {
    return <p className="muted">Завантаження…</p>
  }

  return (
    <div className="panels">
      <section>
        <h2>Нове замовлення</h2>
        <p className="muted small">
          Депо (звідки), точка доставки, вантаж, вага/об’єм. Вартість розраховується на бекенді.
        </p>
        <form onSubmit={onCreate} className="stack">
          <label>
            Депо
            <select
              value={form.pickupDepotId}
              onChange={(e) => setForm((f) => ({ ...f, pickupDepotId: e.target.value }))}
              required
            >
              {depots.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </label>
          <div className="grid2">
            <label>
              Широта доставки
              <input
                type="number"
                step="any"
                value={form.deliveryLatitude}
                onChange={(e) => setForm((f) => ({ ...f, deliveryLatitude: e.target.value }))}
                required
              />
            </label>
            <label>
              Довгота доставки
              <input
                type="number"
                step="any"
                value={form.deliveryLongitude}
                onChange={(e) => setForm((f) => ({ ...f, deliveryLongitude: e.target.value }))}
                required
              />
            </label>
          </div>
          <label>
            Куди (адреса)
            <input
              value={form.deliveryAddress}
              onChange={(e) => setForm((f) => ({ ...f, deliveryAddress: e.target.value }))}
              required
            />
          </label>
          <label>
            Що веземо
            <textarea
              rows={2}
              value={form.productDescription}
              onChange={(e) => setForm((f) => ({ ...f, productDescription: e.target.value }))}
              required
            />
          </label>
          <div className="grid2">
            <label>
              Вага (кг)
              <input
                type="number"
                step="any"
                value={form.weight}
                onChange={(e) => setForm((f) => ({ ...f, weight: e.target.value }))}
                required
              />
            </label>
            <label>
              Об’єм (м³)
              <input
                type="number"
                step="any"
                value={form.volume}
                onChange={(e) => setForm((f) => ({ ...f, volume: e.target.value }))}
                required
              />
            </label>
          </div>
          <label className="inline">
            <input
              type="checkbox"
              checked={form.isUrgent}
              onChange={(e) => setForm((f) => ({ ...f, isUrgent: e.target.checked }))}
            />
            Терміново
          </label>
          <label>
            Потрібно до (необов’язково)
            <input
              type="datetime-local"
              value={form.requiredDeliveryBefore}
              onChange={(e) => setForm((f) => ({ ...f, requiredDeliveryBefore: e.target.value }))}
            />
          </label>
          {error && <p className="error">{error}</p>}
          <button type="submit" className="btn primary">
            Подати заявку
          </button>
        </form>
      </section>

      <section>
        <h2>Мої замовлення</h2>
        <button type="button" className="btn ghost small" onClick={refreshOrders}>
          Оновити
        </button>
        <div className="stack mt">{orders.length === 0 ? <p className="muted">Поки порожньо.</p> : orders.map(fmtOrder)}</div>
      </section>
    </div>
  )
}
