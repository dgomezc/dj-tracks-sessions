using DjTracksSessions.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DjTracksSessions.Web.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] private PlaybackService Playback { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private ElementReference audio;
    private DotNetObjectReference<MainLayout>? callback;
    protected override void OnInitialized() => Playback.AudioCommandRequested += HandleAudioCommand;
    protected override async Task OnAfterRenderAsync(bool firstRender) { if (!firstRender) return; callback = DotNetObjectReference.Create(this); await JS.InvokeVoidAsync("djTracksSessionsAudio.init", audio, callback); await JS.InvokeVoidAsync("djTracksSessionsAudio.setVolume", audio, Playback.Volume); }
    private async void HandleAudioCommand(AudioCommand command)
    {
        try
        {
            await JS.InvokeVoidAsync("djTracksSessionsAudio.command", audio, command.Kind.ToString(), command.StreamUrl, command.Value);
        }
        catch (JSDisconnectedException) { }
        catch (Exception) { Playback.NotifyError("No se pudo reproducir el audio."); }
    }
    [JSInvokable] public void AudioLoaded(double duration) => Playback.NotifyLoaded(duration);
    [JSInvokable] public void AudioTime(double position, double duration) => Playback.NotifyTime(position, duration);
    [JSInvokable] public void AudioPaused() => Playback.NotifyPaused();
    [JSInvokable] public void AudioEnded() => Playback.NotifyEnded();
    [JSInvokable] public void AudioUnavailable(string message) => Playback.NotifyUnavailable(message);
    [JSInvokable] public void AudioError(string message) => Playback.NotifyError(message);
    public void Dispose() { Playback.AudioCommandRequested -= HandleAudioCommand; callback?.Dispose(); }
}
