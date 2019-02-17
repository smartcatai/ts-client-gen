$ErrorActionPreference = "Stop"

function getMSBuildPath()
{
	$version = "15.0"
	$mainFolder = Resolve-Path (Split-Path -Path $script:MyInvocation.MyCommand.Path -Parent)
	$vswhere = Join-Path $mainFolder "packages\vswhere\tools\vswhere.exe"

	if (-Not (Test-Path $vswhere)) {
		throw "vswhere not found, ensure packages are restored. Expected path is: " + $vswhere
	}

	$vspath = & "$vswhere" -version "$version" -products * -requires Microsoft.Component.MSBuild -property installationPath
	if ($vspath -Eq $null) {
		throw "VS or VS Build Tools $version was not found"
	}

	$msbuildpath = Join-Path $vspath "MSBuild\$version\Bin\MSBuild.exe"
	if (-Not (Test-Path $msbuildpath)) {
		throw "MSBuild not found, expected path: " + $msbuildpath
	}

	return $msbuildpath
}

$mainFolder = Resolve-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$msbuildExe = getMSBuildPath

& "$mainFolder\.paket\paket.exe" restore
& "$msbuildExe" /target:"Clean;Build" /p:RestorePackages=false /p:Configuration=Release /p:Platform="Any CPU" "$mainFolder\TSClientGen.sln"