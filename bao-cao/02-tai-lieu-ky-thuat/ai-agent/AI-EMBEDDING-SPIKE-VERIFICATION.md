# Xác minh lại Sentence Embedding E5 spike (.NET/ONNX)

**Thời điểm:** 2026-10-17 03:46 UTC (10:46 ICT). **HEAD:** `38bbd423b963aaf5779d4ae3356f942ff236643b`. Working tree đã dirty; toàn bộ `.tools/mock-ai-embedding-spike/` là untracked. Audit này chỉ thêm báo cáo này. Đọc cùng `AI-EMBEDDING-CURRENT-STATE-AUDIT.md`; kết quả ở đây là chạy lại **component spike hiện tại**, không phải AI chat/API hay kiểm tra nghiệp vụ.

## 1. Kết luận ngắn

| Hạng mục | Trạng thái | Bằng chứng mới |
|---|---|---|
| Source spike build độc lập | **PASS** | Build `net8.0` từ bản sao source bằng `--no-restore`: 0 warning, 0 error; không chạm output API. |
| Model E5/tokenizer/dependency hiện diện và load được | **PASS** | SHA-256 khớp manifest cũ; .NET và Python đều load cùng artifact, thấy input int64 và output `[batch,sequence,384]`. |
| Inference .NET + adapter lifecycle | **PASS** | 9 vector 384 chiều, norm ≈1, repeat delta 0; 7/7 lifecycle check trả `true`. |
| Token/mask và vector parity .NET/Python | **PASS trên 9 câu minh họa sẵn có** | IDs/mask bằng nhau; max abs delta `7.382780313e-05`, cross cosine tối thiểu `0.999999881`. |
| Cosine/threshold/abstain | **PASS về hành vi code và runner** | CAL/SUP cũ chạy xong; low score, close margin, negation rẽ sang unsupported/clarification. Không suy ra chất lượng ngữ nghĩa. |
| .NET encode latency tách riêng | **NOT RUN** | Runner hiện đo inference với input đã encode; không có timer encode .NET độc lập. |
| AI API, database, integration, tập đánh giá hai intent độc lập | **NOT RUN** | Ngoài phạm vi yêu cầu. |

**Đủ điều kiện kỹ thuật để bắt đầu *thiết kế* thực nghiệm hai intent ở mức spike**, vì artifact, inference và parity hiện chạy được. **Chưa đủ bằng chứng để kết luận ngữ nghĩa/độ chính xác, chọn threshold triển khai hay tích hợp chat.** Không tạo dataset, không chọn hai intent trong lượt này.

## 2. Source, provenance và đồng bộ artifact

- Project `.tools/mock-ai-embedding-spike/MockAiEmbeddingSpike.csproj:1-13` target `net8.0`, package `Microsoft.ML.OnnxRuntime 1.27.1` và `Tokenizers.HuggingFace 3.23.1`. Cache restore `obj/project.assets.json` có sẵn; `dotnet --version` hiện `10.0.200`. Python `.venv312`: Python 3.12.5, `tokenizers 0.21.4`, `onnxruntime 1.22.1`, `numpy 2.2.4` (lệnh ở mục 5).
- `config.json` khai báo `_name_or_path=intfloat/multilingual-e5-small`, hidden size 384, max positions 512, tokenizer class XLM-R; `1_Pooling/config.json` bật mean pooling, tắt CLS/max; `modules.json` nối Transformer → Pooling → Normalize (`.tools/mock-ai-embedding-spike/artifacts/onnx/config.json:2-25`; `artifacts/1_Pooling/config.json:1-6`; `artifacts/modules.json:1-20`). Model card **đã cache tại máy** nói dùng `query: ` cho nhiệm vụ ngoài retrieval (`artifacts/README.md:18337-18338`). Không truy cập Hugging Face trong lượt này.
- Revision nguồn `614241f622f53c4eeff9890bdc4f31cfecc418b3` được ghi trong `profiles/max-targeted-revision-v2.json:16-23` và báo cáo lịch sử `system-audit/02-static-review/MOCK-AI-EMBEDDING-SPIKE.md:67-86`. **INSPECTED provenance nội bộ**: không xác thực revision trực tiếp với remote. `transformers_version=4.30.2` trong `config.json` là metadata của model config, không phải version ONNX Runtime hiện dùng.

| Artifact đã đọc từ `.tools/mock-ai-embedding-spike/artifacts/onnx/` | SHA-256 đo lại | So với báo cáo cũ |
|---|---|---|
| `model_O4.onnx` | `4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0` | Khớp |
| `tokenizer.json` | `0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39` | Khớp |
| `tokenizer_config.json` | `A1D6BC8734A6F635DC158508BEF000F8E2E5A759C7D92F984B2C86E5FF53425B` | Khớp |
| `config.json` | `BBB7C1333FC4B3E27FBC9CD5D2070AABCC1D4DFB99917C3633E772F97545A6B6` | Khớp |

