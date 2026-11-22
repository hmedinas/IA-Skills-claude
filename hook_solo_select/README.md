# hook_solo_select

Hook `PreToolUse` (herramientas `Bash` y `PowerShell`) que hace **cumplir por código** la regla "contra la BD solo SELECT".
Hasta ahora esa regla dependía de que Claude la recordara desde `CLAUDE.md`.

Guía del componente: [docs/03-hooks.md](../docs/03-hooks.md).

## Qué bloquea
Solo mira comandos que usan un cliente SQL: `sqlcmd`, `osql`, `isql`, `bcp`, `Invoke-Sqlcmd`, `psql`, `mysql`, `mariadb`, `sqlite3`, `mssql-cli`, `sqlpackage`.
Dentro de ellos bloquea (exit 2, y Claude recibe el motivo):

| Bloquea | Ejemplo |
|---|---|
| Palabras de escritura o ejecución | `INSERT`, `UPDATE`, `DELETE`, `MERGE`, `TRUNCATE`, `CREATE`, `ALTER`, `DROP`, `GRANT`, `REVOKE`, `DENY`, `EXEC`, `BACKUP`, `RESTORE`, `DBCC`, `BULK`, `OPENROWSET`, `KILL`… |
| `SELECT … INTO` | Crea tablas |
| Procedimientos del sistema | `sp_…`, `xp_…` |
| Ejecutar un archivo | `sqlcmd -i`, `Invoke-Sqlcmd -InputFile`, `psql -f`, `mysql < x.sql`, `.read` (no se puede ver su contenido) |
| `bcp … in` | Carga datos |

Deja pasar todo lo demás: `SELECT` escritos en el propio comando (`sqlcmd -Q "SELECT …"`), `sqlcmd -f 65001` (página de códigos)
y cualquier comando que no use un cliente SQL. Compara **palabras completas** (`FechaUpdate` no cuenta como `UPDATE`) y solo mira
el comando, no su descripción.

Es **conservador**: si una consulta de lectura contiene una de esas palabras como texto (`WHERE Accion = 'DELETE'`), también se bloquea.
En ese caso el script lo ejecuta el usuario.

## Instalar
**Plugin** (todos tus proyectos):
```bash
claude plugin install hook_solo_select@ia-skills-claude
```
**A mano en un proyecto:**
1. Copiar `scripts/solo-select.sh` a `<proyecto>/.claude/hooks/solo-select.sh`, con fin de línea LF.
2. Añadir a `<proyecto>/.claude/settings.json`, sin borrar lo que haya:
```json
{ "hooks": { "PreToolUse": [ { "matcher": "Bash|PowerShell",
  "hooks": [ { "type": "command", "command": "bash .claude/hooks/solo-select.sh", "timeout": 10 } ] } ] } }
```
Comprobar con `/hooks`.

## Probar
```bash
echo '{"tool_name":"Bash","tool_input":{"command":"sqlcmd -Q \"DELETE FROM T\""}}' | bash scripts/solo-select.sh; echo "salida=$?"   # 2
echo '{"tool_name":"Bash","tool_input":{"command":"sqlcmd -Q \"SELECT 1\""}}'      | bash scripts/solo-select.sh; echo "salida=$?"   # 0
```
Necesita `bash` (en Windows, Git Bash). Para leer el JSON usa `python3`, `python` o `node`, el primero que encuentre.
Si no hay ninguno, analiza el JSON entero, lo que es más estricto.

Es una barrera más, no la única: lo seguro de verdad es que el login de BD que usa Claude **solo tenga permisos de lectura**.
