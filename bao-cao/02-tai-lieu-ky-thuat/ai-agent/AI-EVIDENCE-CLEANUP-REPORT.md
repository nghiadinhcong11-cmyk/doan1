# AI Evidence Cleanup Report

**Đối chiếu:** 17/10/2026

## Files kept

Giữ final report, index, baseline manifest, toàn bộ dataset/reference/holdout, ONNX model/tokenizer, spike/harness, raw failure evidence, UAT checklist, live log, logging implementation, role-scope audit và future roadmap.

## Files updated

Bổ sung `Current status / Superseded by` cho các báo cáo `AI-*.md` liên quan. Kết quả lịch sử và ngày audit cũ không bị sửa.

Đồng bộ trạng thái bằng ghi chú mới trong:

- `bao-cao/01-khoa-luan/ban-chinh/14_AI_he_thong_hien_tai.md`
- `bao-cao/02-tai-lieu-ky-thuat/system-audit/10-AI-INTEGRATION.md`
- `bao-cao/01-khoa-luan/chuong-4/CHAPTER-4-E2E-VERIFICATION.md`
- `bao-cao/01-khoa-luan/chuong-4/CHAPTER-4-EVIDENCE-AUDIT.md`
- `bao-cao/01-khoa-luan/chuong-4/CHAPTER-4-MODULE-CODE-MAP.md`
- `bao-cao/01-khoa-luan/ban-chinh/THESIS-SOURCE-OF-TRUTH-AUDIT.md`

Các ghi chú phân biệt Mock/provider wording lịch sử với manager-only E5 pilot hiện tại.

## Files archived

**None.** Không có thư mục archive AI đã được xác định chắc chắn; các báo cáo lịch sử chứa evidence riêng.

## Files deleted

Đã xóa các output có thể tái tạo, không được source/harness hiện tại tham chiếu:

- `.uat-failure-build/`
- `.pilotbuild/`
- `.role-scope-build/`
- `.uatloggingbuild/`
- `.uatloggingbuild2/`
- `logs/ai-uat/rotation-check.log*`

**NO_SAFE_DELETE_CANDIDATES** áp dụng cho reports, datasets, model/tokenizer, harness, raw failure evidence và live UAT log.

## Dependency/verification

- Static reference search trước cleanup.
- Baseline manifest, model/tokenizer và harness vẫn tồn tại.
- Live correlation: 5 IDs, 5 HTTP 200.
- Regression: 5/5 PASS.
- Runtime harness: 32 PASS / 0 FAIL.
- Không database, migration, browser, API restart hoặc source nghiệp vụ.
