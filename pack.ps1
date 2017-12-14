$ErrorActionPreference = "Stop"
$mainFolder = Resolve-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$paketExe = "$mainFolder\.paket\paket.exe"

& "$paketExe" pack "$mainFolder\nuget"