# BGInfoUpdater

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)](https://www.microsoft.com/windows)

A lightweight Windows app that overlays system information on your wallpaper without external dependencies.

## Features

- **Preserves your wallpaper** - Never loses your original background
- **Real-time info** - IP, location, ISP details updated every 5 minutes  
- **4K display support** - Auto-scales for high-resolution screens
- **Professional overlay** - Semi-transparent design in bottom-right corner
- **Zero setup** - Works instantly, no configuration needed

## Quick Start

1. Download [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Extract BGInfoUpdater files to any folder
3. Run `BGInfoUpdater.exe` (runs silently)
4. For debugging: `BGInfoUpdater.exe --debug` (shows console output)

**Optional:** Run `setup-autostart.ps1` for automatic Windows startup.

## How It Works

The app detects your current wallpaper, adds a clean overlay with system info, and updates it every 5 minutes. Your original wallpaper is always preserved.

## Troubleshooting

**Won't start?** Install .NET 8.0 Runtime and check Windows Event Viewer  
**Info not updating?** Check internet connection and firewall  
**Wallpaper issues?** Ensure Windows background is set to "Picture" mode  
**Need logs?** Run `BGInfoUpdater.exe --debug` to see console output

## Building

```bash
git clone https://github.com/s4180t/BGInfoUpdater.git
cd BGInfoUpdater
dotnet build -c Release
```

## License

MIT License - see [LICENSE](LICENSE) file.
