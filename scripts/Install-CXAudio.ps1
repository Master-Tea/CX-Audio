[CmdletBinding()]
param(
    [string]$PackagePath = '',
    [string]$CertificatePath = (Join-Path $PSScriptRoot 'MasterTea-CXAudio.cer')
)

$ErrorActionPreference = 'Stop'
$expectedThumbprint = '4C800525ACB48750E0B613F0A1EA4AD3D719A476'
if ([string]::IsNullOrWhiteSpace($PackagePath)) {
    $packages = @(Get-ChildItem -LiteralPath $PSScriptRoot -File -Filter 'CXAudioOutput_*_x64.msix')
    if ($packages.Count -ne 1) { throw 'Pass -PackagePath when the folder does not contain exactly one CX Audio MSIX.' }
    $PackagePath = $packages[0].FullName
}
$PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
$CertificatePath = (Resolve-Path -LiteralPath $CertificatePath).Path
$certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($CertificatePath)
if ($certificate.Thumbprint -ne $expectedThumbprint) { throw 'The public certificate is not the established CX Audio signing identity.' }
$signature = Get-AuthenticodeSignature -LiteralPath $PackagePath
if ($null -eq $signature.SignerCertificate -or $signature.SignerCertificate.Thumbprint -ne $expectedThumbprint) {
    throw 'The MSIX was not signed by the established CX Audio signing identity.'
}
Import-Certificate -FilePath $CertificatePath -CertStoreLocation 'Cert:\CurrentUser\Root' | Out-Null
Add-AppxPackage -Path $PackagePath
Write-Host 'CX Audio installed. Open Virtual Speaker (Advanced) in the sidebar to install the optional VB-CABLE driver.'
