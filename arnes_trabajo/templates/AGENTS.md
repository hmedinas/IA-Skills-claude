# AGENTS.md — Cómo trabajar en {{PROYECTO}} (para cualquier agente o modelo)

> Punto de entrada para **cualquier** modelo o herramienta (Claude, otro LLM, Cursor).
> Las **reglas del código** están en `CLAUDE.md`. Este archivo explica **la forma de trabajar**.
> Arnés instalado el {{FECHA}} con la skill `arnes_trabajo`.

## 0. Al empezar una sesión (OBLIGATORIO)
1. Ejecutar `bash init.sh`, o `bash init.sh --build` para compilar también.
   En Claude Code se ejecuta solo al iniciar, mediante el hook `SessionStart` de `.claude/settings.json`.
2. Leer `progress.md`: tarea en curso, siguiente paso y pendientes del usuario.
3. Leer `CLAUDE.md` (reglas). Si la tarea toca un módulo, leer también su sección de `Resumen.md` y su `plan-implementacion-*.md`.
4. Para decisiones y preferencias del usuario, empezar por el índice `doc/agentes/memoria/MEMORY.md` y seguir a las notas.
5. Si la tarea tiene ID o toca un módulo, leer sus fichas en `{{FICHAS_DIR}}/` y sus ADR en `{{ADR_DIR}}/`: es lo que ya hicieron y decidieron los demás.

## 1. Documentos y para qué sirve cada uno
| Archivo | Qué es | Cuándo se actualiza |
|---|---|---|
| `progress.md` | Bitácora corta de la tarea en curso (< 60 líneas) | Al cerrar cada paso |
| `Resumen.md` | Estado del producto, decisiones y bitácora (largo, acumulativo) | Al cerrar cada cambio de producto (`CLAUDE.md` §3) |
| `plan-implementacion-<modulo>.md` | Plan por etapas con estado, casillas `[x]` y gates | Al aprobarlo y al cerrar cada etapa |
| `CLAUDE.md` | Reglas de código, SQL y estilo | Solo si el usuario cambia una regla |
| `doc/agentes/memoria/` | {{MEMORIA_TEXTO}} (decisiones del usuario) | Con `bash init.sh --sync-memoria` |
| `{{FICHAS_BORRADORES_DIR}}/` | Fichas de requerimiento en **Borrador** | Al cerrar cada tarea (§4) |
| `{{FICHAS_DIR}}/` | Fichas **aprobadas** (`<PREFIJO>-<n>-<titulo>.md`): objetivo, decisiones, commits, pruebas, pendientes. Memoria común del equipo | Cuando el usuario aprueba una ficha |
| `{{ADR_DIR}}/NNNN-….md` | Decisiones de arquitectura que perduran (ADR aceptados; los propuestos, en `{{ADR_BORRADORES_DIR}}/`) | Cuando el usuario acepta un ADR |
{{DOCS_TABLA}}

### Planes de implementación
- Un `plan-implementacion-<modulo>.md` en la raíz, **solo para módulos grandes** (varias etapas o varias sesiones).
  Para cambios pequeños, explicar el plan en el chat, esperar el OK e implementar.
- **Estados:**
  - `Borrador`: lo escribe el agente.
  - `En revisión`: el usuario lo está mirando.
  - `Aprobado`: **solo el usuario** lo aprueba, editando el archivo o diciéndolo explícitamente en el chat. En el segundo caso el agente lo registra con fecha y sus palabras.
  - `En curso`, `Implementado` y `Obsoleto`.
- **No se implementa nada de un plan que no esté `Aprobado` o `En curso`.**
- **Estructura** (ver la plantilla de la skill):
  - objetivo en una frase;
  - alcance y **fuera de alcance**;
  - decisiones tomadas **y descartadas**;
  - diseño;
  - etapas `E1, E2…` con pasos `- [ ]` y un **gate de verificación** al final de cada una;
  - criterios de aceptación comprobables;
  - registro de avance.
