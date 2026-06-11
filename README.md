# CloseAppsOpen

Aplicativo de console para fechar todos os aplicativos abertos no Windows de forma rápida e prática.

## Como usar

Execute o programa e utilize os atalhos do teclado:

| Tecla | Ação |
|-------|------|
| `A` | Fecha todos os aplicativos listados |
| `S` | Seleciona quais aplicativos fechar |
| `R` | Atualiza a lista de aplicativos |
| `Q` | Sai do programa |

### Fechamento seletivo

Ao pressionar `S`, digite os números dos aplicativos separados por vírgula:

```
> 1,3,5
```

Ou digite `todos` para fechar todos da lista.

## Comportamento

- Lista apenas janelas visíveis com título
- Tenta fechar graciosamente (`CloseMainWindow`) antes de forçar (`Kill`)
- Aguarda até 2 segundos para o processo encerrar antes de forçar
- Exibe confirmação antes de fechar qualquer aplicativo
- Mostra o resultado com quantos foram fechados e quantos falharam

## Requisitos

- Windows
- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)

## Build

```bash
dotnet build
dotnet run
```
