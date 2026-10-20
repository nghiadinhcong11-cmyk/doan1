import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs'
import path from 'node:path'

const keyPath = process.env.VITE_HTTPS_KEY_PATH
const certPath = process.env.VITE_HTTPS_CERT_PATH
const lanMode = Boolean(process.env.DOAN_DEV_LAN_HOST)

// https://vitejs.dev/config/
export default defineConfig(({ command }) => {
  const httpsEnabled = command === 'serve' && process.env.VITE_HTTPS !== 'false'
  const hasCertificate = Boolean(keyPath && certPath) && fs.existsSync(keyPath) && fs.existsSync(certPath)

  if (httpsEnabled && (!keyPath || !certPath || !hasCertificate)) {
    throw new Error('Admin Web HTTPS PEM paths are missing or invalid. Run scripts/setup-https.ps1 first.')
  }

  return {
    plugins: [react()],
    server: {
      port: 5173,
      strictPort: true,
      fs: { allow: [__dirname, path.resolve(__dirname, '../shared')] },
      https: httpsEnabled ? { key: fs.readFileSync(keyPath), cert: fs.readFileSync(certPath) } : undefined,
      // LAN binding is opt-in and is enabled by setup-https.ps1 together with
      // a certificate whose SAN covers the configured LAN host.
      host: lanMode ? true : 'localhost'
    }
  }
})
