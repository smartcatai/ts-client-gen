$ErrorActionPreference = "Stop"
$mainFolder = Resolve-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$msbuildExe = (Get-ItemProperty "HKLM:\software\Microsoft\MSBuild\ToolsVersions\14.0").MSBuildToolsPath + "msbuild.exe"

& "$mainFolder\.paket\paket.exe" restore
& "$msbuildExe" /target:"Clean;Build" /p:RestorePackages=false /p:Configuration=Release /p:Platform="Any CPU" "$mainFolder\TSClientGen.sln"