import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { syncDeliveryProofs, DEFAULT_DELIVERY_PROOF_SYNC_ENDPOINT } from './offline/deliveryProofQueue'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)

// Offline-first DeliveryProof sync bootstrap.
if ('serviceWorker' in navigator) {
  navigator.serviceWorker
    .register('/sw.js')
    .catch(() => {
      // Service worker registration is best-effort for this blueprint.
    })
}

window.addEventListener('online', () => {
  syncDeliveryProofs(DEFAULT_DELIVERY_PROOF_SYNC_ENDPOINT).catch(() => {})

  navigator.serviceWorker.ready
    .then((registration) => registration.active?.postMessage({ type: 'SYNC_DELIVERY_PROOFS' }))
    .catch(() => {})
})
