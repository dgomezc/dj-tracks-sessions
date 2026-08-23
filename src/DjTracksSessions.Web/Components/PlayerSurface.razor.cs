using DjTracksSessions.Contracts;
using DjTracksSessions.Web.Services;
using Microsoft.AspNetCore.Components;

namespace DjTracksSessions.Web.Components;

public partial class PlayerSurface : IDisposable
{
    [Inject] protected PlaybackService Service { get; set; } = default!;
    private bool QueueOpen;
    private string StatusLabel => Service.Status switch { PlaybackStatus.Empty => "Sin pista", PlaybackStatus.Loading => "Cargando", PlaybackStatus.Playing => "Reproduciendo", PlaybackStatus.Paused => "En pausa", PlaybackStatus.Ended => "Finalizada", PlaybackStatus.Unavailable => "No disponible", PlaybackStatus.Error => "Error", _ => "" };
    private bool CanPrevious => Service.Queue.CurrentIndex > 0;
    protected override void OnInitialized() => Service.Changed += Refresh;
    private void Refresh()
    {
        if (Service.Queue.Current is null) QueueOpen = false;
        InvokeAsync(StateHasChanged);
    }
    private void Toggle() => Service.TogglePlayPause();
    private void Previous() => Service.Previous();
    private void Next() => Service.Next();
    private void Clear() => Service.Clear();
    private void Remove(TrackIdentity identity) => Service.Remove(identity);
    private void VolumeChanged(ChangeEventArgs args) => Service.SetVolume(double.TryParse(args.Value?.ToString(), out var value) ? value : Service.Volume);
    private void SeekChanged(ChangeEventArgs args) => Service.Seek(double.TryParse(args.Value?.ToString(), out var value) ? value : Service.Position);
    public void Dispose() => Service.Changed -= Refresh;
}
