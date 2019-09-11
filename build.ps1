$ErrorActionPreference = "Stop"
$mainFolder = Resolve-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)

&dotnet build -c Release "$mainFolder\TSClientGen.sln"