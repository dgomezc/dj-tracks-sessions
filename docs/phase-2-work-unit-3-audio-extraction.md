# Phase 2 Work Unit 3: Audio Extraction

The read-only extraction coordinator consumes audio files already discovered by the Phase 2 scanner. It returns one outcome per file, containing separate technical properties and current tags, or a stable error code and message when TagLibSharp2 cannot read that file. A malformed file therefore does not discard successful results for other files.

The production adapter uses TagLibSharp2 0.6.0 and resolves each discovered path again under the supplied canonical root before opening it. It does not hash, write, reconcile, persist, analyze, or modify files. Provider, personal, and effective metadata are intentionally absent from these current-tag read models.

TagLibSharp2's proven capability limitation remains applicable: unknown-tag preservation is proven for MP3 and FLAC; M4A, AIFF, and WAV extraction is limited to the common fields exposed by the library. This work unit does not claim unknown-field enumeration or preservation for those formats.

## Safety And Rollback Boundary

- Tests generate or copy fixtures into an isolated temporary directory.
- Extraction opens files for reading only and verifies source bytes remain unchanged where tested.
- No configured music root is accessed.
- Rollback is deleting the new adapter, read models, tests, and this document; no production files or database records are changed.

## Verification Command

```bash
dotnet test tests/DjTracksSessions.IntegrationTests/DjTracksSessions.IntegrationTests.csproj --filter 'FullyQualifiedName~ReadOnlyAudioExtractionTests'
```
