using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

class Program
{
    // Store the original wallpaper path to avoid losing it
    private static string? _originalWallpaperPath = null;
    private static string _originalWallpaperStorePath = Path.Combine(Path.GetTempPath(), "BGInfoUpdater_original_wallpaper.txt");
    private static bool _debugMode = false;
    
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    [DllImport("user32.dll")]
    static extern bool EnumDisplaySettings(string? lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;
    const int SPI_SETDESKWALLPAPER = 20;
    const int SPIF_UPDATEINIFILE = 1;
    const int SPIF_SENDWININICHANGE = 2;
    const int SM_CXSCREEN = 0; // Width of the screen (logical)
    const int SM_CYSCREEN = 1; // Height of the screen (logical)
    const int HORZRES = 8; // Horizontal width in pixels (physical)
    const int VERTRES = 10; // Vertical height in pixels (physical)

    static void DebugLog(string message)
    {
        if (_debugMode)
        {
            Console.WriteLine(message);
        }
    }

    static async Task Main(string[] args)
    {
        // Check if debug mode is enabled
        _debugMode = args.Length > 0 && args[0].ToLower() == "--debug";
        
        // Show console window only in debug mode
        var consoleHandle = GetConsoleWindow();
        ShowWindow(consoleHandle, _debugMode ? SW_SHOW : SW_HIDE);

        // Use temp directory to avoid permission issues
        string tempFolder = Path.GetTempPath();

        while (true)
        {
            try
            {
                var (ip, location, org) = await FetchIpGeoInfo();
                var lastUpdated = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                
                DebugLog($"Fetched info - IP: {ip}, Location: {location}");
                UpdateWallpaper(ip, location, org, lastUpdated, tempFolder);
                DebugLog($"Wallpaper update completed at {lastUpdated}");
            }
            catch (Exception ex)
            {
                DebugLog($"Error in main loop: {ex.Message}");
            }
            await Task.Delay(TimeSpan.FromMinutes(5));
        }
    }

    static async Task<(string ip, string location, string org)> FetchIpGeoInfo()
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync("https://ipinfo.io/json");
        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;
        string ip = root.GetProperty("ip").GetString() ?? "";
        string city = root.GetProperty("city").GetString() ?? "";
        string region = root.GetProperty("region").GetString() ?? "";
        string country = root.GetProperty("country").GetString() ?? "";
        string org = root.GetProperty("org").GetString() ?? "";
        string location = $"{city}, {region}, {country}";
        return (ip, location, org);
    }

