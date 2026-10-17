import json
from pathlib import Path

import numpy as np


root = Path("artifacts")
dotnet = json.loads((root / "dotnet-results.json").read_text(encoding="utf-8"))
python = json.loads((root / "python-results.json").read_text(encoding="utf-8"))

dotnet_inputs = dotnet["inputs"]
python_inputs = python["inputs"]
ids_equal = all(a["ids"] == b["ids"] for a, b in zip(dotnet_inputs, python_inputs))
mask_equal = all(a["attentionMask"] == b["attention_mask"] for a, b in zip(dotnet_inputs, python_inputs))
vectors_a = np.asarray(dotnet["vectors"], dtype=np.float32)
vectors_b = np.asarray(python["vectors"], dtype=np.float32)
delta = np.abs(vectors_a - vectors_b)
cosines = (vectors_a * vectors_b).sum(axis=1)

print(f"TOKEN_IDS_EQUAL={ids_equal}")
print(f"ATTENTION_MASKS_EQUAL={mask_equal}")
print(f"VECTOR_SHAPES={vectors_a.shape}/{vectors_b.shape}")
print(f"MAX_ABS_VECTOR_DELTA={delta.max():.9e}")
print(f"MEAN_ABS_VECTOR_DELTA={delta.mean():.9e}")
print(f"MIN_CROSS_IMPLEMENTATION_COSINE={cosines.min():.9f}")
print(f"MAX_CROSS_IMPLEMENTATION_COSINE_DELTA={np.abs(1.0 - cosines).max():.9e}")
print(f"DOTNET_REPEAT_DELTA={dotnet['maxRepeatAbsDelta']:.9e}")
print(f"PYTHON_REPEAT_DELTA={python['max_repeat_abs_delta']:.9e}")
