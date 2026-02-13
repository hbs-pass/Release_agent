namespace IFA.Simulator.Models;

// ─────────────────────────────────────────────────────────
//  Tipos base del dominio IFA
// ─────────────────────────────────────────────────────────

/// <summary>
/// Tipos de eventos soportados por el sistema IFA.
/// </summary>
public enum EventType
{
    Intrusion,
    Fire,
    Tamper,
    LowBattery,
    AcPowerLoss,
    ZoneRestore,
    Alarm
}

/// <summary>
/// Severidad del evento, usada por el Rules Engine para priorizar acciones.
/// </summary>
public enum Severity
{
    Info,
    Warning,
    Critical
}

/// <summary>
/// Mensaje RAW recibido desde el dispositivo en la red.
/// Esto es lo que escucha el LISTENER antes de ser procesado.
/// </summary>
public record RawDeviceMessage(
    string DeviceId,
    string Manufacturer,
    string RawPayload,
    DateTime ReceivedAt
);

/// <summary>
/// Evento ya decodificado por el PHASER — lenguaje legible por humanos.
/// </summary>
public record AlarmEvent(
    string EventId,
    string DeviceId,
    string Manufacturer,
    EventType Type,
    Severity Severity,
    string Zone,
    string Description,
    DateTime OccurredAt
);

/// <summary>
/// Acción a ejecutar determinada por el RULES ENGINE.
/// </summary>
public record DispatchAction(
    string EventId,
    ActionTarget Target,
    string Payload,
    DateTime ScheduledAt
);

/// <summary>
/// Destinos posibles del DISPATCHER.
/// </summary>
public enum ActionTarget
{
    WebClient,
    VMS,
    Email,
    InstantMessage
}
