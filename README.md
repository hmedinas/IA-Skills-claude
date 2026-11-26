# IA-Skills-claude

Marketplace personal de componentes para [Claude Code](https://claude.com/claude-code): skills, agentes, hooks y MCP.
Cada componente es un plugin que se instala por separado. Las guías de cómo se crea e instala cada tipo están en [`docs/`](docs/README.md).

## Instalar
Manual completo, paso a paso: [docs/00-instalacion.md](docs/00-instalacion.md).

```
claude plugin marketplace add hmedinas/IA-Skills-claude     # una vez
claude plugin install <componente>@ia-skills-claude
```

## Componentes

### Skills
- **base_net10**: genera un backend .NET 10 (Clean Architecture 6 capas, MediatR, Dapper, JWT, Serilog, Swagger).
- **base_nuxt4**: genera un starter Nuxt 4 (PrimeVue + Tailwind + Pinia, login mock, theming, dark mode).
- **base_flutter**: genera una app Flutter (Clean Architecture feature-first, Provider o Riverpod, Dio + mocks, theming, routing, login).
- **arnes_trabajo**: arnés de trabajo con agentes. Instala o revisa AGENTS.md, CLAUDE.md, progress.md, Resumen.md, init.sh (hook SessionStart), los agentes y la memoria. También diseña planes por etapas con aprobación humana, los implementa con gates y cierra documentando, con una **ficha de requerimiento** por tarea (prefijos configurables: `TCK-`, `RFC-`, `REQ-`, `EVO-`…) y ADR para las decisiones que perduran. En repos compartidos se instala en **modo privado** (`.git/info/exclude`): cada uno tiene su arnés y solo se comparten las fichas y ADR aprobados. Uso: `/arnes_trabajo instalar | revisar | diseno <modulo> | implementar <modulo> | cierre`. [Guía](docs/07-arnes-trabajo.md)

### Agentes
- **agente_implementador**: ejecuta un encargo concreto siguiendo AGENTS.md y CLAUDE.md y deja los builds en verde (Sonnet). [README](agente_implementador/README.md)
- **agente_revisor**: revisa el diff (solo lectura) buscando bugs, seguridad y reglas rotas (Opus). [README](agente_revisor/README.md)

### Hooks
- **hook_inicio_sesion**: `SessionStart`; ejecuta el `init.sh` del proyecto si existe. **No hace falta si usas `arnes_trabajo`**, que ya instala ese hook. [README](hook_inicio_sesion/README.md)
- **hook_solo_select**: `PreToolUse`; bloquea comandos SQL que no sean SELECT (sqlcmd, Invoke-Sqlcmd, psql, mysql…). [README](hook_solo_select/README.md)

### MCP
- **mcp_sql_lectura**: servidor MCP en C# / .NET 10 de **solo lectura** para SQL Server. Herramientas `consultar`, `listar_tablas`, `describir_tabla`, `ver_definicion` y `buscar_en_codigo`, con tres barreras: validador, transacción con rollback y conexión ReadOnly. [README](mcp_sql_lectura/README.md)

## Guías
| Guía | Contenido |
|---|---|
| [00 · Manual de instalación](docs/00-instalacion.md) | Paso a paso: requisitos, marketplace, arnés, comprobar, actualizar y desinstalar |
| [Índice y mapa](docs/README.md) | Qué es cada componente y cuál necesitas |
| [01 · Skills y comandos](docs/01-skills-y-comandos.md) | `SKILL.md`, cabecera, `$ARGUMENTS`, instalación |
| [02 · Agentes](docs/02-agentes.md) | Subagentes: rol, modelo, herramientas |
| [03 · Hooks](docs/03-hooks.md) | Eventos, códigos de salida, bloquear acciones |
| [04 · MCP](docs/04-mcp.md) | Servidores de herramientas; ejemplo en C# |
| [05 · Plugins y marketplace](docs/05-plugins-y-marketplace.md) | Empaquetar, validar y publicar |
| [06 · Settings y modos](docs/06-settings-y-modos.md) | Permisos, modos de trabajo y estilo de respuesta |
| [07 · Arnés de trabajo](docs/07-arnes-trabajo.md) | Instalación, modo privado para repos compartidos, los cinco modos, el cierre y las fichas |
| [06 · Settings y modos](docs/06-settings-y-modos.md) | Permisos, modos (`Shift+Tab`), estilos de salida |

## Publicar un cambio
1. `claude plugin validate .`: debe terminar en `Validation passed`. Los avisos de kebab-case son esperados.
2. Subir `version` en el `plugin.json` del componente.
3. Actualizar este README, `docs/README.md` y la web (`IA-Web/web/public/data/catalog.json`).
4. Commit y push. Quien lo tenga instalado lo recibe con `claude plugin marketplace update ia-skills-claude`.
