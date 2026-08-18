# Phase 0 Work Unit 1: Audio Capability Spike

Status: focused disposable round-trip test proven locally; Phase 0 exit criteria remain open because this work unit does not prove all unknown-tag cases or target container tooling.

## Scope

This spike selects TagLibSharp2 0.6.0 for format-specific metadata read/write experiments. It covers only representative read/write round trips for MP3, FLAC, M4A, AIFF, and WAV. It does not implement PersonalGenre mappings, artwork normalization, audio analysis, provider clients, or application mutation workflows.

## Capability Matrix

| Format | Read fixture | Write metadata | Reopen verification | Technical evidence | Unknown-tag evidence | Current result |
|---|---|---|---|---|---|---|
| MP3 | Minimal ID3v2 plus audio placeholder | ID3v2 title/artist | Proven | MPEG properties when present | Custom ID3v2 user-text frame proven | Proven for spike fixture |
| FLAC | STREAMINFO plus Vorbis Comment | Vorbis Comment title/artist | Proven | FLAC STREAMINFO | Non-standard Vorbis Comment field proven | Proven for spike fixture |
| M4A | MP4/M4A container with AAC-shaped track | MP4 metadata atoms | Proven | MP4 audio properties | Unknown `ilst` atom not proven by this test | Degraded: common fields only |
| AIFF | FORM/AIFF COMM and SSND chunks | ID3v2 tag chunk | Proven | AIFF COMM properties | Unknown chunk not proven by this test | Degraded: common fields only |
| WAV | RIFF/WAVE fmt and data chunks | RIFF INFO or ID3v2 | Proven | WAV fmt properties | Unknown RIFF chunk not proven by this test | Degraded: common fields only |

The test must record, for each fixture, the source path, destination path, source SHA-256, destination SHA-256, byte lengths, technical properties, pre-write tags, post-write tags, and any unsupported/degraded fields. A changed destination hash is expected; the source hash must remain unchanged.

## Safety And Rollback Boundary

- Fixture bytes are generated in the test and copied into an isolated `Path.GetTempPath()` directory.
- The canonical fixture bytes are never opened for mutation.
- The output is a separate sibling file; no configured music root is read.
- The source file is not replaced by this spike. Rollback is deleting the disposable temporary directory after the test.
- Production atomic replacement, collision handling, approval, history, and user-facing mutation are later work and are intentionally not implemented here.

## Environment Limitation

The current environment does not provide `ffmpeg`, `ffprobe`, or Docker. Those tools are not needed for the format metadata spike, but no container architecture proof is claimed by this document. A valid audio-encoded fixture set and `linux/amd64` tool-image verification remain outside this work unit.

## Verification Command

```bash
dotnet test tests/DjTracksSessions.IntegrationTests/DjTracksSessions.IntegrationTests.csproj --filter FullyQualifiedName~AudioMetadataRoundTripTests
```
