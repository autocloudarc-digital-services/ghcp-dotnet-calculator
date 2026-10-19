#!/usr/bin/env pwsh
# Copyright (c) Microsoft Corporation.
# SPDX-License-Identifier: MIT
#Requires -Version 7.0

<#
.SYNOPSIS
    Sets up the .NET 8 calculator solution and xUnit test project.
.DESCRIPTION
    Creates or verifies the calculator-xunit-testing workspace required by prompt
    1.12.1. The script scaffolds the console and test projects, pins compatible
    xUnit package versions, adds project references, applies starter templates
    when generated source files are missing, and validates build/test discovery.
.PARAMETER RepoRoot
    Repository root. Defaults to git rev-parse --show-toplevel.
.PARAMETER WorkspaceRelativePath
    Relative workspace path where calculator-xunit-testing is created.
.PARAMETER SolutionName
    Solution name used for the .NET solution file.
.EXAMPLE
    ./Set-DotnetCalculatorSolution.ps1
.EXAMPLE
    ./Set-DotnetCalculatorSolution.ps1 -WorkspaceRelativePath src/workspace
.NOTES
    This script is owned by the calculator-setup skill and is safe to rerun.
#>
[CmdletBinding()]
param
(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot,

    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$WorkspaceRelativePath = 'src/workspace',

    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$SolutionName = 'calculator'
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true
Set-StrictMode -Version Latest

#region Functions
function Write-Status
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]$Message,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Info', 'Success', 'Warning', 'Error')]
        [string]$Kind = 'Info'
    )

    $Color = switch ($Kind)
    {
        'Success' { 'Green' }
        'Warning' { 'Yellow' }
        'Error' { 'Red' }
        default { 'Cyan' }
    }

    Write-Host "[$Kind] $Message" -ForegroundColor $Color
}

function Invoke-CheckedCommand
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string[]]$ArgumentList,

        [Parameter(Mandatory = $false)]
        [string]$WorkingDirectory = (Get-Location).Path
    )

    Push-Location -Path $WorkingDirectory
    try
    {
        $Output = & $FilePath @ArgumentList 2>&1
        if ($LASTEXITCODE -ne 0)
        {
            throw "$FilePath $($ArgumentList -join ' ') failed with exit code $LASTEXITCODE.`n$Output"
        }

        return $Output
    }
    finally
    {
        Pop-Location
    }
}

function Get-RepositoryRoot
{
    [CmdletBinding()]
    param()

    if (-not [string]::IsNullOrWhiteSpace($RepoRoot))
    {
        return (Resolve-Path -Path $RepoRoot -ErrorAction Stop).Path
    }

    $DetectedRoot = & git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($DetectedRoot))
    {
        throw 'Unable to determine repository root. Run from inside the repository or pass -RepoRoot.'
    }

    return $DetectedRoot.Trim()
}

function Set-ProjectProperty
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [xml]$ProjectXml,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $PropertyGroup = $ProjectXml.Project.PropertyGroup | Select-Object -First 1
    if ($null -eq $PropertyGroup)
    {
        $PropertyGroup = $ProjectXml.CreateElement('PropertyGroup')
        $ProjectXml.Project.AppendChild($PropertyGroup) | Out-Null
    }

    $Node = $PropertyGroup.SelectSingleNode($Name)
    if ($null -eq $Node)
    {
        $Node = $ProjectXml.CreateElement($Name)
        $PropertyGroup.AppendChild($Node) | Out-Null
    }

    if ($Node.InnerText -eq $Value)
    {
        return $false
    }

    $Node.InnerText = $Value
    return $true
}

