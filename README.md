# BGInfoUpdater

A Windows application that automatically updates your desktop background with system information using Sysinternals BGInfo.

## Features

- Automatically updates system information every 5 minutes
- Displays IP address information
- Shows last update timestamp
- Includes location information
- Shows organizational details
- Runs silently in the background
- Supports automatic startup with Windows

## Prerequisites

- Windows operating system with permission to modify desktop background
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- [BGInfo from Microsoft Sysinternals](https://learn.microsoft.com/en-us/sysinternals/downloads/bginfo)

## Project Structure

### Source Code

```plaintext
├── Program.cs              # Main application source
├── BGInfoUpdater.csproj    # Project file
├── BGInfoUpdater.sln       # Solution file
├── README.md               # This documentation
└── scripts/
    └── setup-autostart.ps1 # PowerShell script for automatic startup
```

### Runtime Structure (After Installation)

```plaintext
├── BGInfoUpdater.exe       # Main application executable
├── setup-autostart.ps1     # PowerShell script for automatic startup
├── config.bgi              # BGInfo configuration file (create manually)
├── BGInfo/                 # Directory for BGInfo executables
│   └── Bginfo64.exe        # BGInfo executable (download separately)
└── bginfo_temp/            # Directory for temporary data files
    ├── ip.txt              # Current IP information
    ├── lastupdated.txt     # Timestamp of last update
    ├── location.txt        # Geographic location data
    └── org.txt             # Organization information
```

## Installation

1. **Install Prerequisites**
   - Download and install [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Download [BGInfo](https://learn.microsoft.com/en-us/sysinternals/downloads/bginfo) from Microsoft Sysinternals

2. **Initial Setup**
   - Create a directory where you want to install BGInfoUpdater
   - Copy all BGInfoUpdater files to this directory
   - Create a `BGInfo` subdirectory
   - Extract `Bginfo64.exe` from the downloaded BGInfo.zip into the `BGInfo` directory
   - Create your own `config.bgi` file (see Configuration section below)

3. **Choose Running Mode**

### Option A: Manual Running

- Simply run `BGInfoUpdater.exe` whenever you want to update your desktop information

### Option B: Automatic Startup (Recommended)

- Right-click `setup-autostart.ps1`
- Select 'Run with PowerShell'
- The application will now start automatically when you log in

## How It Works

1. The application runs in the background and performs these actions every 5 minutes:
   - Retrieves your current IP address information
   - Updates location data based on IP
   - Gathers organizational information
   - Updates timestamp
   - Calls BGInfo to refresh your desktop background

2. All temporary data is stored in the `bginfo_temp` folder:
   - `ip.txt`: Contains current IP address information
   - `lastupdated.txt`: Timestamp of the last successful update
   - `location.txt`: Geographic location information
   - `org.txt`: Organization details

## Configuration

The `config.bgi` file contains the BGInfo display configuration. This defines:

- What information is displayed
- Where it appears on the desktop
- Formatting and styling

### Creating config.bgi

The `config.bgi` file must be created manually. Follow these steps to create it:

1. Run `BGInfo\Bginfo64.exe` directly
2. Configure the layout and built-in fields as desired
3. Add these custom fields (File > New Field for each):
   - **IP Address**:
     - Field name: `ip`
     - Type: File
     - File path: `bginfo_temp\ip.txt`
   - **Location**:
     - Field name: `location`
     - Type: File
     - File path: `bginfo_temp\location.txt`
   - **Organization**:
     - Field name: `org`
     - Type: File
     - File path: `bginfo_temp\org.txt`
   - **Last Updated**:
     - Field name: `lastupdated`
     - Type: File
     - File path: `bginfo_temp\lastupdated.txt`
4. Position the fields on your layout as desired
5. Click File > Save As... and save as `config.bgi` in the main application directory

## Troubleshooting

1. **Application Won't Start**
   - Verify .NET 8.0 Runtime is installed
   - Ensure BGInfo64.exe is in the correct location
   - Check Windows Event Viewer for error messages

2. **Information Not Updating**
   - Verify internet connectivity
   - Check if bginfo_temp folder is writable
   - Ensure BGInfo has necessary permissions

3. **Desktop Background Not Changing**
   - Verify you have permissions to change desktop background
   - Check if you have created the config.bgi file in the correct location
   - Verify all required custom fields are properly configured in config.bgi
   - Ensure BGInfo64.exe is properly installed

## Network Requirements

- Outbound internet access is required for IP and location information
- The following directories must be writable:
  - `bginfo_temp` directory
  - Directory containing `config.bgi`

## Building from Source

If you're building from source:

1. Clone the repository
2. Open `BGInfoUpdater.sln` in Visual Studio
3. Build the solution
4. Copy required files to deployment directory:
   - All files from `bin/Release/net8.0`
   - `setup-autostart.ps1`

Note: The `config.bgi` file must be created manually and is not included in the deployment.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Note: This project uses BGInfo from Microsoft Sysinternals which is licensed separately under Microsoft's Sysinternals Software License Terms.

## Support

For issues, questions, or contributions:

- [Create an issue in the repository]
- [Contact information]
- [Additional support resources]
