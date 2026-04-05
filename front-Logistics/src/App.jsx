import { useEffect, useMemo, useState } from 'react'
import { HubConnectionState } from '@microsoft/signalr'
import { enqueueDeliveryProof } from './offline/deliveryProofQueue'
import { createTrackingConnection, startTracking, stopTracking } from './realtime/tracking'
import './App.css'

function App() {
  const [apiBaseUrl, setApiBaseUrl] = useState('https://localhost:7246')
  const [routeId, setRouteId] = useState('')
  const [vehicleId, setVehicleId] = useState('')
  const [orderId, setOrderId] = useState('')
  const [signature, setSignature] = useState('')
  const [file, setFile] = useState(null)
  const [status, setStatus] = useState('disconnected')
  const [lastError, setLastError] = useState('')
  const [locations, setLocations] = useState([])
  const [accessToken, setAccessToken] = useState('')

  const connection = useMemo(() => {
    if (!routeId) return null
    return createTrackingConnection(apiBaseUrl, routeId, accessToken.trim() || null)
  }, [apiBaseUrl, routeId, accessToken])

  useEffect(() => {
    if (!connection) return

    const onVehicleLocationChanged = (payload) => {
      setLocations((prev) => {
        const next = [{ ...payload, receivedAt: new Date().toISOString() }, ...prev]
        return next.slice(0, 20)
      })
      setStatus('connected')
    }

    connection.onreconnecting(() => setStatus('reconnecting'))
    connection.onreconnected(() => setStatus('connected'))
    connection.onclose(() => setStatus('disconnected'))

    startTracking(connection, onVehicleLocationChanged).catch((err) => {
      setLastError(String(err))
      setStatus('error')
    })

    return () => {
      stopTracking(connection, onVehicleLocationChanged).catch(() => {})
    }
  }, [connection])

  const connectionStateLabel = useMemo(() => {
    if (!connection) return status
    switch (connection.state) {
      case HubConnectionState.Connected:
        return 'connected'
      case HubConnectionState.Reconnecting:
        return 'reconnecting'
      case HubConnectionState.Connecting:
        return 'connecting'
      default:
        return status
    }
  }, [connection, status])

  async function handleQueueProof(event) {
    event.preventDefault()
    if (!file || !orderId) return

    const clientProofId = `${orderId}-${Date.now()}`
    try {
      await enqueueDeliveryProof({
        clientProofId,
        orderId,
        signature,
        photoBlob: file,
        photoFileName: file.name,
      })
      setSignature('')
      setFile(null)
      setLastError('')
    } catch (err) {
      setLastError(String(err))
    }
  }

  return (
    <main className="tms-layout">
      <section className="panel">
        <h1>TMS Dispatcher Console</h1>
        <p>Realtime route tracking + offline delivery proof queue.</p>
      </section>

      <section className="panel">
        <h2>Tracking</h2>
        <div className="row">
          <label>
            API Base URL
            <input value={apiBaseUrl} onChange={(e) => setApiBaseUrl(e.target.value)} />
          </label>
          <label>
            Route ID
            <input value={routeId} onChange={(e) => setRouteId(e.target.value)} />
          </label>
          <label>
            Vehicle ID (filter)
            <input value={vehicleId} onChange={(e) => setVehicleId(e.target.value)} />
          </label>
          <label>
            JWT (login)
            <input
              type="password"
              autoComplete="off"
              placeholder="Bearer from /api/auth/login"
              value={accessToken}
              onChange={(e) => setAccessToken(e.target.value)}
            />
          </label>
        </div>
        <p className="status">Connection: {connectionStateLabel}</p>
        {lastError && <p className="error">{lastError}</p>}
        <ul className="log">
          {locations
            .filter((item) => !vehicleId || item.vehicleId === vehicleId)
            .map((item) => (
              <li key={`${item.vehicleId}-${item.recordedAt}`}>
                {item.vehicleId} :: {item.lat.toFixed(5)}, {item.lng.toFixed(5)} :: {item.speed ?? 0} km/h
              </li>
            ))}
        </ul>
      </section>

      <section className="panel">
        <h2>Offline Delivery Proof</h2>
        <form onSubmit={handleQueueProof} className="proof-form">
          <label>
            Order ID
            <input value={orderId} onChange={(e) => setOrderId(e.target.value)} required />
          </label>
          <label>
            Signature
            <input value={signature} onChange={(e) => setSignature(e.target.value)} required />
          </label>
          <label>
            Photo
            <input type="file" accept="image/*" onChange={(e) => setFile(e.target.files?.[0] ?? null)} required />
          </label>
          <button type="submit">Queue Proof</button>
        </form>
      </section>
    </main>
  )
}

export default App
