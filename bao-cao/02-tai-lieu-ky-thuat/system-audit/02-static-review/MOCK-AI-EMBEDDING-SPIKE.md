# Mock AI Sentence Embedding Spike

## Tạm dừng chuẩn bị so sánh model — 2026-10-15

Giữ E5/max-targeted-revision-v2 làm mốc, chưa tích hợp runtime. Ứng viên MiniLM multilingual L12 v2 (revision e8f8c211226b894fcb81acc59f3b34ba3efd5f42) đã tải xong O4/tokenizer/metadata vào `artifacts/minilm/` trước khi nhận yêu cầu dừng, nhưng **chưa load/inference/scoring/benchmark/so sánh**. Session tải71907 đã kết thúc exit0; không còn tác vụ đó chạy, không dừng process người dùng. E5 `artifacts/onnx/model.onnx` chuẩn vẫn là file tải dở cũ, không dùng hoặc tự xóa.

Kiểm kê file/bytes/SHA256 thực tế, hai snapshot RAM trống~561→450MiB, Git state và quy tắc tiếp tục nằm đầu [calibration report](MOCK-AI-EMBEDDING-CALIBRATION.md). Số đo E5 ở các mục dưới là **lịch sử**, không phải số đo mới hoặc kết quả MiniLM. Chỉ cập nhật tài liệu bàn giao; không tải thêm, sửa code, chạy build/inference/API/database/migration/seed hoặc commit/push trong lượt tạm dừng. Phiên sau đọc `chatgpt-context/04-WORK-HANDOFF.md` trước, kiểm tra artifact/RAM rồi mới quyết định có tiếp tục đúng một ứng viên hay ghi BLOCKED.

## 1. Trạng thái và phạm vi

Spike này chạy độc lập trong `.tools/mock-ai-embedding-spike/`, không được API/frontend tham chiếu. Không sửa `MockAiProvider`, `MockAiIntentRecognizer`, orchestrator, tool registry, permission, confirmation hoặc service nghiệp vụ.

Mốc repository: HEAD `38bbd423b963aaf5779d4ae3356f942ff236643b`. Working tree đã dirty trước khi bắt đầu; các thay đổi người dùng được giữ nguyên.

Đã thực hiện:

- restore package .NET;
- tải artifact model/tokenizer có nguồn từ Hugging Face;
- chạy ONNX Runtime trong .NET;
- chạy implementation tham chiếu tối thiểu bằng Python `tokenizers + onnxruntime + numpy`;
- so sánh token IDs, attention masks, pooling, normalization và vector;
- đo khởi tạo, warm inference, batch inference và process working set.

Không thực hiện:

- không chạy API/frontend/database;
- không gọi AI tool nghiệp vụ;
- không chạy 104 manual case;
- không tích hợp embedding vào chat;
- không commit/push.

## 2. Project và cách chạy lại

Project thử nghiệm:

- `.tools/mock-ai-embedding-spike/MockAiEmbeddingSpike.csproj`
- `.tools/mock-ai-embedding-spike/Program.cs`
- `.tools/mock-ai-embedding-spike/reference.py`
- `.tools/mock-ai-embedding-spike/compare.py`

Các artifact/cache/build output nằm dưới `.tools/mock-ai-embedding-spike/artifacts/`, `.venv/`, `.venv312/`, `bin/`, `obj/` và đã được thêm vào `.gitignore`. Binary lớn không được đưa vào Git.

### .NET

```powershell
cd .tools/mock-ai-embedding-spike
dotnet restore MockAiEmbeddingSpike.csproj
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore
dotnet run --no-build -c Release -- --artifact-root artifacts --model artifacts/onnx/model_O4.onnx --output artifacts/dotnet-results.json
```

### Implementation tham chiếu

Python 3.12 virtual environment chỉ cài:

- `tokenizers==0.21.4`
- `onnxruntime==1.22.1`
- `numpy==2.2.4`

```powershell
.\.venv312\Scripts\python.exe reference.py artifacts
.\.venv312\Scripts\python.exe compare.py
```

