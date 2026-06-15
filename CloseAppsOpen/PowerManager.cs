using System.Diagnostics;

namespace CloseAppsOpen;

static class PowerManager
{
    public static void Shutdown(int delaySeconds = 0)
    {
        Process.Start(new ProcessStartInfo("shutdown", $"/s /t {delaySeconds} /c \"Desligando via CloseAppsOpen\"")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        });
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
