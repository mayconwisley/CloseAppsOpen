# CloseAppsOpen

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
```

## Modo interativo

Sem argumentos, abre um menu com a lista de aplicativos abertos:

| Tecla | Ação |
|-------|------|
| `A` | Fecha todos os aplicativos listados |
| `S` | Seleciona quais fechar (por número, nome ou `todos`) |
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

## Build

```bash
dotnet build
dotnet run
dotnet run -- --help
```
