using IFA.Simulator.Core.Models;
using Microsoft.Extensions.Logging;

namespace IFA.Simulator.Core.Drivers;

internal static class DriverHelpers
{
    public static Dictionary<string, string> ParseParts(string payload) =>
        payload.Split('|')
               .Where(p => p.Contains(':'))
               .Select(p => p.Split(':', 2))
               .ToDictionary(a => a[0], a => a[1]);

    public static string NewEventId() =>
        Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}

public interface IDeviceListener
{
    string Manufacturer { get; }
    IAsyncEnumerable<RawDeviceMessage> ListenAsync(CancellationToken ct);
}

public sealed class DmpListener : IDeviceListener
{
    private readonly ILogger<DmpListener> _log;
    public string Manufacturer => "DMP";

    private static readonly string[] Payloads =
    [
        "EVENT|ZONE:01|CODE:1130|ACCT:4521|AREA:1",
        "EVENT|ZONE:02|CODE:1110|ACCT:4521|AREA:1",
        "EVENT|ZONE:03|CODE:1137|ACCT:4521|AREA:1",
        "EVENT|ZONE:04|CODE:1120|ACCT:4521|AREA:2",
        "EVENT|ZONE:01|CODE:3130|ACCT:4521|AREA:1",
        "EVENT|ZONE:05|CODE:1301|ACCT:4521|AREA:3",
        "EVENT|ZONE:02|CODE:3110|ACCT:4521|AREA:1",
    ];

    public DmpListener(ILogger<DmpListener> log) => _log = log;

    public async IAsyncEnumerable<RawDeviceMessage> ListenAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var payload in Payloads)
        {
            if (ct.IsCancellationRequested) yield break;
            _log.LogDebug("[LISTENER:DMP] {Payload}", payload);
            yield return new RawDeviceMessage($"DMP-{Random.Shared.Next(100, 999)}", Manufacturer, payload, DateTime.Now);
            await Task.Delay(1800, ct).ConfigureAwait(false);
        }
    }
}

public sealed class AxisListener : IDeviceListener
{
    private readonly ILogger<AxisListener> _log;
    public string Manufacturer => "AXIS";

    private static readonly string[] Payloads =
    [
        "AXIS|TYP:FIRE|ZONE:A1|SEV:HIGH|CAM:CH01",
        "AXIS|TYP:TAMPER|ZONE:B2|SEV:MEDIUM|CAM:CH03",
        "AXIS|TYP:INTRUSION|ZONE:C1|SEV:HIGH|CAM:CH02",
        "AXIS|TYP:MEDICAL|ZONE:A3|SEV:HIGH|CAM:CH04",
    ];

    public AxisListener(ILogger<AxisListener> log) => _log = log;

    public async IAsyncEnumerable<RawDeviceMessage> ListenAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var payload in Payloads)
        {
            if (ct.IsCancellationRequested) yield break;
            _log.LogDebug("[LISTENER:AXIS] {Payload}", payload);
            yield return new RawDeviceMessage($"AXIS-{Random.Shared.Next(100, 999)}", Manufacturer, payload, DateTime.Now);
            await Task.Delay(2200, ct).ConfigureAwait(false);
        }
    }
}

public sealed class HanwhaListener : IDeviceListener
{
    private readonly ILogger<HanwhaListener> _log;
    public string Manufacturer => "HANWHA";

    private static readonly string[] Payloads =
    [
        "HNW|EVT:MOTION|CHANNEL:1|CONF:95|TS:AUTO",
        "HNW|EVT:FIRE_SMOKE|CHANNEL:3|CONF:88|TS:AUTO",
        "HNW|EVT:LOITERING|CHANNEL:2|CONF:72|TS:AUTO",
    ];

    public HanwhaListener(ILogger<HanwhaListener> log) => _log = log;

    public async IAsyncEnumerable<RawDeviceMessage> ListenAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var payload in Payloads)
        {
            if (ct.IsCancellationRequested) yield break;
            _log.LogDebug("[LISTENER:HANWHA] {Payload}", payload);
            yield return new RawDeviceMessage($"HNW-{Random.Shared.Next(100, 999)}", Manufacturer, payload, DateTime.Now);
            await Task.Delay(2500, ct).ConfigureAwait(false);
        }
    }
}

public interface IDevicePhaser
{
    string     Manufacturer { get; }
    AlarmEvent Decode(RawDeviceMessage raw);
}

