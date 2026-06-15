namespace CloseAppsOpen;

static class ConsoleUI
{
    public static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║          FECHAR APLICATIVOS ABERTOS              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintHelp()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("CloseAppsOpen v1.0.0");
        Console.ResetColor();
        Console.WriteLine("Fecha aplicativos abertos no Windows via linha de comando.\n");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("USO:");
        Console.ResetColor();
        Console.WriteLine("  closeappsopen [opções]\n");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("OPÇÕES:");
        Console.ResetColor();

        (string Flag, string Desc)[] opts =
        [
            ("-a, --all",             "Fecha todos os aplicativos abertos"),
            ("-s, --shutdown",        "Fecha tudo e desliga o PC"),
            ("-k, --kill <nome>",     "Fecha processos que contenham <nome> (pode repetir)"),
            ("-l, --list",            "Lista os aplicativos abertos e sai"),
            ("-e, --exclude <nome>",  "Exclui processo pelo nome (pode repetir)"),
            ("-f, --force",           "Não pede confirmação"),
            ("-t, --timeout <ms>",    "Tempo antes de forçar encerramento (padrão: 2000ms)"),
            ("-v, --version",         "Exibe a versão"),
            ("-h, --help",            "Exibe esta ajuda"),
        ];

        foreach (var (flag, desc) in opts)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"  {flag,-26}");
            Console.ResetColor();
            Console.WriteLine(desc);
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nEXEMPLOS:");
        Console.ResetColor();
        Console.WriteLine("  closeappsopen                        Abre o menu interativo");
        Console.WriteLine("  closeappsopen --list                 Lista os apps abertos");
        Console.WriteLine("  closeappsopen --all                  Fecha tudo (com confirmação)");
        Console.WriteLine("  closeappsopen --all --force          Fecha tudo sem perguntar");
        Console.WriteLine("  closeappsopen --kill chrome          Fecha processos com 'chrome' no nome");
        Console.WriteLine("  closeappsopen --all -e explorer      Fecha tudo exceto o Explorer");
        Console.WriteLine("  closeappsopen -a -e chrome -e slack  Fecha tudo exceto Chrome e Slack");
        Console.WriteLine("  closeappsopen --timeout 5000 --all   Aguarda 5s antes de forçar");
    }

    public static void PrintProcessList(List<(int Pid, string Name, string Title)> processes)
    {
        if (processes.Count == 0) { Print("Nenhum aplicativo encontrado.", ConsoleColor.Yellow); return; }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"{"PID",-8} {"Processo",-22} Janela");
        Console.WriteLine($"{"───────",-8} {"──────────────────────",-22} ──────────────────────────────");
        Console.ResetColor();

        foreach (var (pid, name, title) in processes)
        {
            string trunc = title.Length > 50 ? title[..47] + "..." : title;
            Console.ForegroundColor = ConsoleColor.DarkGray; Console.Write($"{pid,-8}");
            Console.ForegroundColor = ConsoleColor.Green;    Console.Write($" {name,-22}");
            Console.ForegroundColor = ConsoleColor.Gray;     Console.WriteLine($" {trunc}");
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"\nTotal: {processes.Count} aplicativo(s)");
        Console.ResetColor();
    }

    public static void ShowMenu(List<(int Pid, string Name, string Title)> processes)
    {
        Console.Clear();
        PrintHeader();

        if (processes.Count == 0)
        {
            Print("  Nenhum aplicativo com janela visível encontrado.", ConsoleColor.Yellow);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  {"#",-4} {"Processo",-22} Janela");
            Console.WriteLine($"  {"─",-4} {"──────────────────────",-22} ──────────────────────────────");
            Console.ResetColor();

            for (int i = 0; i < processes.Count; i++)
            {
                var (_, name, title) = processes[i];
                string trunc = title.Length > 45 ? title[..42] + "..." : title;
                Console.ForegroundColor = ConsoleColor.White;  Console.Write($"  {i + 1,-4}");
                Console.ForegroundColor = ConsoleColor.Green;  Console.Write($" {name,-22}");
                Console.ForegroundColor = ConsoleColor.Gray;   Console.WriteLine($" {trunc}");
            }

            Console.ResetColor();
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine($"  Total: {processes.Count} aplicativo(s)");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  [A] Fechar Todos    [S] Selecionar    [R] Atualizar    [Q] Sair");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  [D] Fechar Tudo e Desligar PC");
        Console.ResetColor();
        Console.Write("\n  Opção: ");
    }

    public static bool Confirm(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"  {message} [S/N]: ");
        Console.ResetColor();
        var key = Console.ReadKey(intercept: true).Key;
        Console.WriteLine();
        return key == ConsoleKey.S;
    }

    public static void WaitKey()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("\n  Pressione qualquer tecla...");
        Console.ResetColor();
        Console.ReadKey(intercept: true);
    }

    public static void Print(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
