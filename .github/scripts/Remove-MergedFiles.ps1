# =============================================================================
# Remove-MergedFiles.ps1
# =============================================================================
# Purpose : After SmartAssembly runs, it may produce merged or intermediate
#           assemblies (e.g. Merged_*.dll, *.bak, *.pdb that were consumed).
#           This script removes those files from the obfuscated output folder.
#
# Usage   : .\Remove-MergedFiles.ps1 -TargetDir "C:\artifacts\obfuscated"
# =============================================================================

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType Container })]
    [string] $TargetDir,

    # Extend this list with any patterns specific to your project
    [string[]] $PatternsToRemove = @(
        "Merged_*.dll",          # SmartAssembly merged intermediates
        "*.Merged.dll",
        "*.bak",                 # Backup copies created by SA
        "*.original.dll",
        "*.pdb",                 # Debug symbols (not needed in production)
        "*.xml",                 # IntelliSense XML docs (not needed at runtime)
        "*Obfuscation*.log",     # SA log files
        "SmartAssembly.*.dll"    # SA runtime helpers (if statically linked)
    ),

    # Files that should never be deleted, even if they match a pattern above
    [string[]] $ProtectedFiles = @(
        "*.resources.dll"        # Satellite resource assemblies — keep them
    )
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "=== Remove-MergedFiles ===" -ForegroundColor Cyan
Write-Host "Target : $TargetDir"

# Build a set of protected file names for quick look-up
$protectedSet = [System.Collections.Generic.HashSet[string]]::new(
    [System.StringComparer]::OrdinalIgnoreCase
)

foreach ($pattern in $ProtectedFiles) {
    Get-ChildItem -Path $TargetDir -Filter $pattern -Recurse -ErrorAction SilentlyContinue |
        ForEach-Object { [void]$protectedSet.Add($_.FullName) }
}

$removedCount = 0
$skippedCount = 0

foreach ($pattern in $PatternsToRemove) {
    $matches = Get-ChildItem -Path $TargetDir -Filter $pattern -Recurse -ErrorAction SilentlyContinue

    foreach ($file in $matches) {
        if ($protectedSet.Contains($file.FullName)) {
            Write-Host "  [SKIP] Protected: $($file.Name)" -ForegroundColor DarkYellow
            $skippedCount++
            continue
        }

        try {
            Remove-Item -Path $file.FullName -Force
            Write-Host "  [DEL]  $($file.FullName)" -ForegroundColor Yellow
            $removedCount++
        }
        catch {
            Write-Warning "  Could not remove $($file.FullName): $_"
        }
    }
}

Write-Host ""
Write-Host "Done. Removed: $removedCount file(s), Skipped (protected): $skippedCount file(s)." -ForegroundColor Green