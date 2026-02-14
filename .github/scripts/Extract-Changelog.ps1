# Extract-Changelog.ps1
# Parse CHANGELOG.md into release notes, taking the first N sections.

param(
    [Parameter(Mandatory)]
    [string]$Tag,

    [string]$ChangelogPath = "CHANGELOG.md",

    [string]$OutputPath = "release-notes.md",

    [int]$MaxSections = 5,

    [int]$MaxLength = 10000
)

$ErrorActionPreference = 'Stop'

$changelog = Get-Content $ChangelogPath -Raw -ErrorAction SilentlyContinue

if ([string]::IsNullOrWhiteSpace($changelog)) {
    $notes = "Release $Tag"
} else {
    $sections = $changelog -split '---'
    $relevantSections = $sections | Select-Object -First $MaxSections
    $notes = ($relevantSections -join "`n---`n").Trim()

    if ($notes.Length -gt $MaxLength) {
        $notes = $notes.Substring(0, $MaxLength) + "`n`n... (see CHANGELOG.md for full details)"
    }
}

$notes | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Release notes written to $OutputPath ($($notes.Length) chars)"
