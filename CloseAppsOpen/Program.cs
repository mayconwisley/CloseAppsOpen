using CloseAppsOpen;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "CloseAppsOpen";

var cli = CliArgs.Parse(Environment.GetCommandLineArgs()[1..]);

if (cli.Help)    { ConsoleUI.PrintHelp(); return 0; }
if (cli.Version) { Console.WriteLine("CloseAppsOpen v1.0.0"); return 0; }

if (cli.List)
{
    ConsoleUI.PrintProcessList(ProcessManager.GetVisible(cli.Exclude));
    return 0;
}

if (cli.CloseAll)
{
    var procs = ProcessManager.GetVisible(cli.Exclude);
    if (procs.Count == 0) { ConsoleUI.Print("Nenhum aplicativo encontrado.", ConsoleColor.Yellow); return 0; }
    if (!cli.Force && !ConsoleUI.Confirm($"Fechar todos os {procs.Count} aplicativo(s)?")) return 0;
    return ProcessManager.Close(procs, cli.Timeout) ? 0 : 1;
}

if (cli.Kill.Count > 0)
{
    var all = ProcessManager.GetVisible(cli.Exclude);
    var targets = all.Where(p =>
        cli.Kill.Any(k => p.Name.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                          p.Title.Contains(k, StringComparison.OrdinalIgnoreCase))).ToList();

    if (targets.Count == 0)
    {
        ConsoleUI.Print("Nenhum processo encontrado com os filtros informados.", ConsoleColor.Yellow);
        return 1;
    }

    ConsoleUI.PrintProcessList(targets);
    if (!cli.Force && !ConsoleUI.Confirm($"Fechar {targets.Count} processo(s) acima?")) return 0;
    return ProcessManager.Close(targets, cli.Timeout) ? 0 : 1;
}

InteractiveMode.Run(cli);
return 0;
