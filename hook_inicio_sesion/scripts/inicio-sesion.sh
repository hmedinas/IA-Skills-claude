#!/usr/bin/env bash
# inicio-sesion.sh — hook SessionStart del plugin hook_inicio_sesion.
# Si el proyecto tiene init.sh en la raíz, lo ejecuta. En SessionStart, lo que se imprime entra como contexto para Claude.
# Si no hay init.sh, no hace nada. Nunca bloquea el arranque (siempre exit 0).
# Si el proyecto ya lanza init.sh con su propio hook (lo instala /arnes_trabajo), no lo repite: evita ejecutarlo dos veces.
raiz="${CLAUDE_PROJECT_DIR:-$(pwd)}"
[ -f "$raiz/init.sh" ] || exit 0
for s in "$raiz/.claude/settings.json" "$raiz/.claude/settings.local.json"; do
  if [ -f "$s" ] && grep -q 'init\.sh' "$s"; then
    echo "hook_inicio_sesion: el proyecto ya ejecuta init.sh desde ${s#"$raiz"/}; no se repite."
    exit 0
  fi
done
(cd "$raiz" && bash init.sh) 2>&1 | head -80
exit 0
