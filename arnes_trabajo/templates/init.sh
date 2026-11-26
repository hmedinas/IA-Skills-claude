#!/usr/bin/env bash
# init.sh — comprueba que {{PROYECTO}} esté en buen estado ANTES de empezar a trabajar (arnés: skill arnes_trabajo).
# Uso: bash init.sh [--build] [--sync-memoria]
#   (sin flags)      rápido: estructura, git, scripts SQL recientes, planes, progress.md, memoria
#   --build          además compila con BUILD_BACK / BUILD_FRONT (salida en carpeta temporal)
#   --sync-memoria   copia la memoria del agente (~/.claude/projects/<clave>/memory) a doc/agentes/memoria
# Nunca modifica el código del proyecto ni ejecuta SQL.

# ---------------- Configuración del proyecto (editable) ----------------
PROYECTO="{{PROYECTO}}"
REPOS="{{REPOS}}"                 # carpetas con .git separadas por espacio; "." si la raíz es el repo
REQUERIDOS="{{REQUERIDOS}}"       # archivos/carpetas clave del proyecto (además de los del arnés)
NO_TOCAR="{{NO_TOCAR}}"           # reglas de otras herramientas: se avisa de que existen, no se tocan
SCRIPTS_DIR="{{SCRIPTS_DIR}}"     # carpeta de scripts de BD numerados; vacío si no hay BD
MEMORIA_EXCLUIR="{{MEMORIA_EXCLUIR}}"  # notas de memoria privadas que no se copian al repo
MEMORIA_DST="doc/agentes/memoria"
FICHAS_DIR="{{FICHAS_DIR}}"       # fichas de requerimiento (TCK-/REQ-/EVO-*.md)
ADR_DIR="{{ADR_DIR}}"             # decisiones de arquitectura (NNNN-*.md)
# Builds de verificación: siempre a "$OUT" (carpeta temporal) para no pisar lo que el usuario tiene abierto.
BUILD_BACK='{{BUILD_BACK}}'
BUILD_FRONT='{{BUILD_FRONT}}'
# ------------------------------------------------------------------------

ROOT="$(cd "$(dirname "$0")" && pwd)"; cd "$ROOT" || exit 1
BUILD=0; SYNC=0
for a in "$@"; do case "$a" in --build) BUILD=1;; --sync-memoria) SYNC=1;; esac; done
FALLOS=0; ok(){ echo "  OK  $1"; }; mal(){ echo "  !!  $1"; FALLOS=$((FALLOS+1)); }; avi(){ echo "  ..  $1"; }
excluida(){ for x in $MEMORIA_EXCLUIR; do [ "$1" = "$x" ] && return 0; done; return 1; }

echo "== $PROYECTO init ($(date '+%d/%m/%Y %H:%M'))"

echo "-- Estructura"
for f in AGENTS.md CLAUDE.md progress.md Resumen.md .claude/settings.json .claude/agents/implementador.md \
         .claude/agents/revisor.md "$MEMORIA_DST/MEMORY.md" $FICHAS_DIR $ADR_DIR $SCRIPTS_DIR $REQUERIDOS; do
  [ -e "$f" ] && ok "$f" || mal "falta $f"; done
for d in $NO_TOCAR; do [ -e "$d" ] && ok "$d (no tocar)"; done

echo "-- Git (commit/push solo si el usuario lo pide)"
[ -e .git ] || avi "la raíz no es repo"
for d in $REPOS; do
  nombre="$d"; [ "$d" = "." ] && nombre="(raíz)"
  [ -e "$d/.git" ] || { mal "$nombre no es repo"; continue; }
  n=$(git -C "$d" status --porcelain | wc -l | tr -d ' ')
  GIT_TERMINAL_PROMPT=0 timeout 15 git -C "$d" fetch -q 2>/dev/null || avi "$nombre: fetch no disponible (sin red o sin credenciales)"
  ah=$(git -C "$d" rev-list --count @{u}..HEAD 2>/dev/null || echo "?"); be=$(git -C "$d" rev-list --count HEAD..@{u} 2>/dev/null || echo "?")
  msg="$nombre: $(git -C "$d" branch --show-current) · $n cambios sin commit · $ah sin subir · $be por bajar"
  if [ "$be" != "0" ] && [ "$be" != "?" ]; then mal "$msg"; else avi "$msg"; fi
done

