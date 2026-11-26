#!/usr/bin/env bash
# init.sh — comprueba que {{PROYECTO}} esté en buen estado ANTES de empezar a trabajar (arnés: skill arnes_trabajo).
# Uso: bash init.sh [--build] [--sync-memoria]
#   (sin flags)      rápido: estructura, git, scripts SQL recientes, planes, progress.md, memoria
#   --build          además compila con BUILD_BACK / BUILD_FRONT (salida en carpeta temporal)
#   --sync-memoria   copia la memoria del agente (~/.claude/projects/<clave>/memory) a doc/agentes/memoria
#                    (en modo privado es una copia local, excluida de git: la memoria nunca se comparte)
# Nunca modifica el código del proyecto ni ejecuta SQL.

# ---------------- Configuración del proyecto (editable) ----------------
PROYECTO="{{PROYECTO}}"
MODO="{{MODO}}"                   # privado: el arnés no se versiona (.git/info/exclude) · versionado: se sube con el repo
REPOS="{{REPOS}}"                 # carpetas con .git separadas por espacio; "." si la raíz es el repo
REQUERIDOS="{{REQUERIDOS}}"       # archivos/carpetas clave del proyecto (además de los del arnés)
NO_TOCAR="{{NO_TOCAR}}"           # reglas de otras herramientas: se avisa de que existen, no se tocan
SCRIPTS_DIR="{{SCRIPTS_DIR}}"     # carpeta de scripts de BD numerados; vacío si no hay BD
MEMORIA_EXCLUIR="{{MEMORIA_EXCLUIR}}"  # notas de memoria privadas que no se copian al repo
MEMORIA_DST="doc/agentes/memoria"
PREFIJOS_FICHA="{{PREFIJOS_FICHA}}"   # prefijos de ID de las fichas; EVO (id aleatorio) siempre vale
FICHAS_DIR="{{FICHAS_DIR}}"       # fichas aprobadas (<PREFIJO>-<n>-<titulo>.md)
ADR_DIR="{{ADR_DIR}}"             # ADR aceptados (NNNN-*.md)
FICHAS_BORRADORES="{{FICHAS_BORRADORES_DIR}}"  # fichas en Borrador (modo versionado: = FICHAS_DIR)
ADR_BORRADORES="{{ADR_BORRADORES_DIR}}"        # ADR propuestos     (modo versionado: = ADR_DIR)
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

# ---- Arnés fuera de git (solo modo privado)
if [ "$MODO" = privado ]; then
  echo "-- Arnés excluido de git (modo privado)"
  if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    avi "la raíz no es repo: el arnés de la raíz ya queda fuera de git"
  else
    vis=0; lista=""
    # Si el equipo ya versiona archivos en .claude/, la instalación solo excluyó los del arnés:
    # se comprueban esos y no la carpeta entera (si no, saldría un falso "visible para git").
    if [ -n "$(git ls-files -- .claude | head -1)" ]; then
      CLAUDE_ARNES=".claude/agents/implementador.md .claude/agents/revisor.md .claude/settings.local.json"
    else
      CLAUDE_ARNES=".claude"
    fi
    for f in CLAUDE.md AGENTS.md progress.md Resumen.md init.sh $CLAUDE_ARNES plan-implementacion-*.md \
             "$MEMORIA_DST" "$FICHAS_BORRADORES" "$ADR_BORRADORES"; do
      [ -n "$f" ] && [ -e "$f" ] || continue
      [ "$f" = "$FICHAS_DIR" ] || [ "$f" = "$ADR_DIR" ] && continue
      # visible = versionado, o sin versionar y no ignorado
      if [ -n "$(git ls-files -- "$f" | head -1)" ] || ! git check-ignore -q -- "$f"; then
        vis=$((vis+1)); lista="$lista $f"
      fi
    done
    if [ $vis = 0 ]; then ok "ningún archivo del arnés es visible para git"
    else mal "$vis archivo(s) del arnés visibles para git (revisar .git/info/exclude):$lista"; fi
  fi
fi

# ---- Fichas y ADR
estado_de(){ grep -m1 -E '^> *\*\*Estado:\*\*' "$1" | sed -E 's/^> *\*\*Estado:\*\* *//' | cut -c1-40; }
PREF_RE=$(printf '%s\n' $PREFIJOS_FICHA EVO | grep . | sort -u | paste -sd'|' -)
if { [ -n "$FICHAS_DIR" ] && [ -d "$FICHAS_DIR" ]; } || { [ -n "$ADR_DIR" ] && [ -d "$ADR_DIR" ]; }; then
  echo "-- Fichas y ADR"
  separados=0; [ "$FICHAS_BORRADORES" != "$FICHAS_DIR" ] && separados=1
  for f in "$FICHAS_DIR"/*.md; do
    [ -e "$f" ] || continue
    n=$(basename "$f"); est=$(estado_de "$f")
    echo "$n" | grep -qiE "^($PREF_RE)-" || avi "$n: prefijo fuera de PREFIJOS_FICHA ($PREFIJOS_FICHA)"
    case "$est" in
      Aprobad*) ;;
      *) if [ $separados = 1 ]; then mal "$FICHAS_DIR/$n está en '${est:-sin estado}': ahí solo van fichas Aprobadas (el borrador va en $FICHAS_BORRADORES)"
         else avi "$n — ${est:-sin estado} (pendiente de aprobar)"; fi;;
    esac
  done
  for f in "$ADR_DIR"/*.md; do
    [ -e "$f" ] || continue
    n=$(basename "$f"); est=$(estado_de "$f")
    case "$est" in
      Aceptad*|Sustituid*|Obsolet*) ;;
      *) if [ "$ADR_BORRADORES" != "$ADR_DIR" ]; then mal "$ADR_DIR/$n está en '${est:-sin estado}': ahí solo van ADR aceptados (el propuesto va en $ADR_BORRADORES)"
         else avi "ADR $n — ${est:-sin estado} (pendiente de aceptar)"; fi;;
    esac
  done
  for d in "$FICHAS_BORRADORES" "$ADR_BORRADORES"; do
    [ -n "$d" ] && [ -d "$d" ] || continue
    [ "$d" = "$FICHAS_DIR" ] || [ "$d" = "$ADR_DIR" ] && continue
    for f in "$d"/*.md; do [ -e "$f" ] && avi "borrador pendiente de aprobar: $f"; done
  done
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
