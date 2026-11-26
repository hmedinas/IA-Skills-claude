# 00 · Manual de instalación paso a paso

> Desde cero hasta tener el arnés de trabajo funcionando en tu proyecto. Sirve para Windows, macOS y Linux.
> Si solo quieres un componente suelto (un agente, un hook o el MCP), haz los pasos 1 a 3 y salta a la sección *Otros componentes*, al final.

## Resumen
```bash
claude plugin marketplace add hmedinas/IA-Skills-claude    # 1 vez por equipo
claude plugin install arnes_trabajo@ia-skills-claude        # 1 vez por equipo
# en la raíz de tu proyecto, sesión nueva de Claude Code:
/arnes_trabajo instalar
```
El resto de este manual explica cada paso, qué debes ver y qué hacer si algo falla.

---

## Paso 1 · Comprobar los requisitos
Abre una terminal y ejecuta:

```bash
claude --version     # Claude Code
git --version        # Git
bash --version       # bash
```

| Si falla… | Qué hacer |
|---|---|
| `claude` | Instala Claude Code: https://code.claude.com/docs/en/setup |
| `git` | Instala Git: https://git-scm.com/downloads |
| `bash` en **Windows** | Viene con **Git for Windows**. Instálalo y abre de nuevo la terminal. Claude Code lo usa para los hooks |

En macOS y Linux, `bash` ya viene instalado.

## Paso 2 · Añadir el marketplace
Una sola vez por equipo:

```bash
claude plugin marketplace add hmedinas/IA-Skills-claude
```

Así Claude Code sabe de dónde sacar los componentes. Es un repositorio público y no hace falta cuenta de GitHub.

## Paso 3 · Instalar la skill del arnés
```bash
claude plugin install arnes_trabajo@ia-skills-claude
```

Comprueba que aparece:

```bash
claude plugin list
```

Debe salir `arnes_trabajo@ia-skills-claude`, activado.

> ⚠️ **No instales también `hook_inicio_sesion`.** El arnés ya trae ese hook. Si lo tienes de antes:
> `claude plugin uninstall hook_inicio_sesion@ia-skills-claude`.

## Paso 4 · Abrir el proyecto
1. Abre una terminal **en la raíz de tu proyecto**: la carpeta del repo o, si tienes varios repos, la carpeta que los contiene.
2. Arranca una sesión **nueva**:
   ```bash
   claude
   ```
   Si ya tenías una sesión abierta, ciérrala y abre otra, o escribe `/reload-plugins`.
3. Escribe `/arn` y comprueba que se autocompleta `/arnes_trabajo`.

## Paso 5 · Lanzar la instalación
```
/arnes_trabajo instalar
```

Si el nombre corto choca con otra skill, usa el completo: `/arnes_trabajo:arnes_trabajo instalar`.

Primero el agente **mira el proyecto**, sin tocar nada: repos, stack, comandos de build, arnés que ya exista, reglas de Cursor o Copilot,
scripts SQL, cuántas personas hacen commits en el repo y si algún archivo del arnés ya está en git.

## Paso 6 · Responder las preguntas
Solo pregunta lo que no pudo detectar, en bloques de 3 o 4. Lo que detectó viene marcado como opción recomendada.

| Bloque | Qué te pregunta | Consejo |
|---|---|---|
| **A · Proyecto** | Nombre, idioma, repos, quién hace commit, **modo del arnés** | **Repo compartido con otras personas → Privado.** Proyecto solo tuyo → Versionado |
| **A2 · Fichas** | Carpetas de fichas y ADR, **prefijos de ID** | Los de tu sistema de tickets, p. ej. `TCK RFC REQ EVO`. `EVO` siempre está |
| **B · Verificación** | Comando de build de cada parte y servidores que ya tienes abiertos | Así no te arranca ni te pisa nada |
| **C · Datos** | Si hay BD, si es de solo lectura y dónde van los scripts | Con BD de producción: solo lectura, y los scripts los ejecutas tú |
| **D · Reglas propias** | Convenciones, fin de línea, datos sensibles | Lo que siempre le acabas repitiendo a la IA |

**¿Privado o versionado?**

| | Privado | Versionado |
|---|---|---|
| Para | Repos en los que trabajan varias personas | Proyectos propios, o un equipo que comparte el mismo arnés |
| `CLAUDE.md`, `progress.md`, `init.sh`, `.claude/`… | En tu disco; git no los ve (`.git/info/exclude`) | Se suben con el repo |
| Fichas y ADR | Borrador local; las **aprobadas** se suben | Todo se sube |
| Memoria del agente | Solo tuya | Copia versionada |

La explicación completa, con un ejemplo, está en la guía [07 · Arnés de trabajo](07-arnes-trabajo.md), sección *Repo compartido*.

## Paso 7 · Revisar y aprobar
Antes de escribir nada, el agente te enseña:
- los archivos que va a **crear**;
- los que va a **integrar**, si ya existían (por ejemplo, tu `CLAUDE.md`), con la sección que añade. **Nunca los sobrescribe**;
- en modo privado, las líneas que añade a `.git/info/exclude`.

Léelo y contesta que sí, o pide cambios.

## Paso 8 · Comprobar que funciona
El agente ejecuta `bash init.sh` y te enseña la salida. Debe acabar así:

```
== LISTO — siguiente: leer progress.md y AGENTS.md
```

Las líneas `!!` son problemas que hay que corregir antes de empezar. Las líneas `..` son solo informativas.

