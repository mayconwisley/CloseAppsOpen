namespace CloseAppsOpen;

sealed class CliArgs
{
	public bool Help { get; private set; }
	public bool Version { get; private set; }
	public bool CloseAll { get; private set; }
	public bool Shutdown { get; private set; }
	public bool List { get; private set; }
	public bool Force { get; private set; }
	public int Timeout { get; private set; } = 2000;
	public List<string> Kill { get; private set; } = [];
	public List<string> Exclude { get; private set; } = [];

	public static CliArgs Parse(string[] argv)
	{
		var a = new CliArgs();
		for (int i = 0; i < argv.Length; i++)
		{
			switch (argv[i].ToLowerInvariant())
			{
				case "-h": case "--help": a.Help = true; break;
				case "-v": case "--version": a.Version = true; break;
				case "-a": case "--all": a.CloseAll = true; break;
				case "-s": case "--shutdown": a.Shutdown = true; a.CloseAll = true; break;
				case "-l": case "--list": a.List = true; break;
				case "-f": case "--force": a.Force = true; break;
				case "-t":
				case "--timeout":
					if (i + 1 < argv.Length && int.TryParse(argv[++i], out int ms))
						a.Timeout = ms;
					break;
				case "-k":
				case "--kill":
					if (i + 1 < argv.Length) a.Kill.Add(argv[++i]);
					break;
				case "-e":
				case "--exclude":
					if (i + 1 < argv.Length) a.Exclude.Add(argv[++i]);
					break;
				default:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Argumento desconhecido: {argv[i]}. Use --help para ver as opções.");
					Console.ResetColor();
					Environment.Exit(1);
					break;
			}
		}
		return a;
	}
}