if [ -n "$SCRIPTS_DIR" ] && [ -d "$SCRIPTS_DIR" ]; then
  echo "-- Scripts de $SCRIPTS_DIR/ modificados en los últimos 3 días (los ejecuta el usuario)"
  find "$SCRIPTS_DIR" -name '*.sql' -mtime -3 2>/dev/null | sort | tail -15 | sed 's/^/  ..  /'
  ult=$(ls "$SCRIPTS_DIR" 2>/dev/null | grep -oE '^[0-9]+' | sort -n | tail -1)
  [ -n "$ult" ] && avi "último número en $SCRIPTS_DIR/: $ult (siguiente: $(printf '%02d' $((10#$ult+1))))"
fi

planes=$(ls plan-implementacion-*.md 2>/dev/null)
if [ -n "$planes" ]; then
  echo "-- Planes de implementación"
  for p in $planes; do
    est=$(grep -m1 -iE '^\*\*Estado:\*\*|^> *Estado:|^\*\*Status:\*\*' "$p" | sed -E 's/.*(Estado|Status):\**[: ]*//I' | cut -c1-80)
    avi "$p — ${est:-sin línea de estado}"
  done
fi

if [ -n "$FICHAS_DIR" ] && [ -d "$FICHAS_DIR" ]; then
  borr=$(grep -lE '^> *\*\*Estado:\*\* *Borrador' "$FICHAS_DIR"/*.md 2>/dev/null)
  if [ -n "$borr" ]; then
    echo "-- Fichas en Borrador (pendientes de aprobar por el usuario)"
    echo "$borr" | head -10 | sed 's/^/  ..  /'
  fi
fi

echo "-- progress.md"
if [ -f progress.md ]; then
  grep -m1 'Última actualización' progress.md | sed 's/^> */  ..  /'
  awk '/^## Siguiente paso/{f=1;next}/^## /{f=0}f&&/\[ \]/' progress.md | head -5 | sed 's/^/  ->/'
  l=$(wc -l < progress.md | tr -d ' '); [ "$l" -gt 60 ] && avi "progress.md tiene $l líneas (> 60): pasar lo histórico a Resumen.md"
fi

# Clave de proyecto de Claude Code: ruta absoluta con todo carácter no alfanumérico cambiado por "-"
CLAVE=$(printf '%s' "$(pwd -W 2>/dev/null || pwd)" | sed 's/[^A-Za-z0-9]/-/g')
MEM="$HOME/.claude/projects/$CLAVE/memory"
echo "-- Memoria"
if [ -d "$MEM" ]; then
  mkdir -p "$MEMORIA_DST"
  filtrar(){ if [ -n "$MEMORIA_EXCLUIR" ]; then grep -vF -e "$(echo $MEMORIA_EXCLUIR | tr ' ' '\n')" "$1"; else cat "$1"; fi; }
  if [ $SYNC = 1 ]; then
    for f in "$MEM"/*.md; do b=$(basename "$f"); excluida "$b" && continue
      if [ "$b" = MEMORY.md ]; then filtrar "$f" > "$MEMORIA_DST/$b"; else cp "$f" "$MEMORIA_DST/"; fi; done
    ok "memoria copiada a $MEMORIA_DST${MEMORIA_EXCLUIR:+ (excluidas: $MEMORIA_EXCLUIR)}"
  else
    dif=0; for f in "$MEM"/*.md; do b=$(basename "$f"); excluida "$b" && continue
      if [ "$b" = MEMORY.md ]; then filtrar "$f" | cmp -s - "$MEMORIA_DST/$b" || dif=$((dif+1))
      else cmp -s "$f" "$MEMORIA_DST/$b" || dif=$((dif+1)); fi; done
    if [ $dif = 0 ]; then ok "$MEMORIA_DST al día"; else avi "$dif nota(s) de memoria distintas a $MEMORIA_DST (usar --sync-memoria)"; fi
  fi
else
  avi "sin memoria del agente en $MEM (normal si aún no se ha guardado ninguna nota)"
fi

if [ $BUILD = 1 ]; then
  OUT="${TMPDIR:-/tmp}/arnes-build-$CLAVE"; mkdir -p "$OUT"; export OUT
  for parte in back front; do
    cmd="$BUILD_BACK"; [ $parte = front ] && cmd="$BUILD_FRONT"
    echo "-- Build $parte"
    if [ -z "$cmd" ]; then avi "sin comando de build para $parte"; continue; fi
    if (eval "$cmd") > "$OUT/$parte.log" 2>&1; then ok "$parte compilado"
    else mal "build $parte (ver $OUT/$parte.log)"; grep -m5 -iE ' error |error:' "$OUT/$parte.log"; fi
  done
fi

if [ $FALLOS = 0 ]; then echo "== LISTO — siguiente: leer progress.md y AGENTS.md"; else echo "== $FALLOS problema(s) — revisar antes de empezar"; fi
exit 0
