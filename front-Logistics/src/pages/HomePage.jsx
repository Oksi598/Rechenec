import { useAuth } from '../auth/AuthContext'
import { CustomerPanel } from '../panels/CustomerPanel'
import { DispatcherPanel } from '../panels/DispatcherPanel'
import { DriverPanel } from '../panels/DriverPanel'
import { WarehousePanel } from '../panels/WarehousePanel'

export function HomePage() {
  const { roles } = useAuth()

  if (roles.includes('Dispatcher')) return <DispatcherPanel />
  if (roles.includes('Driver')) return <DriverPanel />
  if (roles.includes('Warehouse')) return <WarehousePanel />
  if (roles.includes('Customer')) return <CustomerPanel />

  return (
    <p className="muted pad">
      Немає відомої ролі. Зверніться до адміністратора.
    </p>
  )
}
