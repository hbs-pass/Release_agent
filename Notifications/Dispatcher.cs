using IFA.Simulator.Models;

namespace IFA.Simulator.Notifications;

// ─────────────────────────────────────────────────────────
//  DISPATCHER — Ejecuta acciones determinadas por el Rules Engine
// ─────────────────────────────────────────────────────────

/// <summary>
/// Canal de salida. Cada implementación representa un destino
/// (VMS, Email, WebClient, IM).
/// </summary>
public interface INotificationChannel
{
    ActionTarget Target { get; }
    Task SendAsync(DispatchAction action);
}

/// <summary>Canal WebClient — simula POST al cliente web de IFA.</summary>
public class WebClientChannel : INotificationChannel
{
    public ActionTarget Target => ActionTarget.WebClient;

    public Task SendAsync(DispatchAction action)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"    → [WEB CLIENT] POST enviado | EventId: {action.EventId}");
        Console.WriteLine($"      Payload: {action.Payload}");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}

/// <summary>Canal VMS — simula integración con Video Management System.</summary>
public class VmsChannel : INotificationChannel
{
    public ActionTarget Target => ActionTarget.VMS;

    public Task SendAsync(DispatchAction action)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"    → [VMS] Comando enviado | EventId: {action.EventId}");
        Console.WriteLine($"      {action.Payload}");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}

/// <summary>Canal Email — simula despacho de correo electrónico.</summary>
public class EmailChannel : INotificationChannel
{
    public ActionTarget Target => ActionTarget.Email;

    public Task SendAsync(DispatchAction action)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"    → [EMAIL] Enviado a operadores | EventId: {action.EventId}");
        Console.WriteLine($"      Asunto: {action.Payload[..Math.Min(70, action.Payload.Length)]}...");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}

/// <summary>Canal IM — simula envío por mensajería instantánea (Teams, Slack, WhatsApp).</summary>
public class InstantMessageChannel : INotificationChannel
{
    public ActionTarget Target => ActionTarget.InstantMessage;

    public Task SendAsync(DispatchAction action)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"    → [IM] Mensaje enviado | EventId: {action.EventId}");
        Console.WriteLine($"      {action.Payload}");
        Console.ResetColor();
        return Task.CompletedTask;
    }
}

/// <summary>
/// Dispatcher central. Recibe acciones del Rules Engine
/// y las enruta al canal correcto.
/// </summary>
public class Dispatcher
{
    private readonly Dictionary<ActionTarget, INotificationChannel> _channels;

    public Dispatcher()
    {
        var channels = new INotificationChannel[]
        {
            new WebClientChannel(),
            new VmsChannel(),
            new EmailChannel(),
            new InstantMessageChannel(),
        };

        _channels = channels.ToDictionary(c => c.Target);
    }

    public async Task DispatchAsync(IReadOnlyList<DispatchAction> actions)
    {
        if (actions.Count == 0) return;

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\n  [DISPATCHER] Ejecutando {actions.Count} acción(es)...");
        Console.ResetColor();

        foreach (var action in actions)
        {
            if (_channels.TryGetValue(action.Target, out var channel))
            {
                await channel.SendAsync(action);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"    ✗ Canal no registrado: {action.Target}");
                Console.ResetColor();
            }
        }
    }
}
