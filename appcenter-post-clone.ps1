# This file is executed on app center

Move-Item -Force -Path $env:APPCENTER_SOURCE_DIRECTORY\NuGet.AppCenter.config -Destination $env:APPCENTER_SOURCE_DIRECTORY\NuGet.config

# Get chocolatey
Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

choco install windows-sdk-8.1 wixtoolset

PowerShell.exe -ExecutionPolicy Bypass ` -File download.ps1
