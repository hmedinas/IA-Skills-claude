# 06 · Settings y modos

## Settings: qué es
`settings.json` es la configuración de Claude Code: **permisos** (qué puede hacer sin preguntar), **hooks**, variables de entorno,
modelo y más. Se combinan por niveles; manda el más específico.

| Nivel | Archivo |
|---|---|
| Organización (lo pone un administrador) | Política gestionada |
| Proyecto, solo tú (no se sube) | `<proyecto>/.claude/settings.local.json` |
| Proyecto (se sube a git) | `<proyecto>/.claude/settings.json` |
| Personal | `~/.claude/settings.json` |

### Permisos
```json
{
  "permissions": {
    "allow": ["Bash(git status:*)", "Bash(git diff:*)", "Bash(dotnet build:*)"],
    "ask":   ["Bash(git push:*)"],
    "deny":  ["Bash(git push --force:*)", "Read(./.env)"],
    "defaultMode": "default"
  }
}
```
- `allow`: sin preguntar. `ask`: siempre pregunta. `deny`: nunca.
- Formato de regla: `Herramienta(patrón)`, p. ej. `Bash(npm run test:*)`, `Edit(src/**)`, `mcp__nexus-db__consultar`.
- Atajos: `/permissions` para verlos y editarlos; `/config` para el resto de opciones.
- `deny` es una barrera de configuración; para reglas con lógica (p. ej. "solo SELECT") usa un [hook](03-hooks.md).

## Modos de permisos
Cambian **cuánto pregunta** Claude antes de actuar. Se cambian con **`Shift+Tab`** durante la sesión,
con `--permission-mode` al arrancar o con `permissions.defaultMode` en settings.

| Modo | Qué hace | Cuándo |
|---|---|---|
| `default` | Pide permiso para editar y ejecutar lo que no esté en `allow` | Por defecto; cuando quieres controlar cada paso |
| `acceptEdits` | Edita archivos sin preguntar; los comandos siguen pidiendo permiso | Implementar algo ya acordado |
| `plan` | **Solo lee y propone**: no edita ni ejecuta nada que cambie el proyecto | Diseñar (encaja con `/arnes_trabajo diseno`) |
| `auto` | Decide solo y un clasificador frena lo arriesgado | Tareas largas y de confianza |
| `bypassPermissions` | No pregunta nada | Solo en entornos aislados (contenedores). Nunca contra producción |

## Estilos de salida (*output styles*)
Cambian **cómo responde** Claude: más explicativo, más escueto, en modo aprendizaje…
- Se eligen en `/config`.
- Se pueden crear propios: un `.md` en `~/.claude/output-styles/` (personal) o en `<proyecto>/.claude/output-styles/`,
  con cabecera `name` y `description` y las instrucciones de estilo en el cuerpo.

```markdown
---
name: Explicativo en español
description: Explica el porqué de cada cambio, en español, con ejemplos cortos
---
Responde siempre en español. Antes de cada cambio explica en una frase por qué lo haces.
Termina con "Qué aprendimos": 2–3 bullets.
```

## Otros archivos de contexto
| Archivo | Qué es |
|---|---|
| `CLAUDE.md` (raíz del proyecto, o `~/.claude/CLAUDE.md`) | Reglas que Claude lee siempre al empezar |
| `AGENTS.md` | Lo mismo para cualquier herramienta (convención multi-herramienta; el arnés lo usa como punto de entrada) |
| `.claude/rules/*.md` | Reglas sueltas que se cargan como `CLAUDE.md` |
| Memoria (`~/.claude/projects/<clave>/memory/`) | Notas que Claude guarda entre sesiones |

## Errores típicos
| Síntoma | Causa |
|---|---|
| Pregunta lo mismo una y otra vez | Falta la regla en `allow`. Añádela con `/permissions` o elige "no volver a preguntar" |
| Un cambio en settings no se aplica | Lo pisa otro nivel más específico (`settings.local.json`) |
| En `plan` "no hace nada" | Es lo esperado: solo propone. Sal del modo con `Shift+Tab` para implementar |
