using CloseAppsOpen;
using Xunit;

namespace CloseAppsOpen.Tests;

public class CloseWorkflowTests
{
    [Fact]
    public void CloseFailure_PreventsShutdownAndReportsFailure()
    {
        bool shutdownCalled = false;

        bool succeeded = CloseWorkflow.Run(
            close: () => false,
            shutdown: true,
            powerOff: () => { shutdownCalled = true; return true; });

        Assert.False(succeeded);
        Assert.False(shutdownCalled);
    }

    [Fact]
    public void SuccessfulClose_AllowsShutdown()
    {
        bool shutdownCalled = false;

        bool succeeded = CloseWorkflow.Run(
            close: () => true,
            shutdown: true,
            powerOff: () => { shutdownCalled = true; return true; });

        Assert.True(succeeded);
        Assert.True(shutdownCalled);
    }

    [Fact]
    public void ShutdownCommandFailure_IsReported()
    {
        Assert.False(CloseWorkflow.Run(() => true, shutdown: true, powerOff: () => false));
    }

    [Fact]
    public void CloseOnly_DoesNotCallShutdown()
    {
        Assert.True(CloseWorkflow.Run(() => true, shutdown: false, powerOff: () => throw new Exception()));
    }
}
