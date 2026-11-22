---
name: arnes_trabajo
description: Arnés de trabajo con agentes para cualquier proyecto (Claude Code, Cursor u otro LLM). Instala o revisa AGENTS.md, CLAUDE.md, progress.md, Resumen.md, init.sh con hook SessionStart, agentes implementador/revisor, memoria versionada y planes por etapas con aprobación humana; guía el diseño de un módulo (preguntas en bloques → plan en Borrador), la implementación por etapas con gate y el cierre documentado. Úsala cuando el usuario pida montar, preparar, revisar o copiar "el arnés" o "la forma de trabajar" en un proyecto, diseñar un módulo o funcionalidad grande antes de programarla, implementar un plan aprobado o cerrar una tarea con su documentación. Pensada para proyectos multi-repo, con base de datos de solo lectura y commits solo a petición.
argument-hint: "[instalar | revisar | diseno <modulo> | implementar <modulo> | cierre]"
---

# arnes_trabajo — Arnés de trabajo con agentes

Monta y aplica una forma de trabajar estable entre sesiones y entre herramientas:
- **arranque verificado:** `init.sh` y hook `SessionStart`;
- **contexto corto y persistente:** `progress.md`, `Resumen.md` y la memoria;
- **reglas escritas:** `CLAUDE.md` y `AGENTS.md`;
- **roles:** orquestador, implementador y revisor;
- **planes por etapas** con aprobación humana y gate de verificación;
- **cierre documentado** sin que el usuario tenga que pedirlo.

Las plantillas están en `templates/` (junto a este archivo). Los marcadores `{{ASI}}` los rellena la skill con
lo que detecta en el proyecto y lo que responde el usuario.

## Modos

| Invocación | Qué hace |
|---|---|
| `/arnes_trabajo instalar` | Crea el arnés en un proyecto o completa uno a medias |
| `/arnes_trabajo revisar` | Comprueba un arnés existente y propone lo que falte, sin pisar nada |
| `/arnes_trabajo diseno <modulo>` | Preguntas en bloques → `plan-implementacion-<modulo>.md` en estado **Borrador**. No programa |
| `/arnes_trabajo implementar <modulo>` | Ejecuta un plan **Aprobado**, etapa por etapa, con gate y pausa |
| `/arnes_trabajo cierre` | Lista de cierre: builds, documentos, scripts pendientes, resumen al usuario |

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

### 2. Preguntar en bloques (solo lo que no se pudo detectar)
Usar la herramienta de preguntas con 3–4 preguntas por bloque, con lo detectado como opción recomendada. Parar en
cuanto se pueda rellenar la plantilla.
- **Bloque A, proyecto:** nombre, idioma, repos detectados (confirmar), quién hace commit/push.
- **Bloque B, verificación:** comando de build de cada parte, servidores que el usuario tiene abiertos (puertos) y que no hay que arrancar.
- **Bloque C, datos:** si hay BD; si el acceso del agente es de solo lectura; carpeta y numeración de scripts; quién los ejecuta.
- **Bloque D, reglas propias:**
  - convenciones de UI o de código ya acordadas;
  - fin de línea y codificación (CRLF, BOM);
  - datos sensibles (p. ej. "nunca nombres de clientes reales");
  - notas de memoria privadas que no deben copiarse al repo.

### 3. Generar
1. Enseñar la lista de archivos a **crear** y a **integrar** (los existentes, con la sección que se añade). Esperar el OK.
2. Crear desde `templates/`, sustituyendo los marcadores (tabla al final). Lo que no aplique **se borra**: no se dejan marcadores ni secciones vacías.
   - `AGENTS.md`, `CLAUDE.md`, `progress.md`, `Resumen.md` y `init.sh` van en la raíz.
   - `settings.json` va en `.claude/settings.json`. Si ya existe, **añadir** el hook `SessionStart` sin quitar nada.
   - `agents/implementador.md` y `agents/revisor.md` van en `.claude/agents/`.
   - `memoria/MEMORY.md` va en `doc/agentes/memoria/MEMORY.md`.
   - `plan-implementacion.md` **no se copia** al instalar: es la plantilla del modo `diseno`.
