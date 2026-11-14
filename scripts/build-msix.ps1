[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Platform = 'x64',
    [string]$Version = '1.3.0.0',
    [string]$CertificatePath = '',
    [string]$CertificatePassword = 'CXAudio-MSIX-Local'
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $repoRoot 'CX Audio\CX Audio.csproj'
$packageBuildRoot = Join-Path $repoRoot 'obj\msix-package'
$packageOutput = Join-Path $repoRoot 'dist\msix'
$packageDirectory = Join-Path $packageBuildRoot 'AppPackages'

if (-not (Test-Path -LiteralPath $project)) {
    throw "Project file not found: $project"
}

New-Item -ItemType Directory -Path $packageBuildRoot -Force | Out-Null
New-Item -ItemType Directory -Path $packageOutput -Force | Out-Null

$buildArguments = @(
    'build', $project,
    '-c', $Configuration,
    "-p:Platform=$Platform",
    '-p:EnableMsixTooling=true',
    '-p:WindowsPackageType=MSIX',
    '-p:GenerateAppxPackageOnBuild=true',
    '-p:AppxPackageSigningEnabled=false',
    "-p:AppxPackageDir=$packageDirectory/",
    '-p:DebugType=None',
    '-p:DebugSymbols=false',
    '-p:NuGetAudit=false',
    '--no-restore'
)

Write-Host 'Building unsigned MSIX package...'
& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE"
}

$unsignedPackage = Get-ChildItem -LiteralPath $packageDirectory -Recurse -File -Filter '*.msix' |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if ($null -eq $unsignedPackage) {
    throw "No MSIX package was produced under $packageDirectory"
}

$msixName = "CXAudioOutput_${Version}_x64.msix"
$msixPath = Join-Path $packageOutput $msixName
Copy-Item -LiteralPath $unsignedPackage.FullName -Destination $msixPath -Force

$signTool = Get-ChildItem -LiteralPath 'C:\Program Files (x86)\Windows Kits\10\bin' -Recurse -File -Filter 'signtool.exe' |
    Where-Object { $_.FullName -match '\\x64\\signtool\.exe$' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1
if ($null -eq $signTool) {
    throw 'Windows SDK signtool.exe was not found.'
}

$temporaryPfx = $false
$pfxPath = $CertificatePath
$certificateFile = Join-Path $packageOutput 'MasterTea-CXAudio.cer'

if ([string]::IsNullOrWhiteSpace($pfxPath)) {
    $temporaryPfx = $true
    $pfxPath = Join-Path ([System.IO.Path]::GetTempPath()) ("CXAudio-{0}.pfx" -f [guid]::NewGuid())
    $securePassword = ConvertTo-SecureString $CertificatePassword -AsPlainText -Force
    $certificate = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject 'CN=MasterTea' `
        -FriendlyName 'MasterTea CX Audio MSIX' `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -HashAlgorithm SHA256 `
        -NotAfter (Get-Date).AddYears(5)
    Export-PfxCertificate -Cert $certificate -FilePath $pfxPath -Password $securePassword | Out-Null
    Export-Certificate -Cert $certificate -FilePath $certificateFile -Type CERT | Out-Null
}
elseif (-not (Test-Path -LiteralPath $pfxPath)) {
    throw "Certificate PFX not found: $pfxPath"
}

try {
    Write-Host "Signing $msixName..."
    & $signTool.FullName sign /fd SHA256 /f $pfxPath /p $CertificatePassword $msixPath
    if ($LASTEXITCODE -ne 0) {
        throw "signtool sign failed with exit code $LASTEXITCODE"
    }
}
finally {
    if ($temporaryPfx -and (Test-Path -LiteralPath $pfxPath)) {
        Remove-Item -LiteralPath $pfxPath -Force
    }
}

$signature = Get-AuthenticodeSignature -LiteralPath $msixPath
Write-Host ("MSIX: {0}" -f $msixPath)
Write-Host ("Signature status: {0}" -f $signature.Status)
Write-Host ("Public certificate: {0}" -f $certificateFile)
