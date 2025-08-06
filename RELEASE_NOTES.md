# BGInfoUpdater v1.0.0

First official release of BGInfoUpdater!

## Features

- Automatic system information updates every 5 minutes
- Real-time IP address information display
- Location information integration
- Organizational details display
- Silent background operation
- Windows startup integration

## Installation

1. Download and install BGInfo from Microsoft Sysinternals:
   - Visit [Microsoft Sysinternals BGInfo](https://learn.microsoft.com/en-us/sysinternals/downloads/bginfo)
   - Accept the license terms
   - Download and extract BGInfo to the `BGInfo` folder in the installation directory
2. Download the `BGInfoUpdater.zip` from this release
3. Extract the contents to your desired location
4. Run `setup-autostart.ps1` with administrator privileges to set up automatic startup
5. Create a `config.bgi` file using BGInfo configuration tool (see README for details)

## Requirements

- Windows operating system
- .NET 8.0 Runtime
- Administrative privileges for setup

## Known Issues

None reported

## Notes

- Make sure to read the README.txt included in the package for detailed setup instructions
- The `config.bgi` file must be created manually after installation
