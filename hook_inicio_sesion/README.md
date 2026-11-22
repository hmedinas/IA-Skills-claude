# hook_inicio_sesion

Hook `SessionStart`: al abrir, retomar, limpiar o compactar la sesión, **si el proyecto tiene `init.sh` en la raíz** lo ejecuta.
Su salida (estado de git, scripts pendientes, `progress.md`) entra como contexto para Claude. Si no hay `init.sh`, no hace nada.

Guía del componente: [docs/03-hooks.md](../docs/03-hooks.md).

## Instalar
**Plugin** (todos tus proyectos):
```bash
claude plugin install hook_inicio_sesion@ia-skills-claude
```
**A mano en un proyecto:** añadir a `<proyecto>/.claude/settings.json`, sin borrar lo que haya:
```json
{ "hooks": { "SessionStart": [ { "matcher": "startup|resume|clear|compact",
  "hooks": [ { "type": "command", "command": "bash init.sh", "timeout": 60 } ] } ] } }
```

⚠️ **No uses las dos formas a la vez.** El arnés (`/arnes_trabajo instalar`) ya mete este hook en el `settings.json` del proyecto,
y si además instalas el plugin, `init.sh` se ejecuta dos veces. El plugin conviene si quieres el arranque en todos tus proyectos
sin tocar el `settings.json` de cada uno.

## Probar
```bash
CLAUDE_PROJECT_DIR="/ruta/al/proyecto" bash scripts/inicio-sesion.sh
```
