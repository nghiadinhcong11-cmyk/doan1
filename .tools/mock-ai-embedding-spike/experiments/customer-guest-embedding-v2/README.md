# Customer/Guest E5 Embedding v2

Offline research only. This experiment does not call `AiToolRegistry`, `AiPermissionService`, application services or a database.

The canonical E5 model is reused from `../../artifacts/onnx/model_O4.onnx` with `../../artifacts/onnx/tokenizer.json`. The dataset keeps only two Customer intents (`MENU_QUERY`, `MY_ORDERS`) and one Guest intent (`MENU_QUERY`).

The `category` values are evaluator labels for extraction (`do_uong`, `khai_vi`, `mon_chinh`, `mon_an`), not an authoritative database category list. A runtime adapter must resolve the extracted phrase against the branch's real category data and must clarify when it cannot do so. The current `customer_get_menu` contract supports only optional `category`; item-name lookup, price-range filtering, inventory quantity and availability-by-date are not silently claimed.

Splits are independent. An initial draft holdout was discarded after diagnostic review; the current `holdout.json` is the final frozen holdout and was created before the final evaluation. It must not be used for tuning. Run from repository root:

```text
dotnet run --project .tools/mock-ai-embedding-spike/MockAiEmbeddingSpike.csproj --configuration Release --no-build -- --customer-guest-embedding-eval --customer-guest-root .tools/mock-ai-embedding-spike/experiments/customer-guest-embedding-v2
```

Results are offline evidence only. They do not replace backend authorization, verified Guest QR session checks, customer own-order isolation, branch isolation or manual UAT.
