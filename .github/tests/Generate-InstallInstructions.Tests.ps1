BeforeAll {
    $script:ScriptPath = Join-Path $PSScriptRoot '..' 'scripts' 'Generate-InstallInstructions.ps1'
}

Describe 'Generate-InstallInstructions' {

    It 'creates file with expected content' {
        $output = Join-Path $TestDrive 'INSTALL.md'

        & $script:ScriptPath -OutputPath $output

        Test-Path $output | Should -BeTrue
        $content = Get-Content $output -Raw
        $content | Should -Match 'Installation Instructions'
    }

    It 'includes key installation sections' {
        $output = Join-Path $TestDrive 'INSTALL.md'

        & $script:ScriptPath -OutputPath $output

        $content = Get-Content $output -Raw
        $content | Should -Match 'Unblock'
        $content | Should -Match 'Grasshopper\\Libraries'
        $content | Should -Match 'Restart'
    }

    It 'overwrites existing file' {
        $output = Join-Path $TestDrive 'INSTALL.md'
        'old content' | Set-Content $output

        & $script:ScriptPath -OutputPath $output

        $content = Get-Content $output -Raw
        $content | Should -Not -Match 'old content'
        $content | Should -Match 'Installation Instructions'
    }
}
