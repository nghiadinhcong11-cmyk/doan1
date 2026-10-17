# Khách QR (`guest`)

Route dùng chung `/` và `/scan` với QR/session; đây là guest session chứ không phải role customer đăng nhập. Source: `apps/customer-web/src/App.tsx`, `QRScan.tsx`, `DigitalMenu.tsx` và public/guest API. Case cũ `GST-*` ở `00-LEGACY-CASES.md`.

Truy cập trực tiếp /reservation và /profile được kiểm tra tại [guest-direct-routes.md](guest-direct-routes.md). Chính sách guest cho hai route này chưa được source xác định; xem finding COV-001.
Câu hỏi test tay AI của khách QR/guest: [../ai-chat/03-khach-qr.md](../ai-chat/03-khach-qr.md) (`QR-AI-01..15`). Guest cart/token/QR context phải được giữ nguyên khi reset chat.