Các checksum tham chiếu ở `MOCK-AI-EMBEDDING-SPIKE.md:77-84`, model/tokenizer ở manifest `profiles/max-targeted-revision-v2.json:16-23`. Đây là bằng chứng artifact trên máy **không đổi so với snapshot cũ**; parity thực chạy ở mục 3 là bằng chứng mạnh hơn rằng cặp model/tokenizer *hoạt động đồng bộ trên 9 input có sẵn*. Nó không chứng minh mọi input hoặc provenance remote. `model.onnx` không được dùng. MiniLM trong `artifacts/minilm/` không được load hoặc so sánh.

## 3. Preprocessing, inference và parity

`SentenceEmbeddingOptions` mặc định prefix `query: `, max 512, concurrency 1 (`SentenceEmbeddingOptions.cs:3-9`). Adapter dùng `Tokenizer.FromFile`, special tokens, lấy IDs/mask; chuỗi vượt 512 giữ 511 token đầu và special token cuối (`SentenceEmbeddingAdapter.cs:38-45,71-92`). Nó cấp ba tensor int64 `input_ids`, `token_type_ids` (0), `attention_mask`, chạy `InferenceSession.Run` rồi masked mean-pool `last_hidden_state` và L2-normalize (`SentenceEmbeddingAdapter.cs:159-203`). Python reference độc lập dùng cùng prefix, giới hạn và công thức (`reference.py:26-58,77-79`). Cấu hình pooling/normalize hiện có phù hợp cách tính này. Logic truncation là code riêng của spike; parity 9 câu ngắn **không kiểm hết** hành vi tokenizer khi sát/vượt 512. Runner .NET riêng có kiểm `605 → 512`, token cuối `</s>` (`Program.cs:127-143`, output mới).

**Kết quả chạy lại, cùng 9 câu minh họa hardcode sẵn trong `Program.cs:27-38` và `reference.py:12-22`:**

| Phép kiểm | .NET | Python | Kết luận |
|---|---:|---:|---|
| Model inputs | 3 int64, shape động | 3 int64, shape động | PASS |
| Output | `last_hidden_state`, float `[−1,−1,384]` | cùng shape | PASS |
| Vector đầu tiên L2 norm | `0.999999970` | `1.000000000` | PASS |
| Lặp lại max abs delta | `0` | `0` | PASS trong cùng process |
| IDs / attention masks | — | `True` / `True` từ `compare.py` | PASS |
| Shapes | `[9,384]` | `[9,384]` | PASS |
| Vector max/mean abs delta | `7.382780313e-05` / `1.422257174e-05` | so sánh hai runtime | PASS sai số số học nhỏ |
| Min cross cosine / max delta từ 1 | `0.999999881` / `1.192092896e-07` | so sánh hai runtime | PASS |

`compare.py:12-30` so ID, mask và vector; script **không đặt assert/exit code theo tolerance**. PASS ở đây là đánh giá các giá trị thực xuất ra, không phải chỉ dựa vào exit code. Similarity minh họa từ .NET: revenue paraphrase `0.922995806`, revenue-vs-comparison `0.926378429`, revenue-vs-ranking `0.843845308`, revenue-vs-out-of-scope `0.817911446`, revenue-vs-negative `0.901638329`, menu-vs-continuation `0.877101958` (`Program.cs:93-105`; output chạy mới). Chính việc comparison có score cao hơn paraphrase trong mẫu nhỏ nhắc rằng **cosine không phải xác suất hay nhãn đúng**.

## 4. RAM và latency hiện tại

Đo trên cùng máy Windows/CPU, một lần chạy process riêng từng runtime; không khóa CPU/load, không lập benchmark production. .NET runner lấy `Stopwatch` quanh phần load tokenizer+session và quanh `EmbedBatch` **sau khi đã encode** (`Program.cs:44-48,61-91`). Các số .NET in theo locale Windows bằng dấu phẩy thập phân; dưới đây đã đổi sang dấu chấm. RSS là `Process.WorkingSet64`, không phải peak hoặc bộ nhớ riêng của model.

