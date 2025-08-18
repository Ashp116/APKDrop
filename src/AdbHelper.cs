using System.Diagnostics;

namespace APKDrop;

public class AdbDevice
{
    public string SerialNumber { get; set; }
    public string ProductName { get; set; }
    public string Model {get; set;}
}

public static class AdbHelper
{
    public static string adbPath = Path.Combine(AppContext.BaseDirectory, "src" , "adb", "adb.exe");
    public static List<AdbDevice> ListDevices()
    {
        if (!File.Exists(adbPath))
            throw new FileNotFoundException("adb.exe not found.", adbPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = adbPath,
            Arguments = "devices -l",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var lines = output.Split('\n')
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line) && line.Contains("device"));

        var devices = new List<AdbDevice>();

        foreach (var line in lines)
        {
            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var serial = parts.ElementAtOrDefault(0);
            var keyValuePairs = parts.Skip(1)
                .Select(p => p.Split(':'))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0], p => p[1]);

            keyValuePairs.TryGetValue("model", out var model);
            keyValuePairs.TryGetValue("product", out var product);

            if (!string.IsNullOrEmpty(serial))
            {
                devices.Add(new AdbDevice
                {
                    SerialNumber = serial,
                    ProductName = product ?? "unknown",
                    Model = model ?? "unknown"
                });
            }
        }

        return devices;
    }
    
    public static bool PushAppInstall(string serial, string apkPath)
    {
        if (!File.Exists(adbPath))
            throw new FileNotFoundException("adb.exe not found.", adbPath);

        if (!File.Exists(apkPath))
            throw new FileNotFoundException("APK file not found.", apkPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = adbPath,
            Arguments = $"-s {serial} install \"{apkPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            //throw new InvalidOperationException($"APK install failed:\n{error}\n{output}");
            return false;
        }
        
        return true;
    }
}