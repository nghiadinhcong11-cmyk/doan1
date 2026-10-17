# Manager two-intent experiment v1

This experiment is frozen before execution. `reference.json` is used only to build the two intent prototypes. `development.json` is used only to select score/margin thresholds. `test.json` is held out until reference, scorer and threshold are fixed. No test text is copied into reference or development.

Role: `manager`. Supported read-only mappings are `REVENUE_QUERY -> get_revenue` and `BUSINESS_SUMMARY -> get_business_summary`. The experiment scores semantic intent only; backend parameter extraction, permission and service execution are not invoked.
