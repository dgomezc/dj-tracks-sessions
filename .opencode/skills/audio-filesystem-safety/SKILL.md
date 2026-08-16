---
name: audio-filesystem-safety
description: "Trigger: audio tags, rename, move, delete, artwork, filesystem. Protect original media with confined paths, atomic writes, and fixtures."
license: Apache-2.0
metadata:
  author: dj-tracks-sessions
  version: "1.0"
---

# Audio Filesystem Safety

## Activation Contract

Use for scanning, hashing, tagging, artwork, renaming, moving, deleting, or reconciling audio/session files.

## Hard Rules

- Never run tests or experiments against a real configured music root.
- Canonicalize and prove every path remains under its allowed root before access.
- Use disposable copied fixtures; never mutate the canonical test fixture.
- Preserve unknown tags unless an approved format rule removes them.
- Write to a temporary sibling, reopen and verify, then replace atomically where supported.
- Detect collisions before mutation; never invent suffixes.
- Never delete automatically. Require the product confirmation flow.
- Report batch success/failure per file and make partial completion visible.

## Decision Gates

| Operation | Required evidence |
|---|---|
| Read/index | Original hash unchanged |
| Tag/artwork write | Reopen, verify expected fields, verify retained fields |
| Rename/move | Source/destination confinement and collision test |
| Delete | Explicit confirmation contract and audit result |
| NAS watcher | Periodic reconciliation proves eventual consistency |

## Execution Steps

1. Identify root policy and allowed capability.
2. Build an isolated temporary filesystem scenario.
3. Capture pre-operation hash, tags, artwork, path, and technical properties.
4. Execute through the production adapter.
5. Reopen and compare expected and preserved state.
6. Exercise interruption/error behavior where mutation spans steps.
7. State rollback behavior and remaining filesystem limitations.

## Output Contract

Return fixture paths, safety checks, before/after evidence, failure-path result, and confirmation that no production mount was accessed.

## References

- `../../../AGENTS.md`
- `../../../PLAN.md`
- `../../../DECISIONS.md`
