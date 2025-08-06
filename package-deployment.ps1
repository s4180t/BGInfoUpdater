# Create a deployment directory
$deployDir = ".\deployment"
New-Item -ItemType Directory -Force -Path $deployDir

# Copy the main executable and its dependencies
Copy-Item ".\bin\Release\net8.0\BGInfoUpdater.exe" -Destination $deployDir
Copy-Item ".\bin\Release\net8.0\BGInfoUpdater.dll" -Destination $deployDir
Copy-Item ".\bin\Release\net8.0\BGInfoUpdater.runtimeconfig.json" -Destination $deployDir
Copy-Item ".\bin\Release\net8.0\BGInfoUpdater.deps.json" -Destination $deployDir

# Create BGInfo placeholder directory and readme
New-Item -ItemType Directory -Force -Path "$deployDir\BGInfo"
@"
Download BGInfo from Microsoft Sysinternals:
https://learn.microsoft.com/en-us/sysinternals/downloads/bginfo

1. Extract Bginfo64.exe to this folder
2. Creating initial config.bgi (if not provided):
   - Run Bginfo64.exe manually
   - Configure your desired layout and fields
   - Include these special fields for dynamic updates:
     * <ip> - For IP address
     * <location> - For geographic location
     * <org> - For organization info
     * <lastupdated> - For timestamp
   - Click File > Save As... and save as 'config.bgi'
   - Copy config.bgi to the main application directory
"@ | Out-File -FilePath "$deployDir\BGInfo\README.txt" -Encoding UTF8

# Create bginfo_temp directory
New-Item -ItemType Directory -Force -Path "$deployDir\bginfo_temp"

# Copy the autostart setup script
Copy-Item ".\setup-autostart.ps1" -Destination $deployDir

# Create a README file with instructions
$readmeContent = @"
BGInfo Console Application
========================

Prerequisites:
- .NET 8.0 Runtime (download from https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows OS with permission to modify desktop background

Installation:
1. Install .NET 8.0 Runtime if not already installed
2. Copy this entire folder to any location on the target PC
3. Set up BGInfo configuration:
   a. If config.bgi is missing, create it:
      - Run Bginfo64.exe from the BGInfo folder
      - Configure your desired layout and fields
      - Add these custom fields (File > New Field for each):
        * Name: ip          File: bginfo_temp\ip.txt
        * Name: location    File: bginfo_temp\location.txt
        * Name: org         File: bginfo_temp\org.txt
        * Name: lastupdated File: bginfo_temp\lastupdated.txt
      - Click File > Save As... and save as 'config.bgi'
      - Move config.bgi to the main application directory
4. Choose one of these options:
   
   Option A - Run manually:
   - Run BGInfoUpdater.exe directly
   
   Option B - Set up automatic startup:
   - Right-click setup-autostart.ps1 and select 'Run with PowerShell'
   - The app will now start automatically when you log in

Note: 
- The application needs internet access to fetch IP information
- It will create and update files in the bginfo_temp folder
- It updates the desktop background every 5 minutes
"@

$readmeContent | Out-File -FilePath "$deployDir\README.txt" -Encoding UTF8

Write-Host "Deployment package created in the 'deployment' folder"
Write-Host "Copy the entire deployment folder to run on another PC"
