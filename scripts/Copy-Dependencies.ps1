# =============================================================================
# Copy-Dependencies.ps1
# =============================================================================
# Purpose : Copy additional runtime files (runtimes, native DLLs, config
#           files, license files, third-party redistributables, etc.) from
#           the `extras/` folder into the obfuscated release output.
#
# Usage   : .\Copy-Dependencies.ps1 `
#               -SourceDir "C:\repo\extras" `
#               -TargetDir "C:\artifacts\obfuscated"
# =============================================================================

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType Container })]
    [string] $SourceDir,

    [Parameter(Mandatory = $true)]
    [string] $TargetDir,

    # When $true, existing files in $TargetDir are overwritten
    [bool] $Overwrite = $true,

    # Recurse into sub-folders of $SourceDir, preserving directory structure
    [bool] $Recurse = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Copy-Dependencies ===" -ForegroundColor Cyan
Write-Host "Source : $SourceDir"
Write-Host "Target : $TargetDir"

# Ensure target exists
New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null

$copiedCount = 0
$skippedCount = 0

$getChildParams = @{
    Path    = $SourceDir
    File    = $true
    Recurse = $Recurse
}

foreach ($file in Get-ChildItem @getChildParams) {
    # Preserve relative folder structure inside SourceDir
    $relativePath = $file.FullName.Substring($SourceDir.TrimEnd('\').Length).TrimStart('\')
    $destination  = Join-Path $TargetDir $relativePath
    $destDir      = Split-Path $destination -Parent

    # Create intermediate directories if needed
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
    }

    if ((Test-Path $destination) -and -not $Overwrite) {
        Write-Host "  [SKIP] Already exists: $relativePath" -ForegroundColor DarkYellow
        $skippedCount++
        continue
    }

    try {
        Copy-Item -Path $file.FullName -Destination $destination -Force
        Write-Host "  [COPY] $relativePath" -ForegroundColor Green
        $copiedCount++
    }
    catch {
        Write-Error "  Failed to copy $($file.FullName): $_"
    }
}

Write-Host ""
Write-Host "Done. Copied: $copiedCount file(s), Skipped: $skippedCount file(s)." -ForegroundColor Green