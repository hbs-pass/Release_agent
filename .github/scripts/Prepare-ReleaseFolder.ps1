# =============================================================================
# Prepare-ReleaseFolder.ps1
# =============================================================================
# Purpose : Build the definitive release folder by:
#           1. Copying ALL published files (base layer).
#           2. Overwriting with obfuscated counterparts (top layer).
#
#           After this script runs, $ReleaseDir contains the final content
#           ready to be packaged into an installer.
#
# Usage   : .\Prepare-ReleaseFolder.ps1 `
#               -PublishDir    "C:\artifacts\publish" `
#               -ObfuscatedDir "C:\artifacts\obfuscated" `
#               -ReleaseDir    "C:\artifacts\release"
# =============================================================================

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType Container })]
    [string] $PublishDir,

    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType Container })]
    [string] $ObfuscatedDir,

    [Parameter(Mandatory = $true)]
    [string] $ReleaseDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Copy-Layer {
    param (
        [string] $SourceDir,
        [string] $DestDir,
        [string] $Label
    )

    Write-Host ""
    Write-Host "--- Layer: $Label ---" -ForegroundColor Cyan
    $count = 0

    foreach ($file in Get-ChildItem -Path $SourceDir -File -Recurse) {
        $relative = $file.FullName.Substring($SourceDir.TrimEnd('\').Length).TrimStart('\')
        $dest     = Join-Path $DestDir $relative
        $destDir  = Split-Path $dest -Parent

        if (-not (Test-Path $destDir)) {
            New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        }

        $existed = Test-Path $dest
        Copy-Item -Path $file.FullName -Destination $dest -Force

        $symbol = if ($existed) { "[OVR]" } else { "[NEW]" }
        $color  = if ($existed) { "Yellow" } else { "Green" }
        Write-Host "  $symbol $relative" -ForegroundColor $color
        $count++
    }

    Write-Host "  => $count file(s) processed from $Label"
}

# ---------------------------------------------------------------------------
Write-Host "=== Prepare-ReleaseFolder ===" -ForegroundColor Cyan
Write-Host "Publish     : $PublishDir"
Write-Host "Obfuscated  : $ObfuscatedDir"
Write-Host "Release     : $ReleaseDir"

# Wipe and recreate the release folder for a clean state
if (Test-Path $ReleaseDir) {
    Write-Host ""
    Write-Host "Cleaning existing release folder..." -ForegroundColor DarkYellow
    Remove-Item -Path $ReleaseDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $ReleaseDir | Out-Null

# LAYER 1 — Published files (everything from dotnet publish)
Copy-Layer -SourceDir $PublishDir -DestDir $ReleaseDir -Label "Published (base layer)"

# LAYER 2 — Obfuscated files (overwrite matching files; add new ones)
Copy-Layer -SourceDir $ObfuscatedDir -DestDir $ReleaseDir -Label "Obfuscated (top layer)"

# ---------------------------------------------------------------------------
Write-Host ""
Write-Host "=== Release folder ready ===" -ForegroundColor Green

# Verification: list what ended up in the release folder
$totalFiles = (Get-ChildItem -Path $ReleaseDir -File -Recurse).Count
Write-Host "Total files in release folder: $totalFiles"