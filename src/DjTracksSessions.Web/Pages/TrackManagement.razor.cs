using DjTracksSessions.Contracts;
using DjTracksSessions.Web.Services;
using Microsoft.AspNetCore.Components;

namespace DjTracksSessions.Web.Pages;

public partial class TrackManagement
{
    [Inject] private FilesystemLibraryClient LibraryClient { get; set; } = default!;
    [Inject] private PlaybackService Playback { get; set; } = default!;
    private static readonly LibraryRoot[] Roots = [LibraryRoot.Main, LibraryRoot.Pending, LibraryRoot.Remember];
    private LibraryRoot SelectedRoot = LibraryRoot.Main;
    private string SelectedPath = string.Empty;
    private FilesystemBrowseResponse? Browse;
    private FilesystemAudioItem? SelectedAudio;
    private string? ErrorMessage;
    private bool IsBusy;
    private string SearchTerm = string.Empty;
    private string GenreFilter = string.Empty;
    private string YearFilter = string.Empty;
    private string StateFilter = string.Empty;
    private readonly Dictionary<string, FolderNode> FolderNodes = new(StringComparer.Ordinal);
    private string StatusLabel => ErrorMessage is not null ? "No disponible" : IsBusy ? "Cargando" : $"{Browse?.AudioFiles.Count ?? 0} archivos";
    private IEnumerable<FilesystemAudioItem> FilteredTracks => (Browse?.AudioFiles ?? []).Where(MatchesFilters);
    private IEnumerable<string> AvailableGenres => (Browse?.AudioFiles ?? []).SelectMany(track => track.Genres).Where(genre => !string.IsNullOrWhiteSpace(genre)).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase);
    private IEnumerable<uint> AvailableYears => (Browse?.AudioFiles ?? []).Where(track => track.Year.HasValue).Select(track => track.Year!.Value).Distinct().OrderDescending();
    protected override Task OnInitializedAsync() => LoadAsync();
    private static string RootLabel(LibraryRoot root) => root.ToString();
    private async Task SelectRootAsync(LibraryRoot root)
    {
        SelectedRoot = root;
        SelectedPath = string.Empty;
        FolderNodes.Clear();
        await LoadDirectoryAsync(string.Empty, select: true);
    }

    private async Task ToggleDirectoryAsync(string path)
    {
        if (!FolderNodes.TryGetValue(path, out var node)) return;
        if (!node.IsLoaded)
        {
            await LoadDirectoryAsync(path, select: false);
        }

        node.IsExpanded = !node.IsExpanded;
    }

    private Task SelectDirectoryAsync(string path) => LoadDirectoryAsync(path, select: true);
    private void SelectAudio(FilesystemAudioItem audio) => SelectedAudio = audio;
    private void Play(FilesystemAudioItem audio) => Playback.Add(SelectedRoot, audio, true);
    private void Queue(FilesystemAudioItem audio) => Playback.Add(SelectedRoot, audio);

    private bool MatchesFilters(FilesystemAudioItem track)
    {
        var search = SearchTerm.Trim();
        var matchesSearch = string.IsNullOrEmpty(search) || string.Join(" ", track.FileName, track.Title, string.Join(" ", track.Artists), track.Album, string.Join(" ", track.Genres), track.BeatsPerMinute, track.InitialKey).Contains(search, StringComparison.OrdinalIgnoreCase);
        var matchesGenre = string.IsNullOrEmpty(GenreFilter) || track.Genres.Contains(GenreFilter, StringComparer.OrdinalIgnoreCase);
        var matchesYear = string.IsNullOrEmpty(YearFilter) || track.Year?.ToString() == YearFilter;
        var matchesState = string.IsNullOrEmpty(StateFilter) || (StateFilter == "issues" ? track.ErrorCode is not null : track.ErrorCode is null);
        return matchesSearch && matchesGenre && matchesYear && matchesState;
    }

    private void ClearFilters()
    {
        SearchTerm = string.Empty;
        GenreFilter = string.Empty;
        YearFilter = string.Empty;
        StateFilter = string.Empty;
    }

    private static string FormatDuration(TimeSpan? duration) => duration is null ? "--:--" : $"{(int)duration.Value.TotalMinutes:00}:{duration.Value.Seconds:00}";

    private async Task LoadAsync() => await LoadDirectoryAsync(SelectedPath, select: true);

    private async Task LoadDirectoryAsync(string path, bool select)
    {
        IsBusy = true;
        ErrorMessage = null;
        if (select) SelectedAudio = null;
        try
        {
            var response = await LibraryClient.BrowseAsync(SelectedRoot, path, CancellationToken.None);
            if (response is null) throw new InvalidOperationException("La API no devolvió la carpeta solicitada.");
            FolderNodes[path] = new FolderNode(response);
            if (select)
            {
                SelectedPath = path;
                Browse = response;
            }
        }
        catch (Exception exception) { Browse = null; ErrorMessage = exception.Message; }
        finally { IsBusy = false; }
    }

    private IEnumerable<FolderRow> VisibleFolders
    {
        get
        {
            if (!FolderNodes.TryGetValue(string.Empty, out var root)) yield break;
            foreach (var directory in root.Response.Directories)
            {
                foreach (var row in Descendants(directory, 0)) yield return row;
            }
        }
    }

    private IEnumerable<FolderRow> Descendants(FilesystemDirectoryItem directory, int depth)
    {
        FolderNodes.TryGetValue(directory.RelativePath, out var node);
        yield return new FolderRow(directory, depth, node?.IsExpanded == true, node?.IsLoaded == true);
        if (node?.IsExpanded != true || node.Response is null) yield break;
        foreach (var child in node.Response.Directories)
        {
            foreach (var row in Descendants(child, depth + 1)) yield return row;
        }
    }

    private sealed class FolderNode(FilesystemBrowseResponse response)
    {
        public FilesystemBrowseResponse Response { get; } = response;
        public bool IsLoaded => Response is not null;
        public bool IsExpanded { get; set; }
    }

    private sealed record FolderRow(FilesystemDirectoryItem Directory, int Depth, bool IsExpanded, bool IsLoaded);
}
