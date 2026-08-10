[CmdletBinding()]
param(
    [string]$Version = "1.1.1"
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$bepInExZip = Join-Path $root "BepInEx.zip"
$plugin = Join-Path $root "src\FirewatchHighFpsFix\bin\Release\FirewatchHighFpsFix.dll"
$readme = Join-Path $root "packaging\INSTALL.txt"
$dist = Join-Path $root "dist"
$outputZip = Join-Path $dist "FirewatchEnhanced-$Version-GOG-win-x64.zip"
$staging = Join-Path ([IO.Path]::GetTempPath()) ("FirewatchEnhanced-" + [Guid]::NewGuid().ToString("N"))

foreach ($file in @($bepInExZip, $plugin, $readme)) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Required file was not found: $file"
    }
}

try {
    Expand-Archive -LiteralPath $bepInExZip -DestinationPath $staging

    $bepInExChangelog = Join-Path $staging "changelog.txt"
    if (Test-Path -LiteralPath $bepInExChangelog) {
        Remove-Item -LiteralPath $bepInExChangelog -Force
    }

    New-Item -ItemType Directory -Path "$staging\BepInEx\plugins" -Force | Out-Null
    Copy-Item -LiteralPath $plugin -Destination "$staging\BepInEx\plugins\FirewatchHighFpsFix.dll"
    Copy-Item -LiteralPath $readme -Destination "$staging\FIREWATCH_ENHANCED_README.txt"

    New-Item -ItemType Directory -Path $dist -Force | Out-Null
    if (Test-Path -LiteralPath $outputZip) {
        Remove-Item -LiteralPath $outputZip -Force
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::CreateFromDirectory($staging, $outputZip)

    Write-Host "Package created: $outputZip"
}
finally {
    if (Test-Path -LiteralPath $staging) {
        Remove-Item -LiteralPath $staging -Recurse -Force
    }
}
