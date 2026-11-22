# 01 · Skills y comandos

## Qué es
Una **skill** es una carpeta con un `SKILL.md`: instrucciones en markdown y una cabecera que dice cómo se llama y para qué sirve.
Claude la carga **cuando la invocas con `/nombre`** o **cuando su descripción encaja** con lo que pides.
Puede llevar archivos de apoyo (plantillas, scripts, ejemplos) que Claude lee solo si los necesita.

**Comando** = una skill que invocas tú. Antes iban en `.claude/commands/<nombre>.md` (archivo suelto). Siguen funcionando,
pero lo recomendado ahora es la carpeta de skill, que admite archivos de apoyo.

## Cuándo usarlo
- Claude no sabe **cómo** haces tú algo: tu checklist de revisión, tu forma de crear un módulo, tu plantilla de documento.
- Un procedimiento que repites a menudo y quieres lanzar con una palabra.
- **No** sirve para obligar a nada: si algo tiene que pasar siempre, eso es un [hook](03-hooks.md).

## Dónde vive
| Ámbito | Ruta |
|---|---|
| Personal | `~/.claude/skills/<nombre>/SKILL.md` |
| Proyecto | `<proyecto>/.claude/skills/<nombre>/SKILL.md` |
| Plugin | `<plugin>/skills/<nombre>/SKILL.md` (o `SKILL.md` en la raíz si el plugin es una sola skill) |

## Cómo se crea
1. Crear la carpeta `.claude/skills/<nombre>/`. El nombre va en minúsculas, sin espacios.
2. Crear `SKILL.md` con la cabecera y las instrucciones:

```markdown
---
name: revisar-sql
description: Revisa un script SQL contra las reglas del proyecto (alias T1/T2, idempotencia, cabecera). Úsala cuando el usuario pida revisar un .sql o antes de entregar un script de Script/.
argument-hint: <ruta-del-script>
---

Lee el script `$ARGUMENTS` y comprueba:
1. Alias T1, T2, TX1… (nunca alias semánticos).
2. Que sea idempotente: `IF OBJECT_ID(...) IS NULL`, `CREATE OR ALTER`.
3. Cabecera con número, qué hace y cuándo ejecutarlo.

Devuelve una tabla: línea · problema · corrección. Si no hay fallos, dilo.
```

3. Abrir una sesión nueva (o `/reload-plugins`) y escribir `/revisar-sql Script/16-algo.sql`.

### Campos de la cabecera
| Campo | Para qué |
|---|---|
| `name` | Nombre del comando (`/name`) |
| `description` | **Lo más importante**: Claude la lee para decidir si usar la skill sola. Di qué hace y *cuándo* usarla |
| `argument-hint` | Pista que se muestra al escribir el comando (`<ruta>`, `[modo]`) |
| `disable-model-invocation: true` | Solo se ejecuta si la invocas tú; Claude no la lanza sola |
| `allowed-tools` | Herramientas que puede usar sin pedir permiso (p. ej. `Read, Grep, Bash(git diff:*)`) |
| `model` | Modelo con el que se ejecuta |

### Dentro del cuerpo
- `$ARGUMENTS` es lo que escribes después del comando.
- `` !`comando` `` ejecuta el comando al cargar la skill y mete su salida (p. ej. `` !`git status --short` ``).
- Archivos de apoyo: referéncialos por ruta relativa (`templates/AGENTS.md`). Claude los abre cuando le hacen falta.

## Ejemplo real
`arnes_trabajo/` en este repositorio: `SKILL.md` con cinco modos (`instalar`, `revisar`, `diseno`, `implementar`, `cierre`)
y una carpeta `templates/` con lo que copia al proyecto.

## Cómo se instala
- **A mano:** copiar la carpeta a `~/.claude/skills/` (personal) o a `<proyecto>/.claude/skills/` (proyecto).
- **Como plugin:** `claude plugin install arnes_trabajo@ia-skills-claude`. Con plugin, el comando lleva prefijo: `/arnes_trabajo:arnes_trabajo`
  (o el nombre corto si no choca con otro).

## Errores típicos
| Síntoma | Causa |
|---|---|
| No aparece el comando | La sesión ya estaba abierta: abrir una nueva o `/reload-plugins`. O el archivo no se llama exactamente `SKILL.md` |
| Claude nunca la usa solo | La `description` no dice *cuándo* usarla, o es demasiado genérica |
| Claude la usa cuando no toca | `description` demasiado amplia. Acótala o pon `disable-model-invocation: true` |
| Ignora una regla de la skill | Las skills son instrucciones, no obligaciones. Si debe cumplirse siempre, usa un hook |
