$ErrorActionPreference = "Stop"

function getMSBuildPath() {
	$version = "15.0"
	$mainFolder = Resolve-Path (Split-Path -Path $script:MyInvocation.MyCommand.Path -Parent)
	$vswhere = Join-Path $mainFolder "packages\vswhere\tools\vswhere.exe"

	if (-Not (Test-Path $vswhere)) {
		throw "vswhere not found, ensure packages are restored. Expected path is: " + $vswhere
	}
	
	$msbuildpaths = & "$vswhere" -version "$version" -products * -requires Microsoft.Component.MSBuild -sort -find MSBuild\**\Bin\MSBuild.exe
	
	$msbuildpath = $msbuildpaths | select -First 1
	
	if ($msbuildpath -is [system.array]) {
		$msbuildpath = $msbuildpath[0]
	}
	
	if (($msbuildpath -Eq $null) -or (-Not (Test-Path $msbuildpath))) {
		throw "MSBuild not found (version $version, VS version = $studioVersion). All installs: " + $msbuildpaths
	}

	return $msbuildpath
}

$mainFolder = Resolve-Path (Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
$msbuildExe = getMSBuildPath

& "$mainFolder\.paket\paket.exe" restore
& "$msbuildExe" /target:restore "$mainFolder\TSClientGen.sln"
& "$msbuildExe" /target:"Clean;Build" /p:Configuration=Release /p:Platform="Any CPU" "$mainFolder\TSClientGen.sln"