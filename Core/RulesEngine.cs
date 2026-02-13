using IFA.Simulator.Models;

namespace IFA.Simulator.Core;

// ─────────────────────────────────────────────────────────
//  RULES ENGINE — Decide qué acción tomar
// ─────────────────────────────────────────────────────────

/// <summary>
/// Regla individual del motor. Evalúa un evento y produce
/// una lista de acciones si se cumplen sus condiciones.
/// </summary>
public interface IRule
{
    string Name { get; }
    bool Matches(AlarmEvent ev);
    IEnumerable<DispatchAction> GetActions(AlarmEvent ev);
}

/// <summary>
/// Regla: eventos CRÍTICOS → notifica a VMS + Email + WebClient.
/// </summary>
public class CriticalEventRule : IRule
{
    public string Name => "CriticalEventRule";

    public bool Matches(AlarmEvent ev) =>
        ev.Severity == Severity.Critical;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.VMS,
            $"[VMS] Abrir cámara de {ev.Zone} | Evento: {ev.Type}", DateTime.Now);

        yield return new DispatchAction(ev.EventId, ActionTarget.Email,
            $"ALERTA CRÍTICA — {ev.Description} | Zona: {ev.Zone} | Panel: {ev.DeviceId}", DateTime.Now);

        yield return new DispatchAction(ev.EventId, ActionTarget.WebClient,
            $"{{\"eventId\":\"{ev.EventId}\",\"type\":\"{ev.Type}\",\"zone\":\"{ev.Zone}\",\"severity\":\"CRITICAL\"}}", DateTime.Now);
    }
}

/// <summary>
/// Regla: eventos de FUEGO → agrega notificación por IM adicional.
/// </summary>
public class FireEventRule : IRule
{
    public string Name => "FireEventRule";

    public bool Matches(AlarmEvent ev) =>
        ev.Type == EventType.Fire;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.InstantMessage,
            $"🔥 FUEGO DETECTADO — {ev.Zone} | Panel: {ev.DeviceId} | {ev.OccurredAt:HH:mm:ss}", DateTime.Now);
    }
}

/// <summary>
/// Regla: eventos de WARNING → solo WebClient y log.
/// </summary>
public class WarningEventRule : IRule
{
    public string Name => "WarningEventRule";

    public bool Matches(AlarmEvent ev) =>
        ev.Severity == Severity.Warning;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.WebClient,
            $"{{\"eventId\":\"{ev.EventId}\",\"type\":\"{ev.Type}\",\"zone\":\"{ev.Zone}\",\"severity\":\"WARNING\"}}", DateTime.Now);
    }
}

/// <summary>
/// Regla: restauraciones → solo log informativo en WebClient.
/// </summary>
public class RestoreEventRule : IRule
{
    public string Name => "RestoreEventRule";

    public bool Matches(AlarmEvent ev) =>
        ev.Type == EventType.ZoneRestore;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.WebClient,
            $"{{\"eventId\":\"{ev.EventId}\",\"type\":\"RESTORE\",\"zone\":\"{ev.Zone}\"}}", DateTime.Now);
    }
}

/// <summary>
/// Rules Engine central. Evalúa todas las reglas registradas
/// contra un evento y retorna el conjunto de acciones resultantes.
/// </summary>
public class RulesEngine
{
    private readonly List<IRule> _rules;

    public RulesEngine()
    {
        // Registrar reglas en orden de evaluación
        _rules =
        [
            new CriticalEventRule(),
            new FireEventRule(),
            new WarningEventRule(),
            new RestoreEventRule(),
        ];
    }

    public IReadOnlyList<DispatchAction> Evaluate(AlarmEvent ev)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  [RULES ENGINE] Evaluando evento {ev.EventId} — Tipo: {ev.Type} | Severidad: {ev.Severity}");
        Console.ResetColor();

        var actions = new List<DispatchAction>();

        foreach (var rule in _rules)
        {
            if (rule.Matches(ev))
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"    ✓ Regla aplicada: {rule.Name}");
                Console.ResetColor();
                actions.AddRange(rule.GetActions(ev));
            }
        }

        if (actions.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    — Sin reglas coincidentes. Evento ignorado.");
            Console.ResetColor();
        }

        return actions;
    }
}
