import { openDB, type DBSchema } from 'idb'

export const DEFAULT_DELIVERY_PROOF_SYNC_ENDPOINT = '/api/tms/delivery-proofs/sync'

type DeliveryProofRecord = {
  // Key used for idempotency: must match backend `clientProofId`.
  clientProofId: string
  orderId: string
  signature: string
  photoBlob: Blob
  photoFileName: string
  createdAt: number
}

interface DeliveryProofsDB extends DBSchema {
  deliveryProofs: {
    key: string
    value: DeliveryProofRecord
  }
}

const DB_NAME = 'tms-offline'
const STORE_NAME = 'deliveryProofs'
const DB_VERSION = 1

function getDb() {
  return openDB<DeliveryProofsDB>(DB_NAME, DB_VERSION, {
    upgrade(db) {
      db.createObjectStore(STORE_NAME, { keyPath: 'clientProofId' })
    },
  })
}

export async function enqueueDeliveryProof(
  rec: Omit<DeliveryProofRecord, 'createdAt'> & { createdAt?: number },
  options?: { syncEndpoint?: string }
) {
  const syncEndpoint = options?.syncEndpoint ?? DEFAULT_DELIVERY_PROOF_SYNC_ENDPOINT

  const db = await getDb()
  const payload: DeliveryProofRecord = {
    ...rec,
    createdAt: rec.createdAt ?? Date.now(),
  }

  await db.put(STORE_NAME, payload)

  if (navigator.onLine) {
    // Foreground attempt; if it fails, Background Sync can retry.
    await syncDeliveryProofs(syncEndpoint)
    return
  }

  // Offline: queue it and attempt background sync (if supported).
  if ('serviceWorker' in navigator) {
    try {
      const reg = await navigator.serviceWorker.ready
      // Background Sync tag is consumed by `/sw.js` (public file).
      await reg.sync.register('sync-delivery-proofs')
    } catch {
      // If Background Sync isn't supported, we rely on next app open/online event.
    }
  }
}

export async function syncDeliveryProofs(endpoint: string) {
  const db = await getDb()
  const all = await db.getAll(STORE_NAME)
  if (all.length === 0) return

  // Sequential upload: keeps retries deterministic and avoids hammering the backend.
  for (const rec of all.sort((a, b) => a.createdAt - b.createdAt)) {
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
      // Stop at first failure; remaining records keep for retry.
      throw new Error(`DeliveryProof sync failed: ${res.status}`)
    }

    await db.delete(STORE_NAME, rec.clientProofId)
  }
}

