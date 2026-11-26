# Guías — Cómo se crea y se instala cada componente de Claude Code

> Guías para crear e instalar cada tipo de componente, y para usar los de este repositorio. Cada guía tiene las mismas secciones:
> **qué es · cuándo usarlo · dónde vive · cómo se crea · ejemplo real · cómo se instala · errores típicos**.
> Verificado contra la documentación oficial (Claude Code 2.1.x, noviembre 2026).

## Mapa rápido

| Componente | Qué es, en una frase | Dónde vive | Guía |
|---|---|---|---|
| **Skill / comando** | Instrucciones que Claude sigue cuando escribes `/nombre` o cuando detecta que aplican | `.claude/skills/<nombre>/SKILL.md` | [01](01-skills-y-comandos.md) |
| **Agente** | Otro Claude con rol, modelo y herramientas propios, al que el principal le pasa un encargo | `.claude/agents/<nombre>.md` | [02](02-agentes.md) |
| **Hook** | Un comando **tuyo** que se ejecuta automáticamente en un evento (arrancar, antes de usar una herramienta…) | `.claude/settings.json` | [03](03-hooks.md) |
| **MCP** | Un programa que le da a Claude herramientas nuevas (tu BD, Jira, Azure DevOps…) | Proceso aparte, registrado en `.mcp.json` | [04](04-mcp.md) |
| **Plugin** | Un paquete con skills, agentes, hooks y MCP, que se instala de una vez | Repo con `.claude-plugin/` | [05](05-plugins-y-marketplace.md) |
| **Settings y modos** | Permisos, modo de trabajo (pedir permiso, editar solo, solo planificar) y estilo de respuesta | `settings.json`, `Shift+Tab` | [06](06-settings-y-modos.md) |
| **Arnés de trabajo** | La skill `arnes_trabajo`: instalación, modo privado para repos compartidos, los cinco modos, el cierre y las fichas | Tu proyecto (`AGENTS.md`, `init.sh`, `doc/fichas/`…) | [07](07-arnes-trabajo.md) |

## ¿Cuál necesito?

```
¿Claude no sabe CÓMO hacer algo a tu manera? ............ Skill
¿Quieres repartir el trabajo o aislar un rol? ............ Agente
¿Algo tiene que pasar SIEMPRE, se acuerde Claude o no? ... Hook
¿Claude no PUEDE acceder a algo (BD, API, servicio)? ..... MCP
¿Quieres instalar varias piezas juntas en otro equipo? ... Plugin
¿Quieres cambiar cuánto pregunta o cómo responde? ........ Settings / modos
```

## Ámbitos (válido para todos)

| Ámbito | Carpeta | Para quién |
|---|---|---|
| **Personal** | `~/.claude/…` (Windows: `C:\Users\<tú>\.claude\…`) | Tú, en todos tus proyectos |
| **Proyecto** | `<proyecto>/.claude/…` (se sube a git) | Todos los que trabajen en ese repo |
| **Proyecto, solo tú** | `<proyecto>/.claude/settings.local.json` (no se sube) | Tú, en ese proyecto |
| **Plugin** | Lo instala `claude plugin install` | Quien lo instale desde el marketplace |

## Componentes de este repositorio

| Componente | Tipo | Instalar |
|---|---|---|
| `arnes_trabajo` | Skill (incluye todo el arnés). Guía: [07](07-arnes-trabajo.md) | `claude plugin install arnes_trabajo@ia-skills-claude` |
| `agente_implementador` | Agente | `claude plugin install agente_implementador@ia-skills-claude` |
| `agente_revisor` | Agente | `claude plugin install agente_revisor@ia-skills-claude` |
| `hook_inicio_sesion` | Hook `SessionStart` (no hace falta si usas `arnes_trabajo`) | `claude plugin install hook_inicio_sesion@ia-skills-claude` |
| `hook_solo_select` | Hook `PreToolUse` | `claude plugin install hook_solo_select@ia-skills-claude` |
| `mcp_sql_lectura` | MCP (C#, SQL Server solo lectura) | `claude plugin install mcp_sql_lectura@ia-skills-claude` |
| `base_net10`, `base_nuxt4`, `base_flutter` | Skills generadoras | `claude plugin install <nombre>@ia-skills-claude` |

Antes de instalar cualquiera, una sola vez: `claude plugin marketplace add hmedinas/IA-Skills-claude`.
Cada componente se puede instalar también **a mano**, copiando sus archivos. Cada guía explica cómo.

## Orden recomendado para aprender
1. Una skill sencilla ([01](01-skills-y-comandos.md)): solo markdown, se ve el resultado al momento.
2. El hook `hook_solo_select` ([03](03-hooks.md)): aprendes los eventos y refuerzas una regla real.
3. Un agente nuevo ([02](02-agentes.md)), por ejemplo uno que solo documenta.
4. Un MCP en C# ([04](04-mcp.md)): el más complejo y el más potente. Ejemplo completo: `mcp_sql_lectura`.
5. Empaquetar lo anterior como plugin ([05](05-plugins-y-marketplace.md)).

## Documentación oficial
- Índice completo: https://code.claude.com/docs/llms.txt
- Skills: https://code.claude.com/docs/en/skills · Subagentes: https://code.claude.com/docs/en/sub-agents
- Hooks: https://code.claude.com/docs/en/hooks · MCP: https://code.claude.com/docs/en/mcp
- Plugins: https://code.claude.com/docs/en/plugins-reference · Marketplaces: https://code.claude.com/docs/en/plugin-marketplaces
- Settings: https://code.claude.com/docs/en/settings
