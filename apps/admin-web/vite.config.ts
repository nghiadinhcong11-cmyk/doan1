import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'node:fs'
import path from 'node:path'

const httpsDir = path.resolve(__dirname, '../../.https')
const keyPath = path.join(httpsDir, 'pos-key.pem')
const certPath = path.join(httpsDir, 'pos-cert.pem')
// Do not enable HTTPS merely because a local certificate exists. The API must
// be started with the matching PFX certificate as well, otherwise the browser
// page loads over HTTPS while every API request fails at the TLS handshake.
const https = process.env.VITE_HTTPS === 'true' && fs.existsSync(keyPath) && fs.existsSync(certPath)
  ? { key: fs.readFileSync(keyPath), cert: fs.readFileSync(certPath) }
  : undefined

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    https,
    // Hỗ trợ truy cập từ các thiết bị khác trong cùng mạng LAN nếu cần
    host: true
  }
})
