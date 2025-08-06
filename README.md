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

The application runs in the background and updates your desktop every 5 minutes with current system information including IP address, location, and organization details.

## Configuration

Create a `config.bgi` file using BGInfo's configuration tool:

1. Run `BGInfo\Bginfo64.exe` directly
2. Configure the layout and built-in fields as desired
3. Add custom fields (File > New Field for each):
   - **IP Address**: Field name `ip`, Type: File, Path: `bginfo_temp\ip.txt`
   - **Location**: Field name `location`, Type: File, Path: `bginfo_temp\location.txt`
   - **Organization**: Field name `org`, Type: File, Path: `bginfo_temp\org.txt`
   - **Last Updated**: Field name `lastupdated`, Type: File, Path: `bginfo_temp\lastupdated.txt`
4. Position fields and save as `config.bgi` in the main application directory

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
   - Ensure the `config.bgi` file exists with properly configured custom fields
   - Verify BGInfo64.exe is properly installed

## Building from Source

1. Clone the repository
2. Open `BGInfoUpdater.sln` in Visual Studio
3. Build the solution
4. Copy files from `bin/Release/net8.0` and `setup-autostart.ps1` to deployment directory

Note: Create the `config.bgi` file manually after deployment.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Note: This project uses BGInfo from Microsoft Sysinternals which is licensed separately under Microsoft's Sysinternals Software License Terms.

## Support

For issues, questions, or contributions:

- [Create an issue on GitHub](https://github.com/s4180t/BGInfoUpdater/issues)
- [View the project repository](https://github.com/s4180t/BGInfoUpdater)
- [Submit pull requests](https://github.com/s4180t/BGInfoUpdater/pulls)
