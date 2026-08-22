namespace DjTracksSessions.Contracts;

public sealed record CatalogScanResponse(
    int OrdinarySucceeded,
    int SessionsSucceeded,
    IReadOnlyList<CatalogScanFileFailure> Failures,
    int MissingMarked);

public sealed record CatalogScanFileFailure(string Root, string RelativePath, string ErrorCode, string ErrorMessage);
