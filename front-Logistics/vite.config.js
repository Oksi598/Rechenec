import dns from 'node:dns'
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// Windows: `localhost` часто резолвиться в ::1, а Kestrel на dev часто лише на IPv4 — проксі давав 502.
if (typeof dns.setDefaultResultOrder === 'function') {
  dns.setDefaultResultOrder('ipv4first')
}

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiTarget = env.VITE_API_PROXY || 'http://127.0.0.1:5212'

  return {
    plugins: [react()],
    server: {
      port: 5173,
      strictPort: true,
      // Узгодити WS HMR з IPv4 loopback (менше збоїв, ніж неявний localhost → IPv6).
      host: '127.0.0.1',
      hmr: {
        host: '127.0.0.1',
        port: 5173,
        protocol: 'ws',
      },
      proxy: {
        '/api': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
        },
        '/hubs': {
          target: apiTarget,
          changeOrigin: true,
          secure: false,
          ws: true,
        },
      },
    },
  }
})
