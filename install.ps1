$soapySdrRoot = "$($PWD)\SoapySDRFFTGUI\Resources\SoapySDR"

[System.Environment]::SetEnvironmentVariable('SOAPY_SDR_ROOT', "$soapySdrRoot", 'Machine')

[System.Environment]::SetEnvironmentVariable("Path", $env:Path + ";$soapySdrRoot\bin", "Machine")