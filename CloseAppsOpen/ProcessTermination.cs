using System.Diagnostics;

namespace CloseAppsOpen;

internal interface IProcessControl
{
    bool HasExited { get; }
    bool CloseMainWindow();
    bool WaitForExit(int milliseconds);
    void Kill();
}

internal sealed class SystemProcessControl(Process process) : IProcessControl
{
    public bool HasExited => process.HasExited;
    public bool CloseMainWindow() => process.CloseMainWindow();
    public bool WaitForExit(int milliseconds) => process.WaitForExit(milliseconds);
    public void Kill() => process.Kill();
}

internal static class ProcessTermination
{
    public static bool TryClose(IProcessControl process, int timeout, bool force)
    {
        if (force)
        {
            process.Kill();
            return process.WaitForExit(timeout);
        }

        if (!process.CloseMainWindow())
            return process.HasExited;

        return process.WaitForExit(timeout);
    }
}
