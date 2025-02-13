param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [Parameter()]
    [string]$Version = '0.0.1'
)

# Output Release to the PSFlurl directory, otherwise output to a temporary directory
if ($Configuration -eq 'Release') {
    $outputPath = Join-Path $PSScriptRoot 'PSFlurl'
}
else {
    $tempPath = [System.IO.Path]::GetTempPath()
    $tempFolder = [System.IO.Path]::GetRandomFileName()
    $outputPath = Join-Path $tempPath $tempFolder 'PSFlurl'
    $null = New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
}

# Ensure outputPath directory exists
if (-not (Test-Path $outputPath)) {
    $null = New-Item -ItemType Directory -Path $outputPath -Force
}

if ($Configuration -eq 'Release') {
    # Update the module manifest version
    $manifestPath = Join-Path $PSScriptRoot 'PSFlurl.psd1'
    $rawManifest = Get-Content -Path $manifestPath -Raw
    $versionedManifest = $rawManifest -replace 'ModuleVersion[^\n\r]*', "ModuleVersion = '$Version'"
    Set-Content -Path $manifestPath -Value $versionedManifest -Encoding utf8
    # Package and update the assembly and nuspec version
    dotnet pack -c $Configuration -p:Version=$Version --output $outputPath
}
else {
    dotnet build -c $Configuration
    $assemblyPath = Join-Path $outputpath 'netstandard2.0'
    # Ensure assembly directory exists
    if (-not (Test-Path $assemblyPath)) {
        $null = New-Item -ItemType Directory -Path $assemblyPath -Force
    }
    Copy-Item -Path "$PSScriptRoot\src\bin\$Configuration\netstandard2.0\*Flurl.dll" -Destination $assemblyPath
    Copy-Item -Path 'PSFlurl.psd1' -Destination $outputPath
    Copy-Item -Path 'PSFlurl.Types.ps1xml' -Destination $outputPath
    Import-Module (Join-Path $outputPath 'PSFlurl.psd1') -Force -Verbose
}
Write-Host "Build completed. Package available in $outputPath"
