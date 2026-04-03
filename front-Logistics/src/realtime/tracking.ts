import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export type VehicleLocationChangedPayload = {
  vehicleId: string
  lat: number
  lng: number
  speed: number | null
  recordedAt: string
}

export function createTrackingConnection(apiBaseUrl: string, routeId: string): HubConnection {
  const url = `${apiBaseUrl}/hubs/tracking?routeId=${encodeURIComponent(routeId)}`

  const connection = new HubConnectionBuilder()
    .withUrl(url)
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build()

  return connection
}

export async function startTracking(
  connection: HubConnection,
  onVehicleLocationChanged: (payload: VehicleLocationChangedPayload) => void
) {
  connection.on('VehicleLocationChanged', onVehicleLocationChanged)

  if (connection.state !== 'Connected') {
    await connection.start()
  }
}

