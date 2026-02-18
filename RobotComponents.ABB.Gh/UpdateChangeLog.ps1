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

# Function to clean up commit message text
function Clean-CommitText {
    param([string]$text)
    
    if ([string]::IsNullOrWhiteSpace($text)) {
        return ""
    }
    
    # First, normalize line endings to just \n
    $text = $text -replace "`r`n", "`n"
    $text = $text -replace "`r", "`n"
    
    # Replace newlines that are NOT preceded by a period with a space
    # Use a temporary marker for period+newline combinations
    $text = $text -replace '\.[\s]*\n', ".<KEEPBREAK>"
    
    # Now remove all remaining newlines (these don't follow periods)
    $text = $text -replace '\n', ' '
    
    # Restore the newlines after periods
    $text = $text -replace '<KEEPBREAK>', "`n"
    
    # Split by the preserved line breaks
    $lines = $text -split "`n"
    
    $cleanedLines = @()
    
    foreach ($line in $lines) {
        # Clean up whitespace
        $line = $line.Trim() -replace '\s+', ' '
        
        # Skip empty lines
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }
        
        $cleanedLines += $line
    }
    
    return $cleanedLines
}

$Branch = "ikgeo"
$SinceCommit = "1382290e"

Set-Location $RepoPath

# Verify we're in a git repository
if (-not (Test-Path (Join-Path $RepoPath ".git"))) {
    Write-Error "Not a git repository: $RepoPath"
    exit 1
}

$RepoPath = (Resolve-Path $RepoPath).Path

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
$changelogContent += "Generated on: " + (Get-Date -Format 'yyyy-MM-dd HH:mm')
$changelogContent += "`n"
$changelogContent += "---"
$changelogContent += "`n"

foreach ($commit in $commits) {
        # Combine subject and body
        $fullText = "$($commit.Subject)`n$($commit.Body)"
        
        # Clean and split into lines
        $cleanedLines = Clean-CommitText -text $fullText
        
        # Add bullet points for each line
        foreach ($line in $cleanedLines) {
            if ($line.StartsWith("-")) {
                # Line already has a bullet, keep it as-is
                $changelogContent += "`t"
                $changelogContent += $line
            } else {
                # Add bullet point
                $changelogContent += "- $line"
            }
            $changelogContent += "`n"
        }
        
        $changelogContent += ""
        
        # Build metadata line
        $metadata = "  **Commit:** ``$($commit.Hash.Substring(0, 7))``"
        
        if ($IncludeDate) {
            $metadata += " | **Date:** $($commit.Date)"
        }
        
        if ($IncludeAuthor) {
            $metadata += " | **Author:** $($commit.Author)"
        }
        
        $changelogContent += "`n"
        $changelogContent += $metadata
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

# Make sure RepoPath is set to current directory if not specified
if ([string]::IsNullOrEmpty($RepoPath)) {
    $RepoPath = Get-Location
}

$changelog | Out-File -FilePath (Join-Path $RepoPath "CHANGELOG.md") -Encoding UTF8
Write-Host "Changelog generated"