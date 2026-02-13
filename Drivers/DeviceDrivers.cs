using IFA.Simulator.Models;

namespace IFA.Simulator.Drivers;

// ─────────────────────────────────────────────────────────
//  LISTENER — Escucha mensajes RAW de dispositivos en red
// ─────────────────────────────────────────────────────────

/// <summary>
/// Contrato base para todos los Listeners.
/// Cada fabricante requiere su propia instancia.
/// </summary>
public interface IDeviceListener
{
    string Manufacturer { get; }
    IAsyncEnumerable<RawDeviceMessage> ListenAsync(CancellationToken ct);
}

/// <summary>
/// Listener simulado para paneles DMP.
/// En producción abriría un socket TCP/UDP al panel real.
/// </summary>
public class DmpListener : IDeviceListener
{
    public string Manufacturer => "DMP";

    public async IAsyncEnumerable<RawDeviceMessage> ListenAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        // Datos simulados — reemplazar por socket real en producción
        var simulatedPayloads = new[]
        {
            "EVENT|ZONE:01|CODE:1130|ACCT:1234",   // Intrusion
            "EVENT|ZONE:02|CODE:1110|ACCT:1234",   // Fire
            "EVENT|ZONE:03|CODE:1137|ACCT:1234",   // Low Battery
            "EVENT|ZONE:01|CODE:3130|ACCT:1234",   // Zone Restore
        };

        foreach (var payload in simulatedPayloads)
        {
            if (ct.IsCancellationRequested) yield break;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  [LISTENER:DMP] RAW recibido → {payload}");
            Console.ResetColor();

            yield return new RawDeviceMessage(
                DeviceId:    $"DMP-PANEL-{Random.Shared.Next(100, 999)}",
                Manufacturer: Manufacturer,
                RawPayload:  payload,
                ReceivedAt:  DateTime.Now
            );

            await Task.Delay(TimeSpan.FromSeconds(1.5), ct);
        }
    }
}

/// <summary>
/// Listener simulado para paneles AXIS (control de acceso + fire).
/// </summary>
public class AxisListener : IDeviceListener
{
    public string Manufacturer => "AXIS";

    public async IAsyncEnumerable<RawDeviceMessage> ListenAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var simulatedPayloads = new[]
        {
            "AXIS|TYP:FIRE|ZONE:A1|SEV:HIGH",
            "AXIS|TYP:TAMPER|ZONE:B2|SEV:MEDIUM",
        };

        foreach (var payload in simulatedPayloads)
        {
            if (ct.IsCancellationRequested) yield break;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  [LISTENER:AXIS] RAW recibido → {payload}");
            Console.ResetColor();

            yield return new RawDeviceMessage(
                DeviceId:    $"AXIS-DEV-{Random.Shared.Next(100, 999)}",
                Manufacturer: Manufacturer,
                RawPayload:  payload,
                ReceivedAt:  DateTime.Now
            );

            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }
    }
}


// ─────────────────────────────────────────────────────────
//  PHASER — Decodifica mensajes RAW a AlarmEvent legible
// ─────────────────────────────────────────────────────────

/// <summary>
/// Contrato del Phaser. Cada fabricante requiere su propia implementación.
/// </summary>
public interface IDevicePhaser
{
    string Manufacturer { get; }
    AlarmEvent Decode(RawDeviceMessage raw);
}

/// <summary>
/// Phaser para protocolo DMP (Contact ID / SIA).
/// Mapea códigos DMP a AlarmEvent comprensibles.
/// </summary>
public class DmpPhaser : IDevicePhaser
{
    public string Manufacturer => "DMP";

    // Tabla de códigos DMP → (EventType, Severity, Descripción)
    private static readonly Dictionary<string, (EventType Type, Severity Severity, string Desc)> CodeMap = new()
    {
        { "1130", (EventType.Intrusion, Severity.Critical, "Intrusión detectada en zona") },
        { "1110", (EventType.Fire,      Severity.Critical, "Alarma de fuego activada")     },
        { "1137", (EventType.LowBattery,Severity.Warning,  "Batería baja en panel")        },
        { "3130", (EventType.ZoneRestore,Severity.Info,    "Zona restaurada")              },
    };

    public AlarmEvent Decode(RawDeviceMessage raw)
    {
        var parts = raw.RawPayload.Split('|');
        var zonePart = parts.FirstOrDefault(p => p.StartsWith("ZONE:"))?.Split(':')[1] ?? "??";
        var codePart = parts.FirstOrDefault(p => p.StartsWith("CODE:"))?.Split(':')[1] ?? "0000";

        var (type, severity, desc) = CodeMap.TryGetValue(codePart, out var mapped)
            ? mapped
            : (EventType.Alarm, Severity.Warning, $"Código desconocido: {codePart}");

        return new AlarmEvent(
            EventId:     Guid.NewGuid().ToString("N")[..8].ToUpper(),
            DeviceId:    raw.DeviceId,
            Manufacturer: raw.Manufacturer,
            Type:        type,
            Severity:    severity,
            Zone:        $"Zona {zonePart}",
            Description: desc,
            OccurredAt:  raw.ReceivedAt
        );
    }
}

/// <summary>
/// Phaser para protocolo propietario AXIS.
/// </summary>
public class AxisPhaser : IDevicePhaser
{
    public string Manufacturer => "AXIS";

    public AlarmEvent Decode(RawDeviceMessage raw)
    {
        var parts = raw.RawPayload.Split('|');
        var typPart = parts.FirstOrDefault(p => p.StartsWith("TYP:"))?.Split(':')[1] ?? "UNKNOWN";
        var zonePart = parts.FirstOrDefault(p => p.StartsWith("ZONE:"))?.Split(':')[1] ?? "??";
        var sevPart  = parts.FirstOrDefault(p => p.StartsWith("SEV:"))?.Split(':')[1] ?? "LOW";

        var type = typPart switch
        {
            "FIRE"   => EventType.Fire,
            "TAMPER" => EventType.Tamper,
            _        => EventType.Alarm
        };

        var severity = sevPart switch
        {
            "HIGH"   => Severity.Critical,
            "MEDIUM" => Severity.Warning,
            _        => Severity.Info
        };

        return new AlarmEvent(
            EventId:     Guid.NewGuid().ToString("N")[..8].ToUpper(),
            DeviceId:    raw.DeviceId,
            Manufacturer: raw.Manufacturer,
            Type:        type,
            Severity:    severity,
            Zone:        $"Zona {zonePart}",
            Description: $"Evento AXIS: {typPart} detectado",
            OccurredAt:  raw.ReceivedAt
        );
    }
}

/// <summary>
/// Factory para obtener el Phaser correcto según fabricante.
/// </summary>
public static class PhaserFactory
{
    private static readonly Dictionary<string, IDevicePhaser> Phasers = new()
    {
        { "DMP",  new DmpPhaser()  },
        { "AXIS", new AxisPhaser() },
    };

    public static IDevicePhaser? GetPhaser(string manufacturer) =>
        Phasers.TryGetValue(manufacturer, out var p) ? p : null;
}
