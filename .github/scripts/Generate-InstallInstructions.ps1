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

## Updating Existing Definitions

If you have `.gh`/`.ghx` files with components from an older version, Grasshopper can swap them for the current versions automatically, with wires reconnected: open the file, then use the Grasshopper menu **Solution -> Upgrade Components** (or right-click the canvas and choose **Solution -> Upgrade Components**). Any component this affects will be flagged with an "obsolete" (grey/hidden) icon beforehand if you'd rather check first.
"@

$content | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Install instructions written to $OutputPath"
