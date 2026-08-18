using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DjTracksSessions.Web.Components;

public partial class ThemeToggle
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private bool IsDark;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        IsDark = await JS.InvokeAsync<bool>("djTracksSessionsTheme.isDark");
        StateHasChanged();
    }

    private async Task ToggleTheme()
    {
        IsDark = !IsDark;
        await JS.InvokeVoidAsync("djTracksSessionsTheme.setDark", IsDark);
    }
}
