import { useEffect, useState } from 'react'
import { api } from '../api/client'
import { formatOrderStatus } from '../lib/orderStatus'

export function WarehousePanel() {
  const [orders, setOrders] = useState([])
  const [error, setError] = useState('')

  async function load() {
    setError('')
    try {
      const data = await api('/api/orders/warehouse-board')
      setOrders(data)
    } catch (e) {
      setError(e.message)
    }
  }

  useEffect(() => {
    load()
  }, [])

  return (
    <div className="panels">
      <section>
        <h2>Черга складу</h2>
        <p className="muted small">Активні замовлення (не доставлені / не скасовані).</p>
        <button type="button" className="btn ghost small" onClick={load}>
          Оновити
        </button>
        <div className="table-wrap mt">
          <table className="table">
            <thead>
              <tr>
                <th>Статус</th>
                <th>Що</th>
                <th>Вага / об’єм</th>
                <th>До</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o) => (
                <tr key={o.id}>
                  <td>
                    <span className="badge">{formatOrderStatus(o.status)}</span>
                  </td>
                  <td>{o.productDescription || '—'}</td>
                  <td>
                    {o.weight} кг · {o.volume} м³
                  </td>
                  <td className="small">
                    {o.requiredDeliveryBefore
                      ? new Date(o.requiredDeliveryBefore).toLocaleString()
                      : '—'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          {orders.length === 0 && <p className="muted">Немає записів.</p>}
        </div>
      </section>
      {error && <p className="error">{error}</p>}
    </div>
  )
}