function Set-PackageVersion
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [xml]$ProjectXml,

        [Parameter(Mandatory = $true)]
        [string]$PackageName,

        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $Changed = $false
    $PackageNode = $ProjectXml.SelectSingleNode("//PackageReference[@Include='$PackageName']")
    if ($null -eq $PackageNode)
    {
        $ItemGroup = $ProjectXml.Project.ItemGroup | Select-Object -First 1
        if ($null -eq $ItemGroup)
        {
            $ItemGroup = $ProjectXml.CreateElement('ItemGroup')
            $ProjectXml.Project.AppendChild($ItemGroup) | Out-Null
        }

        $PackageNode = $ProjectXml.CreateElement('PackageReference')
        $PackageNode.SetAttribute('Include', $PackageName)
        $ItemGroup.AppendChild($PackageNode) | Out-Null
        $Changed = $true
    }

    if ($PackageNode.GetAttribute('Version') -eq $Version)
    {
        return $Changed
    }

    $PackageNode.SetAttribute('Version', $Version)
    return $true
}

function Save-ProjectXml
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [xml]$ProjectXml,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $Settings = [System.Xml.XmlWriterSettings]::new()
    $Settings.Indent = $true
    $Settings.OmitXmlDeclaration = $true

    $Writer = [System.Xml.XmlWriter]::Create($Path, $Settings)
    try
    {
        $ProjectXml.Save($Writer)
    }
    finally
    {
        $Writer.Dispose()
    }
}

function Copy-TemplateIfMissing
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]$TemplatePath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationPath
    )

    if (Test-Path -Path $DestinationPath -PathType Leaf)
    {
        Write-Status "Source file already exists: $DestinationPath" 'Info'
        return
    }

    Copy-Item -Path $TemplatePath -Destination $DestinationPath -Force
    Write-Status "Applied template: $DestinationPath" 'Success'
}

function Add-ProjectReferenceIfMissing
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]$TestProjectFile,

        [Parameter(Mandatory = $true)]
        [string]$ConsoleProjectFile,

        [Parameter(Mandatory = $true)]
        [string]$SolutionDir
    )

    [xml]$TestProjectXml = Get-Content -Path $TestProjectFile -Raw
    $ExistingReference = $TestProjectXml.SelectSingleNode("//ProjectReference[@Include='..\calculator\calculator.csproj']")
    if ($null -ne $ExistingReference)
    {
        Write-Status 'Project reference already exists: calculator.tests -> calculator' 'Info'
        return
    }

    Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('add', $TestProjectFile, 'reference', $ConsoleProjectFile) -WorkingDirectory $SolutionDir | Out-Null
    Write-Status 'Project reference ready: calculator.tests -> calculator' 'Success'
}

function Test-SolutionContainsProject
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]$SolutionFile,

        [Parameter(Mandatory = $true)]
        [string]$ProjectPath
    )

    if ($SolutionFile.EndsWith('.slnx', [System.StringComparison]::OrdinalIgnoreCase))
    {
        [xml]$SolutionXml = Get-Content -Path $SolutionFile -Raw
        return $null -ne $SolutionXml.SelectSingleNode("//Project[@Path='$ProjectPath']")
    }

    $SolutionContent = Get-Content -Path $SolutionFile -Raw
    return $SolutionContent -match [regex]::Escape($ProjectPath.Replace('/', '\'))
}

function Add-ProjectToSolution
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory = $true)]
        [string]$SolutionFile,

        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$SolutionDir
    )

    if (Test-SolutionContainsProject -SolutionFile $SolutionFile -ProjectPath $ProjectPath)
    {
        Write-Status "Solution already includes $ProjectPath" 'Info'
        return
    }

    Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('sln', $SolutionFile, 'add', $ProjectPath) -WorkingDirectory $SolutionDir | Out-Null
    Write-Status "Added $ProjectPath to solution" 'Success'
}
#endregion Functions

