# 07 · Arnés de trabajo

> Una forma de trabajar con agentes que se mantiene entre sesiones, entre personas del equipo y entre herramientas
> (Claude Code, Cursor u otro LLM). Funciona con cualquier stack.

## Qué es
Una skill que monta en tu proyecto un pequeño "arnés" y luego te acompaña en cada tarea:

| Pieza | Para qué sirve |
|---|---|
| `AGENTS.md` | Cómo se trabaja: roles, planes, gates, cierre. Lo lee cualquier agente |
| `CLAUDE.md` | Reglas del código, de la BD y de git |
| `progress.md` | Dónde se quedó la tarea en curso (corto, < 60 líneas) |
| `Resumen.md` | Estado del producto, decisiones y bitácora (acumulativo) |
| `init.sh` + hook `SessionStart` | Al abrir Claude, comprueba el proyecto y le da el contexto |
| `.claude/agents/` | Agentes **implementador** (Sonnet) y **revisor** (Opus) |
| `doc/agentes/memoria/` | Copia versionada de la memoria del agente |
| `plan-implementacion-<modulo>.md` | Plan por etapas, que **solo el usuario aprueba** |
| `doc/fichas/` | **Fichas de requerimiento**: lo que queda de cada tarea cerrada |
| `doc/adr/` | Decisiones de arquitectura que perduran (ADR) |

## Cuándo usarlo
- Proyectos en los que trabajáis varias personas, o tú en varias sesiones, y queréis que el agente retome sin volver a explicarlo todo.
- Módulos grandes que conviene diseñar y aprobar antes de programar.
- Proyectos con base de datos de solo lectura, varios repos o una raíz sin git.

## Instalación completa
Requisitos: Claude Code y **bash** (en Windows, Git Bash, que viene con Git for Windows).

```bash
# 1. Añadir el marketplace (una sola vez por equipo)
claude plugin marketplace add hmedinas/IA-Skills-claude

# 2. Instalar la skill
claude plugin install arnes_trabajo@ia-skills-claude
```

Después, **abre una sesión nueva** de Claude Code en la raíz de tu proyecto (o ejecuta `/reload-plugins`) y escribe:

```
/arnes_trabajo instalar
```

Si el nombre corto choca con otra skill, usa el nombre completo: `/arnes_trabajo:arnes_trabajo instalar`.

> ⚠️ **No instales también `hook_inicio_sesion`.** El arnés ya añade al proyecto el hook que ejecuta `init.sh`.
> Con los dos, `init.sh` se ejecutaría dos veces (el plugin lo detecta y no lo repite, pero sobra).
> Si ya lo tienes: `claude plugin uninstall hook_inicio_sesion@ia-skills-claude`.

**A mano**, sin plugin: copia la carpeta `arnes_trabajo/` del repositorio a `~/.claude/skills/` (para ti) o a `<proyecto>/.claude/skills/`
(para todos los del repo).

## Los cinco modos

| Modo | Cuándo | Resultado |
|---|---|---|
| `instalar` | Una vez por proyecto | Archivos del arnés creados o integrados |
| `revisar` | Si el arnés ya existe o lo montó otra persona | Tabla de lo que falta o sobra y parches propuestos |
| `diseno <modulo>` | Antes de programar algo grande | `plan-implementacion-<modulo>.md` en **Borrador**. No escribe código |
| `implementar <modulo>` | Con el plan **Aprobado** | Código por etapas, con gate y pausa entre etapas |
| `cierre` | Al terminar **cualquier** tarea | Documentos al día, **ficha de requerimiento** y resumen |

Sin argumento, la skill mira si el proyecto tiene `AGENTS.md`. Si no lo tiene, propone `instalar`; si lo tiene, ejecuta `revisar`.

### `instalar`
```
/arnes_trabajo instalar
```
1. **Detecta** lo que puede: repos, stack, comandos de build, arnés existente, reglas de Cursor o Copilot, scripts SQL,
   carpetas de fichas o ADR y si tienes activo el plugin `hook_inicio_sesion`.
