# Extract-Changelog.ps1
# Builds release notes from CHANGELOG.md for commits since the previous tag,
# prepended with installation instructions.

param(
    [Parameter(Mandatory)]
    [string]$Tag,

    [string]$ChangelogPath = "CHANGELOG.md",

    [string]$InstallPath = "",

    [string]$OutputPath = "release-notes.md",

    [int]$MaxSections = 5,  # ADD THIS

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

# Check if changelog file exists and is not empty
if (-not (Test-Path $ChangelogPath)) {
    Write-Host "Changelog file not found at: $ChangelogPath"
    $fallback = "Release $Tag"
    $fallback | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Host "Using fallback release notes"
    exit 0
}

$changelog = Get-Content $ChangelogPath -Raw -ErrorAction SilentlyContinue

if ([string]::IsNullOrWhiteSpace($changelog)) {
    Write-Host "Changelog is empty"
    $fallback = "Release $Tag"
    $fallback | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Host "Using fallback release notes"
    exit 0
}

# Split changelog into sections
$sections = $changelog -split '---' | Where-Object { $_.Trim() -ne "" }
Write-Host "Found $($sections.Count) total sections in changelog"

# Filter sections based on commit hashes or take first MaxSections
$matchingSections = @()

if ($hashes.Count -gt 0) {
    # Filter by commit hashes
    foreach ($section in $sections) {
        foreach ($hash in $hashes) {
            if ($section -match [regex]::Escape($hash)) {
                $matchingSections += $section.Trim()
                break
            }
        }
        
        # Stop if we've reached MaxSections
        if ($matchingSections.Count -ge $MaxSections) {
            break
        }
    }
} else {
    # No hashes - just take first MaxSections
    $matchingSections = $sections | Select-Object -First $MaxSections | ForEach-Object { $_.Trim() }
}

Write-Host "Selected $($matchingSections.Count) sections (MaxSections: $MaxSections)"

if ($matchingSections.Count -gt 0) {
    $changelogSections = ($matchingSections -join "`n`n---`n`n").Trim()
}

# If no sections matched, use fallback
if ([string]::IsNullOrWhiteSpace($changelogSections)) {
    Write-Host "No matching changelog sections found"
    $fallback = "Release $Tag"
    $fallback | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Host "Using fallback release notes"
    exit 0
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

# Check if content exceeds MaxLength
if ($notes.Length -gt $MaxLength) {
    $notes = $notes.Substring(0, $MaxLength) + "`n`n... (see CHANGELOG.md for full details)"
    Write-Host "Content truncated at $MaxLength characters"
}

$notes | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Release notes written to $OutputPath ($($notes.Length) chars, $($matchingSections.Count) sections)"
