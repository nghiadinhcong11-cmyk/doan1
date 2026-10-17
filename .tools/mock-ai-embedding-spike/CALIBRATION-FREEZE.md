# Decision review data freeze

The supplemental set was authored before any scoring of that set in this review.
Do not change samples, thresholds or policy after seeing its results without marking
the supplemental set as development data. No final EMB evaluation set was consulted.

- Original `calibration.json` remains byte-identical. SHA256:
  `E68AD7CF07EF403D5479F37B8360B12AD0224C0704377EF17B75097F0D7CB5B0`.
- Supplemental `calibration-supplement.json`: 18 cases, texts, actors, expected intent,
  expected decision and rationale frozen before execution. SHA256:
  `262F921C9B1C3E1C1F94B0FF9D7A2B9D0839658161554FAEA68311B1FD305ED2`.
- Existing labels are not rewritten. `calibration-behavior.json` separately records
  a source-based expected decision. This corrects the earlier conflation of correct
  intent, complete parameters, capability, ownership and business success.
- Same exploratory score/margin: 0.85/0.02. No threshold sweep.
- Supplemental expectations describe readiness for backend checks; backend authorization
  and business execution remain untested. An accepted semantic candidate is not permission.

## Execution record

The supplemental set was scored once. Its output is
`artifacts/decision-review-supplement-once.json` and a full textual snapshot is in
`MOCK-AI-EMBEDDING-CALIBRATION.md`. No semantic tuning followed this run. Final semantic
file hashes for this observation:

- IntentCatalog.cs: `EEE514F49A3B0324F1B89224C2A28AE4CDC7462F6E184AB9A9A25722D6C19AC6`
- IntentDecisionPipeline.cs: `CD8D027D9477AB8B4A8641B3C3C9A3B12FDAAD5BB232CA3A8006E01D9B2B7E14`
- IntentScorer.cs: `E170EEA5C78B12249A01C2EEDF0177DD7E975CD4F10CFD849A9569FC1A72A56F`

The later adapter lifecycle checks change no semantic policy. Repeated scoring after
further tuning must be described as development results, not fresh independent evidence.

## Coverage diagnosis follow-up

Both CAL and SUP are now observed development data. The next diagnostic run adds six
examples only to MENU_QUERY/REVENUE_QUERY/MY_ORDERS in a separate
`three-intent-examples` profile. All 34 baseline examples and competing/unsupported
intents remain present. `full-baseline` remains the default. Threshold .85/.02,
scoring, guards, parser and all expected labels/decisions stay unchanged.
This single intervention is recorded before running it; its output must not be
described as an independent generalization evaluation.

The intervention ran once per existing dataset. CAL correct accept changed 7 to 8;
SUP changed 1 to 3, with zero false accepts in these observed sets. However SUP
top-1 on valid requests fell 7/8 to 5/8 (SUP-05 and SUP-18 regressed). The default
remains full-baseline. No second tuning round or threshold change followed.

## Subsequent scoring-only comparison

The next task held full-baseline, all CAL/SUP texts/labels, O4 model, parser and
guards fixed. Only max versus mean-of-best-two scoring was compared, both at
.85/.02. Sample/decision/adapter file hashes were checked unchanged. Mean improved
some accepted counts but regressed SUP-15 intent and CAL-19/20 refusal decisions;
max remains the default. No new samples, threshold search or other algorithms were
used. These are development observations, not independent evaluation.

## Bounded threshold round completed

`profiles/threshold-trials-v1.json` was recorded before execution: baseline
.85/.02 and exactly three additional margins .015, .01, .005 (score .85 fixed).
No scorer/catalog/parser/guard/model/data changes were made during this round.
The selected frozen manifest is `profiles/max-full-baseline-candidate-v1.json`:
max/full-baseline/.85/.01, experimental and not deployed. The default stays .85/.02.
Its SHA256 is `F598F036DAEBC6299BD04C155C55E35A90A41FFBD3B4460BD8046DAB86FDE861`.
No final EMB evaluation cases were opened/run. The round is closed; future evaluation
must hold this configuration fixed, and any retuning requires a new version and
reclassification of the used evaluation data as development evidence.