2. **Pregunta en bloques** solo lo que no pudo detectar: nombre e idioma, carpeta de fichas y de ADR, cómo se compila,
   qué servidores tienes abiertos, si la BD es de solo lectura y reglas propias.
3. **Enseña la lista** de archivos a crear y a integrar, y espera tu OK. Nunca sobrescribe tu `CLAUDE.md` ni tu `settings.json`:
   añade secciones.
4. Ejecuta `bash init.sh` y te enseña la salida.

### `revisar`
```
/arnes_trabajo revisar
```
Compara el proyecto con las plantillas: roles, lista de cierre, gates, hook `SessionStart` (y si está en la forma robusta),
`progress.md` demasiado largo, planes sin estado, fichas en Borrador o reglas de `.cursor/rules` que no estén en `CLAUDE.md`.
Propone los cambios y solo los aplica con tu OK.

### `diseno <modulo>`
```
/arnes_trabajo diseno exportacion-pdf
```
Lee el código relacionado y te pregunta en bloques de 3–5: alcance y lo que queda fuera, datos, permisos, integración,
estados de la UI y riesgos. Escribe `plan-implementacion-exportacion-pdf.md` en **Borrador** y se para.
Lo que no hayas respondido va a *Decisiones abiertas*: no lo supone.

Para aprobarlo, cambia el estado a `Aprobado` en el archivo o dilo claramente en el chat ("apruebo el plan").

### `implementar <modulo>`
```
/arnes_trabajo implementar exportacion-pdf
```
Solo arranca si el plan está **Aprobado** o **En curso**. Por cada etapa: hace los pasos (delegando en el implementador
y el revisor si el cambio es grande o de riesgo), compila, comprueba el gate y **se para**:

```
Etapa 2 terminada. Archivos: …
Gate: builds en verde ✔ · script 14 ejecutado por el usuario ⏳
¿Sigo con la etapa 3?
```

### `cierre`
```
/arnes_trabajo cierre
```
Se usa al terminar **cualquier** tarea que cambie el producto, con plan o sin él. El agente:

1. Comprueba los builds (o dice exactamente qué falla) y que el revisor no tenga hallazgos abiertos.
2. Actualiza `progress.md`, `Resumen.md` (con entrada de bitácora), el plan y los documentos afectados.
3. **Crea o actualiza la ficha de requerimiento** de la tarea, en **Borrador**.
4. Si alguna decisión perdura más allá de la tarea, **propone** convertirla en ADR. Solo lo escribe si aceptas.
5. Guarda en memoria las preferencias nuevas y te recuerda `bash init.sh --sync-memoria`.
6. Te responde con: qué cambió, qué scripts tienes que ejecutar y en qué orden, qué documentos tocó (con la ruta de la ficha),
   qué verificó y qué no. **No hace commit** salvo que se lo pidas.

Ejemplo de respuesta:

```
Cierre de TCK-123 · Error al exportar PDF
- Cambiado: Backend/Export/PdfService.cs, Frontend/pages/informes.vue
- Scripts: ninguno
- Documentos: progress.md, Resumen.md (bitácora 2026-11-26), doc/fichas/TCK-123-error-al-exportar-pdf.md (Borrador)
- ADR propuesto: "Generar los PDF en el servidor, no en el navegador" → ¿lo creo en doc/adr/0004-…?
- Verificado: build back y front en verde. No verificado: exportación con más de 1.000 filas.
- Sin commit.
```

## La ficha de requerimiento
Lo que queda de cada tarea para quien llegue después, sea otra persona del equipo u otro agente.
Se crea **siempre** en el cierre, en la carpeta elegida al instalar (por defecto `doc/fichas/`).

### Nombre
| La tarea tiene… | Archivo |
|---|---|
| Un ticket asignado | `TCK-<número>-<titulo>.md` → `TCK-123-error-al-exportar-pdf.md` |
| Un requerimiento | `REQ-<número>-<titulo>.md` → `REQ-45-alta-de-proveedores.md` |
| Nada de eso (evolutivo) | `EVO-<id>-<titulo>.md` → `EVO-9f3a61c2-filtro-por-fecha.md` |

