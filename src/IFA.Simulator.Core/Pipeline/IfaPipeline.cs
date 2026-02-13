using System.Threading.Channels;
using IFA.Simulator.Core.Drivers;
using IFA.Simulator.Core.Engine;
using IFA.Simulator.Core.Models;
using IFA.Simulator.Core.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IFA.Simulator.Core.Pipeline;

// ══════════════════════════════════════════════════════════════════
//  IFA PIPELINE — Orquesta Listener → Phaser → Rules → Dispatcher
// ══════════════════════════════════════════════════════════════════

/// <summary>
/// Pipeline principal del agente IFA.
/// Coordina todos los componentes en un flujo async basado en canales.
/// </summary>
public sealed class IfaPipeline
{
    private readonly IEnumerable<IDeviceListener> _listeners;
    private readonly RulesEngine                  _rulesEngine;
    private readonly Dispatcher                   _dispatcher;
    private readonly ILogger<IfaPipeline>         _log;
    private readonly SimulatorOptions             _opts;

    // Canal interno de comunicación entre Listener/Phaser y el RulesEngine
    private readonly Channel<AlarmEvent> _eventChannel;

    // Estado observable en tiempo real
    public SimulatorState State { get; } = new();

    // Evento para notificar al dashboard cuando hay cambios
    public event Action? StateChanged;

    public IfaPipeline(
        IEnumerable<IDeviceListener> listeners,
        RulesEngine                  rulesEngine,
        Dispatcher                   dispatcher,
        IOptions<SimulatorOptions>   opts,
        ILogger<IfaPipeline>         log)
    {
        _listeners   = listeners;
        _rulesEngine = rulesEngine;
        _dispatcher  = dispatcher;
        _log         = log;
        _opts        = opts.Value;

        _eventChannel = Channel.CreateBounded<AlarmEvent>(
            new BoundedChannelOptions(capacity: 256)
            {
                FullMode      = BoundedChannelFullMode.DropOldest,
                SingleReader  = true,
                SingleWriter  = false
            });
    }

    /// <summary>
    /// Inicia el pipeline completo.
    /// Listeners corren en paralelo; el consumer procesa en serie.
    /// </summary>
    public async Task RunAsync(CancellationToken ct)
    {
        State.Status    = SimulatorStatus.Running;
        State.StartedAt = DateTime.Now;
        NotifyStateChanged();

        _log.LogInformation("IFA Pipeline iniciado. Listeners: {Count}",
            _listeners.Count());

        // Productor: un task por listener
        var producerTasks = _listeners
            .Select(l => ProduceEventsAsync(l, ct))
            .ToList();

        // Consumer: procesa eventos del canal
        var consumerTask = ConsumeEventsAsync(ct);

        await Task.WhenAll(producerTasks).ConfigureAwait(false);

        // Cerrar el canal cuando todos los listeners terminen
        _eventChannel.Writer.TryComplete();

        await consumerTask.ConfigureAwait(false);

        State.Status = SimulatorStatus.Stopped;
        NotifyStateChanged();

        _log.LogInformation("IFA Pipeline completado. Total eventos: {Total}", State.TotalEvents);
    }

    // ── Productor ────────────────────────────────────────────────

    private async Task ProduceEventsAsync(IDeviceListener listener, CancellationToken ct)
    {
        var phaser = DriverFactory.GetPhaser(listener.Manufacturer);

        if (phaser is null)
        {
            _log.LogWarning("No hay Phaser para fabricante: {Manufacturer}", listener.Manufacturer);
            return;
        }

        _log.LogInformation("Iniciando listener para {Manufacturer}", listener.Manufacturer);

        await foreach (var raw in listener.ListenAsync(ct).ConfigureAwait(false))
        {
            try
            {
                // PHASER: decodifica RAW → AlarmEvent
                var alarmEvent = phaser.Decode(raw);

                _log.LogInformation(
                    "[PHASER:{Manufacturer}] {EventId} — {Type} | {Severity} | {Zone}",
                    listener.Manufacturer, alarmEvent.EventId,
                    alarmEvent.Type, alarmEvent.Severity, alarmEvent.Zone);

                // Publicar al canal para que el consumer lo procese
                await _eventChannel.Writer
                    .WriteAsync(alarmEvent, ct)
                    .ConfigureAwait(false);

                await Task.Delay(_opts.EventIntervalMs, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error procesando evento de {Manufacturer}", listener.Manufacturer);
            }
        }
    }

    // ── Consumer ────────────────────────────────────────────────

    private async Task ConsumeEventsAsync(CancellationToken ct)
    {
        await foreach (var ev in _eventChannel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
        {
            // RULES ENGINE
            var actions = _rulesEngine.Evaluate(ev);

            // DISPATCHER
            var logs = await _dispatcher.DispatchAsync(actions).ConfigureAwait(false);

            // Actualizar estado
            UpdateState(ev, logs);
        }
    }

    // ── Estado ──────────────────────────────────────────────────

    private void UpdateState(AlarmEvent ev, IReadOnlyList<DispatchLog> logs)
    {
        State.TotalEvents++;
        State.DispatchedCount += logs.Count;

        switch (ev.Severity)
        {
            case Severity.Critical: State.CriticalCount++; break;
            case Severity.Warning:  State.WarningCount++;  break;
            default:                State.InfoCount++;     break;
        }

        State.RecentEvents.Insert(0, ev);
        if (State.RecentEvents.Count > _opts.MaxRecentEventsDisplay)
            State.RecentEvents.RemoveAt(State.RecentEvents.Count - 1);

        foreach (var log in logs)
        {
            State.RecentLogs.Insert(0, log);
            if (State.RecentLogs.Count > _opts.MaxRecentEventsDisplay)
                State.RecentLogs.RemoveAt(State.RecentLogs.Count - 1);
        }

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}


// ══════════════════════════════════════════════════════════════════
//  DI REGISTRATION
// ══════════════════════════════════════════════════════════════════

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios del Core IFA en el contenedor DI.
    /// </summary>
    public static IServiceCollection AddIfaSimulatorCore(
        this IServiceCollection services,
        Action<SimulatorOptions>? configureOptions = null)
    {
        // Options
        var optBuilder = services.AddOptions<SimulatorOptions>();
        if (configureOptions is not null)
            optBuilder.Configure(configureOptions);

        // Listeners — uno por fabricante
        services.AddSingleton<IDeviceListener, DmpListener>();
        services.AddSingleton<IDeviceListener, AxisListener>();
        services.AddSingleton<IDeviceListener, HanwhaListener>();

        // Channels — uno por destino
        services.AddSingleton<INotificationChannel, WebClientChannel>();
        services.AddSingleton<INotificationChannel, VmsChannel>();
        services.AddSingleton<INotificationChannel, EmailChannel>();
        services.AddSingleton<INotificationChannel, InstantMessageChannel>();

        // Core
        services.AddSingleton<RulesEngine>();
        services.AddSingleton<Dispatcher>();
        services.AddSingleton<IfaPipeline>();

        return services;
    }
}
