using System.Net.Http.Json;
using DjTracksSessions.Contracts;

namespace DjTracksSessions.Web.Services;

public sealed class SessionsClient(HttpClient httpClient)
{
    public Task<SessionBrowseResponse?> BrowseAsync(string? path, CancellationToken cancellationToken)
    {
        var query = string.IsNullOrWhiteSpace(path) ? string.Empty : $"?path={Uri.EscapeDataString(path)}";
        return httpClient.GetFromJsonAsync<SessionBrowseResponse>($"sessions/browse{query}", cancellationToken);
    }

    public Task<SessionDetail?> DetailAsync(string path, CancellationToken cancellationToken) =>
        httpClient.GetFromJsonAsync<SessionDetail>($"sessions/detail?path={Uri.EscapeDataString(path)}", cancellationToken);

    public async Task<SessionEditPreview?> PreviewAsync(SessionEditRequest request, CancellationToken cancellationToken) =>
        await PreviewResponseAsync(request, cancellationToken);

    private async Task<SessionEditPreview?> PreviewResponseAsync(SessionEditRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("sessions/edit/preview", request, cancellationToken);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SessionEditPreview>(cancellationToken) : null;
    }

    public async Task<SessionEditResponse?> ApplyAsync(SessionEditRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("sessions/edit", request, cancellationToken);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SessionEditResponse>(cancellationToken) : null;
    }
}
