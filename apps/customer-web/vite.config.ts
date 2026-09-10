import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs'
import path from 'node:path'

const httpsDir = path.resolve(__dirname, '../../.https')
const keyPath = path.join(httpsDir, 'pos-key.pem')
const certPath = path.join(httpsDir, 'pos-cert.pem')
// Use the same certificate as admin-web and the API. Passing `true` alone lets
// Vite choose its own certificate, which can fail TLS negotiation on some
// clients/devices.
const https = process.env.VITE_HTTPS === 'true' && fs.existsSync(keyPath) && fs.existsSync(certPath)
  ? { key: fs.readFileSync(keyPath), cert: fs.readFileSync(certPath) }
  : undefined

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5174,
    host: true, // Cho phép truy cập từ các thiết bị khác trong mạng LAN
    strictPort: true,
    https
  }
})
