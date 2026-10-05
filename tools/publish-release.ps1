[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^v(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)$')]
    [string]$Tag,

    [switch]$PrepareOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-Checked {
    param(
        [string]$Command,
        [string[]]$Arguments
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao executar: $Command $($Arguments -join ' ')"
    }
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Set-Location $repositoryRoot

foreach ($command in @('dotnet', 'git')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "Comando obrigatório não encontrado: $command"
    }
}

if (-not $PrepareOnly -and -not (Get-Command gh -ErrorAction SilentlyContinue)) {
    throw 'GitHub CLI (gh) não encontrado.'
}

$version = $Tag.Substring(1)
[xml]$project = Get-Content 'CloseAppsOpen/CloseAppsOpen.csproj'
if ($project.Project.PropertyGroup.Version -ne $version) {
    throw "A versão do projeto ($($project.Project.PropertyGroup.Version)) deve corresponder à tag ($Tag)."
}

if (-not $PrepareOnly) {
    $pendingChanges = & git status --porcelain
    if ($LASTEXITCODE -ne 0 -or $pendingChanges) {
        throw 'Faça commit de todas as alterações antes de publicar a release.'
    }

    Invoke-Checked gh @('auth', 'status')

    $branch = ((& git branch --show-current) | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $branch) {
        throw 'A publicação exige uma branch local, não um HEAD destacado.'
    }

    $defaultBranch = ((& gh repo view --json defaultBranchRef --jq '.defaultBranchRef.name') | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $defaultBranch) {
        throw 'Não foi possível identificar a branch padrão do repositório.'
    }

    if ($branch -ne $defaultBranch) {
        throw "Publique somente a partir da branch padrão ($defaultBranch). Branch atual: $branch."
    }

    $headCommit = ((& git rev-parse HEAD) | Out-String).Trim()
    $remoteLine = ((& git ls-remote origin "refs/heads/$branch") | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $remoteLine) {
        throw "Não foi possível encontrar a branch $branch no origin. Envie os commits antes de publicar."
    }

    $remoteCommit = ($remoteLine -split '\s+')[0]
    if ($headCommit -ne $remoteCommit) {
        throw "A branch local $branch deve estar sincronizada com origin/$branch antes da publicação."
    }

    $existingTagCommit = ''
    $localTag = ((& git tag --list $Tag) | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Não foi possível verificar a tag local $Tag."
    }

    if ($localTag) {
        $existingTagCommit = ((& git rev-list -n 1 "refs/tags/$Tag") | Out-String).Trim()
        if ($LASTEXITCODE -ne 0 -or $existingTagCommit -ne $headCommit) {
            throw "A tag local $Tag já aponta para outro commit."
        }
    }
}

$artifactDirectory = Join-Path $repositoryRoot ".artifacts/releases/$Tag"
$publishDirectory = Join-Path $artifactDirectory 'publish'
New-Item -ItemType Directory -Path $publishDirectory -Force | Out-Null

Invoke-Checked dotnet @('test', 'CloseAppsOpen.slnx', '-c', 'Release')
Invoke-Checked dotnet @(
    'publish', 'CloseAppsOpen/CloseAppsOpen.csproj',
    '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true',
    '-p:PublishSingleFile=true', "-p:Version=$version", '-o', $publishDirectory
)

$executable = Join-Path $publishDirectory 'CloseAppsOpen.exe'
if (-not (Test-Path $executable)) {
    throw "Executável não encontrado: $executable"
}

$reportedVersion = ((& $executable --version) | Out-String).Trim()
if ($LASTEXITCODE -ne 0 -or $reportedVersion -ne "CloseAppsOpen $Tag") {
    throw "A versão do executável ($reportedVersion) não corresponde à tag $Tag."
}

$archive = Join-Path $artifactDirectory "CloseAppsOpen-$Tag-win-x64.zip"
$checksumFile = Join-Path $artifactDirectory 'SHA256SUMS.txt'
Compress-Archive -LiteralPath $executable -DestinationPath $archive -Force
$checksum = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content -LiteralPath $checksumFile -Value "$checksum  $(Split-Path $archive -Leaf)" -Encoding ascii

Write-Host "Artefatos gerados: $archive e $checksumFile"
if ($PrepareOnly) {
    return
}

if (-not $existingTagCommit) {
    Invoke-Checked git @('tag', '-a', $Tag, '-m', "Release $Tag")
}

Invoke-Checked git @('push', 'origin', "refs/tags/$Tag")
Invoke-Checked gh @(
    'release', 'create', $Tag, $archive, $checksumFile,
    '--verify-tag', '--title', $Tag, '--generate-notes'
)

Invoke-Checked gh @('release', 'view', $Tag, '--json', 'url', '--jq', '.url')
