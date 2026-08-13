[CmdletBinding()]
param(
    [string]$Version = "1.2.1"
)

$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$bepInExZip = Join-Path $root "BepInEx-linux-x64.zip"
$plugin = Join-Path $root "src\FirewatchHighFpsFix\bin\Release\FirewatchHighFpsFix.dll"
$readme = Join-Path $root "packaging\INSTALL-LINUX.txt"
$dist = Join-Path $root "dist"
$outputZip = Join-Path $dist "FirewatchEnhanced-$Version-linux-x64.zip"
$staging = Join-Path ([IO.Path]::GetTempPath()) ("FirewatchEnhanced-linux-" + [Guid]::NewGuid().ToString("N"))
$packageRoot = Join-Path $staging "package"

foreach ($file in @($bepInExZip, $plugin, $readme)) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Required file was not found: $file"
    }
}

try {
    Expand-Archive -LiteralPath $bepInExZip -DestinationPath $packageRoot

    $bepInExChangelog = Join-Path $packageRoot "changelog.txt"
    if (Test-Path -LiteralPath $bepInExChangelog) {
        Remove-Item -LiteralPath $bepInExChangelog -Force
    }

    $runScript = Join-Path $packageRoot "run_bepinex.sh"
    $scriptText = [IO.File]::ReadAllText($runScript)
    $scriptText = $scriptText.Replace('executable_name=""', 'executable_name="fw.x86_64"')
    $scriptText = $scriptText.Replace("`r`n", "`n")
    [IO.File]::WriteAllText($runScript, $scriptText, (New-Object Text.UTF8Encoding($false)))

    $pluginDirectory = Join-Path $packageRoot "BepInEx\plugins"
    New-Item -ItemType Directory -Path $pluginDirectory -Force | Out-Null
    Copy-Item -LiteralPath $plugin -Destination $pluginDirectory
    Copy-Item -LiteralPath $readme -Destination (Join-Path $packageRoot "FIREWATCH_ENHANCED_README.txt")

    New-Item -ItemType Directory -Path $dist -Force | Out-Null
    if (Test-Path -LiteralPath $outputZip) {
        Remove-Item -LiteralPath $outputZip -Force
    }

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    # ZIP entry names must use forward slashes on Unix. CreateFromDirectory
    # uses Windows separators when this script runs on Windows.
    $archive = [IO.Compression.ZipFile]::Open(
        $outputZip,
        [IO.Compression.ZipArchiveMode]::Create)
    try {
        Get-ChildItem -LiteralPath $packageRoot -Recurse -File | ForEach-Object {
            $relativePath = $_.FullName.Substring($packageRoot.Length + 1).Replace("\", "/")
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive,
                $_.FullName,
                $relativePath,
                [IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    }
    finally {
        $archive.Dispose()
    }

    # Preserve the executable bit for the Linux launch script. The chmod step
    # in the readme remains as a fallback for archive tools that ignore it.
    $archive = [IO.Compression.ZipFile]::Open($outputZip, [IO.Compression.ZipArchiveMode]::Update)
    try {
        $entry = $archive.GetEntry("run_bepinex.sh")
        if ($null -ne $entry) {
            # 0100755 shifted into the upper 16 bits, represented as Int32.
            $entry.ExternalAttributes = -2115174400
        }
    }
    finally {
        $archive.Dispose()
    }

    Write-Host "Package created: $outputZip"
}
finally {
    if (Test-Path -LiteralPath $staging) {
        Remove-Item -LiteralPath $staging -Recurse -Force
    }
}
