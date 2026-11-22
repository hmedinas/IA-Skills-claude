# agente_implementador

Agente que ejecuta un encargo concreto y acotado. Sigue `AGENTS.md` y `CLAUDE.md` del proyecto y deja los builds en verde.
No decide el diseño, no hace commit y no documenta: eso lo hace la sesión principal. Modelo: Sonnet.

Guía del componente: [docs/02-agentes.md](../docs/02-agentes.md).

## Instalar
**Plugin** (todos tus proyectos):
```bash
claude plugin marketplace add hmedinas/IA-Skills-claude   # una vez
claude plugin install agente_implementador@ia-skills-claude
```
**A mano:** copiar `agents/implementador.md` a `~/.claude/agents/` (personal) o a `<proyecto>/.claude/agents/` (proyecto).

Comprobar con `/agents`.

## Usar
Pídelo con un encargo cerrado:
> Usa el agente implementador: en `X.cs` añade el método Y siguiendo el patrón de `Z.cs`.
> Criterio: `dotnet build` en verde y el endpoint `/api/...` devuelve `Response<T>`.

Ya va incluido en el arnés (`/arnes_trabajo instalar`). Este componente es para usarlo suelto.
