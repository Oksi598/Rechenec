/** Підписи для статусів з API (camelCase після JsonStringEnumConverter) або числового legacy. */
const BY_NAME = {
  pendingAssignment: 'Очікує призначення',
  pendingLoadOptimization: 'Очікує оптимізації',
  readyForRouting: 'Готове до маршруту',
  inRouting: 'У доставці',
  delivered: 'Доставлено',
  cancelled: 'Скасовано',
}

const BY_INDEX = [
  'Очікує призначення',
  'Очікує оптимізації',
  'Готове до маршруту',
  'У доставці',
  'Доставлено',
  'Скасовано',
]

export function formatOrderStatus(status) {
  if (status === null || status === undefined) return '—'
  if (typeof status === 'number') {
    return BY_INDEX[status] ?? String(status)
  }
  const key = String(status)
  return BY_NAME[key] ?? key.replace(/([A-Z])/g, ' $1').trim()
}
