#!/usr/bin/env bash
# inicio-sesion.sh — hook SessionStart del plugin hook_inicio_sesion.
# Si el proyecto tiene init.sh en la raíz, lo ejecuta. En SessionStart, lo que se imprime entra como contexto para Claude.
# Si no hay init.sh, no hace nada. Nunca bloquea el arranque (siempre exit 0).
raiz="${CLAUDE_PROJECT_DIR:-$(pwd)}"
if [ -f "$raiz/init.sh" ]; then
  (cd "$raiz" && bash init.sh) 2>&1 | head -80
fi
exit 0
