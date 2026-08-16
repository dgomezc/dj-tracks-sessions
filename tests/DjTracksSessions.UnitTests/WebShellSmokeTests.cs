using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DjTracksSessions.UnitTests;

public sealed class WebShellSmokeTests
{
    [Fact]
    public async Task Root_page_exposes_the_Blazor_Blueprint_shell_and_Spanish_navigation()
    {
        using var factory = new WebShellFactory();
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("DJ Tracks &amp; Sessions", html);
        Assert.Contains("Explorer / Player", html);
        Assert.Contains("Analyzer / Tagger", html);
        Assert.Contains("Sesiones", html);
        Assert.Contains("Explorador", html);
        Assert.Contains("Reproductor", html);
        Assert.Contains("Analizador", html);
        Assert.Contains("Etiquetador", html);
        Assert.Contains("Biblioteca de sesiones", html);
        Assert.Contains("href=\"/#explorador\"", html);
        Assert.Contains("href=\"/#sesiones\"", html);
        Assert.Contains("sidebar-menu-button", html);
        Assert.Contains("Cambiar tema", html);
        Assert.Contains("blazorblueprint.css", html);
    }

    private sealed class WebShellFactory : WebApplicationFactory<global::Program>
    {
    }
}
