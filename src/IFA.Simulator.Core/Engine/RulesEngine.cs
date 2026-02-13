using IFA.Simulator.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IFA.Simulator.Core.Engine;

// ══════════════════════════════════════════════════════════════════
//  RULES ENGINE — Motor de decisiones
// ══════════════════════════════════════════════════════════════════

public interface IRule
{
    string Name     { get; }
    int    Priority { get; }
    bool   Matches(AlarmEvent ev);
    IEnumerable<DispatchAction> GetActions(AlarmEvent ev);
}

// ── Reglas concretas ──────────────────────────────────────────────

/// <summary>Regla: eventos críticos → VMS + Email + WebClient.</summary>
public sealed class CriticalEventRule : IRule
{
    public string Name     => "CriticalEventRule";
    public int    Priority => 1;

    public bool Matches(AlarmEvent ev) =>
        ev.Severity == Severity.Critical;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.VMS,
            $"OPEN_CAM|zone={ev.Zone}|event={ev.Type}|device={ev.DeviceId}", DateTime.Now);

        yield return new DispatchAction(ev.EventId, ActionTarget.Email,
            $"[ALERTA CRÍTICA] {ev.Description} | Zona: {ev.Zone} | Panel: {ev.DeviceId} | {ev.OccurredAt:HH:mm:ss}", DateTime.Now);

        yield return new DispatchAction(ev.EventId, ActionTarget.WebClient,
            $"{{\"id\":\"{ev.EventId}\",\"type\":\"{ev.Type}\",\"zone\":\"{ev.Zone}\",\"severity\":\"CRITICAL\",\"ts\":\"{ev.OccurredAt:O}\"}}", DateTime.Now);
    }
}

/// <summary>Regla: fuego → IM adicional.</summary>
public sealed class FireEventRule : IRule
{
    public string Name     => "FireEventRule";
    public int    Priority => 2;

    public bool Matches(AlarmEvent ev) =>
        ev.Type == EventType.Fire;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.InstantMessage,
            $"🔥 FUEGO — {ev.Zone} | {ev.DeviceId} | {ev.OccurredAt:HH:mm:ss} | Conf. VMS abierto", DateTime.Now);
    }
}

/// <summary>Regla: pánico → IM inmediato.</summary>
public sealed class PanicEventRule : IRule
{
    public string Name     => "PanicEventRule";
    public int    Priority => 2;

    public bool Matches(AlarmEvent ev) =>
        ev.Type == EventType.Panic;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.InstantMessage,
            $"🆘 PÁNICO — {ev.Zone} | {ev.DeviceId} | {ev.OccurredAt:HH:mm:ss}", DateTime.Now);
    }
}

/// <summary>Regla: advertencias → WebClient únicamente.</summary>
public sealed class WarningEventRule : IRule
{
    public string Name     => "WarningEventRule";
    public int    Priority => 3;

    public bool Matches(AlarmEvent ev) =>
        ev.Severity == Severity.Warning;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.WebClient,
            $"{{\"id\":\"{ev.EventId}\",\"type\":\"{ev.Type}\",\"zone\":\"{ev.Zone}\",\"severity\":\"WARNING\",\"ts\":\"{ev.OccurredAt:O}\"}}", DateTime.Now);
    }
}

/// <summary>Regla: restauraciones → log informativo.</summary>
public sealed class RestoreEventRule : IRule
{
    public string Name     => "RestoreEventRule";
    public int    Priority => 4;

    public bool Matches(AlarmEvent ev) =>
        ev.Type == EventType.ZoneRestore;

    public IEnumerable<DispatchAction> GetActions(AlarmEvent ev)
    {
        yield return new DispatchAction(ev.EventId, ActionTarget.WebClient,
            $"{{\"id\":\"{ev.EventId}\",\"type\":\"RESTORE\",\"zone\":\"{ev.Zone}\",\"ts\":\"{ev.OccurredAt:O}\"}}", DateTime.Now);
    }
}

// ── Rules Engine central ──────────────────────────────────────────

/// <summary>
/// Evalúa todas las reglas registradas en orden de prioridad
/// y retorna el conjunto de acciones a ejecutar.
/// </summary>
public sealed class RulesEngine
{
    private readonly IReadOnlyList<IRule>  _rules;
    private readonly ILogger<RulesEngine>  _log;

    public RulesEngine(ILogger<RulesEngine> log)
    {
        _log = log;

        _rules = new List<IRule>
        {
            new CriticalEventRule(),
            new FireEventRule(),
            new PanicEventRule(),
            new WarningEventRule(),
            new RestoreEventRule(),
        }
        .OrderBy(r => r.Priority)
        .ToList();

        _log.LogInformation("RulesEngine inicializado con {Count} reglas", _rules.Count);
    }

    public IReadOnlyList<DispatchAction> Evaluate(AlarmEvent ev)
    {
        _log.LogDebug("Evaluando evento {EventId} — Tipo: {Type} | Severidad: {Severity}",
            ev.EventId, ev.Type, ev.Severity);

        var actions = new List<DispatchAction>();

        foreach (var rule in _rules)
        {
            if (!rule.Matches(ev)) continue;

            var ruleActions = rule.GetActions(ev).ToList();
            actions.AddRange(ruleActions);

            _log.LogDebug("  ✓ Regla [{Rule}] aplicada → {Count} acción(es)", rule.Name, ruleActions.Count);
        }

        if (actions.Count == 0)
            _log.LogDebug("  — Sin reglas coincidentes para evento {EventId}", ev.EventId);
        else
            _log.LogInformation("Evento {EventId} → {Count} acción(es) generadas", ev.EventId, actions.Count);

        return actions;
    }
}
