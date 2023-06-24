# SoapySDR-FFT-GUI

SoapySDR FFT viewer(in progress)

## Supports these modules atm: 

 - Airspy
 - RTLSDR
 - PlutoSDR
 - SDRPlay 

### Initial Device enumeration
![Screen](./SoapySDRFFTGUI/images/initial.png)
### Query SDR device
![Screen](./SoapySDRFFTGUI/images/query.png)
### Read 15 samplesnunucsc
![Screen](./SoapySDRFFTGUI/images/sample.png)
### TO DO :

  - MSI installer
    - Adds Resources/SoapySDR to Program Files/SoapySDR
    - Adds environment variable SOAPY_SDR_ROOT
    - Optionally add to PATH..
  - Interface
    - [x] List available sample rates
    - Gain slider(s)
    - PlutoSDR
      - Network address box
      - Auto discovery?
    - [x] Connect to RX stream 
    - [x] Draw spectrum window 
      - [ ] Make te spectrum unmirrored 
        - [ ] transform the data correctly
    - [ ] Sort out bugs
      - [ ] RTLSDR close
      - [ ] RTLSDR params not correctly set
      - [ ] Some gui things
  - More to come . . .