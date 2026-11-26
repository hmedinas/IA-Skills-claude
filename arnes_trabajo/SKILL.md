---
name: arnes_trabajo
description: Arnés de trabajo con agentes para cualquier proyecto (Claude Code, Cursor u otro LLM). Instala o revisa AGENTS.md, CLAUDE.md, progress.md, Resumen.md, init.sh con hook SessionStart, agentes implementador/revisor, memoria versionada y planes por etapas con aprobación humana; guía el diseño de un módulo (preguntas en bloques → plan en Borrador), la implementación por etapas con gate y el cierre documentado con ficha de requerimiento (prefijos configurables) y ADR. Modo privado para repos compartidos (.git/info/exclude). Úsala cuando el usuario pida montar, preparar, revisar o copiar "el arnés" o "la forma de trabajar" en un proyecto, diseñar un módulo o funcionalidad grande antes de programarla, implementar un plan aprobado o cerrar una tarea con su documentación o su ficha de requerimiento. Pensada para proyectos multi-repo, con base de datos de solo lectura y commits solo a petición.
argument-hint: "[instalar | revisar | diseno <modulo> | implementar <modulo> | cierre]"
---

# arnes_trabajo — Arnés de trabajo con agentes

Monta y aplica una forma de trabajar estable entre sesiones y entre herramientas:
- **arranque verificado:** `init.sh` y hook `SessionStart`;
- **contexto corto y persistente:** `progress.md`, `Resumen.md` y la memoria;
- **reglas escritas:** `CLAUDE.md` y `AGENTS.md`;
- **roles:** orquestador, implementador y revisor;
- **planes por etapas** con aprobación humana y gate de verificación;
- **cierre documentado** sin que el usuario tenga que pedirlo, con una **ficha de requerimiento** por tarea y ADR para las decisiones que perduran.

Las plantillas están en `templates/` (junto a este archivo). Los marcadores `{{ASI}}` los rellena la skill con
lo que detecta en el proyecto y lo que responde el usuario.

## Modos

| Invocación | Qué hace |
|---|---|
| `/arnes_trabajo instalar` | Crea el arnés en un proyecto o completa uno a medias |
| `/arnes_trabajo revisar` | Comprueba un arnés existente y propone lo que falte, sin pisar nada |
| `/arnes_trabajo diseno <modulo>` | Preguntas en bloques → `plan-implementacion-<modulo>.md` en estado **Borrador**. No programa |
| `/arnes_trabajo implementar <modulo>` | Ejecuta un plan **Aprobado**, etapa por etapa, con gate y pausa |
| `/arnes_trabajo cierre` | Lista de cierre: builds, documentos, scripts pendientes, **ficha de requerimiento** en Borrador y resumen al usuario |

Sin argumento: si el proyecto no tiene `AGENTS.md`, proponer `instalar`. Si lo tiene, ejecutar `revisar` y
preguntar qué quiere hacer.

---

## Principios que nunca se rompen

1. **Lo que existe manda.** Nunca sobrescribir `CLAUDE.md`, `AGENTS.md`, `Resumen.md`, `progress.md`, `.claude/settings.json`
   ni agentes existentes. Se integran secciones nuevas, enseñando antes el cambio.
   - Los archivos fuera de git (p. ej. una raíz que no es repo) se leen antes de tocarlos, y su cambio se explica antes de hacerlo.
   - No tocar reglas de otras herramientas (`.cursor/rules/`, `.windsurf/`, …). Si contienen reglas, se reflejan en `CLAUDE.md`.
2. **Git a petición.** Commit, push, ramas, stash y merge **solo si el usuario lo pide**.
   - El proyecto puede tener varios repos (subcarpetas con `.git`) y una raíz que no es repo. Todo comando git va con `git -C <repo>`.
   - Tener cambios sin commit es normal y **no bloquea** el trabajo: se informa y se sigue.
3. **Base de datos protegida** (si el proyecto la declara de solo lectura):
   - contra la BD solo `SELECT`;
   - todo cambio va como **script nuevo, numerado e idempotente** en la carpeta de scripts y **lo ejecuta el usuario**;
   - nunca se edita un script ya ejecutado;
   - un script no está ejecutado hasta que el usuario lo confirma;
   - las credenciales no se escriben en ningún archivo.
