# 03 · Hooks

## Qué es
Un **hook** es un comando **tuyo** que Claude Code ejecuta automáticamente cuando ocurre un evento. **No depende de que el modelo
se acuerde ni obedezca**: lo ejecuta el programa, así que es la forma de hacer que una regla se cumpla *siempre*.

## Cuándo usarlo
- Preparar algo al empezar: `SessionStart` ejecuta tu `init.sh`.
- **Bloquear** algo peligroso antes de que ocurra: `PreToolUse`.
- Hacer algo después de editar (formatear, compilar): `PostToolUse`.
- Avisar o comprobar al terminar: `Stop`.

## Eventos más útiles
| Evento | Cuándo se dispara | Puede bloquear | El matcher filtra por |
|---|---|---|---|
| `SessionStart` | Al abrir, retomar, limpiar o compactar la sesión | No | `startup`, `resume`, `clear`, `compact` |
| `UserPromptSubmit` | Al enviar tú un mensaje, antes de que Claude lo procese | Sí | — |
| `PreToolUse` | **Antes** de usar una herramienta | **Sí** | Nombre de herramienta (`Bash`, `Edit`, `Write`, `PowerShell`, `mcp__x__y`) |
| `PostToolUse` | Después de usarla con éxito | No (puede avisar a Claude) | Nombre de herramienta |
| `Stop` | Cuando Claude termina de responder | Sí (le obliga a seguir) | — |
| `SubagentStop` | Cuando termina un agente | Sí | — |
| `PreCompact` / `PostCompact` | Antes y después de compactar el contexto | — | — |
| `SessionEnd` | Al cerrar la sesión | No | — |

Hay más (`Notification`, `PermissionRequest`, `FileChanged`…); la lista completa está en la documentación oficial.

## Dónde vive
| Ámbito | Archivo |
|---|---|
| Personal | `~/.claude/settings.json` |
| Proyecto (se sube a git) | `<proyecto>/.claude/settings.json` |
| Proyecto, solo tú | `<proyecto>/.claude/settings.local.json` |
| Plugin | `<plugin>/hooks/hooks.json` |

## Cómo se crea
### 1. Declararlo en `settings.json`
```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Bash|PowerShell",
        "hooks": [
          { "type": "command", "command": "bash .claude/hooks/solo-select.sh", "timeout": 10 }
        ]
      }
    ]
  }
}
```
- `matcher`: con solo letras y `|` es una lista exacta (`Bash|PowerShell`). Con otros caracteres es una expresión regular (`mcp__.*`).
- `type`: casi siempre `command`. También existen `http`, `prompt`, `agent` y `mcp_tool`.
- **Forma shell** (sin `args`) o **forma exec** (`"command": "node", "args": ["script.js"]`, sin shell de por medio).
  En Windows, la shell es Git Bash si está instalado; si no, PowerShell.

### 2. Escribir el script
- **Entrada:** recibe un JSON por la entrada estándar. En `PreToolUse` incluye `tool_name` y `tool_input`
  (para `Bash`, `tool_input.command` es el comando que Claude quiere ejecutar).
- **Salida:**

| Código de salida | Efecto |
|---|---|
| `0` | Todo bien, la acción sigue. Si imprime un JSON, Claude Code lo interpreta |
| **`2`** | **Bloquea** (en eventos que pueden bloquear). Lo que escribas en *stderr* es el motivo que recibe Claude |
| Otro (p. ej. `1`) | Error que **no bloquea**: la acción sigue y se muestra un aviso. Para hacer cumplir una regla, usa `2` |

Alternativa a `exit 2`: salir con `0` e imprimir
`{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"deny","permissionDecisionReason":"motivo"}}`.

### 3. Probarlo sin Claude
```bash
echo '{"tool_name":"Bash","tool_input":{"command":"sqlcmd -Q \"DELETE FROM T\""}}' | bash .claude/hooks/solo-select.sh; echo "salida=$?"
```

## Ejemplos reales (componentes de este repositorio)
| Componente | Evento | Qué hace |
|---|---|---|
| `hook_inicio_sesion` | `SessionStart` | Si el proyecto tiene `init.sh`, lo ejecuta (estado de git, scripts pendientes, `progress.md`) |
| `hook_solo_select` | `PreToolUse` (`Bash`, `PowerShell`) | Si el comando usa un cliente SQL (`sqlcmd`, `Invoke-Sqlcmd`, `psql`, `mysql`…) con algo distinto de `SELECT`, o con un archivo `-i`, lo **bloquea** |

## Cómo se instala
- **Como plugin:** `claude plugin install hook_solo_select@ia-skills-claude`. Se activa en todos tus proyectos.
- **A mano en un proyecto:**
  1. Copiar `hook_solo_select/scripts/solo-select.sh` a `<proyecto>/.claude/hooks/`.
  2. Añadir el bloque `PreToolUse` de arriba a `<proyecto>/.claude/settings.json`, **sin borrar los hooks que ya haya**.
- Comprobar con `/hooks`: lista los hooks activos y de dónde vienen.

## Errores típicos
| Síntoma | Causa |
|---|---|
| El hook no bloquea | Sale con `1` en vez de `2` |
| No se ejecuta | El `matcher` no coincide con el nombre de la herramienta (distingue mayúsculas), o la sesión es anterior al cambio |
| En plugins falla la ruta | Usar `"${CLAUDE_PLUGIN_ROOT}"/scripts/x.sh`, con comillas por los espacios |
| El script falla en Windows | Fin de línea CRLF en un `.sh`. Guardarlo con LF (`.gitattributes`: `*.sh text eol=lf`) |
| La sesión va lenta | El hook tarda. Poner `timeout` y que haga lo mínimo |
