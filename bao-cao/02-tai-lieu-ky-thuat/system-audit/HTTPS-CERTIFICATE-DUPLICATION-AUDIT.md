# HTTPS Certificate Duplication Audit

Ngày audit: 17/10/2026
Phạm vi: `certs/`, `.https/` và các tham chiếu runtime trong repository.
Trạng thái: **AUDIT ONLY — chưa xóa file**.

## 1. Kết luận tóm tắt

- Không có hai file có cùng SHA-256.
- Có hai cặp PEM/PFX biểu diễn cùng certificate trong `.https/`: cặp hiện hành và cặp backup ngày 12/10/2026.
- `certs/` chứa một cặp LAN hiện hành và một cặp backup cũ; chúng là hai certificate khác nhau.
- `.https/` hiện là fallback/local certificate của Backend và Vite. `certs/doan-pos-lan.pem` và key tương ứng là certificate được `setup-https.ps1` xuất vào environment cho chế độ LAN hoặc localhost.
- Các thư mục backup không được script/runtime tham chiếu. Có thể xem là `DELETE_CANDIDATE` sau khi người dùng xác nhận, nhưng lượt này không xóa.

## 2. Inventory và SHA-256

SHA-256 được tính bằng `Get-FileHash -Algorithm SHA256`. Không đọc hoặc in nội dung private key/password.

| File | Bytes | Sửa đổi UTC | SHA-256 | Phân loại sơ bộ |
|---|---:|---|---|---|
| `certs/doan-pos-lan.pem` | 1610 | 2026-10-16 14:23:03 | `3FF64164074ECB207CA4C18A75F7A060DA35083341DA404C78D4C01201CE44A1` | KEEP |
| `certs/doan-pos-lan-key.pem` | 1704 | 2026-10-16 14:23:03 | `755EA0D621B9EAE7A0F8244FB3DA052B6F1E0C3FE275F70700E58AC96AEFC012` | KEEP |
| `certs/doan-pos-lan-backup.pem` | 1635 | 2026-10-13 15:33:53 | `63D5E29F752287515BC8CA6CE6F04166C982363F9FC81A0241EC3A43AF1BCB9F` | DELETE_CANDIDATE |
| `certs/doan-pos-lan-key-backup.pem` | 1704 | 2026-10-13 15:33:53 | `A5CFE102F1485C608A4084A0DAF7F23F262031C1054E4C704D3C7F2ABA60A07D` | DELETE_CANDIDATE |
| `.https/pos-cert.pem` | 1204 | 2026-10-13 02:59:27 | `66E49D98985EE679ED07D1A4BC64B40BA6DF0CC08BE038EB60E3FA2C2FFD4163` | KEEP |
| `.https/pos-key.pem` | 1732 | 2026-10-13 02:59:27 | `4B0AFBBA2EC4877E0A67069D00C1CCC95B057F446C8BED433A50D681DAEE39E1` | KEEP |
| `.https/pos-cert.pfx` | 2603 | 2026-10-13 02:59:27 | `50F3D667C6AE941EEF8B3499DA1FE41DC2DB9CE129DAF152D64BB853F8CAF679` | KEEP |
| `.https/cert-password.txt` | 32 | 2026-10-13 02:59:27 | `8FC205793B9D9C81989E6B4A9991B06454EF78CA4FAD9C59BE49877F4D76A50F` | KEEP; secret metadata only |
| `.https/backup-20261013-095926/pos-cert.pem` | 1208 | 2026-10-12 15:50:46 | `E38BA562332599705DC0E96B56283E27053657E0CD95BBC7EBFC4B100C865F16` | DELETE_CANDIDATE |
| `.https/backup-20261013-095926/pos-key.pem` | 1732 | 2026-10-12 15:50:46 | `AE61AE5BF62BF6CE1EC02F03196A488702806AB78368741CC4B82155E3A962ED` | DELETE_CANDIDATE |
| `.https/backup-20261013-095926/pos-cert.pfx` | 2603 | 2026-10-12 15:50:46 | `84E5C9EE5A1C04DDF24469DE3437734B2D8D4EAD0C888D5B5B32125BD61A4144` | DELETE_CANDIDATE |
| `.https/backup-20261013-095926/cert-password.txt` | 70 | 2026-10-12 15:50:46 | `7B30F1005AC3C81A85CDD0F6FBE7EC9C002FC627A77639E02AB04975CA5C2F8B` | DELETE_CANDIDATE; secret metadata only |

### SHA groups

Không có nhóm file trùng hoàn toàn theo SHA-256. Hai PFX có cùng dung lượng nhưng khác SHA-256.

## 3. Certificate identity

