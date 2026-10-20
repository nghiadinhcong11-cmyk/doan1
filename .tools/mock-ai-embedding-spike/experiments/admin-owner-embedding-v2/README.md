# Admin owner embedding v2

Version 2 is a new offline experiment. Version 1 remains historical and is not reused as an independent holdout.

Split policy:

- `reference.json`: frozen semantic examples.
- `development.json`: catalog/data design checks.
- `calibration.json`: the only split used for score/margin selection.
- `holdout.json`: frozen before evaluator tuning and used once for final v2 reporting.

The evaluator does not call a tool, API, database, or runtime AI endpoint.
