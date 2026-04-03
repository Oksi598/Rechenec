/* eslint-disable no-restricted-globals */

self.addEventListener('sync', (event) => {
  if (event.tag !== 'sync-delivery-proofs') return

  event.waitUntil(
    (async () => {
      // Background Sync has no direct access to IndexedDB queue data here.
      // So we notify an open client window to perform the actual sync.
      const clients = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
      for (const client of clients) {
        client.postMessage({ type: 'SYNC_DELIVERY_PROOFS' })
      }
    })()
  )
})