Metadata được đọc bằng OpenSSL `x509`/`pkcs12`; private key không được xuất ra console hoặc báo cáo.

| Nhóm | Subject / Issuer | Thumbprint SHA-1 | Serial | Valid UTC | SAN | Private key |
|---|---|---|---|---|---|---|
| `certs` hiện hành | Subject `O=mkcert development certificate`; Issuer mkcert development CA | `B9:50:CB:B7:DE:C4:F5:00:39:08:C9:42:DC:FE:1F:94:9D:9C:73:35` | `3AD595860E7D03EC2C5B243574AC45C1` | 2026-10-16 → 2029-01-16 | `localhost`, `127.0.0.1`, `192.168.11.172` | PEM/key public hashes match |
| `certs` backup | Subject `O=mkcert development certificate`; same mkcert issuer | `D6:85:1D:66:07:40:6E:B9:97:62:09:1B:E8:C8:76:EF:4B:2C:EF:BC` | `D14D1BF174EEA981F1F4314E35AD15FB` | 2026-10-13 → 2029-01-13 | `doan-pos.test`, `localhost`, `127.0.0.1`, `192.168.11.172` | PEM/key public hashes match |
| `.https` hiện hành PEM/PFX | Subject/Issuer `CN=doan-pos.test` | `24:93:2F:85:76:A7:D5:88:9A:00:7E:D0:D3:53:98:52:7A:0F:C1:09` | `6AFECAEAE8F97F8B6E02D9DBE3E68D16F217D20C` | 2026-10-13 → 2029-01-15 | `doan-pos.test`, `localhost`, `127.0.0.1` | PEM/key match; PFX contains same public key |
| `.https` backup PEM/PFX | Subject/Issuer `CN=RestaurantPOS Local` | `94:48:FD:A9:44:BE:92:35:13:BF:90:E4:70:A8:5C:06:98:FB:C5:D3` | `16689553B617CCCD8E2D00B433E441CCA6FE2C74` | 2026-10-12 → 2029-01-14 | `localhost`, `127.0.0.1`, `192.168.14.172` | PEM/key match; PFX contains same public key |

`certs` hiện hành và `.https` hiện hành không cùng certificate. Các backup cũng không cùng certificate với bản hiện hành tương ứng.

## 4. Dependency mapping

### Backend

- `services/api/Program.cs:445` mặc định dùng `.https/pos-cert.pfx` và `cert-password.txt` cạnh PFX.
- Khi `HTTPS_PEM_CERT_PATH` và `HTTPS_PEM_KEY_PATH` có trong Development, backend gọi `DevelopmentHttpsCertificateLoader` với cặp PEM được truyền qua environment.
- `scripts/setup-https.ps1:98-100,148-151` đặt các biến PEM tới `certs/doan-pos-lan.pem` và `certs/doan-pos-lan-key.pem`. Vì vậy `certs` là nguồn active sau khi chạy setup trong cùng PowerShell session.

### Admin Web và Customer Web

- `apps/admin-web/vite.config.ts:6-8` và `apps/customer-web/vite.config.ts:6-8` mặc định đọc `.https/pos-cert.pem` và `.https/pos-key.pem`.
- Khi chạy sau `setup-https.ps1`, `VITE_HTTPS_CERT_PATH` và `VITE_HTTPS_KEY_PATH` override sang cặp trong `certs/`. Đây là đường dùng cho Wi-Fi/LAN nếu SAN khớp host đã chọn.

### Không có dependency runtime vào backup

Không tìm thấy tham chiếu tới `doan-pos-lan-backup.pem`, `doan-pos-lan-key-backup.pem` hoặc thư mục `.https/backup-20261013-095926/` trong script, backend, Vite config hay environment mapping hiện có.

## 5. Phân loại và đề xuất

| File/group | Trạng thái | Đề xuất |
|---|---|---|
| `certs/doan-pos-lan.pem` + key | KEEP | Nguồn certificate LAN/Development do setup script kiểm tra và export. |
| `.https/pos-cert.pem` + key | KEEP | Fallback mặc định cho Vite khi không có environment override. |
| `.https/pos-cert.pfx` + password file | KEEP | Fallback mặc định cho Backend khi không dùng PEM environment. Không đổi password file. |
| `certs/*backup*` | OBSOLETE / DELETE_CANDIDATE | Không có runtime reference; certificate cũ hơn và khác serial/SAN. Chỉ xóa sau khi xác nhận không cần rollback. |
| `.https/backup-20261013-095926/*` | OBSOLETE / DELETE_CANDIDATE | Không có runtime reference; khác Subject/SAN/serial. Giữ tạm nếu cần forensic rollback. |

## 6. Rủi ro và cấu trúc gọn hơn

