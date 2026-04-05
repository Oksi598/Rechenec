/* eslint-disable no-restricted-globals */

self.__APP_CACHE = 'tms-app-v2'
const OFFLINE_SYNC_TAG = 'sync-delivery-proofs'
const DB_NAME = 'tms-offline'
const STORE_NAME = 'deliveryProofs'
const APP_SHELL = ['/', '/index.html', '/manifest.webmanifest']

self.addEventListener('install', (event) => {
  event.waitUntil(
    (async () => {
      const cache = await caches.open(self.__APP_CACHE)
      await cache.addAll(APP_SHELL)
      await self.skipWaiting()
    })()
  )
})

self.addEventListener('activate', (event) => {
  event.waitUntil(
    (async () => {
      const keys = await caches.keys()
      await Promise.all(keys.filter((k) => k !== self.__APP_CACHE).map((k) => caches.delete(k)))
      await self.clients.claim()
    })()
  )
})

self.addEventListener('fetch', (event) => {
  if (event.request.method !== 'GET') return

  const url = new URL(event.request.url)
  if (url.origin !== self.location.origin) return

  // API та SignalR не кешуємо — інакше 401/JSON можуть «залипати» в кеші й ламати вхід.
  if (url.pathname.startsWith('/api/') || url.pathname.startsWith('/hubs/')) {
    event.respondWith(fetch(event.request))
    return
  }

  event.respondWith(
    (async () => {
      const cached = await caches.match(event.request)
      if (cached) return cached

      try {
        const fresh = await fetch(event.request)
        const cache = await caches.open(self.__APP_CACHE)
        cache.put(event.request, fresh.clone())
        return fresh
      } catch {
        return caches.match('/index.html')
      }
    })()
  )
})

self.addEventListener('sync', (event) => {
  if (event.tag !== OFFLINE_SYNC_TAG) return

  event.waitUntil(
    (async () => {
      await syncDeliveryProofsInWorker('/api/tms/delivery-proofs/sync')
    })()
  )
})

self.addEventListener('message', (event) => {
  if (event.data?.type !== 'SYNC_DELIVERY_PROOFS') return

  event.waitUntil(
    (async () => {
      await syncDeliveryProofsInWorker('/api/tms/delivery-proofs/sync')
    })()
  )
})

async function syncDeliveryProofsInWorker(endpoint) {
  const proofs = await readAllProofs()
  for (const rec of proofs.sort((a, b) => a.createdAt - b.createdAt)) {
    const form = new FormData()
    form.append('orderId', rec.orderId)
    form.append('signature', rec.signature)
    form.append('clientProofId', rec.clientProofId)
    form.append('photo', rec.photoBlob, rec.photoFileName)

    const res = await fetch(endpoint, {
      method: 'POST',
      body: form,
      credentials: 'include',
    })

    if (!res.ok) {
      throw new Error(`DeliveryProof sync failed: ${res.status}`)
    }

    await deleteProof(rec.clientProofId)
  }
}

function openQueueDb() {
  return new Promise((resolve, reject) => {
    const req = indexedDB.open(DB_NAME, 1)
    req.onerror = () => reject(req.error ?? new Error('IndexedDB open failed'))
    req.onupgradeneeded = () => {
      const db = req.result
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        db.createObjectStore(STORE_NAME, { keyPath: 'clientProofId' })
      }
    }
    req.onsuccess = () => resolve(req.result)
  })
}

async function readAllProofs() {
  const db = await openQueueDb()
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readonly')
    const store = tx.objectStore(STORE_NAME)
    const req = store.getAll()
    req.onerror = () => reject(req.error ?? new Error('IndexedDB read failed'))
    req.onsuccess = () => resolve(req.result ?? [])
  })
}

async function deleteProof(clientProofId) {
  const db = await openQueueDb()
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite')
    const store = tx.objectStore(STORE_NAME)
    const req = store.delete(clientProofId)
    req.onerror = () => reject(req.error ?? new Error('IndexedDB delete failed'))
    req.onsuccess = () => resolve()
  })
}
