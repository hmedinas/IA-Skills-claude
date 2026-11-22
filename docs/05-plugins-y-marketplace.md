# 05 · Plugins y marketplace

## Qué es
- **Plugin:** una carpeta que **empaqueta** componentes (skills, agentes, hooks, MCP, estilos) para instalarlos de una vez.
  Todo lo que trae queda con el prefijo del plugin: el agente `revisor` del plugin `agente_revisor` aparece como `agente_revisor:revisor`.
- **Marketplace:** un repo con un `.claude-plugin/marketplace.json` que **lista** plugins y dice de dónde sacar cada uno.
  Este repositorio (`IA-Skills-claude`) es tu marketplace.

## Cuándo usarlo
- Llevar tus componentes a otro equipo o a otro proyecto con un comando.
- Compartirlos con otras personas.
- Para algo de un solo proyecto basta con `.claude/` del proyecto; no hace falta plugin.

## Estructura de un plugin
```text
mi-plugin/
├── .claude-plugin/
│   └── plugin.json        ← opcional (nombre, versión, autor). Solo esto va dentro de .claude-plugin/
├── skills/
│   └── <nombre>/SKILL.md  ← o SKILL.md en la raíz si el plugin es UNA sola skill
├── agents/
│   └── <nombre>.md
├── hooks/
│   └── hooks.json         ← mismo formato que en settings.json, envuelto en {"hooks": {...}}
├── scripts/               ← lo que llaman los hooks: "${CLAUDE_PLUGIN_ROOT}"/scripts/x.sh
└── .mcp.json              ← servidores MCP
```

`plugin.json` mínimo:
```json
{
  "name": "agente_revisor",
  "version": "1.0.0",
  "description": "Agente revisor de diffs",
  "author": { "name": "Hugo Medina" }
}
```
- `name` es el único campo obligatorio. Lo recomendado es kebab-case (`agente-revisor`); con guion bajo funciona, pero el validador avisa.
  En este repositorio se mantiene el guion bajo por coherencia con `base_net10` y el resto.
- `version`: si la pones, quien lo instaló se queda en esa versión hasta que la cambies. **Súbela en cada cambio publicado.**

## El marketplace (`.claude-plugin/marketplace.json`)
```json
{
  "name": "ia-skills-claude",
  "owner": { "name": "Hugo Medina" },
  "plugins": [
    { "name": "agente_revisor", "source": "./agente_revisor", "description": "…" }
  ]
}
```
- `source` es una **ruta relativa desde la raíz del repo y empieza por `./`**. Otros orígenes:
  `{ "source": "github", "repo": "usuario/repo" }` y `{ "source": "git-subdir", "url": "usuario/repo", "path": "carpeta" }`.
- ⚠️ `{"source": "local", "path": "x"}` **no es un formato válido**. Así estaba este marketplace hasta el 2026-11-22.
- El `name` de la entrada debe ser igual al `name` del `plugin.json`.

## Comandos
```bash
claude plugin validate ./agente_revisor              # validar un plugin
claude plugin validate .                             # validar el marketplace entero (desde la raíz del repo)
claude plugin marketplace add hmedinas/IA-Skills-claude   # registrar el marketplace (una vez)
claude plugin marketplace add ./                     # o en local, para probar antes de subir
claude plugin install agente_revisor@ia-skills-claude
claude plugin list                                   # instalados y estado
claude plugin details agente_revisor                 # qué componentes trae
claude plugin marketplace update ia-skills-claude    # traer los cambios publicados
claude plugin uninstall agente_revisor@ia-skills-claude
```
Dentro de la sesión: `/plugin` (panel), `/reload-plugins` (recargar tras editar).

## Cómo publicar un componente nuevo (receta)
1. Crear la carpeta del plugin en la raíz del repositorio, con la estructura de arriba.
2. Añadir la entrada en `.claude-plugin/marketplace.json`.
3. `claude plugin validate .` → debe terminar en `Validation passed` (los avisos de kebab-case son esperados).
4. Probar en local: `claude plugin marketplace add ./` → `claude plugin install <nombre>@ia-skills-claude`.
5. Añadirlo al `README.md`, a `docs/README.md` y a la web (`IA-Web/web/public/data/catalog.json`).
6. Commit y push. Los demás lo reciben con `claude plugin marketplace update`.

## Errores típicos
| Síntoma | Causa |
|---|---|
| `Plugin "x" not found in marketplace` | El nombre de la entrada y el de `plugin.json` no coinciden |
| `Source path does not exist` | `source` mal escrito o sin `./` |
| Instalado pero sin componentes | Carpetas dentro de `.claude-plugin/` (van en la raíz del plugin), o `SKILL.md` mal ubicado |
| No llegan los cambios | `version` sin subir, o falta `claude plugin marketplace update` |
| Un `CLAUDE.md` dentro del plugin no hace nada | Normal: no se carga. Las instrucciones van en una skill |