**Solo en modo privado**, comprueba además:

```bash
bash init.sh | grep "visible"     # → OK  ningún archivo del arnés es visible para git
git status --short                # → no deben salir CLAUDE.md, progress.md, init.sh ni .claude/
```

Después, **cierra la sesión y abre otra**. Al arrancar, el hook ejecuta `init.sh` solo y Claude ya sabe dónde se quedó el proyecto.

## Paso 9 · Primera tarea
El ciclo normal:

```
/arnes_trabajo diseno <modulo>        → plan en Borrador. Lo revisas y lo apruebas
/arnes_trabajo implementar <modulo>   → por etapas, con pausa en cada una
/arnes_trabajo cierre                 → documentos al día y ficha de requerimiento en Borrador
```

Para cambios pequeños no hace falta plan: pídelo directamente y, al terminar, ejecuta `/arnes_trabajo cierre`.
Cuando apruebes la ficha, el agente te da el `git add` exacto. **Añade siempre los archivos por nombre: nunca `git add .`**.

---

## Instalar en el resto del equipo
Cada persona repite los pasos 1 a 8 en **su** copia del repo. En modo privado, cada una tiene su propio arnés,
y lo único que comparten son las fichas y los ADR aprobados, que llegan con `git pull`.

## Actualizar
Cuando se publique una versión nueva:

```bash
claude plugin marketplace update ia-skills-claude
```

Después abre una sesión nueva y ejecuta `/arnes_trabajo revisar` en cada proyecto: te propone los cambios de las plantillas nuevas
sin pisar lo tuyo.

## Desinstalar
```bash
claude plugin uninstall arnes_trabajo@ia-skills-claude
```

Esto quita la skill, pero **no** los archivos que creó en tus proyectos. Si también quieres quitarlos:
- borra `AGENTS.md`, `CLAUDE.md` (si lo creó el arnés), `progress.md`, `Resumen.md`, `init.sh`, `.claude/agents/implementador.md`,
  `.claude/agents/revisor.md` y `doc/agentes/`;
- quita el hook `SessionStart` de `.claude/settings.json` (o de `settings.local.json`);
- en modo privado, borra el bloque `# arnes_trabajo (modo privado)` de `.git/info/exclude`.

Las fichas y los ADR aprobados de `doc/fichas/` y `doc/adr/` son del equipo: no los borres.

## Instalar sin plugin (a mano)
Si no puedes usar el marketplace:
1. Descarga el repositorio: https://github.com/hmedinas/IA-Skills-claude (*Code → Download ZIP*).
2. Copia la carpeta `arnes_trabajo/` en:
   - `~/.claude/skills/arnes_trabajo/` (Windows: `C:\Users\<tú>\.claude\skills\arnes_trabajo\`), para usarla en todos tus proyectos; o
   - `<proyecto>/.claude/skills/arnes_trabajo/`, solo para ese proyecto.
3. Sigue desde el paso 4. Sin plugin, el comando es `/arnes_trabajo` y para actualizar hay que volver a copiar la carpeta.

## Otros componentes
Todos se instalan igual, después del paso 2:

| Componente | Qué es | Instalar |
|---|---|---|
| `agente_implementador` | Subagente que ejecuta encargos concretos | `claude plugin install agente_implementador@ia-skills-claude` |
| `agente_revisor` | Subagente que revisa el diff | `claude plugin install agente_revisor@ia-skills-claude` |
| `hook_solo_select` | Bloquea SQL que no sea `SELECT` | `claude plugin install hook_solo_select@ia-skills-claude` |
| `hook_inicio_sesion` | Ejecuta `init.sh` al abrir (**no hace falta con el arnés**) | `claude plugin install hook_inicio_sesion@ia-skills-claude` |
| `mcp_sql_lectura` | MCP de solo lectura para SQL Server (necesita .NET 10) | `claude plugin install mcp_sql_lectura@ia-skills-claude`. Configuración en su README |
| `base_net10`, `base_nuxt4`, `base_flutter` | Generadores de proyectos nuevos | `claude plugin install <nombre>@ia-skills-claude` |

El arnés ya incluye sus propias copias del implementador y del revisor en `.claude/agents/`: no hace falta instalarlos aparte.

## Problemas frecuentes
| Síntoma | Solución |
|---|---|
| `claude plugin marketplace add` falla | Comprueba la conexión y que escribiste `hmedinas/IA-Skills-claude` |
| No aparece `/arnes_trabajo` | La sesión se abrió antes de instalar: abre otra o usa `/reload-plugins`. Comprueba `claude plugin list` |
| `init.sh` no se ejecuta al abrir, en Windows | Falta Git for Windows (bash). Instálalo y abre otra sesión |
| `$'\r': command not found` | `init.sh` tiene fin de línea CRLF. Añade `*.sh text eol=lf` al `.gitattributes` y vuelve a generarlo |
| `init.sh` se ejecuta dos veces | Tienes también `hook_inicio_sesion`: desinstálalo |
| `git pull` falla por `CLAUDE.md` o `progress.md` | Alguien subió su arnés. Pasad al modo privado (`/arnes_trabajo revisar`) |
| `init.sh` dice que hay archivos del arnés visibles para git | Falta una línea en `.git/info/exclude`, o ese archivo ya estaba en git. `/arnes_trabajo revisar` te dice cuál |
| No llegan los cambios de una versión nueva | `claude plugin marketplace update ia-skills-claude` y abrir una sesión nueva |