Python 3.14/MinGW không có wheel tokenizer phù hợp và không được dùng để kết luận; nó chỉ được ghi nhận là môi trường không phù hợp cho reference run.

## 3. Artifact, provenance và checksum

Nguồn:

- Model repository: [intfloat/multilingual-e5-small](https://huggingface.co/intfloat/multilingual-e5-small)
- Revision API tại thời điểm tải: `614241f622f53c4eeff9890bdc4f31cfecc418b3`
- Model repository declares the model license as MIT; tokenizer/runtime/package licenses still need a separate release review.
- Model card hướng dẫn prefix: [README.md](https://huggingface.co/intfloat/multilingual-e5-small/blob/main/README.md)
- Tokenizer config: [tokenizer_config.json](https://huggingface.co/intfloat/multilingual-e5-small/blob/main/onnx/tokenizer_config.json)

Artifact được dùng:

| File | Kích thước | SHA-256 |
|---|---:|---|
| `onnx/model_O4.onnx` | 235,052,531 bytes | `4654C156F3E4171ABC9C716CDB771BF9116455D15AC1AAB364AEEEDE0E3205B0` |
| `onnx/tokenizer.json` | 17,082,730 bytes | `0B44A9D7B51C3C62626640CDA0E2C2F70FDACDC25BBBD68038369D14EBDF4C39` |
| `onnx/tokenizer_config.json` | 443 bytes | `A1D6BC8734A6F635DC158508BEF000F8E2E5A759C7D92F984B2C86E5FF53425B` |
| `onnx/config.json` | 653 bytes | `BBB7C1333FC4B3E27FBC9CD5D2070AABCC1D4DFB99917C3633E772F97545A6B6` |

`model_O4.onnx` là biến thể ONNX chính thức trong cùng repository và được chọn để giảm kích thước spike. Artifact `onnx/model.onnx` chuẩn cũng được bắt đầu tải nhưng bị treo ở trạng thái tải dở dang; không dùng nó để đo hoặc kết luận.

Package .NET:

- .NET SDK `10.0.200`
- Target spike `net8.0`
- `Microsoft.ML.OnnxRuntime 1.27.1`
- `Tokenizers.HuggingFace 3.23.1`

Tài liệu runtime: [ONNX Runtime C# API](https://onnxruntime.ai/docs/get-started/with-csharp). Tài liệu này xác nhận C# binding, CPU package và khuyến nghị tái sử dụng `InferenceSession`; spike đã dùng một session cho toàn bộ phép đo.

## 4. Tokenizer, prefix, input và pooling

### Tokenizer

Spike dùng trực tiếp `tokenizer.json` của model bằng:

- C#: `Tokenizers.HuggingFace.Tokenizer.Tokenizer.FromFile()`
- Python: `tokenizers.Tokenizer.from_file()`

Không có tách từ thủ công.

Các câu được thêm prefix `query: ` cho cả câu nhập và câu mẫu. Model card E5 hướng dẫn:

- dùng `query:`/ `passage:` cho retrieval bất đối xứng;
- dùng `query:` cho semantic similarity/paraphrase/clustering.

Bài toán intent matching ở spike là so sánh câu tương đồng, nên dùng `query:` cho hai phía. Prefix này không phải một token duy nhất: tokenizer XLM-R tách thành các ID bắt đầu `0, 41, 1294, 12`, tương ứng special token và các mảnh của `query:`.

### Input đã xác minh

ONNX Runtime in ra:

```text
input_ids:       int64 [-1, -1]
attention_mask:  int64 [-1, -1]
token_type_ids:  int64 [-1, -1]
last_hidden_state: float [-1, -1, 384]
```

Với câu `Hôm nay doanh thu được bao nhiêu?`:

- token count: 13;
- first IDs: `0,41,1294,12,144779,7630,7873,4911,...`;
- attention mask đều bằng 1;
- special token đầu là `<s>` ID 0;
- special token cuối là `</s>` ID 2 khi câu encoding có token cuối hiển thị.

### Pooling/normalization

Spike thực hiện đúng pipeline model card:

1. lấy `last_hidden_state` có shape `[batch, sequence, 384]`;
2. nhân theo `attention_mask`;
3. mean-pool chỉ trên token mask bằng 1;
4. L2-normalize vector 384 chiều.

Không dùng hidden state của token CLS riêng lẻ. Không mean-pool padding.

### Giới hạn token

Tokenizer config công bố `model_max_length=512`. Spike kiểm tra câu dài:

```text
raw token count: 605
used token count: 512
truncated: True
last token: </s>
```

Khi vượt giới hạn, spike giữ 511 token đầu và token special cuối `</s>`. Đây là logic kiểm chứng tạm thời; phase tích hợp cần dùng cấu hình truncation chính thức của tokenizer adapter thay vì để parser tự xử lý.

## 5. Đối chiếu .NET với implementation tham chiếu

Hai implementation chạy cùng:

- `model_O4.onnx`;
- `tokenizer.json`;
- prefix `query: `;
- input IDs, attention mask, token type IDs;
- attention-mask-aware mean pooling;
- L2 normalization.

Kết quả trên 9 câu kỹ thuật:

| Kiểm tra | Kết quả |
|---|---:|
| Token IDs giống nhau | `True` |
| Attention masks giống nhau | `True` |
| Shape .NET/reference | `(9,384)/(9,384)` |
| Sai lệch vector tuyệt đối lớn nhất | `7.382780313e-05` |
| Sai lệch vector tuyệt đối trung bình | `1.422257174e-05` |
| Cosine thấp nhất giữa hai implementation | `0.999999881` |
| Sai lệch cosine lớn nhất | `1.192092896e-07` |
| Sai lệch khi chạy lại .NET | `0` |
| Sai lệch khi chạy lại Python | `0` |
| Norm vector .NET đầu tiên | `0.999999970` |
| Norm vector Python đầu tiên | `1.000000000` |

Sai lệch nhỏ còn lại phù hợp với khác biệt floating-point/runtime kernel; đây không phải sai khác tokenizer hoặc pooling. Kết quả đủ chứng minh implementation .NET đang thực hiện cùng phép tính với reference cho artifact đã chọn. Đây vẫn chưa phải benchmark production và chưa chứng minh chất lượng phân loại intent.

## 6. Đo tài nguyên thực tế

Máy chạy spike:

- Windows 10/11 x64;
- .NET SDK `10.0.200`;
- target `net8.0`;
- Python reference 3.12.5;
- không đo trên máy production/deployment.

### .NET ONNX Runtime, `model_O4.onnx`

| Đo | Kết quả |
|---|---:|
| Model file | 235.05 MB |
| Tokenizer JSON | 17.08 MB |
| Session initialization | 2,248.311 ms |
| Single warm inference, n=20, min | 4.279 ms |
| Single warm inference, median | 5.422 ms |
| Single warm inference, p95 | 7.565 ms |
| Single warm inference, max | 7.719 ms |
| Batch 9 warm inference, n=10, min | 33.437 ms |
| Batch 9 warm inference, median | 41.763 ms |
| Batch 9 warm inference, p95 | 42.577 ms |
| Batch 9 warm inference, max | 42.830 ms |
| Process working set trước session | 26.9 MB |
| Process working set sau session | 616.8 MB |
| Tăng working set quan sát được | khoảng 590 MB |

### Python reference tham khảo

| Đo | Kết quả |
|---|---:|
| Session initialization | 721.997 ms |
| Single warm median, n=20 | 24.983 ms |
| Single warm p95 | 26.426 ms |
| Batch 9 warm median, n=10 | 68.934 ms |
| Batch 9 warm p95 | 71.509 ms |

Các số Python và .NET không phải phép so sánh hiệu năng công bằng tuyệt đối vì runtime, thread pool, execution provider và process khác nhau. Chúng chỉ cho thấy cả hai implementation chạy được và giúp phát hiện sai khác logic. Số .NET working set là quan sát một process trên máy này, không phải yêu cầu RAM chính thức cho server.

Không đo cold-start toàn bộ deployment, concurrent requests, nhiều session process, container limit, GPU hoặc ARM. Chưa thể kết luận môi trường production đủ tài nguyên.

## 7. Bảng tương đồng minh họa

Các câu dưới đây là tập minh họa spike, không phải `EMB-E01..EMB-E30`, không dùng để chọn threshold chính thức.

| Cặp | Ý nghĩa | Cosine |
|---|---|---:|
| Doanh thu hôm nay ↔ tiền bán hàng hôm nay | cùng intent/paraphrase | 0.922996 |
| Doanh thu hôm nay ↔ so sánh doanh thu | intent gần nhau | 0.926378 |
| Doanh thu hôm nay ↔ món bán chạy | intent khác | 0.843845 |
| Doanh thu hôm nay ↔ kiểm tra tồn kho | ngoài phạm vi gần nghiệp vụ | 0.817911 |
| Doanh thu hôm nay ↔ phủ định doanh thu | phủ định | 0.901638 |
| Thực đơn hôm nay ↔ “còn hôm qua thì sao?” | khác intent/ngữ cảnh thiếu | 0.877102 |

Kết quả cho thấy embedding có thể đưa câu cùng nghĩa lên cao, nhưng cũng cho thấy rủi ro:

- câu revenue và comparison có cosine cao hơn paraphrase revenue trong tập nhỏ;
- câu phủ định vẫn rất gần câu dương;
- câu ngoài phạm vi không tự rơi xuống 0;
- không thể đặt threshold chỉ dựa vào vài điểm minh họa.

Vì vậy vẫn cần negative examples, top-1/margin, parser và rule phủ định/multi-intent. Không chốt threshold từ bảng này.

## 8. Phần đã xác minh và chưa xác minh

### Đã xác minh

- Artifact ONNX chính thức của model chạy được trong .NET.
- Tokenizer Hugging Face tương thích chạy được trong .NET.
- Input types/names và output `last_hidden_state` đúng.
- Token IDs và attention masks .NET khớp reference Python.
- Attention-mask-aware pooling và L2 normalization khớp reference.
- Vector dimension 384.
- Chạy lặp ổn định trên input thử nghiệm.
- Prefix `query:` dùng được cho semantic similarity theo hướng dẫn model card.
- Model 512-token truncation đã được kiểm tra trong spike.
- Session được reuse; build output và artifact không nằm trong Git.

### Chưa xác minh

- Chất lượng intent tiếng Việt trên dữ liệu calibration/evaluation đủ lớn.
- Threshold/margin tối ưu.
- Recall/precision hoặc false-positive rate.
- Chất lượng với telex, typo, tên món thật và câu nhiều ý.
- Hiệu năng concurrent production.
- Tài nguyên trên máy triển khai.
- Chính sách model artifact/license/provenance cuối cùng của hệ thống.
- Cách tích hợp tokenizer/truncation vào runtime application.
- Fallback khi model không tải được.

## 9. Kết luận

**Spike đạt điều kiện kỹ thuật để tiếp tục thiết kế adapter**, chưa đạt điều kiện tích hợp production.

Có thể kết luận:

- multilingual-e5-small ONNX artifact đã chạy đúng trong .NET;
- tokenizer, prefix, input IDs, attention mask, pooling và normalization đã được đối chiếu độc lập;
- sai lệch vector so với reference rất nhỏ;
- session reuse hoạt động và latency warm trên máy spike ở mức vài ms cho một câu;
- memory working set sau load khoảng 617 MB trên máy này, cần xem là rủi ro tài nguyên cho deployment.

Trước khi tích hợp vào Mock chat cần làm tiếp:

1. giữ riêng tập mẫu/calibration/evaluation;
2. đo chất lượng tiếng Việt và cặp intent gần nhau;
3. chốt false-positive budget và threshold/margin từ dữ liệu;
4. thiết kế phủ định, multi-intent, parser tham số và model-unavailable policy;
5. xác minh memory/concurrency trên môi trường triển khai;
6. chỉ sau đó thêm recognizer strategy phía sau `MockAiProvider`.

Trạng thái handoff: **Spike chạy được và đã đối chiếu reference; sentence embedding chưa tích hợp vào chat.**
