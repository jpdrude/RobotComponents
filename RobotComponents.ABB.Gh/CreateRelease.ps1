param([string]$OutputPath)

function Create-InstallMarkdown {
    param([string]$Path)
    
    $content = @"
# Installation Instructions

## Download and Install

1. **Clean old installations**:
   - If you previously installed Robot Components via the Package Manager, uninstall it.
   - Open Rhino
   - Call the command ``PackageManager``
   - Find "Robot Components" in the Installed tab and uninstall it.

2. **Download** the latest release files from the [Releases page](https://github.com/jpdrude/RobotComponents/releases)

3. **Unblock the zip file** (important to prevent Windows security warnings):
   - Right-click the downloaded zip file
   - Select **Properties**
   - Check the **Unblock** checkbox at the bottom
   - Click **OK**

4. **Extract** the zip file contents

5. **Install** the plugin:
   - Open File Explorer and navigate to: ``%appdata%\Grasshopper\Libraries``
   - Paste all extracted files into this folder

6. **Restart** Rhino and Grasshopper

The components should now be available in Grasshopper.

The RobotComponentsEDEK.gha assembly can be installed in the same manner. It is a current built from the [RobotComponents-EDEK-Presets](https://github.com/EDEK-UniKassel/RobotComponents-EDEK-Presets) repository.
"@
    
    $content | Out-File -FilePath $Path -Encoding UTF8
}

# Remove trailing backslashes
$OutputPath = $OutputPath.TrimEnd('\').Trim()

Add-Type -AssemblyName Microsoft.VisualBasic
$version = [Microsoft.VisualBasic.Interaction]::InputBox("Enter version (e.g., v1.0.0) or leave empty to skip", "Create Release")

if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Host "Skipping release"
    exit 0
}

if (-not (Test-Path $OutputPath)) {
    Write-Error "Path does not exist: $OutputPath"
    exit 1
}

$files = Get-ChildItem -Path $OutputPath -Recurse | Where-Object { $_.Extension -in '.dll','.gha','.txt' }

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