- **Gates:** una etapa no se cierra hasta pasar su gate (builds en verde, scripts ejecutados por el usuario, prueba funcional acordada).
  Al cerrarla, marcar `[x]`, anotar la fecha y actualizar `Resumen.md` y `progress.md`.
- **Cambios de diseño:** se anotan primero en el plan ("Decisiones", con fecha) y después se llevan al código.
  Lo que quede fuera de alcance se anota en "Pendiente para siguiente versión".

## 2. Roles (multi-agente)
| Rol | Quién | Hace | No hace |
|---|---|---|---|
| **Orquestador** | La sesión principal | Habla con el usuario, acuerda el diseño, parte el trabajo, delega, integra, actualiza `progress.md` / `Resumen.md` y hace commit solo si se lo piden | No delega decisiones de diseño |
| **Implementador** | `.claude/agents/implementador.md` | Ejecuta un encargo concreto y acotado (archivos, patrón, criterio de aceptación) y compila | No decide diseño, no edita SQL ya ejecutado, no hace commit, no documenta |
| **Revisor** | `.claude/agents/revisor.md` | Revisa el diff contra `CLAUDE.md` y el encargo; reporta hallazgos con `archivo:línea` | No edita código |

**Cuándo usar los tres:** cambios grandes o de riesgo (seguridad, permisos, lógica de negocio, scripts de BD) y tareas mecánicas masivas.
**Cuándo NO:** cambios pequeños (1–3 archivos). El orquestador lo hace solo, porque cada subagente arranca sin contexto y cuesta más.

Flujo: `Orquestador (diseño acordado con el usuario) → Implementador (encargo) → Revisor (diff) → Orquestador (corrige / integra / documenta)`.

## 3. Reglas de trabajo que NO están en el código
- **Idioma:** {{IDIOMA}}.
- **Base de datos:** {{BD_TEXTO}}.
  - Todo cambio de BD va como script en `{{SCRIPTS_DIR}}/` y **lo ejecuta el usuario a mano**.
  - Nunca se edita un script ya ejecutado: se crea uno nuevo numerado.
- **Git:** {{REPOS_TEXTO}}. Commit, push y ramas **solo si el usuario lo pide**. Los cambios sin commit no bloquean el trabajo.
  - Los archivos se añaden **por nombre**: nunca `git add .` ni `git add -A`.
  - **Modo del arnés:** {{MODO_TEXTO}}.
- **Verificación:** {{VERIFICACION_TEXTO}}
- **Entorno local:** {{ENTORNO_TEXTO}}. No arrancar otros servidores.
{{REGLAS_PROPIAS}}

## 4. Cierre de cada tarea
1. Builds en verde, o decir exactamente qué falla.
2. Revisor (si se usó) sin hallazgos abiertos.
3. `progress.md`, `Resumen.md`, el plan y los documentos del módulo actualizados.
4. **Ficha de requerimiento** creada o actualizada en `{{FICHAS_BORRADORES_DIR}}/`, en estado **Borrador** hasta que el usuario la apruebe:
   - nombre `<PREFIJO>-<n>-<titulo>.md` con un prefijo de `{{PREFIJOS_FICHA}}`, o `EVO-<id>-<titulo>.md` si no tiene número (`<id>` = 8 hex aleatorios);
   - al aprobarla pasa a `{{FICHAS_DIR}}/`, se vuelve a filtrar y se dice el `git add` exacto, por nombre;
   - sin credenciales, datos de producción, nombres de servidor ni conversación en bruto;
   - si una decisión perdura, se propone como ADR en `{{ADR_BORRADORES_DIR}}/`.
5. Resumen al usuario con:
   - qué cambió;
   - qué scripts debe ejecutar y en qué orden;
   - qué documentos se tocaron, incluida la ficha;
   - qué se verificó y qué no.