4. **El humano aprueba.** Un plan no se implementa hasta que su estado significa *Aprobado*. Cuenta como aprobación:
   - que el usuario edite el estado en el archivo; o
   - que lo apruebe de forma explícita en el chat ("sí, aplícalo", "apruebo el plan"). En ese caso se registra en el plan:
     `Aprobado por el usuario en chat AAAA-MM-DD: "<sus palabras>"`.

   El agente **nunca** se aprueba a sí mismo ni interpreta como aprobación un "vale" que respondía a otra cosa.
5. **Documentar es parte de terminar.** Al cerrar cada cambio se actualizan `progress.md`, `Resumen.md` y los documentos
   afectados, sin que el usuario lo pida.
6. **Decir la verdad sobre la verificación.** Siempre se dice qué se compiló, qué se probó y qué no.
7. **Idioma del proyecto** en todo lo que se escribe (por defecto, el del usuario).
8. **Ni servidores ni procesos largos** que el usuario ya tenga abiertos. Los builds de verificación van a carpeta temporal.
9. **En un repo compartido, el arnés es personal** (modo privado): sus archivos existen en el disco de cada uno, pero git no los ve.
   Solo se comparten las fichas y los ADR **aprobados**. La memoria del agente **nunca** se comparte.
   Los archivos se añaden a git **por nombre**: nunca `git add .` ni `git add -A`.

---

## Modo `instalar`

### 1. Detectar antes de preguntar
- **Repos:** ¿la raíz es repo? Buscar subcarpetas con `.git` (profundidad 2) y el remoto de cada una (`git -C <d> remote -v`).
- **Stack:** `*.sln`/`*.slnx`/`*.csproj` (.NET), `nuxt.config.*`/`package.json` (Nuxt/Node), `pubspec.yaml` (Flutter),
  `pyproject.toml`, `go.mod`… De ahí salen los comandos de build candidatos.
- **Arnés existente:** `AGENTS.md`, `CLAUDE.md`, `progress.md`, `Resumen.md`, `init.sh`, `.claude/settings.json`, `.claude/agents/`,
  `doc/agentes/memoria/`, `plan-implementacion-*.md`.
- **Reglas de otras herramientas:** `.cursor/rules/`, `.github/copilot-instructions.md`, `PROJECT_GUIDELINES.md`. Se leen para no
  contradecirlas.
- **Base de datos:** carpetas de scripts `.sql` y su patrón de numeración (`NN-accion-objeto.sql`), cadenas de conexión en la configuración.
  Solo se anota que existen; **nunca se copian credenciales**.
- **Memoria del agente:** `~/.claude/projects/<clave>/memory/`. La clave es la ruta absoluta del proyecto con cada carácter
  no alfanumérico cambiado por `-`.
- **Autores del repo:** `git -C <repo> shortlog -sne HEAD | wc -l`. Va con `HEAD`: sin él, `shortlog` espera la entrada estándar y no devuelve nada.
  Si hay más de un autor, el repo es compartido y se recomienda el modo **privado**.
- **Archivos del arnés ya versionados:** `git ls-files -- CLAUDE.md AGENTS.md progress.md Resumen.md init.sh .claude doc/agentes/memoria 'plan-implementacion-*.md'`.
  Si el equipo ya comparte alguno (p. ej. `CLAUDE.md` o `.claude/settings.json`), se tiene en cuenta en el modo privado.
- **Fichas y ADR existentes:** carpetas como `doc/fichas/`, `doc/requerimientos/`, `doc/adr/`, `docs/decisions/`. Si ya hay una, se propone esa.
- **Plugin `hook_inicio_sesion`:** buscar `hook_inicio_sesion@` en `enabledPlugins` de `~/.claude/settings.json` y de
  `.claude/settings.json` (o la salida de `claude plugin list`). Ese plugin **también** ejecuta `init.sh` al arrancar: si está
  activo y el arnés añade su hook, `init.sh` se ejecuta **dos veces** en cada sesión. Ver el paso 3 de *Generar*.

