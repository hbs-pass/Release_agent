using IFA.Simulator.Core.Pipeline;

namespace IFA.Simulator.Worker;

/// <summary>
/// Background service que ejecuta el pipeline IFA como servicio del sistema.
/// Puede instalarse como Windows Service o systemd unit.
/// </summary>
public sealed class IfaWorker : BackgroundService
{
    private readonly IfaPipeline         _pipeline;
    private readonly ILogger<IfaWorker>  _log;

    public IfaWorker(IfaPipeline pipeline, ILogger<IfaWorker> log)
    {
        _pipeline = pipeline;
        _log      = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("IFA Worker activo. PID: {Pid}", Environment.ProcessId);

        try
        {
            await _pipeline.RunAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _log.LogInformation("IFA Worker detenido por señal de cancelación.");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error fatal en IFA Worker");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("IFA Worker apagando...");
        await base.StopAsync(cancellationToken);
        _log.LogInformation("IFA Worker detenido.");
    }
}
