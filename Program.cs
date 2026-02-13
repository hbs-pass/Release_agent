using IFA.Simulator.Core;
using IFA.Simulator.Drivers;
using IFA.Simulator.Notifications;

// ═══════════════════════════════════════════════════════════════
//  IFA — INTRUSION FIRE AGENT SIMULATOR
//  Flujo: LISTENER → PHASER → RULES ENGINE → DISPATCHER
//  Plataforma: .NET 10 | Console App
//  Basado en arquitectura HEMIBLADE / Hyper IT (hyperit.do)
// ═══════════════════════════════════════════════════════════════

Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintHeader();

// ── Instanciar componentes del pipeline ──────────────────────────────

var listeners = new IDeviceListener[]
{
    new DmpListener(),
    new AxisListener(),
};

var rulesEngine = new RulesEngine();
var dispatcher  = new Dispatcher();

using var cts = new CancellationTokenSource();

// Cancelar con Ctrl+C
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\n\n  [SISTEMA] Señal de cancelación recibida. Deteniendo...");
    cts.Cancel();
};

// ── Ejecutar un listener por fabricante en paralelo ──────────────────

var listenerTasks = listeners.Select(listener => RunListenerPipelineAsync(
    listener, rulesEngine, dispatcher, cts.Token));

await Task.WhenAll(listenerTasks);

PrintFooter();

// ════════════════════════════════════════════════════════════════════
//  Pipeline por Listener
// ════════════════════════════════════════════════════════════════════

static async Task RunListenerPipelineAsync(
    IDeviceListener   listener,
    RulesEngine       rulesEngine,
    Dispatcher        dispatcher,
    CancellationToken ct)
{
    PrintSection($"FABRICANTE: {listener.Manufacturer}");

    var phaser = PhaserFactory.GetPhaser(listener.Manufacturer);

    if (phaser is null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ No hay Phaser registrado para {listener.Manufacturer}. Saltando.");
        Console.ResetColor();
        return;
    }

    await foreach (var raw in listener.ListenAsync(ct))
    {
        Console.WriteLine();
        PrintStep(1, "LISTENER  → Mensaje RAW capturado");
        Console.WriteLine($"     Device   : {raw.DeviceId}");
        Console.WriteLine($"     Payload  : {raw.RawPayload}");
        Console.WriteLine($"     Timestamp: {raw.ReceivedAt:yyyy-MM-dd HH:mm:ss}");

        // ── PHASER: decodifica el mensaje RAW ─────────────────────────
        PrintStep(2, "PHASER    → Decodificando mensaje");
        var alarmEvent = phaser.Decode(raw);
        Console.WriteLine($"     EventId  : {alarmEvent.EventId}");
        Console.WriteLine($"     Tipo     : {alarmEvent.Type}");
        Console.WriteLine($"     Severidad: {alarmEvent.Severity}");
        Console.WriteLine($"     Zona     : {alarmEvent.Zone}");
        Console.WriteLine($"     Desc     : {alarmEvent.Description}");

        // ── RULES ENGINE: evalúa qué acciones tomar ───────────────────
        PrintStep(3, "RULES ENGINE → Evaluando reglas");
        var actions = rulesEngine.Evaluate(alarmEvent);

        // ── DISPATCHER: ejecuta las acciones ──────────────────────────
        PrintStep(4, "DISPATCHER → Enviando notificaciones");
        await dispatcher.DispatchAsync(actions);

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"\n  ✓ Evento {alarmEvent.EventId} procesado completamente.");
        Console.ResetColor();
        Console.WriteLine(new string('─', 65));
    }
}

// ════════════════════════════════════════════════════════════════════
//  Helpers de presentación
// ════════════════════════════════════════════════════════════════════

static void PrintHeader()
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine();
    Console.WriteLine("  ╔═══════════════════════════════════════════════════════════╗");
    Console.WriteLine("  ║          IFA — INTRUSION FIRE AGENT SIMULATOR            ║");
    Console.WriteLine("  ║     LISTENER → PHASER → RULES ENGINE → DISPATCHER        ║");
    Console.WriteLine("  ║                   .NET 10 | HyperIT                      ║");
    Console.WriteLine("  ╚═══════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

static void PrintSection(string title)
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"\n  ▶ {title}");
    Console.WriteLine(new string('═', 65));
    Console.ResetColor();
}

static void PrintStep(int number, string description)
{
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine($"\n  [{number}] {description}");
    Console.ResetColor();
}

static void PrintFooter()
{
    Console.ForegroundColor = ConsoleColor.DarkGreen;
    Console.WriteLine("\n  ════════════════════════════════════════════════════════════");
    Console.WriteLine("  Simulación IFA completada. Todos los eventos procesados.");
    Console.WriteLine("  ════════════════════════════════════════════════════════════");
    Console.ResetColor();
}
