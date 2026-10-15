import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs'
import path from 'node:path'

const httpsDir = path.resolve(__dirname, '../../.https')
const keyPath = process.env.VITE_HTTPS_KEY_PATH || path.join(httpsDir, 'pos-key.pem')
const certPath = process.env.VITE_HTTPS_CERT_PATH || path.join(httpsDir, 'pos-cert.pem')
const lanMode = Boolean(process.env.DOAN_DEV_LAN_HOST)
// https://vitejs.dev/config/
export default defineConfig(({ command }) => {
  const httpsEnabled = command === 'serve' && process.env.VITE_HTTPS !== 'false'
  const hasCertificate = fs.existsSync(keyPath) && fs.existsSync(certPath)

  if (httpsEnabled && !hasCertificate) {
    throw new Error(`Customer Web HTTPS certificate is missing: ${certPath}`)
  }

  return {
    plugins: [react()],
    server: {
      port: 5174,
      // LAN binding is opt-in and is enabled by setup-https.ps1 together with
      // a certificate whose SAN covers the configured LAN host.
      host: lanMode ? true : 'localhost',
      strictPort: true,
      fs: { allow: [__dirname, path.resolve(__dirname, '../shared')] },
      https: httpsEnabled ? { key: fs.readFileSync(keyPath), cert: fs.readFileSync(certPath) } : undefined
    }
  }
})