#region Main Execution
if ($MyInvocation.InvocationName -ne '.')
{
    try
    {
        $ResolvedRepoRoot = Get-RepositoryRoot
        $SkillRoot = Split-Path -Path $PSScriptRoot -Parent
        $TemplateDir = Join-Path -Path $SkillRoot -ChildPath 'templates'
        $WorkspacePath = Join-Path -Path $ResolvedRepoRoot -ChildPath $WorkspaceRelativePath
        $SolutionDir = Join-Path -Path $WorkspacePath -ChildPath 'calculator-xunit-testing'
        $ConsoleProjectDir = Join-Path -Path $SolutionDir -ChildPath 'calculator'
        $TestProjectDir = Join-Path -Path $SolutionDir -ChildPath 'calculator.tests'
        $ConsoleProjectFile = Join-Path -Path $ConsoleProjectDir -ChildPath 'calculator.csproj'
        $TestProjectFile = Join-Path -Path $TestProjectDir -ChildPath 'calculator.tests.csproj'

        foreach ($ProjectFile in @($ConsoleProjectFile, $TestProjectFile))
        {
            if (Test-Path -Path $ProjectFile)
            {
                [xml]$ExistingProject = Get-Content -Path $ProjectFile -Raw
                $TargetFramework = $ExistingProject.SelectSingleNode('//TargetFramework')
                if ($null -ne $TargetFramework -and $TargetFramework.InnerText -ne 'net8.0')
                {
                    throw "Initial setup requires net8.0, but $ProjectFile targets $($TargetFramework.InnerText). Preserve this later-stage workspace; do not downgrade it by rerunning setup."
                }
            }
        }

        Write-Status "Repository root: $ResolvedRepoRoot"
        New-Item -ItemType Directory -Path $SolutionDir -Force | Out-Null
        Write-Status "Solution directory ready: $SolutionDir" 'Success'

        Copy-TemplateIfMissing -TemplatePath (Join-Path -Path $TemplateDir -ChildPath 'global.json') -DestinationPath (Join-Path -Path $SolutionDir -ChildPath 'global.json')

        $SolutionFile = Join-Path -Path $SolutionDir -ChildPath "$SolutionName.slnx"
        $LegacySolutionFile = Join-Path -Path $SolutionDir -ChildPath "$SolutionName.sln"
        if (Test-Path -Path $LegacySolutionFile)
        {
            $SolutionFile = $LegacySolutionFile
        }

        if (-not (Test-Path -Path $SolutionFile))
        {
            $SolutionArguments = @('new', 'sln', '--name', $SolutionName)
            $SdkVersion = Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('--version') -WorkingDirectory $SolutionDir
            if ([int]($SdkVersion.ToString().Trim().Split('.')[0]) -ge 9)
            {
                $SolutionArguments += @('--format', 'sln')
            }

            Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList $SolutionArguments -WorkingDirectory $SolutionDir | Out-Null
            $SolutionFile = $LegacySolutionFile
            Write-Status "Created solution file: $SolutionFile" 'Success'
        }
        else
        {
            Write-Status "Solution file already exists: $SolutionFile" 'Info'
        }

        if (-not (Test-Path -Path $ConsoleProjectFile))
        {
            Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('new', 'console', '--name', 'calculator', '--framework', 'net8.0', '--force') -WorkingDirectory $SolutionDir | Out-Null
            Write-Status "Created console project: $ConsoleProjectFile" 'Success'
        }

        if (-not (Test-Path -Path $TestProjectFile))
        {
            Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('new', 'xunit', '--name', 'calculator.tests', '--framework', 'net8.0', '--force') -WorkingDirectory $SolutionDir | Out-Null
            Write-Status "Created xUnit project: $TestProjectFile" 'Success'
        }

        [xml]$ConsoleProjectXml = Get-Content -Path $ConsoleProjectFile -Raw
        $ConsoleProjectChanged = $false
        $ConsoleProjectChanged = (Set-ProjectProperty -ProjectXml $ConsoleProjectXml -Name 'TargetFramework' -Value 'net8.0') -or $ConsoleProjectChanged
        $ConsoleProjectChanged = (Set-ProjectProperty -ProjectXml $ConsoleProjectXml -Name 'ImplicitUsings' -Value 'enable') -or $ConsoleProjectChanged
        $ConsoleProjectChanged = (Set-ProjectProperty -ProjectXml $ConsoleProjectXml -Name 'Nullable' -Value 'enable') -or $ConsoleProjectChanged
        if ($ConsoleProjectChanged)
        {
            Save-ProjectXml -ProjectXml $ConsoleProjectXml -Path $ConsoleProjectFile
        }

        [xml]$TestProjectXml = Get-Content -Path $TestProjectFile -Raw
        $TestProjectChanged = $false
        $TestProjectChanged = (Set-ProjectProperty -ProjectXml $TestProjectXml -Name 'TargetFramework' -Value 'net8.0') -or $TestProjectChanged
        $TestProjectChanged = (Set-ProjectProperty -ProjectXml $TestProjectXml -Name 'ImplicitUsings' -Value 'enable') -or $TestProjectChanged
        $TestProjectChanged = (Set-ProjectProperty -ProjectXml $TestProjectXml -Name 'Nullable' -Value 'enable') -or $TestProjectChanged
        $TestProjectChanged = (Set-ProjectProperty -ProjectXml $TestProjectXml -Name 'IsPackable' -Value 'false') -or $TestProjectChanged
        $TestProjectChanged = (Set-ProjectProperty -ProjectXml $TestProjectXml -Name 'IsTestProject' -Value 'true') -or $TestProjectChanged
        $TestProjectChanged = (Set-ProjectProperty -ProjectXml $TestProjectXml -Name 'SuppressTfmSupportBuildErrors' -Value 'true') -or $TestProjectChanged
        $TestProjectChanged = (Set-PackageVersion -ProjectXml $TestProjectXml -PackageName 'xunit' -Version '2.6.2') -or $TestProjectChanged
        $TestProjectChanged = (Set-PackageVersion -ProjectXml $TestProjectXml -PackageName 'xunit.runner.visualstudio' -Version '2.5.1') -or $TestProjectChanged
        $TestProjectChanged = (Set-PackageVersion -ProjectXml $TestProjectXml -PackageName 'Microsoft.NET.Test.Sdk' -Version '17.5.0') -or $TestProjectChanged
        $TestProjectChanged = (Set-PackageVersion -ProjectXml $TestProjectXml -PackageName 'coverlet.collector' -Version '6.0.0') -or $TestProjectChanged
        if ($TestProjectChanged)
        {
            Save-ProjectXml -ProjectXml $TestProjectXml -Path $TestProjectFile
        }

        $ProgramFile = Join-Path -Path $ConsoleProjectDir -ChildPath 'Program.cs'
        if (Test-Path -Path $ProgramFile)
        {
            Rename-Item -Path $ProgramFile -NewName 'Calculator.cs' -Force
        }

        $UnitTestFile = Join-Path -Path $TestProjectDir -ChildPath 'UnitTest1.cs'
        if (Test-Path -Path $UnitTestFile)
        {
            Rename-Item -Path $UnitTestFile -NewName 'CalculatorTest.cs' -Force
        }

        Copy-TemplateIfMissing -TemplatePath (Join-Path -Path $TemplateDir -ChildPath 'Calculator.cs') -DestinationPath (Join-Path -Path $ConsoleProjectDir -ChildPath 'Calculator.cs')
        Copy-TemplateIfMissing -TemplatePath (Join-Path -Path $TemplateDir -ChildPath 'CalculatorTest.cs') -DestinationPath (Join-Path -Path $TestProjectDir -ChildPath 'CalculatorTest.cs')

        Add-ProjectToSolution -SolutionFile $SolutionFile -ProjectPath 'calculator/calculator.csproj' -SolutionDir $SolutionDir
        Add-ProjectToSolution -SolutionFile $SolutionFile -ProjectPath 'calculator.tests/calculator.tests.csproj' -SolutionDir $SolutionDir
        Add-ProjectReferenceIfMissing -TestProjectFile $TestProjectFile -ConsoleProjectFile $ConsoleProjectFile -SolutionDir $SolutionDir

        Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('build', $SolutionFile) -WorkingDirectory $SolutionDir | Out-Null
        Write-Status 'Build verification succeeded' 'Success'

        Invoke-CheckedCommand -FilePath 'dotnet' -ArgumentList @('test', $SolutionFile, '--list-tests') -WorkingDirectory $SolutionDir | Out-Null
        Write-Status 'Test discovery verification succeeded' 'Success'
        Write-Status 'Calculator solution setup complete.' 'Success'
        exit 0
    }
    catch
    {
        Write-Status $_.Exception.Message 'Error'
        exit 1
    }
}
#endregion Main Execution
