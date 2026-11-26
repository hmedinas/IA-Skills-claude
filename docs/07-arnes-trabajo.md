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
| `doc/agentes/memoria/` | Copia de la memoria del agente (local en modo privado: la memoria es personal) |
| `plan-implementacion-<modulo>.md` | Plan por etapas, que **solo el usuario aprueba** |
| `doc/fichas/` | **Fichas de requerimiento** aprobadas: lo que queda de cada tarea cerrada. Memoria común del equipo |
| `doc/adr/` | Decisiones de arquitectura que perduran (ADR) |
| `doc/fichas/borradores/`, `doc/adr/borradores/` | Solo en modo privado: borradores locales que nunca se suben |

Se instala en uno de dos modos:

| Modo | Para quién | Qué se sube a git |
|---|---|---|
| **Privado** | Repos compartidos con otras personas (recomendado si el repo tiene varios autores) | Solo las fichas y los ADR **aprobados**. El resto del arnés está en tu disco, pero git no lo ve |
| **Versionado** | Proyectos propios, o equipos que acuerdan compartir el mismo arnés | Todo |

## Cuándo usarlo
- Proyectos en los que trabajáis varias personas, o tú en varias sesiones, y queréis que el agente retome sin volver a explicarlo todo.
- Módulos grandes que conviene diseñar y aprobar antes de programar.
- Proyectos con base de datos de solo lectura, varios repos o una raíz sin git.

## Instalación completa
Paso a paso, con comprobaciones y solución de problemas: [00 · Manual de instalación](00-instalacion.md). En resumen:

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
2. **Pregunta en bloques** solo lo que no pudo detectar: nombre e idioma, **modo privado o versionado** (si el repo tiene varios
   autores recomienda *privado*), carpetas de fichas y ADR, **prefijos de ID** de las fichas, cómo se compila, qué servidores tienes
   abiertos, si la BD es de solo lectura y reglas propias.
3. En modo privado, **antes de crear nada**, añade el arnés a `.git/info/exclude`. Si alguno de sus archivos ya está en git, te avisa y pregunta.
4. **Enseña la lista** de archivos a crear y a integrar, y espera tu OK. Nunca sobrescribe tu `CLAUDE.md` ni tu `settings.json`:
   añade secciones.
5. Ejecuta `bash init.sh` y te enseña la salida.

### `revisar`
```
/arnes_trabajo revisar
```
Compara el proyecto con las plantillas: roles, lista de cierre, gates, hook `SessionStart` (y si está en la forma robusta),
`progress.md` demasiado largo, planes sin estado, fichas en Borrador o reglas de `.cursor/rules` que no estén en `CLAUDE.md`.
Si el repo tiene varios autores y git ve archivos del arnés, te propone pasar a **modo privado**.
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

### Nombre y prefijos
`<PREFIJO>-<número>-<titulo>.md`, con los prefijos que use tu equipo. Se eligen al instalar (por defecto `TCK REQ EVO`)
y quedan en `init.sh`, en la variable `PREFIJOS_FICHA`:

```bash
PREFIJOS_FICHA="TCK RFC REQ EVO"
```

| La tarea tiene… | Archivo |
|---|---|
| Un ticket | `TCK-123-error-al-exportar-pdf.md` |
| Una petición de cambio (si tu equipo usa `RFC`) | `RFC-214-limite-de-credito.md` |
| Un requerimiento | `REQ-45-alta-de-proveedores.md` |
| Nada de eso (evolutivo) | `EVO-9f3a61c2-filtro-por-fecha.md` |

- El agente busca el ID con esos prefijos en el nombre de la rama (`feature/rfc-214-limite` → `RFC-214`), en el plan, en `progress.md`
  o en lo que digas. Si no está claro, pregunta una vez. Si aparece un prefijo que no está en la lista, pregunta antes de usarlo.
- El `<id>` de un evolutivo son 8 caracteres hexadecimales aleatorios, así no choca con el de otra persona del equipo.
  No se usa el hash del commit porque al cerrar puede no haberlo y una tarea puede tener varios; los commits se anotan dentro.
  `EVO` siempre está disponible, aunque no lo pongas en la lista.

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
- **Dónde:** en modo versionado, siempre en `doc/fichas/`. En modo privado, el borrador va en `doc/fichas/borradores/` (local)
  y solo pasa a `doc/fichas/` cuando la apruebas. Lo explica la sección *Fichas compartidas*, más abajo.
- **Destino:** hoy, una carpeta de `.md` dentro del repo. Está previsto poder enviarla a un gestor de tickets.
  Mientras tanto, puedes enlazar la ficha aprobada desde el ticket.

### ADR
Una decisión que va a seguir valiendo cuando la tarea se olvide (una librería, una convención, dónde se calcula algo)
se propone como **ADR** en `doc/adr/NNNN-titulo.md`, con contexto, decisión, alternativas y consecuencias.
Empieza en *Propuesto* y solo pasa a *Aceptado* si lo aceptas tú. En modo privado, el propuesto vive en `doc/adr/borradores/`
y pasa a `doc/adr/` al aceptarlo, igual que las fichas.

## Repo compartido: arnés privado
Un equipo de cinco personas trabaja en **el mismo repo**, y cada una instala el arnés en su copia. Si el arnés se versiona:

1. Ana hace `git add .` y sube **su** arnés: su `progress.md` (en qué está ella), su memoria (sus preferencias) y sus fichas en borrador.
2. Luis hace `git pull` y **falla**: git no quiere pisar su `CLAUDE.md` y su `progress.md` locales, que se llaman igual.
3. Si lo arreglan a mano, todos acaban con el `progress.md` y la memoria de Ana, y sus agentes siguen las preferencias de ella.
4. Además, el arnés acaba guardando información interna (rutas, nombres de servidor, cómo se conecta la IA, reglas internas).
   Lo que se sube queda en el historial de git **para siempre**. Y un archivo de instrucciones compartido dirige al agente de todos.

