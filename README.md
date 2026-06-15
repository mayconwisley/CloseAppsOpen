<p align="center">
  <img src="assets/logo.svg" alt="CloseAppsOpen" width="480"/>
</p>

<p align="center">
  <a href="https://github.com/MayconWisley/CloseAppsOpen/actions/workflows/ci.yml">
    <img src="https://github.com/MayconWisley/CloseAppsOpen/actions/workflows/ci.yml/badge.svg" alt="CI"/>
  </a>
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet" alt=".NET 10"/>
  <img src="https://img.shields.io/badge/platform-Windows-0078D4?logo=windows" alt="Windows"/>
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT"/>
</p>

---

Fecha aplicativos abertos no Windows via linha de comando. Suporta modo interativo e uso direto no terminal (ideal para scripts e atalhos no PATH).

## Instalação no PATH

Publique o executável e adicione ao PATH do Windows:

```powershell
dotnet publish CloseAppsOpen\CloseAppsOpen.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

Mova o arquivo `publish\CloseAppsOpen.exe` para uma pasta que esteja no seu PATH (ex: `C:\Tools`) e use de qualquer terminal:

```
closeappsopen --help
```

## Uso

```
closeappsopen [opções]
```

### Opções

| Flag | Descrição |
|------|-----------|
| `-a, --all` | Fecha todos os aplicativos abertos |
| `-s, --shutdown` | Fecha tudo e desliga o PC |
| `-k, --kill <nome>` | Fecha processos que contenham `<nome>` (pode repetir) |
| `-l, --list` | Lista os aplicativos abertos e sai |
| `-e, --exclude <nome>` | Exclui processo pelo nome (pode repetir) |
| `-f, --force` | Não pede confirmação |
| `-t, --timeout <ms>` | Tempo antes de forçar encerramento (padrão: `2000`) |
| `-v, --version` | Exibe a versão |
| `-h, --help` | Exibe a ajuda |

### Exemplos

```bash
# Abre o menu interativo
closeappsopen

# Lista os apps abertos
closeappsopen --list

# Fecha tudo com confirmação
closeappsopen --all

# Fecha tudo sem perguntar
closeappsopen --all --force

# Fecha processos com 'chrome' no nome ou título
closeappsopen --kill chrome

# Fecha tudo exceto o Explorer
closeappsopen --all -e explorer

# Fecha tudo exceto Chrome e Slack
closeappsopen -a -e chrome -e slack

# Aguarda 5 segundos antes de forçar o encerramento
closeappsopen --timeout 5000 --all

# Fecha tudo e desliga o PC (com confirmação)
closeappsopen --shutdown

# Fecha tudo e desliga sem perguntar
closeappsopen --shutdown --force

# Fecha tudo exceto Chrome e desliga
closeappsopen --shutdown -e chrome
```

## Modo interativo

Sem argumentos, abre um menu com a lista de aplicativos abertos:

| Tecla | Ação |
|-------|------|
| `A` | Fecha todos os aplicativos listados |
| `S` | Seleciona quais fechar (por número, nome ou `todos`) |
| `D` | Fecha tudo e desliga o PC |
| `R` | Atualiza a lista |
| `Q` | Sai |

## Comportamento

- Lista apenas janelas visíveis com título
- Tenta fechar graciosamente (`CloseMainWindow`) antes de forçar (`Kill`)
- Aguarda o tempo configurado em `--timeout` antes de forçar
- Pede confirmação antes de fechar (exceto com `--force`)
- Exibe o resultado com quantos foram fechados e quantos falharam
- Retorna código de saída `0` em sucesso e `1` em falha (útil em scripts)

## Requisitos

- Windows
- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) — ou publique como self-contained e não precisa de runtime instalado

## Build e testes

```bash
dotnet build
dotnet test
dotnet run -- --help
```

## CI / CD

O workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml) executa automaticamente em cada push e pull request (usa `CloseAppsOpen.slnx`, formato gerado pelo .NET 10 SDK):

| Etapa | Descrição |
|-------|-----------|
| **Build** | Compila a solução em modo Release |
| **Test** | Executa os 30 testes unitários com xUnit |
| **Publish** | Gera `CloseAppsOpen.exe` self-contained e anexa ao GitHub Release (somente em releases) |

## Estrutura do projeto

```
CloseAppsOpen/
├── CloseAppsOpen/
│   ├── Program.cs          # Ponto de entrada
│   ├── CliArgs.cs          # Parsing de argumentos CLI
│   ├── ProcessManager.cs   # Listagem e encerramento de processos
│   ├── PowerManager.cs     # Desligamento do PC
│   ├── ConsoleUI.cs        # Toda a saída/entrada do console
│   ├── InteractiveMode.cs  # Menu interativo
│   └── app.ico             # Ícone do executável
├── CloseAppsOpen.Tests/
│   └── CliArgsTests.cs     # Testes unitários (xUnit)
├── assets/
│   └── logo.svg            # Logo do projeto
└── tools/
    └── generate-icon.ps1   # Script gerador do app.ico
```
