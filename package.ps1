#!/usr/bin/pwsh

$packageOutputFolder = "$PSScriptRoot\build-artifacts"
New-Item -ItemType directory -Force $packageOutputFolder | Out-Null

$solution = "./StatiqMermaid.sln"

Write-Host "Environment:" -ForegroundColor Cyan
Write-Host "  .NET Version:" (dotnet --version)
Write-Host "  Artifact Path: $packageOutputFolder"

Write-Host "Building solution..." -ForegroundColor "Magenta"
dotnet build $solution `
    --configuration Release `
    --verbosity quiet `

if ($LastExitCode -ne 0)
{
    Write-Host "Build failed, aborting!" -Foreground "Red"
    Exit 1
}
Write-Host "Solution built!" -ForegroundColor "Green"

$dirty = $(git status --porcelain) -ne $null
if ($dirty)
{
    Write-Host "Working directory is dirty, aborting packaging!" -Foreground "Red"
    Exit 1
}

Write-Host "Packing..." -ForegroundColor "Magenta"

Write-Host "Clearing existing $packageOutputFolder... " -NoNewline
Get-ChildItem $packageOutputFolder | Remove-Item
Write-Host "Packages cleared!" -ForegroundColor "Green"

$branch = $(git rev-parse --abbrev-ref HEAD)
$commit = $(git rev-parse HEAD)

dotnet pack ./StatiqMermaid/StatiqMermaid.csproj `
    --no-build `
    --nologo `
    --configuration Release `
    /p:RepositoryBranch=$branch `
    /p:RepositoryCommit=$commit `
    /p:PackageOutputPath=$packageOutputFolder

Write-Host "Packing complete!" -ForegroundColor "Green"
