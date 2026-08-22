using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DjTracksSessions.UnitTests;

public sealed class WebShellSmokeTests
{
    [Fact]
    public async Task Root_page_exposes_the_header_and_current_Spanish_navigation()
    {
        using var factory = new WebShellFactory();
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("DJ Tracks &amp; Sessions", html);
        Assert.Contains("Inicio", html);
        Assert.Contains("Biblioteca", html);
        Assert.Contains("Configuración", html);
        Assert.Contains("href=\"/\"", html);
        Assert.Contains("href=\"/catalogo\"", html);
        Assert.Contains("href=\"/configuracion\"", html);
        Assert.DoesNotContain("BbSidebar", html);
        Assert.DoesNotContain("href=\"/#reproductor\"", html);
        Assert.DoesNotContain("href=\"/#analizador\"", html);
        Assert.DoesNotContain("href=\"/#sesiones\"", html);
        Assert.Contains("Cambiar tema", html);
        Assert.Contains("blazorblueprint.css", html);
    }

    [Fact]
    public async Task Configuration_page_exposes_the_safe_database_connection_check()
    {
        using var factory = new WebShellFactory();
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/configuracion");

        Assert.Contains("Conexión de base de datos", html);
        Assert.DoesNotContain("ConnectionStrings__Postgres", html);
    }

    [Fact]
    public async Task Track_management_page_exposes_the_ordinary_catalog_boundary()
    {
        using var factory = new WebShellFactory();
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/catalogo");

        Assert.Contains("Gestión de pistas", html);
        Assert.Contains("Main", html);
        Assert.Contains("Pending", html);
        Assert.Contains("Remember", html);
        Assert.Contains("Fuera del catálogo ordinario", html);
        Assert.DoesNotContain("Escanear biblioteca", html);
    }

    private sealed class WebShellFactory : WebApplicationFactory<global::Program>
    {
    }
}
