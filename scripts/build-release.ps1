[CmdletBinding()]
param(
    [string]$Version = '1.4.2',
    [switch]$SkipMsix
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $repoRoot 'CX Audio\CX Audio.csproj'
[xml]$projectXml = Get-Content -LiteralPath $project -Raw -Encoding UTF8
if ($projectXml.Project.PropertyGroup.Version -ne $Version) { throw 'Release version must match the project version.' }
$releaseRoot = Join-Path $repoRoot "dist\release\v$Version"
$publishRoot = Join-Path $releaseRoot 'app'
New-Item -ItemType Directory -Path $publishRoot -Force | Out-Null

& dotnet publish $project -c Release -p:Platform=x64 -r win-x64 --self-contained true `
    -p:NuGetAudit=false -p:DebugType=None -p:DebugSymbols=false --no-restore -o ($publishRoot + '\')
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }
if (-not (Test-Path -LiteralPath (Join-Path $publishRoot 'VirtualSpeakerPage.xbf'))) {
    throw 'The published virtual speaker page resource is missing.'
}
foreach ($document in @('README.md', 'LICENSE', 'THIRD-PARTY-NOTICES.md')) {
    Copy-Item -LiteralPath (Join-Path $repoRoot $document) -Destination $publishRoot -Force
}
$zip = Join-Path $releaseRoot "CXAudioOutput-$Version-win-x64.zip"
Compress-Archive -Path (Join-Path $publishRoot '*') -DestinationPath $zip -CompressionLevel Optimal -Force
$deliverables = @($zip)
if (-not $SkipMsix) {
    [xml]$packageXml = Get-Content -LiteralPath (Join-Path $repoRoot 'CX Audio\Package.appxmanifest') -Raw -Encoding UTF8
    $packageVersion = [string]$packageXml.Package.Identity.Version
    & (Join-Path $PSScriptRoot 'build-msix.ps1') -Version $packageVersion
    if ($LASTEXITCODE -ne 0) { throw 'MSIX build did not complete.' }
    foreach ($name in @("CXAudioOutput_${packageVersion}_x64.msix", 'MasterTea-CXAudio.cer')) {
        $destination = Join-Path $releaseRoot $name
        Copy-Item -LiteralPath (Join-Path $repoRoot "dist\msix\$name") -Destination $destination -Force
        $deliverables += $destination
    }
    $install = Join-Path $releaseRoot 'Install-CXAudio.ps1'
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Install-CXAudio.ps1') -Destination $install -Force
    $deliverables += $install
}
$notes = Join-Path $releaseRoot 'RELEASE-NOTES.md'
Copy-Item -LiteralPath (Join-Path $repoRoot "docs\releases\v$Version.md") -Destination $notes -Force
$deliverables += $notes
$checksums = $deliverables | ForEach-Object {
    $hash = Get-FileHash -LiteralPath $_ -Algorithm SHA256
    '{0}  {1}' -f $hash.Hash.ToLowerInvariant(), (Split-Path -Leaf $_)
}
$checksums | Set-Content -LiteralPath (Join-Path $releaseRoot 'SHA256SUMS.txt') -Encoding ascii
Write-Host "Release files: $releaseRoot"
$deliverables | ForEach-Object { Write-Host $_ }
