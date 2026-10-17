# Mock AI embedding spike

This isolated console project is for local verification only. It is not referenced by the API and is not a chat test runner.

Artifacts are placed under `artifacts/` and are ignored. Use `dotnet run -- --artifact-root artifacts` after downloading the exact model/tokenizer files. The program prints model inputs, pooled/normalized vector checks, similarity examples, repeatability and resource timing.

## Decision review

Run from this directory with the already prepared artifacts:

```powershell
dotnet build MockAiEmbeddingSpike.csproj -c Release --no-restore
dotnet run --no-build -c Release -- --decision-review --review-output artifacts/decision-review-development-v2.json
dotnet run --no-build -c Release -- --decision-review --calibration calibration-supplement.json --review-output artifacts/decision-review-supplement-once.json
dotnet run --no-build -c Release -- --adapter-checks
```

`calibration.json` preserves the original 24 cases. `calibration-behavior.json` records
source-based expected decisions separately from original intent labels. The supplemental
18 cases were frozen before their first run; see `CALIBRATION-FREEZE.md`. They must be
marked development data if further tuning uses their results. The final EMB manual
evaluation set is not loaded by this project.

`IntentDecisionPipeline` keeps semantic candidates, score/margin, capability, parameter
state and pending backend authorization separate. Accepted means a candidate is ready
for backend checks; no permission or business success is asserted. No application
project references or tool executions are added.

All cached catalog vectors are held in memory for the lifetime of a scoring run.
Only clause embeddings are computed when the narrow negation/multi-intent policy
needs them. Cosine is a similarity score, not a probability.

`SentenceEmbeddingAdapter` enforces concurrency 1 because tokenizer parallel safety
is unverified. Options default to 4 waiting slots; review/check commands choose 2.
Busy callers are rejected, admitted calls drain on disposal, and cancellation checks
do not promise interruption of synchronous native inference. Lifecycle checks observe
slot ownership; they cannot precisely timestamp native inference entry.

## Three-intent coverage diagnosis

`--catalog-profile full-baseline` is the default and preserves the original 34
examples. `--catalog-profile three-intent-examples` adds six examples only for
MENU_QUERY, REVENUE_QUERY and MY_ORDERS, retaining every other capability/negative.
The latter is a diagnostic experiment, not the recommended profile: it improved
some accepted counts but caused semantic regressions in the observed SUP data.
Threshold .85/.02, scorer, guards and parser are identical between profiles.

```powershell
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --review-output artifacts/coverage-baseline-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --calibration calibration-supplement.json --review-output artifacts/coverage-baseline-sup.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile three-intent-examples --review-output artifacts/coverage-experiment-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile three-intent-examples --calibration calibration-supplement.json --review-output artifacts/coverage-experiment-sup.json
```

Both CAL and SUP have now informed development. Keep the earlier single-run SUP
snapshot as historical evidence; reruns after these additions are development
results and do not establish performance on new questions.

## Fixed-catalog scoring comparison

The latest comparison uses only `full-baseline` (34 retained samples), with two
methods selected by `--scoring max` (default) or `--scoring top2-mean`. The mean
uses the best two distinct sample IDs in the intent; if only one sample exists,
it uses that sample alone. No current competing intent has fewer than two samples.
JSON records `Evidence`, `RankedCandidates`, sample counts, scoring mode and thresholds.

```powershell
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --review-output artifacts/scoring-max-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring top2-mean --review-output artifacts/scoring-top2-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --calibration calibration-supplement.json --review-output artifacts/scoring-max-sup.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring top2-mean --calibration calibration-supplement.json --review-output artifacts/scoring-top2-sup.json
```

Both methods were compared at .85/.02. Mean increased correct accepted counts
but did not consistently improve intent/negative discrimination. Keep max as the
baseline; mean remains a reproducible experiment. No additional thresholds or
algorithms were tried in that comparison. CLI threshold options, when explicitly
provided, use invariant decimal notation; these are not deployment policies.

## Frozen threshold candidate

### Later targeted revision (offline only)

The old v1 manifest and baseline prototype text are preserved as historical evidence.
The current pipeline is v2, so running `full-baseline` now does not reproduce all
v1 decisions. No runtime recognizer has been registered or changed.

`profiles/max-targeted-revision-v2.json` pins a separate 37-sample catalog and
targeted parser/capability changes at max/.85/.01. Select it explicitly with
`--catalog-profile targeted-revision-v2`; CLI defaults are unchanged. Regression
inputs CAL/SUP/EMB keep their original text and labels, now all development data.
`check-targeted-v2.json` holds 15 predeclared internal checks run once after freeze,
not an external independent test or a chat runner. Full before/after and new-check
results are in `MOCK-AI-EMBEDDING-CALIBRATION.md`. The new ready coverage is only
3/6; comma-preface multi-intent and menu/order routing errors block integration.
Do not silently modify v2 or tune it against these results and call that evaluation.

The following threshold-round description is historical (before v2):

The bounded threshold round tested exactly T0 .85/.02, T1 .85/.015, T2 .85/.01
and T3 .85/.005 with max/full-baseline and unchanged parser/guards/CAL/SUP.
The predeclared list is `profiles/threshold-trials-v1.json`.

Selected manifest: `profiles/max-full-baseline-candidate-v1.json`, status
“ứng viên thử nghiệm, chưa triển khai”. It freezes .85/.01 and source/model/catalog
hashes for a later independent component evaluation. The manifest is not loaded
automatically: the CLI default is still .85/.02 and runtime chat is unchanged.

```powershell
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.01 --review-output artifacts/threshold-T2-cal.json
dotnet run --no-build -c Release -- --decision-review --catalog-profile full-baseline --scoring max --minimum-score 0.85 --minimum-margin 0.01 --calibration calibration-supplement.json --review-output artifacts/threshold-T2-sup.json
```

Candidate development results: 14/19 valid requests accepted correctly, 0 false
accepts observed, 5 valid deferred, 22 correct non-accepts and 1 decision mismatch
across 42 cases. T3 had higher coverage but was extremely close to an unresolved
multi-intent boundary; it was not selected. These are development observations.
The calibration report gives the frozen evaluation instructions. The held-out
final EMB set was not opened or run during threshold selection; do not test this
unintegrated component through the chat UI. Any later tuning on evaluation errors
must mark that evaluation set as development data.
