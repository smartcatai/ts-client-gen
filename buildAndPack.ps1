$ErrorActionPreference = "Stop"
$mainFolder = Resolve-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

& "$mainFolder/build.ps1"
& "$mainFolder/pack.ps1"