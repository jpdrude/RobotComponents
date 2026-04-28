# Extract-Changelog.ps1
# Builds release notes from CHANGELOG.md for commits since the previous tag,
# prepended with installation instructions.

param(
    [Parameter(Mandatory)]
    [string]$Tag,

    [string]$ChangelogPath = "CHANGELOG.md",

    [string]$InstallPath = "",

    [string]$OutputPath = "release-notes.md",

    [int]$MaxSections = 5,

    [int]$MaxLength = 10000
)

$ErrorActionPreference = 'Stop'

function Write-ReleaseNotes {
    param([string]$ChangelogContent)

    $install = ""
    if (-not [string]::IsNullOrWhiteSpace($InstallPath) -and (Test-Path $InstallPath)) {
        $install = (Get-Content $InstallPath -Raw).Trim()
    }

    $parts = @()
    if (-not [string]::IsNullOrWhiteSpace($install)) {
        $parts += $install
        $parts += "---"
    }
    if (-not [string]::IsNullOrWhiteSpace($ChangelogContent)) {
        $parts += "## What's New in $Tag"
        $parts += ""
        $parts += $ChangelogContent
    } else {
        $parts += "Release $Tag"
    }

    $notes = $parts -join "`n`n"

    if ($notes.Length -gt $MaxLength) {
        $notes = $notes.Substring(0, $MaxLength) + "`n`n... (see CHANGELOG.md for full details)"
        Write-Host "Content truncated at $MaxLength characters"
    }

    $notes | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Host "Release notes written to $OutputPath ($($notes.Length) chars)"
}

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

# Check if changelog file exists and has content
if (-not (Test-Path $ChangelogPath)) {
    Write-Host "Changelog file not found at: $ChangelogPath"
    Write-ReleaseNotes -ChangelogContent ""
    exit 0
}

$changelog = Get-Content $ChangelogPath -Raw -ErrorAction SilentlyContinue

if ([string]::IsNullOrWhiteSpace($changelog)) {
    Write-Host "Changelog is empty"
    Write-ReleaseNotes -ChangelogContent ""
    exit 0
}

# Split changelog into sections and keep only real entries (those with a Commit: marker)
$allSections = $changelog -split '---' | Where-Object { $_.Trim() -ne "" }
$entrySections = $allSections | Where-Object { $_ -match '\*\*Commit:\*\*' }
Write-Host "Found $($entrySections.Count) changelog entry sections"

# Try to match sections by commit hash first
$matchingSections = @()

if ($hashes.Count -gt 0) {
    foreach ($section in $entrySections) {
        foreach ($hash in $hashes) {
            if ($section -match [regex]::Escape($hash)) {
                $matchingSections += $section.Trim()
                break
            }
        }
        if ($matchingSections.Count -ge $MaxSections) { break }
    }
    Write-Host "Hash-matched $($matchingSections.Count) sections"
}

# Fall back to most recent entry sections when hash matching finds nothing
if ($matchingSections.Count -eq 0) {
    Write-Host "No hash-matched sections; using first $MaxSections entry sections"
    $matchingSections = $entrySections | Select-Object -First $MaxSections | ForEach-Object { $_.Trim() }
}

Write-Host "Selected $($matchingSections.Count) sections (MaxSections: $MaxSections)"

$changelogContent = ""
if ($matchingSections.Count -gt 0) {
    $changelogContent = ($matchingSections -join "`n`n---`n`n").Trim()
}

Write-ReleaseNotes -ChangelogContent $changelogContent
