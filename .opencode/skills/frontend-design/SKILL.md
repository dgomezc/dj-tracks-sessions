---
name: frontend-design
description: "Trigger: frontend design, UI refresh, Blazor styling, dashboard layout. Design or refactor the project's desktop-first DJ interface."
license: Apache-2.0
metadata:
  author: dj-tracks-sessions
  version: "1.0"
---

## Activation Contract

Load when designing or visually refactoring Blazor pages, layouts, themes, or UI states in this project.

## Hard Rules

- Design desktop-first, operational DJ software: dense, composed, and calm rather than consumer-app decoration.
- Write all user-facing text in Spanish; keep code, identifiers, comments, and technical artifacts in English.
- Start with installed Blazor Blueprint primitives. Query the Blueprint MCP before adding or changing a component API; if unavailable, use native semantic HTML and CSS only.
- Keep Explorer / Player visually and structurally separate from Analyzer / Tagger. Keep Sessions independent.
- Provide accessible contrast, visible keyboard focus, and clear default, hover, active, empty, and unavailable states.
- Do not imply unimplemented catalog, playback, analysis, tagging, or session behavior is functional.

## Decision Gates

| Situation | Action |
| --- | --- |
| Existing Blueprint primitive is known | Preserve it and style around it. |
| Component API is unknown | Do not guess; use semantic HTML/CSS. |
| Screen represents future behavior | Use descriptive static status, never interactive-looking controls. |

## Execution Steps

1. Read `AGENTS.md`, `PLAN.md`, and the existing page, layout, and theme files.
2. Preserve navigation anchors, theme behavior, and product boundaries.
3. Define hierarchy, spacing, palette tokens, surfaces, and states before editing markup.
4. Use semantic landmarks and heading order; verify light and dark CSS values together.
5. Extend smoke tests for durable Spanish labels and navigation anchors.

## Output Contract

Return files changed, preserved behaviors, visual states covered, verification, rollback boundary, and blockers.

## References

- `../../../AGENTS.md`
- `../../../PLAN.md`
- `../../../DECISIONS.md`
