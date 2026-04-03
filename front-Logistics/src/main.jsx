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

  navigator.serviceWorker.addEventListener('message', (event) => {
    if (event.data?.type === 'SYNC_DELIVERY_PROOFS') {
      syncDeliveryProofs(DEFAULT_DELIVERY_PROOF_SYNC_ENDPOINT).catch(() => {
        // Keep queued proofs for the next retry.
      })
    }
  })
}

window.addEventListener('online', () => {
  syncDeliveryProofs(DEFAULT_DELIVERY_PROOF_SYNC_ENDPOINT).catch(() => {})
})
