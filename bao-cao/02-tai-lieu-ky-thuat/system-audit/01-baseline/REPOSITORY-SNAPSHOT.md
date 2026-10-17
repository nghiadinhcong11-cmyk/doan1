# Repository Snapshot

## Snapshot metadata

| Field | Value |
|---|---|
| Repository path | `D:\Dev\doan` |
| Branch | `main` |
| HEAD commit | `0a480965f5a582c6da5c30fb70d9a5a2d1cd3d4e` |
| HEAD short | `0a480965` |
| HEAD subject | `chore: remove obsolete Docker Compose configuration` |
| HEAD time | `2026-10-02T17:23:17+07:00` |
| Survey time | `2026-10-15T11:02:37+07:00` |
| Survey operation | Read-only repository inspection; no build, database, migration or runtime test executed |

The timestamp is the survey-session time, not a commit timestamp and not a claim that the working tree represents a reproducible release snapshot.

## Working tree status at survey time

`git status --porcelain=v1 -uall` reported:

| Status category | Count | Interpretation |
|---|---:|---|
| Modified tracked entries | 137 | Existing edits, including frontend, backend, configuration and documentation-related files |
| Deleted tracked entries | 149 | Existing deletions, including old documentation and previously present test files |
| Untracked entries | 19,474 | Includes source additions, audit/report material, generated/build/dependency artefacts and other local files |
| Total status entries | 19,760 | Not a source-file count and not a release manifest |

The tree is therefore **not clean**. These changes were pre-existing and were not reverted. The counts include untracked dependency/build output; a later audit should create a clean or explicitly frozen snapshot before treating a result as reproducible.

## Relevant changed areas observed

The status includes changes or additions in these areas:

- `apps/admin-web/` and `apps/customer-web/`.
- `services/api/Program.cs`, project/configuration files, AI, Application, Domain, Infrastructure and WebAPI.
- `services/api/src/Infrastructure/Persistence/Migrations/`.
- `packages/shared/` and root npm metadata.
- `scripts/` and diagnostic tools.
- `ba-test-frontend/`, `bao-cao/`, `evidence/` and `test-results/`.
- `.env.example`, `README.md`, `.gitignore` and development certificate-related files.
- Deletions under older `docs/`, `baocaodoan/` and the former `tests/RestaurantPOS.Tests/` paths.

This is a high-level inventory, not a claim that every listed path belongs to one logical change. Use the exact Git status for future comparison.

## Scope and reproducibility limit

The commit identifies the last committed base only. It does not identify the combined state of the current working tree. Until the relevant changes are committed, stashed by the owner, or exported as a review snapshot, another person cannot reproduce this survey from the commit alone.

No secret, token, password, full connection string or certificate private material is recorded here.