    static void UpdateWallpaper(string ip, string location, string org, string lastUpdated, string tempFolder)
    {
        try
        {
            string outputPath = Path.Combine(tempFolder, "wallpaper_with_info.bmp");
            
            // Load stored original wallpaper path from file
            LoadOriginalWallpaperPath();
            
            // Check if we need to update our stored original wallpaper
            string currentPath = GetCurrentWallpaperPath();
            
            // If we don't have a stored original, or current wallpaper is clearly a new user wallpaper
            if (_originalWallpaperPath == null || IsUserWallpaper(currentPath))
            {
                if (!string.IsNullOrEmpty(currentPath) && IsUserWallpaper(currentPath))
                {
                    _originalWallpaperPath = currentPath;
                    SaveOriginalWallpaperPath();
                    DebugLog($"Stored new original wallpaper: {Path.GetFileName(_originalWallpaperPath)}");
                }
            }
            
            // Load background image with proper disposal
            using (Bitmap backgroundImage = LoadBackgroundImage())
            {
                // Get actual screen resolution for all calculations
                var (finalScreenWidth, finalScreenHeight) = GetActualScreenResolution();
                
                // If the background image is larger than screen, we need to resize it to screen resolution
                // so Windows displays it correctly without cropping
                Bitmap finalImage;
                if (backgroundImage.Width != finalScreenWidth || backgroundImage.Height != finalScreenHeight)
                {
                    DebugLog($"Resizing wallpaper from {backgroundImage.Width}x{backgroundImage.Height} to {finalScreenWidth}x{finalScreenHeight}");
                    
                    // Calculate how Windows would display this image (fill screen, center, crop excess)
                    float scaleX = (float)finalScreenWidth / backgroundImage.Width;
                    float scaleY = (float)finalScreenHeight / backgroundImage.Height;
                    float scale = Math.Max(scaleX, scaleY); // Use larger scale to fill screen
                    
                    int scaledWidth = (int)(backgroundImage.Width * scale);
                    int scaledHeight = (int)(backgroundImage.Height * scale);
                    
                    // Calculate what area of the scaled image would be visible (center crop)
                    int cropX = (scaledWidth - finalScreenWidth) / 2;
                    int cropY = (scaledHeight - finalScreenHeight) / 2;
                    
                    // Create final image at screen resolution with the properly scaled/cropped content
                    finalImage = new Bitmap(finalScreenWidth, finalScreenHeight);
                    using (Graphics resizeGraphics = Graphics.FromImage(finalImage))
                    {
                        resizeGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        resizeGraphics.SmoothingMode = SmoothingMode.HighQuality;
                        
                        // First scale the entire image
                        using (Bitmap scaledImage = new Bitmap(scaledWidth, scaledHeight))
                        {
                            using (Graphics scaleGraphics = Graphics.FromImage(scaledImage))
                            {
                                scaleGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                scaleGraphics.DrawImage(backgroundImage, 0, 0, scaledWidth, scaledHeight);
                            }
                            
                            // Then crop the center portion to fit screen
                            resizeGraphics.DrawImage(scaledImage,
                                new Rectangle(0, 0, finalScreenWidth, finalScreenHeight),
                                new Rectangle(cropX, cropY, finalScreenWidth, finalScreenHeight),
                                GraphicsUnit.Pixel);
                        }
                    }
                }
                else
                {
                    // Create a copy of the background image since we're disposing the original
                    finalImage = new Bitmap(backgroundImage);
                }
                
                // Process the final image with system information overlay
                using (finalImage)
                {
                    ProcessFinalImage(finalImage, ip, location, org, lastUpdated, outputPath);
                }
            }
        }
        catch (Exception ex)
        {
            DebugLog($"Error updating wallpaper: {ex.Message}");
        }
    }
    
