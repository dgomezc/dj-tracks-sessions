namespace DjTracksSessions.Contracts;

public static class ApiErrorCodes
{
    public const string ExampleNotFound = "example.not_found";
    public const string ValidationFailed = "validation.failed";
    public const string ScanFailed = "scan.failed";
}

public sealed record ApiContractMarker;

public sealed record ValidationExampleRequest(string Name);

public sealed record ValidationExampleResponse(string Name);
