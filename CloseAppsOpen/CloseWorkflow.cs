namespace CloseAppsOpen;

internal static class CloseWorkflow
{
    public static bool Run(Func<bool> close, bool shutdown, Func<bool> powerOff)
    {
        if (!close())
            return false;

        return !shutdown || powerOff();
    }
}
