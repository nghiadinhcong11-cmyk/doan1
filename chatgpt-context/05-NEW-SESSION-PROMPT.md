# New Session Prompts

Replace the final placeholder with the task for the new session.

## Codex with the repository open

```text
You are working in the repository D:\Dev\doan. First read AGENTS.md and the files in chatgpt-context/, starting with 00-START-HERE.md. Then inspect the current Git status and the source paths relevant to the task below, including changes since the context reference commit. Treat current source as authoritative over old reports, distinguish static inspection from runtime evidence, preserve existing user changes, and do not print secrets. Do not claim PASS or verified behavior without matching evidence. If the task affects money, authorization, branch isolation, schema, deletion or order state, stop and ask only for information that materially changes the decision; continue safe read-only work.

Task for this session:
<DÁN NHIỆM VỤ Ở ĐÂY>
```

## ChatGPT with attached files only

```text
Use the attached ChatGPT context pack and any other supplied files as the available evidence. You do not have repository access, so do not claim to have read the repository or verified current source beyond the attachments. Distinguish user-provided facts, source-backed facts, runtime evidence and unknowns. If the task depends on a missing source excerpt, configuration, schema or test result, identify the exact missing part and ask for it; do not invent architecture, functionality, secrets or requirements. Keep recommendations within the existing project scope and label optional expansions with their cost.

Task for this session:
<DÁN NHIỆM VỤ Ở ĐÂY>
```
