var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { application = "DjTracksSessions.Web" }));

app.Run();

public partial class Program;
