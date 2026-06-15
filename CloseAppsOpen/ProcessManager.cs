using System.Diagnostics;

namespace CloseAppsOpen;

static class ProcessManager
{
	public static List<(int Pid, string Name, string Title)> GetVisible(IEnumerable<string> exclude)
	{
		var excluded = new HashSet<string>(exclude, StringComparer.OrdinalIgnoreCase);
		int selfPid = Environment.ProcessId;
		return Process.GetProcesses()
			.Where(p =>
			{
				try
				{
					return p.Id != selfPid
						&& p.MainWindowHandle != IntPtr.Zero
						&& !string.IsNullOrWhiteSpace(p.MainWindowTitle)
						&& !excluded.Contains(p.ProcessName);
				}
				catch { return false; }
			})
			.OrderBy(p => p.MainWindowTitle)
			.Select(p => (p.Id, p.ProcessName, p.MainWindowTitle))
			.ToList();
	}

	public static bool Close(List<(int Pid, string Name, string Title)> targets, int timeout)
	{
		Console.WriteLine();
		int closed = 0, failed = 0;

		foreach (var (pid, name, title) in targets)
		{
			try
			{
				var p = Process.GetProcessById(pid);
				p.CloseMainWindow();
				if (!p.WaitForExit(timeout)) p.Kill();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"  ✓ {name} — {title}");
				closed++;
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"  ✗ {name} — não foi possível fechar");
				failed++;
			}
		}

		Console.ResetColor();
		Console.WriteLine();
		Console.ForegroundColor = failed > 0 ? ConsoleColor.Yellow : ConsoleColor.Cyan;
		Console.WriteLine($"  Resultado: {closed} fechado(s), {failed} falha(s).");
		Console.ResetColor();
		return failed == 0;
	}
}
