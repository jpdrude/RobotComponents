# Generate-InstallInstructions.ps1
# Write static INSTALL.md content for release artifacts.

param(
    [Parameter(Mandatory)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$content = @"
# Installation Instructions

## Limitations
- Only works for Rhino 8

## Download and Install

1. **Download** the latest release zip from this page

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

$content | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Install instructions written to $OutputPath"
