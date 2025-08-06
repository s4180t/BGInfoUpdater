# Get the current directory where the script is running
$currentDir = Split-Path -Parent -Path $MyInvocation.MyCommand.Path
$exePath = Join-Path $currentDir "BGInfoUpdater.exe"

# Create a shortcut in the Windows Startup folder
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut([System.IO.Path]::Combine([System.Environment]::GetFolderPath("Startup"), "BGInfoUpdater.lnk"))
$Shortcut.TargetPath = $exePath
$Shortcut.WorkingDirectory = $currentDir
$Shortcut.Description = "BGInfo Updater Application"
$Shortcut.Save()

Write-Host "BGInfo Updater has been set up to run at startup!"
Write-Host "The application will start automatically when you log in."
