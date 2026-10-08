using CloseAppsOpen;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "CloseAppsOpen";

CliArgs cli;
try
{
	cli = CliArgs.Parse(Environment.GetCommandLineArgs()[1..]);
}
catch (ArgumentException ex)
{
	ConsoleUI.Print(ex.Message, ConsoleColor.Red);
	return 2;
}

if (cli.Help)
{
	ConsoleUI.PrintHelp();
	return 0;
}

if (cli.Version)
{
	var version = typeof(CliArgs).Assembly.GetName().Version?.ToString(3) ?? "desconhecida";
	Console.WriteLine($"CloseAppsOpen v{version}");
	return 0;
}
if (cli.List)
{
	ConsoleUI.PrintProcessList(ProcessManager.GetVisible(cli.Exclude));
	return 0;
}

if (cli.CloseAll)
{
	var procs = ProcessManager.GetVisible(cli.Exclude);
	string action = cli.Shutdown ? "Fechar todos e DESLIGAR o PC" : $"Fechar todos os {procs.Count} aplicativo(s)";

	if (procs.Count == 0 && !cli.Shutdown)
	{
		ConsoleUI.Print("Nenhum aplicativo encontrado.", ConsoleColor.Yellow);
		return 0;
	}

	if (!cli.Force && !ConsoleUI.Confirm(action))
		return 0;

	bool succeeded = CloseWorkflow.Run(
		() => procs.Count == 0 || ProcessManager.Close(procs, cli.Timeout, cli.Force),
		cli.Shutdown,
		() =>
		{
			ConsoleUI.Print("  Desligando o PC...", ConsoleColor.Red);
			return PowerManager.Shutdown();
		});
	if (!succeeded && cli.Shutdown)
		ConsoleUI.Print("  Desligamento cancelado ou não iniciado: houve falha no fechamento ou no comando de desligamento.", ConsoleColor.Red);
	return succeeded ? 0 : 1;
}

if (cli.Kill.Count > 0)
{
	var all = ProcessManager.GetVisible(cli.Exclude);
	var targets = ProcessSelection.ByFilter(all, cli.Kill);

	if (targets.Count == 0)
	{
		ConsoleUI.Print("Nenhum processo encontrado com os filtros informados.", ConsoleColor.Yellow);
		return 1;
	}

	ConsoleUI.PrintProcessList(targets);
	if (!cli.Force && !ConsoleUI.Confirm($"Fechar {targets.Count} processo(s) acima?"))
		return 0;
	return ProcessManager.Close(targets, cli.Timeout, cli.Force) ? 0 : 1;
}

return InteractiveMode.Run(cli) ? 0 : 1;
