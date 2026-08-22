using BlazorBlueprint.Components;
using DjTracksSessions.Web;
using DjTracksSessions.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBlueprintComponents();
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl must be configured.");
builder.Services.AddHttpClient<DatabaseConnectionDiagnosticClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute));
builder.Services.AddHttpClient<FilesystemLibraryClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute));

var app = builder.Build();

app.MapStaticAssets();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