public sealed class DmpPhaser : IDevicePhaser
{
    public string Manufacturer => "DMP";

    private static readonly Dictionary<string, (EventType, Severity, string)> CodeMap = new()
    {
        ["1110"] = (EventType.Fire,        Severity.Critical, "Alarma de fuego activada"),
        ["1120"] = (EventType.Panic,       Severity.Critical, "Botón de pánico activado"),
        ["1130"] = (EventType.Intrusion,   Severity.Critical, "Intrusión detectada en zona"),
        ["1137"] = (EventType.LowBattery,  Severity.Warning,  "Batería baja en panel"),
        ["1301"] = (EventType.AcPowerLoss, Severity.Warning,  "Pérdida de energía AC"),
        ["3110"] = (EventType.ZoneRestore, Severity.Info,     "Zona de fuego restaurada"),
        ["3130"] = (EventType.ZoneRestore, Severity.Info,     "Zona de intrusión restaurada"),
    };

    public AlarmEvent Decode(RawDeviceMessage raw)
    {
        var parts = DriverHelpers.ParseParts(raw.RawPayload);
        var zone  = parts.GetValueOrDefault("ZONE", "??");
        var code  = parts.GetValueOrDefault("CODE", "0000");
        var area  = parts.GetValueOrDefault("AREA", "1");

        var (type, severity, desc) = CodeMap.TryGetValue(code, out var mapped)
            ? mapped
            : (EventType.Alarm, Severity.Warning, $"Código DMP desconocido: {code}");

        return new AlarmEvent(
            DriverHelpers.NewEventId(), raw.DeviceId, raw.Manufacturer,
            type, severity, $"Área {area} / Zona {zone}", desc, raw.ReceivedAt);
    }
}

public sealed class AxisPhaser : IDevicePhaser
{
    public string Manufacturer => "AXIS";

    public AlarmEvent Decode(RawDeviceMessage raw)
    {
        var parts = DriverHelpers.ParseParts(raw.RawPayload);
        var typ   = parts.GetValueOrDefault("TYP", "UNKNOWN");
        var zone  = parts.GetValueOrDefault("ZONE", "??");
        var sev   = parts.GetValueOrDefault("SEV", "LOW");

        var type = typ switch
        {
            "FIRE"      => EventType.Fire,
            "TAMPER"    => EventType.Tamper,
            "INTRUSION" => EventType.Intrusion,
            "MEDICAL"   => EventType.Medical,
            _           => EventType.Alarm
        };

        var severity = sev switch
        {
            "HIGH"   => Severity.Critical,
            "MEDIUM" => Severity.Warning,
            _        => Severity.Info
        };

        return new AlarmEvent(
            DriverHelpers.NewEventId(), raw.DeviceId, raw.Manufacturer,
            type, severity, $"Zona {zone}", $"AXIS: {typ} detectado en {zone}", raw.ReceivedAt);
    }
}

public sealed class HanwhaPhaser : IDevicePhaser
{
    public string Manufacturer => "HANWHA";

    public AlarmEvent Decode(RawDeviceMessage raw)
    {
        var parts   = DriverHelpers.ParseParts(raw.RawPayload);
        var evt     = parts.GetValueOrDefault("EVT", "UNKNOWN");
        var channel = parts.GetValueOrDefault("CHANNEL", "?");
        var conf    = parts.GetValueOrDefault("CONF", "0");

        var (type, severity) = evt switch
        {
            "FIRE_SMOKE" => (EventType.Fire,      Severity.Critical),
            "MOTION"     => (EventType.Intrusion,  Severity.Warning),
            "LOITERING"  => (EventType.Intrusion,  Severity.Warning),
            _            => (EventType.Alarm,       Severity.Info)
        };

        return new AlarmEvent(
            DriverHelpers.NewEventId(), raw.DeviceId, raw.Manufacturer,
            type, severity, $"Canal {channel}", $"Hanwha: {evt} (confianza: {conf}%)", raw.ReceivedAt);
    }
}

public static class DriverFactory
{
    private static readonly Dictionary<string, IDevicePhaser> Phasers = new()
    {
        ["DMP"]    = new DmpPhaser(),
        ["AXIS"]   = new AxisPhaser(),
        ["HANWHA"] = new HanwhaPhaser(),
    };

    public static IDevicePhaser? GetPhaser(string manufacturer) =>
        Phasers.TryGetValue(manufacturer.ToUpperInvariant(), out var p) ? p : null;
}