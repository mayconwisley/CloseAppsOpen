using CloseAppsOpen;
using Xunit;

namespace CloseAppsOpen.Tests;

public class InteractiveShutdownTests
{
    private static readonly List<(int Pid, string Name, string Title)> Processes = [(42, "editor", "Documento")];

    [Fact]
    public void ConfirmedShutdown_ForcesCloseBeforePowerOff()
    {
        bool? forceReceived = null;
        bool poweredOff = false;

        bool succeeded = InteractiveMode.ShutdownAll(
            Processes,
            2000,
            (_, _, force) => { forceReceived = force; return true; },
            () => { poweredOff = true; return true; },
            _ => true);

        Assert.True(succeeded);
        Assert.True(forceReceived);
        Assert.True(poweredOff);
    }

    [Fact]
    public void CloseFailure_PreventsPowerOff()
    {
        bool poweredOff = false;

        bool succeeded = InteractiveMode.ShutdownAll(
            Processes,
            2000,
            (_, _, _) => false,
            () => { poweredOff = true; return true; },
            _ => true);

        Assert.False(succeeded);
        Assert.False(poweredOff);
    }

    [Fact]
    public void RejectedConfirmation_DoesNothing()
    {
        bool closeCalled = false;

        bool succeeded = InteractiveMode.ShutdownAll(
            Processes,
            2000,
            (_, _, _) => { closeCalled = true; return true; },
            () => throw new Exception("Desligamento não deveria ser chamado"),
            _ => false);

        Assert.True(succeeded);
        Assert.False(closeCalled);
    }

    [Fact]
    public void ForcedShutdown_UsesWindowsForceFlag()
    {
        var info = PowerManager.CreateShutdownStartInfo(0, force: true);

        Assert.Equal(["/s", "/f", "/t", "0", "/c", "Desligando via CloseAppsOpen"], info.ArgumentList);
    }

    [Fact]
    public void RegularShutdown_DoesNotUseWindowsForceFlag()
    {
        var info = PowerManager.CreateShutdownStartInfo(0, force: false);

        Assert.DoesNotContain("/f", info.ArgumentList);
    }
}
