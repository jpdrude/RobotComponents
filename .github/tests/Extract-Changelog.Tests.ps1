BeforeAll {
    $script:ScriptPath = Join-Path $PSScriptRoot '..' 'scripts' 'Extract-Changelog.ps1'
}

Describe 'Extract-Changelog' {

    It 'extracts first 5 sections from a normal changelog' {
        $changelog = Join-Path $TestDrive 'CHANGELOG.md'
        $output = Join-Path $TestDrive 'notes.md'

        # Build 8 sections separated by ---
        $sections = 1..8 | ForEach-Object { "- Change number $_`n  **Commit:** ``abc$_`` | **Date:** 2026-01-0$_" }
        ($sections -join "`n---`n") | Set-Content $changelog

        & $script:ScriptPath -Tag 'v3.2.1' -ChangelogPath $changelog -OutputPath $output

        $content = Get-Content $output -Raw
        $content | Should -Match 'Change number 1'
        $content | Should -Match 'Change number 5'
        $content | Should -Not -Match 'Change number 6'
    }

    It 'returns fallback when changelog is missing' {
        $changelog = Join-Path $TestDrive 'NonExistent.md'
        $output = Join-Path $TestDrive 'notes.md'

        & $script:ScriptPath -Tag 'v3.2.1' -ChangelogPath $changelog -OutputPath $output


        $content = Get-Content $output -Raw
        $content.Trim() | Should -Be 'Release v3.2.1'
    }

    It 'returns fallback when changelog is empty' {
        $changelog = Join-Path $TestDrive 'CHANGELOG.md'
        $output = Join-Path $TestDrive 'notes.md'
        '' | Set-Content $changelog

        & $script:ScriptPath -Tag 'v3.2.1' -ChangelogPath $changelog -OutputPath $output

        $content = Get-Content $output -Raw
        $content.Trim() | Should -Be 'Release v3.2.1'
    }

    It 'truncates content exceeding MaxLength' {
        $changelog = Join-Path $TestDrive 'CHANGELOG.md'
        $output = Join-Path $TestDrive 'notes.md'

        # Create a very long changelog (>500 chars per section, 20 sections)
        $sections = 1..20 | ForEach-Object { "- " + ("A" * 500) + " change $_" }
        ($sections -join "`n---`n") | Set-Content $changelog

        & $script:ScriptPath -Tag 'v3.2.1' -ChangelogPath $changelog -OutputPath $output -MaxLength 200

        $content = Get-Content $output -Raw
        $content | Should -Match 'see CHANGELOG\.md for full details'
    }

    It 'takes fewer sections when file has less than MaxSections' {
        $changelog = Join-Path $TestDrive 'CHANGELOG.md'
        $output = Join-Path $TestDrive 'notes.md'

        $sections = 1..3 | ForEach-Object { "- Change number $_" }
        ($sections -join "`n---`n") | Set-Content $changelog

        & $script:ScriptPath -Tag 'v1.0.0' -ChangelogPath $changelog -OutputPath $output -MaxSections 5

        $content = Get-Content $output -Raw
        $content | Should -Match 'Change number 1'
        $content | Should -Match 'Change number 3'
    }

    It 'respects custom MaxSections parameter' {
        $changelog = Join-Path $TestDrive 'CHANGELOG.md'
        $output = Join-Path $TestDrive 'notes.md'

        $sections = 1..8 | ForEach-Object { "- Change number $_" }
        ($sections -join "`n---`n") | Set-Content $changelog

        & $script:ScriptPath -Tag 'v1.0.0' -ChangelogPath $changelog -OutputPath $output -MaxSections 2

        $content = Get-Content $output -Raw
        $content | Should -Match 'Change number 1'
        $content | Should -Match 'Change number 2'
        $content | Should -Not -Match 'Change number 3'
    }
}
