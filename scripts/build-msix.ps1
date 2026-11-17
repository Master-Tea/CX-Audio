[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$Platform = 'x64',
    [string]$Version = '1.4.2.2',
    [string]$CertificateThumbprint = '4C800525ACB48750E0B613F0A1EA4AD3D719A476',
    [string]$CertificatePath = '',
    [string]$CertificatePassword = ''
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $repoRoot 'CX Audio\CX Audio.csproj'
$manifestPath = Join-Path $repoRoot 'CX Audio\Package.appxmanifest'
$packageBuildRoot = Join-Path $repoRoot 'obj\msix-package'
$packageOutput = Join-Path $repoRoot 'dist\msix'
$packageDirectory = Join-Path $packageBuildRoot 'AppPackages'
$certificateFile = Join-Path $packageOutput 'MasterTea-CXAudio.cer'
$expectedThumbprint = ($CertificateThumbprint -replace '\s', '').ToUpperInvariant()

# Resolve the established signing identity BEFORE building. Never create a new certificate.
if ([string]::IsNullOrWhiteSpace($CertificatePath)) {
    $certificate = Get-Item -LiteralPath ("Cert:\CurrentUser\My\" + $expectedThumbprint) -ErrorAction SilentlyContinue
    if ($null -eq $certificate -or -not $certificate.HasPrivateKey) {
        throw "The original signing certificate and private key were not found in CurrentUser\My ($expectedThumbprint). Import the original PFX, or provide -CertificatePath and -CertificatePassword. No new certificate has been created."
    }
    $signArguments = @('/s', 'My', '/sha1', $expectedThumbprint)
}
else {
    if (-not (Test-Path -LiteralPath $CertificatePath -PathType Leaf)) { throw "Original certificate PFX not found: $CertificatePath" }
    $CertificatePath = (Resolve-Path -LiteralPath $CertificatePath).Path
    $certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new(
        $CertificatePath, $CertificatePassword,
        [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::EphemeralKeySet)
    if (-not $certificate.HasPrivateKey) { throw 'The provided original certificate does not contain its private key.' }
    if ($certificate.Thumbprint -ne $expectedThumbprint) { throw 'The PFX does not match the established signing certificate. Refusing to silently change the update signing identity.' }
    $signArguments = @('/f', $CertificatePath)
    if (-not [string]::IsNullOrEmpty($CertificatePassword)) { $signArguments += @('/p', $CertificatePassword) }
}

[xml]$manifest = Get-Content -LiteralPath $manifestPath -Raw
if ($certificate.Subject -ne $manifest.Package.Identity.Publisher) { throw 'The signing certificate does not match the manifest publisher.' }
if ($certificate.NotAfter -le (Get-Date)) { throw 'The original signing certificate has expired.' }
if (Test-Path -LiteralPath $certificateFile) {
    $existingPublic = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($certificateFile)
    if ($existingPublic.Thumbprint -ne $expectedThumbprint) { throw 'The existing CER has a different thumbprint. Refusing to replace it with another signing identity.' }
}
if (-not (Test-Path -LiteralPath $project)) { throw "Project file not found: $project" }

New-Item -ItemType Directory -Path $packageBuildRoot -Force | Out-Null
New-Item -ItemType Directory -Path $packageOutput -Force | Out-Null
# Unpackaged registration-free WinRT ignores package identity. Installed apps
# instead resolve WinRT classes from AppxManifest, so register the exact SDK
# components restored for this project in the package manifest as well.
$assets = Get-Content -LiteralPath (Join-Path $repoRoot 'CX Audio\obj\project.assets.json') -Raw | ConvertFrom-Json
$packageManifest = Join-Path $packageBuildRoot 'Package.appxmanifest'
$manifestNamespace = 'http://schemas.microsoft.com/appx/manifest/foundation/windows10'
$extensions = $manifest.CreateElement('Extensions', $manifestNamespace)
$registeredClasses = @{}
foreach ($library in $assets.libraries.PSObject.Properties) {
    if ($library.Name -notlike 'Microsoft.WindowsAppSDK.*/*') { continue }
    foreach ($folder in $assets.packageFolders.PSObject.Properties.Name) {
        $fragmentPath = Join-Path (Join-Path $folder $library.Value.path) 'runtimes-framework\package.appxfragment'
        if (-not (Test-Path -LiteralPath $fragmentPath)) { continue }
        [xml]$fragment = Get-Content -LiteralPath $fragmentPath -Raw -Encoding UTF8
        foreach ($extension in $fragment.Fragment.Extensions.Extension) {
            $extensions.AppendChild($manifest.ImportNode($extension, $true)) | Out-Null
            foreach ($class in $extension.InProcessServer.ActivatableClass) {
                $registeredClasses[$class.ActivatableClassId] = $true
            }
        }
        break
    }
}
foreach ($requiredClass in @('Microsoft.UI.Xaml.Application', 'Microsoft.Windows.ApplicationModel.Resources.ResourceManager')) {
    if (-not $registeredClasses.ContainsKey($requiredClass)) { throw "Missing SDK activation registration: $requiredClass" }
}
$manifest.Package.AppendChild($extensions) | Out-Null
$manifest.Save($packageManifest)
$buildArguments = @(
    'build', $project, '-c', $Configuration, "-p:Platform=$Platform",
    '-p:EnableMsixTooling=true', '-p:WindowsPackageType=MSIX',
    "-p:CXAudioPackageManifest=$packageManifest",
    '-p:GenerateAppxPackageOnBuild=true', '-p:AppxPackageSigningEnabled=false',
    "-p:AppxPackageDir=$packageDirectory/",
    '-p:DebugType=None', '-p:DebugSymbols=false', '-p:NuGetAudit=false', '--no-restore'
)
Write-Host 'Building MSIX with the original signing identity...'
& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }

$unsignedPackage = Get-ChildItem -LiteralPath $packageDirectory -Recurse -File -Filter '*.msix' |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -eq $unsignedPackage) { throw "No MSIX package was produced under $packageDirectory" }
Add-Type -AssemblyName System.IO.Compression.FileSystem
$packageArchive = [System.IO.Compression.ZipFile]::OpenRead($unsignedPackage.FullName)
try {
    $entry = $packageArchive.GetEntry('AppxManifest.xml')
    $reader = [System.IO.StreamReader]::new($entry.Open())
    try { [xml]$builtManifest = $reader.ReadToEnd() } finally { $reader.Dispose() }
    if ($builtManifest.Package.Identity.Version -ne $Version) { throw 'Built MSIX version does not match the requested release filename.' }
    foreach ($requiredClass in @('Microsoft.UI.Xaml.Application', 'Microsoft.Windows.ApplicationModel.Resources.ResourceManager')) {
        $found = $builtManifest.SelectNodes('//*[local-name()="ActivatableClass"]') | Where-Object { $_.ActivatableClassId -eq $requiredClass }
        if (-not $found) { throw "Built MSIX is missing required activation registration: $requiredClass" }
    }
}
finally { $packageArchive.Dispose() }
$msixName = "CXAudioOutput_${Version}_x64.msix"
$msixPath = Join-Path $packageOutput $msixName
Copy-Item -LiteralPath $unsignedPackage.FullName -Destination $msixPath -Force

$signTool = Get-ChildItem -LiteralPath 'C:\Program Files (x86)\Windows Kits\10\bin' -Recurse -File -Filter 'signtool.exe' |
    Where-Object { $_.Directory.Name -eq 'x64' } | Sort-Object FullName -Descending | Select-Object -First 1
if ($null -eq $signTool) { throw 'Windows SDK signtool.exe was not found.' }
& $signTool.FullName sign /fd SHA256 @signArguments $msixPath
if ($LASTEXITCODE -ne 0) { throw "signtool sign failed with exit code $LASTEXITCODE" }

# Export only the SAME public certificate if absent; keep existing CER bytes untouched.
if (-not (Test-Path -LiteralPath $certificateFile)) {
    [System.IO.File]::WriteAllBytes($certificateFile, $certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
}
$signature = Get-AuthenticodeSignature -LiteralPath $msixPath
if ($null -eq $signature.SignerCertificate -or $signature.SignerCertificate.Thumbprint -ne $expectedThumbprint) {
    throw 'The completed MSIX is not signed by the established certificate.'
}
Write-Host ("MSIX: {0}" -f $msixPath)
Write-Host ("Signing certificate: {0}" -f $signature.SignerCertificate.Thumbprint)
Write-Host ("Signature trust status on this computer: {0}" -f $signature.Status)
Write-Host ("Original public certificate: {0}" -f $certificateFile)
