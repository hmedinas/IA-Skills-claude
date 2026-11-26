# hook_inicio_sesion

Hook `SessionStart`: al abrir, retomar, limpiar o compactar la sesión, **si el proyecto tiene `init.sh` en la raíz** lo ejecuta.
Su salida (estado de git, scripts pendientes, `progress.md`) entra como contexto para Claude. Si no hay `init.sh`, no hace nada.

Guía del componente: [docs/03-hooks.md](../docs/03-hooks.md).

> ⚠️ **¿Usas `arnes_trabajo`? Entonces no necesitas este plugin.**
> `/arnes_trabajo instalar` ya añade al `.claude/settings.json` del proyecto un hook `SessionStart` que ejecuta `init.sh`.
> Con los dos, `init.sh` se ejecutaría **dos veces** en cada sesión. Para evitarlo, este plugin no hace nada en los proyectos
> cuyo `.claude/settings.json` (o `settings.local.json`) ya lanza `init.sh`, y lo dice en una línea. Aun así, lo limpio es elegir uno:
> - **Proyecto compartido con un equipo** → el hook del arnés (viaja con el repo; nadie tiene que instalar nada). Quita el plugin:
>   `claude plugin uninstall hook_inicio_sesion@ia-skills-claude`.
> - **Proyectos tuyos sin arnés que tienen su propio `init.sh`** → este plugin, para no tocar el `settings.json` de cada uno.

## Instalar
**Plugin** (todos tus proyectos):
```bash
claude plugin marketplace add hmedinas/IA-Skills-claude   # una vez
claude plugin install hook_inicio_sesion@ia-skills-claude
```
**A mano en un proyecto:** añadir a `<proyecto>/.claude/settings.json`, sin borrar lo que haya:
```json
{ "hooks": { "SessionStart": [ { "matcher": "startup|resume|clear|compact",
  "hooks": [ { "type": "command", "shell": "bash",
               "command": "bash \"${CLAUDE_PROJECT_DIR:-.}/init.sh\"", "timeout": 60 } ] } ] } }
```
- `"shell": "bash"`: en Windows obliga a usar Git Bash aunque la shell por defecto sea PowerShell.
- `${CLAUDE_PROJECT_DIR:-.}`: encuentra `init.sh` aunque Claude se arranque desde una subcarpeta.

## Probar
```bash
CLAUDE_PROJECT_DIR="/ruta/al/proyecto" bash scripts/inicio-sesion.sh
```
