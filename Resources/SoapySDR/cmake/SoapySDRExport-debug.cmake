#----------------------------------------------------------------
# Generated CMake target import file for configuration "Debug".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "SoapySDR" for configuration "Debug"
set_property(TARGET SoapySDR APPEND PROPERTY IMPORTED_CONFIGURATIONS DEBUG)
set_target_properties(SoapySDR PROPERTIES
  IMPORTED_IMPLIB_DEBUG "${_IMPORT_PREFIX}/lib/SoapySDR.lib"
  IMPORTED_LOCATION_DEBUG "${_IMPORT_PREFIX}/bin/SoapySDR.dll"
  )

list(APPEND _cmake_import_check_targets SoapySDR )
list(APPEND _cmake_import_check_files_for_SoapySDR "${_IMPORT_PREFIX}/lib/SoapySDR.lib" "${_IMPORT_PREFIX}/bin/SoapySDR.dll" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
