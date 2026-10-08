using CloseAppsOpen;
using Xunit;

namespace CloseAppsOpen.Tests;

public class ProcessSelectionTests
{
    [Fact]
    public void ByFilter_MatchesNameOrTitleWithoutDuplicates()
    {
        (int Pid, string Name, string Title)[] processes =
        [
            (1, "chrome", "Documentação"),
            (2, "code", "Projeto Chrome"),
            (3, "notepad", "Notas")
        ];

        var selected = ProcessSelection.ByFilter(processes, ["CHROME", "Documentação"]);

        Assert.Equal([1, 2], selected.Select(p => p.Pid));
    }
}
