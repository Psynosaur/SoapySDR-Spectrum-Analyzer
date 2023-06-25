# SoapySDR-FFT-GUI

SoapySDR FFT viewer(in progress)

## Supports these modules atm: 

 - Airspy
 - RTLSDR
 - PlutoSDR
 - SDRPlay 

### Program sample
![Screen](./SoapySDRFFTGUI/images/Spectrum vs Online WB.png)
### TO DO :

### Install instructions

```powershell
.\install.ps1
```
  - Install script
    - [x] Compiled dll's in release mode
    - [x] Powershell install script
      - Adds **SOAPY_SDR_ROOT** environment variable
      - Adds **{SOAPY_SDR_ROOT}\bin** folder to path 
  - Interface
    - [x] List available sample rates
    - Gain slider(s)
    - PlutoSDR
      - Network address box
      - Auto discovery?
    - [x] Connect to RX stream 
    - [x] Draw spectrum window 
      - [x] Make te spectrum unmirrored 
        - [x] transform the data correctly
          - [ ] Fix the overlapping of frequency bins
    - [ ] Sort out bugs
      - [x] RTLSDR close
      - [ ] RTLSDR params not correctly set
      - [ ] Some gui things
  - More to come . . .