# IFA Simulator — Build Script
# Uso: ./build-release.ps1

param(
    [string]$Runtime = "win-x64",
    [string]$Output  = "./release"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  IFA Simulator — Build Release"         -ForegroundColor Cyan
Write-Host "  Runtime: $Runtime"                      -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Restaurar paquetes
Write-Host "[1/3] Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore IFA.Simulator.sln
if ($LASTEXITCODE -ne 0) { Write-Error "Restore fallido"; exit 1 }

# Build Web Dashboard
Write-Host "[2/3] Publicando Web Dashboard..." -ForegroundColor Yellow
dotnet publish src/IFA.Simulator.Web/IFA.Simulator.Web.csproj `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -o "$Output/web"

if ($LASTEXITCODE -ne 0) { Write-Error "Publish Web fallido"; exit 1 }

# Build Worker Service
Write-Host "[3/3] Publicando Worker Service..." -ForegroundColor Yellow
dotnet publish src/IFA.Simulator.Worker/IFA.Simulator.Worker.csproj `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -o "$Output/worker"

if ($LASTEXITCODE -ne 0) { Write-Error "Publish Worker fallido"; exit 1 }

Write-Host ""
Write-Host "✓ Build completado exitosamente." -ForegroundColor Green
Write-Host "  Web    → $Output/web"           -ForegroundColor Green
Write-Host "  Worker → $Output/worker"        -ForegroundColor Green
Write-Host ""