### 2. Preguntar en bloques (solo lo que no se pudo detectar)
Usar la herramienta de preguntas con 3–4 preguntas por bloque, con lo detectado como opción recomendada. Parar en
cuanto se pueda rellenar la plantilla.
- **Bloque A, proyecto:** nombre, idioma, repos detectados (confirmar), quién hace commit/push y **modo del arnés**:
  - **Privado** (recomendado si el repo tiene varios autores): los archivos del arnés están en el disco pero git no los ve
    (`.git/info/exclude`). Solo se versionan las fichas y los ADR aprobados.
  - **Versionado** (proyecto propio, o el equipo acuerda compartir el arnés): todo se sube con el repo.
- **Bloque A2, fichas:**
  - carpeta de las fichas aprobadas (por defecto `doc/fichas`) y de los ADR (por defecto `doc/adr`). En modo privado, además,
    la de borradores locales (por defecto `doc/fichas/borradores` y `doc/adr/borradores`). En modo versionado, los borradores van en la misma carpeta;
  - **prefijos de ID** que usa el equipo (por defecto `TCK REQ EVO`; p. ej. `TCK RFC REQ EVO`). `EVO`, con id aleatorio, siempre está disponible.
  Por ahora el destino es siempre una carpeta de `.md`; la integración con un gestor de tickets queda para más adelante.
- **Bloque B, verificación:** comando de build de cada parte, servidores que el usuario tiene abiertos (puertos) y que no hay que arrancar.
- **Bloque C, datos:** si hay BD; si el acceso del agente es de solo lectura; carpeta y numeración de scripts; quién los ejecuta.
- **Bloque D, reglas propias:**
  - convenciones de UI o de código ya acordadas;
  - fin de línea y codificación (CRLF, BOM);
  - datos sensibles (p. ej. "nunca nombres de clientes reales");
  - notas de memoria privadas que no deben copiarse al repo.

