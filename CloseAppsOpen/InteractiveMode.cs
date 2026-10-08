namespace CloseAppsOpen;

static class InteractiveMode
{
    public static bool Run(CliArgs cli)
    {
        bool succeeded = true;
        while (true)
        {
            var processes = ProcessManager.GetVisible(cli.Exclude);
            ConsoleUI.ShowMenu(processes);

            var key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.A:
                    if (processes.Count > 0 && ConsoleUI.Confirm($"Fechar todos os {processes.Count} aplicativo(s)?"))
                        succeeded &= ProcessManager.Close(processes, cli.Timeout, cli.Force);
                    if (processes.Count > 0) ConsoleUI.WaitKey();
                    break;
                case ConsoleKey.S:
                    succeeded &= SelectAndClose(processes, cli.Timeout, cli.Force);
                    break;
                case ConsoleKey.D:
                    return ShutdownAll(processes, cli.Timeout, cli.Force) && succeeded;
                case ConsoleKey.R:
                    break;
                case ConsoleKey.Q:
                    return succeeded;
            }
        }
    }

    static bool ShutdownAll(List<(int Pid, string Name, string Title)> processes, int timeout, bool force)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ATENÇÃO: Esta ação fechará todos os aplicativos e desligará o PC.");
        Console.ResetColor();

        if (!ConsoleUI.Confirm("Confirmar desligamento?"))
            return true;

        bool succeeded = CloseWorkflow.Run(
            () => processes.Count == 0 || ProcessManager.Close(processes, timeout, force),
            shutdown: true,
            powerOff: () =>
            {
                ConsoleUI.Print("\n  Desligando o PC...", ConsoleColor.Red);
                return PowerManager.Shutdown();
            });
        if (!succeeded)
            ConsoleUI.Print("  Desligamento cancelado ou não iniciado: houve falha no fechamento ou no comando de desligamento.", ConsoleColor.Red);
        return succeeded;
    }

    static bool SelectAndClose(List<(int Pid, string Name, string Title)> processes, int timeout, bool force)
    {
        if (processes.Count == 0) return true;

        Console.WriteLine();
        Console.WriteLine("  Números separados por vírgula (ex: 1,3,5), nome do processo ou 'todos':");
        Console.Write("  > ");
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input)) return true;

        List<(int Pid, string Name, string Title)> selected;

        if (input.Equals("todos", StringComparison.OrdinalIgnoreCase))
        {
            selected = processes;
        }
        else
        {
            var byNumber = input.Split(',')
                .Select(s => int.TryParse(s.Trim(), out int n) ? n : -1)
                .Where(n => n >= 1 && n <= processes.Count)
                .Select(n => processes[n - 1])
                .DistinctBy(p => p.Pid)
                .ToList();

            selected = byNumber.Count > 0
                ? byNumber
                : ProcessSelection.ByFilter(processes, [input]);
        }

        if (selected.Count == 0)
        {
            ConsoleUI.Print("  Nenhum item válido selecionado.", ConsoleColor.Red);
            ConsoleUI.WaitKey();
            return true;
        }

        bool succeeded = true;
        if (ConsoleUI.Confirm($"Fechar {selected.Count} processo(s)?"))
            succeeded = ProcessManager.Close(selected, timeout, force);
        ConsoleUI.WaitKey();
        return succeeded;
    }
}