| Chỉ số | .NET hiện tại | Python hiện tại | Diễn giải |
|---|---:|---:|---|
| Khởi tạo | **2,889.848 ms** | **655.578 ms** | .NET gồm tokenizer+session; Python timer chỉ quanh session sau load tokenizer, không so ngang tuyệt đối. |
| Single warm inference n=20, median / p95 | **17.995 / 25.836 ms** | **25.583 / 32.399 ms** | .NET dùng input đã encode; Python `embed` cũng dùng input đã encode. |
| Batch 9 warm n=10, median / p95 | **114.650 / 129.254 ms** | **164.215 / 201.240 ms** | Bị ảnh hưởng bởi CPU/process khác thời điểm. |
| .NET process RSS trước/sau | **26,505,216 / 609,853,440 bytes** | Không đo trong reference | Tăng **583,348,224 bytes ≈ 556.3 MiB**; sau ≈ 581.6 MiB. |
| Python tokenize 9 câu sẵn có, median / min | — | **0.305 / 0.261 ms** | `timeit.repeat`: 5 lần × 100 batch 9 câu, warmup trước; chỉ là Python encode, không thay thế số .NET. |

Số .NET cũ trong `MOCK-AI-EMBEDDING-SPIKE.md:197-226` có median warm 5.422 ms và RSS sau 616.8 MB; lần này chậm hơn. Không suy luận nguyên nhân từ một lần chạy. `Program.cs:115-123` ghi `rssBefore`/`rssAfter` trong JSON **đều lấy tại thời điểm cuối**, nên hai trường JSON không dùng cho delta RAM; dùng dòng console `PROCESS_RSS_BEFORE/AFTER` mới ở trên (`Program.cs:41-42,93-94,118-122`). **.NET encode latency riêng: NOT RUN**, vì `Encode` private và runner không có timer riêng; không sửa code để đo.

## 5. Scorer, threshold và abstain

`IntentScorer.Score` nhóm ví dụ theo intent, tính cosine bằng tích vô hướng trên vector đã normalize, lấy max mặc định hoặc trung bình top-two, sắp top/second và margin; score dưới threshold → `Unsupported`, margin thấp → `ClarificationRequired`, unsupported/write label → `Unsupported`, còn lại → `Accepted` (`IntentScorer.cs:26-59`). `DecisionReviewRunner` mặc định max/.85/.02; profile v2 cần chọn tường minh và manifest ghi max/.85/.01 (`DecisionReviewRunner.cs:29-59`; `profiles/max-targeted-revision-v2.json:3-23,64-65`). `IntentDecisionPipeline` bổ sung rule negation, multi-intent, capability/parameter/subject, và chỉ đánh dấu `ToolForBackendCheck` khi Accepted; **không thực thi tool hay kiểm quyền backend** (`IntentDecisionPipeline.cs:23-56,60-141`). Cosine threshold không được coi là độ tin cậy xác suất.

Đã chạy lại **dữ liệu CAL/SUP có sẵn**, không sửa/chọn câu:

| Run | Tham số | Outcome runner | Ví dụ hành vi kỹ thuật |
|---|---|---|---|
| CAL baseline (24) | full-baseline, max, .85/.02 | `CorrectAccept=7`, `ValidDeferred=4`, `CorrectNonAccept=13` | Runner hoạt động; đây là nhãn cũ. |
| CAL v2 (24) | targeted-revision-v2, max, .85/.01 | `CorrectAccept=9`, `ValidDeferred=2`, `CorrectNonAccept=13` | `CAL-11` score .915194, margin .008524 → `CloseCandidates`. |
| SUP v2 (18) | targeted-revision-v2, max, .85/.01 | `CorrectAccept=6`, `ValidDeferred=2`, `CorrectNonAccept=10` | `SUP-17` score .819668 → `LowSimilarity`; `SUP-18` margin .008525 → `CloseCandidates`; `SUP-10` → `NegatedRequest`. |

Đây chỉ là **kiểm tra thực thi pipeline và quyết định trên dữ liệu phát triển đã quan sát**, không phải đánh giá độ chính xác tiếng Việt mới. `CorrectAccept` là outcome của runner theo nhãn/expectation hiện có, không phải PASS nghiệp vụ. Không chạy EMB/new v2 hoặc chọn threshold khác. Adapter lifecycle hiện chạy với model thật: `busyWhenFull`, `cancelWaiting`, `slotRecoveredAfterCancellation`, `cancelResultAfterNativeWork`, `slotRecoveredAfterNativeWork`, `activeRequestSurvivesDispose`, `rejectAfterDisposeIncludingEmpty` đều `true` (`AdapterLifecycleChecks.cs:7-57`; output mới).

## 6. Lệnh, đầu ra và giới hạn kiểm chứng