- Xóa `.https/pos-cert.pfx` hoặc password file sẽ làm Backend fallback không khởi động nếu session không có PEM environment.
- Xóa `.https/pos-cert.pem` hoặc key sẽ làm Vite fallback lỗi nếu chạy trực tiếp, không qua `setup-https.ps1`.
- Xóa `certs` hiện hành sẽ làm mất đường LAN/override do setup script và isolated HTTPS verification sử dụng.
- Các backup không được runtime dùng, nhưng xóa sẽ mất khả năng phục hồi forensic. Nên xác nhận thủ công trước.

Cấu trúc hợp lý hiện tại là giữ hai nguồn có vai trò khác nhau:

1. `certs/`: cặp mkcert hiện hành do `setup-https.ps1` quản lý cho Development/LAN.
2. `.https/`: fallback local PEM/PFX đã được cấu hình cho Vite/Backend.
3. Không tạo thêm bản backup trong hai thư mục nguồn; nếu cần lưu lịch sử, chuyển ra kho lưu trữ riêng ngoài runtime path sau khi có quy trình backup rõ ràng.

## 7. Lệnh và verification đã thực hiện

- Liệt kê recursive file metadata và SHA-256 bằng PowerShell.
- Đọc certificate identity/SAN/serial/validity bằng OpenSSL, không xuất key material.
- So sánh public-key hash giữa từng PEM và private key tương ứng: tất cả bốn cặp PEM/key đều khớp.
- So sánh public key của hai PFX với PEM tương ứng: cả hai cặp PFX/PEM đều cùng certificate/key.
- Tìm dependency bằng `rg` trong scripts, backend, frontend, tài liệu và harness.

Không xóa, regenerate, sửa trust store, sửa cấu hình hoặc khởi động/restart API.

## Kết luận

Hai thư mục **không có file trùng SHA-256** và **không chứa cùng một certificate giữa `certs` và `.https`**. `.https` hiện hành có PEM/PFX cùng certificate ở hai định dạng; backup trong `.https` là bản certificate khác, không phải duplicate byte-level. Các file backup là ứng viên dư thừa theo dependency hiện tại, nhưng chỉ nên xóa sau xác nhận rollback không cần thiết. Nguồn chính cho quy trình Development/LAN là cặp hiện hành trong `certs/`; `.https` hiện hành phải giữ làm fallback cho Backend/Vite.

## 8. Cleanup verification — 17/10/2026

Theo yêu cầu cleanup, sáu file backup đã được xóa sau khi đối chiếu lại SHA-256 và xác nhận không có runtime reference:

- `certs/doan-pos-lan-backup.pem`
- `certs/doan-pos-lan-key-backup.pem`
- `.https/backup-20261013-095926/pos-cert.pem`
- `.https/backup-20261013-095926/pos-key.pem`
- `.https/backup-20261013-095926/pos-cert.pfx`
- `.https/backup-20261013-095926/cert-password.txt`

Thư mục `.https/backup-20261013-095926/` đã được xóa sau khi rỗng. Không có file hiện hành nào bị sửa hoặc xóa.

### Kết quả sau cleanup

- Sáu file hiện hành được giữ và tồn tại; SHA-256 khớp inventory trước cleanup.
- Public-key comparison: `certs/doan-pos-lan.pem` ↔ `certs/doan-pos-lan-key.pem` **MATCH**; `.https/pos-cert.pem` ↔ `.https/pos-key.pem` **MATCH**.
- `.https/pos-cert.pfx` vẫn có thumbprint `24:93:2F:85:76:A7:D5:88:9A:00:7E:D0:D3:53:98:52:7A:0F:C1:09`, cùng certificate identity với `.https/pos-cert.pem`.
- `setup-https.ps1 -LanHost 192.168.11.172 -ValidateOnly` **NOT RUN** vì địa chỉ này không hiện diện trên các interface IPv4 hiện tại; không đoán IP thay thế.
- Kestrel vẫn trỏ tới `.https/pos-cert.pfx` mặc định hoặc PEM environment do `setup-https.ps1` cấp; Vite vẫn trỏ tới `.https` fallback hoặc PEM environment override trong `certs`.
- Không khởi động/restart API, không sửa source, script, database hoặc trust store.

### Trạng thái tiếp tục Development HTTPS

**GO có điều kiện**: cleanup không làm thay đổi cặp certificate hiện hành và dependency mapping vẫn hợp lệ. Cần chạy lại `setup-https.ps1 -LanHost <IP-Wi-Fi-hiện-tại> -ValidateOnly` trong PowerShell có IP LAN thực tế trước khi dùng chế độ Wi-Fi; kiểm tra này chưa PASS trong môi trường hiện tại.
