using CloseAppsOpen;
using Xunit;

namespace CloseAppsOpen.Tests;

public class ProcessTerminationTests
{
    [Fact]
    public void GracefulTimeout_LeavesProcessOpen()
    {
        var process = new FakeProcess { ExitAfterWait = false };

        bool closed = ProcessTermination.TryClose(process, 2000, force: false);

        Assert.False(closed);
        Assert.Equal(1, process.CloseCalls);
        Assert.Equal(0, process.KillCalls);
        Assert.Equal(2000, process.LastWaitMilliseconds);
    }

    [Fact]
    public void Force_RequiresConfirmedExit()
    {
        var process = new FakeProcess { ExitAfterWait = false };

        bool closed = ProcessTermination.TryClose(process, 2000, force: true);

        Assert.False(closed);
        Assert.Equal(1, process.KillCalls);
        Assert.Equal(0, process.CloseCalls);
    }

    [Fact]
    public void GracefulExit_IsSuccess()
    {
        var process = new FakeProcess { ExitAfterWait = true };

        Assert.True(ProcessTermination.TryClose(process, 5000, force: false));
        Assert.Equal(0, process.KillCalls);
    }

    [Fact]
    public void NoMainWindow_IsFailureWhenProcessStillRuns()
    {
        var process = new FakeProcess { CloseAccepted = false };

        Assert.False(ProcessTermination.TryClose(process, 2000, force: false));
        Assert.Null(process.LastWaitMilliseconds);
    }

    private sealed class FakeProcess : IProcessControl
    {
        public bool HasExited { get; set; }
        public bool CloseAccepted { get; set; } = true;
        public bool ExitAfterWait { get; set; }
        public int CloseCalls { get; private set; }
        public int KillCalls { get; private set; }
        public int? LastWaitMilliseconds { get; private set; }

        public bool CloseMainWindow() { CloseCalls++; return CloseAccepted; }
        public bool WaitForExit(int milliseconds) { LastWaitMilliseconds = milliseconds; return ExitAfterWait; }
        public void Kill() { KillCalls++; }
    }
}
