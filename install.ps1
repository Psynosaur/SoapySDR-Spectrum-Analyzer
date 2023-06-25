$soapySdrRoot = "$($PWD)\Resources\SoapySDR"

[System.Environment]::SetEnvironmentVariable('SOAPY_SDR_ROOT', "$soapySdrRoot", 'Machine')
Write-Output "Set SOAPY_SDR_ROOT environment variable to : $soapySdrRoot"

[System.Environment]::SetEnvironmentVariable("Path", $env:Path + ";$soapySdrRoot\bin", "Machine")
Write-Output "Set $soapySdrRoot\bin in PATH"

Write-Output "Restart Powershell and use soapySDRUtil --info"