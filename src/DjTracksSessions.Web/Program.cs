using BlazorBlueprint.Components;
using DjTracksSessions.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddBlazorBlueprintComponents();

var app = builder.Build();

app.MapStaticAssets();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
