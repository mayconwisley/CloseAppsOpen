using System.Diagnostics;

namespace CloseAppsOpen;

static class PowerManager
{
    public static bool Shutdown(int delaySeconds = 0)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo("shutdown", $"/s /t {delaySeconds} /c \"Desligando via CloseAppsOpen\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
            if (process is null) return false;
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
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
