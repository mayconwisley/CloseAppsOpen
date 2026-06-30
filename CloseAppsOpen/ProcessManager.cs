using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CloseAppsOpen;

static class ProcessManager
{
	private const int TH32CS_SNAPPROCESS = 0x00000002;

	[StructLayout(LayoutKind.Sequential)]
	private struct PROCESSENTRY32
	{
		public uint dwSize;
		public uint cntUsage;
		public uint th32ProcessID;
		public IntPtr th32DefaultHeapID;
		public uint th32ModuleID;
		public uint cntThreads;
		public uint th32ParentProcessID;
		public int pcPriClassBase;
		public uint dwFlags;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szExeFile;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

	[DllImport("kernel32.dll")]
	private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll")]
	private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CloseHandle(IntPtr hObject);

	// Toda a cadeia de processos pai (shell, conhost/OpenConsole, WindowsTerminal...)
	// que hospeda este app. Precisa ser excluída ou fecharíamos a janela/terminal
	// que está rodando o próprio programa antes de terminar de fechar os demais.
	// Observação: dentro do Windows Terminal, o app roda via pseudoconsole (ConPTY),
	// então GetConsoleWindow() não aponta para o WindowsTerminal.exe — por isso
	// excluímos a árvore de ancestrais inteira.
	private static HashSet<int> AncestorPids()
	{
		var ancestors = new HashSet<int>();
		var parentMap = new Dictionary<int, int>();
		IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
		if (snapshot == IntPtr.Zero || snapshot == new IntPtr(-1))
			return ancestors;

		try
		{
			var entry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf<PROCESSENTRY32>() };
			if (Process32First(snapshot, ref entry))
			{
				do { parentMap[(int)entry.th32ProcessID] = (int)entry.th32ParentProcessID; }
				while (Process32Next(snapshot, ref entry));
			}
		}
		catch { /* ignora; retorna o que tiver */ }
		finally { CloseHandle(snapshot); }

		int pid = Environment.ProcessId;
		while (parentMap.TryGetValue(pid, out int parent) && parent != 0 && ancestors.Add(parent))
			pid = parent;

		return ancestors;
	}

	private static string SafeConsoleTitle()
	{
		try { return OperatingSystem.IsWindows() ? Console.Title : ""; }
		catch { return ""; }
	}

	public static List<(int Pid, string Name, string Title)> GetVisible(IEnumerable<string> exclude)
	{
		var excluded = new HashSet<string>(exclude, StringComparer.OrdinalIgnoreCase);
		int selfPid = Environment.ProcessId;
		var ancestors = AncestorPids();

		// Título do nosso console (definido em Program.cs). O terminal que hospeda
		// o app reflete esse título na própria janela — sob Windows Terminal, o
		// WindowsTerminal.exe NÃO é nosso processo pai (a comunicação é via ConPTY),
		// então a cadeia de ancestrais não o alcança. Casar pelo título é o jeito
		// confiável de identificar e excluir justamente o terminal que nos hospeda.
		string ownTitle = SafeConsoleTitle();

		return Process.GetProcesses()
			.Where(p =>
			{
				try
				{
					return p.Id != selfPid
						&& !ancestors.Contains(p.Id)
						&& p.MainWindowHandle != IntPtr.Zero
						&& !string.IsNullOrWhiteSpace(p.MainWindowTitle)
						&& !(ownTitle.Length > 0 && p.MainWindowTitle == ownTitle)
						&& !excluded.Contains(p.ProcessName);
				}
				catch { return false; }
			})
			.OrderBy(p => p.MainWindowTitle)
			.Select(p => (p.Id, p.ProcessName, p.MainWindowTitle))
			.ToList();
	}

	public static bool Close(List<(int Pid, string Name, string Title)> targets, int timeout, bool force = false)
	{
		Console.WriteLine();
		int closed = 0, failed = 0;

		foreach (var (pid, name, title) in targets)
		{
			try
			{
				var p = Process.GetProcessById(pid);
				if (force)
				{
					// Encerramento imediato: não envia WM_CLOSE, então apps como
					// o Office NÃO exibem o diálogo "Salvar alterações?". Em troca,
					// descarta qualquer trabalho não salvo sem aviso.
					p.Kill();
					p.WaitForExit(timeout);
				}
				else
				{
					// Fechamento gentil (equivale a clicar no X): dá ao app a chance
					// de pedir confirmação. Só força (Kill) se não sair no timeout.
					p.CloseMainWindow();
					if (!p.WaitForExit(timeout)) p.Kill();
				}
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
