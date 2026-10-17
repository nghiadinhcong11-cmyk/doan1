import json
import os
import sys
import time
from pathlib import Path

import numpy as np
import onnxruntime as ort
from tokenizers import Tokenizer


TEXTS = [
    "Hôm nay doanh thu được bao nhiêu?",
    "Tiền bán hàng trong ngày hiện tại là bao nhiêu?",
    "So sánh doanh thu hôm nay với hôm qua.",
    "Món nào đang bán chạy nhất tuần này?",
    "Tóm tắt tình hình kinh doanh tháng này.",
    "Tôi muốn xem thực đơn hôm nay.",
    "Còn hôm qua thì sao?",
    "Kiểm tra tồn kho nguyên liệu.",
    "Tôi không muốn xem doanh thu hôm nay.",
]


def encode(tokenizer, text):
    encoded = tokenizer.encode("query: " + text, add_special_tokens=True)
    ids = encoded.ids
    tokens = encoded.tokens
    if len(ids) > 512:
        ids = ids[:511] + ids[-1:]
        tokens = tokens[:511] + tokens[-1:]
    return {
        "text": text,
        "ids": ids,
        "attention_mask": [1] * len(ids),
        "tokens": tokens,
    }


def embed(session, batch):
    max_length = max(len(item["ids"]) for item in batch)
    ids = np.zeros((len(batch), max_length), dtype=np.int64)
    mask = np.zeros((len(batch), max_length), dtype=np.int64)
    type_ids = np.zeros((len(batch), max_length), dtype=np.int64)
    for row, item in enumerate(batch):
        length = len(item["ids"])
        ids[row, :length] = item["ids"]
        mask[row, :length] = item["attention_mask"]

    outputs = session.run(None, {
        "input_ids": ids,
        "attention_mask": mask,
        "token_type_ids": type_ids,
    })
    hidden = outputs[0]
    pooled = (hidden * mask[:, :, None]).sum(axis=1) / mask.sum(axis=1, keepdims=True)
    normalized = pooled / np.linalg.norm(pooled, axis=1, keepdims=True)
    return normalized.astype(np.float32)


def stats(values):
    values = sorted(values)
    return {
        "n": len(values),
        "min": float(values[0]),
        "median": float(np.percentile(values, 50)),
        "p95": float(np.percentile(values, 95)),
        "max": float(values[-1]),
    }


def main():
    root = Path(sys.argv[1] if len(sys.argv) > 1 else "artifacts")
    model = root / "onnx" / "model_O4.onnx"
    tokenizer_path = root / "onnx" / "tokenizer.json"
    output_path = root / "python-results.json"
    tokenizer = Tokenizer.from_file(str(tokenizer_path))
    start = time.perf_counter()
    session = ort.InferenceSession(str(model), providers=["CPUExecutionProvider"])
    init_ms = (time.perf_counter() - start) * 1000
    encoded = [encode(tokenizer, text) for text in TEXTS]
    vectors = embed(session, encoded)
    repeat = embed(session, encoded)
    print(f"SESSION_INIT_MS={init_ms:.3f}")
    print(f"INPUTS={[(x.name, x.type, x.shape) for x in session.get_inputs()]}")
    print(f"OUTPUTS={[(x.name, x.type, x.shape) for x in session.get_outputs()]}")
    print(f"VECTOR_DIMENSION={vectors.shape[1]}")
    print(f"VECTOR_NORM_FIRST={np.linalg.norm(vectors[0]):.9f}")
    print(f"REPEAT_MAX_ABS_DELTA={np.max(np.abs(vectors - repeat)):.9e}")
    for item in encoded:
        print(f"TOKENS|{item['text']}|count={len(item['ids'])}|ids={item['ids'][:12]}|tokens={item['tokens'][:12]}")

    embed(session, encoded[:1])
    single = []
    for _ in range(20):
        start = time.perf_counter()
        embed(session, encoded[:1])
        single.append((time.perf_counter() - start) * 1000)
    batch = []
    for _ in range(10):
        start = time.perf_counter()
        embed(session, encoded)
        batch.append((time.perf_counter() - start) * 1000)
    print(f"SINGLE_WARM_LATENCY_MS={stats(single)}")
    print(f"BATCH_{len(encoded)}_WARM_LATENCY_MS={stats(batch)}")

    result = {
        "model": model.name,
        "tokenizer": tokenizer_path.name,
        "prefix": "query: ",
        "pooling": "attention-mask-aware mean pooling over last_hidden_state, then L2 normalization",
        "dimension": int(vectors.shape[1]),
        "inputs": encoded,
        "vectors": vectors.tolist(),
        "max_repeat_abs_delta": float(np.max(np.abs(vectors - repeat))),
        "single_warm_latency_ms": stats(single),
        "batch_warm_latency_ms": stats(batch),
    }
    output_path.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"RESULT_JSON={output_path}")


if __name__ == "__main__":
    main()