En un repo compartido, el arnés tiene que ser **personal y local**: sus archivos están en el disco de cada uno, pero git no los ve.
Eso hace el **modo privado**, con `.git/info/exclude`: funciona como un `.gitignore`, pero es local, no se sube y solo afecta a tu copia.

Al instalar en modo privado, el arnés añade a `.git/info/exclude`:

```
# arnes_trabajo (modo privado)
/CLAUDE.md
/AGENTS.md
/progress.md
/Resumen.md
/init.sh
/.claude/
/plan-implementacion-*.md
/doc/agentes/memoria/
/doc/fichas/borradores/
/doc/adr/borradores/
```

- `doc/fichas/` y `doc/adr/` **no** se excluyen: ahí van las fichas y los ADR aprobados, que son la memoria común del equipo.
- Si el equipo ya versiona alguno de esos archivos (por ejemplo, un `CLAUDE.md` común), el arnés te avisa y no lo excluye sin preguntar.
  Excluir un archivo que ya está en git no lo oculta.
- La **memoria del agente no se comparte nunca**: es personal.
- Cada vez que abres Claude, `init.sh` comprueba que git no ve ningún archivo del arnés. Si alguno se cuela, lo marca como problema:
  `!!  2 archivo(s) del arnés visibles para git (revisar .git/info/exclude): CLAUDE.md .claude`.

### Reglas para equipos
- **Nunca `git add .` ni `git add -A`.** Añade los archivos por nombre (`git add src/… doc/fichas/RFC-214-….md`).
  Así no se sube nada que no quieras, aunque algo no esté excluido.
- Cada persona instala su arnés en su copia. Las reglas comunes del equipo se acuerdan y se versionan aparte (un `CONTRIBUTING.md`,
  por ejemplo), no dentro del arnés de nadie.
- Si alguien ya subió su arnés, `/arnes_trabajo revisar` lo detecta y propone el modo privado. Sacarlo de git es `git rm --cached <archivo>`
  (el archivo sigue en el disco), pero lo decide el equipo: al hacer pull se borra de las copias de los demás, y lo ya subido sigue en el historial.

## Fichas compartidas: borrador local → aprobada en git → publicada en la web
En modo privado el arnés es personal, pero las fichas **aprobadas** sí se versionan: así cada agente sabe lo que hicieron los demás.

**Ejemplo:**
1. Ana cierra `RFC-214`. Su agente deja la ficha en **Borrador** en `doc/fichas/borradores/RFC-214-limite-de-credito.md`, que es local.
2. Ana la revisa y la aprueba ("apruebo la ficha"). **Solo entonces** el agente:
   - la marca como *Aprobada*;
   - vuelve a pasar el filtro (sin credenciales, datos de producción, nombres de servidor ni conversación en bruto);
   - la mueve a `doc/fichas/`;
   - le da a Ana el comando exacto: `git add doc/fichas/RFC-214-limite-de-credito.md`.
3. Ana la sube en su commit, añadiéndola **por nombre**.
4. Luis hace `git pull`. Antes de empezar `RFC-230`, su agente lee las fichas y los ADR relacionados en `doc/fichas/` y `doc/adr/`.
5. Quien se ocupa de la documentación publica a mano las fichas aprobadas en la web del equipo. El agente no publica nada.

| Dónde | Qué hay | ¿En git? |
|---|---|---|
| `doc/fichas/borradores/`, `doc/adr/borradores/` | Fichas en Borrador y ADR propuestos | No, nunca |
| `doc/fichas/`, `doc/adr/` | Fichas aprobadas y ADR aceptados | Sí, añadidos por nombre |

`init.sh` avisa de los borradores pendientes de aprobar y marca como problema cualquier ficha de `doc/fichas/` que no esté *Aprobada*.
En **modo versionado** todo esto se simplifica: borrador y aprobada están en `doc/fichas/` y solo cambia el estado.

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
| `git pull` falla por `CLAUDE.md` o `progress.md` locales | Alguien subió su arnés y el tuyo no estaba excluido. Usad el modo privado (sección *Repo compartido*) y añadid archivos por nombre |
| `init.sh` dice que hay archivos del arnés visibles para git | Falta una línea en `.git/info/exclude`, o ese archivo ya estaba versionado. Ejecuta `/arnes_trabajo revisar` |
| `init.sh` dice que hay una ficha en `doc/fichas/` que no está Aprobada | En modo privado ahí solo van las aprobadas. Muévela a `doc/fichas/borradores/` o apruébala |
| Una ficha usa un prefijo que tu equipo no tiene | Añádelo a `PREFIJOS_FICHA` en `init.sh` |
| `init.sh` se ejecuta dos veces al abrir | Tienes también el plugin `hook_inicio_sesion`. Desinstálalo: el arnés ya lo incluye |
| `$'\r': command not found` | `init.sh` se guardó con CRLF. Debe ir con LF: añade `*.sh text eol=lf` al `.gitattributes` del proyecto |
| El hook no hace nada en Windows | No está Git Bash, o el hook no lleva `"shell": "bash"`. Ejecuta `/arnes_trabajo revisar` |
| No aparece `/arnes_trabajo` | La sesión ya estaba abierta: abre otra o usa `/reload-plugins` |
| El agente quiere implementar un plan en Borrador | No debería: el plan tiene que estar `Aprobado`. Recuérdaselo o apruébalo de forma explícita |
| La ficha tiene datos que no deberían estar | Bórralos y díselo al agente: el filtro es obligatorio y la ficha sigue en Borrador hasta que la apruebes |
