using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    static async Task Main(string[] args)
    {
        string basePath = ".";
        string bginfoExe = Path.Combine(basePath, "BGInfo", "Bginfo64.exe");
        
        // Check if BGInfo is present
        if (!File.Exists(bginfoExe))
        {
            // Show console window for error message
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
            
            Console.WriteLine("Error: BGInfo not found!");
            Console.WriteLine("\nPlease download BGInfo from Microsoft Sysinternals:");
            Console.WriteLine("https://learn.microsoft.com/en-us/sysinternals/downloads/bginfo");
            Console.WriteLine("\nAfter downloading:");
            Console.WriteLine("1. Create a 'BGInfo' folder in the same directory as this program");
            Console.WriteLine("2. Extract Bginfo64.exe from the downloaded BGInfo.zip into the 'BGInfo' folder");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            return;
        }

        // Hide console window for normal operation
        var consoleHandle = GetConsoleWindow();
        ShowWindow(consoleHandle, SW_HIDE);

        string tempFolder = Path.Combine(basePath, "bginfo_temp");
        Directory.CreateDirectory(tempFolder);
        string ipFile = Path.Combine(tempFolder, "ip.txt");
        string locationFile = Path.Combine(tempFolder, "location.txt");
        string orgFile = Path.Combine(tempFolder, "org.txt");
        string lastUpdatedFile = Path.Combine(tempFolder, "lastupdated.txt");
        string bginfoBgi = Path.Combine(basePath, "config.bgi");

        while (true)
        {
            try
            {
                var (ip, location, org) = await FetchIpGeoInfo();
                var lastUpdated = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                File.WriteAllText(ipFile, ip, Encoding.UTF8);
                File.WriteAllText(locationFile, location, Encoding.UTF8);
                File.WriteAllText(orgFile, org, Encoding.UTF8);
                File.WriteAllText(lastUpdatedFile, lastUpdated, Encoding.UTF8);
                RunBgInfo(bginfoExe, bginfoBgi);
            }
            catch (Exception ex)
            {
                // Optionally log error
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

    static void RunBgInfo(string exe, string bgi)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = $"{bgi} /timer:0 /silent /nolicprompt",
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);
    }
}
