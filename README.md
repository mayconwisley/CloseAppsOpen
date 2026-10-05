<p align="center">
  <img src="assets/logo.svg" alt="CloseAppsOpen" width="480"/>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet" alt=".NET 10"/>
  <img src="https://img.shields.io/badge/platform-Windows-0078D4?logo=windows" alt="Windows"/>
  <img src="https://img.shields.io/badge/license-MIT-green" alt="MIT"/>
</p>

---

Fecha aplicativos abertos no Windows via linha de comando. Suporta modo interativo e uso direto no terminal (ideal para scripts e atalhos no PATH).

## Instalação no PATH

Baixe o ZIP da [versão mais recente](https://github.com/mayconwisley/CloseAppsOpen/releases/latest), extraia `CloseAppsOpen.exe` e adicione a pasta ao PATH do Windows. O executável distribuído é independente da instalação do .NET.

Para compilar a partir do código-fonte:

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
| `-f, --force` | Mata direto (`Kill`), sem confirmação nem fechamento gentil |
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

# Mata tudo na hora (Kill direto), sem perguntar nem dar chance de salvar
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
- Por padrão tenta fechar graciosamente (`CloseMainWindow`) antes de forçar (`Kill`)
- Aguarda o tempo configurado em `--timeout` antes de forçar
- Com `--force`, mata direto (`Kill`) sem `CloseMainWindow` — não pede confirmação **e** não dispara diálogos de "salvar?" (descarta trabalho não salvo)
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

## Publicar uma release

A compilação, os testes e o empacotamento acontecem nesta máquina. O GitHub recebe somente a tag e os artefatos prontos; não há workflow de build no GitHub Actions.

Pré-requisitos: Windows, .NET 10 SDK, Git e [GitHub CLI](https://cli.github.com/) autenticado com acesso de escrita ao repositório (`gh auth login`). Atualize `<Version>` em `CloseAppsOpen/CloseAppsOpen.csproj`, faça commit e envie a branch padrão ao `origin` antes de publicar.

Para preparar e verificar os arquivos localmente, sem criar tag ou release:

```powershell
.\tools\publish-release.ps1 -Tag v1.0.0 -PrepareOnly
```

Para publicar:

```powershell
.\tools\publish-release.ps1 -Tag v1.0.0
```

O script exige que a tag corresponda à versão do projeto e que a branch padrão local esteja sincronizada com o `origin`. Ele executa os testes, publica um executável `win-x64` independente do runtime .NET, verifica a versão do binário, cria o ZIP com a licença MIT e o arquivo SHA-256 em `.artifacts/releases/<tag>/`, envia uma tag anotada e cria a GitHub Release com esses dois arquivos. Se o envio da tag funcionar, mas a criação da release falhar, execute o mesmo comando novamente após corrigir a falha.

## Contribuindo

Correções e melhorias são bem-vindas. Consulte o [guia de contribuição](CONTRIBUTING.md) antes de abrir um pull request.

## Licença

Este projeto é distribuído sob a [licença MIT](LICENSE.txt). O aviso de copyright e o texto da licença devem acompanhar as cópias distribuídas.

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
