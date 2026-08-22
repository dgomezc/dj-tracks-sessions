using System.Net.Http.Json;
using DjTracksSessions.Contracts;

namespace DjTracksSessions.Web.Services;

public sealed class FilesystemLibraryClient(HttpClient httpClient)
{
    public Task<FilesystemBrowseResponse?> BrowseAsync(LibraryRoot root, string? path, CancellationToken cancellationToken)
    {
        var query = $"root={root}";
        if (!string.IsNullOrWhiteSpace(path)) query += $"&path={Uri.EscapeDataString(path)}";
        return httpClient.GetFromJsonAsync<FilesystemBrowseResponse>($"library/browse?{query}", cancellationToken);
    }

}
