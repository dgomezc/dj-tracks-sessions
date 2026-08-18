using DjTracksSessions.Web.Services;
using Microsoft.AspNetCore.Components;

namespace DjTracksSessions.Web.Pages;

public partial class Configuration
{
    [Inject]
    private DatabaseConnectionDiagnosticClient DatabaseConnectionDiagnosticClient { get; set; } = default!;

    private DatabaseConnectionState _state = DatabaseConnectionState.Idle;

    private string StatusLabel => _state switch
    {
        DatabaseConnectionState.Checking => "Comprobando",
        DatabaseConnectionState.Connected => "Conectada",
        DatabaseConnectionState.Unavailable => "No disponible",
        _ => "Sin comprobar"
    };

    private string StatusDescription => _state switch
    {
        DatabaseConnectionState.Checking => "La aplicación está comprobando la disponibilidad de la base de datos.",
        DatabaseConnectionState.Connected => "La aplicación puede conectarse a la base de datos configurada.",
        DatabaseConnectionState.Unavailable => "La aplicación no puede conectarse ahora. Revisa la configuración segura fuera de la interfaz.",
        _ => "Inicia una comprobación para conocer la disponibilidad actual."
    };

    private string StatusClass => _state switch
    {
        DatabaseConnectionState.Checking => "connection-status-checking",
        DatabaseConnectionState.Connected => "connection-status-connected",
        DatabaseConnectionState.Unavailable => "connection-status-unavailable",
        _ => "connection-status-idle"
    };

    private async Task CheckConnectionAsync()
    {
        _state = DatabaseConnectionState.Checking;

        try
        {
            _state = await DatabaseConnectionDiagnosticClient.CheckAsync(CancellationToken.None);
        }
        catch (HttpRequestException)
        {
            _state = DatabaseConnectionState.Unavailable;
        }
    }
}
