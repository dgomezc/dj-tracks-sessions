using System.Net;
using System.Net.Http.Json;

namespace DjTracksSessions.Web.Services;

public sealed class DatabaseConnectionDiagnosticClient(HttpClient httpClient)
{
    public async Task<DatabaseConnectionState> CheckAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("configuration/database-connection", cancellationToken);

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            return DatabaseConnectionState.Unavailable;
        }

        if (!response.IsSuccessStatusCode)
        {
            return DatabaseConnectionState.Unavailable;
        }

        var diagnostic = await response.Content.ReadFromJsonAsync<DatabaseConnectionDiagnosticResponse>(cancellationToken);
        return diagnostic?.State == "connected"
            ? DatabaseConnectionState.Connected
            : DatabaseConnectionState.Unavailable;
    }

    private sealed record DatabaseConnectionDiagnosticResponse(string State);
}

public enum DatabaseConnectionState
{
    Idle,
    Checking,
    Connected,
    Unavailable
}
