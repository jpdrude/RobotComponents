param([string]$OutputPath)

function Create-InstallMarkdown {
    param([string]$Path)
    
    $content = @"
# Installation Instructions

## Download and Install

1. **Download** the latest release files from the [Releases page](https://github.com/jpdrude/RobotComponents/releases)

2. **Unblock the zip file** (important to prevent Windows security warnings):
   - Right-click the downloaded zip file
   - Select **Properties**
   - Check the **Unblock** checkbox at the bottom
   - Click **OK**

3. **Extract** the zip file contents

4. **Install** the plugin:
   - Open File Explorer and navigate to: ``%appdata%\Grasshopper\Libraries``
   - Paste all extracted files into this folder

5. **Restart** Rhino and Grasshopper

The components should now be available in Grasshopper.
"@
    
    $content | Out-File -FilePath $Path -Encoding UTF8
}

# Remove trailing backslashes
$OutputPath = $OutputPath.TrimEnd('\').Trim()

Add-Type -AssemblyName Microsoft.VisualBasic
$version = [Microsoft.VisualBasic.Interaction]::InputBox("Enter version (e.g., 0.1.0) or leave empty to skip", "Create Release")

if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Host "Skipping release"
    exit 0
}

if (-not (Test-Path $OutputPath)) {
    Write-Error "Path does not exist: $OutputPath"
    exit 1
}

$files = Get-ChildItem -Path $OutputPath -Recurse | Where-Object { $_.Extension -in '.dll','.gha' }

if ($files.Count -eq 0) {
    Write-Error "No files found"
    exit 1
}

Write-Host "Creating zip file..."
$zipPath = Join-Path $OutputPath "RobotComponents-v$version.zip"
Compress-Archive -Path $files.FullName -DestinationPath $zipPath -Force

$installMd = Join-Path $OutputPath "INSTALL.md"
Create-InstallMarkdown -Path $installMd

$filesToUpload = $files.FullName + $installMd

Write-Host "Creating GitHub release..."

gh auth status 2>&1 | Write-Host

gh release create $version $zipPath --repo jpdrude/RobotComponents --title "Release $version" --notes-file $installMd

if ($LASTEXITCODE -ne 0) {
    Write-Error "GitHub release failed: $output"
    exit 1
}

Write-Host "Release created successfully"
exit 0
