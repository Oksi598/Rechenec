import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export type VehicleLocationChangedPayload = {
  vehicleId: string
  lat: number
  lng: number
  speed: number | null
  recordedAt: string
}

export function createTrackingConnection(
  apiBaseUrl: string,
  routeId: string,
  accessToken?: string | null
): HubConnection {
  const url = `${apiBaseUrl}/hubs/tracking?routeId=${encodeURIComponent(routeId)}`

  const connection = new HubConnectionBuilder()
    .withUrl(url, {
      accessTokenFactory: () => accessToken ?? '',
    })
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect([0, 2000, 5000, 10000])
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

export async function stopTracking(
  connection: HubConnection,
  onVehicleLocationChanged: (payload: VehicleLocationChangedPayload) => void
) {
  connection.off('VehicleLocationChanged', onVehicleLocationChanged)

  if (connection.state !== 'Disconnected') {
    await connection.stop()
  }
}

