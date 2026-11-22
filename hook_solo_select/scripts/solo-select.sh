#!/usr/bin/env bash
# solo-select.sh — hook PreToolUse (Bash / PowerShell) del plugin hook_solo_select.
# Si el comando que Claude quiere ejecutar usa un cliente SQL y contiene algo distinto de SELECT
# (o ejecuta un archivo .sql), lo BLOQUEA con exit 2. El motivo (stderr) le llega a Claude.
# Todo lo que no use un cliente SQL pasa sin tocar (exit 0).
#
# Prueba manual:
#   echo '{"tool_name":"Bash","tool_input":{"command":"sqlcmd -Q \"DELETE FROM T1\""}}' | bash solo-select.sh; echo $?

entrada="$(cat)"

# 1. Sacar tool_input.command del JSON (python3 / python / node; si no hay ninguno, se usa el JSON entero).
extraer='import sys,json
try: print(json.load(sys.stdin).get("tool_input",{}).get("command",""))
except Exception: sys.exit(1)'
cmd=""
for py in python3 python; do
  if command -v "$py" >/dev/null 2>&1; then cmd="$(printf '%s' "$entrada" | "$py" -c "$extraer" 2>/dev/null)" && break; fi
done
if [ -z "$cmd" ] && command -v node >/dev/null 2>&1; then
  cmd="$(printf '%s' "$entrada" | node -e 'let s="";process.stdin.on("data",d=>s+=d).on("end",()=>{try{console.log((JSON.parse(s).tool_input||{}).command||"")}catch(e){process.exit(1)}})' 2>/dev/null)"
fi
[ -z "$cmd" ] && cmd="$entrada"

# 2. ¿Usa un cliente SQL? Si no, no es asunto de este hook.
printf '%s' "$cmd" | grep -qiE '(^|[^[:alnum:]_-])(sqlcmd|osql|isql|bcp|invoke-sqlcmd|psql|mysql|mariadb|sqlite3|mssql-cli|sqlpackage)([^[:alnum:]_-]|$)' || exit 0

bloquear(){
  echo "Bloqueado por hook_solo_select: $1" >&2
  echo "La base de datos es de SOLO LECTURA para Claude: solo se permiten consultas SELECT escritas en el propio comando." >&2
  echo "Si hace falta cambiar algo, escribe el script en la carpeta de scripts del proyecto y que lo ejecute el usuario." >&2
  exit 2
}

# 3. Palabras de escritura o de ejecución (palabra completa, sin distinguir mayúsculas).
palabra=$(printf '%s' "$cmd" | grep -oiwE 'insert|update|delete|merge|truncate|create|alter|drop|grant|revoke|deny|exec|execute|backup|restore|dbcc|bulk|openrowset|opendatasource|shutdown|kill|reconfigure|attach|detach' | head -1)
[ -n "$palabra" ] && bloquear "el comando contiene '$palabra'."
printf '%s' "$cmd" | grep -qiE '\bselect\b.*\binto\b' && bloquear "SELECT ... INTO crea tablas."
printf '%s' "$cmd" | grep -qiE '(^|[^[:alnum:]_])(sp|xp)_[[:alnum:]_]+' && bloquear "llama a un procedimiento del sistema (sp_/xp_)."
# Ejecutar un archivo: sqlcmd/osql -i, Invoke-Sqlcmd -InputFile, psql -f/--file, mysql < archivo, sqlite3 .read
# (ojo: en sqlcmd "-f 65001" es la página de códigos, no un archivo; -f solo cuenta para psql)
printf '%s' "$cmd" | grep -qiE '(^|[[:space:]])(-i|-inputfile)([[:space:]]|=|$)|<[[:space:]]*[^[:space:]]+\.sql|\.read[[:space:]]' \
  && bloquear "ejecuta un archivo de script (no se puede comprobar su contenido)."
printf '%s' "$cmd" | grep -qiE '(^|[^[:alnum:]_])psql([[:space:]].*)?[[:space:]](-f|--file)([[:space:]]|=|$)' \
  && bloquear "psql -f ejecuta un archivo de script."
# bcp ... in  (carga datos)
printf '%s' "$cmd" | grep -qiE '(^|[^[:alnum:]_])bcp[[:space:]].*[[:space:]]in[[:space:]]' && bloquear "bcp in carga datos."

exit 0
