# GitHub Changelog Generator
# This script fetches all commits since a specified commit hash and generates a changelog

param(
    [Parameter(Mandatory=$true)]
    [string]$RepoPath,
    
    [Parameter(Mandatory=$false)]
    [switch]$IncludeAuthor,
    
    [Parameter(Mandatory=$false)]
    [switch]$IncludeDate
)

$Branch = "ikgeo"
$SinceCommit = "34f534fe"

# Verify we're in a git repository
if (-not (Test-Path (Join-Path $RepoPath ".git"))) {
    Write-Error "Not a git repository: $RepoPath"
    exit 1
}

# Change to repository directory
Push-Location $RepoPath

# Verify the commit hash exists
$commitExists = git cat-file -t $SinceCommit 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "Commit hash '$SinceCommit' not found in repository"
    exit 1
}

Write-Host "Fetching commits since $SinceCommit..."

# Build the git log format string
$formatString = "%H%n%s%n%b%n%an%n%ad%n---END---"
    
# Determine the commit range
$commitRange = if ($Branch) {
    "$SinceCommit..$Branch"
} else {
    "$SinceCommit..HEAD"
}

Write-Host "Commit range: $commitRange"
    
# Get all commits since the specified commit (excluding the specified commit itself)
# Git returns an array of lines, so we need to join them
$gitLogArray = git log $commitRange --pretty=format:$formatString --date=short
  
if ($null -eq $gitLogArray -or $gitLogArray.Count -eq 0) {
    Write-Warning "No commits found since $SinceCommit"
    Write-Host "DEBUG: Git exit code was: $LASTEXITCODE" -ForegroundColor Magenta
    exit 0
}

# Join the array into a single string with newlines
$gitLog = $gitLogArray -join "`n"

if ([string]::IsNullOrWhiteSpace($gitLog)) {
    Write-Warning "No commits found since $SinceCommit"
    Write-Host "DEBUG: Git exit code was: $LASTEXITCODE" -ForegroundColor Magenta
    exit 0
}

Write-Host "Found git log data: $($gitLog.Length) characters, $($gitLogArray.Count) lines"

# Parse the git log output
$commits = @()
$logEntries = $gitLog -split "---END---" | Where-Object { $_.Trim() -ne "" }
    
foreach ($entry in $logEntries) {
    $lines = $entry.Trim() -split "`n"
    if ($lines.Count -ge 5) {
        $commit = @{
            Hash = $lines[0].Trim()
            Subject = $lines[1].Trim()
            Body = ($lines[2..($lines.Count-3)] -join "`n").Trim()
            Author = $lines[$lines.Count-2].Trim()
            Date = $lines[$lines.Count-1].Trim()
        }
        $commits += $commit
    }
}

Write-Host "Found $($commits.Count) commits"

# Generate changelog content
$changelogContent = @()
$changelogContent += "### Changelog"
$changelogContent += "`n"
$changelogContent += "Generated on: $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
$changelogContent += "`n"
$changelogContent += "---"
$changelogContent += "`n"

foreach ($commit in $commits) {
    #$changelogContent += "$($commit.Subject)"

    #if (-not [string]::IsNullOrWhiteSpace($commit.Body)) {
    #    $changelogContent += $commit.Body
    #}

    $fullText = "$($commit.Subject)"
    if (-not [string]::IsNullOrWhiteSpace($commit.Body)) {
        $fullText += $commit.Body
    }

    # Split by line breaks and filter out empty lines
    $paragraphs = $fullText -split "`n" | Where-Object { $_.Trim() -ne "" }
    
    # Add bullets to each paragraph
    foreach ($paragraph in $paragraphs) {
        $changelogContent += "- $($paragraph.Trim())"
        $changelogContent += "`n"
    }

    # Build metadata line
    $changelogContent += "`n"
    $changelogContent += "`t"
    $metadata = "**Commit:** ``$($commit.Hash.Substring(0, 7))``"
        
    if ($IncludeDate) {
        $metadata += " | **Date:** $($commit.Date)"
    }
        
    if ($IncludeAuthor) {
        $metadata += " | **Author:** $($commit.Author)"
    }
        
    $changelogContent += $metadata
    $changelogContent += ""
        
    $changelogContent += "`n"
    $changelogContent += "`n"
    $changelogContent += "---"
    $changelogContent += "`n"
    $changelogContent += "`n"
}


$changelog = @"
# Changelog

## All notable changes to this modified version of Robot Components are documented here.

$changelogContent

"@

$changelog | Out-File -FilePath (Join-Path $RepoPath "CHANGELOG.md") -Encoding UTF8
Write-Host "Changelog generated"