### 3. Generar
0. **Solo en modo privado, antes de crear nada:**
   - Comprobar con `git ls-files` que ninguno de los archivos del arnés está ya en git. Si alguno lo está, **avisar y no excluirlo sin preguntar**
     (excluir un archivo versionado no lo oculta, y sacarlo del índice afecta a todo el equipo). Opciones para el usuario:
     integrar en el archivo compartido solo lo común, enseñándolo antes; o dejar ese archivo fuera del arnés.
     Si `.claude/` ya tiene archivos versionados, no se excluye entera: solo `/.claude/agents/implementador.md`, `/.claude/agents/revisor.md`
     y `/.claude/settings.local.json`, y el hook va en `.claude/settings.local.json`.
   - Añadir a `.git/info/exclude` las líneas que falten, **sin duplicar**, bajo el comentario `# arnes_trabajo (modo privado)`.
     Con las rutas elegidas en el bloque A2:
     ```
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
     `{{FICHAS_DIR}}/` y `{{ADR_DIR}}/` **no** se excluyen: ahí van las aprobadas, que son la memoria común del equipo.
   - Si la raíz no es repo y los repos son subcarpetas, el arnés de la raíz ya queda fuera de git. Solo hay que excluir lo que caiga
     dentro de un repo, en el `.git/info/exclude` de ese repo.
1. Enseñar la lista de archivos a **crear** y a **integrar** (los existentes, con la sección que se añade) y, en modo privado,
   las líneas añadidas a `.git/info/exclude`. Esperar el OK.
2. Crear desde `templates/`, sustituyendo los marcadores (tabla al final). Lo que no aplique **se borra**: no se dejan marcadores ni secciones vacías.
   - `AGENTS.md`, `CLAUDE.md`, `progress.md`, `Resumen.md` y `init.sh` van en la raíz.
   - `settings.json` va en `.claude/settings.json`. Si ya existe, **añadir** el hook `SessionStart` sin quitar nada.
     Si el plugin `hook_inicio_sesion` está activo, **avisar** de que `init.sh` se ejecutaría dos veces y preguntar qué prefiere:
     quitar el plugin (`claude plugin uninstall hook_inicio_sesion@ia-skills-claude`, recomendado si el proyecto se comparte) o no añadir el hook al proyecto.
   - `agents/implementador.md` y `agents/revisor.md` van en `.claude/agents/`.
   - `memoria/MEMORY.md` va en `doc/agentes/memoria/MEMORY.md`. En modo privado es una **copia local** (`{{MEMORIA_TEXTO}}`):
     la memoria es personal y no se comparte nunca.
   - `plan-implementacion.md` **no se copia** al instalar: es la plantilla del modo `diseno`.
   - `ficha-requerimiento.md` y `adr.md` **no se copian**: son las plantillas del modo `cierre`. Las carpetas `{{FICHAS_DIR}}/` y
     `{{ADR_DIR}}/` se crean vacías con un `.gitkeep`. Las de borradores del modo privado se crean **sin** `.gitkeep` (están excluidas).
3. Respetar el fin de línea dominante del proyecto en los `.md`. `init.sh` siempre con LF.
4. Ejecutar `bash init.sh` y corregir lo que marque. En modo privado debe salir `OK  ningún archivo del arnés es visible para git`.
5. Guardar en la memoria del agente una nota breve: "arnés instalado AAAA-MM-DD, reglas clave X/Y/Z".
6. Responder con:
   - qué se creó y qué se integró;
   - la salida de `init.sh`;
   - qué decidió el usuario;
   - qué queda pendiente.

## Modo `revisar`
1. Ejecutar `bash init.sh` si existe.
2. Contrastar el proyecto con las plantillas:
   - ¿están los roles?
   - ¿la lista de cierre?
   - ¿los gates y la aprobación humana?
   - ¿el hook `SessionStart`?
   - ¿`progress.md` tiene menos de 60 líneas?
   - ¿los planes tienen estado?
   - ¿hay reglas de `.cursor/rules` que no estén en `CLAUDE.md`?
   - ¿el hook usa la forma robusta (`"shell": "bash"` y `"${CLAUDE_PROJECT_DIR:-.}/init.sh"`)? Si sigue con `bash init.sh` a secas, proponer el cambio;
   - ¿está activo también el plugin `hook_inicio_sesion`? Entonces `init.sh` se ejecuta dos veces: avisar (ver `instalar`);
   - ¿existen la carpeta de fichas y la de ADR, y hay fichas en **Borrador** pendientes de aprobar?
   - ¿las fichas usan los prefijos de `PREFIJOS_FICHA`?
   - **¿repo compartido con el arnés a la vista?** Si el repo tiene varios autores (`git shortlog -sne HEAD`) y hay archivos del arnés
     visibles para git (`git status --porcelain --ignored`, `git ls-files`), proponer el modo privado. Si ya estaban versionados, explicar
     que dejar de versionarlos es `git rm --cached <archivo>` (el archivo sigue en el disco), que lo decide el equipo porque al hacer pull
     se borra de las copias de los demás, y que lo ya subido sigue en el historial.
   - en modo privado, ¿hay en `{{FICHAS_DIR}}/` fichas que no estén *Aprobadas*? No deberían estar ahí.
3. Informar en una tabla: falta, sobra, desactualizado. Proponer parches concretos y aplicarlos **solo con OK**.
4. Si `progress.md` supera 60 líneas, proponer pasar lo histórico a la bitácora de `Resumen.md`.

## Modo `diseno <modulo>`
Diseñar antes de programar. **En este modo no se escribe código.**
1. **Contexto:**
   - leer `CLAUDE.md`, `AGENTS.md`, `progress.md`, la sección del módulo en `Resumen.md` y los planes existentes, imitando su estilo;
   - revisar el código relacionado (tablas, endpoints, pantallas) para no inventar nada.
2. **Tamaño:**
   - si son 1–3 archivos, no hace falta plan: explicar en el chat qué se va a hacer, esperar el OK e implementar;
   - si es un módulo grande (varias etapas o sesiones), seguir con los pasos siguientes.
3. **Preguntas en bloques de 3–5**, de lo más estructural a lo más fino:
   - alcance y qué queda **fuera**;
   - datos y relaciones con lo existente;
   - permisos y quién hace qué;
   - integración con otros módulos;
   - estados de la UI (vacío, error, solo lectura);
   - riesgos;
   - decisiones ya cerradas.

   Dejar de preguntar cuando se puedan nombrar los archivos implicados, la primera y la última etapa y cómo se comprueba cada una.
   **No suponer decisiones:** lo que no esté respondido va a "Decisiones abiertas".
4. **Escribir** `plan-implementacion-<modulo>.md` desde `templates/plan-implementacion.md`, con estado **Borrador** y la fecha del sistema.
   Si el módulo es demasiado grande, partirlo en etapas o en varios planes.
5. **Parar.** Decir al usuario que lo revise y lo apruebe (principio 4). No proponer empezar a implementar.

## Modo `implementar <modulo>`
1. **Localizar el plan.** Aceptar el nombre completo, el slug o el módulo. Si no aparece, listar los planes y preguntar.
2. **Validar el estado.** Solo se sigue si significa *Aprobado* o *En curso*, en cualquier idioma:
   - *Borrador* o *En revisión* → parar y pedir la aprobación (sin ofrecer "empiezo igualmente").
   - *Implementado* u *Obsoleto* → parar y preguntar.
   - Valor dudoso → preguntar, no suponer.
3. **Situar al usuario:**
   - el objetivo;
   - la etapa siguiente sin cerrar, con su gate y sus criterios;
   - el estado git de cada repo (solo informativo).

   Pasar el estado a **En curso** y esperar el "adelante".
4. **Por cada etapa:**
   - **Quién lo hace:** tareas pequeñas, el orquestador solo. Cambios grandes o de riesgo (seguridad, permisos, BD, lógica de negocio),
     un encargo al **implementador** y después el **revisor** (ver `AGENTS.md` §2).
   - **Scripts de BD:** número = el mayor existente + 1. Cabecera, idempotencia, orden de ejecución y "lo ejecuta el usuario".
   - **Builds** en verde, o el error exacto.
   - **Gate:** no se marca `[x]` hasta pasarlo. Si incluye "el usuario ejecuta el script" o "el usuario prueba", queda pendiente de esa confirmación.
   - **Pausa:**

     ```
     Etapa N terminada. Archivos: …
     Gate: … (✔ hecho / ⏳ pendiente del usuario)
     ¿Sigo con la etapa N+1?
     ```
5. **Reglas durante la implementación:**
   - **Se implementa lo acordado.** Si algo parece mejorable, se comenta. Si se cambia, se cambia primero en el plan
     ("Decisiones", con fecha) y después en el código.
   - **Ambigüedad** → parar, describirla, dar 2–3 opciones concretas y esperar.
   - **Petición fuera de alcance** → recordarlo y anotarla en "Pendiente para siguiente versión". Solo se hace si el usuario lo decide.
     En ese caso entra en el plan como etapa o paso nuevo.
6. **Al terminar la última etapa:**
   - repasar los criterios de aceptación uno por uno;
   - el estado pasa a **Implementado** solo cuando el usuario confirma las pruebas;
   - después, modo `cierre`.

## Modo `cierre`
Se aplica al terminar **cualquier** tarea que cambie el producto, use o no el modo `implementar`.
1. Builds en verde, o qué falla exactamente.
2. Revisor sin hallazgos abiertos, si se usó.
3. Actualizar:
   - `progress.md` (corto, con el siguiente paso);
   - `Resumen.md`: fecha, sección del módulo, decisiones, scripts y si están ejecutados, pendientes y entrada de bitácora `AAAA-MM-DD`;
   - el plan: `[x]`, gate y registro de avance;
   - los documentos que dependan del cambio (resumen de API, manual de usuario, roadmap).
4. **Ficha de requerimiento**, siempre (ver la sección siguiente).
5. Si una decisión perdura más allá de la tarea (arquitectura, convención, librería, regla de negocio), **proponer** convertirla en ADR.
   Solo se escribe si el usuario acepta: `{{ADR_BORRADORES_DIR}}/NNNN-titulo.md` desde `templates/adr.md`, en estado **Propuesto**, y se enlaza en la ficha.
   Pasa a `{{ADR_DIR}}/` cuando el usuario lo acepta (ver *Aprobar una ficha o un ADR*).
6. Si hubo decisiones o preferencias nuevas del usuario, guardarlas en la memoria del agente y recordar `bash init.sh --sync-memoria`.
7. Responder al usuario con:
   - qué cambió;
   - qué scripts debe ejecutar y en qué orden;
   - qué documentos se actualizaron, con la ruta de la ficha y su estado (**Borrador**, pendiente de su aprobación);
   - los ADR propuestos, si hay;
   - qué se verificó y qué no;
   - que no hay commit, salvo que lo pidiera.

### Ficha de requerimiento
Cada tarea cerrada deja una ficha, creada desde `templates/ficha-requerimiento.md`. Es lo que lee quien llegue después
(otra persona del equipo u otro agente) para saber qué se pidió, qué se decidió, qué se tocó y qué falta.

**Dónde se guarda** (según el `MODO` de `init.sh`):

| Modo | Borrador | Aprobada |
|---|---|---|
| Versionado | `{{FICHAS_DIR}}/` | `{{FICHAS_DIR}}/` (solo cambia el estado) |
| Privado | `{{FICHAS_BORRADORES_DIR}}/`: local, excluida de git, **nunca se sube** | `{{FICHAS_DIR}}/`: versionada, memoria común del equipo |

**Nombre del archivo:** `<PREFIJO>-<número>-<titulo>.md`, con uno de los prefijos de `PREFIJOS_FICHA` (en `init.sh`; por defecto `TCK REQ EVO`).
`<titulo>` va en minúsculas, sin tildes, con guiones y con un máximo de ~50 caracteres.

| La tarea tiene… | Archivo | Ejemplo |
|---|---|---|
| Un número en el sistema del equipo (ticket, requerimiento, petición de cambio…) | `<PREFIJO>-<número>-<titulo>.md` | `TCK-123-error-al-exportar-pdf.md`, `RFC-214-limite-de-credito.md` |
| Nada de eso (evolutivo) | `EVO-<id>-<titulo>.md` | `EVO-9f3a61c2-filtro-por-fecha.md` |

- **Detectar el ID:** buscar `<PREFIJO>-<número>` con cualquiera de los prefijos de `PREFIJOS_FICHA`, sin distinguir mayúsculas
  (también `<PREFIJO>_<número>` o `<PREFIJO>/<número>`), en el nombre de la rama, en el plan, en `progress.md` y en el mensaje del usuario.
  Ejemplo: rama `feature/rfc-214-limite` → `RFC-214`. Si no aparece, o aparecen varios, **preguntar una vez**. No inventar números.
  Un prefijo que no está en la lista no se usa sin preguntar. Si el usuario lo confirma, proponer añadirlo a `PREFIJOS_FICHA`.
- El `<id>` de un evolutivo son 8 caracteres hexadecimales aleatorios, para que no choque con el de otra persona del equipo:
  `od -An -N4 -tx1 /dev/urandom | tr -d ' \n'` (o `openssl rand -hex 4`). Se comprueba que no exista ya en las carpetas de fichas.
  No se usa el hash de un commit porque al cerrar puede no haber commit todavía y una tarea puede tener varios; los commits van dentro de la ficha.
- Al crearla, la ruta se anota en el plan (cabecera) y en `progress.md` (`Ficha: …`), para encontrarla en el siguiente cierre.

**Crear o actualizar:** buscar una ficha con ese ID en `{{FICHAS_BORRADORES_DIR}}/` y en `{{FICHAS_DIR}}/`.
- Si no existe, se crea en **Borrador** con la fecha del sistema.
- Si existe en Borrador, se **actualiza**: se parchean las secciones y se añade una línea al historial.
- Si existe *Aprobada* y hay cambios:
  - en modo versionado, vuelve a **Borrador** con el motivo;
  - en modo privado, la aprobada no se toca: se crea en `{{FICHAS_BORRADORES_DIR}}/` una copia en Borrador con los cambios, que la sustituye cuando se aprueba.

**Cómo se rellena** (sin inventar nada: lo que no se sepa se deja como pendiente o se pregunta):
- **Objetivo, contexto, decisiones y descartes:** del plan (`Objetivo`, `Decisiones`, `Fuera de alcance`) o, si no hay plan, de lo acordado en la sesión.
- **Qué se cambió:** por cada repo, `git -C <repo> branch --show-current`, `git -C <repo> log --oneline` de la tarea y `git -C <repo> status --short`.
  Si no hay commit, se escribe "sin commit todavía" y los archivos tocados.
- **Qué se ejecutó y dónde, pruebas y resultado:** de los gates, de `init.sh --build` y de lo que el usuario confirmó. Lo no probado va a *No verificado*.
- **Hallazgos y pendientes:** de `progress.md` y del plan (*Pendiente para siguiente versión*).

**Filtro antes de guardar** (obligatorio, porque la ficha se comparte):
- sin credenciales, tokens, claves ni cadenas de conexión;
- sin datos de producción: filas reales, nombres, correos o importes de personas o clientes;
- sin nombres de servidor, IPs internas ni rutas de máquinas concretas (se dice "entorno de pruebas", no su nombre);
- sin conversación en bruto: se resume lo decidido.

### Aprobar una ficha o un ADR
Solo el usuario aprueba, con las mismas reglas que los planes (principio 4). El agente nunca aprueba una ficha ni acepta un ADR por su cuenta.
1. Cambiar el estado a **Aprobada** (ficha) o **Aceptado** (ADR), con la fecha y, si fue en el chat, las palabras del usuario.
2. **Volver a pasar el filtro.** Es lo último antes de que el archivo pueda entrar en git.
3. **Modo privado:** mover el archivo de `{{FICHAS_BORRADORES_DIR}}/` a `{{FICHAS_DIR}}/` (o de `{{ADR_BORRADORES_DIR}}/` a `{{ADR_DIR}}/`),
   sustituyendo la versión aprobada anterior si la había. Después, decir al usuario el comando exacto:
   ```
   git add {{FICHAS_DIR}}/RFC-214-limite-de-credito.md
   ```
   Siempre **por nombre**: nunca `git add .` ni `git add -A`. El commit lo hace el usuario, o el agente si se lo pide.
4. **Modo versionado:** el archivo ya está en su sitio. Se indica igualmente el `git add` por nombre.
5. La publicación en la web del equipo la hace a mano quien se ocupa de la documentación, a partir de `{{FICHAS_DIR}}/`. El agente no publica nada.

**Antes de empezar una tarea**, leer en `{{FICHAS_DIR}}/` y `{{ADR_DIR}}/` las fichas y los ADR relacionados (mismo módulo o mismo ID):
son lo que ya hicieron y decidieron los demás.

---

## Marcadores de las plantillas

| Marcador | Qué es | Ejemplo |
|---|---|---|
| `{{PROYECTO}}` | Nombre del proyecto | `Gestor de Pedidos` |
| `{{IDIOMA}}` | Idioma de trabajo | `español` |
| `{{FECHA}}` | Fecha de instalación `AAAA-MM-DD` | `2026-11-22` |
| `{{STACK_TABLA}}` | Filas `\| Capa \| Stack \| Carpeta \|` | `\| API \| .NET 10, MediatR, Dapper \| Backend/ \|` |
| `{{REPOS}}` | Carpetas repo separadas por espacio (`.` = raíz) | `Backend Frontend` |
| `{{REPOS_TEXTO}}` | Frase sobre repos y raíz | `Dos repos (Backend/, Frontend/); la raíz no es repo` |
| `{{REQUERIDOS}}` | Archivos clave extra que `init.sh` comprueba | `Backend/GestorPedidos.slnx Frontend/nuxt.config.ts` |
| `{{NO_TOCAR}}` | Carpetas de otras herramientas | `.cursor/rules` |
| `{{MODO}}` | Modo del arnés: `privado` o `versionado` | `privado` |
| `{{MODO_TEXTO}}` | Frase sobre el modo para `AGENTS.md` | `privado: el arnés está en .git/info/exclude; solo se versionan fichas y ADR aprobados` |
| `{{MEMORIA_TEXTO}}` | Qué es `doc/agentes/memoria/` | `Copia local (excluida de git) de la memoria del agente` / `Copia versionada de la memoria del agente` |
| `{{PREFIJOS_FICHA}}` | Prefijos de ID de las fichas, separados por espacio | `TCK RFC REQ EVO` |
| `{{FICHAS_DIR}}` | Carpeta de fichas aprobadas | `doc/fichas` |
| `{{FICHAS_BORRADORES_DIR}}` | Carpeta de fichas en Borrador (en modo versionado, la misma que `{{FICHAS_DIR}}`) | `doc/fichas/borradores` |
| `{{ADR_DIR}}` | Carpeta de ADR aceptados | `doc/adr` |
| `{{ADR_BORRADORES_DIR}}` | Carpeta de ADR propuestos (en modo versionado, la misma que `{{ADR_DIR}}`) | `doc/adr/borradores` |
| `{{SCRIPTS_DIR}}` | Carpeta de scripts de BD (vacío si no hay) | `Script` |
| `{{SCRIPT_PATRON}}` | Patrón de nombre | `NN-accion-Objeto-descripcion.sql` |
| `{{BD_TEXTO}}` | Regla de BD acordada | `solo SELECT con el login de lectura que indique el usuario` |
| `{{BUILD_BACK}}` / `{{BUILD_FRONT}}` | Comando de build a carpeta temporal `$OUT` (vacío = no) | `dotnet build Backend/X.slnx -nologo -v q -p:OutDir="$OUT/back/"` |
| `{{VERIFICACION_TEXTO}}` | Cómo se verifica cada parte, con sus límites | — |
| `{{ENTORNO_TEXTO}}` | Servidores que ya tiene abiertos el usuario | `API en el IDE :5001, front dev :3000` |
| `{{REGLAS_PROPIAS}}` | Bullets con reglas del bloque D | `- UI: nada de text-xs` |
| `{{MEMORIA_EXCLUIR}}` | Notas privadas que no se copian | `notas-personales.md` |
| `{{DOCS_TABLA}}` | Filas extra de documentos del proyecto | `\| Backend/api_resumen.md \| Endpoints \| Al cambiar un Controller \|` |

Plantillas del modo `cierre` (se rellenan al crear cada ficha o ADR, no al instalar):

| Marcador | Qué es | Ejemplo |
|---|---|---|
| `{{FICHA_ID}}` | ID de la ficha | `TCK-123`, `RFC-214`, `EVO-9f3a61c2` |
| `{{FICHA_TIPO}}` | Qué significa el prefijo (ticket, requerimiento, petición de cambio, evolutivo…) | `Petición de cambio` |
| `{{TITULO}}` | Título corto de la tarea o del ADR | `Error al exportar PDF` |
| `{{PLAN}}` | Plan relacionado o `—` | `plan-implementacion-exportar.md` |
| `{{REFERENCIA}}` | Enlace o código en el gestor de tickets, o `—` | `TCK-123` |
| `{{NNNN}}` | Número del ADR (mayor existente + 1) | `0003` |

## Referencia

- **Origen:** este arnés se probó en proyectos .NET 10 + Nuxt 4 con dos repos, raíz sin git y BD de solo lectura. Sirve para cualquier stack.
- **Ideas incorporadas del flujo *spec-driven*:**
  - aprobación humana como única puerta;
  - plantilla con objetivo, fuera de alcance, decisiones descartadas y criterios comprobables;
  - preguntas en bloques;
  - pausa por etapa.
- **Destino de las fichas:** hoy, una carpeta de `.md` en el repo. Está previsto poder enviarlas a un gestor de tickets; hasta entonces,
  la ficha aprobada se puede copiar o enlazar desde el ticket.
- **Ideas descartadas a propósito:** rama automática por plan, commits automáticos por paso, y bloquear el trabajo si hay cambios sin commit.
- **Skills compatibles:** `base_net10`, `base_nuxt4` y `base_flutter` generan el código del proyecto. Esta skill solo define la forma de trabajar.
  Tras generar un proyecto con ellas, ejecutar `/arnes_trabajo instalar`.
