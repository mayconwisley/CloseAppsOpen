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
	public List<string> Kill { get; } = [];
	public List<string> Exclude { get; } = [];

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
				case "-t": case "--timeout":
					var value = ReadValue(argv, ref i);
					if (!int.TryParse(value, out int ms) || ms < 0)
						throw new ArgumentException("--timeout exige um número inteiro não negativo de milissegundos.");
					a.Timeout = ms;
					break;
				case "-k": case "--kill":
					a.Kill.Add(ReadValue(argv, ref i));
					break;
				case "-e": case "--exclude":
					a.Exclude.Add(ReadValue(argv, ref i));
					break;
				default:
					throw new ArgumentException($"Argumento desconhecido: {argv[i]}. Use --help para ver as opções.");
			}
		}
		if (a.List && (a.CloseAll || a.Kill.Count > 0))
			throw new ArgumentException("--list não pode ser combinado com --all, --shutdown ou --kill.");
		if (a.CloseAll && a.Kill.Count > 0)
			throw new ArgumentException("--kill não pode ser combinado com --all ou --shutdown.");
		return a;
	}

	private static string ReadValue(string[] argv, ref int index)
	{
		string option = argv[index];
		if (index + 1 >= argv.Length || string.IsNullOrWhiteSpace(argv[index + 1]) || argv[index + 1].StartsWith('-'))
			throw new ArgumentException($"{option} exige um valor. Use --help para ver as opções.");

		return argv[++index];
	}
}
