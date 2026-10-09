using System.Diagnostics;

namespace CloseAppsOpen;

static class PowerManager
{
    public static bool Shutdown(int delaySeconds = 0, bool force = false)
    {
        try
        {
            using var process = Process.Start(CreateShutdownStartInfo(delaySeconds, force));
            if (process is null) return false;
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    internal static ProcessStartInfo CreateShutdownStartInfo(int delaySeconds, bool force)
    {
        var info = new ProcessStartInfo("shutdown")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        info.ArgumentList.Add("/s");
        if (force) info.ArgumentList.Add("/f");
        info.ArgumentList.Add("/t");
        info.ArgumentList.Add(delaySeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        info.ArgumentList.Add("/c");
        info.ArgumentList.Add("Desligando via CloseAppsOpen");
        return info;
    }

    public static void CancelShutdown()
    {
        Process.Start(new ProcessStartInfo("shutdown", "/a")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }
}