- El número sale del plan, de `progress.md`, del nombre de la rama o de lo que digas. Si no está claro, el agente pregunta una vez.
- El `<id>` de un evolutivo son 8 caracteres hexadecimales aleatorios, así no choca con el de otra persona del equipo.
  No se usa el hash del commit porque al cerrar puede no haberlo y una tarea puede tener varios; los commits se anotan dentro.

### Qué contiene
1. **Objetivo y contexto.**
2. **Decisiones** y alternativas descartadas, y los ADR propuestos.
3. **Qué se cambió:** repo, rama, commits (o "sin commit todavía") y archivos.
4. **Qué se ejecutó y en qué entorno** (local, desarrollo, pruebas), con el resultado.
5. **Pruebas** y resultado, y lo que **no** se verificó.
6. **Hallazgos.**
7. **Pendientes**, con quién los tiene.
8. Historial de la ficha.

Plantilla completa: [`arnes_trabajo/templates/ficha-requerimiento.md`](https://github.com/hmedinas/IA-Skills-claude/blob/main/arnes_trabajo/templates/ficha-requerimiento.md).

### Reglas
- **Filtro obligatorio:** sin credenciales, sin datos de producción (filas, nombres, correos, importes reales),
  sin nombres de servidor ni IPs y sin conversaciones copiadas tal cual. La ficha se versiona y se comparte.
- **Borrador hasta que la apruebes**, igual que los planes. El agente nunca la aprueba por su cuenta.
  Si una ficha aprobada se actualiza en otro cierre, vuelve a Borrador con el motivo.
- **Una tarea, una ficha:** su ruta se anota en el plan y en `progress.md`, y los cierres siguientes la actualizan en vez de crear otra.
- **Destino:** hoy, una carpeta de `.md` dentro del repo. Está previsto poder enviarla a un gestor de tickets.
  Mientras tanto, puedes enlazar la ficha aprobada desde el ticket.

### ADR
Una decisión que va a seguir valiendo cuando la tarea se olvide (una librería, una convención, dónde se calcula algo)
se propone como **ADR** en `doc/adr/NNNN-titulo.md`, con contexto, decisión, alternativas y consecuencias.
Empieza en *Propuesto* y solo pasa a *Aceptado* si lo aceptas tú.

## El hook de inicio (y Windows)
El arnés añade esto al `.claude/settings.json` del proyecto:

```json
{ "type": "command", "shell": "bash",
  "command": "bash \"${CLAUDE_PROJECT_DIR:-.}/init.sh\"", "timeout": 60 }
```

- `"shell": "bash"`: en Windows ejecuta el hook con Git Bash y no con PowerShell.
- `${CLAUDE_PROJECT_DIR:-.}`: encuentra `init.sh` aunque Claude se haya abierto desde una subcarpeta.

Si tu proyecto tiene la forma antigua (`"command": "bash init.sh"`), `/arnes_trabajo revisar` te propone cambiarla.

## Errores típicos
| Síntoma | Causa y solución |
|---|---|
| `init.sh` se ejecuta dos veces al abrir | Tienes también el plugin `hook_inicio_sesion`. Desinstálalo: el arnés ya lo incluye |
| `$'\r': command not found` | `init.sh` se guardó con CRLF. Debe ir con LF: añade `*.sh text eol=lf` al `.gitattributes` del proyecto |
| El hook no hace nada en Windows | No está Git Bash, o el hook no lleva `"shell": "bash"`. Ejecuta `/arnes_trabajo revisar` |
| No aparece `/arnes_trabajo` | La sesión ya estaba abierta: abre otra o usa `/reload-plugins` |
| El agente quiere implementar un plan en Borrador | No debería: el plan tiene que estar `Aprobado`. Recuérdaselo o apruébalo de forma explícita |
| La ficha tiene datos que no deberían estar | Bórralos y díselo al agente: el filtro es obligatorio y la ficha sigue en Borrador hasta que la apruebes |
