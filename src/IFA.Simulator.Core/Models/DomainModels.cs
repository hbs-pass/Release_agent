namespace IFA.Simulator.Core.Models;

// ══════════════════════════════════════════════════════════════════
//  DOMAIN MODELS — IFA Intrusion Fire Agent
// ══════════════════════════════════════════════════════════════════

public enum EventType
{
    Intrusion,
    Fire,
    Tamper,
    LowBattery,
    AcPowerLoss,
    ZoneRestore,
    Alarm,
    Panic,
    Medical
}

public enum Severity
{
    Info    = 0,
    Warning = 1,
    Critical= 2
}

public enum ActionTarget
{
    WebClient,
    VMS,
    Email,
    InstantMessage
}

public enum SimulatorStatus
{
    Idle,
    Running,
    Paused,
    Stopped
}

/// <summary>
/// Mensaje RAW que llega del dispositivo al LISTENER.
/// </summary>
public sealed record RawDeviceMessage(
    string   DeviceId,
    string   Manufacturer,
    string   RawPayload,
    DateTime ReceivedAt
);

/// <summary>
/// Evento decodificado por el PHASER — ya legible por humanos.
/// </summary>
public sealed record AlarmEvent(
    string    EventId,
    string    DeviceId,
    string    Manufacturer,
    EventType Type,
    Severity  Severity,
    string    Zone,
    string    Description,
    DateTime  OccurredAt
)
{
    public string SeverityLabel => Severity switch
    {
        Severity.Critical => "CRÍTICO",
        Severity.Warning  => "ADVERTENCIA",
        _                 => "INFO"
    };

    public string SeverityColor => Severity switch
    {
        Severity.Critical => "error",
        Severity.Warning  => "warning",
        _                 => "info"
    };

    public string TypeIcon => Type switch
    {
        EventType.Fire      => "local_fire_department",
        EventType.Intrusion => "sensors",
        EventType.Tamper    => "warning",
        EventType.LowBattery=> "battery_alert",
        EventType.ZoneRestore => "check_circle",
        EventType.Panic     => "sos",
        EventType.Medical   => "medical_services",
        _                   => "notifications_active"
    };
}

/// <summary>
/// Acción determinada por el RULES ENGINE para el DISPATCHER.
/// </summary>
public sealed record DispatchAction(
    string       EventId,
    ActionTarget Target,
    string       Payload,
    DateTime     ScheduledAt
);

/// <summary>
/// Registro de log de una acción ejecutada por el DISPATCHER.
/// </summary>
public sealed record DispatchLog(
    string       EventId,
    ActionTarget Target,
    string       Summary,
    bool         Success,
    DateTime     ExecutedAt
);

/// <summary>
/// Snapshot del estado del simulador en tiempo real.
/// </summary>
public sealed class SimulatorState
{
    public SimulatorStatus Status          { get; set; } = SimulatorStatus.Idle;
    public int             TotalEvents     { get; set; }
    public int             CriticalCount   { get; set; }
    public int             WarningCount    { get; set; }
    public int             InfoCount       { get; set; }
    public int             DispatchedCount { get; set; }
    public DateTime?       StartedAt       { get; set; }
    public List<AlarmEvent>   RecentEvents { get; set; } = [];
    public List<DispatchLog>  RecentLogs   { get; set; } = [];
}

/// <summary>
/// Opciones configurables del simulador.
/// </summary>
public sealed class SimulatorOptions
{
    public int    EventIntervalMs        { get; set; } = 2000;
    public int    MaxRecentEventsDisplay { get; set; } = 50;
    public bool   EnableEmailChannel     { get; set; } = true;
    public bool   EnableVmsChannel       { get; set; } = true;
    public bool   EnableImChannel        { get; set; } = true;
    public bool   EnableWebClientChannel { get; set; } = true;
    public string LogDirectory           { get; set; } = "logs";
}
