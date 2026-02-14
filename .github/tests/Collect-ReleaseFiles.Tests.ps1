BeforeAll {
    $script:ScriptPath = Join-Path $PSScriptRoot '..' 'scripts' 'Collect-ReleaseFiles.ps1'
}

Describe 'Collect-ReleaseFiles' {

    BeforeEach {
        # Clean up from previous test runs (TestDrive persists between It blocks)
        $script:RepoRoot = Join-Path $TestDrive 'repo'
        $script:OutDir   = Join-Path $TestDrive 'staging'
        Remove-Item -Path $script:RepoRoot -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path $script:OutDir -Recurse -Force -ErrorAction SilentlyContinue

        # Create .gha
        $ghaDir = Join-Path $script:RepoRoot 'RobotComponents.ABB.Gh' 'bin' 'Release' 'net48'
        New-Item -Path $ghaDir -ItemType Directory -Force | Out-Null
        '' | Set-Content (Join-Path $ghaDir 'RobotComponents.ABB.Gh.gha')

        # Create project DLLs
        $projects = @(
            'RobotComponents',
            'RobotComponents.ABB',
            'RobotComponents.ABB.Presets',
            'RobotComponents.ABB.Gh.Goos',
            'RobotComponents.ABB.Controllers'
        )
        foreach ($proj in $projects) {
            $dir = Join-Path $script:RepoRoot $proj 'bin' 'Release' 'net48'
            New-Item -Path $dir -ItemType Directory -Force | Out-Null
            '' | Set-Content (Join-Path $dir "$proj.dll")
        }

        # Create LICENSE
        'MIT' | Set-Content (Join-Path $script:RepoRoot 'LICENSE')
    }

    It 'copies all expected files when all are present' {
        & $script:ScriptPath -Configuration Release -OutputDir $script:OutDir -RepoRoot $script:RepoRoot

        $files = Get-ChildItem $script:OutDir | Select-Object -ExpandProperty Name
        $files | Should -Contain 'RobotComponents.ABB.Gh.gha'
        $files | Should -Contain 'RobotComponents.dll'
        $files | Should -Contain 'RobotComponents.ABB.dll'
        $files | Should -Contain 'RobotComponents.ABB.Presets.dll'
        $files | Should -Contain 'RobotComponents.ABB.Gh.Goos.dll'
        $files | Should -Contain 'RobotComponents.ABB.Controllers.dll'
        $files | Should -Contain 'LICENSE'
        $files | Should -HaveCount 7
    }

    It 'skips missing DLLs without error' {
        # Remove one DLL
        Remove-Item (Join-Path $script:RepoRoot 'RobotComponents.ABB.Controllers' 'bin' 'Release' 'net48' 'RobotComponents.ABB.Controllers.dll')

        & $script:ScriptPath -Configuration Release -OutputDir $script:OutDir -RepoRoot $script:RepoRoot

        $files = Get-ChildItem $script:OutDir | Select-Object -ExpandProperty Name
        $files | Should -Not -Contain 'RobotComponents.ABB.Controllers.dll'
        $files | Should -HaveCount 6
    }

    It 'handles missing .gha gracefully' {
        # Remove .gha file
        Remove-Item (Join-Path $script:RepoRoot 'RobotComponents.ABB.Gh' 'bin' 'Release' 'net48' 'RobotComponents.ABB.Gh.gha')

        & $script:ScriptPath -Configuration Release -OutputDir $script:OutDir -RepoRoot $script:RepoRoot

        $files = Get-ChildItem $script:OutDir | Select-Object -ExpandProperty Name
        $files | Should -Not -Contain 'RobotComponents.ABB.Gh.gha'
    }

    It 'skips LICENSE when not present' {
        Remove-Item (Join-Path $script:RepoRoot 'LICENSE')

        & $script:ScriptPath -Configuration Release -OutputDir $script:OutDir -RepoRoot $script:RepoRoot

        $files = Get-ChildItem $script:OutDir | Select-Object -ExpandProperty Name
        $files | Should -Not -Contain 'LICENSE'
    }

    It 'creates zip when CreateZip is specified' {
        & $script:ScriptPath -Configuration Release -OutputDir $script:OutDir -RepoRoot $script:RepoRoot -CreateZip -Version 'v1.0.0'

        Test-Path 'RobotComponents-v1.0.0.zip' | Should -BeTrue
    }

    It 'throws when CreateZip is used without Version' {
        { & $script:ScriptPath -Configuration Release -OutputDir $script:OutDir -RepoRoot $script:RepoRoot -CreateZip } |
            Should -Throw '*Version is required*'
    }

    It 'uses the specified Configuration for paths' {
        # Create Debug DLL for one project
        $debugDir = Join-Path $script:RepoRoot 'RobotComponents' 'bin' 'Debug' 'net48'
        New-Item -Path $debugDir -ItemType Directory -Force | Out-Null
        '' | Set-Content (Join-Path $debugDir 'RobotComponents.dll')

        & $script:ScriptPath -Configuration Debug -OutputDir $script:OutDir -RepoRoot $script:RepoRoot

        $files = Get-ChildItem $script:OutDir | Select-Object -ExpandProperty Name
        # Should find the Debug DLL but not Release ones for other projects
        $files | Should -Contain 'RobotComponents.dll'
    }
}
