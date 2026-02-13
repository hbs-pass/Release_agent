using IFA.Simulator.Core.Models;
using IFA.Simulator.Core.Pipeline;

namespace IFA.Simulator.Web.Services;

/// <summary>
/// Servicio singleton que expone el estado del pipeline IFA
/// a los componentes Blazor vía eventos reactivos.
/// </summary>
public sealed class DashboardService
{
    private readonly IfaPipeline        _pipeline;
    private readonly ILogger<DashboardService> _log;
    private CancellationTokenSource?    _cts;

    public SimulatorState State => _pipeline.State;

    // Evento que Blazor escucha para actualizar el UI en tiempo real
    public event Action? OnStateChanged;

    public DashboardService(IfaPipeline pipeline, ILogger<DashboardService> log)
    {
        _pipeline = pipeline;
        _log      = log;

        // Suscribir al pipeline para propagar cambios al UI
        _pipeline.StateChanged += () => OnStateChanged?.Invoke();
    }

    public async Task StartAsync()
    {
        if (State.Status == SimulatorStatus.Running) return;

        _log.LogInformation("Dashboard: iniciando pipeline IFA");
        _cts = new CancellationTokenSource();

        try
        {
            await _pipeline.RunAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            _log.LogInformation("Dashboard: pipeline detenido por usuario");
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        _log.LogInformation("Dashboard: señal de parada enviada");
    }

    public async Task RestartAsync()
    {
        Stop();
        await Task.Delay(500);
        await StartAsync();
    }
}
