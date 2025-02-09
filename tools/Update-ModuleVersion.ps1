[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$Version,
    
    [Parameter(Mandatory)]
    [string]$Path
)

$ErrorActionPreference = 'Stop'
Write-Host "Processing version: $Version"
if($Version -match '(\d+\.\d+\.\d+)(-[a-zA-Z0-9\.]+)?') {
    $moduleVersion = $matches[1]
    Write-Host "Module version: $moduleVersion"
    $prerelease = if($matches[2]){($matches[2]) -replace '[.-]'}else{$null}
    Write-Host "Prerelease: $prerelease"
    
    Update-ModuleManifest -Path $Path -ModuleVersion $moduleVersion
    if($prerelease) {
        Update-ModuleManifest -Path $Path -Prerelease $prerelease
    }
} else {
    Write-Error "Invalid version format: $Version"
    exit 1
}