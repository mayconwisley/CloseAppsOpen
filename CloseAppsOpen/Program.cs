using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "Fechar Aplicativos";

while (true)
{
    var processes = GetVisibleProcesses();
    ShowMenu(processes);

    var key = Console.ReadKey(intercept: true).Key;

    switch (key)
    {
        case ConsoleKey.A:
            CloseProcesses(processes, "todos");
            break;
        case ConsoleKey.R:
            continue;
        case ConsoleKey.S:
            SelectAndClose(processes);
            break;
        case ConsoleKey.Q:
            return;
    }
}

static List<(int Pid, string Name, string Title)> GetVisibleProcesses()
{
    return Process.GetProcesses()
        .Where(p =>
        {
            try { return p.MainWindowHandle != IntPtr.Zero && !string.IsNullOrWhiteSpace(p.MainWindowTitle); }
            catch { return false; }
        })
        .OrderBy(p => p.MainWindowTitle)
        .Select(p => (p.Id, p.ProcessName, p.MainWindowTitle))
        .ToList();
}

static void ShowMenu(List<(int Pid, string Name, string Title)> processes)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║          FECHAR APLICATIVOS ABERTOS              ║");
    Console.WriteLine("╚══════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();

    if (processes.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  Nenhum aplicativo com janela visível encontrado.");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  {"#",-4} {"Processo",-20} Janela");
        Console.WriteLine($"  {"─",-4} {"──────────────────",-20} ──────────────────────────────");
        Console.ResetColor();

        for (int i = 0; i < processes.Count; i++)
        {
            var (_, name, title) = processes[i];
            string truncTitle = title.Length > 45 ? title[..42] + "..." : title;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {i + 1,-4}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" {name,-20}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($" {truncTitle}");
        }

        Console.ResetColor();
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine($"  Total: {processes.Count} aplicativo(s)");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  [A] Fechar Todos    [S] Selecionar    [R] Atualizar    [Q] Sair");
    Console.ResetColor();
    Console.Write("\n  Opção: ");
}

static void SelectAndClose(List<(int Pid, string Name, string Title)> processes)
{
    if (processes.Count == 0) return;

    Console.WriteLine();
    Console.WriteLine("  Digite os números separados por vírgula (ex: 1,3,5) ou 'todos':");
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
        selected = input.Split(',')
            .Select(s => int.TryParse(s.Trim(), out int n) ? n : -1)
            .Where(n => n >= 1 && n <= processes.Count)
            .Select(n => processes[n - 1])
            .Distinct()
            .ToList();
    }

    if (selected.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  Nenhum item válido selecionado.");
        Console.ResetColor();
        Thread.Sleep(1500);
        return;
    }

    CloseProcesses(selected, $"{selected.Count} selecionado(s)");
}

static void CloseProcesses(List<(int Pid, string Name, string Title)> targets, string label)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"  Fechar {label}? [S/N]: ");
    Console.ResetColor();

    var key = Console.ReadKey(intercept: true).Key;
    Console.WriteLine();

    if (key != ConsoleKey.S && key != ConsoleKey.Enter) return;

    int closed = 0, failed = 0;

    foreach (var (pid, name, title) in targets)
    {
        try
        {
            var p = Process.GetProcessById(pid);
            p.CloseMainWindow();
            if (!p.WaitForExit(2000))
                p.Kill();
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
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"  Resultado: {closed} fechado(s), {failed} falha(s).");
    Console.ResetColor();
    Console.Write("  Pressione qualquer tecla...");
    Console.ReadKey(intercept: true);
}
