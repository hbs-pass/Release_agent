# IFA Simulator — HyperIT
**Intrusion Fire Agent — Simulador de flujo completo**

```
LISTENER → PHASER → RULES ENGINE → DISPATCHER
```

Construido en **.NET 10** | Blazor Server + MudBlazor | Worker Service

---

## Arquitectura

```
IFA.Simulator/
├── src/
│   ├── IFA.Simulator.Core/          # Librería central (independiente de UI)
│   │   ├── Models/                  # Modelos de dominio
│   │   ├── Drivers/                 # Listener + Phaser por fabricante
│   │   ├── Engine/                  # Rules Engine
│   │   ├── Notifications/           # Dispatcher + Canales
│   │   └── Pipeline/                # Orquestador + DI Registration
│   │
│   ├── IFA.Simulator.Web/           # Dashboard Blazor Server (tiempo real)
│   │   ├── Components/Pages/        # Dashboard.razor
│   │   ├── Components/Layout/       # MainLayout dark theme
│   │   ├── Services/                # DashboardService
│   │   └── wwwroot/                 # CSS personalizado
│   │
│   └── IFA.Simulator.Worker/        # Background Service (headless)
│       ├── IfaWorker.cs             # BackgroundService
│       └── Program.cs               # Host + Serilog
└── docs/
```

---

## Fabricantes soportados

| Fabricante | Listener | Phaser | Protocolo simulado |
|---|---|---|---|
| DMP | `DmpListener` | `DmpPhaser` | Contact ID / SIA-DC09 |
| AXIS | `AxisListener` | `AxisPhaser` | Propietario AXIS |
| Hanwha Vision | `HanwhaListener` | `HanwhaPhaser` | Analítica de video |

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows / Linux / macOS

---

## Ejecución rápida

### Dashboard Web (recomendado para demo)
```bash
cd src/IFA.Simulator.Web
dotnet run
# Abrir: https://localhost:5001
```

### Worker headless (para servidor / servicio)
```bash
cd src/IFA.Simulator.Worker
dotnet run
```

---

## Build para Release

### Web Dashboard
```bash
dotnet publish src/IFA.Simulator.Web/IFA.Simulator.Web.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o ./release/web
```

### Worker Service
```bash
dotnet publish src/IFA.Simulator.Worker/IFA.Simulator.Worker.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o ./release/worker
```

### Linux (servidor)
```bash
dotnet publish src/IFA.Simulator.Web/IFA.Simulator.Web.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -o ./release/web-linux
```

---

## Instalar como Windows Service (Worker)

```powershell
sc.exe create "IFA-Simulator" binPath="C:\release\worker\IFA.Simulator.Worker.exe"
sc.exe start "IFA-Simulator"
```

---

## Agregar un nuevo fabricante

**1. Crear Listener:**
```csharp
public class NuevoListener : IDeviceListener {
    public string Manufacturer => "NUEVO";
    public async IAsyncEnumerable<RawDeviceMessage> ListenAsync(CancellationToken ct) {
        // Conectar al dispositivo real (TCP, UDP, Serial, HTTP)
    }
}
```

**2. Crear Phaser:**
```csharp
public class NuevoPhaser : IDevicePhaser {
    public string Manufacturer => "NUEVO";
    public AlarmEvent Decode(RawDeviceMessage raw) { ... }
}
```

**3. Registrar en `DriverFactory` y DI (`ServiceCollectionExtensions`).**

---

## Canales del Dispatcher

| Canal | Clase | Integración real |
|---|---|---|
| WebClient | `WebClientChannel` | HTTP POST al cliente IFA |
| VMS | `VmsChannel` | SDK Milestone / NX Witness / DW Spectrum |
| Email | `EmailChannel` | SmtpClient / SendGrid / Mailgun |
| IM | `InstantMessageChannel` | Teams Webhook / Slack API / WhatsApp Business |

---

## Licencia
HyperIT © 2025 — Uso interno