Tất cả lệnh chạy offline. Để tránh ghi đè spike/API, đã sao chép **chỉ source `.cs`, `.csproj`, `obj/project.assets.json`, model O4, tokenizer và CAL/SUP cần thiết** vào `%TEMP%\ai-embedding-verify-20261017-1040`; build/output JSON nằm tại đó trong lúc đo. Đã xác minh path nằm trong `%TEMP%` và xóa thư mục tạm sau khi chép kết quả vào báo cáo (`TEMP_REMOVED=True`). Không chạy `restore`, không tải artifact/package, không sửa source, không truy cập database, không khởi động API. Các lệnh chính (PowerShell; ba lệnh đầu và lệnh encode chạy từ repo, các lệnh .NET/Python parity từ thư mục tạm):

```powershell
dotnet --version
.\.tools\mock-ai-embedding-spike\.venv312\Scripts\python.exe -B -c "import sys,tokenizers,onnxruntime,numpy; print(sys.version); print(tokenizers.__version__,onnxruntime.__version__,numpy.__version__)"
Get-FileHash .tools/mock-ai-embedding-spike/artifacts/onnx/model_O4.onnx,.tools/mock-ai-embedding-spike/artifacts/onnx/tokenizer.json,.tools/mock-ai-embedding-spike/artifacts/onnx/tokenizer_config.json,.tools/mock-ai-embedding-spike/artifacts/onnx/config.json -Algorithm SHA256
dotnet build .\src\MockAiEmbeddingSpike.csproj -c Release --no-restore --output .\bin
dotnet .\bin\MockAiEmbeddingSpike.dll --artifact-root artifacts --output artifacts/dotnet-results.json
$env:PYTHONIOENCODING='utf-8'
D:\Dev\doan\.tools\mock-ai-embedding-spike\.venv312\Scripts\python.exe -B D:\Dev\doan\.tools\mock-ai-embedding-spike\reference.py artifacts
D:\Dev\doan\.tools\mock-ai-embedding-spike\.venv312\Scripts\python.exe -B D:\Dev\doan\.tools\mock-ai-embedding-spike\compare.py
dotnet .\bin\MockAiEmbeddingSpike.dll --adapter-checks
dotnet .\bin\MockAiEmbeddingSpike.dll --decision-review --artifact-root artifacts --calibration calibration.json --review-output artifacts/decision-review-current-baseline.json --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.02
dotnet .\bin\MockAiEmbeddingSpike.dll --decision-review --artifact-root artifacts --calibration calibration.json --review-output artifacts/decision-review-current-v2.json --catalog-profile targeted-revision-v2 --scoring max --minimum-score 0.85 --minimum-margin 0.01
dotnet .\bin\MockAiEmbeddingSpike.dll --decision-review --artifact-root artifacts --calibration calibration-supplement.json --review-output artifacts/decision-review-current-v2-sup.json --catalog-profile targeted-revision-v2 --scoring max --minimum-score 0.85 --minimum-margin 0.01
```

Đo riêng Python encode bằng venv sẵn có, không tạo file: `python.exe -B -c "import sys,timeit,statistics; sys.path.insert(0,r'.tools/mock-ai-embedding-spike'); import reference; from tokenizers import Tokenizer; t=Tokenizer.from_file(r'.tools/mock-ai-embedding-spike/artifacts/onnx/tokenizer.json'); f=lambda:[t.encode('query: '+x,add_special_tokens=True) for x in reference.TEXTS]; f(); s=timeit.repeat(f,number=100,repeat=5); print(statistics.median(s)*10,min(s)*10)"`. Lệnh thực tế dùng `.venv312\Scripts\python.exe` và `PYTHONIOENCODING=utf-8`.

**FAIL rồi xử lý bằng môi trường, không sửa code:** Lần đầu chạy `reference.py` sau khi đã tính vector thất bại với `UnicodeEncodeError` của console `cp1258` khi in tiếng Việt (exit 1). Chạy lại với `PYTHONIOENCODING=utf-8` thành công (exit 0). Lần đầu chạy decision-review từ thư mục tạm cũng thất bại do chưa copy `calibration.json`/`calibration-behavior.json`; sau khi copy file đầu vào hiện có, ba run đều exit 0. Hai lỗi thao tác này không phải lỗi inference/model, được giữ trong biên bản.

**BLOCKED/NOT RUN:** Không xác thực trực tiếp revision nguồn qua mạng; không đo .NET encode riêng, peak/steady-state RAM, nhiều lần chạy/độ dao động theo tải, parity 512-token trên tập có sẵn, MiniLM, AI API và đánh giá intent hai nhóm độc lập. Không gọi `MockAiProvider`/`AiOrchestrator` và không kết luận mô hình hiểu tiếng Việt.


## Current status / Superseded by

Historical spike verification; current: retained for model/adapter evidence, superseded for acceptance by final report.