    static void ProcessFinalImage(Bitmap finalImage, string ip, string location, string org, string lastUpdated, string outputPath)
    {
        try
        {
            // Draw system information on the final image
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                
                // Calculate font sizes based on screen resolution (not image size)
                var (screenWidth, screenHeight) = GetActualScreenResolution();
                
                // Base font sizes on 4K resolution, scale for other resolutions
                float baseTitleSize = 28f * (screenWidth / 3840f); // Scale based on width
                float baseInfoSize = 20f * (screenWidth / 3840f);
                
                // Ensure minimum readable sizes
                baseTitleSize = Math.Max(baseTitleSize, 14f);
                baseInfoSize = Math.Max(baseInfoSize, 10f);
                
                // Define fonts with screen-based sizing
                using (Font titleFont = new Font("Segoe UI", baseTitleSize, FontStyle.Bold))
                using (Font infoFont = new Font("Segoe UI", baseInfoSize, FontStyle.Regular))
                {
                    // Simple bottom-right positioning (image is now at screen resolution)
                    string[] lines = {
                        "System Information",
                        $"IP: {ip}",
                        $"Location: {location}",
                        $"ISP: {org}",
                        $"Updated: {lastUpdated}"
                    };
                    
                    // Calculate text dimensions
                    float maxWidth = 0;
                    float totalHeight = 0;
                    SizeF[] lineSizes = new SizeF[lines.Length];
                    
                    for (int i = 0; i < lines.Length; i++)
                    {
                        Font font = i == 0 ? titleFont : infoFont;
                        lineSizes[i] = g.MeasureString(lines[i], font);
                        maxWidth = Math.Max(maxWidth, lineSizes[i].Width);
                        totalHeight += lineSizes[i].Height + (i > 0 ? 4 : 12); // More space between lines
                    }
                    
                    // Add generous margins for taskbar - scale based on screen resolution
                    float rightMargin = 100f * (screenWidth / 3840f); // Scale margin
                    float bottomMargin = 150f * (screenHeight / 2160f); // Extra space for taskbar
                    
                    // Ensure minimum margins
                    rightMargin = Math.Max(rightMargin, 50f);
                    bottomMargin = Math.Max(bottomMargin, 100f);
                    
                    float startX = finalImage.Width - maxWidth - rightMargin;
                    float startY = finalImage.Height - totalHeight - bottomMargin;
                    
                    // Draw a subtle semi-transparent background for readability
                    RectangleF backgroundRect = new RectangleF(startX - 20, startY - 15, maxWidth + 40, totalHeight + 30);
                    using (var backgroundBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
                    {
                        g.FillRectangle(backgroundBrush, backgroundRect);
                    }
                    using (var borderPen = new Pen(Color.FromArgb(120, 255, 255, 255), 2))
                    {
                        g.DrawRectangle(borderPen, Rectangle.Round(backgroundRect));
                    }
                    
                    // Draw text with white color for contrast against dark background
                    float currentY = startY;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        Font font = i == 0 ? titleFont : infoFont;
                        
                        // Use white text on dark background with proper disposal
                        using (var brush = new SolidBrush(Color.White))
                        {
                            g.DrawString(lines[i], font, brush, startX, currentY);
                        }
                        
                        currentY += lineSizes[i].Height + (i == 0 ? 12 : 4); // More space after title
                    }
                }
            }
            
            // Save the image as BMP (Windows wallpaper compatible format)
            try
            {
                finalImage.Save(outputPath, ImageFormat.Bmp);
            }
            catch (Exception saveEx)
            {
                DebugLog($"Error saving image: {saveEx.Message}");
                // Try saving with a unique filename
                outputPath = Path.Combine(Path.GetTempPath(), $"wallpaper_{DateTime.Now:yyyyMMdd_HHmmss}.bmp");
                finalImage.Save(outputPath, ImageFormat.Bmp);
            }
            
            // Set as wallpaper
            SetWallpaper(outputPath);
        }
        catch (Exception ex)
        {
            DebugLog($"Error processing final image: {ex.Message}");
        }
    }

    static Bitmap LoadBackgroundImage()
    {
        // Try to load the original wallpaper
        if (!string.IsNullOrEmpty(_originalWallpaperPath) && File.Exists(_originalWallpaperPath))
        {
            try
            {
                // Try to load the wallpaper regardless of extension (TranscodedWallpaper has no extension)
                var image = new Bitmap(_originalWallpaperPath);
                DebugLog($"Successfully loaded original wallpaper: {_originalWallpaperPath}");
                return image;
            }
            catch (Exception ex)
            {
                DebugLog($"Failed to load original wallpaper: {ex.Message}");
                // Try loading as different formats
                try
                {
                    using (var fs = new FileStream(_originalWallpaperPath, FileMode.Open, FileAccess.Read))
                    {
                        using var tempImage = Image.FromStream(fs);
                        var image = new Bitmap(tempImage);
                        DebugLog($"Successfully loaded wallpaper using stream: {_originalWallpaperPath}");
                        return image;
                    }
                }
                catch (Exception ex2)
                {
                    DebugLog($"Failed to load wallpaper using stream: {ex2.Message}");
                }
            }
        }
        
        // If we couldn't load the original wallpaper, create a default one
        var (screenWidth, screenHeight) = GetActualScreenResolution();
        DebugLog("Using default background");
        return CreateGradientBackground(screenWidth, screenHeight);
    }

    static Bitmap CreateGradientBackground(int width, int height)
    {
        Bitmap bitmap = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            // Create a simple solid background
            using (var brush = new SolidBrush(Color.FromArgb(102, 126, 234))) // #667eea
            {
                g.FillRectangle(brush, 0, 0, width, height);
            }
        }
        return bitmap;
    }

    static (int width, int height) GetActualScreenResolution()
    {
        // Try to get the actual display mode first
        DEVMODE devMode = new DEVMODE();
        devMode.dmSize = (short)Marshal.SizeOf(devMode);
        
        if (EnumDisplaySettings(null, -1, ref devMode)) // -1 = current settings
        {
            // Get logical resolution for comparison
            int logicalWidth = GetSystemMetrics(SM_CXSCREEN);
            int logicalHeight = GetSystemMetrics(SM_CYSCREEN);
            
            // If display mode shows higher resolution than logical, we're probably on a high-DPI display
            if (devMode.dmPelsWidth > logicalWidth || devMode.dmPelsHeight > logicalHeight)
            {
                return (devMode.dmPelsWidth, devMode.dmPelsHeight);
            }
        }
        
        // Fallback to the previous method
        IntPtr hdc = GetDC(IntPtr.Zero);
        try
        {
            // Get actual physical resolution
            int physicalWidth = GetDeviceCaps(hdc, HORZRES);
            int physicalHeight = GetDeviceCaps(hdc, VERTRES);
            
            // Get logical resolution for comparison
            int logicalWidth = GetSystemMetrics(SM_CXSCREEN);
            int logicalHeight = GetSystemMetrics(SM_CYSCREEN);
            
            // Use physical resolution if different from logical (indicates DPI scaling)
            if (physicalWidth != logicalWidth || physicalHeight != logicalHeight)
            {
                return (physicalWidth, physicalHeight);
            }
            else
            {
                return (logicalWidth, logicalHeight);
            }
        }
        finally
        {
            ReleaseDC(IntPtr.Zero, hdc);
        }
    }

    static string GetCurrentWallpaperPath()
    {
        try
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
            {
                return key?.GetValue("Wallpaper")?.ToString() ?? "";
            }
        }
        catch
        {
            return "";
        }
    }

    static void SetWallpaper(string path)
    {
        // Ensure we have an absolute path
        string absolutePath = Path.GetFullPath(path);
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, absolutePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
    }

    static void LoadOriginalWallpaperPath()
    {
        try
        {
            if (File.Exists(_originalWallpaperStorePath))
            {
                string storedPath = File.ReadAllText(_originalWallpaperStorePath).Trim();
                if (!string.IsNullOrEmpty(storedPath) && File.Exists(storedPath))
                {
                    _originalWallpaperPath = storedPath;
                }
                else
                {
                    // Clean up invalid stored path
                    if (File.Exists(_originalWallpaperStorePath))
                        File.Delete(_originalWallpaperStorePath);
                }
            }
        }
        catch (Exception ex)
        {
            DebugLog($"Error loading original wallpaper path: {ex.Message}");
        }
    }

    static void SaveOriginalWallpaperPath()
    {
        try
        {
            if (!string.IsNullOrEmpty(_originalWallpaperPath))
            {
                File.WriteAllText(_originalWallpaperStorePath, _originalWallpaperPath);
            }
        }
        catch (Exception ex)
        {
            DebugLog($"Error saving original wallpaper path: {ex.Message}");
        }
    }

    static bool IsUserWallpaper(string wallpaperPath)
    {
        if (string.IsNullOrEmpty(wallpaperPath))
            return false;

        string fileName = Path.GetFileName(wallpaperPath);
        string directory = Path.GetDirectoryName(wallpaperPath) ?? "";
        
        // Our generated wallpapers
        if (fileName.Contains("wallpaper_with_info") || 
            fileName.StartsWith("wallpaper_") ||
            directory.Contains("Temp"))
        {
            return false;
        }
        
        // Windows default locations for user wallpapers
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        
        // Check if it's in typical user wallpaper locations
        if (directory.StartsWith(picturesFolder, StringComparison.OrdinalIgnoreCase) ||
            directory.StartsWith(userProfile, StringComparison.OrdinalIgnoreCase) ||
            wallpaperPath.Contains("Desktop", StringComparison.OrdinalIgnoreCase) ||
            wallpaperPath.Contains("Documents", StringComparison.OrdinalIgnoreCase) ||
            wallpaperPath.Contains("Downloads", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check for common image extensions to distinguish from system files
        string extension = Path.GetExtension(wallpaperPath).ToLowerInvariant();
        if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" || 
            extension == ".bmp" || extension == ".gif" || extension == ".webp")
        {
            // If it has a proper image extension and doesn't seem to be our generated file, consider it a user wallpaper
            return true;
        }
        
        // Windows system wallpaper (TranscodedWallpaper, CachedImage_xxx) - these change when user changes wallpaper
        if (fileName == "TranscodedWallpaper" || fileName.StartsWith("CachedImage_"))
        {
            return true; // These are user-selected wallpapers, just transcoded by Windows
        }
        
        return false;
    }
}
