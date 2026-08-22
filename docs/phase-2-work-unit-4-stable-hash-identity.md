# Phase 2 Work Unit 4: Stable Hash Identity

The scan/extraction coordinator calculates a stable content identity for every discovered audio file using SHA-256 over the exact source bytes. The digest is uppercase hexadecimal and is independent of the file path, extension, tags, or metadata extraction result. It is suitable for correlating unchanged content after later path moves or renames; reconciliation and persistence are outside this work unit.

Hashing is incremental: the adapter opens the root-confined file for read-only access and feeds 64 KiB chunks to `SHA256` with `FileOptions.SequentialScan`. It never loads the complete file into memory for identity calculation. Metadata extraction remains the existing bounded adapter and its technical/tag outcome is retained even if hashing fails.

Each extraction outcome reports hash success through `ContentHash`, or a per-file `HashErrorCode` and `HashErrorMessage`. Root escape and missing-file failures remain visible rather than being converted into a batch failure. A hash failure does not mutate or delete the source file.

## Safety And Rollback Boundary

- Tests create disposable temporary roots and fixtures only; no configured music root is accessed.
- The hasher re-resolves every path under the supplied canonical root before opening it.
- Files are opened read-only and source bytes are verified unchanged by tests.
- Rollback is deleting the hasher, outcome fields, coordinator integration, tests, and this document; no production files or database records are changed.

## Focused Verification Command

```bash
dotnet test tests/DjTracksSessions.IntegrationTests/DjTracksSessions.IntegrationTests.csproj --filter 'FullyQualifiedName~ReadOnlyAudioExtractionTests'
```
