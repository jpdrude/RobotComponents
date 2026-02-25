# Extract-Changelog.ps1
# Builds release notes from CHANGELOG.md for commits since the previous tag,
# prepended with installation instructions.

param(
    [Parameter(Mandatory)]
    [string]$Tag,

    [string]$ChangelogPath = "CHANGELOG.md",

    [string]$InstallPath = "",

    [string]$OutputPath = "release-notes.md",

    [int]$MaxLength = 10000
)

$ErrorActionPreference = 'Stop'

# Find the previous tag
$prevTag = git describe --tags --abbrev=0 "$Tag^" 2>$null
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($prevTag)) {
    Write-Host "No previous tag found, including all commits"
    $commitRange = $Tag
} else {
    Write-Host "Previous tag: $prevTag"
    $commitRange = "$prevTag..$Tag"
}

# Get short hashes of commits in range
$hashLines = git log $commitRange --pretty=format:"%h" 2>$null
$hashes = if ([string]::IsNullOrWhiteSpace($hashLines)) { @() } else { $hashLines -split "`n" | Where-Object { $_ -ne "" } }
Write-Host "Found $($hashes.Count) commits since $prevTag"

# Extract matching sections from CHANGELOG.md
$changelogSections = ""
$changelog = Get-Content $ChangelogPath -Raw -ErrorAction SilentlyContinue

if (-not [string]::IsNullOrWhiteSpace($changelog) -and $hashes.Count -gt 0) {
    $sections = $changelog -split '---'
    $matchingSections = @()

    foreach ($section in $sections) {
        foreach ($hash in $hashes) {
            if ($section -match [regex]::Escape($hash)) {
                $matchingSections += $section.Trim()
                break
            }
        }
    }

    if ($matchingSections.Count -gt 0) {
        $changelogSections = ($matchingSections -join "`n`n---`n`n").Trim()
    }
}

if ([string]::IsNullOrWhiteSpace($changelogSections)) {
    $changelogSections = "_No changelog entries found for this release._"
}

# Read install instructions
$installInstructions = ""
if (-not [string]::IsNullOrWhiteSpace($InstallPath) -and (Test-Path $InstallPath)) {
    $installInstructions = (Get-Content $InstallPath -Raw).Trim()
}

# Combine
$parts = @()
if (-not [string]::IsNullOrWhiteSpace($installInstructions)) {
    $parts += $installInstructions
    $parts += "---"
}
$parts += "## What's New in $Tag"
$parts += ""
$parts += $changelogSections

$notes = $parts -join "`n`n"

if ($notes.Length -gt $MaxLength) {
    $notes = $notes.Substring(0, $MaxLength) + "`n`n... (see CHANGELOG.md for full details)"
}

$notes | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Release notes written to $OutputPath ($($notes.Length) chars)"
