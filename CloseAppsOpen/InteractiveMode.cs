namespace CloseAppsOpen;

static class InteractiveMode
{
    public static void Run(CliArgs cli)
    {
        while (true)
        {
            var processes = ProcessManager.GetVisible(cli.Exclude);
            ConsoleUI.ShowMenu(processes);

            var key = Console.ReadKey(intercept: true).Key;
            switch (key)
            {
                case ConsoleKey.A:
                    if (processes.Count > 0 && ConsoleUI.Confirm($"Fechar todos os {processes.Count} aplicativo(s)?"))
                        ProcessManager.Close(processes, cli.Timeout);
                    if (processes.Count > 0) ConsoleUI.WaitKey();
                    break;
                case ConsoleKey.S:
                    SelectAndClose(processes, cli.Timeout);
                    break;
                case ConsoleKey.D:
                    ShutdownAll(processes, cli.Timeout);
                    return;
                case ConsoleKey.R:
                    break;
                case ConsoleKey.Q:
                    return;
            }
        }
    }

    static void ShutdownAll(List<(int Pid, string Name, string Title)> processes, int timeout)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ATENÇÃO: Esta ação fechará todos os aplicativos e desligará o PC.");
        Console.ResetColor();

        if (!ConsoleUI.Confirm("Confirmar desligamento?"))
            return;

        if (processes.Count > 0)
            ProcessManager.Close(processes, timeout);

        ConsoleUI.Print("\n  Desligando o PC...", ConsoleColor.Red);
        PowerManager.Shutdown();
    }

    static void SelectAndClose(List<(int Pid, string Name, string Title)> processes, int timeout)
    {
        if (processes.Count == 0) return;

        Console.WriteLine();
        Console.WriteLine("  Números separados por vírgula (ex: 1,3,5), nome do processo ou 'todos':");
        Console.Write("  > ");
        string? input = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input)) return;

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
                .ToList();

            selected = byNumber.Count > 0
                ? byNumber
                : processes.Where(p =>
                    p.Name.Contains(input, StringComparison.OrdinalIgnoreCase) ||
                    p.Title.Contains(input, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (selected.Count == 0)
        {
            ConsoleUI.Print("  Nenhum item válido selecionado.", ConsoleColor.Red);
            ConsoleUI.WaitKey();
            return;
        }

        if (ConsoleUI.Confirm($"Fechar {selected.Count} processo(s)?"))
            ProcessManager.Close(selected, timeout);
        ConsoleUI.WaitKey();
    }
}
