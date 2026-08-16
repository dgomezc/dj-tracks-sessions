---
name: execute-plan-work-unit
description: "Trigger: implement phase, work unit, PLAN.md. Execute exactly one plan work unit, verify it, and stop for manual approval."
license: Apache-2.0
metadata:
  author: dj-tracks-sessions
  version: "1.0"
---

# Execute Plan Work Unit

## Activation Contract

Use when implementing or correcting one numbered work unit from `PLAN.md`.

## Hard Rules

- Read `AGENTS.md`, `DECISIONS.md`, and the complete target phase before editing.
- Implement exactly one named work unit. Do not begin adjacent or future units.
- Preserve user changes and never test against the production music mount.
- Include behavior, tests, and necessary documentation in the same work unit.
- Stop after verification and wait for explicit manual approval.

## Decision Gates

| Situation | Action |
|---|---|
| Work unit is ambiguous | Ask one focused question and stop |
| Dependency belongs to an earlier unit | Complete only the missing prerequisite or report the blocker |
| Work exceeds one reviewable unit | Split internally and request approval before expanding scope |
| Verification fails | Fix within scope; do not mark complete |

## Execution Steps

1. State the phase, work unit, acceptance evidence, and files likely affected.
2. Inspect current code and relevant tests.
3. Implement the smallest complete vertical behavior.
4. Run focused tests, formatting/build checks, and a runtime scenario when a boundary exists.
5. Record exact commands/results and the rollback boundary.
6. Update plan status only if the repository introduces an explicit status mechanism.
7. Return the manual validation steps and stop.

## Output Contract

Return scope completed, files changed, automated evidence, runtime evidence, rollback boundary, and concise manual validation steps. Never claim phase approval.

## References

- `../../../PLAN.md`
- `../../../AGENTS.md`
- `../../../DECISIONS.md`
