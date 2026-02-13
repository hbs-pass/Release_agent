using IFA.Simulator.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IFA.Simulator.Core.Notifications;

// ══════════════════════════════════════════════════════════════════
//  CHANNELS — Canales de salida del DISPATCHER
// ══════════════════════════════════════════════════════════════════

public interface INotificationChannel
{
    ActionTarget Target  { get; }
    bool         Enabled { get; }
    Task<DispatchLog> SendAsync(DispatchAction action);
}

public sealed class WebClientChannel : INotificationChannel
{
    private readonly ILogger<WebClientChannel> _log;
    private readonly SimulatorOptions          _opts;

    public ActionTarget Target  => ActionTarget.WebClient;
    public bool         Enabled => _opts.EnableWebClientChannel;

    public WebClientChannel(ILogger<WebClientChannel> log, IOptions<SimulatorOptions> opts)
    {
        _log  = log;
        _opts = opts.Value;
    }

    public Task<DispatchLog> SendAsync(DispatchAction action)
    {
        // En producción: HttpClient.PostAsync() al endpoint del WebClient IFA
        _log.LogInformation("[WEB CLIENT] POST → EventId: {EventId} | {Payload}",
            action.EventId, action.Payload);

        return Task.FromResult(new DispatchLog(
            action.EventId, Target,
            $"WebClient POST enviado — {action.Payload[..Math.Min(60, action.Payload.Length)]}",
            Success: true, DateTime.Now));
    }
}

public sealed class VmsChannel : INotificationChannel
{
    private readonly ILogger<VmsChannel> _log;
    private readonly SimulatorOptions    _opts;

    public ActionTarget Target  => ActionTarget.VMS;
    public bool         Enabled => _opts.EnableVmsChannel;

    public VmsChannel(ILogger<VmsChannel> log, IOptions<SimulatorOptions> opts)
    {
        _log  = log;
        _opts = opts.Value;
    }

    public Task<DispatchLog> SendAsync(DispatchAction action)
    {
        // En producción: integración vía SDK Milestone / NX Witness / DW Spectrum
        _log.LogInformation("[VMS] Comando → EventId: {EventId} | {Payload}",
            action.EventId, action.Payload);

        return Task.FromResult(new DispatchLog(
            action.EventId, Target,
            $"VMS comando enviado — {action.Payload}",
            Success: true, DateTime.Now));
    }
}

public sealed class EmailChannel : INotificationChannel
{
    private readonly ILogger<EmailChannel> _log;
    private readonly SimulatorOptions      _opts;

    public ActionTarget Target  => ActionTarget.Email;
    public bool         Enabled => _opts.EnableEmailChannel;

    public EmailChannel(ILogger<EmailChannel> log, IOptions<SimulatorOptions> opts)
    {
        _log  = log;
        _opts = opts.Value;
    }

    public Task<DispatchLog> SendAsync(DispatchAction action)
    {
        // En producción: SmtpClient o SendGrid / Mailgun
        _log.LogInformation("[EMAIL] Enviado → EventId: {EventId}", action.EventId);

        return Task.FromResult(new DispatchLog(
            action.EventId, Target,
            $"Email enviado — {action.Payload[..Math.Min(80, action.Payload.Length)]}",
            Success: true, DateTime.Now));
    }
}

public sealed class InstantMessageChannel : INotificationChannel
{
    private readonly ILogger<InstantMessageChannel> _log;
    private readonly SimulatorOptions               _opts;

    public ActionTarget Target  => ActionTarget.InstantMessage;
    public bool         Enabled => _opts.EnableImChannel;

    public InstantMessageChannel(ILogger<InstantMessageChannel> log, IOptions<SimulatorOptions> opts)
    {
        _log  = log;
        _opts = opts.Value;
    }

    public Task<DispatchLog> SendAsync(DispatchAction action)
    {
        // En producción: Teams Webhook / Slack API / WhatsApp Business API
        _log.LogInformation("[IM] Mensaje → EventId: {EventId} | {Payload}",
            action.EventId, action.Payload);

        return Task.FromResult(new DispatchLog(
            action.EventId, Target,
            $"IM enviado — {action.Payload}",
            Success: true, DateTime.Now));
    }
}

// ══════════════════════════════════════════════════════════════════
//  DISPATCHER — Enruta acciones a los canales correctos
// ══════════════════════════════════════════════════════════════════

public sealed class Dispatcher
{
    private readonly Dictionary<ActionTarget, INotificationChannel> _channels;
    private readonly ILogger<Dispatcher>                            _log;

    public Dispatcher(
        IEnumerable<INotificationChannel> channels,
        ILogger<Dispatcher>               log)
    {
        _log      = log;
        _channels = channels.ToDictionary(c => c.Target);

        _log.LogInformation("Dispatcher inicializado con {Count} canal(es): {Channels}",
            _channels.Count, string.Join(", ", _channels.Keys));
    }

    public async Task<IReadOnlyList<DispatchLog>> DispatchAsync(IReadOnlyList<DispatchAction> actions)
    {
        if (actions.Count == 0) return [];

        _log.LogDebug("Dispatcher ejecutando {Count} acción(es)", actions.Count);

        var logs = new List<DispatchLog>();

        foreach (var action in actions)
        {
            if (!_channels.TryGetValue(action.Target, out var channel))
            {
                _log.LogWarning("Canal no registrado: {Target}", action.Target);
                continue;
            }

            if (!channel.Enabled)
            {
                _log.LogDebug("Canal {Target} deshabilitado. Omitiendo.", action.Target);
                continue;
            }

            try
            {
                var logEntry = await channel.SendAsync(action).ConfigureAwait(false);
                logs.Add(logEntry);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error en canal {Target} para evento {EventId}",
                    action.Target, action.EventId);

                logs.Add(new DispatchLog(
                    action.EventId, action.Target,
                    $"ERROR: {ex.Message}",
                    Success: false, DateTime.Now));
            }
        }

        return logs;
    }
}
