namespace CloseAppsOpen;

internal static class ProcessSelection
{
    public static List<(int Pid, string Name, string Title)> ByFilter(
        IEnumerable<(int Pid, string Name, string Title)> processes,
        IEnumerable<string> filters)
    {
        var terms = filters.ToList();
        return processes.Where(process => terms.Any(term =>
                process.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                process.Title.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .DistinctBy(process => process.Pid)
            .ToList();
    }
}
