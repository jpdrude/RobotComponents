BeforeAll {
    $script:ScriptPath = Join-Path $PSScriptRoot '..' 'scripts' 'Validate-Version.ps1'
}

Describe 'Validate-Version' {

    It 'passes when tag matches code version' {
        $file = Join-Path $TestDrive 'VersionNumbering.cs'
        @'
public static class VersionNumbering
{
    public const string CurrentVersion = "3.2.1";
}
'@ | Set-Content $file

        { & $script:ScriptPath -Tag 'v3.2.1' -VersionFilePath $file } | Should -Not -Throw
    }

    It 'passes when tag has no v prefix' {
        $file = Join-Path $TestDrive 'VersionNumbering.cs'
        @'
public static class VersionNumbering
{
    public const string CurrentVersion = "3.2.1";
}
'@ | Set-Content $file

        { & $script:ScriptPath -Tag '3.2.1' -VersionFilePath $file } | Should -Not -Throw
    }

    It 'throws when versions do not match' {
        $file = Join-Path $TestDrive 'VersionNumbering.cs'
        @'
public static class VersionNumbering
{
    public const string CurrentVersion = "3.2.1";
}
'@ | Set-Content $file

        { & $script:ScriptPath -Tag 'v3.3.0' -VersionFilePath $file } | Should -Throw '*does not match*'
    }

    It 'throws when file has no CurrentVersion' {
        $file = Join-Path $TestDrive 'VersionNumbering.cs'
        @'
public static class VersionNumbering
{
    public const string SomeOtherField = "hello";
}
'@ | Set-Content $file

        { & $script:ScriptPath -Tag 'v3.2.1' -VersionFilePath $file } | Should -Throw '*Could not parse*'
    }

    It 'throws when file does not exist' {
        $file = Join-Path $TestDrive 'NonExistent.cs'

        { & $script:ScriptPath -Tag 'v3.2.1' -VersionFilePath $file } | Should -Throw
    }
}
