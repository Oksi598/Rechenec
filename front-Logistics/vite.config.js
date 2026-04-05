import dns from 'node:dns'
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// Node → Kestrel: спочатку IPv4, щоб проксі не потрапляв у ::1 без слухача.
if (typeof dns.setDefaultResultOrder === 'function') {
  dns.setDefaultResultOrder('ipv4first')
}

function apiProxyTarget(env) {
  return (env.VITE_API_PROXY ?? '').trim() || 'http://127.0.0.1:5212'
}

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const target = apiProxyTarget(env)

  const proxy = {
    '/api': {
      target,
      changeOrigin: true,
      secure: false,
    },
    '/hubs': {
      target,
      changeOrigin: true,
      secure: false,
      ws: true,
    },
  }

  return {
    plugins: [react()],
    server: {
      port: 5173,
      strictPort: true,
      proxy,
    },
    preview: {
      port: 4173,
      strictPort: true,
      proxy,
    },
  }
})
