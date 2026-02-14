# Collect-ReleaseFiles.ps1
# Gather .gha, project DLLs, and LICENSE into a staging directory.
# Optionally create a zip archive for releases.

param(
    [Parameter(Mandatory)]
    [string]$Configuration,

    [Parameter(Mandatory)]
    [string]$OutputDir,

    [string]$RepoRoot = ".",

    [switch]$CreateZip,

    [string]$Version = ""
)

$ErrorActionPreference = 'Stop'

New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Collect .gha from the Gh project
$ghaBin = Join-Path $RepoRoot "RobotComponents.ABB.Gh" "bin" $Configuration "net48"
Get-ChildItem -Path $ghaBin -Filter "*.gha" -ErrorAction SilentlyContinue |
    Copy-Item -Destination $OutputDir

# Collect all project DLLs (exclude test and GH2 assemblies)
$projects = @(
    "RobotComponents",
    "RobotComponents.ABB",
    "RobotComponents.ABB.Presets",
    "RobotComponents.ABB.Gh.Goos",
    "RobotComponents.ABB.Controllers"
)
foreach ($proj in $projects) {
    $dll = Join-Path $RepoRoot $proj "bin" $Configuration "net48" "$proj.dll"
    if (Test-Path $dll) {
        Copy-Item $dll -Destination $OutputDir
    }
}

# Include license
$license = Join-Path $RepoRoot "LICENSE"
if (Test-Path $license) {
    Copy-Item $license -Destination $OutputDir
}

Write-Host "Staged files:"
Get-ChildItem $OutputDir | Format-Table Name, Length

if ($CreateZip) {
    if ([string]::IsNullOrWhiteSpace($Version)) {
        throw "Version is required when CreateZip is specified."
    }
    $zipPath = "RobotComponents-$Version.zip"
    Compress-Archive -Path (Join-Path $OutputDir '*') -DestinationPath $zipPath -Force
    Write-Host "Created: $zipPath"
}
