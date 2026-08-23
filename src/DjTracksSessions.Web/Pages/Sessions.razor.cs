using DjTracksSessions.Contracts;
using Microsoft.AspNetCore.Components;
using DjTracksSessions.Web.Services;

namespace DjTracksSessions.Web.Pages;

public partial class Sessions
{
    [Inject] private SessionsClient Client { get; set; } = default!;
    [Inject] private PlaybackService Playback { get; set; } = default!;
    private string SelectedPath = string.Empty;
    private SessionBrowseResponse? Browse;
    private SessionAudioItem? SelectedAudio;
    private SessionDetail? SelectedDetail;
    private SessionEditPreview? EditPreview;
    private string? EditError;
    private string? EditTitle;
    private string? EditArtists;
    private string? EditAlbum;
    private uint? EditYear;
    private string? EditGenre;
    private string? ErrorMessage;
    private bool IsBusy;
    private readonly Dictionary<string, FolderNode> FolderNodes = new(StringComparer.Ordinal);
    private string StatusLabel => ErrorMessage is not null ? "No disponible" : IsBusy ? "Cargando" : $"{Browse?.AudioFiles.Count ?? 0} archivos";

    protected override Task OnInitializedAsync() => LoadDirectoryAsync(string.Empty, true);

    private async Task SelectDirectoryAsync(string path) => await LoadDirectoryAsync(path, true);

    private async Task ToggleDirectoryAsync(string path)
    {
        if (!FolderNodes.TryGetValue(path, out var node)) return;
        if (!node.IsLoaded) await LoadDirectoryAsync(path, false);
        node.IsExpanded = !node.IsExpanded;
    }

    private async Task LoadDirectoryAsync(string path, bool select)
    {
        IsBusy = true; ErrorMessage = null;
        try
        {
            var response = await Client.BrowseAsync(path, CancellationToken.None) ?? throw new InvalidOperationException("La API no devolvió la carpeta solicitada.");
            FolderNodes[path] = new(response);
            if (select) { SelectedPath = path; Browse = response; SelectedAudio = null; SelectedDetail = null; EditPreview = null; }
        }
        catch (Exception exception) { ErrorMessage = exception.Message; Browse = null; }
        finally { IsBusy = false; }
    }

    private async Task SelectAudioAsync(SessionAudioItem audio)
    {
        SelectedAudio = audio; SelectedDetail = null; EditPreview = null; EditError = null;
        try
        {
            SelectedDetail = await Client.DetailAsync(audio.RelativePath, CancellationToken.None);
            if (SelectedDetail is null) throw new InvalidOperationException("La API no devolvió el detalle solicitado.");
            EditTitle = SelectedDetail.Title; EditArtists = string.Join(", ", SelectedDetail.Artists); EditAlbum = SelectedDetail.Album; EditYear = SelectedDetail.Year; EditGenre = string.Join(", ", SelectedDetail.Genres);
        }
        catch (Exception exception) { EditError = exception.Message; }
    }

    private void PlayAudio(SessionAudioItem audio) => Playback.Add(audio, play: true);

    private async Task PreviewEditAsync()
    {
        if (SelectedDetail is null) return;
        EditError = null; EditPreview = await Client.PreviewAsync(new(new(SelectedDetail.Identity.RelativePath), new(EditTitle, EditArtists, EditAlbum, EditYear, EditGenre)), CancellationToken.None);
        if (EditPreview is null) EditError = "No se pudo preparar la previsualización.";
    }

    private async Task ApplyEditAsync()
    {
        if (SelectedDetail is null || EditPreview is null) return;
        var response = await Client.ApplyAsync(new(new(SelectedDetail.Identity.RelativePath), new(EditTitle, EditArtists, EditAlbum, EditYear, EditGenre)), CancellationToken.None);
        if (response is null) { EditError = "No se pudieron guardar los cambios."; return; }
        SelectedDetail = response.Session; EditPreview = null; EditError = null;
    }

    private IEnumerable<FolderRow> VisibleFolders
    {
        get { if (!FolderNodes.TryGetValue(string.Empty, out var root)) yield break; foreach (var directory in root.Response.Directories) foreach (var row in Descendants(directory, 0)) yield return row; }
    }

    private IEnumerable<FolderRow> Descendants(SessionDirectoryItem directory, int depth)
    {
        FolderNodes.TryGetValue(directory.RelativePath, out var node);
        yield return new(directory, depth, node?.IsExpanded == true);
        if (node?.IsExpanded != true) yield break;
        foreach (var child in node.Response.Directories) foreach (var row in Descendants(child, depth + 1)) yield return row;
    }

    private static string FormatDuration(TimeSpan? duration) => duration is null ? "—" : $"{(int)duration.Value.TotalMinutes:00}:{duration.Value.Seconds:00}";
    private sealed class FolderNode(SessionBrowseResponse response) { public SessionBrowseResponse Response { get; } = response; public bool IsLoaded => true; public bool IsExpanded { get; set; } }
    private sealed record FolderRow(SessionDirectoryItem Directory, int Depth, bool IsExpanded);
}
