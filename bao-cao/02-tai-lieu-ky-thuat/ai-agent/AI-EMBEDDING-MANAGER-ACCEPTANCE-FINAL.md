# AI Embedding Manager Acceptance Final

**Đối chiếu:** 17/10/2026
**Repository:** `D:\\Dev\\doan`
**Baseline:** `manager-two-intent-v3.3`

## 1. Kết luận

| Thành phần | Trạng thái |
|---|---|
| `REVENUE_QUERY` | **UAT_RETEST_PASS** |
| `BUSINESS_SUMMARY` | **UAT_RETEST_PASS** |
| Manager Pilot | **PENDING_FINAL_SIGNOFF** |
| Production rollout | **NO-GO** |
| Remove MockAiProvider | **NO-GO** |

Đây là nghiệm thu giới hạn cho manager pilot, không phải production approval cho toàn bộ AI system.

## 2. Phạm vi và runtime

`POST /api/Ai/chat` → `AiController` → trusted JWT role/BranchId → `AiOrchestrator` → `ManagerEmbeddingPilot` → `AiToolRegistry` → `AiPermissionService` → read-only business tool → application service.

Feature gate chỉ hoạt động trong Development/Test/Testing và role `manager`. CLARIFY/UNSUPPORTED không tạo tool execution. Role khác và flag OFF giữ provider path hiện tại.

## 3. Baseline và capability contract

- Model: `intfloat/multilingual-e5-small` ONNX.
- Tokenizer, checksum, catalog, threshold và dataset v3.3 không thay đổi.
- Revenue: `today`, `month`, `year`.
- Summary: `today`, `yesterday`, `this_week`, `last_week`, `this_month`, `last_month`.
- Hai tool read-only, branch-scoped theo trusted context.
- Baseline manifest: [AI-EMBEDDING-V33-BASELINE-MANIFEST.json](AI-EMBEDDING-V33-BASELINE-MANIFEST.json).
- Baseline vẫn là `RESEARCH_BASELINE_LOCKED`, không phải `PRODUCTION_APPROVED`.

## 4. Automated verification

| Hạng mục | Kết quả |
|---|---|
| Five-case regression | **5/5 PASS** |
| Runtime integration harness | **32 PASS / 0 FAIL** |
| Backend build | **PASS**, 0 warning, 0 error |
| ONNX adapter/lifecycle | **PASS** |
| Negation/OOD/unsupported/compound | **PASS** |
| Permission/branch isolation | **PASS** |
| Database/API production | **NOT RUN** |
| Browser bởi Codex | **NOT RUN** |

Harness cô lập, không truy cập database thật.

## 5. Manual UAT lần đầu và root cause

Lần đầu: **16/20 PASS**, **4/20 FAIL** (M-R03, M-R04, M-S05, M-X01); M-S02 cần xác minh. Không mô tả lần đầu là 20/20.

- **M-R03:** `this_month` bị kiểm tra như revenue period chưa chuẩn hóa; đã map sang argument `month`.
- **M-R04:** “báo cáo” mở rộng nhầm thành summary dù yêu cầu revenue-only; đã phân biệt “báo cáo doanh thu”.
- **M-S02:** isolated và live đều cho `BUSINESS_SUMMARY(yesterday)`; không còn lỗi sau khi có correlation evidence.
- **M-S05:** câu yêu cầu nhiều KPI nhưng candidate là revenue; đã áp dụng capability composition để chọn summary.
- **M-X01:** menu/danh sách món/giá chưa bị chặn như OOD; đã bổ sung boundary trước period validation.

Không thay đổi model, tokenizer, threshold hoặc dataset.

## 6. Manual retest và live evidence

| ID | Correlation ID | Kết quả |
|---|---|---|
| M-R03 | `e0f1e4c1-a494-467a-be6c-96b8da2517d2` | PASS — revenue |
| M-R04 | `44dc7883-b82a-4a75-b210-5b83d7a728a1` | PASS — revenue |
| M-S02 | `20bb46a2-728d-4c9a-b822-4a68b9911a2e` | PASS — business summary |
| M-S05 | `959f8e6a-0597-4cd2-b355-3ec6a4f66d7a` | PASS — business summary |
| M-X01 | `ce76555d-413a-4333-ac19-320b61c0f795` | PASS — UNSUPPORTED |

Live log: `services/api/logs/ai-uat/ai-chat.log`.

Đối chiếu được 5/5 HTTP 200, 4/4 ACCEPT đúng intent/tool, 4/4 SELECTED → AUTHORIZED → EXECUTED, 4/4 branch scope match YES và M-X01 không có tool execution.

M-R03 ghi `AI_PERIOD=this_month` trong log. Source `ManagerEmbeddingPilot.ToToolPeriod` tạo argument `period=month` cho `get_revenue`, nhưng log không ghi JSON arguments. Vì vậy chỉ xác nhận kỳ policy/log và mapping trong source; không tuyên bố log đã chứng minh trực tiếp argument runtime `month`.

## 7. Permission, branch và safety

Trusted JWT cung cấp role/BranchId; input chat không thể đổi branch. Permission service chạy trước execution. Read-only guard, fail-closed, CLARIFY/UNSUPPORTED no-tool và MockAiProvider fallback boundary vẫn giữ nguyên. Customer/Guest, POS, KDS và role ngoài pilot không thay đổi.

## 8. Performance quan sát

Isolated harness ghi nhận cold khoảng 1.75–2.70 giây, warm median khoảng 6.6–9.6 ms, warm p95 khoảng 8.5–22.4 ms và RSS delta khoảng 0.5 GB. Đây là số đo cục bộ, không phải production capacity/SLO.

## 9. Chưa được chứng minh

- Chưa có final sign-off độc lập cho toàn bộ checklist manager.
- Log chưa ghi trực tiếp JSON tool arguments.
- Chưa kiểm thử production capacity/database production/browser bởi Codex.
- Chưa mở rộng pilot sang role khác.
- MockAiProvider removal vẫn NO-GO.
- M-R04 có margin thấp; không thay threshold baseline.

## 10. Quyết định

**GO** cho tiếp tục nghiệm thu giới hạn Development/Test.

**PENDING_FINAL_SIGNOFF** cho manager pilot; hai intent chưa đánh dấu APPROVED.

**NO-GO** cho production rollout và loại bỏ MockAiProvider.
