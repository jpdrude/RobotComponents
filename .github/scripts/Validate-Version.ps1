# Validate-Version.ps1
# Compares a git tag version against the CurrentVersion in VersionNumbering.cs

param(
    [Parameter(Mandatory)]
    [string]$Tag,

    [Parameter(Mandatory)]
    [string]$VersionFilePath
)

$ErrorActionPreference = 'Stop'

$tagVersion = $Tag -replace '^v', ''

$versionFile = Get-Content $VersionFilePath -Raw
if ($versionFile -match 'CurrentVersion\s*=\s*"([^"]+)"') {
    $codeVersion = $Matches[1]
} else {
    throw "Could not parse version from $VersionFilePath"
}

Write-Host "Tag version:  $tagVersion"
Write-Host "Code version: $codeVersion"

if ($tagVersion -ne $codeVersion) {
    throw "Tag version ($tagVersion) does not match $VersionFilePath ($codeVersion). Update VersionNumbering.cs before tagging."
}