3. Respetar el fin de línea dominante del proyecto en los `.md`. `init.sh` siempre con LF.
4. Ejecutar `bash init.sh` y corregir lo que marque.
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
4. Si hubo decisiones o preferencias nuevas del usuario, guardarlas en la memoria del agente y recordar `bash init.sh --sync-memoria`.
5. Responder al usuario con:
   - qué cambió;
   - qué scripts debe ejecutar y en qué orden;
   - qué documentos se actualizaron;
   - qué se verificó y qué no;
   - que no hay commit, salvo que lo pidiera.

---

## Marcadores de las plantillas

| Marcador | Qué es | Ejemplo |
|---|---|---|
| `{{PROYECTO}}` | Nombre del proyecto | `Nexus Change` |
| `{{IDIOMA}}` | Idioma de trabajo | `español` |
| `{{FECHA}}` | Fecha de instalación `AAAA-MM-DD` | `2026-11-22` |
| `{{STACK_TABLA}}` | Filas `\| Capa \| Stack \| Carpeta \|` | `\| API \| .NET 10, MediatR, Dapper \| Backend/ \|` |
| `{{REPOS}}` | Carpetas repo separadas por espacio (`.` = raíz) | `Backend Nexus_Change` |
| `{{REPOS_TEXTO}}` | Frase sobre repos y raíz | `Dos repos (Backend/, Nexus_Change/); la raíz no es repo` |
| `{{REQUERIDOS}}` | Archivos clave extra que `init.sh` comprueba | `Backend/Nexus_change.slnx Nexus_Change/nuxt.config.ts` |
| `{{NO_TOCAR}}` | Carpetas de otras herramientas | `.cursor/rules` |
| `{{SCRIPTS_DIR}}` | Carpeta de scripts de BD (vacío si no hay) | `Script` |
| `{{SCRIPT_PATRON}}` | Patrón de nombre | `NN-accion-Objeto-descripcion.sql` |
| `{{BD_TEXTO}}` | Regla de BD acordada | `solo SELECT con el login de lectura que indique el usuario` |
| `{{BUILD_BACK}}` / `{{BUILD_FRONT}}` | Comando de build a carpeta temporal `$OUT` (vacío = no) | `dotnet build Backend/X.slnx -nologo -v q -p:OutDir="$OUT/back/"` |
| `{{VERIFICACION_TEXTO}}` | Cómo se verifica cada parte, con sus límites | — |
| `{{ENTORNO_TEXTO}}` | Servidores que ya tiene abiertos el usuario | `API en VS :7209, front dev :3000` |
| `{{REGLAS_PROPIAS}}` | Bullets con reglas del bloque D | `- UI: nada de text-xs` |
| `{{MEMORIA_EXCLUIR}}` | Notas privadas que no se copian | `revision-semestral-gxo.md` |
| `{{DOCS_TABLA}}` | Filas extra de documentos del proyecto | `\| Backend/api_resumen.md \| Endpoints \| Al cambiar un Controller \|` |

## Referencia

- **Origen:** este arnés se probó en proyectos .NET 10 + Nuxt 4 con dos repos, raíz sin git y BD de producción de solo lectura.
- **Ideas incorporadas del flujo *spec-driven*:**
  - aprobación humana como única puerta;
  - plantilla con objetivo, fuera de alcance, decisiones descartadas y criterios comprobables;
  - preguntas en bloques;
  - pausa por etapa.
- **Ideas descartadas a propósito:** rama automática por plan, commits automáticos por paso, y bloquear el trabajo si hay cambios sin commit.
- **Skills compatibles:** `base_net10`, `base_nuxt4` y `base_flutter` generan el código del proyecto. Esta skill solo define la forma de trabajar.
  Tras generar un proyecto con ellas, ejecutar `/arnes_trabajo instalar`.
