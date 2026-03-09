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

# Include dependency assemblies from DLLs folder
$dllsDir = Join-Path $RepoRoot "DLLs"
$depAssemblies = @(
    "ABB.Robotics.Controllers.PC.dll",
    "MathNet.Numerics.dll",
    "RobotStudio.Services.RobApi.Desktop.dll",
    "RobotStudio.Services.RobApi.dll",
    "ikgeoInterface_GoFa.dll",
    "libgcc_s_seh-1.dll",
    "libstdc++-6.dll",
    "libwinpthread-1.dll"
)
foreach ($dep in $depAssemblies) {
    $depPath = Join-Path $dllsDir $dep
    if (Test-Path $depPath) {
        Copy-Item $depPath -Destination $OutputDir
    }
}

# Include license files
$license = Join-Path $RepoRoot "LICENSE"
if (Test-Path $license) {
    Copy-Item $license -Destination $OutputDir
}
$licensesDir = Join-Path $dllsDir "licenses"
if (Test-Path $licensesDir) {
    $licensesOut = Join-Path $OutputDir "licenses"
    Copy-Item $licensesDir -Destination $licensesOut -Recurse -Force
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
